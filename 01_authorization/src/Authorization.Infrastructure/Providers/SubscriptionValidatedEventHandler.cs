// <copyright file="SubscriptionValidatedEventHandler.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pulse.Authorization.Core.Exceptions;
using Pulse.Authorization.Infrastructure.Providers.Interfaces;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;
using Pulse.ExceptionMiddleware.Exceptions;

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
            if (subEvent?.Data == null || subEvent?.Data?.AccountId <= 0 || subEvent?.Data?.Products.Count() == 0 || subEvent?.Data?.ContactIds.Count() == 0)
            {
                return;
            }

            var productCodes = subEvent!.Data.Products.Where(p => p.ProductCode != null).Select(p => p.ProductCode);
            await _subscriptionEventRepository.AddSubscriptionAuthorizationsOnAccountAsync(subEvent!.Data.AccountId, productCodes!);
            var result = await _subscriptionEventRepository.AddSubscriptionAuthorizationsForContacts(subEvent!.Data.ContactIds, subEvent!.Data.AccountId, productCodes);
            var groupedByContact = result.Item1.GroupBy(c => c.ContactId);

            foreach (var group in groupedByContact)
            {
                var contactId = group.Key;
                var codes = group.Select(g => g.Authorization.Code).Distinct();

                await _authorizationEventPublisher.PublishAuthorizationUpdatedEventAsync(contactId, subEvent.Data.AccountId, codes!, true);
            }

            var errors = result.Item2;
            if (errors.Any())
            {
                throw new BadRequestException(Errors.SubscriptionAuthorizationErrorCode, string.Format(Errors.SubscriptionAuthorizationErrorMessage, string.Join("\n", errors.ToArray())));
            }
        }
    }
}
