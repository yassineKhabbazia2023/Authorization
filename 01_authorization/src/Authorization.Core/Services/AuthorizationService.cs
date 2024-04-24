// <copyright file="AuthorizationService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Pulse.Authorization.Core.Exceptions;
using Pulse.Authorization.Core.Interfaces;
using Pulse.Authorization.Core.Models;
using Pulse.Authorization.Core.Constants;

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

    public async Task<List<string>> GetContactAuthorization(int contactId, int? accountId)
    {
        var contact = await _contactRepository.GetContactByIdAsync(contactId);

        if (contact == null)
        {
            throw new NotFoundException(Errors.NotFoundContactCode, string.Format(Errors.NotFoundContactMessage, contactId));
        }

        if (contact!.Type == GlobalConstants.ContactTypeCustomer)
        {
            return await GetCustomerAuthorization(contactId, accountId);
        }

        if (contact!.Type == GlobalConstants.ContactTypeCollab)
        {
            return await GetCollabAuthorization(contactId, accountId);
        }

        throw new NotFoundException(Errors.NotFoundContactTypeCode, string.Format(Errors.NotFoundContactTypeMessage, contactId, contact!.Type);
    }

    private async Task<List<string>> GetCustomerAuthorization(int contactId, int? accountId)
    {
        if (accountId == null)
        {
            return await _authorizationRepository.GetContactAuthorizations(contactId, GlobalConstants.DefaultAccountIdCustomer);
        }

        return await _authorizationRepository.GetContactAuthorizations(contactId, accountId);
    }

    private async Task<List<string>> GetCollabAuthorization(int contactId, int? accountId)
    {
        if (accountId == null)
        {
            return await _authorizationRepository.GetContactAuthorizations(contactId, GlobalConstants.DefaultAccountIdCollab);
        }

        return await _authorizationRepository.GetContactAuthorizations(contactId, accountId);
    }
}
