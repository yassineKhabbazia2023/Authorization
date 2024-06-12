// <copyright file="AuthorizationRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pulse.Authorization.Infrastructure.Context;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Enum;
using Pulse.Authorization.Infrastructure.Repositories;

namespace Pulse.Authorization.Infrastructure.Tests.Repositories;

public class AuthorizationRepositoryTests
{
    private readonly DbContextOptions<AuthorizationContext> _options;
    private readonly Fixture _fixture;

    public AuthorizationRepositoryTests()
    {
        _options = new DbContextOptionsBuilder<AuthorizationContext>()
                            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                            .Options;
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public async Task GetContactAndAccountAuthorizations_Return_AuthorizationCode()
    {
        using (var context = new AuthorizationContext(_options))
        {
            var contactAuthorizationAccountEntity = _fixture.Build<ContactAuthorizationEntity>()
                            .With(a => a.Authorization)
                            .With(a => a.ContactId, 123)
                            .With(a => a.AccountId, 456)
                            .Without(a => a.Contact)
                            .Without(a => a.Account)
                            .CreateMany(3);

            var expectedAuthorization = contactAuthorizationAccountEntity.Select(c => c.Authorization.Code);

            context.ContactAuthorizationEntity.AddRange(contactAuthorizationAccountEntity);
            await context.SaveChangesAsync();

            var repository = new AuthorizationRepository(context);

            var contactId = contactAuthorizationAccountEntity.First().ContactId;
            var accountId = contactAuthorizationAccountEntity.First().AccountId;

            var receivedAuthorization = await repository.GetContactAccountAuthorizationsAsync(contactId, accountId, null);

            var authExpectJson = JsonConvert.SerializeObject(expectedAuthorization);
            var authResultJson = JsonConvert.SerializeObject(receivedAuthorization);
            Assert.Equal(authExpectJson, authResultJson);
            Assert.NotNull(receivedAuthorization);
        }
    }

    [Fact]
    public async Task GetAccountAuthorizations_Return_AuthorizationCode()
    {
        using (var context = new AuthorizationContext(_options))
        {
            var accountAuthorizationAccountEntity = _fixture.Build<AccountAuthorizationEntity>()
                            .With(a => a.Authorization)
                            .With(a => a.AccountId, 456)
                            .Without(a => a.Account)
                            .CreateMany(3);

            var expectedAuthorization = accountAuthorizationAccountEntity.Select(c => c.Authorization.Code);

            context.AccountAuthorizationEntity.AddRange(accountAuthorizationAccountEntity);
            await context.SaveChangesAsync();

            var repository = new AuthorizationRepository(context);

            var accountId = accountAuthorizationAccountEntity.First().AccountId;

            var receivedAuthorization = await repository.GetAccountAuthorizationAsync(accountId);

            var authExpectJson = JsonConvert.SerializeObject(expectedAuthorization);
            var authResultJson = JsonConvert.SerializeObject(receivedAuthorization);
            Assert.Equal(authExpectJson, authResultJson);
            Assert.NotNull(receivedAuthorization);
        }
    }

    [Fact]
    public async Task GetContactAndAccountAuthorizationsAsync_Return_Empty()
    {
        using (var context = new AuthorizationContext(_options))
        {
            var contactAuthorizationAccountEntity = _fixture.Build<ContactAuthorizationEntity>()
                            .With(a => a.Authorization)
                            .With(a => a.ContactId, 123)
                            .With(a => a.AccountId, 456)
                            .Without(a => a.Contact)
                            .CreateMany(3);

            var expectedAuthorization = contactAuthorizationAccountEntity.Select(c => c.Authorization.Code);

            context.ContactAuthorizationEntity.AddRange(contactAuthorizationAccountEntity);
            await context.SaveChangesAsync();

            var repository = new AuthorizationRepository(context);

            var contactId = contactAuthorizationAccountEntity.First().ContactId;
            var accountId = contactAuthorizationAccountEntity.First().AccountId;

            var receivedAuthorization = await repository.GetContactAccountAuthorizationsAsync(999, 888, null);

            Assert.Empty(receivedAuthorization);
        }
    }

    [Fact]
    public async Task GetContactAndAccountAuthorizationsAsync_Cas_Global()
    {
        using (var context = new AuthorizationContext(_options))
        {
            var contactAuthorizationAccountEntity = _fixture.Build<ContactAuthorizationEntity>()
                            .With(a => a.Authorization)
                            .With(a => a.ContactId, 123)
                            .With(a => a.AccountId, 456)
                            .Without(a => a.Contact)
                            .CreateMany(3);
            var permissionGlobal = contactAuthorizationAccountEntity.Where(x => x.Authorization.View == AuthorizationView.Global.ToString()).ToList();

            var expectedAuthorization = contactAuthorizationAccountEntity.Select(c => c.Authorization.Code);

            context.ContactAuthorizationEntity.AddRange(contactAuthorizationAccountEntity);
            await context.SaveChangesAsync();

            var repository = new AuthorizationRepository(context);

            var contactId = contactAuthorizationAccountEntity.First().ContactId;
            var accountId = contactAuthorizationAccountEntity.First().AccountId;

            var receivedAuthorization = await repository.GetContactAccountAuthorizationsAsync(123, 456, true);

            Assert.Equal(permissionGlobal.Count, receivedAuthorization.Count);
        }
    }

    [Fact]
    public async Task GetContactAndAccountAuthorizationsAsync_Cas_Partial()
    {
        using (var context = new AuthorizationContext(_options))
        {
            var contactAuthorizationAccountEntity = _fixture.Build<ContactAuthorizationEntity>()
                            .With(a => a.Authorization)
                            .With(a => a.ContactId, 123)
                            .With(a => a.AccountId, 456)
                            .Without(a => a.Contact)
                            .CreateMany(3);
            var permissionPartial = contactAuthorizationAccountEntity.Where(x => x.Authorization.View == AuthorizationView.Partial.ToString()).ToList();

            var expectedAuthorization = contactAuthorizationAccountEntity.Select(c => c.Authorization.Code);

            context.ContactAuthorizationEntity.AddRange(contactAuthorizationAccountEntity);
            await context.SaveChangesAsync();

            var repository = new AuthorizationRepository(context);

            var contactId = contactAuthorizationAccountEntity.First().ContactId;
            var accountId = contactAuthorizationAccountEntity.First().AccountId;

            var receivedAuthorization = await repository.GetContactAccountAuthorizationsAsync(123, 456, false);

            Assert.Equal(permissionPartial.Count, receivedAuthorization.Count);
        }
    }

    [Fact]
    public async Task GetContactAuthorizationsAsync_Return_AuthorizationCode()
    {
        using (var context = new AuthorizationContext(_options))
        {
            var contactAuthorizationAccountEntity = _fixture.Build<ContactAuthorizationEntity>()
                            .With(a => a.Authorization)
                            .With(a => a.ContactId, 123)
                            .Without(a => a.Contact)
                            .Without(a => a.Account)
                            .CreateMany(3);

            var expectedAuthorization = contactAuthorizationAccountEntity.Select(c => c.Authorization.Code);

            context.ContactAuthorizationEntity.AddRange(contactAuthorizationAccountEntity);
            await context.SaveChangesAsync();

            var repository = new AuthorizationRepository(context);

            var contactId = contactAuthorizationAccountEntity.First().ContactId;
            var accountId = contactAuthorizationAccountEntity.First().AccountId;

            var receivedAuthorization = await repository.GetContactAuthorizationAsync(contactId);

            var authExpectJson = JsonConvert.SerializeObject(expectedAuthorization);
            var authResultJson = JsonConvert.SerializeObject(receivedAuthorization);
            Assert.Equal(authExpectJson, authResultJson);
            Assert.NotNull(receivedAuthorization);
        }
    }

    [Fact]
    public async Task DeletePermissionAsync_Should_DeletePermission()
    {
        using (var context = new AuthorizationContext(_options))
        {
            var contactAuthorizationAccountEntity = _fixture.Build<ContactAuthorizationEntity>()
                            .With(a => a.Authorization)
                            .With(a => a.ContactId, 123)
                            .With(a => a.AccountId, -1)
                            .Without(a => a.Contact)
                            .Without(a => a.Account)
                            .CreateMany(3);
            context.ContactAuthorizationEntity.AddRange(contactAuthorizationAccountEntity);
            await context.SaveChangesAsync();

            var repository = new AuthorizationRepository(context);

            var permissionBefore = await repository.GetContactAccountAuthorizationsAsync(123, -1, null);
            Assert.NotEmpty(permissionBefore);

            await repository.DeleteContactAuthorizationAsync(123, -1);
            var permissionAfter = await repository.GetContactAccountAuthorizationsAsync(123, -1, null);
            Assert.Empty(permissionAfter);
        }
    }

    [Fact]
    public async Task AddSubscriptionAuthorizationsOnAccountAsync_Should_AddAccountAuthorizations_And_ReturnSaidAuthorizations()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AuthorizationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        var expectedCount = 3;
        var authorizationEntities = _fixture.Build<AuthorizationEntity>()
                            .With(a => a.ProductCode)
                            .CreateMany(expectedCount);

        var account = _fixture.Build<AccountEntity>()
            .With(a => a.AccountId, 42)
            .Create();

        using var context = new AuthorizationContext(options);
        var productCodes = authorizationEntities.Select(a => a.ProductCode);
        context.AccountEntity.Add(account);
        context.AuthorizationEntity.AddRange(authorizationEntities);
        context.SaveChanges();
        var repository = new AuthorizationRepository(context);

        // Act
        var result = await repository.AddSubscriptionAuthorizationsOnAccountAsync(account.AccountId, productCodes!);

        // Assert
        Assert.NotNull(result);
        result.Should().NotBeEmpty();
        Assert.Equal(result.Count(), expectedCount);
    }

