// <copyright file="Resource.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Authorization.Core.Models
{
    public class Resource
    {
        public int? ResourceId { get; set; }

        public string? Name { get; set; }

        public string? Category { get; set; }

        public string? Label { get; set; }

        public int? ParentResourceId { get; set; }

        public IEnumerable<Action?>? Actions { get; set; }

        public IEnumerable<Resource?>? Childrens { get; set; }
    }
}
