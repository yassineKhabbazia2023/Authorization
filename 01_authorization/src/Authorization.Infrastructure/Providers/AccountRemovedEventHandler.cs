// <copyright file="AccountRemovedEventHandler.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pulse.Authorization.Infrastructure.Providers.Interfaces;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;

namespace Pulse.Authorization.Infrastructure.Providers;

public class AccountRemovedEventHandler : IEventHandler
{
    private readonly ILogger<AccountRemovedEventHandler> _logger;
    private readonly IAccountEventRepository _accountEventRepository;

    public AccountRemovedEventHandler(
    ILogger<AccountRemovedEventHandler> logger,
    IAccountEventRepository accountEventRepository)
    {
        _logger = logger;
        _accountEventRepository = accountEventRepository;
    }

    public async Task HandleAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        var accountEvent = JsonConvert.DeserializeObject<AccountRemovedEvent>(message);
        _logger.LogInformation("Consommation de l'event type: {EventType}, accountId: {AccountId}",
        accountEvent?.EventType,
        accountEvent?.Data?.AccountId);

        if (accountEvent?.Data == null || accountEvent?.Data?.AccountId <= 0)
        {
            return;
        }

        await _accountEventRepository.RemoveAccountAsync(accountEvent!.Data.AccountId);
        await _accountEventRepository.RemoveAccountAuthorizationsAsync(accountEvent!.Data.AccountId);
        _logger.LogInformation("L'entité avec l'identifiant: {AccountId} vient d'être supprimée.", accountEvent!.Data.AccountId);
    }
}
