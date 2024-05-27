// <copyright file="ConfigurationRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Pulse.Authorization.Core.Constants;
using Pulse.Authorization.Infrastructure.Context;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Mappers;
using Pulse.Authorization.Infrastructure.Repositories;

namespace Pulse.Authorization.Infrastructure.Tests.Repositories;

public class ConfigurationRepositoryTests
{
    private readonly Fixture _fixture;

    public ConfigurationRepositoryTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public async Task GetAccountConfigurationAsync_WhenClientHasAuthorization_ShouldReturnsConfigurations()
    {
        var options = new DbContextOptionsBuilder<AuthorizationContext>()
                            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                            .Options;

        // Arrange
        using var context = new AuthorizationContext(options);
        var accountId = 456;

        var accountEntity = _fixture.Build<AccountEntity>()
                .With(a => a.AccountId, accountId)
                .With(a => a.AccountNumber, "AAZZEEE4578")
                .With(a => a.LegalName, "Test")
                .With(a => a.AccountGlobalUniqueId, Guid.NewGuid())
                .Without(a => a.AccountAuthorizationEntity)
                .Without(a => a.ContactAuthorizationEntity)
                .Without(a => a.RoleEntity)
                .Create();

        var accountAuthorizations = _fixture.Build<AccountAuthorizationEntity>()
                .With(a => a.Authorization)
                .With(a => a.AccountId, accountId)
                .With(a => a.Account, accountEntity)
                .CreateMany(10);

        foreach (var auth in accountAuthorizations)
        {
            auth.Authorization.Configurable = true;
            auth.Authorization.Type = GlobalConstants.CustomerCategory;
        }

        await context.AccountAuthorizationEntity.AddRangeAsync(accountAuthorizations);
        await context.SaveChangesAsync();

        var expectedAuthorization = accountAuthorizations
            .Select(c => c.Authorization)
            .MapAuthorizationToConfiguration();

        var repository = new ConfigurationRepository(context);

        // Act
        var receivedAuthorization = await repository.GetAccountConfigurationAsync(accountId, GlobalConstants.CustomerCategory);

        // Assert
        receivedAuthorization.Should().BeEquivalentTo(expectedAuthorization);
    }

    [Fact]
    public async Task GetAccountConfigurationAsync_WhenCollabHasAuthorization_ShouldReturnsConfigurations()
    {
        var options = new DbContextOptionsBuilder<AuthorizationContext>()
                            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                            .Options;

        // Arrange
        using var context = new AuthorizationContext(options);
        var accountId = 456;
        var accountEntity = _fixture.Build<AccountEntity>()
                .With(a => a.AccountId, accountId)
                .With(a => a.AccountNumber, "AAZZEEE4578")
                .With(a => a.LegalName, "Test")
                .With(a => a.AccountGlobalUniqueId, Guid.NewGuid())
                .Without(a => a.AccountAuthorizationEntity)
                .Without(a => a.ContactAuthorizationEntity)
                .Without(a => a.RoleEntity)
                .Create();

        var accountAuthorizations = _fixture.Build<AccountAuthorizationEntity>()
                .With(a => a.Authorization)
                .With(a => a.AccountId, accountId)
                .With(a => a.Account, accountEntity)
                .CreateMany(10);

        foreach (var auth in accountAuthorizations)
        {
            auth.Authorization.Configurable = true;
            auth.Authorization.Type = GlobalConstants.CollabCategory;
        }

        await context.AccountAuthorizationEntity.AddRangeAsync(accountAuthorizations);
        await context.SaveChangesAsync();

        var expectedAuthorization = accountAuthorizations
            .Select(c => c.Authorization)
            .MapAuthorizationToConfiguration();

        var repository = new ConfigurationRepository(context);

        // Act
        var receivedAuthorization = await repository.GetAccountConfigurationAsync(accountId, GlobalConstants.CollabCategory);

        // Assert
        receivedAuthorization.Should().BeEquivalentTo(expectedAuthorization);
    }

