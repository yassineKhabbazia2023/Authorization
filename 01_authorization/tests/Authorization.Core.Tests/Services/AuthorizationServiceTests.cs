// <copyright file="AuthorizationServiceTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Kpmg.ExceptionMiddleware.AdvancedException;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Moq;
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

    [Fact]
    public async Task GetNavigationAsync_Should_ReturnsResourceList()
    {
        // Arrange
        var accountId = 123;
        var resourceMocked = _fixture.Create<NavigationRequest>();
        var contactMocked = _fixture.Create<Contact>();
        _authorizationRepository.Setup(repository => repository.GetNavigationsAsync(It.IsAny<int>(), It.IsAny<Contact>()))
            .ReturnsAsync(resourceMocked);

        var contactRepository = new Mock<IContactRepository>(MockBehavior.Strict);
        contactRepository.Setup(repository => repository.GetContactByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(contactMocked);

        var authorizationService = new AuthorizationService(_authorizationRepository.Object, contactRepository.Object);

        // Act
        var resources = await authorizationService.GetNavigationsAsync(accountId, contactMocked.ContactId);

        // Assert
        Assert.Equal(resourceMocked, resources);
    }

    [Fact]
    public async Task GetNavigationAsync_Should_Throw_NotFoundException()
    {
        // Arrange
        var accountId = 123;
        var resourceMocked = _fixture.Create<NavigationRequest>();
        _authorizationRepository.Setup(repository => repository.GetNavigationsAsync(It.IsAny<int>(), It.IsAny<Contact>()))
            .ReturnsAsync(resourceMocked);

        var contactRepository = new Mock<IContactRepository>(MockBehavior.Strict);
        contactRepository.Setup(repository => repository.GetContactByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Contact)null!);

        var authorizationService = new AuthorizationService(_authorizationRepository.Object, contactRepository.Object);

        // Act
        var act = async () => await authorizationService.GetNavigationsAsync(accountId, 234);

        // Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(act);
        Assert.Equal(Errors.NotFoundContactCode, exception.Code);
        Assert.Equal(string.Format(Errors.NotFoundContactMessage, 234), exception.Message);
    }
}
