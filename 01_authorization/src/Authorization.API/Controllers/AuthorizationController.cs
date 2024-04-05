// <copyright file="AuthorizationController.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Pulse.Authorization.Core.Interfaces;
using Pulse.Authorization.Core.Models;
using Action = Pulse.Authorization.Core.Models.Action;

namespace Pulse.Authorization.API.Controllers
{
    [Route("api/authorizations")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        private readonly IAuthorizationService _authorizationService;

        public AuthorizationController(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        /// <summary>
        /// Récupère la liste des authorizations possibles.
        /// </summary>
        /// <param name="accountId">Identifiant de l'entité morale.</param>
        /// <returns>Liste des authorizations possibles.</returns>
        [HttpGet()]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyCollection<Resource?>))]
        public async Task<ActionResult<IReadOnlyCollection<Resource?>>> GetRessourcesAsync(int accountId)
        {
            var result = await _authorizationService.GetAuthorizationsAsync(accountId);

            return Ok(result);
        }

        /// <summary>
        /// Récupère la liste des authorizations d'une entité morale.
        /// </summary>
        /// <param name="contactId">Identifiant du contat.</param>
        /// <param name="accountId">Identifiant de l'entité morale.</param>
        /// <returns>Liste des authorizations d'un contact au sein d'une entité morale.</returns>
        [HttpGet("{accountId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyCollection<Hub?>))]
        public async Task<ActionResult<IReadOnlyCollection<Resource?>>> GetAuthorizationsAsync(int contactId, int accountId)
        {

            return Ok();
        }

        /// <summary>
        /// Récupère la liste des authorizations d'un contact sur une entité morale.
        /// </summary>
        /// <param name="contactId">Identifiant du contat.</param>
        /// <param name="accountId">Identifiant de l'entité morale.</param>
        /// <returns>Liste des authorizations d'un contact au sein d'une entité morale.</returns>
        [HttpGet("{accountId}/{contactId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyCollection<Resource?>))]
        public async Task<ActionResult<IReadOnlyCollection<Resource?>>> GetAuthAsyncAsync(int contactId, int accountId)
        {

            return Ok();
        }

        /// <summary>
        /// Ajouter une authorization pour un contact sur une entité morale.
        /// </summary>
        /// <param name="contactId">Identifiant du contact.</param>
        /// <param name="accountId">Identifiant de l'entité morale.</param>
        /// <returns>Http 200.</returns>
        [HttpPost("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> CreateAuthorizationAsync(int contactId, int? accountId)
        {
            return Ok();
        }

        /// <summary>
        /// Supprimer une authorization pour un contact sur une entité morale.
        /// </summary>
        /// <param name="contactId">Identifiant du contact.</param>
        /// <param name="accountId">Identifiant de l'entité morale.</param>
        /// <param name="actionId">Identifiant de l'action.</param>
        /// <returns>Http 200.</returns>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DelteAuthorizationAsync(int contactId, int? accountId, int actionId)
        {
            return Ok();
        }
    }
}
