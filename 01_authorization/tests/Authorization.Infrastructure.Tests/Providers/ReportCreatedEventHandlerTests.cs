// <copyright file="AuthorizationCreatedEventHandlerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Moq;
using Pulse.Authorization.Infrastructure.Constants;
using Pulse.Authorization.Infrastructure.Interfaces;
using Pulse.Authorization.Infrastructure.Providers;
using Pulse.Authorization.Infrastructure.Providers.Interfaces;

namespace Pulse.Authorization.Infrastructure.Tests.Providers;

public class ReportCreatedEventHandlerTests
{
    private readonly Mock<IAuthorizationEventPublisher> _authorizationEventPublisherMock = new(MockBehavior.Strict);
    private readonly Mock<IAuthorizationRepository> _authorizationRepositoryMock = new(MockBehavior.Strict);

    public ReportCreatedEventHandlerTests()
    {
        _authorizationEventPublisherMock.Setup(a => a.PublishAuthorizationUpdatedEventAsync(It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>(), false))
            .Returns(Task.CompletedTask)
            .Verifiable();
        _authorizationRepositoryMock.Setup(a => a.CreateRapportBIAuthorizationsOnAccountAsync(It.IsAny<int>(), It.IsAny<string[]>()))
            .ReturnsAsync(GlobalConstants.PowerBIDefaultPermissions)
            .Verifiable();
    }

    [Fact]
    public async Task HandleAsync_WithValidMessage_ShouldCreatesAuthorization()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ReportCreatedEventHandler>>();

        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

        var handler = new ReportCreatedEventHandler(loggerMock.Object, _authorizationRepositoryMock.Object, _authorizationEventPublisherMock.Object);
        var message = "{\"EventType\":\"AuthorizationCreatedEvent\",\"Data\":{\"AccountId\":123,\"Codes\":[\"code1\",\"code2\"]}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        _authorizationRepositoryMock.Verify();
        _authorizationEventPublisherMock.Verify();
    }

    [Fact]
    public async Task HandleAsync_WithNullMessage_ShouldNotCreateAuthorization()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ReportCreatedEventHandler>>();
        var handler = new ReportCreatedEventHandler(loggerMock.Object, _authorizationRepositoryMock.Object, _authorizationEventPublisherMock.Object);

        // Act
        await handler.HandleAsync(null!);

        // Assert
        _authorizationRepositoryMock.Verify(repo => repo.CreateRapportBIAuthorizationsOnAccountAsync(It.IsAny<int>(), It.IsAny<string[]>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMessageMissingAccountId_ShouldNotCreateAuthorization()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ReportCreatedEventHandler>>();
        var handler = new ReportCreatedEventHandler(loggerMock.Object, _authorizationRepositoryMock.Object, _authorizationEventPublisherMock.Object);
        var message = "{\"EventType\":\"ReportCreatedEvent\",\"Data\":{\"Codes\":[\"code1\",\"code2\"]}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        _authorizationRepositoryMock.Verify(repo => repo.CreateRapportBIAuthorizationsOnAccountAsync(It.IsAny<int>(), It.IsAny<string[]>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMessageMissingData_ShouldNotCreateAuthorization()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ReportCreatedEventHandler>>();
        var handler = new ReportCreatedEventHandler(loggerMock.Object, _authorizationRepositoryMock.Object, _authorizationEventPublisherMock.Object);
        var message = "{\"EventType\":\"ReportCreatedEvent\"}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        _authorizationRepositoryMock.Verify(repo => repo.CreateRapportBIAuthorizationsOnAccountAsync(It.IsAny<int>(), It.IsAny<string[]>()), Times.Never);
    }
}
