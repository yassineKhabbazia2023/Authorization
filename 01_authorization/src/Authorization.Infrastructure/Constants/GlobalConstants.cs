// <copyright file="GlobalConstants.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace Pulse.Authorization.Infrastructure.Constants
{
    [ExcludeFromCodeCoverage]
    public static class GlobalConstants
    {
        public static readonly int RetryTimespan = 3000;
        public static readonly string DisabledDelegationStatus = "disabled";

        public static readonly int DefaultAccountIdCollab = -1;
        public static readonly string CustomerCategory = "customer";
        public static readonly string CollabCategory = "collaborator";

        public static readonly string[] PowerBIDefaultPermissions = ["CORAPP001", "CLRAPP001", "CLRAPP002"];

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

        public static readonly ReadOnlyDictionary<string, string> CustomerToMirrorCodes = new(new Dictionary<string, string>{
            { "CLGED0001", "COGED0001" },
            { "CLGED0002", "COGED0002" },
            { "CLSILA001", "COLANC001" },
            { "CLEVP001", "COLANC001" },
            { "CLMEG001", "COLANC001" },
            { "CLPEN001", "COLANC001" },
            { "CLKPI0001", "COKPI0001" },
            { "CLBANK001", "COBANK001" },
        });
    }
}
