// <copyright file="IRoleEventRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Authorization.Core.Models.Subscriptions;
using Pulse.Authorization.Infrastructure.Entities;

namespace Pulse.Authorization.Infrastructure.Providers.Interfaces;

public interface IRoleEventRepository
{
    Task CreateRoleAsync(RoleEntity roleEntity);

    Task UpdateRoleAsync(RoleEntity roleEntity);

    Task DeleteRoleAsync(int contactId, int accountId);

    Task DeleteContactRolesAsync(int contactId);

    Task<bool> DoesRoleExistAsync(RoleEntity roleEntity);

    ContactRolesSubscription RetrieveContactsHavingRole(IEnumerable<int> contactIds, int accountId);

    Task<RoleEntity> GetRole(RoleEntity roleEntity);

    Task DeleteContactAuthorizations(int contactId, int accountId);
}
