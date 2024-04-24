// <copyright file="ConfigurationService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Pulse.Authorization.Core.Exceptions;
using Pulse.Authorization.Core.Interfaces;
using Pulse.Authorization.Core.Requests;

namespace Pulse.Authorization.Core.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IAuthorizationRepository _authorizationRepository;
        private readonly IContactRepository _contactRepository;

        public ConfigurationService(IAuthorizationRepository authorizationRepository, IContactRepository contactRepository)
        {
            _authorizationRepository = authorizationRepository;
            _contactRepository = contactRepository;
        }

    }
}
