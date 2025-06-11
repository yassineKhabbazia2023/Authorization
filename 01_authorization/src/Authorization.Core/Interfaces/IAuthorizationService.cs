// <copyright file="IAuthorizationService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Pulse.Authorization.Core.Models.Utils;
using Pulse.Authorization.Core.Request;

namespace Pulse.Authorization.Core.Interfaces;

public interface IAuthorizationService
{
    Task<List<string>> GetContactAuthorizationAsync(int contactId, int? accountId);

    Task DeleteContactAuthorizationAsync(int contactId, int? accountId);

    Task<List<string>> GetAllContactAuthorizationsAsync(int contactId);

    Task<Paging<int>> GetContactIdsByAuthorizationCodesAsync(List<string> codes, Pagination? pagination);
}
