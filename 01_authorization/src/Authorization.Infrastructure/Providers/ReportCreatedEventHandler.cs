// <copyright file="ReportCreatedEventHandler.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Pulse.Authorization.Core.Interfaces;
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
    private readonly IContactRepository _contactRepository;
    private readonly IAuthorizationEventPublisher _authorizationEventPublisher;

    public ReportCreatedEventHandler(ILogger<ReportCreatedEventHandler> logger, IAuthorizationRepository authorizationRepository, IContactRepository contactRepository, IAuthorizationEventPublisher authorizationEventPublisher)
    {
        _logger = logger;
        _authorizationRepository = authorizationRepository;
        _authorizationEventPublisher = authorizationEventPublisher;
        _contactRepository = contactRepository;
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

        var createdAuthorizations = await _authorizationRepository.CreateReportingAuthorizationsOnAccountAsync(reportEventData.AccountId, GlobalConstants.PowerBIDefaultPermissions);

        if (!createdAuthorizations.Any())
        {
            _logger.LogWarning("Les codes d'authorization reporting existent déjà pour l'account: {AccountId}", reportEventData.AccountId);
        }
        else
        {
            await _authorizationEventPublisher.PublishAuthorizationUpdatedEventAsync(null!, reportEventData.AccountId, createdAuthorizations);
        }

        var signatories = (await _contactRepository.GetSignatoriesAsync(reportEventData.AccountId)).ToList();
        if (signatories.Any())
        {
            var createdSignatoryAuthorizations = (await _authorizationRepository.CreateReportingAuthorizationsForSignatoriesAsync(signatories, reportEventData.AccountId)).ToList();

            if (!createdSignatoryAuthorizations.Any())
            {
                _logger.LogWarning("Les codes d'authorization reporting existent déjà pour le(s) signataire(s): {ContactIds}", signatories);
            }
            else
            {
                foreach (var result in createdSignatoryAuthorizations.GroupBy(c => c.Item1))
                {
                    var contactId = result.Key;
                    var codes = result.Select(r => r.Item2).Distinct();

                    await _authorizationEventPublisher.PublishAuthorizationCreatedEventAsync(contactId, reportEventData.AccountId, codes);
                }
            }
        }
    }
}
