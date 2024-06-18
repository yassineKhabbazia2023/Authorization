// <copyright file="GlobalConstants.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Authorization.Infrastructure.Constants
{
    public static class GlobalConstants
    {
        public static readonly int RetryTimespan = 3000;
        public static readonly string DisabledDelegationStatus = "disabled";

        public static readonly int DefaultAccountIdCollab = -1;
        public static readonly string CustomerCategory = "customer";
        public static readonly string CollabCategory = "collaborator";

        public static readonly string[] DefaultAccountPermissions =
        [
            "CLADMI001", "CLUSER001", "CLUSER002", "CLUSER003",
            "CLUSER004", "CLOFF001", "CLINFO001", "COUSER001",
            "COUSER002", "COOFF003", "COINFO001"
        ];

        public static readonly string[] DefaultSignatoryPermissions =
        [
            "CLADMI001", "CLUSER001", "CLUSER002", "CLUSER003",
            "CLUSER004", "CLOFF001", "CLINFO001",
        ];
    }
}
