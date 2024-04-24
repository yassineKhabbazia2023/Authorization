// <copyright file="AuthorizationService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Kpmg.ExceptionMiddleware.AdvancedExceptions;
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

    public async Task<List<string>> GetContactAuthorization(int contactId, int? accountId)
    {
        var contact = await _contactRepository.GetContactByIdAsync(contactId);

        if (contact == null)
        {
            throw new NotFoundException(Errors.NotFoundContactCode, string.Format(Errors.NotFoundContactMessage, contactId));
        }

        List<string> contactAuthorization = new ();

        switch (contact!.Type)
        {
            case Constants.Constants.ContactTypeCustomer:
                contactAuthorization = await GetCustomerAuthorization(contactId, accountId);
                break;
            case Constants.Constants.ContactTypeCollab:
                contactAuthorization = await GetCollabAuthorization(contactId, accountId);
                break;
            default:
                throw new NotFoundException(Errors.NotFoundContactTypeCode, string.Format(Errors.NotFoundContactTypeMessage, contactId, contact!.Type));
        }

        return contactAuthorization;
    }

    public async Task<List<string>> GetCustomerAuthorization(int contactId, int? accountId)
    {
        var contactAuthorization = await _authorizationRepository.GetContactAuthorizations(contactId, accountId);

        return contactAuthorization;
    }

    public async Task<List<string>> GetCollabAuthorization(int contactId, int? accountId)
    {
        var contactAuthorization = await _authorizationRepository.GetContactAuthorizations(contactId, accountIdTemp);

        var collabAuthorization = await _authorizationRepository.GetContactAuthorizations(contactId, accountId);

        return contactAuthorization.Intersect(collabAuthorization).ToList();
    }
}
