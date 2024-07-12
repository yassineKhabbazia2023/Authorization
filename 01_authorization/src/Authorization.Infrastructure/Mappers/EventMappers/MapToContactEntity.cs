// <copyright file="MapToContactEntity.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Back.Events.IntegrationEvents.EventsData;
using Pulse.Authorization.Infrastructure.Entities;

namespace Pulse.Authorization.Infrastructure.Mappers.EventMappers
{
    public static class MapToContactEntity
    {
        public static ContactEntity ToContactEntity(this ContactStateEventData source)
        {
            if (source == null)
            {
                return null!;
            }

            return new ContactEntity
            {
                ContactId = source.ContactId,
                ContactGlobalUniqueId = source.ContactGlobalUniqueId,
                FirstName = source.FirstName,
                LastName = source.LastName,
                Email = source.Email,
                Status = source.Status,
                Type = source.Type,
                PersonaName = source.PersonaName,
            };
        }

        public static void ToContactEntity(this ContactEntity source, ContactEntity destination)
        {
            if (source == null || destination == null)
            {
                return;
            }

            destination.FirstName = source.FirstName;
            destination.LastName = source.LastName;
            destination.Email = source.Email;
            destination.Status = source.Status;
            destination.Type = source.Type;
            destination.PersonaName = source.PersonaName;
            destination.LastUpdateDate = DateTime.UtcNow;
        }
    }
}
