// <copyright file="ISubscriptionEventRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Authorization.Infrastructure.Entities;

namespace Pulse.Authorization.Infrastructure.Providers.Interfaces
{
    public interface ISubscriptionEventRepository
    {
        public Task<IEnumerable<AccountAuthorizationEntity>> AddSubscriptionAuthorizationsOnAccountAsync(int accountId, IEnumerable<string> productCodes);

        public Task<IEnumerable<ContactAuthorizationEntity>> AddSubscriptionAuthorizationsOnAccountSignatoriesAsync(int accountId, IEnumerable<int> contactIds, IEnumerable<string> productCodes);
    }
}
