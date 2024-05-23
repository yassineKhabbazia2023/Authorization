// <copyright file="RoleEventRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;
using Pulse.Authorization.Core.Constants;
using Pulse.Authorization.Infrastructure.Context;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Extensions;
using Pulse.Authorization.Infrastructure.Mappers.EventMappers;
using Pulse.Authorization.Infrastructure.Providers.Interfaces;

namespace Pulse.Authorization.Infrastructure.Repositories
{
    public class RoleEventRepository : IRoleEventRepository
    {
        private readonly AuthorizationContext _authorizationContext;
        private readonly AsyncRetryPolicy _retryPolicy;

        public RoleEventRepository(AuthorizationContext authorziationContext)
        {
            _authorizationContext = authorziationContext;
            _authorizationContext.HandleEFCoreFailure();
            _retryPolicy = Policy
                    .Handle<SqlException>()
                    .WaitAndRetryAsync(
                        retryCount: 1,
                        sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(GlobalConstants.RetryTimespan));
        }

        public async Task CreateRoleAsync(RoleEntity roleEntity)
        {
            await _authorizationContext.RoleEntity.AddAsync(roleEntity);
            await _retryPolicy.ExecuteAsync(async () =>
            {
                await _authorizationContext.SaveChangesAsync();
            });
        }

        public async Task DeleteRoleAsync(int contactId, int accountId)
        {
            var roleToDelete = _authorizationContext.RoleEntity.Where(x => x.ContactId == contactId && x.AccountId == accountId).FirstOrDefault();
            if (roleToDelete != null)
            {
                _authorizationContext.Remove(roleToDelete);
            }

            await _retryPolicy.ExecuteAsync(async () =>
            {
                await _authorizationContext.SaveChangesAsync();
            });
        }

        public async Task UpdateRoleAsync(RoleEntity roleEntity)
        {
            var existingRole = await _authorizationContext.RoleEntity.SingleAsync(x => x.ContactId == roleEntity.ContactId && x.AccountId == roleEntity.AccountId);
            roleEntity.ToRoleEntity(existingRole);

            await _retryPolicy.ExecuteAsync(async () =>
            {
                await _authorizationContext.SaveChangesAsync();
            });
        }
    }
}
