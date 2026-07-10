// <copyright file="ConfigurationRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Authorization.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Extensions;
using Pulse.Authorization.Infrastructure.Interfaces;
using Pulse.ExceptionMiddleware.Exceptions;
using Pulse.Authorization.Core.Exceptions;
using Pulse.Authorization.Infrastructure.Constants;

namespace Pulse.Authorization.Infrastructure.Repositories;

public class ConfigurationRepository : IConfigurationRepository
{
    private readonly AuthorizationContext _authorizationContext;

    public ConfigurationRepository(AuthorizationContext authorizationContext)
    {
        _authorizationContext = authorizationContext;
        _authorizationContext.HandleEFCoreFailure();
    }

    public async Task<IEnumerable<AuthorizationEntity>> GetAccountAuthorizationsAsync(int accountId, string? type = null, bool configurable = true)
    {
        return await GetAccountAuthorizationsCoreAsync(accountId, type, configurable, targetAccountType: null);
    }

    public async Task<IEnumerable<AuthorizationEntity>> GetAccountAuthorizationsAsync(int accountId, string? type, bool configurable, string targetAccountType)
    {
        return await GetAccountAuthorizationsCoreAsync(accountId, type, configurable, ResolveTargetAccountType(targetAccountType));
    }

    private async Task<IEnumerable<AuthorizationEntity>> GetAccountAuthorizationsCoreAsync(int accountId, string? type, bool configurable, string? targetAccountType)
    {
        var query = _authorizationContext
                     .AccountAuthorizationEntity
                     .Include(x => x.Authorization)
                     .Where(x => x.AccountId == accountId);

        if (configurable)
        {
            query = query.Where(x => x.Authorization.Configurable == true);
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            query = query.Where(x => x.Authorization.Type == type);
        }

        var authorization = await ApplyTargetAccountTypeFilter(query.Select(x => x.Authorization), targetAccountType)
                     .Distinct()
                     .ToListAsync();

        return authorization;
    }

    public async Task<IEnumerable<AuthorizationEntity>> GetContactAuthorizationsAsync(int contactId, int accountId)
    {
        return await GetContactAuthorizationsCoreAsync(contactId, accountId, targetAccountType: null);
    }

    public async Task<IEnumerable<AuthorizationEntity>> GetContactAuthorizationsAsync(int contactId, int accountId, string targetAccountType)
    {
        return await GetContactAuthorizationsCoreAsync(contactId, accountId, ResolveTargetAccountType(targetAccountType));
    }

    private async Task<IEnumerable<AuthorizationEntity>> GetContactAuthorizationsCoreAsync(int contactId, int accountId, string? targetAccountType)
    {
        var query = _authorizationContext
                     .ContactAuthorizationEntity
                     .Include(x => x.Authorization)
                     .Where(x => x.ContactId == contactId
                            && x.AccountId == accountId
                            && x.Authorization.Configurable == true)
                     .Select(x => x.Authorization);

        var authorization = await ApplyTargetAccountTypeFilter(query, targetAccountType)
                     .Distinct()
                     .ToListAsync();

        return authorization;
    }

    public async Task<IEnumerable<AuthorizationEntity>> GetAuthorizationEntitiesByCodeAsync(IEnumerable<string> codes)
    {
        return await _authorizationContext
                     .AuthorizationEntity
                     .Where(x => codes.Contains(x.Code))
                     .Distinct()
                     .ToListAsync();
    }

    private async Task<IEnumerable<ContactAuthorizationEntity>> GetContactAccountConfigurationAsync(int contactId, int accountId)
    {
        return await _authorizationContext
                     .ContactAuthorizationEntity
                     .Include(x => x.Authorization)
                     .Where(x => x.ContactId == contactId && x.AccountId == accountId && x.Authorization.Configurable!.Value)
                     .Select(x => x)
                     .Distinct()
                     .ToListAsync();
    }

