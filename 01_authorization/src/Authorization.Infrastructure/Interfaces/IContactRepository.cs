// <copyright file="IContactRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Authorization.Infrastructure.Entities;

namespace Pulse.Authorization.Core.Interfaces
{
    public interface IContactRepository
    {
        Task<ContactEntity> GetContactByIdAsync(int contactId);

        Task<IEnumerable<int>> GetSignatoriesAsync(int accountId);
    }
}
