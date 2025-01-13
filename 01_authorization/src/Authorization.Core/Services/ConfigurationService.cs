// <copyright file="ConfigurationService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Kpmg.ExceptionMiddleware.AdvancedException;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Pulse.Authorization.Core.Exceptions;
using Pulse.Authorization.Core.Interfaces;
using Pulse.Authorization.Core.Mappers;
using Pulse.Authorization.Core.Models;
using Pulse.Authorization.Infrastructure.Constants;
using Pulse.Authorization.Infrastructure.Enum;
using Pulse.Authorization.Infrastructure.Interfaces;
using Pulse.Authorization.Infrastructure.Providers.Interfaces;

namespace Pulse.Authorization.Core.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly IConfigurationRepository _configurationRepository;
    private readonly IContactRepository _contactRepository;
    private readonly IAuthorizationEventPublisher _authorizationEventPublisher;
    private readonly IAccountRepository _accountRepository;

    public ConfigurationService(IConfigurationRepository configurationRepository, IContactRepository contactRepository, IAuthorizationEventPublisher authorizationEventPublisher, IAccountRepository accountRepository)
    {
        _configurationRepository = configurationRepository;
        _contactRepository = contactRepository;
        _authorizationEventPublisher = authorizationEventPublisher;
        _accountRepository = accountRepository;
    }

    public async Task<IEnumerable<Configuration>> GetContactAccountConfigurationAsync(int contactId, int? accountId)
    {
        var contact = await _contactRepository.GetContactByIdAsync(contactId);

        if (contact == null)
        {
            throw new NotFoundException(Errors.NotFoundContactCode, string.Format(Errors.NotFoundContactMessage, contactId));
        }

        IEnumerable<Configuration> configurations;
        IEnumerable<Configuration> contactAuthorization;

        if (contact!.Type == ContactType.Customer.ToString())
        {
            configurations = (await _configurationRepository.GetAccountAuthorizationsAsync(accountId!.Value, GlobalConstants.CustomerCategory)).MapAuthorizationToConfiguration();
            contactAuthorization = (await _configurationRepository.GetContactAuthorizationsAsync(contactId, accountId!.Value)).MapAuthorizationToConfiguration();
        }
        else if (contact!.Type == ContactType.Collaborator.ToString())
        {
            configurations = (await _configurationRepository.GetAccountAuthorizationsAsync(GlobalConstants.DefaultAccountIdCollab, GlobalConstants.CollabCategory)).MapAuthorizationToConfiguration();
            contactAuthorization = (await _configurationRepository.GetContactAuthorizationsAsync(contactId, GlobalConstants.DefaultAccountIdCollab)).MapAuthorizationToConfiguration();
        }
        else
        {
            throw new NotFoundException(Errors.NotFoundContactTypeCode, string.Format(Errors.NotFoundContactTypeMessage, contactId, contact!.Type));
        }

        return EnableContactConfiguration(configurations, contactAuthorization);
    }

    private static List<Configuration> EnableContactConfiguration(IEnumerable<Configuration> configurations, IEnumerable<Configuration> contactAuthorization)
    {
        var tConfigurations = new List<Configuration>();
        foreach (var accountConf in configurations.ToArray())
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
        try
        {
            await _configurationRepository.CreateOrUpdateContactAccountAuthorizationAsync(contactId, accountId.Value, codes);
        }
        catch (ArgumentException ex)
        {
            throw new BadRequestException(Errors.NotConfigurablePermissionCode, string.Format(Errors.NotConfigurablePermissionMessage, ex.Data["Code"]));
        }

        await _authorizationEventPublisher.PublishAuthorizationUpdatedEventAsync(contactId, accountId.Value, codes.ToList());
    }

    public async Task<IEnumerable<Configuration>> GetAccountConfigurationAsync(int accountId, string? type, bool configurable = true)
    {
        var account = await _accountRepository.GetAccountByIdAsync(accountId);
        if(account == null)
        {
            throw new NotFoundException(Errors.NotFoundAccountCode, Errors.NotFoundAccountMessage);
        }

        var accAuths =  (await _configurationRepository.GetAccountAuthorizationsAsync(accountId, type, configurable)).MapAuthorizationToConfiguration();
        var availableAuths = (await _configurationRepository.GetAvailableAuthorizationsAsync(type, configurable)).MapAuthorizationToConfiguration();
        return EnableAccountConfiguration(availableAuths, accAuths);
    }

    private static List<Configuration> EnableAccountConfiguration(IEnumerable<Configuration> configurations, IEnumerable<Configuration> accountConfiguration)
    {
        var tConfigurations = new List<Configuration>();
        foreach (var conf in configurations.ToArray())
        {
            var actions = conf.Actions.ToArray();
            foreach (var action in actions)
            {
                var cat = accountConfiguration.FirstOrDefault(a => a.Category == conf.Category);
                if (cat != null)
                {
                    action.Enabled = cat.Actions.Any(ac => ac.ActionId == action.ActionId);
                }
            }

            conf.Actions = actions;
            tConfigurations.Add(conf);
        }

        return tConfigurations;
    }

    public async Task CreateOrUpdateAccountAuthorizationAsync(int accountId, IEnumerable<string> codes, string? type, bool configurable = true)
    {
        await _configurationRepository.CreateOrUpdateAccountAuthorizationAsync(accountId, codes, type, configurable);
        await _authorizationEventPublisher.PublishAuthorizationUpdatedEventAsync(null, accountId, codes.ToList());

    }
}
