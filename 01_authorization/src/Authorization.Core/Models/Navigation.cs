// <copyright file="Navigation.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Authorization.Core.Models;

public class Navigation
{
    public required string Name { get; set; }

    public required string Label { get; set; }

    public IEnumerable<Navigation>? Childrens { get; set; }
}
