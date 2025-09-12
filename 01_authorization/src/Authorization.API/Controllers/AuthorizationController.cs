// <copyright file="AuthorizationController.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pulse.Authorization.Core.Exceptions;
using Pulse.Authorization.Core.Interfaces;
using Pulse.Authorization.Core.Models;
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
    /// Récupérer les utilisateurs qui ont ces codes de permissions.
    /// </summary>
    /// <param name="codes">Listes des permissions.</param>
    /// <param name="accountId">Identifiant account.</param>
    /// <param name="pagination">Pagination params.</param>
    /// <returns>Listes des Contacts.</returns>
    [HttpGet("authorization/contacts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status204NoContent, Type = typeof(ErrorResponse))]
    public async Task<ActionResult<IEnumerable<Contact>>> GetContactIdsByAuthorizationCodesAndAccountId([FromQuery] List<string> codes, [FromQuery] int accountId, [FromQuery] Pagination? pagination)
    {
        var result = await _authorizationService.GetContactIdsByAuthorizationCodesAndAccountIdAsync(codes, accountId, pagination);
        if(result.TotalItems == 0)
        {
            throw new NoContentException(Errors.NoContentCode, Errors.NoContentMessage);
        }

        return Ok(result);
    }

    /// <summary>
    /// Récupérer les utilisateurs signataires sur un account.
    /// </summary>
    /// <param name="accountId">Identifiant account.</param>
    /// <returns>Listes des contacts signataires sur un account.</returns>
    [HttpGet("authorization/contacts/signatory")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status204NoContent, Type = typeof(ErrorResponse))]
    public async Task<ActionResult<IEnumerable<Contact>>> GetContactIdsByAccountIdSignatoryAsync([FromQuery] int accountId)
    {
        var result = await _authorizationService.GetContactIdsByAccountIdSignatoryAsync(accountId);
        if (result.Count == 0)
        {
            throw new NoContentException(Errors.NoContentCode, Errors.NoContentMessage);
        }

        return Ok(result);
    }

    /// <summary>
    /// Attribuer une permission à une liste d'user.
    /// </summary>
    /// <param name="permission">Permission code.</param>
    /// <param name="fileCSV">Fichier csv des emails d'user.</param>
    /// <returns>Status code.</returns>
    /// <exception cref="BadRequestException">Fichier csv manquant.</exception>
    /// <exception cref="NotFoundException">Account ou content n'existe pas.</exception>
    [HttpPost("authorization/setPermission")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> SetPermissionForContactEmailAsync([Required][FromQuery] string permission, IFormFile fileCSV)
    {
        if (fileCSV == null || fileCSV.Length == 0)
        {
            throw new ArgumentNullException(Errors.NullArgumentCode, Errors.NullArgumentMessage);
        }

        await _authorizationService.SetPermissionForContactEmailAsync(permission, fileCSV);
        return Ok();
    }
}
