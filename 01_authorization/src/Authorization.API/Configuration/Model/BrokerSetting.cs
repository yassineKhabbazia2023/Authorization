// <copyright file="BrokerSetting.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Authorization.API.Configuration.Models
{
    public class BrokerSetting
    {
        public string? ServiceBusNamespace { get; set; }

        public string? ManagedIdentityClientId { get; set; }

        public string? PushTopicName { get; set; }

        public List<PullTopic>? PullTopics { get; set; }
    }
}
