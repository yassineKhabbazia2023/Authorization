// <copyright file="AuthorizationRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Pulse.Authorization.Infrastructure.Constants;
using Pulse.Authorization.Infrastructure.Context;
using Pulse.Authorization.Infrastructure.Entities;
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
                    .AsNoTracking()
                    .Include(x => x.Authorization)
                    .Include(x => x.Contact)
                    .Where(x => x.Authorization.Type == x.Contact.Type)
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
                    .AsNoTracking()
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
                    .AsNoTracking()
                    .Include(x => x.Authorization)
                    .Where(x => x.ContactId == contactId && x.Authorization.View != AuthorizationView.Partial.ToString())
                    .Include(x => x.Contact)
                    .Where(x => x.Contact.Type == x.Authorization.Type)
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

    public async Task<IEnumerable<AccountAuthorizationEntity>> AddSubscriptionAuthorizationsOnAccountAsync(int accountId, IEnumerable<string> productCodes)
    {
        var authorizations = await _authorizationContext.AuthorizationEntity.ToListAsync();
        var authorizationsByProductCode = authorizations.Where(a => a.ProductCode != null && productCodes.Contains(a.ProductCode));
        var accountAuthorizationsByProductCode = authorizationsByProductCode
            .DistinctBy(a => a.AuthorizationId).Select(a =>
        {
            return new AccountAuthorizationEntity
            {
                AccountId = accountId,
                Authorization = a,
                Enabled = true,
            };
        });

        var filteredAccountAuthorizations = accountAuthorizationsByProductCode.Where(r => !_authorizationContext.AccountAuthorizationEntity.Any(a => a.AuthorizationId == r.Authorization.AuthorizationId
            && a.AccountId == r.AccountId)).ToList();

        _authorizationContext.AccountAuthorizationEntity.AddRange(filteredAccountAuthorizations);

        await _authorizationContext.SaveChangesAsync();
        filteredAccountAuthorizations.AddRange(await AddMirrorAuthorizationsOnAccountAsync(accountId, authorizationsByProductCode));

        return filteredAccountAuthorizations;
    }

    private async Task<IEnumerable<AccountAuthorizationEntity>> AddMirrorAuthorizationsOnAccountAsync(int accountId, IEnumerable<AuthorizationEntity> authorizations)
    {
        var mirrorCodes = authorizations.Where(a => GlobalConstants.CustomerToMirrorCodes.ContainsKey(a.Code))
            .Select(a =>
            {
               return GlobalConstants.CustomerToMirrorCodes[a.Code];
            });

        var authorizationCodes = await _authorizationContext.AuthorizationEntity.Where(a => mirrorCodes.Contains(a.Code)).ToListAsync();

        var accountAuthorizations = authorizationCodes.Select(a =>
        {
            return new AccountAuthorizationEntity
            {
                AccountId = accountId,
                Authorization = a,
                Enabled = true,
            };
        });

        var filteredAccountAuthorizations = accountAuthorizations.Where(r => !_authorizationContext.AccountAuthorizationEntity.Any(a => a.AuthorizationId == r.Authorization.AuthorizationId
            && a.AccountId == r.AccountId));

        _authorizationContext.AccountAuthorizationEntity.AddRange(filteredAccountAuthorizations);

        await _authorizationContext.SaveChangesAsync();

        return filteredAccountAuthorizations;
    }

    public async Task<IEnumerable<ContactAuthorizationEntity>> AddSubscriptionAuthorizationsOnAccountSignatoriesAsync(int accountId, IEnumerable<string> productCodes)
    {
        var authorizations = await _authorizationContext.AuthorizationEntity.ToListAsync();
        var roles = (await _authorizationContext.RoleEntity.AsNoTracking()
               .Include(r => r.Contact).AsNoTracking()
               .Where(r => r.AccountId == accountId
               && r.Contact!.Type == ContactType.Customer.ToString()
               && r.IsSignatory.HasValue && r.IsSignatory.Value).ToListAsync())
               .DistinctBy(r => r.ContactId);

        var toReturn = new List<ContactAuthorizationEntity>();
        foreach (var role in roles)
        {
            var range = authorizations.Where(a => a.ProductCode != null && productCodes.Contains(a.ProductCode)).DistinctBy(a => a.AuthorizationId).Select(a =>
            {
                return new ContactAuthorizationEntity
                {
                    AccountId = accountId,
                    Authorization = a,
                    ContactId = role.ContactId,
                    CreationDate = DateTime.UtcNow,
                };
            });
            toReturn.AddRange(range);
        }

        toReturn = toReturn.Distinct().ToList();
        var filtered = toReturn.Where(r => !_authorizationContext.ContactAuthorizationEntity.Any(a => a.AuthorizationId == r.Authorization.AuthorizationId
            && a.AccountId == r.AccountId
            && a.ContactId == r.ContactId));

        _authorizationContext.ContactAuthorizationEntity.AddRange(filtered);
        await _authorizationContext.SaveChangesAsync();
        return toReturn;
    }

    public async Task<IEnumerable<string>> CreateDefaultAuthorizationsOnAccountAsync(int accountId)
    {
        var authorizations = await _authorizationContext.AuthorizationEntity.AsNoTracking().Where(a => GlobalConstants.DefaultAccountPermissions.Contains(a.Code)).Distinct().ToListAsync();
        _authorizationContext.AccountAuthorizationEntity.AddRange(authorizations.Where(a => !a.AccountAuthorizationEntity.Any(ac => ac.AuthorizationId == a.AuthorizationId && ac.AccountId == accountId))
            .Select(a =>
            {
                return new AccountAuthorizationEntity
                {
                    AccountId = accountId,
                    AuthorizationId = a.AuthorizationId,
                    Enabled = true,
                };
            }));

        await _authorizationContext.SaveChangesAsync();

        return GlobalConstants.DefaultAccountPermissions;
    }

    public async Task<IEnumerable<string>> CreateRapportBIAuthorizationsOnAccountAsync(int accountId, List<string> codes)
    {
        var authorizations = await _authorizationContext.AuthorizationEntity.AsNoTracking().Where(a => codes.Contains(a.Code)).Distinct().ToListAsync();
        _authorizationContext.AccountAuthorizationEntity.AddRange(authorizations.Where(a => !a.AccountAuthorizationEntity.Any(ac => ac.AuthorizationId == a.AuthorizationId && ac.AccountId == accountId))
            .Select(a =>
            {
                return new AccountAuthorizationEntity
                {
                    AccountId = accountId,
                    AuthorizationId = a.AuthorizationId,
                    Enabled = true,
                };
            }));

        await _authorizationContext.SaveChangesAsync();

        return codes;
    }

    public async Task<IEnumerable<string>> CreateDefaultAuthorizationsOnSignatoryAsync(int contactId, int accountId)
    {
        var authorizations = await _authorizationContext.AuthorizationEntity.AsNoTracking().Where(a => GlobalConstants.DefaultSignatoryPermissions.Contains(a.Code)).Distinct().ToListAsync();
        var filtered = authorizations.Where(a => !_authorizationContext.ContactAuthorizationEntity.Any(c => c.AuthorizationId == a.AuthorizationId
        && c.ContactId == contactId
        && c.AccountId == accountId));

        _authorizationContext.ContactAuthorizationEntity.AddRange(filtered.Select(a =>
            {
                return new ContactAuthorizationEntity
                {
                    ContactId = contactId,
                    AccountId = accountId,
                    AuthorizationId = a.AuthorizationId,
                    CreationDate = DateTime.UtcNow,
                };
            }));

        await _authorizationContext.SaveChangesAsync();

        return GlobalConstants.DefaultSignatoryPermissions;
    }
}
