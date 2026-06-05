// <copyright file="RoleCreatedEventHandlerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Moq;
using Pulse.Authorization.Infrastructure.Providers.Interfaces;
using Pulse.Authorization.Infrastructure.Providers;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Interfaces;
using Pulse.Authorization.Infrastructure.Constants;
using Pulse.Authorization.Core.Interfaces;

namespace Pulse.Authorization.Infrastructure.Tests.Providers;

public class RoleCreatedEventHandlerTests
{
    private readonly Mock<IAuthorizationEventPublisher> _authorizationEventPublisherMock = new(MockBehavior.Strict);
    private readonly Mock<IAuthorizationEventRepository> _authorizationEventRepository = new(MockBehavior.Strict);
    private readonly Mock<IAuthorizationRepository> _authorizationRepository = new(MockBehavior.Strict);
    private readonly Mock<IOnboardingEventPublisher> _onboardingEventPublisher = new(MockBehavior.Strict);
    private readonly Mock<IContactEventRepository> _contactEventRepository = new();
    private readonly Mock<IAccountRepository> _accountRepository = new();

    public RoleCreatedEventHandlerTests()
    {
        _authorizationRepository.Setup(a => a.CreateDefaultAuthorizationsOnSignatoryAsync(It.IsAny<int>(), It.IsAny<int>()))
        .ReturnsAsync(GlobalConstants.DefaultSignatoryPermissions)
            .Verifiable();

        _authorizationRepository.Setup(a => a.CreateDefaultAuthorizationsOnNonSignatoryAsync(It.IsAny<int>(), It.IsAny<int>()))
        .ReturnsAsync(GlobalConstants.DefaultNonSignatoryPermissions)
            .Verifiable();

        _authorizationEventPublisherMock.Setup(a => a.PublishAuthorizationUpdatedEventAsync(It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>(), false))
            .Returns(Task.CompletedTask)
            .Verifiable();

        _authorizationEventRepository.Setup(a => a.IsPennylaneActivatedAsync(It.IsAny<int>()))
            .ReturnsAsync(true);

        _onboardingEventPublisher.Setup(o => o.PublishOnBoardingEventAsync(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        _contactEventRepository.Setup(c => c.IsContactClientAsync(It.IsAny<int>()))
            .ReturnsAsync(true);

        var account = new AccountEntity
        {
            AccountId = 22,
            AccountNumber = "ACC20240001",
            AccountGlobalUniqueId = Guid.NewGuid(),
            LegalName = "Test Account",
            IsActive = true,
        };
        _accountRepository.Setup(c => c.GetAccountByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(account)
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

        var handler = new RoleCreatedEventHandler(loggerMock.Object,
            repositoryMock.Object,
            _authorizationEventPublisherMock.Object,
            _authorizationRepository.Object,
            _authorizationEventRepository.Object,
            _onboardingEventPublisher.Object,
            _contactEventRepository.Object,
            _accountRepository.Object);
        var message = "{\"EventType\":\"RoleCreatedEvent\",\"Data\":{\"ContactId\":123,\"AccountId\":22,\"IsSignatory\":1,\"IsFavorite\":1,\"IsDelegation\":1,}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.CreateRoleAsync(It.IsAny<RoleEntity>()), Times.Once);
        _authorizationEventPublisherMock.Verify(x => x.PublishAuthorizationUpdatedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>(), It.IsAny<bool>()), Times.Once);
        _authorizationRepository.Verify(x => x.CreateDefaultAuthorizationsOnSignatoryAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        _onboardingEventPublisher.Verify(x => x.PublishOnBoardingEventAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
        _accountRepository.Verify(x => x.GetAccountByIdAsync(It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNonSignatoryCustomer_ShouldCreateDefaultNonSignatoryAuthorizations()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RoleCreatedEventHandler>>();
        var repositoryMock = new Mock<IRoleEventRepository>();

        var handler = new RoleCreatedEventHandler(loggerMock.Object,
            repositoryMock.Object,
            _authorizationEventPublisherMock.Object,
            _authorizationRepository.Object,
            _authorizationEventRepository.Object,
            _onboardingEventPublisher.Object,
            _contactEventRepository.Object,
            _accountRepository.Object);
        var message = "{\"EventType\":\"RoleCreatedEvent\",\"Data\":{\"ContactId\":123,\"AccountId\":22,\"IsSignatory\":0,\"IsFavorite\":1,\"IsDelegation\":1,}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.CreateRoleAsync(It.IsAny<RoleEntity>()), Times.Once);
        _authorizationRepository.Verify(x => x.CreateDefaultAuthorizationsOnNonSignatoryAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        _authorizationRepository.Verify(x => x.CreateDefaultAuthorizationsOnSignatoryAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        _authorizationEventPublisherMock.Verify(x => x.PublishAuthorizationUpdatedEventAsync(It.IsAny<int>(), It.IsAny<int>(), GlobalConstants.DefaultNonSignatoryPermissions, It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNonSignatoryNonCustomer_ShouldNotCreateDefaultAuthorizations()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RoleCreatedEventHandler>>();
        var repositoryMock = new Mock<IRoleEventRepository>();

        _contactEventRepository.Setup(c => c.IsContactClientAsync(It.IsAny<int>()))
            .ReturnsAsync(false);

        var handler = new RoleCreatedEventHandler(loggerMock.Object,
            repositoryMock.Object,
            _authorizationEventPublisherMock.Object,
            _authorizationRepository.Object,
            _authorizationEventRepository.Object,
            _onboardingEventPublisher.Object,
            _contactEventRepository.Object,
            _accountRepository.Object);
        var message = "{\"EventType\":\"RoleCreatedEvent\",\"Data\":{\"ContactId\":123,\"AccountId\":22,\"IsSignatory\":0,\"IsFavorite\":1,\"IsDelegation\":1,}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.CreateRoleAsync(It.IsAny<RoleEntity>()), Times.Once);
        _authorizationRepository.Verify(x => x.CreateDefaultAuthorizationsOnNonSignatoryAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        _authorizationRepository.Verify(x => x.CreateDefaultAuthorizationsOnSignatoryAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        _authorizationEventPublisherMock.Verify(x => x.PublishAuthorizationUpdatedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>(), It.IsAny<bool>()), Times.Never);
        _onboardingEventPublisher.Verify(x => x.PublishOnBoardingEventAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithNullMessage_ShouldNotCreateRole()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RoleCreatedEventHandler>>();
        var repositoryMock = new Mock<IRoleEventRepository>();
        var handler = new RoleCreatedEventHandler(loggerMock.Object,
            repositoryMock.Object,
            _authorizationEventPublisherMock.Object,
            _authorizationRepository.Object,
            _authorizationEventRepository.Object,
            _onboardingEventPublisher.Object,
            _contactEventRepository.Object,
            _accountRepository.Object);

        // Act
        await handler.HandleAsync(null!);

        // Assert
        repositoryMock.Verify(repo => repo.CreateRoleAsync(It.IsAny<RoleEntity>()), Times.Never);
        _authorizationEventPublisherMock.Verify(x => x.PublishAuthorizationUpdatedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>(), It.IsAny<bool>()), Times.Never);
        _authorizationRepository.Verify(x => x.CreateDefaultAuthorizationsOnSignatoryAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        _onboardingEventPublisher.Verify(x => x.PublishOnBoardingEventAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        _accountRepository.Verify(x => x.GetAccountByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidAccountId_ShouldNotCreateRole()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RoleCreatedEventHandler>>();
        var repositoryMock = new Mock<IRoleEventRepository>();
        var handler = new RoleCreatedEventHandler(loggerMock.Object,
            repositoryMock.Object,
            _authorizationEventPublisherMock.Object,
            _authorizationRepository.Object,
            _authorizationEventRepository.Object,
            _onboardingEventPublisher.Object,
            _contactEventRepository.Object,
            _accountRepository.Object);
        var message = "{\"EventType\":\"RoleCreatedEvent\",\"Data\":{\"ContactId\":123,\"AccountId\":-2,\"IsSignatory\":1,\"IsFavorite\":1,\"IsDelegation\":1,}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.CreateRoleAsync(It.IsAny<RoleEntity>()), Times.Never);
        _authorizationEventPublisherMock.Verify(x => x.PublishAuthorizationUpdatedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>(), It.IsAny<bool>()), Times.Never);
        _authorizationRepository.Verify(x => x.CreateDefaultAuthorizationsOnSignatoryAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        _onboardingEventPublisher.Verify(x => x.PublishOnBoardingEventAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        _accountRepository.Verify(x => x.GetAccountByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidContactId_ShouldNotCreateRole()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RoleCreatedEventHandler>>();
        var repositoryMock = new Mock<IRoleEventRepository>();
        var handler = new RoleCreatedEventHandler(loggerMock.Object,
            repositoryMock.Object,
            _authorizationEventPublisherMock.Object,
            _authorizationRepository.Object,
            _authorizationEventRepository.Object,
            _onboardingEventPublisher.Object,
            _contactEventRepository.Object,
            _accountRepository.Object);
        var message = "{\"EventType\":\"RoleCreatedEvent\",\"Data\":{\"ContactId\":0,\"AccountId\":4,\"IsSignatory\":1,\"IsFavorite\":1,\"IsDelegation\":1,}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.CreateRoleAsync(It.IsAny<RoleEntity>()), Times.Never);
        _authorizationEventPublisherMock.Verify(x => x.PublishAuthorizationUpdatedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>(), It.IsAny<bool>()), Times.Never);
        _authorizationRepository.Verify(x => x.CreateDefaultAuthorizationsOnSignatoryAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        _onboardingEventPublisher.Verify(x => x.PublishOnBoardingEventAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        _accountRepository.Verify(x => x.GetAccountByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMessageMissingData_ShouldNotCreateRole()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RoleCreatedEventHandler>>();
        var repositoryMock = new Mock<IRoleEventRepository>();
        var handler = new RoleCreatedEventHandler(loggerMock.Object,
            repositoryMock.Object,
            _authorizationEventPublisherMock.Object,
            _authorizationRepository.Object,
            _authorizationEventRepository.Object,
            _onboardingEventPublisher.Object,
            _contactEventRepository.Object,
            _accountRepository.Object);
        var message = "{\"EventType\":\"RoleCreatedEvent\"}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.CreateRoleAsync(It.IsAny<RoleEntity>()), Times.Never);
        _authorizationEventPublisherMock.Verify(x => x.PublishAuthorizationUpdatedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>(), It.IsAny<bool>()), Times.Never);
        _authorizationRepository.Verify(x => x.CreateDefaultAuthorizationsOnSignatoryAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        _onboardingEventPublisher.Verify(x => x.PublishOnBoardingEventAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        _accountRepository.Verify(x => x.GetAccountByIdAsync(It.IsAny<int>()), Times.Never);
    }
}
