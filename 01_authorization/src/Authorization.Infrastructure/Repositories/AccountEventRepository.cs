// <copyright file="AccountEventRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Xml.Linq;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;
using Pulse.Authorization.Core.Constants;
using Pulse.Authorization.Core.Enum;
using Pulse.Authorization.Infrastructure.Context;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Extensions;
using Pulse.Authorization.Infrastructure.Mappers.EventMappers;
using Pulse.Authorization.Infrastructure.Providers.Interfaces;

namespace Pulse.Authorization.Infrastructure.Repositories
{
    public class AccountEventRepository : IAccountEventRepository
    {
        private readonly AuthorizationContext _authorizationContext;

        public AccountEventRepository(AuthorizationContext authorizationContext)
        {
            _authorizationContext = authorizationContext;
            _authorizationContext.HandleEFCoreFailure();
        }

        public async Task CreateAccountAsync(AccountEntity accountEntity)
        {
            await _authorizationContext.AccountEntity.AddAsync(accountEntity);
            await _authorizationContext.SaveChangesAsync();
        }

        public async Task UpdateAccountAsync(AccountEntity accountEntity)
        {
            var existingContact = await _authorizationContext.AccountEntity.SingleAsync(x => x.AccountId == accountEntity.AccountId);
            accountEntity.ToAccountEntity(existingContact);

            await _authorizationContext.SaveChangesAsync();
        }

        public async Task RemoveAccountAsync(int accountId)
        {
            var existingAccount = await _authorizationContext.AccountEntity.SingleAsync(x => x.AccountId == accountId);
            existingAccount.Status = AccountStatus.Revoked.ToString();

            await _authorizationContext.SaveChangesAsync();
        }

        public async Task RemoveAccountAuthorizationsAsync(int accountId)
        {
            await _authorizationContext.AccountAuthorizationEntity.Where(x => x.AccountId == accountId).ForEachAsync(et =>
            {
                _authorizationContext.Entry(et).State = EntityState.Deleted;
            });

            await _authorizationContext.ContactAuthorizationEntity.Where(x => x.AccountId == accountId).ForEachAsync(et =>
            {
                _authorizationContext.Entry(et).State = EntityState.Deleted;
            });

            await _authorizationContext.SaveChangesAsync();
        }
    }
}
