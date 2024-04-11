// <copyright file="MapAuthorizationEntityToModel.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>
using Pulse.Authorization.Core.Models;
using Action = Pulse.Authorization.Core.Models.Action;
using Entities = Authorization.Infrastructure.Entities;
namespace Pulse.Account.Infrastructure.Mappers
{
    public static class MapAuthorizationEntityToModel
    {

        public static IEnumerable<Resource> MapToResources(this ICollection<Entities.AccountEntity> source, int accountId)
        {
            return source?.Select(a => a.MapToResource(accountId) !) ?? Enumerable.Empty<Resource>();
        }

        public static Resource? MapToResource(this Entities.AccountEntity source, int accountId)
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
