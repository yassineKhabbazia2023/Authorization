// <copyright file="MapAuthorizationEntityToModel.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Core.Models;
using ActionModel = Pulse.Authorization.Core.Models.Action;

namespace Pulse.Authorization.Infrastructure.Mappers;

public static class MapAuthorizationEntityToModel
{
    // add test
    public static IEnumerable<Configuration> MapAuthorizationToConfiguration(this IEnumerable<AuthorizationEntity> sources)
    {
        return sources == null ? Enumerable.Empty<Configuration>() :
                       sources.GroupBy(auth => auth.Category)
                      .Select(auth => new Configuration
                      {
                          Category = auth.Key,
                          Actions = auth.Select(item => item.MapAuthorizationToAction())!
                      });
    }

    public static ActionModel? MapAuthorizationToAction(this AuthorizationEntity source)
    {
        return source == null ? null : new ActionModel
        {
            Name = source.Name,
            Label = source.Label,
            Code = source.Code,
        };
    }
}
