// <copyright file="ConfigurationService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Pulse.Authorization.Core.Exceptions;
using Pulse.Authorization.Core.Interfaces;
using Pulse.Authorization.Core.Models;
using Pulse.Authorization.Core.Requests;

namespace Pulse.Authorization.Core.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly IConfigurationRepository _configurationRepository;
    private readonly IContactRepository _contactRepository;

    public ConfigurationService(IConfigurationRepository configurationRepository, IContactRepository contactRepository)
    {
        _configurationRepository = configurationRepository;
        _contactRepository = contactRepository;
    }

    public async Task<IEnumerable<Configuration>> GetContactAuthorizations(int contactId, int? accountId)
    {
        return null;
    }
}
