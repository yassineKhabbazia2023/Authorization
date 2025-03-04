// <copyright file="IAuthorizationRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Authorization.Infrastructure.Interfaces;

public interface IAuthorizationRepository
{
    Task<List<string>> GetContactAuthorizationAsync(int contactId);

    Task<List<string>> GetAccountAuthorizationAsync(int accountId);

    Task<List<string>> GetContactAccountAuthorizationsAsync(int contactId, int accountId, bool? viewGlobal);

    Task DeleteContactAuthorizationAsync(int contactId, int accountId);

    Task<IEnumerable<string>> CreateDefaultAuthorizationsOnAccountAsync(int accountId);

    Task<IEnumerable<string>> CreateDefaultAuthorizationsOnSignatoryAsync(int contactId, int accountId);

    Task<IEnumerable<string>> CreateRapportBIAuthorizationsOnAccountAsync(int accountId, string[] codes);

    public Task<IEnumerable<Entities.AccountAuthorizationEntity>> AddSubscriptionAuthorizationsOnAccountAsync(int accountId, IEnumerable<string> productCodes);

    public Task<IEnumerable<Entities.ContactAuthorizationEntity>> AddSubscriptionAuthorizationsOnAccountSignatoriesAsync(int accountId, IEnumerable<string> productCodes);
}
