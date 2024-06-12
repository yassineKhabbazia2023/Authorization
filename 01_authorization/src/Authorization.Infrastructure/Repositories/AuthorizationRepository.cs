// <copyright file="AuthorizationRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Pulse.Authorization.Infrastructure.Context;
using Pulse.Authorization.Infrastructure.Enum;
using Pulse.Authorization.Infrastructure.Extensions;
using Pulse.Authorization.Infrastructure.Interfaces;

namespace Pulse.Authorization.Infrastructure.Repositories;

public class AuthorizationRepository : IAuthorizationRepository
{
    private readonly AuthorizationContext _authorizationContext;

    public AuthorizationRepository(AuthorizationContext authorizationContext)
    {
        _authorizationContext = authorizationContext;
        _authorizationContext.HandleEFCoreFailure();
    }

    public async Task<List<string>> GetContactAccountAuthorizationsAsync(int contactId, int accountId, bool? viewGlobal)
    {
        var contactAuthorization = _authorizationContext
                    .ContactAuthorizationEntity
                    .Include(x => x.Authorization)
                    .Where(x => x.ContactId == contactId
                        && x.AccountId == accountId
                        && ((viewGlobal != null && x.Authorization.View != (viewGlobal.Value
                                                            ? AuthorizationView.Partial.ToString()
                                                            : AuthorizationView.Global.ToString()))
                             || viewGlobal == null))
                    .Select(x => x.Authorization.Code)
                    .Distinct();

        var result = await contactAuthorization.ToListAsync();
        return result;
    }

    public async Task<List<string>> GetAccountAuthorizationAsync(int accountId)
    {
        var contactAuthorizationCodes = _authorizationContext
                    .AccountAuthorizationEntity
                    .Include(x => x.Authorization)
                    .Where(x => x.AccountId == accountId)
                    .Select(x => x.Authorization.Code)
                    .Distinct();

        var result = await contactAuthorizationCodes.ToListAsync();
        return result;
    }

    public async Task<List<string>> GetContactAuthorizationAsync(int contactId)
    {
        var contactAuthorizationCodes = _authorizationContext
                    .ContactAuthorizationEntity
                    .Include(x => x.Authorization)
                    .Where(x => x.ContactId == contactId && x.Authorization.View != AuthorizationView.Partial.ToString())
                    .Select(x => x.Authorization.Code)
                    .Distinct();

        var result = await contactAuthorizationCodes.ToListAsync();
        return result;
    }

    public async Task DeleteContactAuthorizationAsync(int contactId, int accountId)
    {
        var permissions = _authorizationContext
                .ContactAuthorizationEntity
                .Where(x => x.ContactId == contactId && x.AccountId == accountId);

        _authorizationContext.ContactAuthorizationEntity.RemoveRange(permissions);
        await _authorizationContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<Entities.AccountAuthorizationEntity>> AddSubscriptionAuthorizationsOnAccountAsync(int accountId, IEnumerable<string> productCodes)
    {
        var authorizations = await _authorizationContext.AuthorizationEntity.AsNoTracking().ToListAsync();
        var range = authorizations.Where(a => productCodes.Contains(a.ProductCode))
            .DistinctBy(a => a.AuthorizationId).Select(a =>
        {
            return new Entities.AccountAuthorizationEntity
            {
                AccountId = accountId,
                AuthorizationId = a.AuthorizationId,
                Enabled = true,
            };
        });

        _authorizationContext.AccountAuthorizationEntity.AddRange(range);

        await _authorizationContext.SaveChangesAsync();
        return range;
    }

    public async Task<IEnumerable<Entities.ContactAuthorizationEntity>> AddSubscriptionAuthorizationsOnAccountSignatoriesAsync(int accountId, IEnumerable<int> contactIds, IEnumerable<string> productCodes)
    {
        var authorizations = await _authorizationContext.AuthorizationEntity.AsNoTracking().ToListAsync();
        var roles = (await _authorizationContext.RoleEntity.AsNoTracking()
               .Include(r => r.Contact).AsNoTracking()
               .Where(r => r.AccountId == accountId
               && contactIds.Contains(r.ContactId)
               && r.Contact!.Type == ContactType.Customer.ToString()
               && r.IsSignatory.HasValue && r.IsSignatory.Value).ToListAsync())
               .DistinctBy(r => r.ContactId);

        var toReturn = new List<Entities.ContactAuthorizationEntity>();
        foreach (var role in roles)
        {
            var range = authorizations.Where(a => productCodes.Contains(a.ProductCode)).DistinctBy(a => a.AuthorizationId).Select(a =>
            {
                return new Entities.ContactAuthorizationEntity
                {
                    AccountId = accountId,
                    AuthorizationId = a.AuthorizationId,
                    ContactId = role.ContactId,
                    CreationDate = DateTime.UtcNow,
                };
            });
            toReturn.AddRange(range);
        }

        _authorizationContext.ContactAuthorizationEntity.AddRange(toReturn);
        await _authorizationContext.SaveChangesAsync();
        return toReturn;
    }
}
