// <copyright file="Pagination.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Authorization.Core.Request
{
    public class Pagination
    {
        public int PageNumber { get; set; } = default;

        public int PageSize { get; set; } = default;
    }
}
