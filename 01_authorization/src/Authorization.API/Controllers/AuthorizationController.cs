// <copyright file="AuthorizationController.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Pulse.Authorization.Core.Interfaces;
using Pulse.ExceptionMiddleware.Model;

namespace Pulse.Authorization.API.Controllers;

[Route("api")]
[ApiController]
public class AuthorizationController : ControllerBase
{
    private readonly IAuthorizationService _authorizationService;

    public AuthorizationController(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Récupère la liste des codes associés à un contact.
    /// </summary>
    /// <param name="contactId">Identifiant du contact.</param>
    /// <returns>Liste des codes.</returns>
    [HttpGet("authorizations")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<string>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public async Task<ActionResult<IList<string>>> GetAllContactAuthorizationsAsync([Required][FromQuery] int contactId)
    {
        var result = await _authorizationService.GetAllContactAuthorizationsAsync(contactId);

        return Ok(result);
    }

    /// <summary>
    /// Récupère la liste des menus authorisés par contact.
    /// </summary>
    /// <param name="contactId">Identifiant du contat.</param>
    /// <param name="accountId">Identifiant de l'entité morale.</param>
    /// <returns>Liste des codes de menu.</returns>
    [HttpGet("authorization")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<string>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
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
    [HttpDelete("authorization")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public async Task<ActionResult> DeleteContactAuthorizationAsync([Required][FromQuery] int contactId, [FromQuery] int? accountId)
    {
        await _authorizationService.DeleteContactAuthorizationAsync(contactId, accountId);

        return Ok();
    }
}
