// <copyright file="SwaggerModel.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace Pulse.Authorization.API.Configuration.Model;

[ExcludeFromCodeCoverage]
public class SwaggerModel
{
    public string UiEndpoint { get; set; } = null!;

    public string JsonEndpoint { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Version { get; set; } = null!;

    public string ContactName { get; set; } = null!;

    public string ContactEmail { get; set; } = null!;

    public string LicenseName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public Uri TermsOfService { get; set; } = null!;

    public string RouteTemplate { get; set; } = null!;
}
