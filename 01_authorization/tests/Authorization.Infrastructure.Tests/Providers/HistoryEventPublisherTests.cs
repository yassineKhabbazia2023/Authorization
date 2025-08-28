// <copyright file="HistoryEventPublisherTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Moq;
using Pulse.Account.Core.Enum;
using Pulse.Authorization.Core.Interfaces;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Enum;
using Pulse.Authorization.Infrastructure.Interfaces;
using Pulse.Authorization.Infrastructure.Providers;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;
using Pulse.Back.Events.IntegrationEvents.EventsData;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Authorization.Infrastructure.Tests.Providers;

public class HistoryEventPublisherTests
{
    private readonly Fixture _fixture;
    private readonly Mock<IContactRepository> _contactRepository;
    private readonly Mock<IAccountRepository> _accountRepository;
    private readonly Mock<IConfigurationRepository> _configurationRepository;
    private readonly Mock<IEventPublisher> _eventPublisher;

    public HistoryEventPublisherTests()
    {
        _configurationRepository = new Mock<IConfigurationRepository>();
        _contactRepository = new Mock<IContactRepository>();
        _accountRepository = new Mock<IAccountRepository>();
        _eventPublisher = new Mock<IEventPublisher>();

        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public async Task PublishHistoryCreatedEvent_ShouldPublishEvent()
    {
        var contact = _fixture.Build<ContactEntity>()
            .With(c => c.Type, ContactType.Collaborator.ToString())
            .Create();

        var targetContact = _fixture.Build<ContactEntity>()
            .With(c => c.Type, ContactType.Collaborator.ToString())
            .Create();

        var account = _fixture.Create<AccountEntity>();
        var addedPermissions = new List<AuthorizationEntity>
        {
            new() { Code = "COUSER001", Label = "Permission 1" }
        };
        var deletedPermissions = new List<AuthorizationEntity>
        {
            new() { Code = "COUSER002", Label = "Permission 2" }
        };

        _contactRepository.Setup(x => x.GetContactByIdAsync(1)).ReturnsAsync(contact);
        _contactRepository.Setup(x => x.GetContactByIdAsync(2)).ReturnsAsync(targetContact);
        _accountRepository.Setup(x => x.GetAccountByIdAsync(1)).ReturnsAsync(account);
        _configurationRepository.Setup(x => x.GetAuthorizationEntitiesByCodeAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(addedPermissions.Concat(deletedPermissions));

        HistoryCreatedEvent? publishedEvent = null;
        _eventPublisher.Setup(x => x.PublishAsync(It.IsAny<HistoryCreatedEvent>(), null, null))
            .Callback<object, object?, object?>((e, _, _) => publishedEvent = (HistoryCreatedEvent)e)
            .Returns(Task.CompletedTask);

        var publisher = new HistoryEventPublisher(_contactRepository.Object, _accountRepository.Object, _configurationRepository.Object, _eventPublisher.Object);

        await publisher.PublishHistoryCreatedEvent(1, 2, 1, new[] { "PERM001" }, new[] { "PERM002" });

        _eventPublisher.Verify(x => x.PublishAsync(It.IsAny<HistoryCreatedEvent>(), null, null), Times.Once);

        Assert.NotNull(publishedEvent);
        Assert.Equal(ActionCode.MAJPERMK.ToString(), publishedEvent.Data.Action.Code);
    }

    [Fact]
    public async Task PublishHistoryCreatedEvent_WithNonCollaborator_ShouldUseCustomerActionCode()
    {
        var contact = _fixture.Build<ContactEntity>()
            .With(c => c.Type, "Customer")
            .Create();

        var targetContact = _fixture.Build<ContactEntity>()
            .With(c => c.Type, "Customer")
            .Create();

        var account = _fixture.Create<AccountEntity>();
        var addedPermissions = new List<AuthorizationEntity>
        {
            new() { Code = "COUSER001", Label = "Permission 1" }
        };
        var deletedPermissions = new List<AuthorizationEntity>
        {
            new() { Code = "COUSER002", Label = "Permission 2" }
        };

        _contactRepository.Setup(x => x.GetContactByIdAsync(1)).ReturnsAsync(contact);
        _contactRepository.Setup(x => x.GetContactByIdAsync(2)).ReturnsAsync(targetContact);
        _accountRepository.Setup(x => x.GetAccountByIdAsync(1)).ReturnsAsync(account);
        _configurationRepository.Setup(x => x.GetAuthorizationEntitiesByCodeAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(addedPermissions.Concat(deletedPermissions));

        HistoryCreatedEvent? publishedEvent = null;
        _eventPublisher.Setup(x => x.PublishAsync(It.IsAny<HistoryCreatedEvent>(), null, null))
            .Callback<object, object?, object?>((e, _, _) => publishedEvent = (HistoryCreatedEvent)e)
            .Returns(Task.CompletedTask);

        var publisher = new HistoryEventPublisher(_contactRepository.Object, _accountRepository.Object, _configurationRepository.Object, _eventPublisher.Object);

        await publisher.PublishHistoryCreatedEvent(1, 2, 1, new[] { "PERM001" }, new[] { "PERM002" });

        _eventPublisher.Verify(x => x.PublishAsync(It.IsAny<HistoryCreatedEvent>(), null, null), Times.Once);

        Assert.NotNull(publishedEvent);
        Assert.Equal(ActionCode.MAJPERMC.ToString(), publishedEvent.Data.Action.Code);
    }

    [Fact]
    public async Task PublishHistoryCreatedEvent_WithNoCurrentUserFound_ShouldThrowNotFoundException()
    {
        var addedPermissionCodes = new[] { "COUSER001" };
        var deletedPermissionCodes = new[] { "CORAPP01" };

        _contactRepository.Setup(x => x.GetContactByIdAsync(1)).ReturnsAsync((ContactEntity?)null);

        _configurationRepository.Setup(x => x.GetAuthorizationEntitiesByCodeAsync(addedPermissionCodes))
            .ReturnsAsync(new[] { new AuthorizationEntity { Code = "COUSER001", Label = "Added" } });
        _configurationRepository.Setup(x => x.GetAuthorizationEntitiesByCodeAsync(deletedPermissionCodes))
            .ReturnsAsync(new[] { new AuthorizationEntity { Code = "CORAPP01", Label = "Deleted" } });

        var publisher = new HistoryEventPublisher(
            _contactRepository.Object,
            _accountRepository.Object,
            _configurationRepository.Object,
            _eventPublisher.Object);

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            publisher.PublishHistoryCreatedEvent(1, 2, 1, addedPermissionCodes, deletedPermissionCodes));

        Assert.Equal("AUT002", ex.Code);
        Assert.Equal("Le contact avec l'identifiant 1 est introuvable", ex.Message);

        _contactRepository.Verify(x => x.GetContactByIdAsync(It.IsAny<int>()), Times.Once);
        _accountRepository.Verify(x => x.GetAccountByIdAsync(It.IsAny<int>()), Times.Never);
        _configurationRepository.Verify(x => x.GetAuthorizationEntitiesByCodeAsync(It.IsAny<IEnumerable<string>>()), Times.Exactly(2));
        _eventPublisher.Verify(x => x.PublishAsync(It.IsAny<BaseEvent<HistoryCreatedEventData>>(), null!, null), Times.Never);
    }

    [Fact]
    public async Task PublishHistoryCreatedEvent_WithNoTargetUserFound_ShouldThrowNotFoundException()
    {
        var addedPermissionCodes = new[] { "COUSER001" };
        var deletedPermissionCodes = new[] { "CORAPP01" };

        _contactRepository.Setup(x => x.GetContactByIdAsync(1)).ReturnsAsync(_fixture.Create<ContactEntity>());
        _contactRepository.Setup(x => x.GetContactByIdAsync(2)).ReturnsAsync((ContactEntity?)null);

        _configurationRepository.Setup(x => x.GetAuthorizationEntitiesByCodeAsync(addedPermissionCodes))
            .ReturnsAsync(new[] { new AuthorizationEntity { Code = "COUSER001", Label = "Added" } });
        _configurationRepository.Setup(x => x.GetAuthorizationEntitiesByCodeAsync(deletedPermissionCodes))
            .ReturnsAsync(new[] { new AuthorizationEntity { Code = "CORAPP01", Label = "Deleted" } });

        var publisher = new HistoryEventPublisher(
            _contactRepository.Object,
            _accountRepository.Object,
            _configurationRepository.Object,
            _eventPublisher.Object);

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            publisher.PublishHistoryCreatedEvent(1, 2, 1, addedPermissionCodes, deletedPermissionCodes));

        Assert.Equal("AUT002", ex.Code);
        Assert.Equal("Le contact avec l'identifiant 2 est introuvable", ex.Message);

        _contactRepository.Verify(x => x.GetContactByIdAsync(It.IsAny<int>()), Times.Exactly(2));
        _accountRepository.Verify(x => x.GetAccountByIdAsync(It.IsAny<int>()), Times.Never);
        _configurationRepository.Verify(x => x.GetAuthorizationEntitiesByCodeAsync(It.IsAny<IEnumerable<string>>()), Times.Exactly(2));
        _eventPublisher.Verify(x => x.PublishAsync(It.IsAny<BaseEvent<HistoryCreatedEventData>>(), null!, null), Times.Never);
    }

    [Fact]
    public async Task PublishHistoryCreatedEvent_WithNoAccountFound_ShouldThrowNotFoundException()
    {
        var addedPermissionCodes = new[] { "CORAPP01" };
        var deletedPermissionCodes = new[] { "COUSER001" };

        _contactRepository.Setup(x => x.GetContactByIdAsync(It.IsAny<int>())).ReturnsAsync(_fixture.Create<ContactEntity>());
        _accountRepository.Setup(x => x.GetAccountByIdAsync(It.IsAny<int>())).ReturnsAsync((AccountEntity?)null);

        _configurationRepository.Setup(x => x.GetAuthorizationEntitiesByCodeAsync(addedPermissionCodes))
            .ReturnsAsync(new[] { new AuthorizationEntity { Code = "CORAPP01", Label = "Added" } });
        _configurationRepository.Setup(x => x.GetAuthorizationEntitiesByCodeAsync(deletedPermissionCodes))
            .ReturnsAsync(new[] { new AuthorizationEntity { Code = "COUSER001", Label = "Deleted" } });

        var publisher = new HistoryEventPublisher(_contactRepository.Object, _accountRepository.Object, _configurationRepository.Object, _eventPublisher.Object);

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            publisher.PublishHistoryCreatedEvent(1, 2, 1, addedPermissionCodes, deletedPermissionCodes));

        Assert.Equal("AUT001", ex.Code);
        Assert.Equal("L'identifiant de l'entité saisi 1 est introuvable", ex.Message);

        _contactRepository.Verify(x => x.GetContactByIdAsync(It.IsAny<int>()), Times.Exactly(2));
        _accountRepository.Verify(x => x.GetAccountByIdAsync(It.IsAny<int>()), Times.Once);
        _configurationRepository.Verify(x => x.GetAuthorizationEntitiesByCodeAsync(It.IsAny<IEnumerable<string>>()), Times.Exactly(2));
        _eventPublisher.Verify(x => x.PublishAsync(It.IsAny<BaseEvent<HistoryCreatedEventData>>(), null!, null), Times.Never);
    }

    [Fact]
    public async Task PublishHistoryCreatedEvent_WithNoPermissionFound_ShouldReturnWithoutCallingOtherDependencies()
    {
        // Arrange
        _configurationRepository
            .Setup(x => x.GetAuthorizationEntitiesByCodeAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(Enumerable.Empty<AuthorizationEntity>());

        var publisher = new HistoryEventPublisher(
            _contactRepository.Object,
            _accountRepository.Object,
            _configurationRepository.Object,
            _eventPublisher.Object);

        // Act
        await publisher.PublishHistoryCreatedEvent(1, 2, 1, null!, null!);

        // Assert
        _configurationRepository.Verify(x => x.GetAuthorizationEntitiesByCodeAsync(It.IsAny<IEnumerable<string>>()), Times.Exactly(2));
        _contactRepository.Verify(x => x.GetContactByIdAsync(It.IsAny<int>()), Times.Never);
        _accountRepository.Verify(x => x.GetAccountByIdAsync(It.IsAny<int>()), Times.Never);
        _eventPublisher.Verify(x => x.PublishAsync(It.IsAny<BaseEvent<HistoryCreatedEventData>>(), null!, null), Times.Never);
    }
}
