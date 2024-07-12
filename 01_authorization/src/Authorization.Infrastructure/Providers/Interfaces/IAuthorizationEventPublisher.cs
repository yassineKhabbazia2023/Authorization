// <copyright file="IAuthorizationEventPublisher.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Back.Events;
using Pulse.Back.Events.IntegrationEvents;

namespace Pulse.Authorization.Infrastructure.Providers.Interfaces;

public interface IAuthorizationEventPublisher
{
    Task PublishAuthorizationCreatedEventAsync(int contactId, int accountId, IEnumerable<string> codes);

    Task PublishAuthorizationUpdatedEventAsync(int? contactId, int accountId, IEnumerable<string> codes);
}
