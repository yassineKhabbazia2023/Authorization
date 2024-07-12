// <copyright file="RoleUpdatedEventHandlerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Moq;
using Pulse.Authorization.Infrastructure.Providers.Interfaces;
using Pulse.Authorization.Infrastructure.Providers;
using Pulse.Authorization.Infrastructure.Entities;

namespace Pulse.Authorization.Infrastructure.Tests.Providers
{
    public class RoleUpdatedEventHandlerTests
    {
        [Fact]
        public async Task HandleAsync_WithValidMessage_ShouldUpdateRole()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RoleUpdatedEventHandler>>();
            var repositoryMock = new Mock<IRoleEventRepository>();

            loggerMock.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

            var handler = new RoleUpdatedEventHandler(loggerMock.Object, repositoryMock.Object);
            var message = "{\"EventType\":\"RoleUpdatedEvent\",\"Data\":{\"ContactId\":123,\"AccountId\":22, \"IsSignatory\":1}}";

            // Act
            await handler.HandleAsync(message);

            // Assert
            repositoryMock.Verify(repo => repo.UpdateRoleAsync(It.IsAny<RoleEntity>()), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_WithNullMessage_ShouldNotUpdateRole()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RoleUpdatedEventHandler>>();
            var repositoryMock = new Mock<IRoleEventRepository>();
            var handler = new RoleUpdatedEventHandler(loggerMock.Object, repositoryMock.Object);

            // Act
            await handler.HandleAsync(null!);

            // Assert
            repositoryMock.Verify(repo => repo.UpdateRoleAsync(It.IsAny<RoleEntity>()), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_WithMessageMissingContactId_ShouldNotUpdateRole()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RoleUpdatedEventHandler>>();
            var repositoryMock = new Mock<IRoleEventRepository>();
            var handler = new RoleUpdatedEventHandler(loggerMock.Object, repositoryMock.Object);
            var message = "{\"EventType\":\"RoleUpdatedEvent\",\"Data\":{\"AccountId\":22}}";

            // Act
            await handler.HandleAsync(message);

            // Assert
            repositoryMock.Verify(repo => repo.UpdateRoleAsync(It.IsAny<RoleEntity>()), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_WithMessageMissingData_ShouldNotUpdateRole()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RoleUpdatedEventHandler>>();
            var repositoryMock = new Mock<IRoleEventRepository>();
            var handler = new RoleUpdatedEventHandler(loggerMock.Object, repositoryMock.Object);
            var message = "{\"EventType\":\"RoleUpdatedEvent\"}";

            // Act
            await handler.HandleAsync(message);

            // Assert
            repositoryMock.Verify(repo => repo.UpdateRoleAsync(It.IsAny<RoleEntity>()), Times.Never);
        }
    }
}
