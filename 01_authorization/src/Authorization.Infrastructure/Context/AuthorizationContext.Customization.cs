using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pulse.Authorization.Infrastructure.Entities;

namespace Pulse.Authorization.Infrastructure.Context
{
    /// <summary>
    /// All customization for entities should be made here 
    /// because we have database first approach and the main AuthorizationContext 
    /// is auto generated.
    /// </summary>
    public partial class AuthorizationContext
    {
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            // accepted status are Invited Connected Removed Declared
            // modelBuilder.Entity<ContactEntity>(builder => builder.HasQueryFilter(contact => contact.Status.ToLower() != "removed"));
        }
    }
}
