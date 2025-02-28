// <copyright file="MapToAccountEntity.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Back.Events.IntegrationEvents.EventsData;
using Pulse.Authorization.Infrastructure.Entities;

namespace Pulse.Authorization.Infrastructure.Mappers.EventMappers
{
    public static class MapToAccountEntity
    {
        public static AccountEntity ToAccountEntity(this AccountStateEventData source)
        {
            if (source == null)
            {
                return null!;
            }

            return new AccountEntity
            {
                AccountId = source.AccountId,
                AccountGlobalUniqueId = source.AccountGlobalUniqueId,
                AccountNumber = source.AccountNumber,
                LegalName = source.LegalName,
                Status = source.Status,
                IsActive = source.IsActive,
            };
        }

        public static void ToAccountEntity(this AccountEntity source, AccountEntity destination)
        {
            if (source == null || destination == null)
            {
                return;
            }

            destination.AccountNumber = source.AccountNumber;
            destination.LegalName = source.LegalName;
            destination.Status = source.Status;
            destination.IsActive = source.IsActive;
            destination.LastUpdateDate = DateTime.UtcNow;
        }
    }
}
