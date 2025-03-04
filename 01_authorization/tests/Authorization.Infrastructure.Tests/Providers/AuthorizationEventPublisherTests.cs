// <copyright file="AuthorizationEventPublisherTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using FluentAssertions;
using Moq;
using Pulse.Authorization.Infrastructure.Providers;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents.EventsData.BaseEventData;

namespace Pulse.Account.Infrastructure.Tests.Providers
{
    public class AccountEventPublisherTests
    {
        private readonly Fixture _fixture;

        public AccountEventPublisherTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public async Task PublishAuthorizationCreatedEventAsync_Should_PublishEvent()
        {
            // Arrange
            var publisherMock = new Mock<IEventPublisher>();
            var authorizationEventPublisher = new AuthorizationEventPublisher(publisherMock.Object);
            var codes = _fixture.Create<List<string>>();

            publisherMock.Setup(p => p.PublishAsync(It.IsAny<BaseEvent<AuthorizationEventData>>(), null!, null)).Callback<BaseEvent<AuthorizationEventData>, string, string>((@event, _, _) =>
            {
                @event.Data.Codes.Should().BeEquivalentTo(codes);
                @event.Data.AccountId.Should().Be(1);
                @event.Data.ContactId.Should().Be(1);
            }).Returns(Task.CompletedTask).Verifiable();

            // Act
            await authorizationEventPublisher.PublishAuthorizationCreatedEventAsync(1, 1, codes);

            // Assert
            publisherMock.Verify(p => p.PublishAsync(It.IsAny<BaseEvent<AuthorizationEventData>>(), null!, null), Times.Once);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task PublishAuthorizationUpdateddEventAsync_Should_PublishEvent(bool fromOffer)
        {
            // Arrange
            var publisherMock = new Mock<IEventPublisher>();
            var authorizationEventPublisher = new AuthorizationEventPublisher(publisherMock.Object);
            var codes = _fixture.Create<List<string>>();

            publisherMock.Setup(p => p.PublishAsync(It.IsAny<BaseEvent<AuthorizationEventData>>(), null!, null)).Callback<BaseEvent<AuthorizationEventData>, string, string>((@event, _, _) =>
            {
                @event.Data.Codes.Should().BeEquivalentTo(codes);
                @event.Data.AccountId.Should().Be(1);
                @event.Data.ContactId.Should().Be(1);
                @event.Data.FromOfferActivation.Should().Be(fromOffer);
            }).Returns(Task.CompletedTask).Verifiable();

            // Act
            await authorizationEventPublisher.PublishAuthorizationUpdatedEventAsync(1, 1, codes, fromOffer);

            // Assert
            publisherMock.Verify(p => p.PublishAsync(It.IsAny<BaseEvent<AuthorizationEventData>>(), null!, null), Times.Once);
        }
    }
}
