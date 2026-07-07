// <copyright file="MapToAccountEntityTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Mappers.EventMappers;
using Pulse.Back.Events.IntegrationEvents.EventsData;

namespace Pulse.Authorization.Infrastructure.Tests.Mappers.Events;
public class MapToAccountEntityTests
{
    [Fact]
    public void ToAccountEntity_ShouldMapAccountStateEventDataToAccountEntity()
    {
        var source = new AccountStateEventData
        {
            AccountId = 1,
            AccountGlobalUniqueId = Guid.NewGuid(),
            AccountNumber = "1234CBD",
            LegalName = "illegal",
            IsActive = true,
            Status = "ToDeploy",
            AccountType = "Client"
        };

        var result = source.ToAccountEntity();

        Assert.NotNull(result);
        Assert.Equal(source.AccountId, result.AccountId);
        Assert.Equal(source.AccountGlobalUniqueId, result.AccountGlobalUniqueId);
        Assert.Equal(source.AccountNumber, result.AccountNumber);
        Assert.Equal(source.LegalName, result.LegalName);
        Assert.Equal(source.Status, result.Status);
        Assert.Equal(source.AccountType, result.AccountType);
        Assert.Equal(source.IsActive, result.IsActive);
    }

    [Fact]
    public void ToAccountEntity_WithNullSource_ShouldReturnNull()
    {
        var result = MapToAccountEntity.ToAccountEntity(null!);

        Assert.Null(result);
    }

    [Fact]
    public void ToAccountEntity_ShouldMapAccountEntityToAccountEntity()
    {
        var accountCible = new AccountEntity
        {
            AccountNumber = "AUN029UD",
            LegalName = "pas legal",
            IsActive = true,
            Status = "ToDeploy",
            AccountType = "Partner"
        };

        var existingAccount = new AccountEntity
        {
            AccountNumber = "AAFGGGG",
            LegalName = "moyen legal",
            IsActive = true,
            Status = "Connected",
            AccountType = "Client"
        };

        accountCible.ToAccountEntity(existingAccount);

        Assert.Equal(accountCible.AccountNumber, existingAccount.AccountNumber);
        Assert.Equal(accountCible.LegalName, existingAccount.LegalName);
        Assert.Equal(accountCible.Status, existingAccount.Status);
        Assert.Equal(accountCible.AccountType, existingAccount.AccountType);
        Assert.Equal(accountCible.IsActive, existingAccount.IsActive);
        Assert.NotNull(existingAccount.LastUpdateDate);
    }

    [Fact]
    public void ToAccountEntity_WithSource_ShouldReturn()
    {
        var account = new AccountEntity
        {
            AccountNumber = "AUN029UD",
            LegalName = "pas legal",
            IsActive = true,
            Status = "ToDeploy",
            AccountType = "Client"
        };

        account.ToAccountEntity(null!);

        Assert.Equal("AUN029UD", account.AccountNumber);
        Assert.Equal("pas legal", account.LegalName);
        Assert.Equal("ToDeploy", account.Status);
        Assert.Equal("Client", account.AccountType);
        Assert.True(account.IsActive);
    }
}
