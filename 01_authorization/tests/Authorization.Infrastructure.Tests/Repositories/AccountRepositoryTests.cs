// <copyright file="AccountRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Pulse.Authorization.Infrastructure.Context;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Repositories;
using Pulse.Authorization.Tests.Helpers;

namespace Pulse.Authorization.Infrastructure.Tests.Repositories;

public class AccountRepositoryTests
{
    private readonly DbContextOptions<AuthorizationContext> _options;
    private readonly Fixture _fixture;

    public AccountRepositoryTests()
    {
        _options = new DbContextOptionsBuilder<AuthorizationContext>()
                            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                            .Options;
        _fixture = EntityFixtureFactory.Create();
    }

    [Fact]
    public async Task GetAccountByIdAsync_Return_Account()
    {
        using (var context = new AuthorizationContext(_options))
        {
            var accountEntity = _fixture.Create<AccountEntity>();

            context.AccountEntity.Add(accountEntity);
            await context.SaveChangesAsync();

            var expectedAccount = accountEntity;

            var repository = new AccountRepository(context);

            var receivedAccount = await repository.GetAccountByIdAsync(accountEntity.AccountId);

            Assert.Equivalent(expectedAccount.AccountId, receivedAccount.AccountId);
        }
    }

    [Fact]
    public async Task GetAccountByIdAsync_WhenUnknownAccountId_Returns_null()
    {
        using (var context = new AuthorizationContext(_options))
        {
            var accountEntity = _fixture.Build<AccountEntity>().With(a => a.AccountId, 12).Create();

            context.AccountEntity.Add(accountEntity);
            await context.SaveChangesAsync();

            var repository = new AccountRepository(context);

            var receivedAccount = await repository.GetAccountByIdAsync(1);

            Assert.Null(receivedAccount);
        }
    }
}