    [Fact]
    public async Task AddSubscriptionAuthorizationsOnContactAsync_Should_AddContactAuthorizations_And_ReturnSaidAuthorizations()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AuthorizationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        var authorizationEntities = _fixture.Build<AuthorizationEntity>()
                            .With(a => a.ProductCode)
                            .CreateMany(3);
        var account = _fixture.Build<AccountEntity>()
            .With(a => a.AccountId, 42)
            .Create();

        var contacts = _fixture.Build<ContactEntity>()
            .With(c => c.Type, ContactType.Customer.ToString())
            .CreateMany(12).DistinctBy(c => c.ContactId);

        using (var context = new AuthorizationContext(options))
        {
            context.AccountEntity.Add(account);
            context.ContactEntity.AddRange(contacts);
            context.AuthorizationEntity.AddRange(authorizationEntities);
            context.SaveChanges();

            var roles = contacts.Select(c =>
            {
                return new RoleEntity()
                {
                    ContactId = c.ContactId,
                    AccountId = account.AccountId,
                    IsSignatory = true,
                };
            });

            context.RoleEntity.AddRange(roles);
            context.SaveChanges();
        }

        using var ct = new AuthorizationContext(options);
        var contactIds = contacts.Select(c => c.ContactId);
        var productCodes = authorizationEntities.Select(a => a.ProductCode).Distinct();
        var repository = new AuthorizationRepository(ct);

        // Act
        var result = await repository.AddSubscriptionAuthorizationsOnAccountSignatoriesAsync(account.AccountId, contactIds, productCodes!);

        // Assert
        Assert.NotNull(result);
        result.Should().NotBeEmpty();
        Assert.Equal(result.Count(), productCodes.Count() * contacts.Count());
    }
}
