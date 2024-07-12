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
            AccountGlobalUniqueId = new Guid(),
            AccountNumber = "1234CBD",
            LegalName = "illegal",
            Status = "ToDeploy"
        };

        var result = source.ToAccountEntity();

        Assert.NotNull(result);
        Assert.Equal(source.AccountId, result.AccountId);
        Assert.Equal(source.AccountGlobalUniqueId, result.AccountGlobalUniqueId);
        Assert.Equal(source.AccountNumber, result.AccountNumber);
        Assert.Equal(source.LegalName, result.LegalName);
        Assert.Equal(source.Status, result.Status);
    }

    [Fact]
    public void ToAccountEntity_ShouldMapAccountEntityToAccountEntity()
    {
        var accountCible = new AccountEntity
        {
            AccountNumber = "AUN029UD",
            LegalName = "pas legal",
            Status = "ToDeploy"
        };

        var existingAccount = new AccountEntity
        {
            AccountNumber = "AAFGGGG",
            LegalName = "moyen legal",
            Status = "Connected"
        };

        accountCible.ToAccountEntity(existingAccount);

        Assert.Equal(accountCible.AccountNumber, existingAccount.AccountNumber);
        Assert.Equal(accountCible.LegalName, existingAccount.LegalName);
        Assert.Equal(accountCible.Status, existingAccount.Status);
        Assert.NotNull(existingAccount.LastUpdateDate);
    }
}
