// <copyright file="MapAuthorizationEntityToModel.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>
using Pulse.Account.Infrastructure.Entities;
using Pulse.Authorization.Core.Models;
using Action = Pulse.Authorization.Core.Models.Action;

namespace Pulse.Account.Infrastructure.Mappers
{
    public static class MapAuthorizationEntityToModel
    {

        public static IEnumerable<Resource> MapToResources(this ICollection<AccountEntity> source, int accountId)
        {
            return source?.Select(a => a.MapToResource(accountId) !) ?? Enumerable.Empty<Resource>();
        }

        public static Resource? MapToResource(this AccountEntity source, int accountId)
        {
            if (source == null)
            {
                return null;
            }

            return new Resource
            {
                ResourceId = 0,
                Actions = new List<Action> { },
                ResourceName = string.Empty,
            };
        }
    }
}
