// <copyright file="HistoryEventPublisher.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Text;
using Pulse.Account.Core.Enum;
using Pulse.Authorization.Core.Exceptions;
using Pulse.Authorization.Core.Interfaces;
using Pulse.Authorization.Infrastructure.Enum;
using Pulse.Authorization.Infrastructure.Interfaces;
using Pulse.Authorization.Infrastructure.Providers.Interfaces;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;
using Pulse.Back.Events.IntegrationEvents.EventsData;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Authorization.Infrastructure.Providers;

public class HistoryEventPublisher : IHistoryEventPublisher
{
    private readonly IContactRepository _contactRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IConfigurationRepository _configurationRepository;
    private readonly IEventPublisher _eventPublisher;

    public HistoryEventPublisher(IContactRepository contactRepository, IAccountRepository accountRepository, IConfigurationRepository configurationRepository, IEventPublisher eventPublisher)
    {
        _contactRepository = contactRepository;
        _accountRepository = accountRepository;
        _configurationRepository = configurationRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task PublishHistoryCreatedEvent(int currentUserId, int contactId, int accountId, IEnumerable<string> addedPermissionCodes, IEnumerable<string> deletedPermissionCodes)
    {
        var addedPermissions = await _configurationRepository.GetAuthorizationEntitiesByCodeAsync(addedPermissionCodes);
        var deletedPermissions = await _configurationRepository.GetAuthorizationEntitiesByCodeAsync(deletedPermissionCodes);

        if (!addedPermissions.Any() && !deletedPermissions.Any())
        {
            return;
        }

        var currentUser = await _contactRepository.GetContactByIdAsync(currentUserId)
            ?? throw new NotFoundException(Errors.NotFoundContactCode, string.Format(Errors.NotFoundContactMessage, currentUserId));
        var currentUserName = currentUser.FirstName + " " + currentUser.LastName;

        var contact = await _contactRepository.GetContactByIdAsync(contactId)
            ?? throw new NotFoundException(Errors.NotFoundContactCode, string.Format(Errors.NotFoundContactMessage, contactId));
        var contactName = contact.FirstName + " " + contact.LastName;

        var account = await _accountRepository.GetAccountByIdAsync(accountId)
            ?? throw new NotFoundException(Errors.NotFoundAccountCode, string.Format(Errors.NotFoundAccountMessage, accountId));

        var details = new StringBuilder();

        foreach (var permission in addedPermissions)
        {
            details.AppendLine("- Le droit " + permission.Code + "-" + permission.Label + " a été ajouté");
        }

        foreach (var permission in deletedPermissions)
        {
            details.AppendLine("- Le droit " + permission.Code + "-" + permission.Label + " a été retiré");
        }

        var actionCode = ContactType.Collaborator.ToString().Equals(contact.Type)
            ? ActionCode.MAJPERMK.ToString()
            : ActionCode.MAJPERMC.ToString();

        var data = new HistoryCreatedEventData
        {
            CreationDate = DateTime.UtcNow,
            Account = new AccountHistoryEventData
            {
                AccountId = accountId,
                AccountNumber = account.AccountNumber,
                LegalName = account.LegalName,
            },
            Action = new ActionHistoryEventData
            {
                Code = actionCode,
            },
            User = new UserHistoryEventData
            {
                Email = currentUser.Email,
                Name = currentUserName,
                UserType = currentUser.Type,
            },
            TargetUser = new UserHistoryEventData
            {
                Email = contact.Email,
                Name = contactName,
                UserType = contact.Type,
            },
            Details = details.ToString()
        };

        await _eventPublisher.PublishAsync(new HistoryCreatedEvent(data));
    }
}
