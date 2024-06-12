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

        public SubscriptionValidatedEventHandler(
            ILogger<SubscriptionValidatedEventHandler> logger,
            ISubscriptionEventRepository subscriptionEventRepository)
        {
            _logger = logger;
            _subscriptionEventRepository = subscriptionEventRepository;
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

            await _subscriptionEventRepository.AddSubscriptionAuthorizationsOnAccountAsync(subEvent!.Data.AccountId, subEvent.Data.Products.Select(p => p.ProductCode!));
            await _subscriptionEventRepository.AddSubscriptionAuthorizationsOnAccountSignatoriesAsync(subEvent.Data.AccountId, subEvent.Data.ContactIds, subEvent.Data.Products.Select(p => p.ProductCode!));
        }
    }
}
