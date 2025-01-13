// <copyright file="PullTopic.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace Pulse.Authorization.API.Configuration.Models;

[ExcludeFromCodeCoverage]
public class PullTopic
{
    public string? TopicName { get; set; }

    public List<string>? Subscriptions { get; set; }
}
