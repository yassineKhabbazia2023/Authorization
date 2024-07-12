// <copyright file="RoleCreatedEventHandlerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Moq;
using Pulse.Authorization.Infrastructure.Providers.Interfaces;
using Pulse.Authorization.Infrastructure.Providers;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Interfaces;
using System.Linq.Expressions;
using Pulse.Authorization.Infrastructure.Constants;

namespace Pulse.Authorization.Infrastructure.Tests.Providers
{
    public class RoleCreatedEventHandlerTests
    {
        private readonly Mock<IAuthorizationEventPublisher> _authorizationEventPublisherMock = new(MockBehavior.Strict);
        private readonly Mock<IAuthorizationRepository> _authorizationRepository = new(MockBehavior.Strict);

        public RoleCreatedEventHandlerTests()
        {
            _authorizationRepository.Setup(a => a.CreateDefaultAuthorizationsOnSignatoryAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(GlobalConstants.DefaultSignatoryPermissions)
                .Verifiable();

            _authorizationEventPublisherMock.Setup(a => a.PublishAuthorizationUpdatedEventAsync(It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                .Returns(Task.CompletedTask)
                .Verifiable();
        }

        [Fact]
        public async Task HandleAsync_WithValidMessage_ShouldCreatesRole()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RoleCreatedEventHandler>>();
            var repositoryMock = new Mock<IRoleEventRepository>();

            loggerMock.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

            var handler = new RoleCreatedEventHandler(loggerMock.Object, repositoryMock.Object, _authorizationEventPublisherMock.Object, _authorizationRepository.Object);
            var message = "{\"EventType\":\"RoleCreatedEvent\",\"Data\":{\"ContactId\":123,\"AccountId\":\"22\",\"IsSignatory\":1,\"IsFavorite\":1,\"IsDelegation\":1,}}";

            // Act
            await handler.HandleAsync(message);

            // Assert
            repositoryMock.Verify(repo => repo.CreateRoleAsync(It.IsAny<RoleEntity>()), Times.Once);
            _authorizationEventPublisherMock.Verify();
            _authorizationRepository.Verify();
        }

        [Fact]
        public async Task HandleAsync_WithNullMessage_ShouldNotCreateRole()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RoleCreatedEventHandler>>();
            var repositoryMock = new Mock<IRoleEventRepository>();
            var handler = new RoleCreatedEventHandler(loggerMock.Object, repositoryMock.Object, _authorizationEventPublisherMock.Object, _authorizationRepository.Object);

            // Act
            await handler.HandleAsync(null!);

            // Assert
            repositoryMock.Verify(repo => repo.CreateRoleAsync(It.IsAny<RoleEntity>()), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_WithInvalidMessage_ShouldNotCreateRole()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RoleCreatedEventHandler>>();
            var repositoryMock = new Mock<IRoleEventRepository>();
            var handler = new RoleCreatedEventHandler(loggerMock.Object, repositoryMock.Object, _authorizationEventPublisherMock.Object, _authorizationRepository.Object);
            var message = "{\"EventType\":\"RoleCreatedEvent\",\"Data\":{\"ContactId\":123,\"AccountId\":\"-2\",\"IsSignatory\":1,\"IsFavorite\":1,\"IsDelegation\":1,}}";

            // Act
            await handler.HandleAsync(message);

            // Assert
            repositoryMock.Verify(repo => repo.CreateRoleAsync(It.IsAny<RoleEntity>()), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_WithMessageMissingData_ShouldNotCreateRole()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RoleCreatedEventHandler>>();
            var repositoryMock = new Mock<IRoleEventRepository>();
            var handler = new RoleCreatedEventHandler(loggerMock.Object, repositoryMock.Object, _authorizationEventPublisherMock.Object, _authorizationRepository.Object);
            var message = "{\"EventType\":\"RoleCreatedEvent\"}";

            // Act
            await handler.HandleAsync(message);

            // Assert
            repositoryMock.Verify(repo => repo.CreateRoleAsync(It.IsAny<RoleEntity>()), Times.Never);
        }
    }
}
