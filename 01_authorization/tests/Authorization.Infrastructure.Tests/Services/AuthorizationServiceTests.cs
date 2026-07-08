// <copyright file="AuthorizationServiceTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Text;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Pulse.Authorization.Core.Exceptions;
using Pulse.Authorization.Core.Interfaces;
using Pulse.Authorization.Core.Models;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Enum;
using Pulse.Authorization.Infrastructure.Interfaces;
using Pulse.Authorization.Infrastructure.Repositories;
using Pulse.Authorization.Infrastructure.Services;
using Pulse.Authorization.Tests.Helpers;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Authorization.Infrastructure.Tests.Services;

public class AuthorizationServiceTests
{
    private readonly Mock<IAuthorizationRepository> _authorizationRepository;

    private readonly Fixture _fixture;

    public AuthorizationServiceTests()
    {
        _authorizationRepository = new Mock<IAuthorizationRepository>(MockBehavior.Strict);
        _fixture = EntityFixtureFactory.Create();
    }

    [Fact]
    public async Task GetAllContactAuthorizationsAsync_Should_ReturnsAuthorizationList()
    {
        // Arrange
        var contactId = 234;
        var expectedResult = _fixture.Create<List<string>>();
        _authorizationRepository.Setup(repository => repository.GetAllContactAuthorizationsAsync(contactId))
            .ReturnsAsync(expectedResult);

        var contactRepository = new Mock<IContactRepository>(MockBehavior.Strict);
        var authorizationService = new AuthorizationService(_authorizationRepository.Object, contactRepository.Object);

        // Act
        var result = await authorizationService.GetAllContactAuthorizationsAsync(contactId);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task GetContactIdsByAuthorizationCodesAndAccountIdSignatoryAsync_Should_ReturnsContactList()
    {
        // Arrange
        var accountId = 123;
        var expectedResult = _fixture.Create<List<Contact>>();

        _authorizationRepository
            .Setup(repository => repository.GetContactIdsByAccountIdSignatoryAsync(accountId))
            .ReturnsAsync(expectedResult);

        var contactRepository = new Mock<IContactRepository>(MockBehavior.Strict);
        var authorizationService = new AuthorizationService(_authorizationRepository.Object, contactRepository.Object);

        // Act
        var result = await authorizationService.GetContactIdsByAccountIdSignatoryAsync(accountId);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Theory]
    [InlineData("Collaborator")]
    [InlineData("Customer")]
    [InlineData("Wrong-Type")]
    public async Task GetNavigationAsync_Should_ReturnsResourceList(string contactType)
    {
        // Arrange
        var accountId = 123;
        var contactId = 456;
        var menuCodeMocked = _fixture.Create<List<string>>();
        var menuCodeMockedDefault = _fixture.Create<List<string>>();
        var menuCodeMockedContact = _fixture.Create<List<string>>();
        var menuCodeMockedAccount = contactType.Equals("Collaborator")
            ? menuCodeMockedDefault.Concat(menuCodeMocked).ToList()
            : menuCodeMocked;
        var contactMocked = _fixture.Build<ContactEntity>()
                                    .With(c => c.Type, contactType)
                                    .With(c => c.ContactId, contactId)
                                    .Create();
        _authorizationRepository.Setup(repository => repository.GetContactAccountAuthorizationsAsync(contactId, accountId, It.IsAny<bool?>()))
            .ReturnsAsync(menuCodeMocked);

        _authorizationRepository.Setup(repository => repository.GetContactAccountAuthorizationsAsync(contactId, -1, true))
           .ReturnsAsync(menuCodeMockedDefault);

        _authorizationRepository.Setup(repository => repository.GetContactAccountAuthorizationsAsync(contactId, -1, false))
           .ReturnsAsync(menuCodeMocked);

        _authorizationRepository.Setup(repository => repository.GetAccountAuthorizationAsync(accountId))
            .ReturnsAsync(menuCodeMockedAccount);

        _authorizationRepository.Setup(repository => repository.GetContactAuthorizationAsync(contactId))
            .ReturnsAsync(menuCodeMockedContact);

        var contactRepository = new Mock<IContactRepository>(MockBehavior.Strict);
        contactRepository.Setup(repository => repository.GetContactByIdAsync(contactId))
            .ReturnsAsync(contactMocked);

        var authorizationService = new AuthorizationService(_authorizationRepository.Object, contactRepository.Object);

        if (contactType.Equals("Wrong-Type"))
        {
            // Act
            var act = async () => await authorizationService.GetContactAuthorizationAsync(contactId, accountId);

            // Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(act);
            Assert.Equal(Errors.NotFoundContactTypeCode, exception.Code);
            Assert.Equal(string.Format(Errors.NotFoundContactTypeMessage, contactId, "Wrong-Type"), exception.Message);
        }
        else
        {
            // Act
            var resources = await authorizationService.GetContactAuthorizationAsync(contactId, accountId);

            // Assert
            Assert.Equal(menuCodeMocked, resources);
        }
    }

    [Fact]
    public async Task GetNavigationAsync_Should_Throw_NotFoundException()
    {
        // Arrange
        var accountId = 123;
        var contactId = 234;
        var resourceMocked = _fixture.Create<List<string>>();
        _authorizationRepository.Setup(repository => repository.GetContactAccountAuthorizationsAsync(contactId, accountId, null))
            .ReturnsAsync(resourceMocked);

        var contactRepository = new Mock<IContactRepository>(MockBehavior.Strict);
        contactRepository.Setup(repository => repository.GetContactByIdAsync(contactId))
            .ReturnsAsync((ContactEntity)null!);

        var authorizationService = new AuthorizationService(_authorizationRepository.Object, contactRepository.Object);

        // Act
        var act = async () => await authorizationService.GetContactAuthorizationAsync(contactId, accountId);

        // Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(act);
        Assert.Equal(Errors.NotFoundContactCode, exception.Code);
        Assert.Equal(string.Format(Errors.NotFoundContactMessage, 234), exception.Message);
    }

    [Fact]
    public async Task DeletePermission_DeletePermission()
    {
        // Arrange
        var authorizationRepository = new Mock<IAuthorizationRepository>();
        authorizationRepository.Setup(c => c.DeleteContactAuthorizationAsync(It.IsAny<int>(), It.IsAny<int>())).Returns(Task.CompletedTask);
        var contactRepository = new Mock<IContactRepository>();
        var contactMocked = _fixture.Create<ContactEntity>();
        contactRepository.Setup(c => c.GetContactByIdAsync(123)).ReturnsAsync(contactMocked);
        var authorizationService = new AuthorizationService(authorizationRepository.Object, contactRepository.Object);

        // Act
        await authorizationService.DeleteContactAuthorizationAsync(123, null);

        // Assert
        authorizationRepository.Verify(c => c.DeleteContactAuthorizationAsync(123, -1));
    }

    [Fact]
    public async Task DeletePermission_WithNullContact_ShouldThrowNotFoundException()
    {
        var contactRepository = new Mock<IContactRepository>();

        var service = new AuthorizationService(null!, contactRepository.Object);

        var result = await Assert.ThrowsAsync<NotFoundException>(async () => await service.DeleteContactAuthorizationAsync(1, It.IsAny<int>()));

        Assert.Equal(Errors.NotFoundContactCode, result.Code);
        Assert.Equal(string.Format(Errors.NotFoundContactMessage, 1), result.Message);
    }

    [Fact]
    public async Task DeletePermission_WithCustomerContactAndNullAccountId_ShouldThrowBadRequestException()
    {
        var contactRepository = new Mock<IContactRepository>();
        var contactMocked = _fixture.Build<ContactEntity>()
            .With(c => c.Type, ContactType.Customer.ToString())
            .Create();
        contactRepository.Setup(c => c.GetContactByIdAsync(It.IsAny<int>())).ReturnsAsync(contactMocked);

        int? accountId = null;
        var service = new AuthorizationService(null!, contactRepository.Object);

        var action = async () => await service.DeleteContactAuthorizationAsync(It.IsAny<int>(), null!);
        var exceptionResult = await action.Should().ThrowAsync<BadRequestException>();

        exceptionResult.Which.Code.Should().Be(Errors.NotFoundAccountCode);
        exceptionResult.WithMessage(string.Format(Errors.NotFoundAccountMessage, accountId));
    }

    [Fact]
    public async Task SetPermissionForContactEmailAsync_ReadsEmailsAndCallsRepository()
    {
        // Arrange
        var csvContent = "user1@example.com\nuser2@example.com\n";
        var bytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(bytes);

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
        fileMock.Setup(f => f.Length).Returns(stream.Length);
        fileMock.Setup(f => f.FileName).Returns("emails.csv");
        fileMock.Setup(f => f.ContentType).Returns("text/csv");

        var repoMock = new Mock<IAuthorizationRepository>();
        var service = new AuthorizationService(repoMock.Object, null!);

        var permission = "COGED002";

        // Act
        await service.SetPermissionForContactEmailAsync(permission, fileMock.Object);

        // Assert
        repoMock.Verify(r => r.SetPermissionByContactEmailAsync(
            permission,
            It.Is<List<string>>(l =>
                l.Count == 2 &&
                l[0] == "user1@example.com" &&
                l[1] == "user2@example.com")), Times.Once);
    }
}
