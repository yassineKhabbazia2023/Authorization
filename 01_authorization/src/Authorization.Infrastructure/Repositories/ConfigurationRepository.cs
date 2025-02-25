// <copyright file="ConfigurationRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Authorization.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Extensions;
using Pulse.Authorization.Infrastructure.Interfaces;
using System.Configuration;
using Pulse.ExceptionMiddleware.Exceptions;
using Pulse.Authorization.Core.Exceptions;

namespace Pulse.Authorization.Infrastructure.Repositories;

public class ConfigurationRepository : IConfigurationRepository
{
    private readonly AuthorizationContext _authorizationContext;

    public ConfigurationRepository(AuthorizationContext authorizationContext)
    {
        _authorizationContext = authorizationContext;
        _authorizationContext.HandleEFCoreFailure();
    }

    public async Task<IEnumerable<AuthorizationEntity>> GetAccountAuthorizationsAsync(int accountId, string? type = null, bool configurable = true)
    {
        var authorization = (await GetAccountConfigurationAsync(accountId, type, configurable))
                     .Select(x => x.Authorization)
                     .Distinct();

        return authorization;
    }

    public async Task<IEnumerable<AuthorizationEntity>> GetContactAuthorizationsAsync(int contactId, int accountId)
    {
        var authorization = await _authorizationContext
                     .ContactAuthorizationEntity
                     .Include(x => x.Authorization)
                     .Where(x => x.ContactId == contactId
                            && x.AccountId == accountId
                            && x.Authorization.Configurable == true)
                     .Select(x => x.Authorization)
                     .Distinct()
                     .ToListAsync();

        return authorization;
    }

    private async Task<IEnumerable<AuthorizationEntity>> GetAuthorizationEntitiesByCodeAsync(IEnumerable<string> codes)
    {
        return await _authorizationContext
                     .AuthorizationEntity
                     .Where(x => codes.Contains(x.Code))
                     .Distinct()
                     .ToListAsync();
    }

    private async Task<IEnumerable<ContactAuthorizationEntity>> GetContactAccountConfigurationAsync(int contactId, int accountId)
    {
        return await _authorizationContext
                     .ContactAuthorizationEntity
                     .Include(x => x.Authorization)
                     .Where(x => x.ContactId == contactId && x.AccountId == accountId && x.Authorization.Configurable!.Value)
                     .Select(x => x)
                     .Distinct()
                     .ToListAsync();
    }

    private async Task<IEnumerable<AccountAuthorizationEntity>> GetAccountConfigurationAsync(int accountId, string? type = null, bool configurable = true)
    {
        var query = _authorizationContext
                     .AccountAuthorizationEntity
                     .Include(x => x.Authorization)
                     .Where(x => x.AccountId == accountId);

        if (configurable)
        {
            query = query.Where(x => x.Authorization.Configurable == true);
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            query = query.Where(x => x.Authorization.Type == type);
        }

        return await query.Distinct().ToListAsync();
    }

    private async Task DeleteContactAccountAuthorizationAsync(IEnumerable<ContactAuthorizationEntity> contactAuthorizations)
    {
        _authorizationContext.ContactAuthorizationEntity.RemoveRange(contactAuthorizations);
        await _authorizationContext.SaveChangesAsync();
    }

    private async Task DeleteAccountAuthorizationAsync(IEnumerable<AccountAuthorizationEntity> accountAuthorizations)
    {
        _authorizationContext.AccountAuthorizationEntity.RemoveRange(accountAuthorizations);
        await _authorizationContext.SaveChangesAsync();
    }

    public async Task CreateOrUpdateContactAccountAuthorizationAsync(int contactId, int accountId, IEnumerable<string> codes)
    {
        var authorizations = await GetAuthorizationEntitiesByCodeAsync(codes);

        var notConfigurable = authorizations.FirstOrDefault(a => a.Configurable == false);
        if (notConfigurable != null)
        {
            throw new BadRequestException(Errors.NotConfigurablePermissionCode, string.Format(Errors.NotConfigurablePermissionMessage, notConfigurable.Code));
        }

        var oldContactAuthorizationEntities = await GetContactAccountConfigurationAsync(contactId, accountId);

        if (oldContactAuthorizationEntities?.Any() == true)
        {
            await DeleteContactAccountAuthorizationAsync(oldContactAuthorizationEntities);
        }

        var authorizationIds = authorizations.Select(a => a.AuthorizationId);
        var newContactAuthorizationEntities = authorizationIds.Select(auth => new ContactAuthorizationEntity()
        {
            AccountId = accountId,
            ContactId = contactId,
            AuthorizationId = auth,
            CreationDate = DateTime.UtcNow
        });

        await _authorizationContext.ContactAuthorizationEntity.AddRangeAsync(newContactAuthorizationEntities);
        await _authorizationContext.SaveChangesAsync();
    }

    public async Task CreateOrUpdateAccountAuthorizationAsync(int accountId, IEnumerable<string> codes, string? type, bool configurableCheck = true)
    {
        var authorizations = await GetAuthorizationEntitiesByCodeAsync(codes);

        if (configurableCheck)
        {
            var notConfigurable = authorizations.FirstOrDefault(a => a.Configurable == false);
            if (notConfigurable != null)
            {
                throw new BadRequestException(Errors.NotConfigurablePermissionCode, string.Format(Errors.NotConfigurablePermissionMessage, notConfigurable.Code));
            }
        }

        var oldAccountAuthorizationEntities = await GetAccountConfigurationAsync(accountId, type, configurableCheck);

        if (oldAccountAuthorizationEntities?.Count() > 0)
        {
            await DeleteAccountAuthorizationAsync(oldAccountAuthorizationEntities);
        }

        var authorizationIds = authorizations.Select(a => a.AuthorizationId);
        var newAccountAuthorizations = authorizationIds.Select(auth => new AccountAuthorizationEntity()
        {
            AccountId = accountId,
            AuthorizationId = auth,
            Enabled = true,
        });

        await _authorizationContext.AccountAuthorizationEntity.AddRangeAsync(newAccountAuthorizations);
        await _authorizationContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<AuthorizationEntity>> GetAvailableAuthorizationsAsync(string? type, bool configurable = true)
    {
        var query = _authorizationContext.AuthorizationEntity.AsQueryable();
        if (configurable)
        {
            query = query.Where(a => a.Configurable == true);
        }

        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(a => a.Type == type);
        }

        return await query.ToListAsync();
    }
}
