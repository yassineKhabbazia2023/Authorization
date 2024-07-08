// <copyright file="SubscriptionValidatedEventHandler.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pulse.Authorization.Infrastructure.Providers.Interfaces;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;

namespace Pulse.Authorization.Infrastructure.Providers
{
    public class SubscriptionValidatedEventHandler : IEventHandler
    {
        private readonly ILogger<SubscriptionValidatedEventHandler> _logger;
        private readonly ISubscriptionEventRepository _subscriptionEventRepository;
        private readonly IAuthorizationEventPublisher _authorizationEventPublisher;

        public SubscriptionValidatedEventHandler(
            ILogger<SubscriptionValidatedEventHandler> logger,
            ISubscriptionEventRepository subscriptionEventRepository,
            IAuthorizationEventPublisher authorizationEventPublisher)
        {
            _logger = logger;
            _subscriptionEventRepository = subscriptionEventRepository;
            _authorizationEventPublisher = authorizationEventPublisher;
        }

        public async Task HandleAsync(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            var subEvent = JsonConvert.DeserializeObject<SubscriptionValidatedEvent>(message);
            _logger.LogInformation("Consommation de l'event type: {EventType}, accountId: {AccountId}",
            subEvent?.EventType,
            subEvent?.Data?.AccountId);

            if (subEvent?.Data == null || subEvent?.Data?.AccountId <= 0)
            {
                return;
            }

            var productCodes = subEvent!.Data.Products.Where(p => p.ProductCode != null).Select(p => p.ProductCode);
            await _subscriptionEventRepository.AddSubscriptionAuthorizationsOnAccountAsync(subEvent!.Data.AccountId, productCodes!);
            var result = await _subscriptionEventRepository.AddSubscriptionAuthorizationsOnAccountSignatoriesAsync(subEvent.Data.AccountId,
                                                                                                                   productCodes!);
            var groupedByContact = result.GroupBy(c => c.ContactId);

            foreach (var group in groupedByContact)
            {
                var contactId = group.Key;
                var codes = group.Select(g => g.Authorization.Code);

                await _authorizationEventPublisher.PublishAuthorizationUpdatedEventAsync(contactId, subEvent.Data.AccountId, codes!);
            }
        }
    }
}
