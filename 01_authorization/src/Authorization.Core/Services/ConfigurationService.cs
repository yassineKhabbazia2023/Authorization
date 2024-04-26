// <copyright file="ConfigurationService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Pulse.Authorization.Core.Exceptions;
using Pulse.Authorization.Core.Interfaces;
using Pulse.Authorization.Core.Models;

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

    public async Task<IEnumerable<Configuration>> GetContactConfigurationAsync(int contactId, int accountId)
    {
        var contact = await _contactRepository.GetContactByIdAsync(contactId);

        if (contact == null)
        {
            throw new NotFoundException(Errors.NotFoundContactCode, string.Format(Errors.NotFoundContactMessage, contactId));
        }

        return await _configurationRepository.GetContactConfigurationAsync(contactId, accountId);
    }
}
