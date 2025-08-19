// <copyright file="HistoryEventPublisherTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Moq;
using Pulse.Authorization.Core.Interfaces;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Interfaces;
using Pulse.Authorization.Infrastructure.Providers;
using Pulse.Back.Events.Abstractions;
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
        _contactRepository.Setup(x => x.GetContactByIdAsync(It.IsAny<int>())).ReturnsAsync(_fixture.Create<ContactEntity>());
        _accountRepository.Setup(x => x.GetAccountByIdAsync(It.IsAny<int>())).ReturnsAsync(_fixture.Create<AccountEntity>());
        _configurationRepository.Setup(x => x.GetAuthorizationEntitiesByCodeAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(_fixture.CreateMany<AuthorizationEntity>());

        var publisher = new HistoryEventPublisher(_contactRepository.Object, _accountRepository.Object, _configurationRepository.Object, _eventPublisher.Object);

        await publisher.PublishHistoryCreatedEvent(1, 2, 1, new List<string> { "toto" }, new List<string> { "tata" });

        _contactRepository.Verify(x => x.GetContactByIdAsync(It.IsAny<int>()), Times.Exactly(2));
        _accountRepository.Verify(x => x.GetAccountByIdAsync(It.IsAny<int>()), Times.Once);
        _configurationRepository.Verify(x => x.GetAuthorizationEntitiesByCodeAsync(It.IsAny<IEnumerable<string>>()), Times.Exactly(2));
        _eventPublisher.Verify(x => x.PublishAsync(It.IsAny<BaseEvent<HistoryCreatedEventData>>(), null!, null), Times.Once);
    }

    [Fact]
    public async Task PublishHistoryCreatedEvent_WithNoCurrentUserFound_ShouldThrowNotFoundException()
    {
        ContactEntity invalidContact = null!;
        _contactRepository.Setup(x => x.GetContactByIdAsync(1)).ReturnsAsync(invalidContact);

        var publisher = new HistoryEventPublisher(_contactRepository.Object, _accountRepository.Object, _configurationRepository.Object, _eventPublisher.Object);

        var result = await Assert.ThrowsAsync<NotFoundException>(async () => await publisher.PublishHistoryCreatedEvent(1, 2, 1, null!, null!));

        Assert.Equal("AUT002", result.Code);
        Assert.Equal("Le contact avec l'identifiant 1 est introuvable", result.Message);

        _contactRepository.Verify(x => x.GetContactByIdAsync(It.IsAny<int>()), Times.Once);
        _accountRepository.Verify(x => x.GetAccountByIdAsync(It.IsAny<int>()), Times.Never);
        _configurationRepository.Verify(x => x.GetAuthorizationEntitiesByCodeAsync(It.IsAny<IEnumerable<string>>()), Times.Never);
        _eventPublisher.Verify(x => x.PublishAsync(It.IsAny<BaseEvent<HistoryCreatedEventData>>(), null!, null), Times.Never);
    }

    [Fact]
    public async Task PublishHistoryCreatedEvent_WithNoTargetUserFound_ShouldThrowNotFoundException()
    {
        ContactEntity invalidContact = null!;
        _contactRepository.Setup(x => x.GetContactByIdAsync(1)).ReturnsAsync(_fixture.Create<ContactEntity>());
        _contactRepository.Setup(x => x.GetContactByIdAsync(2)).ReturnsAsync(invalidContact);

        var publisher = new HistoryEventPublisher(_contactRepository.Object, _accountRepository.Object, _configurationRepository.Object, _eventPublisher.Object);

        var result = await Assert.ThrowsAsync<NotFoundException>(async () => await publisher.PublishHistoryCreatedEvent(1, 2, 1, null!, null!));

        Assert.Equal("AUT002", result.Code);
        Assert.Equal("Le contact avec l'identifiant 2 est introuvable", result.Message);

        _contactRepository.Verify(x => x.GetContactByIdAsync(It.IsAny<int>()), Times.Exactly(2));
        _accountRepository.Verify(x => x.GetAccountByIdAsync(It.IsAny<int>()), Times.Never);
        _configurationRepository.Verify(x => x.GetAuthorizationEntitiesByCodeAsync(It.IsAny<IEnumerable<string>>()), Times.Never);
        _eventPublisher.Verify(x => x.PublishAsync(It.IsAny<BaseEvent<HistoryCreatedEventData>>(), null!, null), Times.Never);
    }

    [Fact]
    public async Task PublishHistoryCreatedEvent_WithNoAccountFound_ShouldThrowNotFoundException()
    {
        AccountEntity invalidAccount = null!;
        _contactRepository.Setup(x => x.GetContactByIdAsync(It.IsAny<int>())).ReturnsAsync(_fixture.Create<ContactEntity>());
        _accountRepository.Setup(x => x.GetAccountByIdAsync(It.IsAny<int>())).ReturnsAsync(invalidAccount);

        var publisher = new HistoryEventPublisher(_contactRepository.Object, _accountRepository.Object, _configurationRepository.Object, _eventPublisher.Object);

        var result = await Assert.ThrowsAsync<NotFoundException>(async () => await publisher.PublishHistoryCreatedEvent(1, 2, 1, null!, null!));

        Assert.Equal("AUT001", result.Code);
        Assert.Equal("L'identifiant de l'entité saisi 1 est introuvable", result.Message);

        _contactRepository.Verify(x => x.GetContactByIdAsync(It.IsAny<int>()), Times.Exactly(2));
        _accountRepository.Verify(x => x.GetAccountByIdAsync(It.IsAny<int>()), Times.Once);
        _configurationRepository.Verify(x => x.GetAuthorizationEntitiesByCodeAsync(It.IsAny<IEnumerable<string>>()), Times.Never);
        _eventPublisher.Verify(x => x.PublishAsync(It.IsAny<BaseEvent<HistoryCreatedEventData>>(), null!, null), Times.Never);
    }

    [Fact]
    public async Task PublishHistoryCreatedEvent_WithNoPermissionFound_ShouldThrowNotFoundException()
    {
        _contactRepository.Setup(x => x.GetContactByIdAsync(It.IsAny<int>())).ReturnsAsync(_fixture.Create<ContactEntity>());
        _accountRepository.Setup(x => x.GetAccountByIdAsync(It.IsAny<int>())).ReturnsAsync(_fixture.Create<AccountEntity>());
        _configurationRepository.Setup(x => x.GetAuthorizationEntitiesByCodeAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(Enumerable.Empty<AuthorizationEntity>());

        var publisher = new HistoryEventPublisher(_contactRepository.Object, _accountRepository.Object, _configurationRepository.Object, _eventPublisher.Object);

        var result = await Assert.ThrowsAsync<NotFoundException>(async () => await publisher.PublishHistoryCreatedEvent(1, 2, 1, null!, null!));

        Assert.Equal("AUT014", result.Code);
        Assert.Equal("Aucune permission n'a été trouvée.", result.Message);

        _contactRepository.Verify(x => x.GetContactByIdAsync(It.IsAny<int>()), Times.Exactly(2));
        _accountRepository.Verify(x => x.GetAccountByIdAsync(It.IsAny<int>()), Times.Once);
        _configurationRepository.Verify(x => x.GetAuthorizationEntitiesByCodeAsync(It.IsAny<IEnumerable<string>>()), Times.Exactly(2));
        _eventPublisher.Verify(x => x.PublishAsync(It.IsAny<BaseEvent<HistoryCreatedEventData>>(), null!, null), Times.Never);
    }
}
