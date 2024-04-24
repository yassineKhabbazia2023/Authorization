// <copyright file="IConfigurationRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Authorization.Core.Models;

namespace Pulse.Authorization.Core.Interfaces;

public interface IConfigurationRepository
{
    Task<IEnumerable<Configuration>> GetContactAuthorizations(int contactId, int? accountId);
}
