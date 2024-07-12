// <copyright file="AccountUpdatedEventHandler.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pulse.Authorization.Infrastructure.Mappers.EventMappers;
using Pulse.Authorization.Infrastructure.Providers.Interfaces;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;

namespace Pulse.Authorization.Infrastructure.Providers
{
    public class AccountUpdatedEventHandler : IEventHandler
    {
        private readonly ILogger<AccountUpdatedEventHandler> _logger;
        private readonly IAccountEventRepository _accountEventRepository;

        public AccountUpdatedEventHandler(
       ILogger<AccountUpdatedEventHandler> logger,
       IAccountEventRepository accountEventRepository)
        {
            _logger = logger;
            _accountEventRepository = accountEventRepository;
        }

        public async Task HandleAsync(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            var accountEvent = JsonConvert.DeserializeObject<AccountUpdatedEvent>(message);
            _logger.LogInformation("Consommation de l'event type: {EventType}, accountId: {AccountId}",
            accountEvent?.EventType,
            accountEvent?.Data?.AccountId);

            if (accountEvent?.Data == null || accountEvent?.Data?.AccountId <= 0)
            {
                return;
            }

            var accountEntity = accountEvent!.Data.ToAccountEntity();
            await _accountEventRepository.UpdateAccountAsync(accountEntity);

            _logger.LogInformation("L'entité avec l'identifiant: {AccountId} vient d'être modifiée.", accountEvent!.Data.AccountId);
        }
    }
}
