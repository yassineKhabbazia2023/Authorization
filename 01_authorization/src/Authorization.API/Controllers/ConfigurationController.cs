// <copyright file="ConfigurationController.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Pulse.Authorization.Core.Interfaces;
using Pulse.ExceptionMiddleware.Model;
using ConfigurationModel = Pulse.Authorization.Core.Models.Configuration;

namespace Pulse.Authorization.API.Controllers;

[Route("api/authorizations/configuration")]
[ApiController]
public class ConfigurationController : ControllerBase
{
    private readonly IConfigurationService _configurationService;

    public ConfigurationController(IConfigurationService configurationService)
    {
        _configurationService = configurationService;
    }

    /// <summary>
    /// Récupère la configuration des authorizations d'un contact sur un account.
    /// </summary>
    /// <param name="contactId">Identifiant du contact.</param>
    /// <param name="accountId">Identifiant de l'entité morale.</param>
    /// <returns>La liste des configurations.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ConfigurationModel?>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public async Task<ActionResult<IEnumerable<ConfigurationModel?>>> GetContactAccountConfigurationAsync([Required][FromQuery] int contactId, [FromQuery] int? accountId)
    {
        var configuration = await _configurationService.GetContactAccountConfigurationAsync(contactId, accountId);
        return Ok(configuration);
    }

    /// <summary>
    /// Récupère la configuration des authorizations d'un account.
    /// </summary>
    /// <param name="accountId">Identifiant de l'entité morale.</param>
    /// <param name="type">Le type de permissions gérées (optionnel).</param>
    /// <param name="configurable">Indique si on doit vérifier si les permissions sont configurables.</param>
    /// <returns>La liste des configurations.</returns>
    [HttpGet("account")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ConfigurationModel?>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public async Task<ActionResult<IEnumerable<ConfigurationModel?>>> GetAccountConfigurationAsync([FromQuery] int accountId, string? type = null, bool configurable = true)
    {
        var configuration = await _configurationService.GetAccountConfigurationAsync(accountId, type, configurable);
        return Ok(configuration);
    }

    /// <summary>
    /// Créer ou modifier une authorization pour un contact sur une entité morale.
    /// </summary>
    /// <param name="currentUserId">Identifiant du contact à l'origine de l'action.</param>
    /// <param name="contactId">Identifiant du contact.</param>
    /// <param name="accountId">Identifiant de l'entité.</param>
    /// <param name="codes">La liste des codes d'authorization du contact sur l'entité.</param>
    /// <returns>Http 200.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public async Task<ActionResult> CreateOrUpdateContactAccountAuthorizationAsync([FromHeader(Name = "CurrentUser")] int currentUserId, int contactId, int? accountId, IList<string> codes)
    {
        await _configurationService.CreateOrUpdateContactAccountAuthorizationAsync(currentUserId, contactId, accountId, codes);
        return Ok();
    }

    /// <summary>
    /// Créer ou modifier une authorization sur une entité morale.
    /// </summary>
    /// <param name="accountId">Identifiant de l'entité.</param>
    /// <param name="codes">La liste des codes d'authorization sur l'entité.</param>
    /// <param name="type">Le type de permissions gérées (optionnel).</param>
    /// <param name="configurable">Indique si on doit vérifier si les permissions sont configurables.</param>
    /// <returns>Http 200.</returns>
    [HttpPost("account")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public async Task<ActionResult> CreateOrUpdateAccountAuthorizationAsync(int accountId, IList<string> codes, string? type = null, bool configurable = true)
    {
        await _configurationService.CreateOrUpdateAccountAuthorizationAsync(accountId, codes, type, configurable);
        return Ok();
    }
}
