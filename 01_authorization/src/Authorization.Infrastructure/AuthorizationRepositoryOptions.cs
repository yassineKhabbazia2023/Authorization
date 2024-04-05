// <copyright file="AccountRepositoryOptions.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Authorization.Infrastructure
{
    public class AuthorizationRepositoryOptions
    {
        public string? ConnectionString { get; set; }

        internal void Validate()
        {
            if (string.IsNullOrWhiteSpace(this.ConnectionString!))
            {
                throw new InvalidOperationException($"Instance of {nameof(AuthorizationRepositoryOptions)} is invalid, {nameof(AuthorizationRepositoryOptions.ConnectionString)} is null or empty.");
            }
        }
    }
}
