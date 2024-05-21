using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace Pulse.Authorization.Infrastructure.Entities
{
    public class RoleEntity
    {
        public int AccountId { get; set; }

        public AccountEntity? Account { get; set; }

        public int ContactId { get; set; }

        public ContactEntity? Contact { get; set; }

        public bool? IsFavorite { get; set; }

        public bool? IsSignatory { get; set; }

        public bool? IsDelegation { get; set; }
    }
}
