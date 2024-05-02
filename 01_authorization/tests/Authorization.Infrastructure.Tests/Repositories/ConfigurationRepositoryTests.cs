// <copyright file="ConfigurationRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using FluentAssertions;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Microsoft.EntityFrameworkCore;
using Pulse.Authorization.Core.Exceptions;
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

    [Fact]
    public async Task DeleteContactAccountAuthorizationAsync_ShouldReturn_OK()
    {
        // Arrange
        var contactId = 123;
        var accountId = 456;
        using var context = new AuthorizationContext(_options);
        var contactAuthorizations = _fixture.Build<ContactAuthorization>()
                        .With(a => a.Authorization)
                        .With(a => a.ContactId, contactId)
                        .With(a => a.AccountId, accountId)
                        .Without(a => a.Contact)
                        .CreateMany(10);

        var expectedCode = contactAuthorizations.Select(x => x.Authorization.Code);

        context.ContactAuthorization.AddRange(contactAuthorizations);
        await context.SaveChangesAsync();

        var repository = new ConfigurationRepository(context);

        // Act
        await repository.DeleteContactAccountAuthorizationAsync(contactAuthorizations);
        var receivedAuthorization = await repository.GetContactAccountConfigurationAsync(contactId, accountId);

        // Assert
        receivedAuthorization.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateContactAccountAuthorizationAsync_ShouldReturn_OK()
    {
        // Arrange
        var contactId = 123;
        var accountId = 456;
        using var context = new AuthorizationContext(_options);
        var accountEntity = _fixture.Build<AccountEntity>()
                            .With(a => a.AccountId, accountId)
                            .Without(a => a.AccountAuthorization)
                            .Without(a => a.ContactAuthorization)
                            .Create();
        var contactEntity = _fixture.Build<ContactEntity>()
                            .With(a => a.ContactId, contactId)
                            .Without(a => a.ContactAuthorization)
                            .Create();
        var oldAuthorizationEntity = _fixture.Build<AuthorizationEntity>()
                        .Without(a => a.ContactAuthorization)
                        .Without(a => a.AccountAuthorization)
                        .CreateMany(3)
                        .ToList();
        var oldContactAuthorizations = _fixture.Build<ContactAuthorization>()
                        .With(a => a.ContactId, contactId)
                        .With(a => a.AccountId, accountId)
                        .Without(a => a.Contact)
                        .Without(a => a.Account)
                        .Without(a => a.Authorization)
                        .CreateMany(3)
                        .ToList();
        oldContactAuthorizations.ForEach(auth => auth.Authorization = oldAuthorizationEntity[oldContactAuthorizations.IndexOf(auth)]);

        var newAuthorizationEntity = _fixture.Build<AuthorizationEntity>()
                        .Without(a => a.ContactAuthorization)
                        .Without(a => a.AccountAuthorization)
                        .CreateMany(3)
                        .ToList();
        var newContactAuthorizations = _fixture.Build<ContactAuthorization>()
                        .With(a => a.ContactId, contactId)
                        .With(a => a.AccountId, accountId)
                        .Without(a => a.Contact)
                        .Without(a => a.Authorization)
                        .CreateMany(3)
                        .ToList();
        newContactAuthorizations.ForEach(auth => auth.Authorization = newAuthorizationEntity[newContactAuthorizations.IndexOf(auth)]);

        var newExpectedCode = newContactAuthorizations.Select(x => x.Authorization.Code);

        context.AccountEntity.Add(accountEntity);
        context.ContactEntity.Add(contactEntity);
        context.ContactAuthorization.AddRange(oldContactAuthorizations);
        context.AuthorizationEntity.AddRange(newAuthorizationEntity);
        await context.SaveChangesAsync();

        var repository = new ConfigurationRepository(context);

        // Act
        await repository.UpdateContactAccountAuthorizationAsync(contactId, accountId, newExpectedCode);
        var newAuthorization = await repository.GetContactAccountConfigurationAsync(contactId, accountId);

        // Assert
        newAuthorization.Should().NotBeNullOrEmpty();
        newAuthorization.Select(x => x.Authorization.Code).Should().BeEquivalentTo(newExpectedCode);
        newAuthorization.Select(x => x.AuthorizationId).Should().BeEquivalentTo(newAuthorizationEntity.Select(x => x.AuthorizationId));
    }

    [Fact]
    public async Task UpdateContactAccountAuthorizationAsync_ShouldThrow_NotFoundException()
    {
        // Arrange
        var contactId = 123;
        var accountId = 456;
        var codes = new List<string>() { "DDD", "EEE" };
        using var context = new AuthorizationContext(_options);
        var accountEntity = _fixture.Build<AccountEntity>()
                            .With(a => a.AccountId, accountId)
                            .Without(a => a.AccountAuthorization)
                            .Without(a => a.ContactAuthorization)
                            .Create();
        var contactEntity = _fixture.Build<ContactEntity>()
                            .With(a => a.ContactId, contactId)
                            .Without(a => a.ContactAuthorization)
                            .Create();

        context.AccountEntity.Add(accountEntity);
        context.ContactEntity.Add(contactEntity);
        await context.SaveChangesAsync();

        var repository = new ConfigurationRepository(context);

        // Act
        var act = async () => await repository.UpdateContactAccountAuthorizationAsync(contactId, accountId, codes);

        // Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(act);
        Assert.Equal(Errors.NotFoundContactAccountAuthCode, exception.Code);
        Assert.Equal(string.Format(Errors.NotFoundContactAccountAuthCode, contactId, string.Join('-', codes), accountId), exception.Message);
    }
}
