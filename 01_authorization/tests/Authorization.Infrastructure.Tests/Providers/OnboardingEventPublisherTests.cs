// <copyright file="OnboardingEventPublisherTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Moq;
using Pulse.Authorization.Infrastructure.Providers;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;

namespace Pulse.Authorization.Infrastructure.Tests.Providers;

public class OnboardingEventPublisherTests
{
    [Fact]
    public async Task PublishOnBoardingEventAsync_Should_Invoke_PublishAsync()
    {
        // Arrange
        var mockEventPublisher = new Mock<IEventPublisher>();
        var onboardingEventPublisher = new OnboardingEventPublisher(mockEventPublisher.Object);

        // Act
        await onboardingEventPublisher.PublishOnBoardingEventAsync(1, "number");

        // Assert
        mockEventPublisher.Verify(ep => ep.PublishAsync(It.Is<OnboardingEvent>(e => e.Data.ContactId == 1), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }
}
