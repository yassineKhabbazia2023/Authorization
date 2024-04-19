// <copyright file="IAuthorizationRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Authorization.Core.Models;
using Pulse.Authorization.Core.Requests;

namespace Pulse.Authorization.Core.Interfaces
{
    public interface IAuthorizationRepository
    {
        Task<NavigationRequest> GetNavigationsAsync(int? accountId, Contact contact);
    }
}
