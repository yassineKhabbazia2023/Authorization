// <copyright file="ContactRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Authorization.Infrastructure.Context;
using Pulse.Authorization.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Pulse.Authorization.Infrastructure.Extensions;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Enum;

namespace Pulse.Authorization.Infrastructure.Repositories;

public class ContactRepository : IContactRepository
{
    private readonly AuthorizationContext _authorizationContext;

    public ContactRepository(AuthorizationContext authorizationContext)
    {
        _authorizationContext = authorizationContext;
        _authorizationContext.HandleEFCoreFailure();
    }

    public async Task<ContactEntity> GetContactByIdAsync(int contactId)
    {
        var contact = await _authorizationContext.ContactEntity.AsNoTracking()
                                                .FirstOrDefaultAsync(c => c.ContactId == contactId);

        return contact!;
    }

    public async Task<IEnumerable<int>> GetSignatoriesAsync(int accountId)
    {
        var signatoryRoles = await _authorizationContext.RoleEntity.AsNoTracking()
            .Include(r => r.Contact)
            .Where(r => r.AccountId == accountId && r.IsSignatory == true && r.Contact.Type == ContactType.Customer.ToString())
            .Select(r => r.Contact.ContactId)
            .ToListAsync();

        return signatoryRoles;
    }
}
