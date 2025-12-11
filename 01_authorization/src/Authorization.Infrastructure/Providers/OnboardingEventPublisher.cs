// <copyright file="OnboardingEventPublisher.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Authorization.Infrastructure.Providers.Interfaces;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;
using Pulse.Back.Events.IntegrationEvents.EventsData;

namespace Pulse.Authorization.Infrastructure.Providers;

public class OnboardingEventPublisher : IOnboardingEventPublisher
{
    private readonly IEventPublisher _eventPublisher;

    public OnboardingEventPublisher(IEventPublisher eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }

    public async Task PublishOnBoardingEventAsync(int contactId, string accountNumber)
    {
        var data = new OnboardingEventData
        {
            ContactId = contactId,
            AccountNumber = accountNumber,
        };

        var @event = new OnboardingEvent(data);

        await _eventPublisher.PublishAsync(@event);
    }
}
