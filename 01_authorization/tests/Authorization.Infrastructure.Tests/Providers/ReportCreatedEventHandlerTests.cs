// <copyright file="ReportCreatedEventHandlerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using Pulse.Authorization.Core.Interfaces;
using Pulse.Authorization.Infrastructure.Constants;
using Pulse.Authorization.Infrastructure.Interfaces;
using Pulse.Authorization.Infrastructure.Providers;
using Pulse.Authorization.Infrastructure.Providers.Interfaces;

namespace Pulse.Authorization.Infrastructure.Tests.Providers;

public class ReportCreatedEventHandlerTests
{
    private readonly Mock<IAuthorizationEventPublisher> _authorizationEventPublisherMock = new(MockBehavior.Strict);
    private readonly Mock<IAuthorizationRepository> _authorizationRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IContactRepository> _contactRepository = new(MockBehavior.Strict);
    private readonly Mock<ILogger<ReportCreatedEventHandler>> _logger = new Mock<ILogger<ReportCreatedEventHandler>>();
    private readonly Fixture _fixture;

    public ReportCreatedEventHandlerTests()
    {
        _authorizationEventPublisherMock.Setup(a => a.PublishAuthorizationCreatedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
            .Returns(Task.CompletedTask)
            .Verifiable();
        _authorizationEventPublisherMock.Setup(a => a.PublishAuthorizationUpdatedEventAsync(It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>(), false))
            .Returns(Task.CompletedTask)
            .Verifiable();

        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public async Task HandleAsync_WithValidMessage_ShouldCreatesAuthorization()
    {
        // Arrange
        var contact = _fixture.CreateMany<int>();
        _contactRepository.Setup(c => c.GetSignatoriesAsync(It.IsAny<int>()))
            .ReturnsAsync(contact)
            .Verifiable();

        var createdAuthorizations = new List<(int, string)>
        {
            (8, "code1"),
            (8, "code2"),
            (9, "code1")
        };
        _authorizationRepositoryMock.Setup(a => a.CreateReportingAuthorizationsForSignatoriesAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<int>()))
            .ReturnsAsync(createdAuthorizations);

        _authorizationRepositoryMock.Setup(a => a.CreateReportingAuthorizationsOnAccountAsync(It.IsAny<int>(), It.IsAny<string[]>()))
            .ReturnsAsync(GlobalConstants.PowerBIDefaultPermissions)
            .Verifiable();

        var handler = new ReportCreatedEventHandler(_logger.Object, _authorizationRepositoryMock.Object, _contactRepository.Object, _authorizationEventPublisherMock.Object);
        var message = "{\"EventType\":\"ReportCreatedEvent\",\"Data\":{\"AccountId\":123,\"ReportId\":456}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        _authorizationRepositoryMock.Verify(a => a.CreateReportingAuthorizationsOnAccountAsync(It.IsAny<int>(), It.IsAny<string[]>()), Times.Once);
        _authorizationRepositoryMock.Verify(a => a.CreateReportingAuthorizationsForSignatoriesAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<int>()), Times.Once);
        _authorizationEventPublisherMock.Verify(a => a.PublishAuthorizationUpdatedEventAsync(It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>(), It.IsAny<bool>()), Times.Once);
        _authorizationEventPublisherMock.Verify(a => a.PublishAuthorizationCreatedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()), Times.Exactly(2));
        _contactRepository.Verify(c => c.GetSignatoriesAsync(It.IsAny<int>()), Times.Once);
        _logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        _logger.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithNoCreatedAuthorizations_ShouldNotPublishAuthorizationUpdatedEventAsync()
    {
        // Arrange
        var contact = _fixture.CreateMany<int>();
        _contactRepository.Setup(c => c.GetSignatoriesAsync(It.IsAny<int>()))
            .ReturnsAsync(contact)
            .Verifiable();

        var createdSignatoriesAuthorizations = new List<(int, string)>
        {
            (1, "code1"),
            (2, "code2")
        };
        _authorizationRepositoryMock.Setup(a => a.CreateReportingAuthorizationsForSignatoriesAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<int>()))
            .ReturnsAsync(createdSignatoriesAuthorizations);

        var createdAuthorizations = Enumerable.Empty<string>();
        _authorizationRepositoryMock.Setup(a => a.CreateReportingAuthorizationsOnAccountAsync(It.IsAny<int>(), It.IsAny<string[]>()))
            .ReturnsAsync(createdAuthorizations)
            .Verifiable();

