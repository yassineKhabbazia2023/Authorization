// <copyright file="IConfigurationService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Authorization.Core.Models;

namespace Pulse.Authorization.Core.Interfaces;

public interface IConfigurationService
{
    Task<IEnumerable<Configuration>> GetContactAccountConfigurationAsync(int contactId, int? accountId);

    Task<IEnumerable<Configuration>> GetAccountConfigurationAsync(int accountId, string? type, bool configurable = true);

    Task CreateOrUpdateContactAccountAuthorizationAsync(int currentUserId, int contactId, int? accountId, IEnumerable<string> codes);

    Task CreateOrUpdateAccountAuthorizationAsync(int accountId, IEnumerable<string> codes, string? type, bool configurable = true);
}
