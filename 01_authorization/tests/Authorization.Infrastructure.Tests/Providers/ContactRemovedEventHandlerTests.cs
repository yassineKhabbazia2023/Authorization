// <copyright file="ContactRemovedEventHandlerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Moq;
using Pulse.Authorization.Infrastructure.Providers;
using Pulse.Authorization.Infrastructure.Providers.Interfaces;
using Xunit;

namespace Pulse.Authorization.Infrastructure.Tests.Providers;

public class ContactRemovedEventHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidMessage_ShouldRemoveContactAndAuthorizations()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ContactRemovedEventHandler>>();
        var repositoryMock = new Mock<IContactEventRepository>();

        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

        var handler = new ContactRemovedEventHandler(loggerMock.Object, repositoryMock.Object);
        var message = "{\"EventType\":\"ContactRemovedEvent\",\"Data\":{\"ContactId\":123,\"FirstName\":\"John Doe\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.RemoveContactAsync(It.IsAny<int>()), Times.Once);
        repositoryMock.Verify(repo => repo.RemoveContactAuthorizationsAsync(It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNullMessage_ShouldNotRemoveContact()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ContactRemovedEventHandler>>();
        var repositoryMock = new Mock<IContactEventRepository>();
        var handler = new ContactRemovedEventHandler(loggerMock.Object, repositoryMock.Object);

        // Act
        await handler.HandleAsync(null!);

        // Assert
        repositoryMock.Verify(repo => repo.RemoveContactAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMessageMissingContactId_ShouldNotRemoveContact()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ContactRemovedEventHandler>>();
        var repositoryMock = new Mock<IContactEventRepository>();
        var handler = new ContactRemovedEventHandler(loggerMock.Object, repositoryMock.Object);
        var message = "{\"EventType\":\"ContactRemovedEvent\",\"Data\":{\"FirstName\":\"John Doe\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.RemoveContactAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMessageMissingData_ShouldNotRemoveContact()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ContactRemovedEventHandler>>();
        var repositoryMock = new Mock<IContactEventRepository>();
        var handler = new ContactRemovedEventHandler(loggerMock.Object, repositoryMock.Object);
        var message = "{\"EventType\":\"ContactRemovedEvent\"}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.RemoveContactAsync(It.IsAny<int>()), Times.Never);
    }
}
