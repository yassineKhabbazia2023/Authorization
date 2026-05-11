// <copyright file="SubscriptionValidatedEventHandler.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pulse.Authorization.Core.Exceptions;
using Pulse.Authorization.Infrastructure.Providers.Interfaces;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;
using Pulse.Back.Events.IntegrationEvents.EventsData;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Authorization.Infrastructure.Providers
{
    public class SubscriptionValidatedEventHandler : IEventHandler
    {
        private readonly ILogger<SubscriptionValidatedEventHandler> logger;
        private readonly ISubscriptionEventRepository subscriptionEventRepository;
        private readonly IAuthorizationEventPublisher authorizationEventPublisher;

        public SubscriptionValidatedEventHandler(
            ILogger<SubscriptionValidatedEventHandler> logger,
            ISubscriptionEventRepository subscriptionEventRepository,
            IAuthorizationEventPublisher authorizationEventPublisher)
        {
            this.logger = logger;
            this.subscriptionEventRepository = subscriptionEventRepository;
            this.authorizationEventPublisher = authorizationEventPublisher;
        }

        public async Task HandleAsync(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            var subEvent = JsonConvert.DeserializeObject<SubscriptionValidatedEvent>(message);

            this.logger.LogInformation("Consommation de l'event type: {EventType}, accountId: {AccountId}", subEvent?.EventType, subEvent?.Data?.AccountId);

            if (subEvent?.Data == null)
            {
                this.logger.LogError("Payload Data null: {Message}", message);
                return;
            }

            if (subEvent.Data.AccountId <= 0)
            {
                this.logger.LogError("AccountId <= 0: {Message}", message);
                return;
            }

            // Matérialiser products (tableau, Length O(1))
            var products = (subEvent.Data.Products ?? Enumerable.Empty<SubscriptionProductData>()).ToArray();
            if (products.Length == 0)
            {
                this.logger.LogError("Products list is null or empty: {Message}", message);
                return;
            }

            // Normalisation ProductCodes (tableau non-nullable)
            var productCodes = products
                .Select(p => p?.ProductCode?.Trim())
                .Where(code => !string.IsNullOrWhiteSpace(code))
                .Cast<string>() // plus de null après filtre
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (productCodes.Length == 0)
            {
                this.logger.LogError("Tous les ProductCodes sont vides ou nuls: {Message}", message);
                return;
            }

            // Dépot : operation sur account
            await this.subscriptionEventRepository.AddSubscriptionAuthorizationsOnAccountAsync(
                subEvent.Data.AccountId, productCodes);

            // Matérialiser ContactIds d'origine
            var contactIdsSource = subEvent.Data.ContactIds?.ToArray() ?? Array.Empty<int>();
            if (contactIdsSource.Length == 0)
            {
                this.logger.LogError("ContactIds is null or empty: {Message}", message);
                return;
            }

            // Vérifier s'il existe au moins un ContactId valide (> 0)
            var validContactIds = contactIdsSource.Where(id => id > 0).ToArray();
            if (validContactIds.Length == 0)
            {
                this.logger.LogError("Aucun ContactId valide trouvé: {Message}", message);
                return;
            }


            // Dépot : operation sur contacts (on passe les IDs d'origine pour couvrir le test)
            var result = await this.subscriptionEventRepository.AddSubscriptionAuthorizationsForContacts(
                contactIdsSource,
                subEvent.Data.AccountId,
                productCodes);

            var groupedByContact = result.Item1.GroupBy(c => c.ContactId);

            foreach (var group in groupedByContact)
            {
                var contactId = group.Key;
                var codes = group.Select(g => g.Authorization.Code).Distinct();
                await this.authorizationEventPublisher
                    .PublishAuthorizationUpdatedEventAsync(contactId, subEvent.Data.AccountId, codes, true);
            }

            var errors = result.Item2.ToList();
            if (errors.Count != 0)
            {
                this.logger.LogWarning("Erreurs d'autorisation: {Errors} | Payload: {Payload}", errors, message);
                return; // Le message est consommé sans impact DLQ
            }
        }
    }
}
