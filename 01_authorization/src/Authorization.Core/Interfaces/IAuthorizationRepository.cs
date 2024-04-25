// <copyright file="IAuthorizationRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Authorization.Core.Interfaces;

public interface IAuthorizationRepository
{
    Task<List<string>> GetContactAuthorizations(int contactId);

    Task<List<string>> GetAccountAuthorizations(int accountId);

    Task<List<string>> GetContactAndAccountAuthorizations(int contactId, int accountId);
}
