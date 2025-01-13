// <copyright file="IConfigurationRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Authorization.Infrastructure.Entities;

namespace Pulse.Authorization.Infrastructure.Interfaces;

public interface IConfigurationRepository
{
    Task<IEnumerable<AuthorizationEntity>> GetAvailableAuthorizationsAsync(string? type, bool configurable = true);

    Task<IEnumerable<AuthorizationEntity>> GetAccountAuthorizationsAsync(int accountId, string? type, bool configurable = true);

    Task<IEnumerable<AuthorizationEntity>> GetContactAuthorizationsAsync(int contactId, int accountId);

    Task CreateOrUpdateContactAccountAuthorizationAsync(int contactId, int accountId, IEnumerable<string> codes);

    Task CreateOrUpdateAccountAuthorizationAsync(int accountId, IEnumerable<string> codes, string? type, bool configurableCheck = true);
}
