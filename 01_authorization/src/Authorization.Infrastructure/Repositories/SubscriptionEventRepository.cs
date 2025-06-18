// <copyright file="SubscriptionEventRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Authorization.Infrastructure.Providers.Interfaces;
using Pulse.Authorization.Infrastructure.Interfaces;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Core.Interfaces;
using Pulse.Authorization.Core.Models.Subscriptions;
using Pulse.Authorization.Core.Mappers;
using Microsoft.Extensions.Logging;

namespace Pulse.Authorization.Infrastructure.Repositories;

public class SubscriptionEventRepository : ISubscriptionEventRepository
{
    private readonly IAuthorizationRepository _authorizationRepository;
    private readonly IContactRepository _contactRepository;
    private readonly IRoleEventRepository _roleEventRepository;

    public SubscriptionEventRepository(IAuthorizationRepository authorizationRepository, IRoleEventRepository roleEventRepository, IContactRepository contactRepository)
    {
        _authorizationRepository = authorizationRepository;
        _roleEventRepository = roleEventRepository;
        _contactRepository = contactRepository;
    }

    public async Task<IEnumerable<AccountAuthorizationEntity>> AddSubscriptionAuthorizationsOnAccountAsync(int accountId, IEnumerable<string> productCodes)
    {
        return await _authorizationRepository.AddSubscriptionAuthorizationsOnAccountAsync(accountId, productCodes);
    }

    public async Task<IEnumerable<ContactAuthorizationEntity>> AddSubscriptionAuthorizationsOnAccountSignatoriesAsync(int accountId, IEnumerable<string> productCodes)
    {
        return await _authorizationRepository.AddSubscriptionAuthorizationsOnAccountSignatoriesAsync(accountId, productCodes);
    }

    public async Task<(IEnumerable<ContactAuthorizationEntity>, IEnumerable<string>)> AddSubscriptionAuthorizationsForContacts(IEnumerable<int> contactIds, int accountId, IEnumerable<string> productCodes)
    {
        ContactsSubscription contactSubscription = _contactRepository.RetrieveExistedContacts(contactIds);

        ContactRolesSubscription contactRolesSubscription = _roleEventRepository.RetrieveContactsHavingRole(contactIds, accountId);

        ProductCodesSubscriptions productCodesSubscriptions = _authorizationRepository.RetrieveExistedProductCodes(productCodes);

        var collabAuthorizationSubscriptions = new ContactAuthorizationSubscription { AccountId = accountId, AuthorizationIds = productCodesSubscriptions.CollabAuthorizationIds, ContactIds = contactSubscription.CollabContacts };
        var clientAuthorizationSubscriptions = new ContactAuthorizationSubscription { AccountId = accountId, AuthorizationIds = productCodesSubscriptions.ClientAuthorizationIds, ContactIds = contactSubscription.ClientContacts };

        IEnumerable<ContactAuthorizationEntity> contactAuthorizationEntities = [];
        contactAuthorizationEntities = contactAuthorizationEntities
            .Concat(collabAuthorizationSubscriptions.MapToContactAuthorizationEntities())
            .Concat(clientAuthorizationSubscriptions.MapToContactAuthorizationEntities());

        contactAuthorizationEntities = await _authorizationRepository.AddSubscriptionAuthorizationOnAccountContactsAsync(contactAuthorizationEntities);

        IEnumerable<string> errors = [];
        errors = contactSubscription.BuildErrors().Concat(contactRolesSubscription.BuildErrors()).Concat(productCodesSubscriptions.BuildErrors());

        return (contactAuthorizationEntities, errors);
    }

}
