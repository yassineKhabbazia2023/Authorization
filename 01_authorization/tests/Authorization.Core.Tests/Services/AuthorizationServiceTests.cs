// <copyright file="AuthorizationServiceTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System;
using AutoFixture;
using Kpmg.ExceptionMiddleware.AdvancedException;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Moq;
using Pulse.Authorization.Core.Constants;
using Pulse.Authorization.Core.Exceptions;
using Pulse.Authorization.Core.Interfaces;
using Pulse.Authorization.Core.Models;
using Pulse.Authorization.Core.Models.Paging;
using Pulse.Authorization.Core.Requests;
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
        var menuCodeMocked = _fixture.Create<List<string>>();
        var contactMocked = _fixture.Build<Contact>()
                                    .With(c => c.Type, contactType)
                                    .Create();
        _authorizationRepository.Setup(repository => repository.GetContactAndAccountAuthorizations(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(menuCodeMocked);

        _authorizationRepository.Setup(repository => repository.GetAccountAuthorizations(It.IsAny<int>()))
            .ReturnsAsync(menuCodeMocked);

        var contactRepository = new Mock<IContactRepository>(MockBehavior.Strict);
        contactRepository.Setup(repository => repository.GetContactByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(contactMocked);

        var authorizationService = new AuthorizationService(_authorizationRepository.Object, contactRepository.Object);

        if (contactType.Equals("Wrong-Type"))
        {
            // Act
            var act = async () => await authorizationService.GetContactAuthorization(contactMocked.ContactId, accountId);

            // Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(act);
            Assert.Equal(Errors.NotFoundContactTypeCode, exception.Code);
            Assert.Equal(string.Format(Errors.NotFoundContactTypeMessage, contactMocked.ContactId, "Wrong-Type"), exception.Message);
        }
        else
        {
            // Act
            var resources = await authorizationService.GetContactAuthorization(contactMocked.ContactId, accountId);

            // Assert
            Assert.Equal(menuCodeMocked, resources);
        }
    }

    [Fact]
    public async Task GetNavigationAsync_Should_Throw_NotFoundException()
    {
        // Arrange
        var accountId = 123;
        var resourceMocked = _fixture.Create<List<string>>();
        _authorizationRepository.Setup(repository => repository.GetContactAndAccountAuthorizations(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(resourceMocked);

        var contactRepository = new Mock<IContactRepository>(MockBehavior.Strict);
        contactRepository.Setup(repository => repository.GetContactByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Contact)null!);

        var authorizationService = new AuthorizationService(_authorizationRepository.Object, contactRepository.Object);

        // Act
        var act = async () => await authorizationService.GetContactAuthorization(234, accountId);

        // Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(act);
        Assert.Equal(Errors.NotFoundContactCode, exception.Code);
        Assert.Equal(string.Format(Errors.NotFoundContactMessage, 234), exception.Message);
    }
}
