// <copyright file="AccountCreatedEventHandlerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Moq;
using Pulse.Authorization.Infrastructure.Constants;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Interfaces;
using Pulse.Authorization.Infrastructure.Providers;
using Pulse.Authorization.Infrastructure.Providers.Interfaces;

namespace Pulse.Authorization.Infrastructure.Tests.Providers;

public class AccountCreatedEventHandlerTests
{
    private readonly Mock<IAuthorizationEventPublisher> _authorizationEventPublisherMock = new(MockBehavior.Strict);
    private readonly Mock<IAuthorizationRepository> _authorizationRepositoryMock = new(MockBehavior.Strict);

    public AccountCreatedEventHandlerTests()
    {
        _authorizationEventPublisherMock.Setup(a => a.PublishAuthorizationUpdatedEventAsync(It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>(), false))
            .Returns(Task.CompletedTask)
            .Verifiable();
        _authorizationRepositoryMock.Setup(a => a.CreateDefaultAuthorizationsOnAccountAsync(It.IsAny<int>()))
            .ReturnsAsync(GlobalConstants.DefaultAccountPermissions)
            .Verifiable();
    }

    [Fact]
    public async Task HandleAsync_WithValidMessage_ShouldCreatesAccount()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<AccountCreatedEventHandler>>();
        var repositoryMock = new Mock<IAccountEventRepository>();

        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

        var handler = new AccountCreatedEventHandler(loggerMock.Object, repositoryMock.Object, _authorizationRepositoryMock.Object, _authorizationEventPublisherMock.Object);
        var message = "{\"EventType\":\"AccountCreatedEvent\",\"Data\":{\"AccountId\":123,\"FirstName\":\"John Doe\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.CreateAccountAsync(It.IsAny<AccountEntity>()), Times.Once);
        _authorizationRepositoryMock.Verify();
        _authorizationEventPublisherMock.Verify();
    }

    [Fact]
    public async Task HandleAsync_WithNullMessage_ShouldNotCreateAccount()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<AccountCreatedEventHandler>>();
        var repositoryMock = new Mock<IAccountEventRepository>();
        var handler = new AccountCreatedEventHandler(loggerMock.Object, repositoryMock.Object, _authorizationRepositoryMock.Object, _authorizationEventPublisherMock.Object);

        // Act
        await handler.HandleAsync(null!);

        // Assert
        repositoryMock.Verify(repo => repo.CreateAccountAsync(It.IsAny<AccountEntity>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMessageMissingAccountId_ShouldNotCreateAccount()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<AccountCreatedEventHandler>>();
        var repositoryMock = new Mock<IAccountEventRepository>();
        var handler = new AccountCreatedEventHandler(loggerMock.Object, repositoryMock.Object, _authorizationRepositoryMock.Object, _authorizationEventPublisherMock.Object);
        var message = "{\"EventType\":\"AccountCreatedEvent\",\"Data\":{\"FirstName\":\"John Doe\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.CreateAccountAsync(It.IsAny<AccountEntity>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMessageMissingData_ShouldNotCreateAccount()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<AccountCreatedEventHandler>>();
        var repositoryMock = new Mock<IAccountEventRepository>();
        var handler = new AccountCreatedEventHandler(loggerMock.Object, repositoryMock.Object, _authorizationRepositoryMock.Object, _authorizationEventPublisherMock.Object);
        var message = "{\"EventType\":\"AccountCreatedEvent\"}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.CreateAccountAsync(It.IsAny<AccountEntity>()), Times.Never);
    }
}
