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
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Pulse.Authorization.Core.Exceptions;
using System.Data;
using Pulse.Authorization.Infrastructure.Entities;

namespace Pulse.Authorization.Infrastructure.Repositories;

public class ConfigurationRepository : IConfigurationRepository
{
    private readonly AuthorizationContext _authorizationContext;
    private readonly AsyncRetryPolicy _retryPolicy;

    public ConfigurationRepository(AuthorizationContext authorizationContext)
    {
        _authorizationContext = authorizationContext;

        _retryPolicy = Policy
                .Handle<SqlException>()
                .WaitAndRetryAsync(
                    retryCount: 1,
                    sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(GlobalConstants.RetryTimespan));
    }

    public async Task<IEnumerable<Configuration>> GetAccountConfigurationAsync(int accountId)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var authorization = _authorizationContext
                     .AccountAuthorization
                     .Include(x => x.Authorization)
                     .Where(x => x.AccountId == accountId
                            && x.Authorization.Configurable == true)
                     .Select(x => x.Authorization)
                     .Distinct();

            var result = await authorization.ToListAsync();

            return result.MapAuthorizationToConfiguration();
        });
    }

    public async Task<IEnumerable<Configuration>> GetContactConfigurationAsync(int contactId)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var authorization = _authorizationContext
                     .ContactAuthorization
                     .Include(x => x.Authorization)
                     .Where(x => x.ContactId == contactId
                            && x.Authorization.Configurable == true)
                     .Select(x => x.Authorization)
                     .Distinct();

            var result = await authorization.ToListAsync();

            return result.MapAuthorizationToConfiguration();
        });
    }

    private async Task<IEnumerable<AuthorizationEntity>> GetAuthorizationEntitiesByCodeAsync(IEnumerable<string> codes)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var contactAuthorization = await _authorizationContext
                     .AuthorizationEntity
                     .Where(x => codes.Contains(x.Code))
                     .Select(x => x)
                     .Distinct()
                     .ToListAsync();

            return contactAuthorization;
        });
    }

    public async Task<IEnumerable<ContactAuthorization>> GetContactAccountConfigurationAsync(int contactId, int accountId)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var contactAuthorization = await _authorizationContext
                     .ContactAuthorization
                     .Include(x => x.Authorization)
                     .Where(x => x.ContactId == contactId
                                && x.AccountId == accountId)
                     .Select(x => x)
                     .Distinct()
                     .ToListAsync();

            return contactAuthorization;
        });
    }

    public async Task<int> DeleteContactAccountAuthorizationAsync(IEnumerable<ContactAuthorization> contactAuthorizations)
    {
        int result = 0;
        await _retryPolicy.ExecuteAsync(async () =>
        {
            if (contactAuthorizations.Any())
            {
                _authorizationContext.ContactAuthorization.RemoveRange(contactAuthorizations);
                result = await _authorizationContext.SaveChangesAsync();
            }
        });

        return result;
    }

    public async Task UpdateContactAccountAuthorizationAsync(int contactId, int accountId, IEnumerable<string> codes)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            var oldContactAuthorizationEntities = await GetContactAccountConfigurationAsync(contactId, accountId);

            if (oldContactAuthorizationEntities.Any())
            {
                int deleted = await DeleteContactAccountAuthorizationAsync(oldContactAuthorizationEntities);

                if (deleted > 0)
                {
                    var authorizationByCode = await GetAuthorizationEntitiesByCodeAsync(codes);
                    var newContactAuthorizationEntities = authorizationByCode.Select(auth => new ContactAuthorization()
                    {
                        AccountId = accountId,
                        ContactId = contactId,
                        AuthorizationId = auth.AuthorizationId,
                        CreationDate = DateTime.UtcNow
                    });

                    await _authorizationContext.ContactAuthorization.AddRangeAsync(newContactAuthorizationEntities);
                    await _authorizationContext.SaveChangesAsync();
                }
            }
            else
            {
                throw new NotFoundException(Errors.NotFoundContactAccountAuthCode, string.Format(Errors.NotFoundContactAccountAuthCode, contactId, string.Join('-', codes), accountId));
            }
        });
    }
}
