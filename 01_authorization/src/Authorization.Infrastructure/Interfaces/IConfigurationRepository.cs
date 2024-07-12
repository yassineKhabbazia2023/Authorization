// <copyright file="IConfigurationRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Authorization.Infrastructure.Entities;

namespace Pulse.Authorization.Infrastructure.Interfaces;

public interface IConfigurationRepository
{
    Task<IEnumerable<AuthorizationEntity>> GetAccountConfigurationAsync(int accountId, string type);

    Task<IEnumerable<AuthorizationEntity>> GetContactConfigurationAsync(int contactId, int accountId);

    Task CreateOrUpdateContactAccountAuthorizationAsync(int contactId, int accountId, IEnumerable<string> codes);
}
