// <copyright file="AuthorizationRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Data.SqlClient;
using Polly;
using Polly.Retry;
using Pulse.Authorization.Infrastructure.Context;
using Pulse.Authorization.Core.Constants;
using Pulse.Authorization.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Pulse.Authorization.Infrastructure.Repositories;

public class AuthorizationRepository : IAuthorizationRepository
{
    private readonly AuthorizationContext _authorizationContext;
    private readonly AsyncRetryPolicy _retryPolicy;

    public AuthorizationRepository(AuthorizationContext authorizationContext)
    {
        _authorizationContext = authorizationContext;

        _retryPolicy = Policy
                .Handle<SqlException>()
                .WaitAndRetryAsync(
                    retryCount: 1,
                    sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(GlobalConstants.RetryTimespan));
    }

    public async Task<List<string>> GetContactAccountAuthorizationsAsync(int contactId, int accountId)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var contactAuthorizationCodes = _authorizationContext
                    .ContactAuthorization
                    .Include(x => x.Authorization)
                    .Where(x => x.ContactId == contactId && x.AccountId == accountId)
                    .Select(x => x.Authorization.Code)
                    .Distinct();

            var result = await contactAuthorizationCodes.ToListAsync();
            return result;
        });
    }

    public async Task<List<string>> GetAccountAuthorizationAsync(int accountId)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var contactAuthorizationCodes = _authorizationContext
                    .AccountAuthorization
                    .Include(x => x.Authorization)
                    .Where(x => x.AccountId == accountId)
                    .Select(x => x.Authorization.Code)
                    .Distinct();

            var result = await contactAuthorizationCodes.ToListAsync();
            return result;
        });
    }

    public async Task<List<string>> GetContactAuthorizationAsync(int contactId)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var contactAuthorizationCodes = _authorizationContext
                    .ContactAuthorization
                    .Include(x => x.Authorization)
                    .Where(x => x.ContactId == contactId)
                    .Select(x => x.Authorization.Code)
                    .Distinct();

            var result = await contactAuthorizationCodes.ToListAsync();
            return result;
        });
    }
}
