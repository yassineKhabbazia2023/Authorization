// <copyright file="ConfigurationRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Data.SqlClient;
using Polly;
using Polly.Retry;
using Pulse.Authorization.Infrastructure.Context;
using Pulse.Authorization.Core.Constants;
using Pulse.Authorization.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Pulse.Authorization.Core.Models;
using Pulse.Authorization.Infrastructure.Mappers;
using System.Data;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Extensions;

namespace Pulse.Authorization.Infrastructure.Repositories;

public class ConfigurationRepository : IConfigurationRepository
{
    private readonly AuthorizationContext _authorizationContext;

    public ConfigurationRepository(AuthorizationContext authorizationContext)
    {
        _authorizationContext = authorizationContext;
        _authorizationContext.HandleEFCoreFailure();
    }

    public async Task<IEnumerable<Configuration>> GetAccountConfigurationAsync(int accountId, string type)
    {
        var authorization = await _authorizationContext
                     .AccountAuthorizationEntity
                     .Include(x => x.Authorization)
                     .Where(x => x.AccountId == accountId
                            && x.Authorization.Configurable == true
                            && x.Authorization.Type == type)
                     .Select(x => x.Authorization)
                     .Distinct()
                     .ToListAsync();

        return authorization.MapAuthorizationToConfiguration();
    }

    public async Task<IEnumerable<Configuration>> GetContactConfigurationAsync(int contactId)
    {
        var authorization = await _authorizationContext
                     .ContactAuthorizationEntity
                     .Include(x => x.Authorization)
                     .Where(x => x.ContactId == contactId
                            && x.Authorization.Configurable == true)
                     .Select(x => x.Authorization)
                     .Distinct()
                     .ToListAsync();

        return authorization.MapAuthorizationToConfiguration();
    }

    private async Task<IEnumerable<int>> GetAuthorizationEntitiesByCodeAsync(IEnumerable<string> codes)
    {
        return await _authorizationContext
                     .AuthorizationEntity
                     .Where(x => codes.Contains(x.Code))
                     .Select(x => x.AuthorizationId)
                     .Distinct()
                     .ToListAsync();
    }

    public async Task<IEnumerable<ContactAuthorizationEntity>> GetContactAccountConfigurationAsync(int contactId, int accountId)
    {
        return await _authorizationContext
                     .ContactAuthorizationEntity
                     .Include(x => x.Authorization)
                     .Where(x => x.ContactId == contactId && x.AccountId == accountId && x.Authorization.Configurable!.Value)
                     .Select(x => x)
                     .Distinct()
                     .ToListAsync();
    }

    public async Task DeleteContactAccountAuthorizationAsync(IEnumerable<ContactAuthorizationEntity> contactAuthorizations)
    {
        _authorizationContext.ContactAuthorizationEntity.RemoveRange(contactAuthorizations);
        await _authorizationContext.SaveChangesAsync();
    }

    public async Task CreateOrUpdateContactAccountAuthorizationAsync(int contactId, int accountId, IEnumerable<string> codes)
    {
        var oldContactAuthorizationEntities = await GetContactAccountConfigurationAsync(contactId, accountId);

        if (oldContactAuthorizationEntities?.Any() == true)
        {
            await DeleteContactAccountAuthorizationAsync(oldContactAuthorizationEntities);
        }

        var authorizationIds = await GetAuthorizationEntitiesByCodeAsync(codes);
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
}
