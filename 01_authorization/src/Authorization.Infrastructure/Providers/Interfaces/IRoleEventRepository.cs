// <copyright file="IRoleEventRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Authorization.Infrastructure.Entities;

namespace Pulse.Authorization.Infrastructure.Providers.Interfaces
{
    public interface IRoleEventRepository
    {
        Task CreateRoleAsync(RoleEntity roleEntity);

        Task UpdateRoleAsync(RoleEntity roleEntity);

        Task DeleteRoleAsync(int contactId, int accountId);

        Task DeleteContactRolesAsync(int contactId);
    }
}
