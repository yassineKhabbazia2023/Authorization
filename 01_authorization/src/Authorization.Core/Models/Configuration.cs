// <copyright file="Configuration.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Authorization.Core.Models;

public class Configuration
{
    public required string Category { get; set; }

    public required IEnumerable<Action> Actions { get; set; }
}
