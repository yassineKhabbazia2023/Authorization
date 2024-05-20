// <copyright file="AuthorizationController.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Pulse.Authorization.Core.Interfaces;

namespace Pulse.Authorization.API.Controllers;

[Route("api/authorization")]
[ApiController]
public class AuthorizationController : ControllerBase
{
    private readonly IAuthorizationService _authorizationService;

    public AuthorizationController(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Récupère la liste des menus authorisés par contact.
    /// </summary>
    /// <param name="contactId">Identifiant du contat.</param>
    /// <param name="accountId">Identifiant de l'entité morale.</param>
    /// <returns>Liste des codes de menu.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<string>))]
    public async Task<ActionResult<IList<string>>> GetContactAuthorizationAsync([Required][FromQuery] int contactId, [FromQuery] int? accountId)
    {
        var result = await _authorizationService.GetContactAuthorizationAsync(contactId, accountId);

        return Ok(result);
    }

    /// <summary>
    /// Supprimer des permissions d'un contact dans un account.
    /// </summary>
    /// <param name="contactId">Identifiant du contat.</param>
    /// <param name="accountId">Identifiant de l'entité morale.</param>
    /// <returns>Status code.</returns>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> DeleteContactAuthorizationAsync([Required][FromQuery] int contactId, [FromQuery] int? accountId)
    {
        await _authorizationService.DeleteContactAuthorizationAsync(contactId, accountId);

        return Ok();
    }
}
