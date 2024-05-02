// <copyright file="IConfigurationService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Authorization.Core.Models;

namespace Pulse.Authorization.Core.Interfaces;

public interface IConfigurationService
{
    Task<IEnumerable<Configuration>> GetContactAccountConfigurationAsync(int contactId, int? accountId);

    Task UpdateContactAccountAuthorizationAsync(int contactId, int? accountId, IEnumerable<string> codes);
}
