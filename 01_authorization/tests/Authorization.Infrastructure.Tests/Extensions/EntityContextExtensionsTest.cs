// <copyright file="EntityContextExtensionsTest.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Pulse.Authorization.Infrastructure.Context;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Enum;
using Pulse.Authorization.Infrastructure.Extensions;

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
            catch (Exception)
            {
            }

            // Assert
            Assert.Empty(context.ChangeTracker.Entries());
        }
    }
}
