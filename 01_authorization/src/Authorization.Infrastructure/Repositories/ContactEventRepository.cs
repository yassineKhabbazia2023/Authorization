// <copyright file="ContactEventRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;
using Pulse.Authorization.Core.Constants;
using Pulse.Authorization.Core.Enum;
using Pulse.Authorization.Infrastructure.Context;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Mappers.EventMappers;
using Pulse.Authorization.Infrastructure.Providers.Interfaces;

namespace Pulse.Authorization.Infrastructure.Repositories
{
    public class ContactEventRepository : IContactEventRepository
    {
        private readonly AuthorizationContext _authorizationContext;
        private readonly AsyncRetryPolicy _retryPolicy;

        public ContactEventRepository(AuthorizationContext authorizationContext)
        {
            _authorizationContext = authorizationContext;
            _retryPolicy = Policy
                    .Handle<SqlException>()
                    .WaitAndRetryAsync(
                        retryCount: 1,
                        sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(GlobalConstants.RetryTimespan));
        }

        public async Task CreateContactAsync(ContactEntity contactEntity)
        {
            await _authorizationContext.ContactEntity.AddAsync(contactEntity);
            await _retryPolicy.ExecuteAsync(async () =>
            {
                await _authorizationContext.SaveChangesAsync();
            });
        }

        public async Task RemoveContactAsync(int contactId)
        {
            var existingContact = await _authorizationContext.ContactEntity.SingleAsync(x => x.ContactId == contactId);
            existingContact.Status = ContactStatus.Removed.ToString();

            await _retryPolicy.ExecuteAsync(async () =>
            {
                await _authorizationContext.SaveChangesAsync();
            });
        }

        public async Task UpdateContactAsync(ContactEntity contactEntity)
        {
            var existingContact = await _authorizationContext.ContactEntity.SingleAsync(x => x.ContactId == contactEntity.ContactId);
            contactEntity.ToContactEntity(existingContact);

            await _retryPolicy.ExecuteAsync(async () =>
            {
                await _authorizationContext.SaveChangesAsync();
            });
        }

        public async Task RemoveContactAuthorizationsAsync(int contactId)
        {
            await _authorizationContext.ContactAuthorizationEntity.Where(x => x.ContactId == contactId).ForEachAsync(et =>
            {
                _authorizationContext.Entry(et).State = EntityState.Deleted;
            });

            await _retryPolicy.ExecuteAsync(async () =>
            {
                await _authorizationContext.SaveChangesAsync();
            });
        }
    }
}