    private async Task<IEnumerable<AccountAuthorizationEntity>> GetAccountConfigurationAsync(int accountId, string? type = null, bool configurable = true)
    {
        var query = _authorizationContext
                     .AccountAuthorizationEntity
                     .Include(x => x.Authorization)
                     .Where(x => x.AccountId == accountId);

        if (configurable)
        {
            query = query.Where(x => x.Authorization.Configurable == true);
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            query = query.Where(x => x.Authorization.Type == type);
        }

        return await query.Distinct().ToListAsync();
    }

    private async Task DeleteContactAccountAuthorizationAsync(IEnumerable<ContactAuthorizationEntity> contactAuthorizations)
    {
        _authorizationContext.ContactAuthorizationEntity.RemoveRange(contactAuthorizations);
        await _authorizationContext.SaveChangesAsync();
    }

    private async Task DeleteAccountAuthorizationAsync(IEnumerable<AccountAuthorizationEntity> accountAuthorizations)
    {
        _authorizationContext.AccountAuthorizationEntity.RemoveRange(accountAuthorizations);
        await _authorizationContext.SaveChangesAsync();
    }

    public async Task CreateOrUpdateContactAccountAuthorizationAsync(int contactId, int accountId, IEnumerable<string> codes)
    {
        var authorizations = await GetAuthorizationEntitiesByCodeAsync(codes);

        var notConfigurable = authorizations.FirstOrDefault(a => a.Configurable == false);
        if (notConfigurable != null)
        {
            throw new BadRequestException(Errors.NotConfigurablePermissionCode, string.Format(Errors.NotConfigurablePermissionMessage, notConfigurable.Code));
        }

        var oldContactAuthorizationEntities = await GetContactAccountConfigurationAsync(contactId, accountId);

        if (oldContactAuthorizationEntities?.Any() == true)
        {
            await DeleteContactAccountAuthorizationAsync(oldContactAuthorizationEntities);
        }

        var authorizationIds = authorizations.Select(a => a.AuthorizationId);
        var newContactAuthorizationEntities = authorizationIds.Select(auth => new ContactAuthorizationEntity()
        {
            AccountId = accountId,
            ContactId = contactId,
            AuthorizationId = auth,
            CreationDate = DateTime.UtcNow
        });

        await _authorizationContext.ContactAuthorizationEntity.AddRangeAsync(newContactAuthorizationEntities);
        await _authorizationContext.SaveChangesAsync();
    }

    public async Task CreateOrUpdateAccountAuthorizationAsync(int accountId, IEnumerable<string> codes, string? type, bool configurableCheck = true)
    {
        var authorizations = await GetAuthorizationEntitiesByCodeAsync(codes);
        var accountType = await GetResolvedAccountTypeAsync(accountId);

        if (configurableCheck)
        {
            var notConfigurable = authorizations.FirstOrDefault(a => a.Configurable == false);
            if (notConfigurable != null)
            {
                throw new BadRequestException(Errors.NotConfigurablePermissionCode, string.Format(Errors.NotConfigurablePermissionMessage, notConfigurable.Code));
            }
        }

        ValidateAuthorizationsForAccountType(authorizations, accountType);

        var oldAccountAuthorizationEntities = await GetAccountConfigurationAsync(accountId, type, configurableCheck);

        if (oldAccountAuthorizationEntities?.Count() > 0)
        {
            await DeleteAccountAuthorizationAsync(oldAccountAuthorizationEntities);
        }

        var authorizationIds = authorizations.Select(a => a.AuthorizationId);
        var newAccountAuthorizations = authorizationIds.Select(auth => new AccountAuthorizationEntity()
        {
            AccountId = accountId,
            AuthorizationId = auth,
            Enabled = true,
        });

        await _authorizationContext.AccountAuthorizationEntity.AddRangeAsync(newAccountAuthorizations);
        await _authorizationContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<AuthorizationEntity>> GetAvailableAuthorizationsAsync(string? type, bool configurable = true)
    {
        var query = _authorizationContext.AuthorizationEntity.AsQueryable();
        if (configurable)
        {
            query = query.Where(a => a.Configurable == true);
        }

        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(a => a.Type == type);
        }

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<AuthorizationEntity>> GetAvailableAuthorizationsAsync(string? type, bool configurable, string targetAccountType)
    {
        targetAccountType = ResolveTargetAccountType(targetAccountType);

        var query = _authorizationContext.AuthorizationEntity.AsQueryable();
        if (configurable)
        {
            query = query.Where(a => a.Configurable == true);
        }

        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(a => a.Type == type);
        }

        query = ApplyTargetAccountTypeFilter(query, targetAccountType);

        return await query.ToListAsync();
    }

