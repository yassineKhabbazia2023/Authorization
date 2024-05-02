// <copyright file="ConfigurationService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Microsoft.Extensions.Configuration;
using Pulse.Authorization.Core.Constants;
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

    public async Task<IEnumerable<Configuration>> GetContactAccountConfigurationAsync(int contactId, int? accountId)
    {
        var contact = await _contactRepository.GetContactByIdAsync(contactId);

        if (contact == null)
        {
            throw new NotFoundException(Errors.NotFoundContactCode, string.Format(Errors.NotFoundContactMessage, contactId));
        }

        IEnumerable<Configuration> configurations;

        if (contact!.Type == ContactType.Customer.ToString())
        {
            configurations = await _configurationRepository.GetAccountConfigurationAsync(accountId!.Value);
        }
        else if (contact!.Type == ContactType.Collaborator.ToString())
        {
            configurations = await _configurationRepository.GetAccountConfigurationAsync(GlobalConstants.DefaultAccountIdCollab);
        }
        else
        {
            throw new NotFoundException(Errors.NotFoundContactTypeCode, string.Format(Errors.NotFoundContactTypeMessage, contactId, contact!.Type));
        }

        var contactAuthorization = await _configurationRepository.GetContactConfigurationAsync(contactId);

        EnableContactConfiguration(configurations, contactAuthorization);

        return configurations;
    }

    private static void EnableContactConfiguration(IEnumerable<Configuration> configurations, IEnumerable<Configuration> contactAuthorization)
    {
        if (contactAuthorization?.Any() == true)
        {
            foreach (var accountConf in configurations)
            {
                var contactConfig = contactAuthorization.FirstOrDefault(x => x.Category.Equals(accountConf.Category));

                if (contactConfig?.Actions?.Any() == true)
                {
                    foreach (var action in accountConf.Actions)
                    {
                        action.Enabled = contactConfig.Actions.Any(x => x.ActionId == action.ActionId);
                    }
                }
            }
        }
    }

    public async Task UpdateContactAccountAuthorizationAsync(int contactId, int? accountId, IEnumerable<string> codes)
    {
        var contact = await _contactRepository.GetContactByIdAsync(contactId);

        if (contact == null)
        {
            throw new NotFoundException(Errors.NotFoundContactCode, string.Format(Errors.NotFoundContactMessage, contactId));
        }

        accountId = accountId ?? (contact.Type!.Equals(ContactType.Collaborator.ToString()) ? -1 : throw new NotFoundException(Errors.NotFoundAccountCode, string.Format(Errors.NotFoundAccountMessage, accountId)));

        await _configurationRepository.UpdateContactAccountAuthorizationAsync(contactId, accountId.Value, codes);
    }
}
