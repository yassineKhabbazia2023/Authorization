// <copyright file="MapAuthorizationEntityToModel.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>
using Microsoft.Identity.Client;
using Pulse.Authorization.Core.Models;
using Pulse.Authorization.Core.Models.Subscriptions;
using Pulse.Authorization.Infrastructure.Entities;
using ActionModel = Pulse.Authorization.Core.Models.Action;

namespace Pulse.Authorization.Core.Mappers;

public static class MapAuthorizationEntityToModel
{
    // add test
    public static IEnumerable<Configuration> MapAuthorizationToConfiguration(this IEnumerable<AuthorizationEntity> sources)
    {
        return sources == null ? Enumerable.Empty<Configuration>() :
                       sources.GroupBy(auth => auth.Category)
                      .Select(auth => new Configuration
                      {
                          Category = auth.Key,
                          Actions = auth.Select(item => item.MapAuthorizationToAction())!
                      });
    }

    public static ActionModel? MapAuthorizationToAction(this AuthorizationEntity source)
    {
        return source == null ? null : new ActionModel
        {
            Name = source.Name,
            Label = source.Label,
            Code = source.Code,
            ActionId = source.AuthorizationId,
        };
    }

    public static IEnumerable<ContactAuthorizationEntity> MapToContactAuthorizationEntities(this ContactAuthorizationSubscription contactAuthorizationSubscription)
    {
        foreach(int contactId in contactAuthorizationSubscription.ContactIds)
        {
            foreach(int authorizationId in contactAuthorizationSubscription.AuthorizationIds)
            {
                yield return new ContactAuthorizationEntity { AccountId = contactAuthorizationSubscription.AccountId, ContactId = contactId, AuthorizationId = authorizationId, CreationDate = DateTime.UtcNow };
            }
        }
    }
}
