// <copyright file="AccountEventRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Pulse.Authorization.Infrastructure.Context;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Enum;
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
        IsActive = true,
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
            IsActive = true,
        };

        // Act
        await repository.UpdateAccountAsync(modifiedAccountEntity);
        var updatedAccount = await context.AccountEntity.FirstOrDefaultAsync();

        // Assert
        Assert.NotNull(updatedAccount);
        Assert.Equal(modifiedAccountEntity.AccountId, updatedAccount.AccountId);
        Assert.Equal(modifiedAccountEntity.AccountGlobalUniqueId, updatedAccount.AccountGlobalUniqueId);
        Assert.Equal(modifiedAccountEntity.LegalName, updatedAccount.LegalName);
        Assert.Equal(modifiedAccountEntity.AccountNumber, updatedAccount.AccountNumber);
        Assert.Equal(modifiedAccountEntity.Status, updatedAccount.Status);
        Assert.Equal(modifiedAccountEntity.IsActive, updatedAccount.IsActive);
        Assert.NotNull(updatedAccount.LastUpdateDate);
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
        Assert.False(removedAccount.IsActive);
        Assert.NotNull(removedAccount.LastUpdateDate);
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

    [Fact]
    public async Task DoesAccountExistAsync_WithExistingAccount_ShouldReturnTrue()
    {
        var options = new DbContextOptionsBuilder<AuthorizationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using var context = new AuthorizationContext(options);

        var account = new AccountEntity
        {
            AccountId = 1,
            AccountGlobalUniqueId = Guid.NewGuid(),
            AccountNumber = "number",
            LegalName = "legal",
            Status = "Invited",
            IsActive = true,
            CreationDate = DateTime.UtcNow,
        };
        context.AccountEntity.Add(account);
        await context.SaveChangesAsync();

        var repository = new AccountEventRepository(context);

        var result = await repository.DoesAccountExistAsync(1);

        Assert.True(result);
    }

    [Fact]
    public async Task DoesAccountExistAsync_WithNoExistingAccount_ShouldReturnFalse()
    {
        var options = new DbContextOptionsBuilder<AuthorizationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using var context = new AuthorizationContext(options);

        var repository = new AccountEventRepository(context);

        var result = await repository.DoesAccountExistAsync(1);

        Assert.False(result);
    }
}
