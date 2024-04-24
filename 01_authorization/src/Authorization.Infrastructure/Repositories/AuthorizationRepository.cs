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

namespace Pulse.Authorization.Infrastructure.Repositories
{
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
                        sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(Constants.RetryTimespan));
        }

        public async Task<List<string>> GetContactAuthorizations(int contactId, int? accountId)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var authorization = _authorizationContext.AuthorizationEntity
                                        .Include(a => a.AccountAuthorization)
                                        .Include(a => a.ContactAuthorization)
                                        .Where(a => a.ContactAuthorization.Any(c => c.ContactId == contactId)
                                                && a.AccountAuthorization.Any(a => a.AccountId == accountId))
                                        .Select(a => a.Code)
                                        .Distinct();

                var result = await authorization.ToListAsync();
                return result;
            });
        }
    }
}
