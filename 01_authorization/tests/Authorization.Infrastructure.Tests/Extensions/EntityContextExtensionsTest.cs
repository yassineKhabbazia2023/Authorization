using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Pulse.Authorization.Core.Enum;
using Pulse.Authorization.Infrastructure.Context;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Extensions;
using Pulse.Authorization.Infrastructure.Repositories;

namespace Pulse.Authorization.Infrastructure.Tests.Extensions
{
    public class EntityContextExtensionsTest
    {
        [Fact]
        public virtual async Task HandleEFCoreFailure_ShouldClearChangeTracker()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AuthorizationContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;
            var context = new AuthorizationContext(options);
            var contactEntity = new ContactEntity
            {
                ContactId = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                Status = ContactStatus.Declared.ToString(),
                Type = "Collaborator",
                PersonaName = "None"
            };

            // Act
            context.HandleEFCoreFailure();
            context.ContactEntity.Add(contactEntity);
            await context.SaveChangesAsync();
            try
            {
                context.ContactEntity.Add(contactEntity);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
            }

            // Assert
            Assert.Empty(context.ChangeTracker.Entries());
        }
    }
}
