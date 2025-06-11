// <copyright file="AuthorizationController.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Pulse.Authorization.Core.Exceptions;
using Pulse.Authorization.Core.Interfaces;
using Pulse.Authorization.Core.Request;
using Pulse.ExceptionMiddleware.Exceptions;
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

    /// <summary>
    /// Récupérer des ContactId qui a permission particulier.
    /// </summary>
    /// <param name="codes">Listes des permissions.</param>
    /// <param name="pagination">Pagination params.</param>
    /// <returns>Listes des ContactId.</returns>
    [HttpGet("authorization/contacts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status204NoContent, Type = typeof(ErrorResponse))]
    public async Task<ActionResult<IEnumerable<string>>> GetContactIdsByAuthorizationCodes([FromQuery] List<string> codes, [FromQuery] Pagination? pagination)
    {
        var result = await _authorizationService.GetContactIdsByAuthorizationCodesAsync(codes, pagination);
        if(result.TotalItems == 0)
        {
            throw new NoContentException(Errors.NoContentCode, Errors.NoContentMessage);
        }

        return Ok(result);
    }
}
