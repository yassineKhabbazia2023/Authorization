// <copyright file="RoleEventRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Pulse.Authorization.Core.Exceptions;
using Pulse.Authorization.Core.Models.Subscriptions;
using Pulse.Authorization.Infrastructure.Context;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Extensions;
using Pulse.Authorization.Infrastructure.Mappers.EventMappers;
using Pulse.Authorization.Infrastructure.Providers.Interfaces;
using InvalidOperationException = Pulse.ExceptionMiddleware.Exceptions.InvalidOperationException;

namespace Pulse.Authorization.Infrastructure.Repositories;

public class RoleEventRepository : IRoleEventRepository
{
    private readonly AuthorizationContext _authorizationContext;

    public RoleEventRepository(AuthorizationContext authorziationContext)
    {
        _authorizationContext = authorziationContext;
        _authorizationContext.HandleEFCoreFailure();
    }

    public async Task CreateRoleAsync(RoleEntity roleEntity)
    {
        await _authorizationContext.RoleEntity.AddAsync(roleEntity);
        await _authorizationContext.SaveChangesAsync();
    }

    public async Task DeleteContactRolesAsync(int contactId)
    {
        await _authorizationContext.RoleEntity.Where(r => r.ContactId == contactId)
            .ForEachAsync(r =>
            {
                _authorizationContext.Entry(r).State = EntityState.Deleted;
            });

        await _authorizationContext.SaveChangesAsync();
    }

    public async Task DeleteRoleAsync(int contactId, int accountId)
    {
        var roleToDelete = _authorizationContext.RoleEntity.FirstOrDefault(x => x.ContactId == contactId && x.AccountId == accountId);
        if (roleToDelete != null)
        {
            _authorizationContext.Remove(roleToDelete);
        }

        await _authorizationContext.SaveChangesAsync();
    }

    public async Task UpdateRoleAsync(RoleEntity roleEntity)
    {
        var existingRole = await _authorizationContext.RoleEntity.SingleAsync(x => x.ContactId == roleEntity.ContactId && x.AccountId == roleEntity.AccountId);
        roleEntity.ToRoleEntity(existingRole);

        await _authorizationContext.SaveChangesAsync();
    }

    public async Task<bool> DoesRoleExistAsync(RoleEntity roleEntity)
    {
        var account = await _authorizationContext.AccountEntity.AsNoTracking()
            .FirstOrDefaultAsync(a => roleEntity.AccountId == a.AccountId);
        if (account == null)
        {
            throw new InvalidOperationException(Errors.NotFoundAccountCode, string.Format(Errors.NotFoundAccountMessage, roleEntity.AccountId));
        }

        var contact = await _authorizationContext.ContactEntity.AsNoTracking()
            .FirstOrDefaultAsync(c => roleEntity.ContactId == c.ContactId);
        if (contact == null)
        {
            throw new InvalidOperationException(Errors.NotFoundContactCode, string.Format(Errors.NotFoundContactMessage, roleEntity.ContactId));
        }

        var role = await _authorizationContext.RoleEntity
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.AccountId == roleEntity.AccountId && r.ContactId == roleEntity.ContactId);

        return role != null;
    }

    public ContactRolesSubscription RetrieveContactsHavingRole(IEnumerable<int> contactIds, int accountId)
    {
        var roleQuery = _authorizationContext.RoleEntity.AsNoTracking().Where(role => contactIds.Contains(role.ContactId) && role.AccountId == accountId);
        var existedContacts = roleQuery.Select(r => r.ContactId).Distinct().AsEnumerable();
        var unexistedContacts = contactIds.Except(existedContacts).AsEnumerable();

        return new ContactRolesSubscription(accountId)
        {
            ExistedContactRoles = existedContacts,
            UnexistedContactRoles = unexistedContacts
        };
    }

    public async Task<RoleEntity> GetRole(RoleEntity roleEntity)
    {
        return await _authorizationContext.RoleEntity.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ContactId == roleEntity.ContactId && x.AccountId == roleEntity.AccountId);
    }

    public async Task DeleteContactAuthorizations(int contactId, int accountId)
    {
        var authorizationstoDelete = await _authorizationContext.ContactAuthorizationEntity.Where(a => a.ContactId == contactId && a.AccountId == accountId).ToListAsync();
        _authorizationContext.ContactAuthorizationEntity.RemoveRange(authorizationstoDelete);

        await _authorizationContext.SaveChangesAsync();
    }
}
