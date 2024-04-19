// <copyright file="MapAuthorizationEntityToModel.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Core.Models;
using Action = Pulse.Authorization.Core.Models.Action;

namespace Pulse.Authorization.Infrastructure.Mappers
{
    public static class MapAuthorizationEntityToModel
    {

        public static IEnumerable<Resource> MapToResources(this IEnumerable<ResourceEntity> source)
        {
            return source?.Select(a => a.MapToResource() !) ?? Enumerable.Empty<Resource>();
        }

        public static Resource? MapToResource(this ResourceEntity source)
        {
            if (source == null)
            {
                return null;
            }

            return new Resource
            {
                ResourceId = source.ResourceId,
                ParentResourceId = source.ParentId,
                Name = source.Name,
                Label = source.Label,
                Category = source.Category,
                Childrens = source.InverseParent.Select(c => c.MapToResource()),
                Actions = source.ActionEntity.Select(a => a.MapToAction()),
            };
        }

        public static Action? MapToAction(this ActionEntity source)
        {
            if (source == null)
            {
                return null;
            }

            return new Action
            {
                ActionId = source.ActionId,
                Code = source.Code,
                Name = source.Name,
            };
        }
    }
}
