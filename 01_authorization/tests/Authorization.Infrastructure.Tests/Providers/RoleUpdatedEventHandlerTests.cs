// <copyright file="RoleUpdatedEventHandlerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Moq;
using Pulse.Authorization.Infrastructure.Providers.Interfaces;
using Pulse.Authorization.Infrastructure.Providers;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Interfaces;

namespace Pulse.Authorization.Infrastructure.Tests.Providers
{
    public class RoleUpdatedEventHandlerTests
    {
        [Fact]
        public async Task HandleAsync_WithValidMessage_ShouldUpdateRoleAndAddAuthorizationForContact()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RoleUpdatedEventHandler>>();
            var repositoryMock = new Mock<IRoleEventRepository>();
            var authorizationReposMock = new Mock<IAuthorizationRepository>();
            loggerMock.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

            var existingRole = new RoleEntity
            {
                ContactId = 123,
                AccountId = 22,
                IsSignatory = false
            };

            repositoryMock.Setup(r => r.GetRole(It.IsAny<RoleEntity>())).ReturnsAsync(existingRole);
            authorizationReposMock.Setup(a => a.SetContactAuthorizationFromAccountAuthorization(It.IsAny<int>(), It.IsAny<int>()));

            var handler = new RoleUpdatedEventHandler(loggerMock.Object, repositoryMock.Object, authorizationReposMock.Object);
            var message = "{\"EventType\":\"RoleUpdatedEvent\",\"Data\":{\"ContactId\":123,\"AccountId\":22, \"IsSignatory\":1}}";

            // Act
            await handler.HandleAsync(message);

