// <copyright file="IContactRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Collections.Generic;
using Pulse.Authorization.Core.Models;

namespace Pulse.Authorization.Core.Interfaces
{
    public interface IContactRepository
    {
        Task<Contact> GetContactByIdAsync(int contactId);
    }
}
