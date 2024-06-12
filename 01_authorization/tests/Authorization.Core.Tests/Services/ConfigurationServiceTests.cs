// <copyright file="ConfigurationServiceTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using FluentAssertions;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Moq;
using Pulse.Authorization.Core.Interfaces;
using Pulse.Authorization.Core.Models;
using Pulse.Authorization.Core.Services;
using Pulse.Authorization.Core.Exceptions;
using Kpmg.ExceptionMiddleware.AdvancedException;
using Pulse.Authorization.Infrastructure.Interfaces;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Constants;
using Pulse.Authorization.Infrastructure.Enum;

namespace Pulse.Authorization.Core.Tests.Services;

public class ConfigurationServiceTests
{
    private readonly Mock<IConfigurationRepository> _configurationRepository;
    private readonly Mock<IContactRepository> _contactRepository;
    private readonly Fixture _fixture;

    public ConfigurationServiceTests()
    {
        _configurationRepository = new Mock<IConfigurationRepository>();
        _contactRepository = new Mock<IContactRepository>();
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public async Task GetContactAccountConfigurationAsync_WhenContactNotFound_ThrowsNotFoundException()
    {
        // Arrange
        int contactId = 1;
        int accountId = 1;
        _contactRepository.Setup(x => x.GetContactByIdAsync(contactId)).ReturnsAsync((ContactEntity)null!);
        var configurationService = new ConfigurationService(_configurationRepository.Object, _contactRepository.Object);

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
        var configurationService = new ConfigurationService(_configurationRepository.Object, _contactRepository.Object);

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

        var contactRepository = new Mock<IContactRepository>(MockBehavior.Strict);
        contactRepository.Setup(repository => repository.GetContactByIdAsync(contactId))
            .ReturnsAsync(contactMocked);

        _configurationRepository.Setup(repository => repository.GetAccountConfigurationAsync(accountId, GlobalConstants.CustomerCategory))
            .ReturnsAsync(accountAuthorizations);

        _configurationRepository.Setup(repository => repository.GetContactConfigurationAsync(contactId, accountId))
            .ReturnsAsync(contactAuthorizations);

        var configurationService = new ConfigurationService(_configurationRepository.Object, contactRepository.Object);

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

        var contactRepository = new Mock<IContactRepository>(MockBehavior.Strict);
        contactRepository.Setup(repository => repository.GetContactByIdAsync(contactId))
            .ReturnsAsync(contactMocked);

        _configurationRepository.Setup(repository => repository.GetAccountConfigurationAsync(accountId, GlobalConstants.CustomerCategory))
            .ReturnsAsync(accountAuthorizations);

        _configurationRepository.Setup(repository => repository.GetContactConfigurationAsync(contactId, accountId))
            .ReturnsAsync(contactAuthorizations);

        var configurationService = new ConfigurationService(_configurationRepository.Object, contactRepository.Object);

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

        var contactRepository = new Mock<IContactRepository>(MockBehavior.Strict);
        contactRepository.Setup(repository => repository.GetContactByIdAsync(contactId))
            .ReturnsAsync(contactMocked);

        _configurationRepository.Setup(repository => repository.GetAccountConfigurationAsync(accountId, GlobalConstants.CollabCategory))
            .ReturnsAsync(accountAuthorizations);

        _configurationRepository.Setup(repository => repository.GetContactConfigurationAsync(contactId, accountId))
            .ReturnsAsync(contactAuthorizations);

        var configurationService = new ConfigurationService(_configurationRepository.Object, contactRepository.Object);

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

        var contactRepository = new Mock<IContactRepository>(MockBehavior.Strict);
        contactRepository.Setup(repository => repository.GetContactByIdAsync(contactId))
            .ReturnsAsync(contactMocked);

        _configurationRepository.Setup(repository => repository.GetAccountConfigurationAsync(accountId, GlobalConstants.CollabCategory))
            .ReturnsAsync(defaultAccountAuthorizations);

        _configurationRepository.Setup(repository => repository.GetContactConfigurationAsync(contactId, accountId))
            .ReturnsAsync(contactAuthorizations);

        var configurationService = new ConfigurationService(_configurationRepository.Object, contactRepository.Object);

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

        var contactRepository = new Mock<IContactRepository>(MockBehavior.Strict);
        contactRepository.Setup(repository => repository.GetContactByIdAsync(contactId))
            .ReturnsAsync(contactMocked);

        _configurationRepository.Setup(repository => repository.GetAccountConfigurationAsync(accountId, GlobalConstants.CustomerCategory))
            .ReturnsAsync(accountAuthorizations);

        _configurationRepository.Setup(repository => repository.GetContactConfigurationAsync(contactId, accountId))
            .ReturnsAsync(contactAuthorizations);

        var configurationService = new ConfigurationService(_configurationRepository.Object, contactRepository.Object);

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

        var contactRepository = new Mock<IContactRepository>(MockBehavior.Strict);
        contactRepository.Setup(repository => repository.GetContactByIdAsync(contactId))
            .ReturnsAsync(contactMocked);

        _configurationRepository.Setup(repository => repository.GetAccountConfigurationAsync(accountId, GlobalConstants.CustomerCategory))
            .ReturnsAsync(accountAuthorizations);

        _configurationRepository.Setup(repository => repository.GetContactConfigurationAsync(contactId, accountId))
            .ReturnsAsync(contactAuthorizations);

        var configurationService = new ConfigurationService(_configurationRepository.Object, contactRepository.Object);

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

        var contactRepository = new Mock<IContactRepository>(MockBehavior.Strict);
        contactRepository.Setup(repository => repository.GetContactByIdAsync(contactId))
            .ReturnsAsync(contactMocked);

        _configurationRepository.Setup(repository => repository.CreateOrUpdateContactAccountAuthorizationAsync(contactId, accountId, codes))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var configurationService = new ConfigurationService(_configurationRepository.Object, contactRepository.Object);

        // Act
        await configurationService.CreateOrUpdateContactAccountAuthorizationAsync(contactId, accountId, codes);

        // Assert
        _configurationRepository.Verify();
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

        var contactRepository = new Mock<IContactRepository>(MockBehavior.Strict);
        contactRepository.Setup(repository => repository.GetContactByIdAsync(contactId))
            .ReturnsAsync(contactMocked);

        var configurationService = new ConfigurationService(_configurationRepository.Object, contactRepository.Object);

        // Act
        var act = async () => await configurationService.CreateOrUpdateContactAccountAuthorizationAsync(contactId, null, codes);

        // Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(act);
        Assert.Equal(Errors.NotFoundAccountCode, exception.Code);
        Assert.Equal(Errors.NotFoundAccountMessage, exception.Message);
    }

    [Fact]
    public async Task CreateOrUpdateContactAccountAuthorizationAsync_GivenWrongContactId_ShouldThrow_NotFoundException()
    {
        // Arrange
        var accountId = 123;
        var contactId = 456;

        var contactMocked = _fixture.Build<ContactEntity>()
                                    .With(c => c.ContactId, contactId)
                                    .With(c => c.Type, ContactType.Customer.ToString())
                                    .Create();

        var codes = new List<string>() { "DDD", "EEE" };

        var contactRepository = new Mock<IContactRepository>(MockBehavior.Strict);
        contactRepository.Setup(repository => repository.GetContactByIdAsync(999))
            .ThrowsAsync(new NotFoundException(Errors.NotFoundContactCode, string.Format(Errors.NotFoundContactMessage, 999)));

        var configurationService = new ConfigurationService(_configurationRepository.Object, contactRepository.Object);

        // Act
        var act = async () => await configurationService.CreateOrUpdateContactAccountAuthorizationAsync(999, accountId, codes);

        // Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(act);
        Assert.Equal(Errors.NotFoundContactCode, exception.Code);
        Assert.Equal(string.Format(Errors.NotFoundContactMessage, 999), exception.Message);
    }
}
