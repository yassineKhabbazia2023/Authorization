// <copyright file="AuthorizationCreatedEventHandler.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pulse.Authorization.Infrastructure.Interfaces;
using Pulse.Authorization.Infrastructure.Providers.Interfaces;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;

namespace Pulse.Authorization.Infrastructure.Providers;

public class AuthorizationCreatedEventHandler : IEventHandler
{
    private readonly ILogger<AuthorizationCreatedEventHandler> _logger;
    private readonly IAuthorizationRepository _authorizationRepository;
    private readonly IAuthorizationEventPublisher _authorizationEventPublisher;

    public AuthorizationCreatedEventHandler(ILogger<AuthorizationCreatedEventHandler> logger, IAuthorizationRepository authorizationRepository, IAuthorizationEventPublisher authorizationEventPublisher)
    {
        _logger = logger;
        _authorizationRepository = authorizationRepository;
        _authorizationEventPublisher = authorizationEventPublisher;
    }

    public async Task HandleAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        var authorizationEvent = JsonConvert.DeserializeObject<AuthorizationCreatedEvent>(message);
        _logger.LogInformation("Consommation de l'event type: {EventType}, accountId: {AccountId}",
        authorizationEvent?.EventType,
        authorizationEvent?.Data?.AccountId);

        if (authorizationEvent?.Data == null || authorizationEvent?.Data?.AccountId <= 0)
        {
            return;
        }

        var authorizationEventData = authorizationEvent!.Data;

        _logger.LogInformation("Ajout des permissions : {AuthorizationCode} sur l'entité: {AccountId}.", string.Join(" - ", authorizationEventData.Codes), authorizationEventData.AccountId);

        var createdAuthorizations = await _authorizationRepository.CreateRapportBIAuthorizationsOnAccountAsync(authorizationEventData.AccountId, authorizationEventData.Codes);

        _logger.LogInformation("Les permissions : {AuthorizationCode} viennent d'être ajoutées sur l'entité: {AccountId}.", string.Join(" - ", authorizationEventData.Codes), authorizationEventData.AccountId);

        await _authorizationEventPublisher.PublishAuthorizationUpdatedEventAsync(null!, authorizationEventData.AccountId, createdAuthorizations);
    }
}
