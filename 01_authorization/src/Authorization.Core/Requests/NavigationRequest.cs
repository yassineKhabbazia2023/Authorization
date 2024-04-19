// <copyright file="NavigationRequest.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Authorization.Core.Models;

namespace Pulse.Authorization.Core.Requests;

public class NavigationRequest
{
    public required List<Navigation> OverView { get; set; }

    public required List<Navigation> UnitView { get; set; }
}
