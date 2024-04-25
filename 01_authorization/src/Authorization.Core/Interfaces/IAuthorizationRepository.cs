// <copyright file="IAuthorizationRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Authorization.Core.Interfaces;

public interface IAuthorizationRepository
{
    Task<List<string>> GetContactAuthorizationAsync(int contactId);

    Task<List<string>> GetAccountAuthorizationAsync(int accountId);

    Task<List<string>> GetContactAccountAuthorizationsAsync(int contactId, int accountId);
}
