// <copyright file="AccountRemovedEventHandlerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Moq;
using Pulse.Authorization.Infrastructure.Providers;
using Pulse.Authorization.Infrastructure.Providers.Interfaces;
using Xunit;

namespace Pulse.Authorization.Infrastructure.Tests.Providers;

public class AccountRemovedEventHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidMessage_ShouldRemoveAccountAndAuthorizations()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<AccountRemovedEventHandler>>();
        var repositoryMock = new Mock<IAccountEventRepository>();

        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

        var handler = new AccountRemovedEventHandler(loggerMock.Object, repositoryMock.Object);
        var message = "{\"EventType\":\"AccountRemovedEvent\",\"Data\":{\"AccountId\":123,\"FirstName\":\"John Doe\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.RemoveAccountAsync(It.IsAny<int>()), Times.Once);
        repositoryMock.Verify(repo => repo.RemoveAccountAuthorizationsAsync(It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNullMessage_ShouldNotRemoveAccount()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<AccountRemovedEventHandler>>();
        var repositoryMock = new Mock<IAccountEventRepository>();
        var handler = new AccountRemovedEventHandler(loggerMock.Object, repositoryMock.Object);

        // Act
        await handler.HandleAsync(null!);

        // Assert
        repositoryMock.Verify(repo => repo.RemoveAccountAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMessageMissingAccountId_ShouldNotRemoveAccount()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<AccountRemovedEventHandler>>();
        var repositoryMock = new Mock<IAccountEventRepository>();
        var handler = new AccountRemovedEventHandler(loggerMock.Object, repositoryMock.Object);
        var message = "{\"EventType\":\"AccountRemovedEvent\",\"Data\":{\"FirstName\":\"John Doe\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.RemoveAccountAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMessageMissingData_ShouldNotRemoveAccount()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<AccountRemovedEventHandler>>();
        var repositoryMock = new Mock<IAccountEventRepository>();
        var handler = new AccountRemovedEventHandler(loggerMock.Object, repositoryMock.Object);
        var message = "{\"EventType\":\"AccountRemovedEvent\"}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.RemoveAccountAsync(It.IsAny<int>()), Times.Never);
    }
}
