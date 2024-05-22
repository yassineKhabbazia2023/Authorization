// <copyright file="ContactRepository.cs" company="Pulse">
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
using Pulse.Authorization.Infrastructure.Extensions;

namespace Pulse.Authorization.Infrastructure.Repositories
{
    public class ContactRepository : IContactRepository
    {
        private readonly AuthorizationContext _authorizationContext;

        public ContactRepository(AuthorizationContext authorizationContext)
        {
            _authorizationContext = authorizationContext;
            _authorizationContext.HandleEFCoreFailure();
        }

        public async Task<Contact> GetContactByIdAsync(int contactId)
        {
            var contact = await _authorizationContext.ContactEntity.AsNoTracking()
                                                    .FirstOrDefaultAsync(c => c.ContactId == contactId);

            return contact!.MapToContact();
        }
    }
}
