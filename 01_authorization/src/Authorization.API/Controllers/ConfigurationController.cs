// <copyright file="ConfigurationController.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Pulse.Authorization.Core.Interfaces;
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
    /// Récupère la configuration des authorizations.
    /// </summary>
    /// <param name="contactId">Identifiant du contat.</param>
    /// <param name="accountId">Identifiant de l'entité morale.</param>
    /// <returns>La liste des configurations.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ConfigurationModel?>))]
    public async Task<ActionResult<IEnumerable<ConfigurationModel?>>> GetConfigurationAsync([Required][FromQuery] int contactId, [Required][FromQuery] int accountId)
    {
        var configuration = await _configurationService.GetContactAccountConfigurationAsync(contactId, accountId);
        return Ok(configuration);
    }

    /// <summary>
    /// Modifier une authorization pour un contact sur une entité morale.
    /// </summary>
    /// <param name="contactId">Identifiant du contact.</param>
    /// <param name="personaId">Identifiant du persona.</param>
    /// <returns>Http 200.</returns>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult UpdateContactPersonaAsync(int contactId, int personaId)
    {
        return Ok();
    }
}
