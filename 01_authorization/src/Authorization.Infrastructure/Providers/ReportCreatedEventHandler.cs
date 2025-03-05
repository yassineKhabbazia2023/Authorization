// <copyright file="AuthorizationCreatedEventHandler.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pulse.Authorization.Infrastructure.Constants;
using Pulse.Authorization.Infrastructure.Interfaces;
using Pulse.Authorization.Infrastructure.Providers.Interfaces;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;

namespace Pulse.Authorization.Infrastructure.Providers;

public class ReportCreatedEventHandler : IEventHandler
{
    private readonly ILogger<ReportCreatedEventHandler> _logger;
    private readonly IAuthorizationRepository _authorizationRepository;
    private readonly IAuthorizationEventPublisher _authorizationEventPublisher;

    public ReportCreatedEventHandler(ILogger<ReportCreatedEventHandler> logger, IAuthorizationRepository authorizationRepository, IAuthorizationEventPublisher authorizationEventPublisher)
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

        var reportEvent = JsonConvert.DeserializeObject<ReportCreatedEvent>(message);
        _logger.LogInformation("Consommation de l'event type: {EventType}, accountId: {AccountId}",
        reportEvent?.EventType,
        reportEvent?.Data?.AccountId);

        if (reportEvent?.Data == null || reportEvent?.Data?.AccountId <= 0)
        {
            return;
        }

        var reportEventData = reportEvent!.Data;
        var createdAuthorizations = await _authorizationRepository.CreateRapportBIAuthorizationsOnAccountAsync(reportEventData.AccountId, GlobalConstants.PowerBIDefaultPermissions);
        await _authorizationEventPublisher.PublishAuthorizationUpdatedEventAsync(null!, reportEventData.AccountId, createdAuthorizations);
    }
}
