// <copyright file="MapToRoleEntity.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Back.Events.IntegrationEvents.EventsData;

namespace Pulse.Authorization.Infrastructure.Mappers.EventMappers
{
    public static class MapToRoleEntity
    {
        public static RoleEntity ToRoleEntity(this RoleCreatedEventData source)
        {
            if (source == null)
            {
                return null!;
            }

            return new RoleEntity
            {
                ContactId = source.ContactId,
                AccountId = source.AccountId,
                IsDelegation = source.IsDelegation,
                IsFavorite = source.IsFavorite,
                IsSignatory = source.IsSignatory
            };
        }

        public static RoleEntity ToRoleEntity(this RoleUpdatedEventData source)
        {
            if (source == null)
            {
                return null!;
            }

            return new RoleEntity
            {
                ContactId = source.ContactId,
                AccountId = source.AccountId,
                IsSignatory = source.IsSignatory
            };
        }

        public static void ToRoleEntity(this RoleEntity source, RoleEntity destination)
        {
            if (source == null || destination == null)
            {
                return;
            }

            destination.ContactId = source.ContactId;
            destination.AccountId = source.AccountId;
            destination.IsSignatory = source.IsSignatory;
            destination.LastUpdateDate = DateTime.UtcNow;
        }
    }
}
