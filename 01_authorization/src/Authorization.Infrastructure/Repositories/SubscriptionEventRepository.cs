// <copyright file="SubscriptionEventRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Data.SqlClient;
using Polly.Retry;
using Polly;
using Pulse.Authorization.Infrastructure.Context;
using Pulse.Authorization.Infrastructure.Providers.Interfaces;
using Pulse.Authorization.Core.Constants;
using Microsoft.EntityFrameworkCore;
using Pulse.Authorization.Core.Models;
using Microsoft.Extensions.Logging;
using Pulse.Authorization.Core.Exceptions;
using System.Net;

namespace Pulse.Authorization.Infrastructure.Repositories
{
    public class SubscriptionEventRepository : ISubscriptionEventRepository
    {
        private readonly AuthorizationContext _authorizationContext;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly ILogger<SubscriptionEventRepository> _logger;

        public SubscriptionEventRepository(AuthorizationContext authorizationContext, ILogger<SubscriptionEventRepository> logger)
        {
            _authorizationContext = authorizationContext;
            _logger = logger;
            _retryPolicy = Policy
                    .Handle<SqlException>()
                    .WaitAndRetryAsync(
                        retryCount: 1,
                        sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(GlobalConstants.RetryTimespan));
        }

        public async Task AddSubscriptionAuthorizationsOnAccountAsync(int accountId, IEnumerable<string> productCodes)
        {
            var authorizations = await _authorizationContext.AuthorizationEntity.AsNoTracking().ToListAsync();

            _authorizationContext.AccountAuthorizationEntity.AddRange(
                authorizations.Where(a => productCodes.Contains(a.ProductCode))
                .DistinctBy(a => a.AuthorizationId).Select(a =>
            {
                return new Entities.AccountAuthorizationEntity
                {
                    AccountId = accountId,
                    AuthorizationId = a.AuthorizationId,
                    Enabled = true,
                };
            }));

            await _retryPolicy.ExecuteAsync(async () =>
            {
                await _authorizationContext.SaveChangesAsync();
            });
        }

        public async Task AddSubscriptionAuthorizationsOnContactAsync(int accountId, IEnumerable<int> contactIds, IEnumerable<string> productCodes)
        {
            var authorizations = await _authorizationContext.AuthorizationEntity.AsNoTracking().ToListAsync();
            var roles = (await _authorizationContext.RoleEntity.AsNoTracking()
                   .Include(r => r.Contact).AsNoTracking()
                   .Where(r => r.AccountId == accountId
                   && contactIds.Contains(r.ContactId)
                   && r.Contact!.Type == ContactType.Customer.ToString()
                   && r.IsSignatory.HasValue && r.IsSignatory.Value).ToListAsync())
                   .DistinctBy(r => r.ContactId);

            foreach (var role in roles)
            {
                _authorizationContext.ContactAuthorizationEntity.AddRange(
                    authorizations.Where(a => productCodes.Contains(a.ProductCode)).DistinctBy(a => a.AuthorizationId).Select(a =>
                {
                    return new Entities.ContactAuthorizationEntity
                    {
                        AccountId = accountId,
                        AuthorizationId = a.AuthorizationId,
                        ContactId = role.ContactId,
                    };
                }));
            }

            await _retryPolicy.ExecuteAsync(async () =>
            {
                await _authorizationContext.SaveChangesAsync();
            });
        }
    }
}