            // Assert
            repositoryMock.Verify(repo => repo.UpdateRoleAsync(It.IsAny<RoleEntity>()), Times.Once);
            repositoryMock.Verify(repo => repo.GetRole(It.IsAny<RoleEntity>()), Times.Once);
            authorizationReposMock.Verify(repo => repo.SetContactAuthorizationFromAccountAuthorization(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_WithValidMessage_WithNonSignatory_ShouldUpdateRoleOnly()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RoleUpdatedEventHandler>>();
            var repositoryMock = new Mock<IRoleEventRepository>();
            var authorizationReposMock = new Mock<IAuthorizationRepository>();
            loggerMock.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

            var existingRole = new RoleEntity
            {
                ContactId = 123,
                AccountId = 22,
                IsSignatory = false
            };

            repositoryMock.Setup(r => r.GetRole(It.IsAny<RoleEntity>())).ReturnsAsync(existingRole);
            authorizationReposMock.Setup(a => a.SetContactAuthorizationFromAccountAuthorization(It.IsAny<int>(), It.IsAny<int>()));

            var handler = new RoleUpdatedEventHandler(loggerMock.Object, repositoryMock.Object, authorizationReposMock.Object);
            var message = "{\"EventType\":\"RoleUpdatedEvent\",\"Data\":{\"ContactId\":123,\"AccountId\":22, \"IsSignatory\":0}}";

            // Act
            await handler.HandleAsync(message);

            // Assert
            repositoryMock.Verify(repo => repo.UpdateRoleAsync(It.IsAny<RoleEntity>()), Times.Once);
            repositoryMock.Verify(repo => repo.GetRole(It.IsAny<RoleEntity>()), Times.Once);
            authorizationReposMock.Verify(repo => repo.SetContactAuthorizationFromAccountAuthorization(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_WithNonSignatoryRole_WhenExistingRoleWasSignatory_ShouldDeleteContactPermissions()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RoleUpdatedEventHandler>>();
            var repositoryMock = new Mock<IRoleEventRepository>();
            var authorizationReposMock = new Mock<IAuthorizationRepository>();
            loggerMock.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

            var existingRole = new RoleEntity
            {
                ContactId = 123,
                AccountId = 22,
                IsSignatory = true // Le rôle existant était signataire
            };

            repositoryMock.Setup(r => r.GetRole(It.IsAny<RoleEntity>())).ReturnsAsync(existingRole);
            authorizationReposMock.Setup(a => a.DeleteContactAuthorizationsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string[]>()));

            var handler = new RoleUpdatedEventHandler(loggerMock.Object, repositoryMock.Object, authorizationReposMock.Object);
            var message = "{\"EventType\":\"RoleUpdatedEvent\",\"Data\":{\"ContactId\":123,\"AccountId\":22, \"IsSignatory\":0}}";

            // Act
            await handler.HandleAsync(message);

            // Assert
            repositoryMock.Verify(repo => repo.UpdateRoleAsync(It.IsAny<RoleEntity>()), Times.Once);
            repositoryMock.Verify(repo => repo.GetRole(It.IsAny<RoleEntity>()), Times.Once);
            authorizationReposMock.Verify(repo => repo.DeleteContactAuthorizationsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string[]>()), Times.Once);
            authorizationReposMock.Verify(repo => repo.SetContactAuthorizationFromAccountAuthorization(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_WithNullMessage_ShouldNotUpdateRole()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RoleUpdatedEventHandler>>();
            var repositoryMock = new Mock<IRoleEventRepository>();
            var authorizationReposMock = Mock.Of<IAuthorizationRepository>();
            var handler = new RoleUpdatedEventHandler(loggerMock.Object, repositoryMock.Object, authorizationReposMock);

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
            var authorizationReposMock = Mock.Of<IAuthorizationRepository>();
            var handler = new RoleUpdatedEventHandler(loggerMock.Object, repositoryMock.Object, authorizationReposMock);
            var message = "{\"EventType\":\"RoleUpdatedEvent\",\"Data\":{\"AccountId\":22}}";

            // Act
            await handler.HandleAsync(message);

            // Assert
            repositoryMock.Verify(repo => repo.UpdateRoleAsync(It.IsAny<RoleEntity>()), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_WithMessageInvalidAccountId_ShouldNotUpdateRole()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RoleUpdatedEventHandler>>();
            var repositoryMock = new Mock<IRoleEventRepository>();
            var authorizationReposMock = Mock.Of<IAuthorizationRepository>();
            var handler = new RoleUpdatedEventHandler(loggerMock.Object, repositoryMock.Object, authorizationReposMock);
            var message = "{\"EventType\":\"RoleUpdatedEvent\",\"Data\":{\"ContactId\":123,\"AccountId\":-2}}";

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
            var authorizationReposMock = Mock.Of<IAuthorizationRepository>();
            var handler = new RoleUpdatedEventHandler(loggerMock.Object, repositoryMock.Object, authorizationReposMock);
            var message = "{\"EventType\":\"RoleUpdatedEvent\"}";

            // Act
            await handler.HandleAsync(message);

            // Assert
            repositoryMock.Verify(repo => repo.UpdateRoleAsync(It.IsAny<RoleEntity>()), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_WithNonSignatoryRole_WhenNoExistingRole_ShouldNotDeletePermissions()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RoleUpdatedEventHandler>>();
            var repositoryMock = new Mock<IRoleEventRepository>();
            var authorizationReposMock = new Mock<IAuthorizationRepository>();
            loggerMock.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

            repositoryMock.Setup(r => r.GetRole(It.IsAny<RoleEntity>())).ReturnsAsync((RoleEntity)null);

            var handler = new RoleUpdatedEventHandler(loggerMock.Object, repositoryMock.Object, authorizationReposMock.Object);
            var message = "{\"EventType\":\"RoleUpdatedEvent\",\"Data\":{\"ContactId\":123,\"AccountId\":22, \"IsSignatory\":0}}";

            // Act
            await handler.HandleAsync(message);

            // Assert
            repositoryMock.Verify(repo => repo.UpdateRoleAsync(It.IsAny<RoleEntity>()), Times.Once);
            repositoryMock.Verify(repo => repo.GetRole(It.IsAny<RoleEntity>()), Times.Once);
            authorizationReposMock.Verify(repo => repo.DeleteContactAuthorizationsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string[]>()), Times.Never);
            authorizationReposMock.Verify(repo => repo.SetContactAuthorizationFromAccountAuthorization(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_WithSignatoryRole_WhenNoExistingRole_ShouldSetAuthorizations()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RoleUpdatedEventHandler>>();
            var repositoryMock = new Mock<IRoleEventRepository>();
            var authorizationReposMock = new Mock<IAuthorizationRepository>();
            loggerMock.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

            repositoryMock.Setup(r => r.GetRole(It.IsAny<RoleEntity>())).ReturnsAsync((RoleEntity)null);
            authorizationReposMock.Setup(a => a.SetContactAuthorizationFromAccountAuthorization(It.IsAny<int>(), It.IsAny<int>()));

            var handler = new RoleUpdatedEventHandler(loggerMock.Object, repositoryMock.Object, authorizationReposMock.Object);
            var message = "{\"EventType\":\"RoleUpdatedEvent\",\"Data\":{\"ContactId\":123,\"AccountId\":22, \"IsSignatory\":1}}";

            // Act
            await handler.HandleAsync(message);

            // Assert
            repositoryMock.Verify(repo => repo.UpdateRoleAsync(It.IsAny<RoleEntity>()), Times.Once);
            repositoryMock.Verify(repo => repo.GetRole(It.IsAny<RoleEntity>()), Times.Once);
            authorizationReposMock.Verify(repo => repo.SetContactAuthorizationFromAccountAuthorization(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            authorizationReposMock.Verify(repo => repo.DeleteContactAuthorizationsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string[]>()), Times.Never);
        }
    }
}
