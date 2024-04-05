// <copyright file="Resource.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Authorization.Core.Models
{
    public class Resource
    {
        public int? ResourceId { get; set; }

        public string? ResourceName { get; set; }

        public int? ParentResourceId { get; set; }

        public IReadOnlyCollection<Action>? Actions { get; set; }

        public IReadOnlyCollection<Resource>? Childrens { get; set; }
    }
}
