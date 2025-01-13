// <copyright file="AccountRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Authorization.Infrastructure.Context;
using Pulse.Authorization.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Extensions;

namespace Pulse.Authorization.Infrastructure.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AuthorizationContext _authorizationContext;

        public AccountRepository(AuthorizationContext authorizationContext)
        {
            _authorizationContext = authorizationContext;
            _authorizationContext.HandleEFCoreFailure();
        }

        public async Task<AccountEntity> GetAccountByIdAsync(int accountId)
        {
            var account = await _authorizationContext.AccountEntity.AsNoTracking()
                                                    .FirstOrDefaultAsync(c => c.AccountId == accountId);

            return account!;
        }
    }
}
