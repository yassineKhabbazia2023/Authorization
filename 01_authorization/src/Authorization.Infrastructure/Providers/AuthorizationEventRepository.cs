// <copyright file="AuthorizationEventRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Pulse.Authorization.Infrastructure.Constants;
using Pulse.Authorization.Infrastructure.Context;
using Pulse.Authorization.Infrastructure.Providers.Interfaces;

namespace Pulse.Authorization.Infrastructure.Providers;

public class AuthorizationEventRepository : IAuthorizationEventRepository
{
    private readonly AuthorizationContext _authorizationContext;

    public AuthorizationEventRepository(AuthorizationContext authorizationContext)
    {
        _authorizationContext = authorizationContext;
    }

    public async Task<bool> IsPennylaneActivatedAsync(int accountId)
    {
        var auths = await _authorizationContext.AccountAuthorizationEntity
            .AsNoTracking()
            .Include(a => a.Authorization)
            .Where(a => a.AccountId == accountId && a.Account.IsActive && GlobalConstants.PennylaneCodes.Contains(a.Authorization.Code))
            .ToListAsync();

        return auths?.Count > 0;
    }
}
