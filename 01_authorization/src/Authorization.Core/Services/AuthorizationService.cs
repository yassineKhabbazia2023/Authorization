// <copyright file="AuthorizationService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Pulse.Authorization.Core.Exceptions;
using Pulse.Authorization.Core.Interfaces;
using Pulse.Authorization.Core.Models;
using Action = Pulse.Authorization.Core.Models.Action;

namespace Pulse.Authorization.Core.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly IAuthorizationRepository _authorizationRepository;
        private readonly IContactRepository _contactRepository;

        public AuthorizationService(IAuthorizationRepository authorizationRepository, IContactRepository contactRepository)
        {
            _authorizationRepository = authorizationRepository;
            _contactRepository = contactRepository;
        }

        public async Task<IEnumerable<Resource>> GetResourceByAccountIdAsync(int? accountId, int contactId, string categoryName)
        {
            var contact = await _contactRepository.GetContactByIdAsync(contactId);

            if (contact == null)
            {
                throw new NotFoundException(Errors.NotFoundContactCode, string.Format(Errors.NotFoundContactMessage, contactId));
            }

            if (contact!.Type!.Equals(Constants.Constants.CONTACTTYPECOLLAB) && accountId == null)
            {
                accountId = -1;
            }

            return await _authorizationRepository.GetResourceByAccountIdAsync(accountId, contact, categoryName);
        }
    }
}
