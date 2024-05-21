// <copyright file="AuthorizationRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;
using Pulse.Authorization.Core.Constants;
using Pulse.Authorization.Core.Enum;
using Pulse.Authorization.Core.Interfaces;
using Pulse.Authorization.Infrastructure.Context;

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

    public async Task<List<string>> GetContactAccountAuthorizationsAsync(int contactId, int accountId, bool? viewGlobal)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var contactAuthorization = _authorizationContext
                    .ContactAuthorizationEntity
                    .Include(x => x.Authorization)
                    .Where(x => x.ContactId == contactId
                        && x.AccountId == accountId
                        && ((viewGlobal != null && x.Authorization.View != (viewGlobal.Value
                                                            ? AuthorizationView.Partial.ToString()
                                                            : AuthorizationView.Global.ToString()))
                             || viewGlobal == null))
                    .Select(x => x.Authorization.Code)
                    .Distinct();

            var result = await contactAuthorization.ToListAsync();
            return result;
        });
    }

    public async Task<List<string>> GetAccountAuthorizationAsync(int accountId)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var contactAuthorizationCodes = _authorizationContext
                    .AccountAuthorizationEntity
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
                    .ContactAuthorizationEntity
                    .Include(x => x.Authorization)
                    .Where(x => x.ContactId == contactId && x.Authorization.View != AuthorizationView.Partial.ToString())
                    .Select(x => x.Authorization.Code)
                    .Distinct();

            var result = await contactAuthorizationCodes.ToListAsync();
            return result;
        });
    }

    public async Task DeleteContactAuthorizationAsync(int contactId, int accountId)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            var permissions = _authorizationContext
                .ContactAuthorizationEntity
                .Where(x => x.ContactId == contactId && x.AccountId == accountId);

            _authorizationContext.ContactAuthorizationEntity.RemoveRange(permissions);
            await _authorizationContext.SaveChangesAsync();
        });
    }
}
