// <copyright file="ConfigurationRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Runtime.InteropServices;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Identity.Client;
using Pulse.Authorization.Core.Exceptions;
using Pulse.Authorization.Infrastructure.Constants;
using Pulse.Authorization.Infrastructure.Context;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Repositories;
using Pulse.ExceptionMiddleware.Exceptions;

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
            .Select(c => c.Authorization);

        var repository = new ConfigurationRepository(context);

        // Act
        var receivedAuthorization = await repository.GetAccountAuthorizationsAsync(accountId, GlobalConstants.CustomerCategory);

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
            .Select(c => c.Authorization);

        var repository = new ConfigurationRepository(context);

        // Act
        var receivedAuthorization = await repository.GetAccountAuthorizationsAsync(accountId, GlobalConstants.CollabCategory);

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
                        .Without(a => a.Account)
                        .CreateMany(10);

        foreach (var auth in contactAuthorizations)
        {
            auth.Authorization.Configurable = true;
        }

        var expectedAuthorization = contactAuthorizations
            .Select(c => c.Authorization);

        context.ContactAuthorizationEntity.AddRange(contactAuthorizations);
        await context.SaveChangesAsync();

        var repository = new ConfigurationRepository(context);

        // Act
        var receivedAuthorization = await repository.GetContactAuthorizationsAsync(123, 457);

        // Assert
        receivedAuthorization.Should().BeEquivalentTo(expectedAuthorization);
    }

    [Fact]
    public async Task CreateOrUpdateContactAccountAuthorizationAsync_WithExistingAuthorization_ShouldUpdateAuthorizations()
    {
        var options = new DbContextOptionsBuilder<AuthorizationContext>()
                            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                            .Options;

        // Arrange
        var contactId = 1251;
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
        var newAuthorization = await context.ContactAuthorizationEntity.Include(c => c.Authorization).Where(c => c.ContactId == contactId && c.AccountId == accountId && newExpectedCode.Contains(c.Authorization.Code)).ToListAsync();

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
                            .With(a => a.IsActive, true)
                            .Without(a => a.ContactAuthorizationEntity)
                            .Create();
        var auth1 = _fixture.Build<AuthorizationEntity>()
            .With(a => a.Code, "DDD")
            .With(a => a.Configurable, true)
            .Create();
        var auth2 = _fixture.Build<AuthorizationEntity>()
            .With(a => a.Code, "EEE")
            .With(a => a.Configurable, true)
            .Create();

        context.AccountEntity.Add(accountEntity);
        context.ContactEntity.Add(contactEntity);
        context.AuthorizationEntity.AddRange(new List<AuthorizationEntity> { auth1, auth2 });
        await context.SaveChangesAsync();

        var repository = new ConfigurationRepository(context);

        // Act
        await repository.CreateOrUpdateContactAccountAuthorizationAsync(contactId, accountId, codes);
        var newAuthorization = await context.ContactAuthorizationEntity.Include(c => c.Authorization).Where(c => c.ContactId == contactId && c.AccountId == accountId && codes.Contains(c.Authorization.Code)).ToListAsync();

        // Assert
        newAuthorization.Should().NotBeNullOrEmpty();
        newAuthorization.Select(x => x.Authorization.Code).Should().BeEquivalentTo(codes);
    }

    [Fact]
    public async Task CreateOrUpdateContactAccountAuthorizationAsync_ShouldThrowArgumentException_IfOneOfTheCodesIsNotConfigurable()
    {
        var options = new DbContextOptionsBuilder<AuthorizationContext>()
                            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                            .Options;

        string code = "HAKOUNA_MATATA";
        var authorizationEntity = _fixture.Build<AuthorizationEntity>()
            .With(x => x.Code, code)
            .With(x => x.Configurable, false)
            .Create();

        int contactId = 1;
        int accountId = 1;

        using (var context = new AuthorizationContext(options))
        {
            context.AuthorizationEntity.Add(authorizationEntity);
            context.SaveChanges();

            var repository = new ConfigurationRepository(context);
            var action = async () => await repository.CreateOrUpdateContactAccountAuthorizationAsync(contactId, accountId, new List<string> { code });

            var exceptionResult = await action.Should().ThrowAsync<BadRequestException>();
            exceptionResult.WithMessage($"La permission avec le code suivant: HAKOUNA_MATATA n'est pas configurable.");
            exceptionResult.Which.Code.Should().Be(Errors.NotConfigurablePermissionCode);
        }
    }

    [Fact]
    public async Task CreateOrUpdateAccountAuhtorizationAsync_ShouldDeleteOldAuthorizations_IfExisted()
    {
        string code = "SI_TU_NEXISTE_PAS";
        string type = "BANANAS_ON_THE_GROUND";
        int accountId = 1;
        int authorizationId = 1;
        AccountEntity accountEntity = _fixture.Build<AccountEntity>()
            .With(x => x.AccountId, 1)
            .Without(x => x.AccountAuthorizationEntity)
            .Without(x => x.ContactAuthorizationEntity)
            .Without(x => x.RoleEntity)
            .Create();

        AuthorizationEntity existingAuthorizationEntity = _fixture.Build<AuthorizationEntity>()
            .With(x => x.AuthorizationId, 1)
            .With(x => x.Code, code)
            .With(x => x.Configurable, true)
            .With(x => x.Type, type)
            .Without(x => x.Persona)
            .Without(x => x.AccountAuthorizationEntity)
            .Without(x => x.ContactAuthorizationEntity)
            .Create();

        List<AuthorizationEntity> newAuthorizationEntities = _fixture.Build<AuthorizationEntity>()
            .With(x => x.Configurable, true)
            .With(x => x.Type, type)
            .Without(x => x.Persona)
            .Without(x => x.AccountAuthorizationEntity)
            .Without(x => x.ContactAuthorizationEntity)
            .CreateMany(3).ToList();

        AccountAuthorizationEntity accountAuthorizationEntity = new AccountAuthorizationEntity { AccountId = accountId, AuthorizationId = authorizationId, Enabled = true };

        var dbOptions = new DbContextOptionsBuilder<AuthorizationContext>()
                            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                            .Options;
        using (var context = new AuthorizationContext(dbOptions))
        {
            context.AccountEntity.Add(accountEntity);
            context.AuthorizationEntity.Add(existingAuthorizationEntity);
            context.AuthorizationEntity.AddRange(newAuthorizationEntities);
            context.SaveChanges();
            context.AccountAuthorizationEntity.Add(accountAuthorizationEntity);
            context.SaveChanges();

            var repos = new ConfigurationRepository(context);
            List<string> codes = new List<string> { code };
            codes.AddRange(newAuthorizationEntities.Select(x => x.Code).ToList());

            await repos.CreateOrUpdateAccountAuthorizationAsync(accountId, codes, type);

            var authorizations = context.AccountAuthorizationEntity.Include(x => x.Authorization)
                .Where(x => x.Enabled == true
                && x.AccountId == 1
                && x.Authorization.Type.Equals(type))
                .ToList();

            authorizations.Count().Should().Be(4);
            // this means that the old account authorization is well deleted and recreated.
            authorizations.FirstOrDefault(x => x.AccountId == 1 && x.AuthorizationId == 1).Should().NotBeNull();
        }
    }

    [Fact]
    public async Task CreateOrUpdateAccountAuthorizationAsync_WithExistingAuthorization_ShouldUpdateAuthorizations()
    {
        var options = new DbContextOptionsBuilder<AuthorizationContext>()
                            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                            .Options;

        // Arrange
        var accountId = 459;
        using var context = new AuthorizationContext(options);
        var accountEntity = _fixture.Build<AccountEntity>()
                            .With(a => a.AccountId, accountId)
                            .Without(a => a.AccountAuthorizationEntity)
                            .Without(a => a.ContactAuthorizationEntity)
                            .Create();
        var authorizationMock = _fixture.Build<AuthorizationEntity>()
            .With(a => a.Configurable, true)
            .CreateMany(3)
            .ToList();
        var oldAccountAuthorizations = _fixture.Build<AccountAuthorizationEntity>()
                        .With(a => a.AccountId, accountId)
                        .Without(a => a.Account)
                        .Without(a => a.Authorization)
                        .CreateMany(3)
                        .ToList();
        oldAccountAuthorizations.ForEach(auth => auth.Authorization = authorizationMock[oldAccountAuthorizations.IndexOf(auth)]);

        var newAuthorizationEntity = _fixture.Build<AuthorizationEntity>()
                        .Without(a => a.ContactAuthorizationEntity)
                        .Without(a => a.AccountAuthorizationEntity)
                        .With(a => a.Configurable, true)
                        .CreateMany(3)
                        .ToList();
        var newAccountAuthorizations = _fixture.Build<AccountAuthorizationEntity>()
                        .With(a => a.AccountId, accountId)
                        .Without(a => a.Account)
                        .Without(a => a.Authorization)
                        .CreateMany(3)
                        .ToList();
        newAccountAuthorizations.ForEach(auth => auth.Authorization = newAuthorizationEntity[newAccountAuthorizations.IndexOf(auth)]);

        var newExpectedCode = newAccountAuthorizations.Select(x => x.Authorization.Code);

        context.AccountEntity.Add(accountEntity);
        context.AccountAuthorizationEntity.AddRange(oldAccountAuthorizations);
        context.AuthorizationEntity.AddRange(newAuthorizationEntity);
        await context.SaveChangesAsync();

        var repository = new ConfigurationRepository(context);

        // Act
        await repository.CreateOrUpdateAccountAuthorizationAsync(accountId, newExpectedCode, GlobalConstants.CustomerCategory, true);
        var newAuthorization = await context.AccountAuthorizationEntity.Include(c => c.Authorization).Where(c => c.AccountId == accountId && newExpectedCode.Contains(c.Authorization.Code)).ToListAsync();

        // Assert
        newAuthorization.Should().NotBeNullOrEmpty();
        newAuthorization.Select(x => x.Authorization.Code).Should().BeEquivalentTo(newExpectedCode);
        newAuthorization.Select(x => x.AuthorizationId).Should().BeEquivalentTo(newAuthorizationEntity.Select(x => x.AuthorizationId));
    }

    [Fact]
    public async Task CreateOrUpdateAccountAuthorizationAsync_WithNoExistingAuthorization_ShouldCreateAuthorizations()
    {
        var options = new DbContextOptionsBuilder<AuthorizationContext>()
                            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                            .Options;

        // Arrange
        var accountId = 460;
        var codes = new List<string>() { "DDD", "EEE" };
        using var context = new AuthorizationContext(options);
        var accountEntity = _fixture.Build<AccountEntity>()
                            .With(a => a.AccountId, accountId)
                            .Without(a => a.AccountAuthorizationEntity)
                            .Without(a => a.ContactAuthorizationEntity)
                            .Create();
        var auth1 = _fixture.Build<AuthorizationEntity>()
            .With(a => a.Code, "DDD")
            .With(a => a.Configurable, true)
            .Create();
        var auth2 = _fixture.Build<AuthorizationEntity>()
            .With(a => a.Code, "EEE")
            .With(a => a.Configurable, true)
            .Create();

        context.AccountEntity.Add(accountEntity);
        context.AuthorizationEntity.AddRange(new List<AuthorizationEntity> { auth1, auth2 });
        await context.SaveChangesAsync();

        var repository = new ConfigurationRepository(context);

        // Act
        await repository.CreateOrUpdateAccountAuthorizationAsync(accountId, codes, GlobalConstants.CustomerCategory, true);
        var newAuthorization = await context.AccountAuthorizationEntity.Include(c => c.Authorization).Where(c => c.AccountId == accountId && codes.Contains(c.Authorization.Code)).ToListAsync();

        // Assert
        newAuthorization.Should().NotBeNullOrEmpty();
        newAuthorization.Select(x => x.Authorization.Code).Should().BeEquivalentTo(codes);
    }

    [Fact]
    public async Task GetAvailableAuthorizationAsync_ShouldReturnAllAuthorizations_IfConfigurableIsFalse()
    {
        List<AuthorizationEntity> authorizationEntities = _fixture.Build<AuthorizationEntity>()
            .Without(x => x.AccountAuthorizationEntity)
            .Without(x => x.ContactAuthorizationEntity)
            .CreateMany(10).ToList();

        var dbOptions = new DbContextOptionsBuilder<AuthorizationContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

        using (var context = new AuthorizationContext(dbOptions))
        {
            context.AuthorizationEntity.AddRange(authorizationEntities);
            context.SaveChanges();

            var repos = new ConfigurationRepository(context);

            var list = await repos.GetAvailableAuthorizationsAsync(type: string.Empty, configurable: false);

            list.Count().Should().Be(10);
        }
    }

    [Fact]
    public async Task GetAvailableAuthorization_ShouldReturnOnlyOneType_IfTypeIsNotNull()
    {
        string authorizationType = "Hakouna_Matata";

        List<AuthorizationEntity> authorizationEntities = _fixture.Build<AuthorizationEntity>()
           .Without(x => x.AccountAuthorizationEntity)
           .Without(x => x.ContactAuthorizationEntity)
           .CreateMany(10).ToList();

        List<AuthorizationEntity> hakounaMatataType = _fixture.Build<AuthorizationEntity>()
           .Without(x => x.AccountAuthorizationEntity)
           .Without(x => x.ContactAuthorizationEntity)
           .With(x => x.Type, authorizationType)
           .CreateMany(4).ToList();

        var dbOptions = new DbContextOptionsBuilder<AuthorizationContext>()
          .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

        using (var context = new AuthorizationContext(dbOptions))
        {
            context.AuthorizationEntity.AddRange(authorizationEntities);
            context.AuthorizationEntity.AddRange(hakounaMatataType);
            context.SaveChanges();

            var repos = new ConfigurationRepository(context);

            var list = await repos.GetAvailableAuthorizationsAsync(type: authorizationType, configurable: false);

            list.Count().Should().Be(4);
            list.All(x => x.Type.Equals(authorizationType)).Should().BeTrue();
        }
    }

    [Fact]
    public async Task GetAvailableAuthorization_ShouldReturnAllConfigurabkeAuthorizations_IfTypeIsNullButConfigurableIsTrue()
    {
        List<AuthorizationEntity> nonConfigurableAuthorizations = _fixture.Build<AuthorizationEntity>()
           .Without(x => x.AccountAuthorizationEntity)
           .Without(x => x.ContactAuthorizationEntity)
           .With(x => x.Configurable, false)
           .CreateMany(8).ToList();

        List<AuthorizationEntity> configurableAuthorizations = _fixture.Build<AuthorizationEntity>()
           .Without(x => x.AccountAuthorizationEntity)
           .Without(x => x.ContactAuthorizationEntity)
           .With(x => x.Configurable, true)
           .CreateMany(6).ToList();

        var dbOptions = new DbContextOptionsBuilder<AuthorizationContext>()
          .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

        using (var context = new AuthorizationContext(dbOptions))
        {
            context.AuthorizationEntity.AddRange(nonConfigurableAuthorizations);
            context.AuthorizationEntity.AddRange(configurableAuthorizations);
            context.SaveChanges();

            var repos = new ConfigurationRepository(context);

            var list = await repos.GetAvailableAuthorizationsAsync(type: string.Empty, configurable: true);

            list.Count().Should().Be(6);
            list.All(x => x.Configurable == true).Should().BeTrue();
        }
    }

    [Fact]
    public async Task GetAvailableAuthorization_ShouldReturnOneTypeWithConfigurationsEnabled_IfTypeIsNotNullAndConfigurableIsTrue()
    {
        string authorizationType = "Hekma_taamaha_laziz";

        List<AuthorizationEntity> configurableAuthorizations = _fixture.Build<AuthorizationEntity>()
           .Without(x => x.AccountAuthorizationEntity)
           .Without(x => x.ContactAuthorizationEntity)
           .With(x => x.Configurable, false)
           .With(x => x.Type, authorizationType)
           .CreateMany(10).ToList();

        List<AuthorizationEntity> nonConfigurableAuthorizations = _fixture.Build<AuthorizationEntity>()
           .Without(x => x.AccountAuthorizationEntity)
           .Without(x => x.ContactAuthorizationEntity)
           .With(x => x.Configurable, true)
           .With(x => x.Type, authorizationType)
           .CreateMany(12).ToList();

        var dbOptions = new DbContextOptionsBuilder<AuthorizationContext>()
          .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

        using (var context = new AuthorizationContext(dbOptions))
        {
            context.AuthorizationEntity.AddRange(configurableAuthorizations);
            context.AuthorizationEntity.AddRange(nonConfigurableAuthorizations);
            context.SaveChanges();

            var repos = new ConfigurationRepository(context);

            var list = await repos.GetAvailableAuthorizationsAsync(type: authorizationType, configurable: true);

            list.Count().Should().Be(12);
            list.All(x => x.Type.Equals(authorizationType) && x.Configurable == true).Should().BeTrue();
        }
    }
}
