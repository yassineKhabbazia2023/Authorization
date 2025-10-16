// <copyright file="RoleDeletedEventHandlerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Moq;
using Pulse.Authorization.Infrastructure.Providers;
using Pulse.Authorization.Infrastructure.Providers.Interfaces;

namespace Pulse.Authorization.Infrastructure.Tests.Providers;

public class RoleDeletedEventHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidMessage_ShouldRemoveRoleAndAuthorizations()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RoleDeletedEventHandler>>();
        var repositoryMock = new Mock<IRoleEventRepository>();

        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

        var handler = new RoleDeletedEventHandler(loggerMock.Object, repositoryMock.Object);
        var message = "{\"EventType\":\"RoleDeletedEvent\",\"Data\":{\"ContactId\":123,\"AccountId\":22}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.DeleteRoleAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        repositoryMock.Verify(repo => repo.DeleteContactAuthorizations(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNullMessage_ShouldNotRemoveRole()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RoleDeletedEventHandler>>();
        var repositoryMock = new Mock<IRoleEventRepository>();
        var handler = new RoleDeletedEventHandler(loggerMock.Object, repositoryMock.Object);

        // Act
        await handler.HandleAsync(null!);

        // Assert
        repositoryMock.Verify(repo => repo.DeleteRoleAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        repositoryMock.Verify(repo => repo.DeleteContactAuthorizations(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMessageMissingContactId_ShouldNotRemoveRole()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RoleDeletedEventHandler>>();
        var repositoryMock = new Mock<IRoleEventRepository>();
        var handler = new RoleDeletedEventHandler(loggerMock.Object, repositoryMock.Object);
        var message = "{\"EventType\":\"RoleDeletedEvent\",\"Data\":{\"AccountId\":22}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.DeleteRoleAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        repositoryMock.Verify(repo => repo.DeleteContactAuthorizations(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMessageMissingData_ShouldNotRemoveRole()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RoleDeletedEventHandler>>();
        var repositoryMock = new Mock<IRoleEventRepository>();
        var handler = new RoleDeletedEventHandler(loggerMock.Object, repositoryMock.Object);
        var message = "{\"EventType\":\"RoleDeletedEvent\"}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.DeleteRoleAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        repositoryMock.Verify(repo => repo.DeleteContactAuthorizations(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMessageAccountIdMinus1_ShouldNotRemoveRole()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RoleDeletedEventHandler>>();
        var repositoryMock = new Mock<IRoleEventRepository>();
        var handler = new RoleDeletedEventHandler(loggerMock.Object, repositoryMock.Object);
        var message = "{\"EventType\":\"RoleDeletedEvent\",\"Data\":{\"ContactId\":22,\"AccountId\":-2}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.DeleteRoleAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        repositoryMock.Verify(repo => repo.DeleteContactAuthorizations(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }
}
