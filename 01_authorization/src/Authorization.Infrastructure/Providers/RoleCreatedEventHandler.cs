// <copyright file="RoleCreatedEventHandler.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pulse.Authorization.Core.Interfaces;
using Pulse.Authorization.Infrastructure.Interfaces;
using Pulse.Authorization.Infrastructure.Mappers.EventMappers;
using Pulse.Authorization.Infrastructure.Providers.Interfaces;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;

namespace Pulse.Authorization.Infrastructure.Providers;

public class RoleCreatedEventHandler(
    ILogger<RoleCreatedEventHandler> logger,
    IRoleEventRepository roleEventRepository,
    IAuthorizationEventPublisher authorizationEventPublisher,
    IAuthorizationRepository authorizationRepository,
    IAuthorizationEventRepository authorizationEventRepository,
    IOnboardingEventPublisher onboardingEventPublisher,
    IContactEventRepository contactEventRepository,
    IAccountRepository accountRepository) : IEventHandler
{
    private readonly ILogger<RoleCreatedEventHandler> _logger = logger;
    private readonly IRoleEventRepository _roleEventRepository = roleEventRepository;
    private readonly IAuthorizationRepository _authorizationRepository = authorizationRepository;
    private readonly IAuthorizationEventRepository _authorizationEventRepository = authorizationEventRepository;
    private readonly IAuthorizationEventPublisher _authorizationEventPublisher = authorizationEventPublisher;
    private readonly IOnboardingEventPublisher _onboardingEventPublisher = onboardingEventPublisher;
    private readonly IContactEventRepository _contactEventRepository = contactEventRepository;
    private readonly IAccountRepository _accountRepository = accountRepository;

    public async Task HandleAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        var roleEvent = JsonConvert.DeserializeObject<RoleCreatedEvent>(message);
        _logger.LogInformation("Consommation de l'event type: {EventType}, contactId: {ContactId}, accountId: {AccountId}",
            roleEvent?.EventType,
            roleEvent?.Data?.ContactId,
            roleEvent?.Data?.AccountId);

        if (roleEvent?.Data == null || roleEvent.Data.ContactId <= 0 || roleEvent.Data.AccountId < -1)
        {
            return;
        }

        var roleEntity = roleEvent!.Data.ToRoleEntity();

        if (!await _roleEventRepository.DoesRoleExistAsync(roleEntity))
        {
            await _roleEventRepository.CreateRoleAsync(roleEntity);

            _logger.LogInformation("Le role de contact: {ContactId}, account: {AccountId} vient d'être crée.", roleEntity.ContactId, roleEntity.AccountId);

            var isCustomer = await _contactEventRepository.IsContactClientAsync(roleEntity.ContactId);

            if (roleEntity.IsSignatory.HasValue && roleEntity.IsSignatory.Value)
            {
                var createdAuthorizations = await _authorizationRepository.CreateDefaultAuthorizationsOnSignatoryAsync(roleEntity.ContactId, roleEntity.AccountId);

                await _authorizationEventPublisher.PublishAuthorizationUpdatedEventAsync(roleEntity.ContactId, roleEntity.AccountId, createdAuthorizations);
            }
            else if (isCustomer)
            {
                var createdAuthorizations = await _authorizationRepository.CreateDefaultAuthorizationsOnNonSignatoryAsync(roleEntity.ContactId, roleEntity.AccountId);

                await _authorizationEventPublisher.PublishAuthorizationUpdatedEventAsync(roleEntity.ContactId, roleEntity.AccountId, createdAuthorizations);
            }

            if (isCustomer && await _authorizationEventRepository.IsPennylaneActivatedAsync(roleEntity.AccountId))
            {
                var account = await _accountRepository.GetAccountByIdAsync(roleEntity.AccountId);
                await _onboardingEventPublisher.PublishOnBoardingEventAsync(roleEntity.ContactId, account.AccountNumber);
            }
        }
        else
        {
            _logger.LogInformation("Le role de contact: {ContactId}, account: {AccountId} existe déjà.", roleEntity.ContactId, roleEntity.AccountId);
        }
    }
}
