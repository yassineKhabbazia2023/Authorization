// <copyright file="ContactEventRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Pulse.Authorization.Infrastructure.Context;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Enum;
using Pulse.Authorization.Infrastructure.Extensions;
using Pulse.Authorization.Infrastructure.Mappers.EventMappers;
using Pulse.Authorization.Infrastructure.Providers.Interfaces;

namespace Pulse.Authorization.Infrastructure.Repositories
{
    public class ContactEventRepository : IContactEventRepository
    {
        private readonly AuthorizationContext _authorizationContext;

        public ContactEventRepository(AuthorizationContext authorizationContext)
        {
            _authorizationContext = authorizationContext;
            _authorizationContext.HandleEFCoreFailure();
        }

        public async Task CreateContactAsync(ContactEntity contactEntity)
        {
            await _authorizationContext.ContactEntity.AddAsync(contactEntity);
            await _authorizationContext.SaveChangesAsync();
        }

        public async Task RemoveContactAsync(int contactId)
        {
            var existingContact = await _authorizationContext.ContactEntity.FirstOrDefaultAsync(x => x.ContactId == contactId);

            if (existingContact != null)
            {
                existingContact.Status = ContactStatus.Removed.ToString();
                existingContact.LastUpdateDate = DateTime.UtcNow;

                await _authorizationContext.SaveChangesAsync();
            }
        }

        public async Task UpdateContactAsync(ContactEntity contactEntity)
        {
            var existingContact = await _authorizationContext.ContactEntity.FirstOrDefaultAsync(x => x.ContactId == contactEntity.ContactId);

            if (existingContact != null)
            {
                contactEntity.ToContactEntity(existingContact);

                await _authorizationContext.SaveChangesAsync();
            }
        }

        public async Task RemoveContactAuthorizationsAsync(int contactId)
        {
            await _authorizationContext.ContactAuthorizationEntity.Where(x => x.ContactId == contactId).ForEachAsync(et =>
            {
                _authorizationContext.Entry(et).State = EntityState.Deleted;
            });

            await _authorizationContext.SaveChangesAsync();
        }

        public async Task<bool> DoesContactExistAsync(int contactId)
        {
            var contact = await _authorizationContext.ContactEntity.FirstOrDefaultAsync(c => c.ContactId == contactId);

            return contact != null;
        }
    }
}
