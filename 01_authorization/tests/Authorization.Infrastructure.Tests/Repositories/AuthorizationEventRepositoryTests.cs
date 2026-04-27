// <copyright file="AuthorizationEventRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Pulse.Authorization.Infrastructure.Context;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Providers;

namespace Pulse.Authorization.Infrastructure.Tests.Repositories;

public class AuthorizationEventRepositoryTests
{
    private readonly Fixture _fixture;

    public AuthorizationEventRepositoryTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    private DbContextOptions<AuthorizationContext> CreateNewContextOptions()
    {
        return new DbContextOptionsBuilder<AuthorizationContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task IsPennylaneActivatedAsync_Nominal_ShouldReturnTrue()
    {
        using var context = new AuthorizationContext(CreateNewContextOptions());

        var authorizations = new List<AuthorizationEntity>
        {
            _fixture.Build<AuthorizationEntity>()
            .With(a => a.AuthorizationId, 1).With(a => a.Code, "CLPEN001")
            .Without(a => a.AccountAuthorizationEntity).Without(a => a.ContactAuthorizationEntity)
            .Create(),
            _fixture.Build<AuthorizationEntity>()
            .With(a => a.AuthorizationId, 2).With(a => a.Code, "COPEN001")
            .Without(a => a.AccountAuthorizationEntity).Without(a => a.ContactAuthorizationEntity)
            .Create(),
            _fixture.Build<AuthorizationEntity>()
            .With(a => a.AuthorizationId, 3).With(a => a.Code, "CLGEDPEN01")
            .Without(a => a.AccountAuthorizationEntity).Without(a => a.ContactAuthorizationEntity)
            .Create(),
        };
        await context.AuthorizationEntity.AddRangeAsync(authorizations);

        var accountAuthorizations = new List<AccountAuthorizationEntity>
        {
            _fixture.Build<AccountAuthorizationEntity>()
            .With(aa => aa.AccountId, 100)
            .With(aa => aa.AuthorizationId, 1)
            .Without(aa => aa.Account).Without(aa => aa.Authorization)
            .Create(),
            _fixture.Build<AccountAuthorizationEntity>()
            .With(aa => aa.AccountId, 100)
            .With(aa => aa.AuthorizationId, 2)
            .Without(aa => aa.Account).Without(aa => aa.Authorization)
            .Create(),
            _fixture.Build<AccountAuthorizationEntity>()
            .With(aa => aa.AccountId, 100)
            .With(aa => aa.AuthorizationId, 3)
            .Without(aa => aa.Account).Without(aa => aa.Authorization)
            .Create(),
        };
        await context.AccountAuthorizationEntity.AddRangeAsync(accountAuthorizations);

        var account = _fixture.Build<AccountEntity>()
            .With(a => a.AccountId, 100)
            .With(a => a.IsActive, true)
            .Without(a => a.AccountAuthorizationEntity)
            .Without(a => a.ContactAuthorizationEntity)
            .Create();
        await context.AccountEntity.AddAsync(account);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new AuthorizationEventRepository(context);

        var result = await repository.IsPennylaneActivatedAsync(100);

        Assert.True(result);
    }

    [Fact]
    public async Task IsPennylaneActivatedAsync_WhenAccountNotExist_ShouldReturnFalse()
    {
        using var context = new AuthorizationContext(CreateNewContextOptions());

        var authorizations = new List<AuthorizationEntity>
        {
            _fixture.Build<AuthorizationEntity>()
            .With(a => a.AuthorizationId, 4).With(a => a.Code, "CLPEN001")
            .Without(a => a.AccountAuthorizationEntity).Without(a => a.ContactAuthorizationEntity)
            .Create(),
            _fixture.Build<AuthorizationEntity>()
            .With(a => a.AuthorizationId, 5).With(a => a.Code, "COPEN001")
            .Without(a => a.AccountAuthorizationEntity).Without(a => a.ContactAuthorizationEntity)
            .Create(),
            _fixture.Build<AuthorizationEntity>()
            .With(a => a.AuthorizationId, 6).With(a => a.Code, "CLGEDPEN01")
            .Without(a => a.AccountAuthorizationEntity).Without(a => a.ContactAuthorizationEntity)
            .Create(),
        };
        await context.AuthorizationEntity.AddRangeAsync(authorizations);

        var accountAuthorizations = new List<AccountAuthorizationEntity>
        {
            _fixture.Build<AccountAuthorizationEntity>()
            .With(aa => aa.AccountId, 101)
            .With(aa => aa.AuthorizationId, 4)
            .Without(aa => aa.Account).Without(aa => aa.Authorization)
            .Create(),
            _fixture.Build<AccountAuthorizationEntity>()
            .With(aa => aa.AccountId, 101)
            .With(aa => aa.AuthorizationId, 5)
            .Without(aa => aa.Account).Without(aa => aa.Authorization)
            .Create(),
            _fixture.Build<AccountAuthorizationEntity>()
            .With(aa => aa.AccountId, 101)
            .With(aa => aa.AuthorizationId, 6)
            .Without(aa => aa.Account).Without(aa => aa.Authorization)
            .Create(),
        };
        await context.AccountAuthorizationEntity.AddRangeAsync(accountAuthorizations);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new AuthorizationEventRepository(context);

        var result = await repository.IsPennylaneActivatedAsync(1);

        Assert.False(result);
    }

    [Fact]
    public async Task IsPennylaneActivatedAsync_WhenAccountDontHavePennylaneAuthorization_ShouldReturnFalse()
    {
        using var context = new AuthorizationContext(CreateNewContextOptions());

        var authorizations = new List<AuthorizationEntity>
        {
            _fixture.Build<AuthorizationEntity>()
            .With(a => a.AuthorizationId, 7).With(a => a.Code, "COADMI001")
            .Without(a => a.AccountAuthorizationEntity).Without(a => a.ContactAuthorizationEntity)
            .Create(),
            _fixture.Build<AuthorizationEntity>()
            .With(a => a.AuthorizationId, 8).With(a => a.Code, "CORAPP001")
            .Without(a => a.AccountAuthorizationEntity).Without(a => a.ContactAuthorizationEntity)
            .Create(),
            _fixture.Build<AuthorizationEntity>()
            .With(a => a.AuthorizationId, 9).With(a => a.Code, "CLRAPP001")
            .Without(a => a.AccountAuthorizationEntity).Without(a => a.ContactAuthorizationEntity)
            .Create(),
        };
        await context.AuthorizationEntity.AddRangeAsync(authorizations);

        var accountAuthorizations = new List<AccountAuthorizationEntity>
        {
            _fixture.Build<AccountAuthorizationEntity>()
            .With(aa => aa.AccountId, 102)
            .With(aa => aa.AuthorizationId, 7)
            .Without(aa => aa.Account).Without(aa => aa.Authorization)
            .Create(),
            _fixture.Build<AccountAuthorizationEntity>()
            .With(aa => aa.AccountId, 102)
            .With(aa => aa.AuthorizationId, 8)
            .Without(aa => aa.Account).Without(aa => aa.Authorization)
            .Create(),
            _fixture.Build<AccountAuthorizationEntity>()
            .With(aa => aa.AccountId, 103)
            .With(aa => aa.AuthorizationId, 9)
            .Without(aa => aa.Account).Without(aa => aa.Authorization)
            .Create(),
        };
        await context.AccountAuthorizationEntity.AddRangeAsync(accountAuthorizations);

        var account = _fixture.Build<AccountEntity>()
            .With(a => a.AccountId, 102)
            .With(a => a.IsActive, true)
            .Without(a => a.AccountAuthorizationEntity)
            .Without(a => a.ContactAuthorizationEntity)
            .Create();
        await context.AccountEntity.AddAsync(account);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new AuthorizationEventRepository(context);

        var result = await repository.IsPennylaneActivatedAsync(102);

        Assert.False(result);
    }

    [Fact]
    public async Task IsPennylaneActivatedAsync_WhenAccountNotActive_ShouldReturnFalse()
    {
        using var context = new AuthorizationContext(CreateNewContextOptions());

        var authorizations = new List<AuthorizationEntity>
        {
            _fixture.Build<AuthorizationEntity>()
            .With(a => a.AuthorizationId, 10).With(a => a.Code, "CLPEN001")
            .Without(a => a.AccountAuthorizationEntity).Without(a => a.ContactAuthorizationEntity)
            .Create(),
            _fixture.Build<AuthorizationEntity>()
            .With(a => a.AuthorizationId, 11).With(a => a.Code, "COPEN001")
            .Without(a => a.AccountAuthorizationEntity).Without(a => a.ContactAuthorizationEntity)
            .Create(),
            _fixture.Build<AuthorizationEntity>()
            .With(a => a.AuthorizationId, 12).With(a => a.Code, "CLGEDPEN01")
            .Without(a => a.AccountAuthorizationEntity).Without(a => a.ContactAuthorizationEntity)
            .Create(),
        };
        await context.AuthorizationEntity.AddRangeAsync(authorizations);

        var accountAuthorizations = new List<AccountAuthorizationEntity>
        {
            _fixture.Build<AccountAuthorizationEntity>()
            .With(aa => aa.AccountId, 103)
            .With(aa => aa.AuthorizationId, 10)
            .Without(aa => aa.Account).Without(aa => aa.Authorization)
            .Create(),
            _fixture.Build<AccountAuthorizationEntity>()
            .With(aa => aa.AccountId, 103)
            .With(aa => aa.AuthorizationId, 11)
            .Without(aa => aa.Account).Without(aa => aa.Authorization)
            .Create(),
            _fixture.Build<AccountAuthorizationEntity>()
            .With(aa => aa.AccountId, 103)
            .With(aa => aa.AuthorizationId, 12)
            .Without(aa => aa.Account).Without(aa => aa.Authorization)
            .Create(),
        };
        await context.AccountAuthorizationEntity.AddRangeAsync(accountAuthorizations);

        var account = _fixture.Build<AccountEntity>()
            .With(a => a.AccountId, 103)
            .With(a => a.IsActive, false)
            .Without(a => a.AccountAuthorizationEntity)
            .Without(a => a.ContactAuthorizationEntity)
            .Create();
        await context.AccountEntity.AddAsync(account);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new AuthorizationEventRepository(context);

        var result = await repository.IsPennylaneActivatedAsync(103);

        Assert.False(result);
    }
}
