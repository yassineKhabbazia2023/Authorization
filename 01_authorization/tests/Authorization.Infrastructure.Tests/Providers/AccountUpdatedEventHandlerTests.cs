// <copyright file="AccountUpdatedEventHandlerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Moq;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Providers;
using Pulse.Authorization.Infrastructure.Providers.Interfaces;
using Xunit;

namespace Pulse.Authorization.Infrastructure.Tests.Providers;

public class AccountUpdatedEventHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidMessage_ShouldUpdateAccount()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<AccountUpdatedEventHandler>>();
        var repositoryMock = new Mock<IAccountEventRepository>();

        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

        var handler = new AccountUpdatedEventHandler(loggerMock.Object, repositoryMock.Object);
        var message = "{\"EventType\":\"AccountUpdatedEvent\",\"Data\":{\"AccountId\":123,\"FirstName\":\"John Doe\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.UpdateAccountAsync(It.IsAny<AccountEntity>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNullMessage_ShouldNotUpdateAccount()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<AccountUpdatedEventHandler>>();
        var repositoryMock = new Mock<IAccountEventRepository>();
        var handler = new AccountUpdatedEventHandler(loggerMock.Object, repositoryMock.Object);

        // Act
        await handler.HandleAsync(null!);

        // Assert
        repositoryMock.Verify(repo => repo.UpdateAccountAsync(It.IsAny<AccountEntity>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMessageMissingAccountId_ShouldNotUpdateAccount()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<AccountUpdatedEventHandler>>();
        var repositoryMock = new Mock<IAccountEventRepository>();
        var handler = new AccountUpdatedEventHandler(loggerMock.Object, repositoryMock.Object);
        var message = "{\"EventType\":\"AccountUpdatedEvent\",\"Data\":{\"FirstName\":\"John Doe\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.UpdateAccountAsync(It.IsAny<AccountEntity>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMessageMissingData_ShouldNotUpdateAccount()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<AccountUpdatedEventHandler>>();
        var repositoryMock = new Mock<IAccountEventRepository>();
        var handler = new AccountUpdatedEventHandler(loggerMock.Object, repositoryMock.Object);
        var message = "{\"EventType\":\"AccountUpdatedEvent\"}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.UpdateAccountAsync(It.IsAny<AccountEntity>()), Times.Never);
    }
}
