// <copyright file="AuthorizationEventPublisher.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Authorization.Infrastructure.Providers.Interfaces;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents.Events.Authorization;
using Pulse.Back.Events.IntegrationEvents.EventsData.BaseEventData;

namespace Pulse.Authorization.Infrastructure.Providers;

public class AuthorizationEventPublisher : IAuthorizationEventPublisher
{
    private readonly IEventPublisher _eventPublisher;

    public AuthorizationEventPublisher(IEventPublisher eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }

    public async Task PublishAuthorizationCreatedEventAsync(int contactId, int accountId, IEnumerable<string> codes)
    {
        var eventData = new AuthorizationEventData
        {
            AccountId = accountId,
            ContactId = contactId,
            Codes = codes.ToList(),
        };

        var @event = new AuthorizationCreatedEvent(eventData);
        await _eventPublisher.PublishAsync(@event);
    }

    public async Task PublishAuthorizationUpdatedEventAsync(int? contactId, int accountId, IEnumerable<string> codes, bool fromOffer = false)
    {
        var eventData = new AuthorizationEventData
        {
            AccountId = accountId,
            ContactId = contactId,
            Codes = codes.ToList(),
            FromOfferActivation = fromOffer,
        };

        var @event = new AuthorizationUpdatedEvent(eventData);
        await _eventPublisher.PublishAsync(@event);
    }
}
