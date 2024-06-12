// <copyright file="SubscriptionEventRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Authorization.Infrastructure.Context;
using Pulse.Authorization.Infrastructure.Providers.Interfaces;
using Microsoft.Extensions.Logging;
using Pulse.Authorization.Infrastructure.Extensions;
using Pulse.Authorization.Infrastructure.Interfaces;
using Pulse.Authorization.Infrastructure.Entities;

namespace Pulse.Authorization.Infrastructure.Repositories
{
    public class SubscriptionEventRepository : ISubscriptionEventRepository
    {
        private readonly IAuthorizationRepository _authorizationRepository;

        public SubscriptionEventRepository(IAuthorizationRepository authorizationRepository)
        {
            _authorizationRepository = authorizationRepository;
        }

        public async Task<IEnumerable<AccountAuthorizationEntity>> AddSubscriptionAuthorizationsOnAccountAsync(int accountId, IEnumerable<string> productCodes)
        {
            return await _authorizationRepository.AddSubscriptionAuthorizationsOnAccountAsync(accountId, productCodes);
        }

        public async Task<IEnumerable<ContactAuthorizationEntity>> AddSubscriptionAuthorizationsOnAccountSignatoriesAsync(int accountId, IEnumerable<int> contactIds, IEnumerable<string> productCodes)
        {
            return await _authorizationRepository.AddSubscriptionAuthorizationsOnAccountSignatoriesAsync(accountId, contactIds, productCodes);
        }
    }
}
