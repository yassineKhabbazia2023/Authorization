// <copyright file="EntityFixtureFactory.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;

namespace Pulse.Authorization.Tests.Helpers;

public static class EntityFixtureFactory
{
    public static Fixture Create()
    {
        var fixture = new Fixture();
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        fixture.Customizations.Add(new IgnoreEntityNavigationProperties());
        return fixture;
    }
}
