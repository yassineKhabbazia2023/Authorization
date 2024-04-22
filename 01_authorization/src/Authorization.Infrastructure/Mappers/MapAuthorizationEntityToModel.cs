// <copyright file="MapAuthorizationEntityToModel.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Core.Models;
using Action = Pulse.Authorization.Core.Models.Action;
using Pulse.Authorization.Core.Requests;
using System.Reflection.Metadata;
using Pulse.Authorization.Core.Constants;

namespace Pulse.Authorization.Infrastructure.Mappers
{
    public static class MapAuthorizationEntityToModel
    {
        public static NavigationRequest MapToNavigationRequest(this IEnumerable<ResourceEntity> source)
        {
            NavigationRequest? navigationRequest = new NavigationRequest
            {
                OverView = new List<Navigation>(),
                UnitView = new List<Navigation>(),
            };

            if (source == null)
            {
                return navigationRequest;
            }

            foreach (var sourceItem in source)
            {
                if (sourceItem.Category.Equals(Constants.ResourceTypeGlobale))
                {
                    navigationRequest.OverView.Add(sourceItem.MapToNavigation());
                }
                else if (sourceItem.Category.Equals(Constants.ResourceTypeUnitaire))
                {
                    navigationRequest.UnitView.Add(sourceItem.MapToNavigation());
                }
            }

            return navigationRequest;
        }

        public static Navigation MapToNavigation(this ResourceEntity source)
        {
            if (source == null)
            {
                return null!;
            }

            return new Navigation
            {
                Name = source.Name,
                Label = source.Label,
                Childrens = source.InverseParent.Select(c => c.MapToNavigation()),
            };
        }

        public static Action? MapToAction(this ActionEntity source)
        {
            if (source == null)
            {
                return null;
            }

            return new Action
            {
                ActionId = source.ActionId,
                Code = source.Code,
                Name = source.Name,
            };
        }
    }
}
