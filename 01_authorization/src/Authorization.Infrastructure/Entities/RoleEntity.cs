// <copyright file="RoleEntity.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Authorization.Infrastructure.Entities
{
    public class RoleEntity
    {
        public int AccountId { get; set; }

        public AccountEntity? Account { get; set; }

        public int ContactId { get; set; }

        public ContactEntity? Contact { get; set; }

        public bool? IsFavorite { get; set; }

    /// <summary>
    /// Le signataire
    /// </summary>
        public bool? IsSignatory { get; set; }

    /// <summary>
    /// Indique, dans les cas où c&apos;&apos;est possible, si le role est lié à une délégation
    /// </summary>
        public bool? IsDelegation { get; set; }
    }
}
