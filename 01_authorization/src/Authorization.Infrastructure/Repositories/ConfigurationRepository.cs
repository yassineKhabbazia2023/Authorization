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

    public async Task DeleteContactAccountAuthorization(int contactId, int accountId, IEnumerable<int> actionId)
    {

    }

    public async Task CreateContactAccountAuthorization(int contactId, int accountId, IEnumerable<int> actionId)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            if (!_authorizationContext.AccountEntity.Any(x => x.AccountId == accountId))
            {
                throw new NotFoundException(Errors.NotFoundAccountCode, string.Format(Errors.NotFoundAccountMessage, accountId));
            }

            if (!_authorizationContext.ContactEntity.Any(x => x.ContactId == contactId))
            {
                throw new NotFoundException(Errors.NotFoundContactCode, string.Format(Errors.NotFoundContactMessage, contactId));
            }

            //var contactAuthorizationEntities 

            //if (contactAuthorizationEntities.Any())
            //{
            //    await _authorizationContext.ContactAuthorization.AddRangeAsync(roleEntities);
            //}

            //await _authorizationContext.ContactAuthorization.AddRangeAsync(delegationEntities);
            //await _authorizationContext.SaveChangesAsync();
        });
    }
}
