// <copyright file="MapAuthorizationEntityToModel.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Core.Models;

namespace Pulse.Authorization.Infrastructure.Mappers;

public static class MapAuthorizationEntityToModel
{
    public static IEnumerable<Configuration> MapAuthorizationToConfiguration(this IEnumerable<AuthorizationEntity> sources)
    {
        return Enumerable.Empty<Configuration>();
    }
}
