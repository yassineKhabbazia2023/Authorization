// <copyright file="ContactCreatedEventHandler.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;
using Pulse.Authorization.Infrastructure.Mappers.EventMappers;
using Pulse.Authorization.Infrastructure.Providers.Interfaces;

namespace Pulse.Authorization.Infrastructure.Providers;

public class ContactCreatedEventHandler : IEventHandler
{
    private readonly ILogger<ContactCreatedEventHandler> _logger;
    private readonly IContactEventRepository _contactEventRepository;

    public ContactCreatedEventHandler(
        ILogger<ContactCreatedEventHandler> logger,
        IContactEventRepository contactEventRepository)
    {
        _logger = logger;
        _contactEventRepository = contactEventRepository;
    }

    public async Task HandleAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        var contactEvent = JsonConvert.DeserializeObject<ContactCreatedEvent>(message);
        _logger.LogInformation("Consommation de l'event type: {EventType}, contactId: {ContactId}",
            contactEvent?.EventType,
            contactEvent?.Data?.ContactId);

        if (contactEvent?.Data == null || contactEvent.Data.ContactId <= 0)
        {
            return;
        }

        var contactEntity = contactEvent!.Data.ToContactEntity();

        if (!await _contactEventRepository.DoesContactExistAsync(contactEntity.ContactId))
        {
            await _contactEventRepository.CreateContactAsync(contactEntity!);

            _logger.LogInformation("Le contact avec l'identifiant: {ContactId} vient d'être crée.", contactEntity.ContactId);
        }
        else
        {
            _logger.LogInformation("Le contact avec l'identifiant: {ContactId} existe déjà.", contactEntity.ContactId);
        }
    }
}
