// <copyright file="IAuthorizationService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Authorization.Core.Requests;

namespace Pulse.Authorization.Core.Interfaces
{
    public interface IAuthorizationService
    {
        public Task<NavigationRequest> GetNavigationsAsync(int? accountId, int contactId);
    }
}
