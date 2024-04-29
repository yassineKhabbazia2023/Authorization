// <copyright file="ConfigurationRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Pulse.Authorization.Infrastructure.Context;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Mappers;
using Pulse.Authorization.Infrastructure.Repositories;

namespace Pulse.Authorization.Infrastructure.Tests.Repositories;

public class ConfigurationRepositoryTests
{
    private readonly DbContextOptions<AuthorizationContext> _options;
    private readonly Fixture _fixture;

    public ConfigurationRepositoryTests()
    {
        _options = new DbContextOptionsBuilder<AuthorizationContext>()
                            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                            .Options;
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public async Task GetAccountConfigurationAsync_WhenAccountHasAuthorization_ShouldReturnsConfigurations()
    {
        // Arrange
        using var context = new AuthorizationContext(_options);
        var accountId = 456;
        var accoutEntity = new AccountEntity
        {
            AccountId = accountId,
            AccountNumber = "AAZZEEE4578",
            LegalName = "Test",
            AccountGlobalUniqueId = Guid.NewGuid(),
        };

        var accountAuthorizations = _fixture.Build<AccountAuthorization>()
                .With(a => a.Authorization)
                .With(a => a.AccountId, accountId)
                .With(a => a.Account, accoutEntity)
                .CreateMany(10);

        foreach (var auth in accountAuthorizations)
        {
            auth.Authorization.Configurable = true;
        }

        await context.AccountAuthorization.AddRangeAsync(accountAuthorizations);
        await context.SaveChangesAsync();

        var expectedAuthorization = accountAuthorizations
            .Select(c => c.Authorization)
            .MapAuthorizationToConfiguration();

        var repository = new ConfigurationRepository(context);

        // Act
        var receivedAuthorization = await repository.GetAccountConfigurationAsync(accountId);

        // Assert
        receivedAuthorization.Should().BeEquivalentTo(expectedAuthorization);
    }

    [Fact]
    public async Task GetContacttConfigurationAsync_WhenContactHasAuthorization_ShouldReturnsConfigurations()
    {
        // Arrange
        using var context = new AuthorizationContext(_options);
        var contactAuthorizations = _fixture.Build<ContactAuthorization>()
                        .With(a => a.Authorization)
                        .With(a => a.ContactId, 123)
                        .With(a => a.AccountId, 456)
                        .Without(a => a.Contact)
                        .CreateMany(10);

        foreach (var auth in contactAuthorizations)
        {
            auth.Authorization.Configurable = true;
        }

        var expectedAuthorization = contactAuthorizations
            .Select(c => c.Authorization)
            .MapAuthorizationToConfiguration();

        context.ContactAuthorization.AddRange(contactAuthorizations);
        await context.SaveChangesAsync();

        var repository = new ConfigurationRepository(context);

        // Act
        var receivedAuthorization = await repository.GetContactConfigurationAsync(123);

        // Assert
        receivedAuthorization.Should().BeEquivalentTo(expectedAuthorization);
    }
}
