// <copyright file="AccountEventRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Pulse.Authorization.Core.Enum;
using Pulse.Authorization.Infrastructure.Context;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Providers;
using Pulse.Authorization.Infrastructure.Repositories;

namespace Pulse.Authorization.Infrastructure.Tests.Repositories;

public class AccountEventRepositoryTests
{
    private readonly AccountEntity _accountEntity = new AccountEntity
    {
        AccountId = 1,
        AccountGlobalUniqueId = Guid.NewGuid(),
        AccountNumber = "4242424242",
        LegalName = "Jooooohnnnnyyyy Piza",
        Status = "Happy",
    };

    [Fact]
    public async Task CreateAccountAsync_WithAccountData_ShouldCreateAccount()
    {
        var options = new DbContextOptionsBuilder<AuthorizationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using var context = new AuthorizationContext(options);
        var repository = new AccountEventRepository(context);

        // Act
        await repository.CreateAccountAsync(_accountEntity);
        var addedAccount = await context.AccountEntity.FirstOrDefaultAsync();

        // Assert
        Assert.NotNull(addedAccount);
        Assert.Equivalent(addedAccount, _accountEntity);
    }

    [Fact]
    public async Task UpdateAccountAsync_WithAccountData_ShouldCreateAccount()
    {
        var options = new DbContextOptionsBuilder<AuthorizationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using var context = new AuthorizationContext(options);
        var repository = new AccountEventRepository(context);

        await context.AccountEntity.AddAsync(_accountEntity);
        await context.SaveChangesAsync();

        var modifiedAccountEntity = new AccountEntity
        {
            AccountId = 1,
            AccountGlobalUniqueId = _accountEntity.AccountGlobalUniqueId,
            AccountNumber = "4242424242",
            LegalName = "Jooooohnnnnyyyy Pasta",
            Status = "Sad",
        };

        // Act
        await repository.UpdateAccountAsync(modifiedAccountEntity);
        var updatedAccount = await context.AccountEntity.FirstOrDefaultAsync();

        // Assert
        Assert.NotNull(updatedAccount);
        Assert.Equivalent(modifiedAccountEntity, updatedAccount);
    }

    [Fact]
    public async Task RemoveAccountAsync_WithAccountData_ShouldRemoveAccount()
    {
        var options = new DbContextOptionsBuilder<AuthorizationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using var context = new AuthorizationContext(options);
        var repository = new AccountEventRepository(context);

        await context.AccountEntity.AddAsync(_accountEntity);
        await context.SaveChangesAsync();

        // Act
        await repository.RemoveAccountAsync(accountId: 1);
        var removedAccount = await context.AccountEntity.FirstOrDefaultAsync();

        // Assert
        Assert.NotNull(removedAccount);
        Assert.Equal(AccountStatus.Revoked.ToString(), removedAccount.Status);
    }

    [Fact]
    public async Task RemoveAccountAuthorizationsAsync_WithAccountData_ShouldRemoveAccountAuthorizations()
    {
        var options = new DbContextOptionsBuilder<AuthorizationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using var context = new AuthorizationContext(options);
        var repository = new AccountEventRepository(context);
        var autAccount = new AccountAuthorizationEntity
        {
            AccountId = _accountEntity.AccountId,
        };
        var autContact = new ContactAuthorizationEntity
        {
            AccountId = _accountEntity.AccountId,
            ContactId = 1,
        };

        await context.AccountEntity.AddAsync(_accountEntity);
        await context.SaveChangesAsync();

        // Act
        await repository.RemoveAccountAuthorizationsAsync(accountId: 1);
        var foundAutContact = await context.ContactAuthorizationEntity.FirstOrDefaultAsync(c => c.AccountId == _accountEntity.AccountId);
        var foundAutAccount = await context.AccountAuthorizationEntity.FirstOrDefaultAsync(c => c.AccountId == _accountEntity.AccountId);

        // Assert
        Assert.Null(foundAutContact);
        Assert.Null(foundAutAccount);
    }
}