    /// <summary>
    /// Applies the target account type filter to an authorization query.
    /// </summary>
    /// <param name="query">The authorization query.</param>
    /// <param name="targetAccountType">The resolved target account type.</param>
    /// <returns>The filtered authorization query.</returns>
    private static IQueryable<AuthorizationEntity> ApplyTargetAccountTypeFilter(IQueryable<AuthorizationEntity> query, string? targetAccountType)
    {
        if (targetAccountType is null)
        {
            return query;
        }

        var includesAllTarget = targetAccountType == GlobalConstants.TargetAccountTypeClient
            || targetAccountType == GlobalConstants.TargetAccountTypeProspect;

        return query.Where(a => a.TargetAccountType == targetAccountType
            || (includesAllTarget && a.TargetAccountType == GlobalConstants.TargetAccountTypeAll));
    }

    /// <summary>
    /// Validates that account authorizations can be assigned to the target account type.
    /// </summary>
    /// <param name="authorizations">The authorizations to assign.</param>
    /// <param name="targetAccountType">The resolved target account type.</param>
    private static void ValidateAuthorizationsForAccountType(IEnumerable<AuthorizationEntity> authorizations, string targetAccountType)
    {
        var invalidAuthorization = authorizations.FirstOrDefault(a => !IsAuthorizationValidForAccountType(a, targetAccountType));
        if (invalidAuthorization is not null)
        {
            throw new BadRequestException(
                Errors.InvalidTargetAccountTypePermissionCode,
                string.Format(Errors.InvalidTargetAccountTypePermissionMessage, invalidAuthorization.Code, targetAccountType));
        }
    }

    /// <summary>
    /// Determines whether an authorization can be assigned to the target account type.
    /// </summary>
    /// <param name="authorization">The authorization to validate.</param>
    /// <param name="targetAccountType">The resolved target account type.</param>
    /// <returns>True when the authorization can be assigned; otherwise, false.</returns>
    private static bool IsAuthorizationValidForAccountType(AuthorizationEntity authorization, string targetAccountType)
    {
        return authorization.TargetAccountType == targetAccountType
            || ((targetAccountType == GlobalConstants.TargetAccountTypeClient
                    || targetAccountType == GlobalConstants.TargetAccountTypeProspect)
                && authorization.TargetAccountType == GlobalConstants.TargetAccountTypeAll);
    }

    /// <summary>
    /// Gets the resolved account type for an account identifier.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <returns>The resolved target account type.</returns>
    private async Task<string> GetResolvedAccountTypeAsync(int accountId)
    {
        var account = await _authorizationContext.AccountEntity
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AccountId == accountId);

        if (account is null)
        {
            throw new NotFoundException(Errors.NotFoundAccountCode, string.Format(Errors.NotFoundAccountMessage, accountId));
        }

        return ResolveTargetAccountType(account.AccountType);
    }

    /// <summary>
    /// Resolves unsupported or missing target account types to the historical client behavior.
    /// </summary>
    /// <param name="targetAccountType">The target account type to resolve.</param>
    /// <returns>The resolved target account type.</returns>
    private static string ResolveTargetAccountType(string? targetAccountType)
    {
        return string.Equals(targetAccountType, GlobalConstants.TargetAccountTypeProspect, StringComparison.OrdinalIgnoreCase)
            ? GlobalConstants.TargetAccountTypeProspect
            : GlobalConstants.TargetAccountTypeClient;
    }
}
