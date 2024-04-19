// <copyright file="IAuthorizationRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Collections.Generic;
using Pulse.Authorization.Core.Models;

namespace Pulse.Authorization.Core.Interfaces
{
    public interface IAuthorizationRepository
    {
        Task<IEnumerable<Resource>> GetResourceByAccountIdAsync(int? accountId, Contact contact, string categoryName);
    }
}
