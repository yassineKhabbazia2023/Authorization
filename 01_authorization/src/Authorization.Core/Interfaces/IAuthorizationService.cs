// <copyright file="IAuthorizationService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Authorization.Core.Interfaces;

public interface IAuthorizationService
{
    Task<IList<string>> GetContactAuthorization(int contactId, int? accountId);
}
