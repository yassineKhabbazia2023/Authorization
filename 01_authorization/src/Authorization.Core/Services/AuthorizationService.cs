// <copyright file="AuthorizationService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Pulse.Authorization.Core.Exceptions;
using Pulse.Authorization.Core.Interfaces;
using Pulse.Authorization.Core.Models;
using Pulse.Authorization.Core.Requests;
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

        public async Task<NavigationRequest> GetNavigationsAsync(int? accountId, int contactId)
        {
            var contact = await _contactRepository.GetContactByIdAsync(contactId);

            if (contact == null)
            {
                throw new NotFoundException(Errors.NotFoundContactCode, string.Format(Errors.NotFoundContactMessage, contactId));
            }

            if (accountId == null)
            {
                switch (contact!.Type)
                {
                    case Constants.Constants.ContactTypeCollab:
                        accountId = -1;
                        break;
                    case Constants.Constants.ContactTypeClient:
                        accountId = 0;
                        break;
                    default:
                        throw new NotFoundException(Errors.NotFoundContactTypeCode, string.Format(Errors.NotFoundContactTypeMessage, contactId, contact!.Type));
                }
            }

            return await _authorizationRepository.GetNavigationsAsync(accountId, contact);
        }
    }
}
