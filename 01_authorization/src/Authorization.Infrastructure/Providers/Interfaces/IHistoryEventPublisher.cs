// <copyright file="IHistoryEventPublisher.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Authorization.Infrastructure.Providers.Interfaces;

public interface IHistoryEventPublisher
{
    Task PublishHistoryCreatedEvent(int currentUserId, int contactId, int accountId, IEnumerable<string> addedPermissionCodes, IEnumerable<string> deletedPermissionCodes);
}
