// <copyright file="MapContactEntityToModel.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Core.Models;

namespace Pulse.Authorization.Infrastructure.Mappers;

public static class MapContactEntityToModel
{
    public static Contact MapToContact(this ContactEntity source)
    {
        if (source == null)
        {
            return null!;
        }

        return new Contact
        {
            ContactId = source.ContactId,
            ContactGlobalUniqueId = source.ContactGlobalUniqueId,
            CreationDate = source.CreationDate,
            Email = source.Email,
            FirstName = source.FirstName,
            LastName = source.LastName,
            Type = source.Type,
            Status = source.Status,
            PersonaName = source.PersonaName
        };
    }
}
