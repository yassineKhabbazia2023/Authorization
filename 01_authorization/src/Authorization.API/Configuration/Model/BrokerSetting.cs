// <copyright file="BrokerSetting.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace Pulse.Authorization.API.Configuration.Models;

[ExcludeFromCodeCoverage]
public class BrokerSetting
{
    public string? ServiceBusNamespace { get; set; }

    public string? ManagedIdentityClientId { get; set; }

    public string? PushTopicName { get; set; }

    public List<PullTopic>? PullTopics { get; set; }
}
