// <copyright file="RoleUpdatedEventHandler.cs" company="Pulse">
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
    public class RoleUpdatedEventHandler : IEventHandler
    {
        private readonly ILogger<RoleUpdatedEventHandler> _logger;
        private readonly IRoleEventRepository _roleEventRepository;
        private readonly IAuthorizationRepository _authorizationRepository;

        public RoleUpdatedEventHandler(
        ILogger<RoleUpdatedEventHandler> logger,
        IRoleEventRepository roleEventRepository,
        IAuthorizationRepository authorizationRepository)
        {
            _logger = logger;
            _roleEventRepository = roleEventRepository;
            _authorizationRepository = authorizationRepository;
        }

        public async Task HandleAsync(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            var roleEvent = JsonConvert.DeserializeObject<RoleUpdatedEvent>(message);
            _logger.LogInformation("Consommation de l'event type: {EventType}, contactId: {ContactId}, accountId: {AccountId}",
                roleEvent?.EventType,
                roleEvent?.Data?.ContactId,
                roleEvent?.Data?.AccountId);

            if (roleEvent?.Data == null || roleEvent?.Data?.ContactId <= 0 || roleEvent?.Data.AccountId < -1)
            {
                return;
            }

            var roleEntity = roleEvent!.Data.ToRoleEntity();

            await _roleEventRepository.UpdateRoleAsync(roleEntity!);

            if (roleEntity.IsSignatory == true)
            {
                await _authorizationRepository.SetContactAuthorizationFromAccountAuthorization(roleEntity.AccountId, roleEntity.ContactId);
            }

            _logger.LogInformation("Le role de contact l'identifiant: {ContactId} et account: {AccountId} vient d'être modifié.", roleEntity.ContactId, roleEntity.AccountId);
        }
    }
}
