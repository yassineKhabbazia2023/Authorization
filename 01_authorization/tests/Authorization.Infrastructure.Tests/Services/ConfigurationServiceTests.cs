// <copyright file="ConfigurationServiceTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using FluentAssertions;
using Moq;
using Pulse.Authorization.Core.Interfaces;
using Pulse.Authorization.Core.Exceptions;
using Pulse.Authorization.Infrastructure.Interfaces;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Constants;
using Pulse.Authorization.Infrastructure.Enum;
using Pulse.Authorization.Infrastructure.Providers.Interfaces;
using Pulse.ExceptionMiddleware.Exceptions;
using Pulse.Authorization.Infrastructure.Services;

namespace Pulse.Authorization.Core.Tests.Services;

public class ConfigurationServiceTests
{
    private readonly Mock<IConfigurationRepository> _configurationRepository;
    private readonly Mock<IContactRepository> _contactRepository;
    private readonly Mock<IAccountRepository> _accountRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IHistoryEventPublisher> _historyEventPublisherMock;
    private readonly Fixture _fixture;

    public ConfigurationServiceTests()
    {
        _configurationRepository = new Mock<IConfigurationRepository>();
        _contactRepository = new Mock<IContactRepository>();
        _historyEventPublisherMock = new Mock<IHistoryEventPublisher>();
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _fixture.Customize<AccountEntity>(c => c.With(a => a.AccountType, (string?)null));
    }

    [Fact]
    public async Task GetContactAccountConfigurationAsync_WhenContactNotFound_ThrowsNotFoundException()
    {
        // Arrange
        int contactId = 1;
        int accountId = 1;
        _contactRepository.Setup(x => x.GetContactByIdAsync(contactId)).ReturnsAsync((ContactEntity)null!);
        var authorizationEventPublisherMock = new Mock<IAuthorizationEventPublisher>(MockBehavior.Strict);
        authorizationEventPublisherMock.Setup(a => a.PublishAuthorizationUpdatedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>(), false)).Verifiable();
        var configurationService = new ConfigurationService(_configurationRepository.Object, _contactRepository.Object, authorizationEventPublisherMock.Object, _accountRepositoryMock.Object, _historyEventPublisherMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(async () =>
        {
            await configurationService.GetContactAccountConfigurationAsync(contactId, accountId);
        });

        exception.Message.Should().Be("Le contact avec l'identifiant 1 est introuvable");
    }

    [Fact]
    public async Task GetContactAccountConfigurationAsync_WhenInvalidContactType_ThrowsNotFoundException()
    {
        // Arrange
        var contactId = 999;
        var accountId = 123;

        var contactMocked = _fixture.Build<ContactEntity>()
                            .With(c => c.ContactId, contactId)
                            .With(c => c.Type, "NewContactType")
                            .Create();

        _contactRepository.Setup(x => x.GetContactByIdAsync(contactId)).ReturnsAsync(contactMocked);
        var authorizationEventPublisherMock = new Mock<IAuthorizationEventPublisher>(MockBehavior.Strict);
        authorizationEventPublisherMock.Setup(a => a.PublishAuthorizationUpdatedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>(), false)).Verifiable();
        var configurationService = new ConfigurationService(_configurationRepository.Object, _contactRepository.Object, authorizationEventPublisherMock.Object, _accountRepositoryMock.Object, _historyEventPublisherMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(async () =>
        {
            await configurationService.GetContactAccountConfigurationAsync(contactId, accountId);
        });

        exception.Message.Should().Be("Le contact avec l'identifiant 999 et de type NewContactType n'est pas reconnu");
    }

    [Fact]
    public async Task GetContactAccountConfigurationAsync_WhenContactTypeIsCustomer_ShouldRetunConfigurationsWithEnablesCustomerActions()
    {
        // Arrange
        var accountId = 123;
        var contactId = 456;

        var contactMocked = _fixture.Build<ContactEntity>()
                                    .With(c => c.ContactId, contactId)
                                    .With(c => c.Type, ContactType.Customer.ToString())
                                    .Create();

        var accountAuthorizations = new List<AuthorizationEntity>
        {
            new() { Category = "CltGESTION", AuthorizationId = 1, Code = "CLADMI001", Name = "Super Admin", },
            new() { Category = "CltGESTION", AuthorizationId = 2, Code = "CLUSER001", Name = "View User", },
            new() { Category = "CltGESTION", AuthorizationId = 3, Code = "CLUSER002", Name = "Add User", },
            new() { Category = "CltGESTION", AuthorizationId = 4, Code = "CLUSER002", Name = "Add User", },
            new() { Category = "CltGESTION", AuthorizationId = 5, Code = "CLUSER003", Name = "Delete User", },
            new() { Category = "CltGESTION", AuthorizationId = 6, Code = "CLOFF001", Name = "View offers", },
            new() { Category = "CltGESTION", AuthorizationId = 7, Code = "CLINFO001", Name = "View informations", },
        };

        var contactAuthorizations = new List<AuthorizationEntity>
        {
            new () { Category = "CltGESTION", AuthorizationId = 6, Code = "CLOFF001", Name = "View offers" },
            new () { Category = "CltGESTION", AuthorizationId = 2, Code = "CLUSER001", Name = "View User" },
        };

        _contactRepository.Setup(repository => repository.GetContactByIdAsync(contactId))
            .ReturnsAsync(contactMocked);

        _configurationRepository.Setup(repository => repository.GetAccountAuthorizationsAsync(accountId, GlobalConstants.CustomerCategory, true))
            .ReturnsAsync(accountAuthorizations);

        _configurationRepository.Setup(repository => repository.GetContactAuthorizationsAsync(contactId, accountId))
            .ReturnsAsync(contactAuthorizations);

        var authorizationEventPublisherMock = new Mock<IAuthorizationEventPublisher>(MockBehavior.Strict);
        authorizationEventPublisherMock.Setup(a => a.PublishAuthorizationUpdatedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>(), false)).Verifiable();
        var configurationService = new ConfigurationService(_configurationRepository.Object, _contactRepository.Object, authorizationEventPublisherMock.Object, _accountRepositoryMock.Object, _historyEventPublisherMock.Object);

