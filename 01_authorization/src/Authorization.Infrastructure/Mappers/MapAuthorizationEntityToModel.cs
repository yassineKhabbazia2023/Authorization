// <copyright file="MapAuthorizationEntityToModel.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Core.Models;
using Action = Pulse.Authorization.Core.Models.Action;
using Pulse.Authorization.Core.Requests;
using System.Reflection.Metadata;
using Pulse.Authorization.Core.Constants;

namespace Pulse.Authorization.Infrastructure.Mappers
{
    public static class MapAuthorizationEntityToModel
    {
        public static IEnumerable<Configuration> MapAuthorizationToConfiguration(this IEnumerable<AuthorizationEntity> sources)
        {
            return Enumerable.Empty<Configuration>();
        }
    }
}
