// <copyright file="AuthorizationRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Data.SqlClient;
using Polly;
using Polly.Retry;
using Pulse.Authorization.Infrastructure.Context;
using Pulse.Authorization.Core.Constants;
using Pulse.Authorization.Core.Interfaces;
using Pulse.Authorization.Core.Models;
using Microsoft.EntityFrameworkCore;
using Pulse.Authorization.Infrastructure.Mappers;
using Pulse.Authorization.Core.Requests;

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

        public async Task<NavigationRequest> GetNavigationsAsync(int? accountId, Contact contact)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var resource = _authorizationContext.ResourceEntity.AsNoTracking()
                                                    .Include(r => r.AccountResourceEntity)
                                                    .ThenInclude(ar => ar.Account)
                                                    .ThenInclude(ac => ac.AuthorizationEntity)
                                                    .Include(r => r.ActionEntity)
                                                    .Include(r => r.InverseParent)
                                                    .ThenInclude(r => r.InverseParent)
                                                    .Where(r => (r.Type.Equals(contact.Type) || r.Type.Equals(Constants.ResourceTypeAll))
                                                           && r.AccountResourceEntity.Any(a => a.Account.AuthorizationEntity.Any(auth => auth.ContactId == contact.ContactId)));

                if (contact.Type!.Equals(Constants.ContactTypeClient) && accountId == 0)
                {
                    var result = await resource.ToListAsync();
                    return resource.MapToNavigationRequest();
                }
                else
                {
                    resource = resource.Where(r => r.AccountResourceEntity.Any(a => a.AccountId == accountId));
                    var result = await resource.ToListAsync();
                    return resource.MapToNavigationRequest();
                }
            });
        }
    }
}
