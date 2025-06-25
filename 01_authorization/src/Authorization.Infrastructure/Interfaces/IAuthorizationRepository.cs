// <copyright file="IAuthorizationRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Authorization.Core.Models;
using Pulse.Authorization.Core.Models.Subscriptions;
using Pulse.Authorization.Core.Models.Utils;
using Pulse.Authorization.Core.Request;
using Pulse.Authorization.Infrastructure.Entities;

namespace Pulse.Authorization.Infrastructure.Interfaces;

public interface IAuthorizationRepository
{
    Task<List<string>> GetContactAuthorizationAsync(int contactId);

    Task<List<string>> GetAccountAuthorizationAsync(int accountId);

    Task<List<string>> GetContactAccountAuthorizationsAsync(int contactId, int accountId, bool? viewGlobal);

    Task DeleteContactAuthorizationAsync(int contactId, int accountId);

    Task<IEnumerable<string>> CreateDefaultAuthorizationsOnAccountAsync(int accountId);

    Task<IEnumerable<string>> CreateDefaultAuthorizationsOnSignatoryAsync(int contactId, int accountId);

    Task<IEnumerable<string>> CreateReportingAuthorizationsOnAccountAsync(int accountId, string[] codes);

    public Task<IEnumerable<Entities.AccountAuthorizationEntity>> AddSubscriptionAuthorizationsOnAccountAsync(int accountId, IEnumerable<string> productCodes);

    public Task<IEnumerable<Entities.ContactAuthorizationEntity>> AddSubscriptionAuthorizationsOnAccountSignatoriesAsync(int accountId, IEnumerable<string> productCodes);

    Task<IEnumerable<ContactAuthorizationEntity>> AddSubscriptionAuthorizationOnAccountContactsAsync(IEnumerable<ContactAuthorizationEntity> contactAuthorizationEntities, int accountId);

    public Task SetContactAuthorizationFromAccountAuthorization(int accountId, int contactId);

    Task DeleteContactAuthorizationsAsync(int contactId, int accountId, string[] permissions);

    Task<IEnumerable<(int, string)>> CreateReportingAuthorizationsForSignatoriesAsync(IEnumerable<int> contactIds, int accountId);

    Task<List<string>> GetAllContactAuthorizationsAsync(int contactId);

    Task<Paging<Contact>> GetContactIdsByAuthorizationCodesAndAccountIdAsync(List<string> codes, int accountId, Pagination? pagination);

    ProductCodesSubscriptions RetrieveExistedProductCodes(IEnumerable<string> productCodes);

    Task SetPermissionByContactEmailAsync(string permission, List<string> emails);
}
