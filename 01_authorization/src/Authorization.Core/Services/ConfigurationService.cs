// <copyright file="ConfigurationService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Kpmg.ExceptionMiddleware.AdvancedException;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
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
            configurations = configurations.Where(c => c.Category.Contains(GlobalConstants.CustomerCategory, StringComparison.OrdinalIgnoreCase));
        }
        else if (contact!.Type == ContactType.Collaborator.ToString())
        {
            configurations = await _configurationRepository.GetAccountConfigurationAsync(GlobalConstants.DefaultAccountIdCollab);
            configurations = configurations.Where(c => c.Category.Contains(GlobalConstants.CollabCategory, StringComparison.OrdinalIgnoreCase));
        }
        else
        {
            throw new NotFoundException(Errors.NotFoundContactTypeCode, string.Format(Errors.NotFoundContactTypeMessage, contactId, contact!.Type));
        }

        var contactAuthorization = await _configurationRepository.GetContactConfigurationAsync(contactId);

        return EnableContactConfiguration(configurations, contactAuthorization);
    }

    private static List<Configuration> EnableContactConfiguration(IEnumerable<Configuration> configurations, IEnumerable<Configuration> contactAuthorization)
    {
        var tConfigurations = new List<Configuration>();
        foreach(var accountConf in configurations.ToArray())
        {
            var actions = accountConf.Actions.ToArray();
            foreach (var action in actions)
            {
                var cat = contactAuthorization.FirstOrDefault(a => a.Category == accountConf.Category);
                if (cat != null)
                {
                    action.Enabled = cat.Actions.Any(ac => ac.ActionId == action.ActionId);
                }
            }

            accountConf.Actions = actions;
            tConfigurations.Add(accountConf);
        }

        return tConfigurations;
    }

    public async Task CreateOrUpdateContactAccountAuthorizationAsync(int contactId, int? accountId, IEnumerable<string> codes)
    {
        var contact = await _contactRepository.GetContactByIdAsync(contactId);

        if (contact == null)
        {
            throw new NotFoundException(Errors.NotFoundContactCode, string.Format(Errors.NotFoundContactMessage, contactId));
        }

        accountId ??= contact.Type!.Equals(ContactType.Collaborator.ToString()) ? -1 :
            throw new BadRequestException(Errors.NotFoundAccountCode, Errors.NotFoundAccountMessage);

        await _configurationRepository.CreateOrUpdateContactAccountAuthorizationAsync(contactId, accountId.Value, codes);
    }
}
