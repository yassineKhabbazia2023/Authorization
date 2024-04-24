// <copyright file="ConfigurationController.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Pulse.Authorization.Core.Interfaces;

namespace Pulse.Authorization.API.Controllers;

[Route("api/authorizations/configuration")]
[ApiController]
public class ConfigurationController : ControllerBase
{
}
