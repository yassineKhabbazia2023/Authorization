// <copyright file="ContactAuthorizationSubscription.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Authorization.Core.Models.Subscriptions
{
    public class ContactAuthorizationSubscription
    {
        public required IEnumerable<int> ContactIds { get; set; }

        public required int AccountId { get; set; }

        public required IEnumerable<int> AuthorizationIds { get; set; } = [];
    }
}
