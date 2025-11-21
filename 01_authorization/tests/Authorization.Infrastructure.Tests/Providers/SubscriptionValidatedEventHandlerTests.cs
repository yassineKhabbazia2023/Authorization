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
public async Task HandleAsync_WithValidMessage_ShouldUpdatePermissionsAndLogWarningOnBusinessError()
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
    Func<Task> action = async () => await handler.HandleAsync(message);

    // Assert
    await action.Should().NotThrowAsync(); // <--- le handler ne jette plus d'exception métier

    // On vérifie l'appel du log Warning sur les erreurs métier
    loggerMock.Verify(
        x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Erreurs d'autorisation") && o.ToString().Contains(errors.First())),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);

    // Vérifie que les méthodes de repository sont appelées
    repositoryMock.Verify(
        x => x.AddSubscriptionAuthorizationsOnAccountAsync(123, It.IsAny<IEnumerable<string>>()), 
        Times.Once);

    repositoryMock.Verify(
        x => x.AddSubscriptionAuthorizationsForContacts(
            It.IsAny<IEnumerable<int>>(),
            123,
            It.IsAny<IEnumerable<string>>()), 
        Times.Once);
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

    [Fact]
    public async Task HandleAsync_WithNullData_ShouldLogErrorAndNotUpdatePermissions()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SubscriptionValidatedEventHandler>>();
        var repositoryMock = new Mock<ISubscriptionEventRepository>();
        var handler = new SubscriptionValidatedEventHandler(loggerMock.Object, repositoryMock.Object, _authorizationEventPublisherMock.Object);
        var message = "{\"EventType\":\"SubscriptionValidatedEvent\",\"Data\":null}";

        loggerMock.Setup(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.AddSubscriptionAuthorizationsOnAccountAsync(It.IsAny<int>(), It.IsAny<IEnumerable<string>>()), Times.Never);
        repositoryMock.Verify(repo => repo.AddSubscriptionAuthorizationsForContacts(It.IsAny<IEnumerable<int>>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()), Times.Never);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Payload Data null")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidAccountId_ShouldLogErrorAndNotUpdatePermissions()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SubscriptionValidatedEventHandler>>();
        var repositoryMock = new Mock<ISubscriptionEventRepository>();
        var handler = new SubscriptionValidatedEventHandler(loggerMock.Object, repositoryMock.Object, _authorizationEventPublisherMock.Object);
        var message = "{\"EventType\":\"SubscriptionValidatedEvent\",\"Data\":{\"AccountId\":0,\"ContactIds\":[12], \"Products\": [{\"ProductCode\": \"MOCK\"}]}}";

        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.AddSubscriptionAuthorizationsOnAccountAsync(It.IsAny<int>(), It.IsAny<IEnumerable<string>>()), Times.Never);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("AccountId <= 0")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyProducts_ShouldLogErrorAndNotUpdatePermissions()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SubscriptionValidatedEventHandler>>();
        var repositoryMock = new Mock<ISubscriptionEventRepository>();
        var handler = new SubscriptionValidatedEventHandler(loggerMock.Object, repositoryMock.Object, _authorizationEventPublisherMock.Object);
        var message = "{\"EventType\":\"SubscriptionValidatedEvent\",\"Data\":{\"AccountId\":123,\"ContactIds\":[12], \"Products\": []}}";

        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.AddSubscriptionAuthorizationsOnAccountAsync(It.IsAny<int>(), It.IsAny<IEnumerable<string>>()), Times.Never);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Products list is null or empty")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyContactIds_ShouldLogErrorAndNotUpdatePermissions()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SubscriptionValidatedEventHandler>>();
        var repositoryMock = new Mock<ISubscriptionEventRepository>();
        var handler = new SubscriptionValidatedEventHandler(loggerMock.Object, repositoryMock.Object, _authorizationEventPublisherMock.Object);
        var message = "{\"EventType\":\"SubscriptionValidatedEvent\",\"Data\":{\"AccountId\":123,\"ContactIds\":[], \"Products\": [{\"ProductCode\": \"MOCK\"}]}}";

        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.AddSubscriptionAuthorizationsOnAccountAsync(It.IsAny<int>(), It.IsAny<IEnumerable<string>>()), Times.Never);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ContactIds is null or empty")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithAllEmptyProductCodes_ShouldLogErrorAndNotUpdatePermissions()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SubscriptionValidatedEventHandler>>();
        var repositoryMock = new Mock<ISubscriptionEventRepository>();
        var handler = new SubscriptionValidatedEventHandler(loggerMock.Object, repositoryMock.Object, _authorizationEventPublisherMock.Object);
        var message = "{\"EventType\":\"SubscriptionValidatedEvent\",\"Data\":{\"AccountId\":123,\"ContactIds\":[12], \"Products\": [{\"ProductCode\": \"\"}, {\"ProductCode\": null}]}}";

        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.AddSubscriptionAuthorizationsOnAccountAsync(It.IsAny<int>(), It.IsAny<IEnumerable<string>>()), Times.Never);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Tous les ProductCodes sont vides ou nuls")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithAllInvalidContactIds_ShouldLogErrorAndNotUpdatePermissions()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SubscriptionValidatedEventHandler>>();
        var repositoryMock = new Mock<ISubscriptionEventRepository>();
        var handler = new SubscriptionValidatedEventHandler(loggerMock.Object, repositoryMock.Object, _authorizationEventPublisherMock.Object);
        var message = "{\"EventType\":\"SubscriptionValidatedEvent\",\"Data\":{\"AccountId\":123,\"ContactIds\":[0, -1, -5], \"Products\": [{\"ProductCode\": \"MOCK\"}]}}";

        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.AddSubscriptionAuthorizationsOnAccountAsync(It.IsAny<int>(), It.IsAny<IEnumerable<string>>()), Times.Never);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Aucun ContactId valide trouvé")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithMixedValidInvalidContactIds_ShouldFilterAndProcessOnlyValid()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SubscriptionValidatedEventHandler>>();
        var repositoryMock = new Mock<ISubscriptionEventRepository>();
        var handler = new SubscriptionValidatedEventHandler(loggerMock.Object, repositoryMock.Object, _authorizationEventPublisherMock.Object);
        var message = "{\"EventType\":\"SubscriptionValidatedEvent\",\"Data\":{\"AccountId\":123,\"ContactIds\":[0, 12, -1, 42], \"Products\": [{\"ProductCode\": \"MOCK\"}]}}";

        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

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
                        ProductCode = "MOCK",
                        Code = "CODE1"
                    }
                }
                }, Array.Empty<string>()));

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.AddSubscriptionAuthorizationsOnAccountAsync(123, It.Is<IEnumerable<string>>(codes => codes.Contains("MOCK"))), Times.Once);

        // On vérifie qu'on passe bien les ContactIds originaux (pas filtrés), car le filtrage se fait dans le handler
        repositoryMock.Verify(repo => repo.AddSubscriptionAuthorizationsForContacts(
            It.Is<IEnumerable<int>>(ids => ids.Contains(0) && ids.Contains(12) && ids.Contains(-1) && ids.Contains(42)),
            123,
            It.IsAny<IEnumerable<string>>()), Times.Once);
    }
}
