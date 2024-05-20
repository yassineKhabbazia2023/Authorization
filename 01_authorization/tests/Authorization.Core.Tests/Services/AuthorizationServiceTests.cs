// <copyright file="AuthorizationServiceTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Moq;
using Pulse.Authorization.Core.Exceptions;
using Pulse.Authorization.Core.Interfaces;
using Pulse.Authorization.Core.Models;
using Pulse.Authorization.Core.Services;

namespace Pulse.Authorization.Core.Tests.Services;

public class AuthorizationServiceTests
{
    private readonly Mock<IAuthorizationRepository> _authorizationRepository;

    private readonly Fixture _fixture;

    public AuthorizationServiceTests()
    {
        _authorizationRepository = new Mock<IAuthorizationRepository>(MockBehavior.Strict);
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
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
        var contactMocked = _fixture.Build<Contact>()
                                    .With(c => c.Type, contactType)
                                    .With(c => c.ContactId, contactId)
                                    .Create();
        _authorizationRepository.Setup(repository => repository.GetContactAccountAuthorizationsAsync(contactId, accountId))
            .ReturnsAsync(menuCodeMocked);

        _authorizationRepository.Setup(repository => repository.GetContactAccountAuthorizationsAsync(contactId, -1))
           .ReturnsAsync(menuCodeMockedDefault);

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
            Assert.Equal(menuCodeMockedAccount, resources);
        }
    }

    [Fact]
    public async Task GetNavigationAsync_Should_Throw_NotFoundException()
    {
        // Arrange
        var accountId = 123;
        var contactId = 234;
        var resourceMocked = _fixture.Create<List<string>>();
        _authorizationRepository.Setup(repository => repository.GetContactAccountAuthorizationsAsync(contactId, accountId))
            .ReturnsAsync(resourceMocked);

        var contactRepository = new Mock<IContactRepository>(MockBehavior.Strict);
        contactRepository.Setup(repository => repository.GetContactByIdAsync(contactId))
            .ReturnsAsync((Contact)null!);

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
        var contactMocked = _fixture.Create<Contact>();
        contactRepository.Setup(c => c.GetContactByIdAsync(123)).ReturnsAsync(contactMocked);
        var authorizationService = new AuthorizationService(authorizationRepository.Object, contactRepository.Object);

        // Act
        await authorizationService.DeleteContactAuthorizationAsync(123, null);

        // Assert
        authorizationRepository.Verify(c => c.DeleteContactAuthorizationAsync(123, -1));
    }
}
