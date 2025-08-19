// <copyright file="SubscriptionValidatedEventHandlerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Providers;
using Pulse.Authorization.Infrastructure.Providers.Interfaces;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Authorization.Infrastructure.Tests.Providers;

public class SubscriptionValidatedEventHandlerTests
{
    private readonly Mock<IAuthorizationEventPublisher> _authorizationEventPublisherMock = new(MockBehavior.Strict);

    public SubscriptionValidatedEventHandlerTests()
    {
        _authorizationEventPublisherMock.Setup(a => a.PublishAuthorizationUpdatedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>(), true))
            .Returns(Task.CompletedTask)
            .Verifiable();
    }

    [Fact]
    public async Task HandleAsync_WithValidMessage_ShouldUpdatePermissions()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SubscriptionValidatedEventHandler>>();
        var repositoryMock = new Mock<ISubscriptionEventRepository>();

        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

        var handler = new SubscriptionValidatedEventHandler(loggerMock.Object, repositoryMock.Object, _authorizationEventPublisherMock.Object);
        var message = "{\"EventType\":\"SubscriptionValidatedEvent\",\"Data\":{\"AccountId\":123,\"ContactIds\":[12,42,69], \"Products\": [{\"ProductCode\": \"MOCK\"}]}}";
        repositoryMock.Setup(r => r.AddSubscriptionAuthorizationsForContacts(
            It.IsAny<IEnumerable<int>>(),
            It.IsAny<int>(),
            It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(
            (new ContactAuthorizationEntity[]
            {
                new ContactAuthorizationEntity
                {
                    AccountId = 123,
                    ContactId = 12,
                    Authorization = new AuthorizationEntity
                    {
                        ProductCode = "MOCK"
                    }
                }
            }, []));

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.AddSubscriptionAuthorizationsOnAccountAsync(123, new string[] { "MOCK" }), Times.Once);
        repositoryMock.Verify(repo => repo.AddSubscriptionAuthorizationsForContacts(It.IsAny<IEnumerable<int>>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()), Times.Once);
        _authorizationEventPublisherMock.Verify(a => a.PublishAuthorizationUpdatedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>(), It.IsAny<bool>()), Times.AtLeastOnce);
    }


    [Fact]
    public async Task HandleAsync_WithValidMessage_ShouldUpdatePermissionsAndThrowException()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SubscriptionValidatedEventHandler>>();
        var repositoryMock = new Mock<ISubscriptionEventRepository>();
        IEnumerable<string> errors = ["Contact 1 does not exists", "Role of Account 2 with Contact 1 does not exists"];
        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

        var handler = new SubscriptionValidatedEventHandler(loggerMock.Object, repositoryMock.Object, _authorizationEventPublisherMock.Object);
        var message = "{\"EventType\":\"SubscriptionValidatedEvent\",\"Data\":{\"AccountId\":123,\"ContactIds\":[12,42,69], \"Products\": [{\"ProductCode\": \"MOCK\"}]}}";
        repositoryMock.Setup(repo => repo.AddSubscriptionAuthorizationsOnAccountAsync(123, new string[] { "MOCK" }));

        repositoryMock.Setup(r => r.AddSubscriptionAuthorizationsForContacts(
            It.IsAny<IEnumerable<int>>(),
            It.IsAny<int>(),
            It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(
            (new ContactAuthorizationEntity[]
            {
                new ContactAuthorizationEntity
                {
                    AccountId = 123,
                    ContactId = 12,
                    Authorization = new AuthorizationEntity
                    {
                        ProductCode = "MOCK"
                    }
                }
            }, errors));

        // Act
        var action = async () => await handler.HandleAsync(message);

        // Assert
        var exception = await action.Should().ThrowAsync<BadRequestException>();
        exception.Which.Code.Should().BeEquivalentTo("AUT013");
        exception.Which.Message.Should().BeEquivalentTo($"La souscription des authorization a confronté des problèmes:\n {string.Join("\n", errors.ToArray())}");
    }

    [Fact]
    public async Task HandleAsync_WithNullMessage_ShouldNotUpdatePermissions()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SubscriptionValidatedEventHandler>>();
        var repositoryMock = new Mock<ISubscriptionEventRepository>();
        var handler = new SubscriptionValidatedEventHandler(loggerMock.Object, repositoryMock.Object, _authorizationEventPublisherMock.Object);

        // Act
        await handler.HandleAsync(null!);

        // Assert
        repositoryMock.Verify(repo => repo.AddSubscriptionAuthorizationsOnAccountAsync(123, new string[] { "MOCK" }), Times.Never);
        repositoryMock.Verify(repo => repo.AddSubscriptionAuthorizationsOnAccountSignatoriesAsync(123, new string[] { "MOCK" }), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMessageMissingContactId_ShouldNotUpdatePermissions()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SubscriptionValidatedEventHandler>>();
        var repositoryMock = new Mock<ISubscriptionEventRepository>();
        var handler = new SubscriptionValidatedEventHandler(loggerMock.Object, repositoryMock.Object, _authorizationEventPublisherMock.Object);
        var message = "{\"EventType\":\"SubScriptionValidatedEvent\",\"Data\":{}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.AddSubscriptionAuthorizationsOnAccountAsync(123, new string[] { "MOCK" }), Times.Never);
        repositoryMock.Verify(repo => repo.AddSubscriptionAuthorizationsOnAccountSignatoriesAsync(123, new string[] { "MOCK" }), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMessageMissingData_ShouldNotUpdatePermissions()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SubscriptionValidatedEventHandler>>();
        var repositoryMock = new Mock<ISubscriptionEventRepository>();
        var handler = new SubscriptionValidatedEventHandler(loggerMock.Object, repositoryMock.Object, _authorizationEventPublisherMock.Object);
        var message = "{\"EventType\":\"SubScriptionValidatedEvent\"}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.AddSubscriptionAuthorizationsOnAccountAsync(123, new string[] { "MOCK" }), Times.Never);
        repositoryMock.Verify(repo => repo.AddSubscriptionAuthorizationsOnAccountSignatoriesAsync(123, new string[] { "MOCK" }), Times.Never);
    }
}
