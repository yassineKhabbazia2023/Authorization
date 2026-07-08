// <copyright file="IgnoreEntityNavigationProperties.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Reflection;
using AutoFixture.Kernel;
using Pulse.Authorization.Infrastructure.Entities;

namespace Pulse.Authorization.Tests.Helpers;

/// <summary>
/// Omits EF Core navigation properties (declared virtual) when AutoFixture builds entities:
/// auto-generated entity graphs get random key values that can collide with keys set
/// explicitly by tests, causing non-deterministic EF Core tracking conflicts.
/// </summary>
public class IgnoreEntityNavigationProperties : ISpecimenBuilder
{
    private static readonly string EntitiesNamespace = typeof(AuthorizationEntity).Namespace;

    public object Create(object request, ISpecimenContext context)
    {
        if (request is PropertyInfo propertyInfo
            && propertyInfo.DeclaringType?.Namespace == EntitiesNamespace
            && propertyInfo.GetGetMethod() is { IsVirtual: true, IsFinal: false })
        {
            return new OmitSpecimen();
        }

        return new NoSpecimen();
    }
}
