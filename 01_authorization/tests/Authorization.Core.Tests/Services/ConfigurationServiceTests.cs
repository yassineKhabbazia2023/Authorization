// <copyright file="ConfigurationServiceTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Reflection.Emit;
using System.Xml.Linq;
using System;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Equivalency;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Moq;
using Pulse.Authorization.Core.Exceptions;
using Pulse.Authorization.Core.Interfaces;
using Pulse.Authorization.Core.Models;
using Pulse.Authorization.Core.Services;
using Pulse.Authorization.Core.Constants;

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
    public async Task GetContactAccountConfigurationAsync_ContactNotFound_ThrowsNotFoundException()
    {
        // Arrange
        int contactId = 1;
        int accountId = 1;
        _contactRepository.Setup(x => x.GetContactByIdAsync(contactId)).ReturnsAsync((Contact)null!);
        var configurationService = new ConfigurationService(_configurationRepository.Object, _contactRepository.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(async () =>
        {
            await configurationService.GetContactAccountConfigurationAsync(contactId, accountId);
        });
    }

    [Fact]
    public async Task GetContactAccountConfigurationAsync_WhenContactTypeIsCustomer_ShouldReturnsContactConfigurations()
    {
        // Arrange
        var accountId = 123;
        var contactId = 456;

        var contactMocked = _fixture.Build<Contact>()
                                    .With(c => c.ContactId, contactId)
                                    .With(c => c.Type, ContactType.Customer.ToString())
                                    .Create();

        var accountAuthorizations = new List<Configuration>
        {
            new Configuration
            {
                Category = "CltGESTION",
                Actions = new List<Models.Action>
                {
                    new Models.Action { ActionId = 1, Code = "CLADMI001", Name = "Super Admin" },
                    new Models.Action { ActionId = 2, Code = "CLUSER001", Name = "View User" },
                    new Models.Action { ActionId = 3, Code = "CLUSER002", Name = "Add User" },
                    new Models.Action { ActionId = 4, Code = "CLUSER002", Name = "Add User" },
                    new Models.Action { ActionId = 5, Code = "CLUSER003", Name = "Delete User" },
                    new Models.Action { ActionId = 6, Code = "CLOFF001", Name = "View offers" },
                    new Models.Action { ActionId = 7, Code = "CLINFO001", Name = "View informations" },
                }
            }
        };

        var contactAuthorizations = new List<Configuration>
        {
                new Configuration
                {
                    Category = "CltGESTION",
                    Actions = new List<Models.Action>
                    {
                        new Models.Action { ActionId = 2, Code = "CLUSER001", Name = "View User" },
                        new Models.Action { ActionId = 6, Code = "CLOFF001", Name = "View offers" },
                    }
                }
        };

        var contactRepository = new Mock<IContactRepository>(MockBehavior.Strict);
        contactRepository.Setup(repository => repository.GetContactByIdAsync(contactId))
            .ReturnsAsync(contactMocked);

        _configurationRepository.Setup(repository => repository.GetAccountConfigurationAsync(accountId))
            .ReturnsAsync(accountAuthorizations);

        _configurationRepository.Setup(repository => repository.GetContactConfigurationAsync(contactId))
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
}
