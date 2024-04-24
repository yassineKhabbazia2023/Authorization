// <copyright file="AdministrationController.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Pulse.Authorization.Core.Interfaces;
using ConfigurationModel = Pulse.Authorization.Core.Models.Configuration;

namespace Pulse.Authorization.API.Controllers;

[Route("api/authorizations/administration")]
[ApiController]
public class AdministrationController : ControllerBase
{
    private readonly IConfigurationService _configurationService;

    public AdministrationController(IConfigurationService configurationService)
    {
        _configurationService = configurationService;
    }

    /// <summary>
    /// Récupère la configuration des authorizations.
    /// </summary>
    /// <param name="contactId">Identifiant du contat.</param>
    /// <param name="accountId">Identifiant de l'entité morale.</param>
    /// <returns>La liste des configurations.</returns>
    [HttpGet("")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ConfigurationModel?>))]
    public async Task<ActionResult<IEnumerable<ConfigurationModel?>>> GetConfigurationAsync([Required][FromQuery] int contactId, [FromQuery] int? accountId)
    {
        //var result = await _configurationService.GetConfigurationAsync(contactId, accountId);

        return Ok();
    }

    /// <summary>
    /// Ajouter une authorization pour un contact sur une entité morale.
    /// </summary>
    /// <param name="contactId">Identifiant du contact.</param>
    /// <param name="accountId">Identifiant de l'entité morale.</param>
    /// <param name="configurations">Liste des configurations.</param>
    /// <returns>Http 200.</returns>
    [HttpPost("")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> CreateConfigurationAsync(int contactId, int? accountId, [FromBody] IEnumerable<ConfigurationModel> configurations)
    {
        return Ok();
    }

    /// <summary>
    /// Modifier une authorization pour un contact sur une entité morale.
    /// </summary>
    /// <param name="contactId">Identifiant du contact.</param>
    /// <param name="personaId">Identifiant du persona.</param>
    /// <param name="accountId">Identifiant de l'entité morale.</param>
    /// <returns>Http 200.</returns>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateConfigurationPersonaAsync(int contactId, int personaId, int? accountId)
    {
        return Ok();
    }
}