    [Fact]
    public async Task GetContacttConfigurationAsync_WhenContactHasAuthorization_ShouldReturnsConfigurations()
    {
        var options = new DbContextOptionsBuilder<AuthorizationContext>()
                            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                            .Options;

        // Arrange
        using var context = new AuthorizationContext(options);
        var contactAuthorizations = _fixture.Build<ContactAuthorizationEntity>()
                        .With(a => a.Authorization)
                        .With(a => a.ContactId, 123)
                        .With(a => a.AccountId, 457)
                        .Without(a => a.Contact)
                        .CreateMany(10);

        foreach (var auth in contactAuthorizations)
        {
            auth.Authorization.Configurable = true;
        }

        var expectedAuthorization = contactAuthorizations
            .Select(c => c.Authorization)
            .MapAuthorizationToConfiguration();

        context.ContactAuthorizationEntity.AddRange(contactAuthorizations);
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
        var options = new DbContextOptionsBuilder<AuthorizationContext>()
                            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                            .Options;

        // Arrange
        var contactId = 124;
        var accountId = 458;
        using var context = new AuthorizationContext(options);
        var contactAuthorizations = _fixture.Build<ContactAuthorizationEntity>()
                        .With(a => a.Authorization)
                        .With(a => a.ContactId, contactId)
                        .With(a => a.AccountId, accountId)
                        .Without(a => a.Contact)
                        .CreateMany(10);

        var expectedCode = contactAuthorizations.Select(x => x.Authorization.Code);

        context.ContactAuthorizationEntity.AddRange(contactAuthorizations);
        await context.SaveChangesAsync();

        var repository = new ConfigurationRepository(context);

        // Act
        await repository.DeleteContactAccountAuthorizationAsync(contactAuthorizations);
        var receivedAuthorization = await repository.GetContactAccountConfigurationAsync(contactId, accountId);

        // Assert
        receivedAuthorization.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateOrUpdateContactAccountAuthorizationAsync_WithExistingAuthorization_ShouldUpdateAuthorizations()
    {
        var options = new DbContextOptionsBuilder<AuthorizationContext>()
                            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                            .Options;

        // Arrange
        var contactId = 125;
        var accountId = 459;
        using var context = new AuthorizationContext(options);
        var accountEntity = _fixture.Build<AccountEntity>()
                            .With(a => a.AccountId, accountId)
                            .Without(a => a.AccountAuthorizationEntity)
                            .Without(a => a.ContactAuthorizationEntity)
                            .Create();
        var contactEntity = _fixture.Build<ContactEntity>()
                            .With(a => a.ContactId, contactId)
                            .Without(a => a.ContactAuthorizationEntity)
                            .Create();
        var authorizationMock = _fixture.Build<AuthorizationEntity>()
            .With(a => a.Configurable, true)
            .CreateMany(3)
            .ToList();
        var oldContactAuthorizations = _fixture.Build<ContactAuthorizationEntity>()
                        .With(a => a.ContactId, contactId)
                        .With(a => a.AccountId, accountId)
                        .Without(a => a.Contact)
                        .Without(a => a.Account)
                        .Without(a => a.Authorization)
                        .CreateMany(3)
                        .ToList();
        oldContactAuthorizations.ForEach(auth => auth.Authorization = authorizationMock[oldContactAuthorizations.IndexOf(auth)]);

        var newAuthorizationEntity = _fixture.Build<AuthorizationEntity>()
                        .Without(a => a.ContactAuthorizationEntity)
                        .Without(a => a.AccountAuthorizationEntity)
                        .With(a => a.Configurable, true)
                        .CreateMany(3)
                        .ToList();
        var newContactAuthorizations = _fixture.Build<ContactAuthorizationEntity>()
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
        context.ContactAuthorizationEntity.AddRange(oldContactAuthorizations);
        context.AuthorizationEntity.AddRange(newAuthorizationEntity);
        await context.SaveChangesAsync();

        var repository = new ConfigurationRepository(context);

        // Act
        await repository.CreateOrUpdateContactAccountAuthorizationAsync(contactId, accountId, newExpectedCode);
        var newAuthorization = await repository.GetContactAccountConfigurationAsync(contactId, accountId);

        // Assert
        newAuthorization.Should().NotBeNullOrEmpty();
        newAuthorization.Select(x => x.Authorization.Code).Should().BeEquivalentTo(newExpectedCode);
        newAuthorization.Select(x => x.AuthorizationId).Should().BeEquivalentTo(newAuthorizationEntity.Select(x => x.AuthorizationId));
    }

    [Fact]
    public async Task CreateOrUpdateContactAccountAuthorizationAsync_WithNoExistingAuthorization_ShouldCreateAuthorizations()
    {
        var options = new DbContextOptionsBuilder<AuthorizationContext>()
                            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                            .Options;
        // Arrange
        var contactId = 12;
        var accountId = 460;
        var codes = new List<string>() { "DDD", "EEE" };
        using var context = new AuthorizationContext(options);
        var accountEntity = _fixture.Build<AccountEntity>()
                            .With(a => a.AccountId, accountId)
                            .Without(a => a.AccountAuthorizationEntity)
                            .Without(a => a.ContactAuthorizationEntity)
                            .Create();
        var contactEntity = _fixture.Build<ContactEntity>()
                            .With(a => a.ContactId, contactId)
                            .Without(a => a.ContactAuthorizationEntity)
                            .Create();
        var auth1 = _fixture.Build<AuthorizationEntity>()
            .With(a => a.Code, "DDD")
            .Create();
        var auth2 = _fixture.Build<AuthorizationEntity>()
            .With(a => a.Code, "EEE")
            .Create();

        context.AccountEntity.Add(accountEntity);
        context.ContactEntity.Add(contactEntity);
        context.AuthorizationEntity.AddRange(new List<AuthorizationEntity> { auth1, auth2 });
        await context.SaveChangesAsync();

        var repository = new ConfigurationRepository(context);

        // Act
        await repository.CreateOrUpdateContactAccountAuthorizationAsync(contactId, accountId, codes);
        var newAuthorization = await repository.GetContactAccountConfigurationAsync(contactId, accountId);

        // Assert
        newAuthorization.Should().NotBeNullOrEmpty();
        newAuthorization.Select(x => x.Authorization.Code).Should().BeEquivalentTo(codes);
    }
}
