// <copyright file="CreateRoleRequest.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Requests
{
    public class CreateRoleRequest
    {
        public int AccountId { get; set; }

        public int ContactId { get; set; }

        public int RessourceId { get; set; }

        public int? ActionId { get; set; }
    }
}
