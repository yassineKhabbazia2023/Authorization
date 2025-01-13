// <copyright file="IContactEventRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Authorization.Infrastructure.Entities;

namespace Pulse.Authorization.Infrastructure.Providers.Interfaces
{
    public interface IContactEventRepository
    {
        Task CreateContactAsync(ContactEntity contactEntity);

        Task UpdateContactAsync(ContactEntity contactEntity);

        Task RemoveContactAsync(int contactId);

        Task RemoveContactAuthorizationsAsync(int contactId);

        Task<bool> DoesContactExistAsync(int contactId);
    }
}
