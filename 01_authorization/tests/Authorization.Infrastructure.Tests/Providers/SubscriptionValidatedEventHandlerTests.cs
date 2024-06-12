// <copyright file="ContactUpdatedEventHandlerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Moq;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Providers;
using Pulse.Authorization.Infrastructure.Providers.Interfaces;
using Xunit;

namespace Pulse.Authorization.Infrastructure.Tests.Providers;

public class SubscriptionValidatedEventHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidMessage_ShouldUpdateContact()
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

        var handler = new SubscriptionValidatedEventHandler(loggerMock.Object, repositoryMock.Object);
        var message = "{\"EventType\":\"SubscriptionValidatedEvent\",\"Data\":{\"AccountId\":123,\"ContactIds\":[12,42,69], \"Products\": [{\"ProductCode\": \"MOCK\"}]}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.AddSubscriptionAuthorizationsOnAccountAsync(123, new string[] { "MOCK" }), Times.Once);
        repositoryMock.Verify(repo => repo.AddSubscriptionAuthorizationsOnAccountSignatoriesAsync(123, new int[] { 12, 42, 69 }, new string[] { "MOCK" }), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNullMessage_ShouldNotUpdateContact()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ContactUpdatedEventHandler>>();
        var repositoryMock = new Mock<IContactEventRepository>();
        var handler = new ContactUpdatedEventHandler(loggerMock.Object, repositoryMock.Object);

        // Act
        await handler.HandleAsync(null!);

        // Assert
        repositoryMock.Verify(repo => repo.UpdateContactAsync(It.IsAny<ContactEntity>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMessageMissingContactId_ShouldNotUpdateContact()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ContactUpdatedEventHandler>>();
        var repositoryMock = new Mock<IContactEventRepository>();
        var handler = new ContactUpdatedEventHandler(loggerMock.Object, repositoryMock.Object);
        var message = "{\"EventType\":\"ContactUpdatedEvent\",\"Data\":{\"FirstName\":\"John Doe\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.UpdateContactAsync(It.IsAny<ContactEntity>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMessageMissingData_ShouldNotUpdateContact()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ContactUpdatedEventHandler>>();
        var repositoryMock = new Mock<IContactEventRepository>();
        var handler = new ContactUpdatedEventHandler(loggerMock.Object, repositoryMock.Object);
        var message = "{\"EventType\":\"ContactUpdatedEvent\"}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.UpdateContactAsync(It.IsAny<ContactEntity>()), Times.Never);
    }
}
