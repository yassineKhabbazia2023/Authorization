// <copyright file="AuthorizationRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Linq;
using System.Linq.Expressions;
using Azure.Core.Pipeline;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Prng;
using Pulse.Authorization.Core.Exceptions;
using Pulse.Authorization.Core.Extensions;
using Pulse.Authorization.Core.Models.Subscriptions;
using Pulse.Authorization.Core.Models.Utils;
using Pulse.Authorization.Core.Request;
using Pulse.Authorization.Infrastructure.Constants;
using Pulse.Authorization.Infrastructure.Context;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Enum;
using Pulse.Authorization.Infrastructure.Extensions;
using Pulse.Authorization.Infrastructure.Interfaces;
using Pulse.Authorization.Infrastructure.Mappers;
using Pulse.ExceptionMiddleware.Exceptions;

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

            var existingContactAuthorization = await _authorizationContext.ContactAuthorizationEntity.AsNoTracking()
                                                                                   .Include(c => c.Authorization).AsNoTracking()
                                                                                   .Where(c => c.ContactId == role.ContactId && c.AccountId == accountId)
                                                                                   .ToListAsync();

            toReturn.AddRange(range);
            toReturn.AddRange(existingContactAuthorization);
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

    public async Task<IEnumerable<string>> CreateReportingAuthorizationsOnAccountAsync(int accountId, string[] codes)
    {
        var authorizations = await _authorizationContext
            .AuthorizationEntity
            .Include(a => a.AccountAuthorizationEntity)
            .Where(a => codes.Contains(a.Code))
            .Distinct()
            .ToListAsync();

        var newAuthorizations = authorizations
            .Where(a => !a.AccountAuthorizationEntity
                         .Any(ac => ac.AccountId == accountId
                                    && ac.AuthorizationId == a.AuthorizationId))
            .ToList();

        var newAccountAuths = newAuthorizations.Select(a => new AccountAuthorizationEntity
        {
            AccountId = accountId,
            AuthorizationId = a.AuthorizationId,
            Enabled = true
        });

        _authorizationContext.AccountAuthorizationEntity.AddRange(newAccountAuths);
        await _authorizationContext.SaveChangesAsync();

        return newAuthorizations.Select(a => a.Code);
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

    public async Task SetContactAuthorizationFromAccountAuthorization(int accountId, int contactId)
    {
        if (accountId == default)
        {
            throw new NullArgumentException(Errors.NullArgumentCode, string.Format(Errors.NullArgumentMessage, nameof(accountId)));
        }

        if (contactId == default)
        {
            throw new NullArgumentException(Errors.NullArgumentCode, string.Format(Errors.NullArgumentMessage, nameof(contactId)));
        }

        var accountAuthorizations = this.GetAuthorizationIdsOfAccount(accountId);
        var existedContactAuthorizations = this.GetAuthorizationIdsOfContact(accountId, contactId);
        var deltaContactAuthorizations = accountAuthorizations.Except(existedContactAuthorizations);

        IEnumerable<ContactAuthorizationEntity> contactAuthorizations = deltaContactAuthorizations.Select(authorizationId => new ContactAuthorizationEntity
        {
            AccountId = accountId,
            ContactId = contactId,
            AuthorizationId = authorizationId,
            CreationDate = DateTime.UtcNow
        }).AsEnumerable();

        if (contactAuthorizations.Any())
        {
            await _authorizationContext.ContactAuthorizationEntity.AddRangeAsync(contactAuthorizations);
            await _authorizationContext.SaveChangesAsync();
        }
    }

    private IEnumerable<int> GetAuthorizationIdsOfAccount(int accountId)
    {
        if (accountId == default)
        {
            throw new NullArgumentException(Errors.NullArgumentCode, string.Format(Errors.NullArgumentMessage, nameof(accountId)));
        }

        var authorizationIds = _authorizationContext.AccountAuthorizationEntity
            .Include(a => a.Authorization)
            .Where(a => a.AccountId == accountId && a.Authorization.Type == AuthorizationType.Customer)
            .Select(a => a.AuthorizationId).AsEnumerable();

        return authorizationIds;
    }

    private IEnumerable<int> GetAuthorizationIdsOfContact(int accountId, int contactId)
    {
        if (accountId == default)
        {
            throw new NullArgumentException(Errors.NullArgumentCode, string.Format(Errors.NullArgumentMessage, nameof(accountId)));
        }

        if (contactId == default)
        {
            throw new NullArgumentException(Errors.NullArgumentCode, string.Format(Errors.NullArgumentMessage, nameof(contactId)));
        }

        var contactAuthorizationIds = _authorizationContext.ContactAuthorizationEntity
            .Where(c => c.AccountId == accountId && c.ContactId == contactId)
            .Select(a => a.AuthorizationId).Distinct().AsEnumerable();

        return contactAuthorizationIds;
    }

    public async Task DeleteContactAuthorizationsAsync(int contactId, int accountId, string[] permissions)
    {
        if (contactId == default(int))
        {
            throw new NullArgumentException(Errors.NullArgumentCode, string.Format(Errors.NullArgumentMessage, nameof(contactId)));
        }

        if (accountId == default(int))
        {
            throw new NullArgumentException(Errors.NullArgumentCode, string.Format(Errors.NullArgumentMessage, nameof(accountId)));
        }

        if (permissions.Count() == 0)
        {
            throw new NullArgumentException(Errors.NullArgumentCode, string.Format(Errors.NullArgumentMessage, nameof(permissions)));
        }

        var authorizationIds = _authorizationContext.AuthorizationEntity
            .AsNoTracking()
            .Where(authorization => permissions.Contains(authorization.Code))?
            .Select(auth => auth.AuthorizationId).ToArray();

        var contactAuthorizationsToDelete = _authorizationContext.ContactAuthorizationEntity
            .Where(contactAuth => authorizationIds.Contains(contactAuth.AuthorizationId) && contactAuth.ContactId == contactId && contactAuth.AccountId == accountId)
            .ToList();

        if (contactAuthorizationsToDelete is not null && contactAuthorizationsToDelete.Count() > 0)
        {
            _authorizationContext.ContactAuthorizationEntity.RemoveRange(contactAuthorizationsToDelete);
            await _authorizationContext.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<(int, string)>> CreateReportingAuthorizationsForSignatoriesAsync(IEnumerable<int> contactIds, int accountId)
    {
        var authorizations = await _authorizationContext
            .AuthorizationEntity
            .Include(a => a.ContactAuthorizationEntity)
            .Where(a => GlobalConstants.PowerBIDefaultSignatoryPermissions.Contains(a.Code))
            .Distinct()
            .ToListAsync();

        var newContactAuthorizations = authorizations
            .SelectMany(auth => contactIds
                .Where(contactId => !auth.ContactAuthorizationEntity
                    .Any(ca => ca.ContactId == contactId
                               && ca.AccountId == accountId
                               && ca.AuthorizationId == auth.AuthorizationId))
                .Select(contactId => new ContactAuthorizationEntity
                {
                    ContactId = contactId,
                    AccountId = accountId,
                    AuthorizationId = auth.AuthorizationId,
                    CreationDate = DateTime.UtcNow,
                    Authorization = auth
                }))
            .ToList();

        _authorizationContext.ContactAuthorizationEntity.AddRange(newContactAuthorizations);
        await _authorizationContext.SaveChangesAsync();

        return newContactAuthorizations.Select(a => (a.ContactId, a.Authorization.Code));
    }

    public async Task<List<string>> GetAllContactAuthorizationsAsync(int contactId)
    {
        return await _authorizationContext
            .ContactAuthorizationEntity
            .AsNoTracking()
            .Include(ca => ca.Authorization)
            .Include(ca => ca.Account)
            .Include(ca => ca.Contact)
            .Where(ca => ca.ContactId == contactId && ca.Contact.IsActive && ca.Account.IsActive)
            .Select(x => x.Authorization.Code)
            .Distinct().ToListAsync();
    }

    public async Task<Paging<int>> GetContactIdsByAuthorizationCodesAndAccountIdAsync(List<string> codes, int accountId, Pagination? pagination)
    {
        var authorizationIdRequired = _authorizationContext.AuthorizationEntity
                                        .AsNoTracking()
                                        .Where(a => codes.Contains(a.Code))
                                        .Select(a => a.AuthorizationId);
        if (await authorizationIdRequired.CountAsync() != codes.Count)
        {
            throw new NotFoundException(Errors.NotFoundPermissionCode, string.Format(Errors.NotFoundPermissionMessage, string.Join(',', codes)));
        }

        var contactsHasRoles = _authorizationContext.RoleEntity
                            .AsNoTracking()
                            .Where(ca => ca.AccountId == accountId)
                            .Select(ca => ca.ContactId);

        var query = _authorizationContext.ContactAuthorizationEntity
                        .AsNoTracking()
                        .Where(ca => authorizationIdRequired.Contains(ca.AuthorizationId))
                        .GroupBy(ca => ca.ContactId)
                        .Where(g => g.Select(ca => ca.AuthorizationId).Distinct().Count() == codes.Count)
                        .Select(g => g.Key);

        var results = query.Intersect(contactsHasRoles);

        var totalItems = await results.CountAsync();
        var totalPages = Paginator.GetTotalPages(totalItems, pagination!.PageSize);

        results = results.Skip((pagination!.PageNumber - 1) * pagination!.PageSize);
        results = results.Take(pagination!.PageSize);

        return new Paging<int>
        {
            Items = await results.ToListAsync(),
            CurrentPage = pagination!.PageNumber,
            TotalItems = totalItems,
            TotalPage = totalPages
        };
    }

    public async Task SetPermissionByContactEmailAsync(string permission, List<string> emails)
    {
        var authorization = await _authorizationContext.AuthorizationEntity.FirstOrDefaultAsync(x => x.Code == permission)
            ?? throw new NotFoundException(Errors.NotFoundPermissionCode, Errors.NotFoundPermissionMessage);

        var contactIds = _authorizationContext.ContactEntity.Where(c => emails.Contains(c.Email) && c.Type == ContactType.Collaborator.ToString()).Select(c => c.ContactId);
        if (contactIds.IsNullOrEmpty())
        {
            throw new NotFoundException(Errors.NotFoundContactCode, Errors.NotFoundContactMessage);
        }

        List<ContactAuthorizationEntity> contactAuthorizations =[];
        foreach (var contactId in contactIds)
        {
            var contactAuthorizationToAdd = new ContactAuthorizationEntity
            {
                AccountId = -1,
                AuthorizationId = authorization.AuthorizationId,
                ContactId = contactId,
                CreationDate = DateTime.UtcNow,
            };
            if (!_authorizationContext.ContactAuthorizationEntity.Contains(contactAuthorizationToAdd))
            {
                contactAuthorizations.Add(contactAuthorizationToAdd);
            }
        }

        await _authorizationContext.ContactAuthorizationEntity.AddRangeAsync(contactAuthorizations);
        await _authorizationContext.SaveChangesAsync();
    }

    public ProductCodesSubscriptions RetrieveExistedProductCodes(IEnumerable<string> productCodes)
    {
        var authorizations = _authorizationContext.AuthorizationEntity.AsNoTracking().Where(x => productCodes.Contains(x.ProductCode));
        ProductCodesSubscriptions productCodesSubscriptions = new ProductCodesSubscriptions
        {
            ClientAuthorizationIds = authorizations.Where(x => x.Type == ContactType.Customer.ToString()).Select(x => x.AuthorizationId).AsEnumerable(),
            CollabAuthorizationIds = authorizations.Where(x => x.Type == ContactType.Collaborator.ToString()).Select(x => x.AuthorizationId).AsEnumerable(),
            UnexistedCodes = productCodes.Except(authorizations.Select(x => x.ProductCode)).AsEnumerable()
        };

        return productCodesSubscriptions;
    }

    public async Task<IEnumerable<ContactAuthorizationEntity>> AddSubscriptionAuthorizationOnAccountContactsAsync(IEnumerable<ContactAuthorizationEntity> contactAuthorizationEntities)
    {
        var notExistedContactAuthorizations = contactAuthorizationEntities
            .Where(ca => !_authorizationContext.ContactAuthorizationEntity.AsNoTracking()
        .Any(db => db.ContactId == ca.ContactId && db.AccountId == ca.AccountId && db.AuthorizationId == ca.AuthorizationId))
            .ToList();

        if (notExistedContactAuthorizations.Any())
        {
            await _authorizationContext.ContactAuthorizationEntity.AddRangeAsync(notExistedContactAuthorizations);
            await _authorizationContext.SaveChangesAsync();
        }

        return notExistedContactAuthorizations;
    }
}
