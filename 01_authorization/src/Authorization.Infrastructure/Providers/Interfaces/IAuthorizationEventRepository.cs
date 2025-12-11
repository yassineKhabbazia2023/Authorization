// <copyright file="IAuthorizationEventRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Authorization.Infrastructure.Providers.Interfaces;

public interface IAuthorizationEventRepository
{
    Task<bool> IsPennylaneActivatedAsync(int accountId);
}
