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
    /// Récupère la configuration des authorizations d'un contact sur un account.
    /// </summary>
    /// <param name="contactId">Identifiant du contact.</param>
    /// <param name="accountId">Identifiant de l'entité morale.</param>
    /// <returns>La liste des configurations.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ConfigurationModel?>))]
    public async Task<ActionResult<IEnumerable<ConfigurationModel?>>> GetContactAccountConfigurationAsync([Required][FromQuery] int contactId, [FromQuery] int? accountId)
    {
        var configuration = await _configurationService.GetContactAccountConfigurationAsync(contactId, accountId);
        return Ok(configuration);
    }

    /// <summary>
    /// Créer ou modifier une authorization pour un contact sur une entité morale.
    /// </summary>
    /// <param name="contactId">Identifiant du contact.</param>
    /// <param name="accountId">Identifiant de l'entité.</param>
    /// <param name="codes">La liste des codes d'authorization du contact sur l'entité.</param>
    /// <returns>Http 200.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> CreateOrUpdateContactAccountAuthorizationAsync(int contactId, int? accountId, IList<string> codes)
    {
        await _configurationService.CreateOrUpdateContactAccountAuthorizationAsync(contactId, accountId, codes);
        return Ok();
    }
}
