// <copyright file="GlobalConstants.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Authorization.Core.Constants
{
    public static class GlobalConstants
    {
        public static readonly int RetryTimespan = 3000;
        public static readonly string DisabledDelegationStatus = "disabled";

        public static readonly int DefaultAccountIdCollab = -1;

        public static readonly string ContactTypeCollab = "Collaborator";
        public static readonly string ContactTypeCustomer = "Customer";
    }
}
