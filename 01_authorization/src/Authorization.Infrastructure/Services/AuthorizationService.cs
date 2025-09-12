// <copyright file="AuthorizationService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Http;
using Pulse.Authorization.Core.Exceptions;
using Pulse.Authorization.Core.Extensions;
using Pulse.Authorization.Core.Interfaces;
using Pulse.Authorization.Core.Models;
using Pulse.Authorization.Core.Models.Utils;
using Pulse.Authorization.Core.Request;
using Pulse.Authorization.Infrastructure.Constants;
using Pulse.Authorization.Infrastructure.Enum;
using Pulse.Authorization.Infrastructure.Interfaces;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Authorization.Infrastructure.Services;

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
        return accountId == null
            ? await _authorizationRepository.GetContactAuthorizationAsync(contactId)
            : await _authorizationRepository.GetContactAccountAuthorizationsAsync(contactId, accountId.Value, false);
    }

    private async Task<List<string>> GetCollabAuthorizationAsync(int contactId, int? accountId)
    {
        accountId = accountId ?? GlobalConstants.DefaultAccountIdCollab;
        var viewGlobal = IsViewGlobal(accountId.Value);

        var collabAuthorization = await _authorizationRepository.GetContactAccountAuthorizationsAsync(contactId, GlobalConstants.DefaultAccountIdCollab, viewGlobal);

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
            throw new BadRequestException(Errors.NotFoundAccountCode, string.Format(Errors.NotFoundAccountMessage, accountId));
        }

        await _authorizationRepository.DeleteContactAuthorizationAsync(contactId, accountId ?? -1);
    }

    public async Task<List<string>> GetAllContactAuthorizationsAsync(int contactId)
        => await _authorizationRepository.GetAllContactAuthorizationsAsync(contactId);

    private static bool IsViewGlobal(int accountId)
    {
        return accountId == GlobalConstants.DefaultAccountIdCollab;
    }

    public async Task<Paging<Contact>> GetContactIdsByAuthorizationCodesAndAccountIdAsync(List<string> codes, int accountId, Pagination? pagination)
    {
        pagination = pagination ?? new Pagination();
        pagination.PageNumber = Paginator.GetValidPageNumber(pagination.PageNumber);
        pagination.PageSize = Paginator.GetValidPageSize(pagination.PageSize);
        return await _authorizationRepository.GetContactIdsByAuthorizationCodesAndAccountIdAsync(codes, accountId, pagination);
    }

    public async Task<List<Contact>> GetContactIdsByAccountIdSignatoryAsync(int accountId)
    {
        return await _authorizationRepository.GetContactIdsByAccountIdSignatoryAsync(accountId);
    }

    public async Task SetPermissionForContactEmailAsync(string permission, IFormFile file)
    {
        List<string> emails =[];
        using var reader = new StreamReader(file.OpenReadStream());

        var line = await reader.ReadLineAsync();
        while(line != null)
        {
            emails.Add(line);
            line = await reader.ReadLineAsync();
        }

        await _authorizationRepository.SetPermissionByContactEmailAsync(permission, emails);
    }
}
