// <copyright file="IOnboardingEventPublisher.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Authorization.Infrastructure.Providers.Interfaces;

public interface IOnboardingEventPublisher
{
    Task PublishOnBoardingEventAsync(int contactId, string accountNumber);
}
