// <copyright file="IAuthorizationService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Authorization.Core.Models;

namespace Pulse.Authorization.Core.Interfaces
{
    public interface IAuthorizationService
    {
        public Task<IEnumerable<Resource>> GetResourceByAccountIdAsync(int? accountId, int contactId, string categoryName);
    }
}
