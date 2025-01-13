// <copyright file="IAccountEventRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Authorization.Infrastructure.Entities;

namespace Pulse.Authorization.Infrastructure.Providers.Interfaces
{
    public interface IAccountEventRepository
    {
        Task CreateAccountAsync(AccountEntity accountEntity);

        Task UpdateAccountAsync(AccountEntity accountEntity);

        Task RemoveAccountAsync(int accountId);

        Task RemoveAccountAuthorizationsAsync(int accountId);

        Task<bool> DoesAccountExistAsync(int accountId);
    }
}
