// <copyright file="AuthorizationService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Pulse.Authorization.Core.Constants;
using Pulse.Authorization.Core.Exceptions;
using Pulse.Authorization.Core.Interfaces;
using Pulse.Authorization.Core.Models;

namespace Pulse.Authorization.Core.Services;

public class AuthorizationService : IAuthorizationService
{
    private readonly IAuthorizationRepository _authorizationRepository;
    private readonly IContactRepository _contactRepository;

    public AuthorizationService(IAuthorizationRepository authorizationRepository, IContactRepository contactRepository)
    {
        _authorizationRepository = authorizationRepository;
        _contactRepository = contactRepository;
    }

    public async Task<List<string>> GetContactAuthorizationAsync(int contactId, int? accountId)
    {
        var contact = await _contactRepository.GetContactByIdAsync(contactId);

        if (contact == null)
        {
            throw new NotFoundException(Errors.NotFoundContactCode, string.Format(Errors.NotFoundContactMessage, contactId));
        }

        if (contact!.Type == ContactType.Customer.ToString())
        {
            return await GetCustomerAuthorizationAsync(contactId, accountId);
        }

        if (contact!.Type == ContactType.Collaborator.ToString())
        {
            return await GetCollabAuthorizationAsync(contactId, accountId);
        }

        throw new NotFoundException(Errors.NotFoundContactTypeCode, string.Format(Errors.NotFoundContactTypeMessage, contactId, contact!.Type));
    }

    private async Task<List<string>> GetCustomerAuthorizationAsync(int contactId, int? accountId)
    {
        if (accountId == null)
        {
            return await _authorizationRepository.GetContactAuthorizationAsync(contactId);
        }

        return await _authorizationRepository.GetContactAccountAuthorizationsAsync(contactId, accountId.Value);
    }

    private async Task<List<string>> GetCollabAuthorizationAsync(int contactId, int? accountId)
    {
        var collabAuthorization = await _authorizationRepository.GetContactAccountAuthorizationsAsync(contactId, GlobalConstants.DefaultAccountIdCollab);

        if (accountId == null)
        {
            return collabAuthorization;
        }
        else
        {
            collabAuthorization.AddRange(await _authorizationRepository.GetContactAccountAuthorizationsAsync(contactId, accountId.Value));
        }

        var accountAuthorization = await _authorizationRepository.GetAccountAuthorizationAsync(accountId.Value);

        return collabAuthorization.Intersect(accountAuthorization).ToList();
    }

    public async Task DeleteContactAuthorizationAsync(int contactId, int? accountId)
    {
        var contact = await _contactRepository.GetContactByIdAsync(contactId);

        if (contact == null)
        {
            throw new NotFoundException(Errors.NotFoundContactCode, string.Format(Errors.NotFoundContactMessage, contactId));
        }

        if (contact.Type == ContactType.Customer.ToString() && accountId == null)
        {
            throw new NotFoundException(Errors.NotFoundAccountCode, string.Format(Errors.NotFoundAccountMessage, accountId));
        }

        await _authorizationRepository.DeleteContactAuthorizationAsync(contactId, accountId ?? -1);
    }
}