        // Act
        var eligibleContactAuthorizations = await configurationService.GetContactAccountConfigurationAsync(contactId, accountId);

        // Assert
        var permissions = eligibleContactAuthorizations.FirstOrDefault();
        permissions!.Category.Should().Be("CltGESTION");
        permissions.Actions.Count().Should().Be(7);
        foreach (var action in permissions.Actions)
        {
            if (action.ActionId == 2 || action.ActionId == 6)
            {
                action.Enabled.Should().BeTrue();
            }
            else
            {
                action.Enabled.Should().BeFalse();
            }
        }
    }

    [Fact]
    public async Task GetContactAccountConfigurationCollabAsync_WhenContactTypeCustomer_ShouldRetunEmpty()
    {
        // Arrange
        var accountId = 123;
        var contactId = 456;

        var contactMocked = _fixture.Build<ContactEntity>()
                                    .With(c => c.ContactId, contactId)
                                    .With(c => c.Type, ContactType.Collaborator.ToString())
                                    .Create();

        var accountAuthorizations = new List<AuthorizationEntity>
        {
            new() { Category = "CltGESTION", AuthorizationId = 1, Code = "CLADMI001", Name = "Super Admin", },
            new() { Category = "CltGESTION", AuthorizationId = 2, Code = "CLUSER001", Name = "View User", },
            new() { Category = "CltGESTION", AuthorizationId = 3, Code = "CLUSER002", Name = "Add User", },
            new() { Category = "CltGESTION", AuthorizationId = 4, Code = "CLUSER002", Name = "Add User", },
            new() { Category = "CltGESTION", AuthorizationId = 5, Code = "CLUSER003", Name = "Delete User", },
            new() { Category = "CltGESTION", AuthorizationId = 6, Code = "CLOFF001", Name = "View offers", },
            new() { Category = "CltGESTION", AuthorizationId = 7, Code = "CLINFO001", Name = "View informations", },
        };

        var contactAuthorizations = new List<AuthorizationEntity>
        {
            new () { Category = "CltGESTION", AuthorizationId = 6, Code = "CLOFF001", Name = "View offers" },
            new () { Category = "CltGESTION", AuthorizationId = 2, Code = "CLUSER001", Name = "View User" },
        };

        _contactRepository.Setup(repository => repository.GetContactByIdAsync(contactId))
            .ReturnsAsync(contactMocked);

        _configurationRepository.Setup(repository => repository.GetAccountAuthorizationsAsync(accountId, GlobalConstants.CustomerCategory, true))
            .ReturnsAsync(accountAuthorizations);

        _configurationRepository.Setup(repository => repository.GetContactAuthorizationsAsync(contactId, accountId))
            .ReturnsAsync(contactAuthorizations);

        var authorizationEventPublisherMock = new Mock<IAuthorizationEventPublisher>(MockBehavior.Strict);
        authorizationEventPublisherMock.Setup(a => a.PublishAuthorizationUpdatedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>(), false)).Verifiable();
        var configurationService = new ConfigurationService(_configurationRepository.Object, _contactRepository.Object, authorizationEventPublisherMock.Object, _accountRepositoryMock.Object, _historyEventPublisherMock.Object);

        // Act
        var eligibleContactAuthorizations = await configurationService.GetContactAccountConfigurationAsync(contactId, accountId);

        // Assert
        eligibleContactAuthorizations.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task GetContactAccountConfigurationCustomerAsync_WhenContactTypeCollab_ShouldRetunEmpty()
    {
        // Arrange
        var accountId = 123;
        var contactId = 456;

        var contactMocked = _fixture.Build<ContactEntity>()
                                    .With(c => c.ContactId, contactId)
                                    .With(c => c.Type, ContactType.Customer.ToString())
                                    .Create();

        var accountAuthorizations = new List<AuthorizationEntity>
        {
            new() { Category = "ColGESTION", AuthorizationId = 1, Code = "CLADMI001", Name = "Super Admin", },
            new() { Category = "ColGESTION", AuthorizationId = 2, Code = "CLUSER001", Name = "View User", },
            new() { Category = "ColGESTION", AuthorizationId = 3, Code = "CLUSER002", Name = "Add User", },
            new() { Category = "ColGESTION", AuthorizationId = 4, Code = "CLUSER002", Name = "Add User", },
            new() { Category = "ColGESTION", AuthorizationId = 5, Code = "CLUSER003", Name = "Delete User", },
            new() { Category = "ColGESTION", AuthorizationId = 6, Code = "CLOFF001", Name = "View offers", },
            new() { Category = "ColGESTION", AuthorizationId = 7, Code = "CLINFO001", Name = "View informations", },
        };

        var contactAuthorizations = new List<AuthorizationEntity>
        {
            new () { Category = "ColGESTION", AuthorizationId = 6, Code = "CLOFF001", Name = "View offers" },
            new () { Category = "ColGESTION", AuthorizationId = 2, Code = "CLUSER001", Name = "View User" },
        };

        _contactRepository.Setup(repository => repository.GetContactByIdAsync(contactId))
            .ReturnsAsync(contactMocked);

        _configurationRepository.Setup(repository => repository.GetAccountAuthorizationsAsync(accountId, GlobalConstants.CollabCategory, true))
            .ReturnsAsync(accountAuthorizations);

        _configurationRepository.Setup(repository => repository.GetContactAuthorizationsAsync(contactId, accountId))
            .ReturnsAsync(contactAuthorizations);

        var authorizationEventPublisherMock = new Mock<IAuthorizationEventPublisher>(MockBehavior.Strict);
        authorizationEventPublisherMock.Setup(a => a.PublishAuthorizationUpdatedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>(), false)).Verifiable();
        var configurationService = new ConfigurationService(_configurationRepository.Object, _contactRepository.Object, authorizationEventPublisherMock.Object, _accountRepositoryMock.Object, _historyEventPublisherMock.Object);

        // Act
        var eligibleContactAuthorizations = await configurationService.GetContactAccountConfigurationAsync(contactId, accountId);

        // Assert
        eligibleContactAuthorizations.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task GetContactAccountConfigurationAsync_WhenContactTypeIsCollaborator_ShouldRetunConfigurationsWithEnablesCollaboratorActions()
    {
        // Arrange
        var accountId = GlobalConstants.DefaultAccountIdCollab;
        var contactId = 789;

        var contactMocked = _fixture.Build<ContactEntity>()
                                    .With(c => c.ContactId, contactId)
                                    .With(c => c.Type, ContactType.Collaborator.ToString())
                                    .Create();

        var defaultAccountAuthorizations = new List<AuthorizationEntity>
        {
            new() { Category = "ColADMIN", AuthorizationId = 10, Code = "COADMI001", Name = "View collabs" },
            new() { Category = "ColADMIN", AuthorizationId = 11, Code = "COADMI002", Name = "Delete collab" },
            new() { Category = "ColADMIN", AuthorizationId = 13, Code = "COADMI003", Name = "Update right collab" },
            new() { Category = "ColESC", AuthorizationId = 14, Code = "COMAND001", Name = "View mandate" },
            new() { Category = "ColOFFRE", AuthorizationId = 15, Code = "COOFF001", Name = "Deploy subscription" },
            new() { Category = "ColOFFRE", AuthorizationId = 16, Code = "COOFF002", Name = "Activate subscription" },
            new() { Category = "ColGESTION", AuthorizationId = 17, Code = "COGES001", Name = "View delegation" },
            new() { Category = "ColGESTION", AuthorizationId = 18, Code = "COCAL001", Name = "View Calendar" },
        };

        var contactAuthorizations = new List<AuthorizationEntity>
        {
            new() { Category = "ColESC", AuthorizationId = 14, Code = "COMAND001", Name = "View mandate" },
            new() { Category = "ColOFFRE", AuthorizationId = 15, Code = "COOFF001", Name = "Deploy subscription" },
            new() { Category = "ColOFFRE", AuthorizationId = 16, Code = "COOFF002", Name = "Activate subscription" },
            new() { Category = "ColGESTION", AuthorizationId = 17, Code = "COGES001", Name = "View delegation" },
        };

        _contactRepository.Setup(repository => repository.GetContactByIdAsync(contactId))
            .ReturnsAsync(contactMocked);

        _configurationRepository.Setup(repository => repository.GetAccountAuthorizationsAsync(accountId, GlobalConstants.CollabCategory, true))
            .ReturnsAsync(defaultAccountAuthorizations);

        _configurationRepository.Setup(repository => repository.GetContactAuthorizationsAsync(contactId, accountId))
            .ReturnsAsync(contactAuthorizations);

        var authorizationEventPublisherMock = new Mock<IAuthorizationEventPublisher>(MockBehavior.Strict);
        authorizationEventPublisherMock.Setup(a => a.PublishAuthorizationUpdatedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>(), false)).Verifiable();
        var configurationService = new ConfigurationService(_configurationRepository.Object, _contactRepository.Object, authorizationEventPublisherMock.Object, _accountRepositoryMock.Object, _historyEventPublisherMock.Object);

        // Act
        var eligibleContactAuthorizations = await configurationService.GetContactAccountConfigurationAsync(contactId, accountId);

        // Assertions for "ColESC" category
        var colEscCategory = eligibleContactAuthorizations.Single(c => c.Category == "ColESC");
        foreach (var action in colEscCategory.Actions)
        {
            if (action.ActionId == 14)
            {
                action.Enabled.Should().BeTrue();
            }
            else
            {
                action.Enabled.Should().BeFalse();
            }
        }

        // Assertions for "ColOFFRE" category
        var colOffreCategory = eligibleContactAuthorizations.Single(c => c.Category == "ColOFFRE");
        foreach (var action in colOffreCategory.Actions)
        {
            if (action.ActionId == 15 || action.ActionId == 16)
            {
                action.Enabled.Should().BeTrue();
            }
            else
            {
                action.Enabled.Should().BeFalse();
            }
        }

        // Assertions for "ColGESTION" category
        var colGestionCategory = eligibleContactAuthorizations.Single(c => c.Category == "ColGESTION");
        foreach (var action in colGestionCategory.Actions)
        {
            if (action.ActionId == 17)
            {
                action.Enabled.Should().BeTrue();
            }
            else
            {
                action.Enabled.Should().BeFalse();
            }
        }
    }

    [Fact]
    public async Task GetContactAccountConfigurationAsync_WhenContactAuthorizationIsEmpty_ShouldReturnConfigurationsWithEnabledAFalseForAllActions()
    {
        // Arrange
        var accountId = 123;
        var contactId = 456;

        var contactMocked = _fixture.Build<ContactEntity>()
                                    .With(c => c.ContactId, contactId)
                                    .With(c => c.Type, ContactType.Customer.ToString())
                                    .Create();

        var accountAuthorizations = new List<AuthorizationEntity>
        {
            new() { Category = "CltGESTION", AuthorizationId = 1, Code = "CLADMI001", Name = "Super Admin", },
            new() { Category = "CltGESTION", AuthorizationId = 2, Code = "CLUSER001", Name = "View User", },
            new() { Category = "CltGESTION", AuthorizationId = 3, Code = "CLUSER002", Name = "Add User", },
            new() { Category = "CltGESTION", AuthorizationId = 4, Code = "CLUSER002", Name = "Add User", },
            new() { Category = "CltGESTION", AuthorizationId = 5, Code = "CLUSER003", Name = "Delete User", },
            new() { Category = "CltGESTION", AuthorizationId = 6, Code = "CLOFF001", Name = "View offers", },
            new() { Category = "CltGESTION", AuthorizationId = 7, Code = "CLINFO001", Name = "View informations", },
        };

        var contactAuthorizations = new List<AuthorizationEntity>();

        _contactRepository.Setup(repository => repository.GetContactByIdAsync(contactId))
            .ReturnsAsync(contactMocked);

        _configurationRepository.Setup(repository => repository.GetAccountAuthorizationsAsync(accountId, GlobalConstants.CustomerCategory, true))
            .ReturnsAsync(accountAuthorizations);

        _configurationRepository.Setup(repository => repository.GetContactAuthorizationsAsync(contactId, accountId))
            .ReturnsAsync(contactAuthorizations);

        var authorizationEventPublisherMock = new Mock<IAuthorizationEventPublisher>(MockBehavior.Strict);
        authorizationEventPublisherMock.Setup(a => a.PublishAuthorizationUpdatedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>(), false)).Verifiable();
        var configurationService = new ConfigurationService(_configurationRepository.Object, _contactRepository.Object, authorizationEventPublisherMock.Object, _accountRepositoryMock.Object, _historyEventPublisherMock.Object);

        // Act
        var eligibleContactAuthorizations = await configurationService.GetContactAccountConfigurationAsync(contactId, accountId);

        // Assert
        var permissions = eligibleContactAuthorizations.FirstOrDefault();
        permissions!.Category.Should().Be("CltGESTION");
        permissions.Actions.Count().Should().Be(7);
        foreach (var action in permissions.Actions)
        {
            action.Enabled.Should().BeFalse();
        }
    }

    [Fact]
    public async Task GetContactAccountConfigurationAsync_WhenContactAuthorizationIsNotEmptyAndMatching_ShouldReturnConfigurationsWithEnabledATrueForMatchingActions()
    {
        // Arrange
        var accountId = 123;
        var contactId = 456;

        var contactMocked = _fixture.Build<ContactEntity>()
                                    .With(c => c.ContactId, contactId)
                                    .With(c => c.Type, ContactType.Customer.ToString())
                                    .Create();

        var accountAuthorizations = new List<AuthorizationEntity>
        {
            new() { Category = "CltGESTION", AuthorizationId = 1, Code = "CLADMI001", Name = "Super Admin", },
            new() { Category = "CltGESTION", AuthorizationId = 2, Code = "CLUSER001", Name = "View User", },
            new() { Category = "CltGESTION", AuthorizationId = 3, Code = "CLUSER002", Name = "Add User", },
            new() { Category = "CltGESTION", AuthorizationId = 4, Code = "CLUSER002", Name = "Add User", },
            new() { Category = "CltGESTION", AuthorizationId = 5, Code = "CLUSER003", Name = "Delete User", },
            new() { Category = "CltGESTION", AuthorizationId = 6, Code = "CLOFF001", Name = "View offers", },
            new() { Category = "CltGESTION", AuthorizationId = 7, Code = "CLINFO001", Name = "View informations", },
        };

        var contactAuthorizations = new List<AuthorizationEntity>
        {
            new () { Category = "CltGESTION", AuthorizationId = 1, Code = "CLADMI001", Name = "Super Admin" },
            new () { Category = "CltGESTION", AuthorizationId = 2, Code = "CLUSER001", Name = "View User" },
        };

        _contactRepository.Setup(repository => repository.GetContactByIdAsync(contactId))
            .ReturnsAsync(contactMocked);

        _configurationRepository.Setup(repository => repository.GetAccountAuthorizationsAsync(accountId, GlobalConstants.CustomerCategory, true))
            .ReturnsAsync(accountAuthorizations);

        _configurationRepository.Setup(repository => repository.GetContactAuthorizationsAsync(contactId, accountId))
            .ReturnsAsync(contactAuthorizations);

        var authorizationEventPublisherMock = new Mock<IAuthorizationEventPublisher>(MockBehavior.Strict);
        authorizationEventPublisherMock.Setup(a => a.PublishAuthorizationUpdatedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>(), false)).Verifiable();
        var configurationService = new ConfigurationService(_configurationRepository.Object, _contactRepository.Object, authorizationEventPublisherMock.Object, _accountRepositoryMock.Object, _historyEventPublisherMock.Object);

        // Act
        var eligibleContactAuthorizations = await configurationService.GetContactAccountConfigurationAsync(contactId, accountId);

        // Assert
        var permissions = eligibleContactAuthorizations.FirstOrDefault();
        permissions!.Category.Should().Be("CltGESTION");
        permissions.Actions.Count().Should().Be(7);
        permissions.Actions.First(a => a.ActionId == 1).Enabled.Should().BeTrue();
        permissions.Actions.First(a => a.ActionId == 2).Enabled.Should().BeTrue();
    }

    [Fact]
    public async Task CreateOrUpdateContactAccountAuthorizationAsync_ShouldReturnTaskCompleted()
    {
        // Arrange
        var accountId = 123;
        var contactId = 456;

        var contactMocked = _fixture.Build<ContactEntity>()
                                    .With(c => c.ContactId, contactId)
                                    .With(c => c.Type, ContactType.Customer.ToString())
                                    .Create();

        var codes = new List<string>() { "DDD", "EEE" };

        _contactRepository.Setup(repository => repository.GetContactByIdAsync(contactId))
            .ReturnsAsync(contactMocked);

        _configurationRepository.Setup(repository => repository.CreateOrUpdateContactAccountAuthorizationAsync(contactId, accountId, codes))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var authorizationEventPublisherMock = new Mock<IAuthorizationEventPublisher>(MockBehavior.Strict);
        authorizationEventPublisherMock.Setup(a => a.PublishAuthorizationUpdatedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>(), false))
            .Returns(Task.CompletedTask)
            .Verifiable();
        var configurationService = new ConfigurationService(_configurationRepository.Object, _contactRepository.Object, authorizationEventPublisherMock.Object, _accountRepositoryMock.Object, _historyEventPublisherMock.Object);

        // Act
        await configurationService.CreateOrUpdateContactAccountAuthorizationAsync(9, contactId, accountId, codes);

        // Assert
        _configurationRepository.VerifyAll();
        authorizationEventPublisherMock.Verify(x => x.PublishAuthorizationUpdatedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>(), false), Times.Once);
        _historyEventPublisherMock.Verify(x => x.PublishHistoryCreatedEvent(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()), Times.Once);
    }

    [Fact]
    public async Task CreateOrUpdateContactAccountAuthorizationAsync_GivenWrongAccountId_ShouldThrow_NotFoundException()
    {
        // Arrange
        var contactId = 456;

        var contactMocked = _fixture.Build<ContactEntity>()
                                    .With(c => c.ContactId, contactId)
                                    .With(c => c.Type, ContactType.Customer.ToString())
                                    .Create();

        var codes = new List<string>() { "DDD", "EEE" };

        _contactRepository.Setup(repository => repository.GetContactByIdAsync(contactId))
            .ReturnsAsync(contactMocked);

        var authorizationEventPublisherMock = new Mock<IAuthorizationEventPublisher>(MockBehavior.Strict);
        authorizationEventPublisherMock.Setup(a => a.PublishAuthorizationUpdatedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>(), false)).Verifiable();
        var configurationService = new ConfigurationService(_configurationRepository.Object, _contactRepository.Object, authorizationEventPublisherMock.Object, _accountRepositoryMock.Object, _historyEventPublisherMock.Object);

        // Act
        var act = async () => await configurationService.CreateOrUpdateContactAccountAuthorizationAsync(13, contactId, null, codes);

        // Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(act);
        Assert.Equal(Errors.NotFoundAccountCode, exception.Code);
        Assert.Equal(Errors.NotFoundAccountMessage, exception.Message);

        authorizationEventPublisherMock.Verify(a => a.PublishAuthorizationUpdatedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>(), false), Times.Never);
        _historyEventPublisherMock.Verify(x => x.PublishHistoryCreatedEvent(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()), Times.Never);
    }

    [Fact]
    public async Task CreateOrUpdateContactAccountAuthorizationAsync_GivenWrongContactId_ShouldThrow_NotFoundException()
    {
        // Arrange
        var configurationService = new ConfigurationService(null!, _contactRepository.Object, null!, null!, _historyEventPublisherMock.Object);

        // Act
        var act = async () => await configurationService.CreateOrUpdateContactAccountAuthorizationAsync(333, 999, It.IsAny<int>(), It.IsAny<IEnumerable<string>>());

        // Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(act);
        Assert.Equal(Errors.NotFoundContactCode, exception.Code);
        Assert.Equal(string.Format(Errors.NotFoundContactMessage, 999), exception.Message);

        _historyEventPublisherMock.Verify(x => x.PublishHistoryCreatedEvent(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()), Times.Never);
    }

    [Fact]
    public async Task CreateOrUpdateContactAccountAuthorizationAsync_WithFailOnRepository_ShouldThrowBadRequestException()
    {
        var contactId = 456;

        var contactMocked = _fixture.Build<ContactEntity>()
                                    .With(c => c.ContactId, contactId)
                                    .With(c => c.Type, ContactType.Customer.ToString())
                                    .Create();

        var codes = new List<string>() { "DDD", "EEE" };

        _contactRepository.Setup(repository => repository.GetContactByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(contactMocked);

        var exception = new ArgumentException();
        exception.Data["Code"] = codes[0];
        _configurationRepository.Setup(repo => repo.CreateOrUpdateContactAccountAuthorizationAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
            .ThrowsAsync(exception);

        var service = new ConfigurationService(_configurationRepository.Object, _contactRepository.Object, null!, null!, _historyEventPublisherMock.Object);

        var result = await Assert.ThrowsAsync<BadRequestException>(async () =>
            await service.CreateOrUpdateContactAccountAuthorizationAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()));

        Assert.NotNull(result);
        Assert.Equal(Errors.NotConfigurablePermissionCode, result.Code);
        Assert.Equal(string.Format(Errors.NotConfigurablePermissionMessage, exception.Data["Code"]), result.Message);

        _historyEventPublisherMock.Verify(x => x.PublishHistoryCreatedEvent(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()), Times.Never);
    }

    [Fact]
    public async Task GetAccountConfigurationAsync_WhenAccountNotFound_ThrowsNotFoundException()
    {
        // Arrange
        int accountId = 1;
        _accountRepositoryMock.Setup(x => x.GetAccountByIdAsync(accountId)).ReturnsAsync((AccountEntity)null!);
        var authorizationEventPublisherMock = new Mock<IAuthorizationEventPublisher>(MockBehavior.Strict);
        authorizationEventPublisherMock.Setup(a => a.PublishAuthorizationUpdatedEventAsync(null, It.IsAny<int>(), It.IsAny<List<string>>(), false)).Verifiable();
        var configurationService = new ConfigurationService(_configurationRepository.Object, _contactRepository.Object, authorizationEventPublisherMock.Object, _accountRepositoryMock.Object, _historyEventPublisherMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(async () =>
        {
            await configurationService.GetAccountConfigurationAsync(accountId, null);
        });

        exception.Message.Should().Be("L'identifiant de l'entité saisi 1 est introuvable");
        exception.Code.Should().Be(Errors.NotFoundAccountCode);
    }

    [Fact]
    public async Task GetAccountConfigurationAsync_ShouldRetunConfigurationsWithEnablesActions()
    {
        // Arrange
        var accountId = 123;

        var accountMocked = _fixture.Build<AccountEntity>()
                                    .With(c => c.AccountId, accountId)
                                    .Create();

        var authorizations = new List<AuthorizationEntity>
        {
            new() { Category = "CltGESTION", AuthorizationId = 1, Code = "CLADMI001", Name = "Super Admin", },
            new() { Category = "CltGESTION", AuthorizationId = 2, Code = "CLUSER001", Name = "View User", },
            new() { Category = "CltGESTION", AuthorizationId = 3, Code = "CLUSER002", Name = "Add User", },
            new() { Category = "CltGESTION", AuthorizationId = 4, Code = "CLUSER002", Name = "Add User", },
            new() { Category = "CltGESTION", AuthorizationId = 5, Code = "CLUSER003", Name = "Delete User", },
            new() { Category = "CltGESTION", AuthorizationId = 6, Code = "CLOFF001", Name = "View offers", },
            new() { Category = "CltGESTION", AuthorizationId = 7, Code = "CLINFO001", Name = "View informations", },
        };

        var accountAuthorizations = new List<AuthorizationEntity>
        {
            new () { Category = "CltGESTION", AuthorizationId = 6, Code = "CLOFF001", Name = "View offers" },
            new () { Category = "CltGESTION", AuthorizationId = 2, Code = "CLUSER001", Name = "View User" },
        };

        _accountRepositoryMock.Setup(a => a.GetAccountByIdAsync(It.IsAny<int>())).ReturnsAsync(accountMocked);

        _configurationRepository.Setup(repository => repository.GetAccountAuthorizationsAsync(accountId, GlobalConstants.CustomerCategory, true))
            .ReturnsAsync(accountAuthorizations);

        _configurationRepository.Setup(r => r.GetAvailableAuthorizationsAsync(It.IsAny<string>(), true)).ReturnsAsync(authorizations);

        var authorizationEventPublisherMock = new Mock<IAuthorizationEventPublisher>(MockBehavior.Strict);
        authorizationEventPublisherMock.Setup(a => a.PublishAuthorizationUpdatedEventAsync(null, It.IsAny<int>(), It.IsAny<List<string>>(), false)).Verifiable();
        var configurationService = new ConfigurationService(_configurationRepository.Object, _contactRepository.Object, authorizationEventPublisherMock.Object, _accountRepositoryMock.Object, _historyEventPublisherMock.Object);

        // Act
        var eligibleContactAuthorizations = await configurationService.GetAccountConfigurationAsync(accountId, GlobalConstants.CustomerCategory, true);

        // Assert
        var permissions = eligibleContactAuthorizations.FirstOrDefault();
        permissions!.Category.Should().Be("CltGESTION");
        permissions.Actions.Count().Should().Be(7);
        foreach (var action in permissions.Actions)
        {
            if (action.ActionId == 2 || action.ActionId == 6)
            {
                action.Enabled.Should().BeTrue();
            }
            else
            {
                action.Enabled.Should().BeFalse();
            }
        }
    }

    [Fact]
    public async Task GetAccountConfigurationAsync_WhenAccountAuthorizationIsEmpty_ShouldReturnConfigurationsWithEnabledAFalseForAllActions()
    {
        // Arrange
        var accountId = 123;

        var accountMocked = _fixture.Build<AccountEntity>()
                                    .With(c => c.AccountId, accountId)
                                    .Create();

        var authorizations = new List<AuthorizationEntity>
        {
            new() { Category = "CltGESTION", AuthorizationId = 1, Code = "CLADMI001", Name = "Super Admin", },
            new() { Category = "CltGESTION", AuthorizationId = 2, Code = "CLUSER001", Name = "View User", },
            new() { Category = "CltGESTION", AuthorizationId = 3, Code = "CLUSER002", Name = "Add User", },
            new() { Category = "CltGESTION", AuthorizationId = 4, Code = "CLUSER002", Name = "Add User", },
            new() { Category = "CltGESTION", AuthorizationId = 5, Code = "CLUSER003", Name = "Delete User", },
            new() { Category = "CltGESTION", AuthorizationId = 6, Code = "CLOFF001", Name = "View offers", },
            new() { Category = "CltGESTION", AuthorizationId = 7, Code = "CLINFO001", Name = "View informations", },
        };

        _accountRepositoryMock.Setup(a => a.GetAccountByIdAsync(It.IsAny<int>())).ReturnsAsync(accountMocked);

        _configurationRepository.Setup(repository => repository.GetAccountAuthorizationsAsync(accountId, GlobalConstants.CustomerCategory, true))
            .ReturnsAsync([]);

        _configurationRepository.Setup(repository => repository.GetAvailableAuthorizationsAsync(null, true))
            .ReturnsAsync(authorizations);

        var authorizationEventPublisherMock = new Mock<IAuthorizationEventPublisher>(MockBehavior.Strict);
        authorizationEventPublisherMock.Setup(a => a.PublishAuthorizationUpdatedEventAsync(null, It.IsAny<int>(), It.IsAny<List<string>>(), false)).Verifiable();
        var configurationService = new ConfigurationService(_configurationRepository.Object, _contactRepository.Object, authorizationEventPublisherMock.Object, _accountRepositoryMock.Object, _historyEventPublisherMock.Object);

        // Act
        var eligibleAccountConfiguration = await configurationService.GetAccountConfigurationAsync(accountId, null);

        // Assert
        var permissions = eligibleAccountConfiguration.FirstOrDefault();
        permissions!.Category.Should().Be("CltGESTION");
        permissions.Actions.Count().Should().Be(7);
        foreach (var action in permissions.Actions)
        {
            action.Enabled.Should().BeFalse();
        }
    }

    [Fact]
    public async Task GetAccountConfigurationAsync_WhenAccountAuthorizationIsNotEmptyAndMatching_ShouldReturnConfigurationsWithEnabledATrueForMatchingActions()
    {
        // Arrange
        var accountId = 123;
        var accountMocked = _fixture.Build<AccountEntity>()
                                    .With(c => c.AccountId, accountId)
                                    .Create();
        var authorizations = new List<AuthorizationEntity>
        {
            new() { Category = "CltGESTION", AuthorizationId = 1, Code = "CLADMI001", Name = "Super Admin", },
            new() { Category = "CltGESTION", AuthorizationId = 2, Code = "CLUSER001", Name = "View User", },
            new() { Category = "CltGESTION", AuthorizationId = 3, Code = "CLUSER002", Name = "Add User", },
            new() { Category = "CltGESTION", AuthorizationId = 4, Code = "CLUSER002", Name = "Add User", },
            new() { Category = "CltGESTION", AuthorizationId = 5, Code = "CLUSER003", Name = "Delete User", },
            new() { Category = "CltGESTION", AuthorizationId = 6, Code = "CLOFF001", Name = "View offers", },
            new() { Category = "CltGESTION", AuthorizationId = 7, Code = "CLINFO001", Name = "View informations", },
        };

        var accountAuthorizations = new List<AuthorizationEntity>
        {
            new () { Category = "CltGESTION", AuthorizationId = 1, Code = "CLADMI001", Name = "Super Admin" },
            new () { Category = "CltGESTION", AuthorizationId = 2, Code = "CLUSER001", Name = "View User" },
        };

        _accountRepositoryMock.Setup(a => a.GetAccountByIdAsync(It.IsAny<int>())).ReturnsAsync(accountMocked);

        _configurationRepository.Setup(repository => repository.GetAvailableAuthorizationsAsync(GlobalConstants.CustomerCategory, true))
            .ReturnsAsync(authorizations);

        _configurationRepository.Setup(repository => repository.GetAccountAuthorizationsAsync(accountId, GlobalConstants.CustomerCategory, true))
            .ReturnsAsync(accountAuthorizations);

        var authorizationEventPublisherMock = new Mock<IAuthorizationEventPublisher>(MockBehavior.Strict);
        authorizationEventPublisherMock.Setup(a => a.PublishAuthorizationUpdatedEventAsync(null, It.IsAny<int>(), It.IsAny<List<string>>(), false)).Verifiable();
        var configurationService = new ConfigurationService(_configurationRepository.Object, _contactRepository.Object, authorizationEventPublisherMock.Object, _accountRepositoryMock.Object, _historyEventPublisherMock.Object);

        // Act
        var eligibleAccountAuthorizations = await configurationService.GetAccountConfigurationAsync(accountId, GlobalConstants.CustomerCategory, true);

        // Assert
        var permissions = eligibleAccountAuthorizations.FirstOrDefault();
        permissions!.Category.Should().Be("CltGESTION");
        permissions.Actions.Count().Should().Be(7);
        permissions.Actions.First(a => a.ActionId == 1).Enabled.Should().BeTrue();
        permissions.Actions.First(a => a.ActionId == 2).Enabled.Should().BeTrue();
    }

    [Fact]
    public async Task CreateOrUpdateAccountAuthorizationAsync_ShouldReturnTaskCompleted()
    {
        // Arrange
        var accountId = 123;

        var accountMocked = _fixture.Build<AccountEntity>()
                                    .With(c => c.AccountId, accountId)
                                    .Create();

        var codes = new List<string>() { "DDD", "EEE" };

        _accountRepositoryMock.Setup(a => a.GetAccountByIdAsync(It.IsAny<int>())).ReturnsAsync(accountMocked);

        _configurationRepository.Setup(repository => repository.CreateOrUpdateAccountAuthorizationAsync(accountId, codes, GlobalConstants.CustomerCategory, true))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var authorizationEventPublisherMock = new Mock<IAuthorizationEventPublisher>(MockBehavior.Strict);
        authorizationEventPublisherMock.Setup(a => a.PublishAuthorizationUpdatedEventAsync(null, It.IsAny<int>(), It.IsAny<List<string>>(), false))
            .Returns(Task.CompletedTask)
            .Verifiable();
        var configurationService = new ConfigurationService(_configurationRepository.Object, _contactRepository.Object, authorizationEventPublisherMock.Object, _accountRepositoryMock.Object, _historyEventPublisherMock.Object);

        // Act
        await configurationService.CreateOrUpdateAccountAuthorizationAsync(accountId, codes, GlobalConstants.CustomerCategory, true);

        // Assert
        _configurationRepository.Verify();
        authorizationEventPublisherMock.Verify();
    }
}
