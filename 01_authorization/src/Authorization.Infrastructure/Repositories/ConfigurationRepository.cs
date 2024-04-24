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
                    sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(Constants.RetryTimespan));
    }

    public async Task<IEnumerable<Configuration>> GetContactAuthorizations(int contactId, int? accountId)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var authorization = _authorizationContext.AuthorizationEntity
                                    .Include(a => a.AccountAuthorization)
                                    .Where(a => a.ContactAuthorization.Any(c => c.ContactId == contactId)
                                            && a.AccountAuthorization.Any(a => a.AccountId == accountId)
                                            && a.Configurable == true);

            var result = await authorization.ToListAsync();

            return result.MapAuthorizationToConfiguration();
        });
    }
}
