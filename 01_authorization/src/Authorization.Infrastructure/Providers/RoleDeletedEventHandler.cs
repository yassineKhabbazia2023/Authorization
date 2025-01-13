// <copyright file="RoleDeletedEventHandler.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pulse.Authorization.Infrastructure.Providers.Interfaces;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;

namespace Pulse.Authorization.Infrastructure.Providers;

public class RoleDeletedEventHandler : IEventHandler
{
    private readonly ILogger<RoleDeletedEventHandler> _logger;
    private readonly IRoleEventRepository _roleEventRepository;

    public RoleDeletedEventHandler(
        ILogger<RoleDeletedEventHandler> logger,
        IRoleEventRepository roleEventRepository)
    {
        _logger = logger;
        _roleEventRepository = roleEventRepository;
    }

    public async Task HandleAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        var roleEvent = JsonConvert.DeserializeObject<RoleDeletedEvent>(message);
        _logger.LogInformation("Consommation de l'event type: {EventType}, contactId: {ContactId}",
            roleEvent?.EventType,
            roleEvent?.Data?.ContactId);

        if (roleEvent?.Data == null || roleEvent?.Data?.ContactId <= 0 || roleEvent?.Data.AccountId < -1)
        {
            return;
        }

        await _roleEventRepository.DeleteRoleAsync(roleEvent!.Data.ContactId, roleEvent!.Data.AccountId);

        _logger.LogInformation("Le role de contact l'identifiant: {ContactId}, accountId: {AccountId} vient d'être supprimé.", roleEvent!.Data.ContactId, roleEvent!.Data.AccountId);
    }
}
