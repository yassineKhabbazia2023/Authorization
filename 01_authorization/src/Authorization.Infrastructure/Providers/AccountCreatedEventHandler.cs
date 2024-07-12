// <copyright file="AccountCreatedEventHandler.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pulse.Authorization.Infrastructure.Interfaces;
using Pulse.Authorization.Infrastructure.Mappers.EventMappers;
using Pulse.Authorization.Infrastructure.Providers.Interfaces;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;

namespace Pulse.Authorization.Infrastructure.Providers
{
    public class AccountCreatedEventHandler : IEventHandler
    {
        private readonly ILogger<AccountCreatedEventHandler> _logger;
        private readonly IAccountEventRepository _accountEventRepository;

        private readonly IAuthorizationRepository _authorizationRepository;

        private readonly IAuthorizationEventPublisher _authorizationEventPublisher;

        public AccountCreatedEventHandler(
       ILogger<AccountCreatedEventHandler> logger,
       IAccountEventRepository accountEventRepository,
       IAuthorizationRepository authorizationRepository,
       IAuthorizationEventPublisher authorizationEventPublisher)
        {
            _logger = logger;
            _accountEventRepository = accountEventRepository;
            _authorizationRepository = authorizationRepository;
            _authorizationEventPublisher = authorizationEventPublisher;
        }

        public async Task HandleAsync(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            var accountEvent = JsonConvert.DeserializeObject<AccountCreatedEvent>(message);
            _logger.LogInformation("Consommation de l'event type: {EventType}, accountId: {AccountId}",
            accountEvent?.EventType,
            accountEvent?.Data?.AccountId);

            if (accountEvent?.Data == null || accountEvent?.Data?.AccountId <= 0)
            {
                return;
            }

            var accountEntity = accountEvent!.Data.ToAccountEntity();
            await _accountEventRepository.CreateAccountAsync(accountEntity);
            _logger.LogInformation("L'entité avec l'identifiant: {AccountId} vient d'être ajoutée.", accountEvent!.Data.AccountId);

            var createdAuthorizations = await _authorizationRepository.CreateDefaultAuthorizationsOnAccountAsync(accountEntity.AccountId);

            await _authorizationEventPublisher.PublishAuthorizationUpdatedEventAsync(null!, accountEntity.AccountId, createdAuthorizations);
        }
    }
}
