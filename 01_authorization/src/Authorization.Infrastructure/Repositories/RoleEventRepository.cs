// <copyright file="RoleEventRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;
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

        public RoleEventRepository(AuthorizationContext authorziationContext)
        {
            _authorizationContext = authorziationContext;
            _authorizationContext.HandleEFCoreFailure();
        }

        public async Task CreateRoleAsync(RoleEntity roleEntity)
        {
            await _authorizationContext.RoleEntity.AddAsync(roleEntity);
            await _authorizationContext.SaveChangesAsync();
        }

        public async Task DeleteContactRolesAsync(int contactId)
        {
            await _authorizationContext.RoleEntity.Where(r => r.ContactId == contactId)
                .ForEachAsync(r =>
                {
                    _authorizationContext.Entry(r).State = EntityState.Deleted;
                });

            await _authorizationContext.SaveChangesAsync();
        }

        public async Task DeleteRoleAsync(int contactId, int accountId)
        {
            var roleToDelete = _authorizationContext.RoleEntity.Where(x => x.ContactId == contactId && x.AccountId == accountId).FirstOrDefault();
            if (roleToDelete != null)
            {
                _authorizationContext.Remove(roleToDelete);
            }

            await _authorizationContext.SaveChangesAsync();
        }

        public async Task UpdateRoleAsync(RoleEntity roleEntity)
        {
            var existingRole = await _authorizationContext.RoleEntity.SingleAsync(x => x.ContactId == roleEntity.ContactId && x.AccountId == roleEntity.AccountId);
            roleEntity.ToRoleEntity(existingRole);

            await _authorizationContext.SaveChangesAsync();
        }
    }
}