        var handler = new ReportCreatedEventHandler(_logger.Object, _authorizationRepositoryMock.Object, _contactRepository.Object, _authorizationEventPublisherMock.Object);
        var message = "{\"EventType\":\"ReportCreatedEvent\",\"Data\":{\"AccountId\":123,\"ReportId\":456}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        _authorizationEventPublisherMock.Verify(a => a.PublishAuthorizationUpdatedEventAsync(It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>(), It.IsAny<bool>()), Times.Never);
        _authorizationEventPublisherMock.Verify(a => a.PublishAuthorizationCreatedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()), Times.Exactly(2));
        _logger.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNoSignatories_ShouldNotPublishAuthorizationCreatedEventAsync()
    {
        // Arrange
        _contactRepository.Setup(c => c.GetSignatoriesAsync(It.IsAny<int>()))
            .ReturnsAsync(Enumerable.Empty<int>())
            .Verifiable();

        var createdSignatoriesAuthorizations = new List<(int, string)>
        {
            (1, "code1"),
            (2, "code2")
        };
        _authorizationRepositoryMock.Setup(a => a.CreateReportingAuthorizationsForSignatoriesAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<int>()))
            .ReturnsAsync(createdSignatoriesAuthorizations);

        _authorizationRepositoryMock.Setup(a => a.CreateReportingAuthorizationsOnAccountAsync(It.IsAny<int>(), It.IsAny<string[]>()))
            .ReturnsAsync(GlobalConstants.DefaultAccountPermissions)
            .Verifiable();

        var handler = new ReportCreatedEventHandler(_logger.Object, _authorizationRepositoryMock.Object, _contactRepository.Object, _authorizationEventPublisherMock.Object);
        var message = "{\"EventType\":\"ReportCreatedEvent\",\"Data\":{\"AccountId\":123,\"ReportId\":456}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        _authorizationEventPublisherMock.Verify(a => a.PublishAuthorizationUpdatedEventAsync(It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>(), It.IsAny<bool>()), Times.Once);
        _authorizationEventPublisherMock.Verify(a => a.PublishAuthorizationCreatedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()), Times.Never);
        _logger.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithNoCreatedSignatoryAuthorizations_ShouldNotPublishAuthorizationCreatedEventAsync()
    {
        // Arrange
        var contacts = _fixture.CreateMany<int>();
        _contactRepository.Setup(c => c.GetSignatoriesAsync(It.IsAny<int>()))
            .ReturnsAsync(contacts)
            .Verifiable();

        _authorizationRepositoryMock.Setup(a => a.CreateReportingAuthorizationsForSignatoriesAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<int>()))
            .ReturnsAsync(Enumerable.Empty<(int, string)>());

        _authorizationRepositoryMock.Setup(a => a.CreateReportingAuthorizationsOnAccountAsync(It.IsAny<int>(), It.IsAny<string[]>()))
            .ReturnsAsync(GlobalConstants.DefaultAccountPermissions)
            .Verifiable();

        var handler = new ReportCreatedEventHandler(_logger.Object, _authorizationRepositoryMock.Object, _contactRepository.Object, _authorizationEventPublisherMock.Object);
        var message = "{\"EventType\":\"ReportCreatedEvent\",\"Data\":{\"AccountId\":123,\"ReportId\":456}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        _authorizationEventPublisherMock.Verify(a => a.PublishAuthorizationUpdatedEventAsync(It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>(), It.IsAny<bool>()), Times.Once);
        _authorizationEventPublisherMock.Verify(a => a.PublishAuthorizationCreatedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()), Times.Never);
        _logger.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNullMessage_ShouldNotCreateAuthorization()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ReportCreatedEventHandler>>();
        var handler = new ReportCreatedEventHandler(loggerMock.Object, _authorizationRepositoryMock.Object, _contactRepository.Object, _authorizationEventPublisherMock.Object);

        // Act
        await handler.HandleAsync(null!);

        // Assert
        _authorizationRepositoryMock.Verify(repo => repo.CreateReportingAuthorizationsOnAccountAsync(It.IsAny<int>(), It.IsAny<string[]>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMessageMissingAccountId_ShouldNotCreateAuthorization()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ReportCreatedEventHandler>>();
        var handler = new ReportCreatedEventHandler(loggerMock.Object, _authorizationRepositoryMock.Object, _contactRepository.Object, _authorizationEventPublisherMock.Object);
        var message = "{\"EventType\":\"ReportCreatedEvent\",\"Data\":{\"ReportId\":456}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        _authorizationRepositoryMock.Verify(repo => repo.CreateReportingAuthorizationsOnAccountAsync(It.IsAny<int>(), It.IsAny<string[]>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMessageMissingData_ShouldNotCreateAuthorization()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ReportCreatedEventHandler>>();
        var handler = new ReportCreatedEventHandler(loggerMock.Object, _authorizationRepositoryMock.Object, _contactRepository.Object, _authorizationEventPublisherMock.Object);
        var message = "{\"EventType\":\"ReportCreatedEvent\"}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        _authorizationRepositoryMock.Verify(repo => repo.CreateReportingAuthorizationsOnAccountAsync(It.IsAny<int>(), It.IsAny<string[]>()), Times.Never);
    }
}
