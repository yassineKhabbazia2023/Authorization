// <copyright file="RoleCreatedEventHandler.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pulse.Authorization.Infrastructure.Interfaces;
using Pulse.Authorization.Infrastructure.Mappers.EventMappers;
using Pulse.Authorization.Infrastructure.Providers.Interfaces;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;

namespace Pulse.Authorization.Infrastructure.Providers;

public class RoleCreatedEventHandler : IEventHandler
{
    private readonly ILogger<RoleCreatedEventHandler> _logger;
    private readonly IRoleEventRepository _roleEventRepository;
    private readonly IAuthorizationRepository _authorizationRepository;
    private readonly IAuthorizationEventPublisher _authorizationEventPublisher;

    public RoleCreatedEventHandler(
        ILogger<RoleCreatedEventHandler> logger,
        IRoleEventRepository roleEventRepository,
        IAuthorizationEventPublisher authorizationEventPublisher,
        IAuthorizationRepository authorizationRepository)
    {
        _logger = logger;
        _roleEventRepository = roleEventRepository;
        _authorizationEventPublisher = authorizationEventPublisher;
        _authorizationRepository = authorizationRepository;
    }

    public async Task HandleAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        var roleEvent = JsonConvert.DeserializeObject<RoleCreatedEvent>(message);
        _logger.LogInformation("Consommation de l'event type: {EventType}, contactId: {ContactId}, accountId: {AccountId}",
            roleEvent?.EventType,
            roleEvent?.Data?.ContactId,
            roleEvent?.Data?.AccountId);

        if (roleEvent?.Data == null || roleEvent.Data.ContactId <= 0 || roleEvent.Data.AccountId < -1)
        {
            return;
        }

        var roleEntity = roleEvent!.Data.ToRoleEntity();

        if (!await _roleEventRepository.DoesRoleExistAsync(roleEntity))
        {
            await _roleEventRepository.CreateRoleAsync(roleEntity);

            _logger.LogInformation("Le role de contact: {ContactId}, account: {AccountId} vient d'être crée.", roleEntity.ContactId, roleEntity.AccountId);

            if (roleEntity.IsSignatory.HasValue && roleEntity.IsSignatory.Value)
            {
                var createdAuthorizations = await _authorizationRepository.CreateDefaultAuthorizationsOnSignatoryAsync(roleEntity.ContactId, roleEntity.AccountId);

                await _authorizationEventPublisher.PublishAuthorizationUpdatedEventAsync(roleEntity.ContactId, roleEntity.AccountId, createdAuthorizations);
            }
        }
        else
        {
            _logger.LogInformation("Le role de contact: {ContactId}, account: {AccountId} existe déjà.", roleEntity.ContactId, roleEntity.AccountId);
        }
    }
}
