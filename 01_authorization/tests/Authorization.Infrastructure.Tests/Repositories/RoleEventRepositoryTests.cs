using Microsoft.EntityFrameworkCore;
using Pulse.Authorization.Core.Enum;
using Pulse.Authorization.Infrastructure.Context;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Repositories;

namespace Pulse.Authorization.Infrastructure.Tests.Repositories
{
    public class RoleEventRepositoryTests
    {
        [Fact]
        public async Task CreateRoleAsync_WithData_ShouldCreateRole()
        {
            var options = new DbContextOptionsBuilder<AuthorizationContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

            using var context = new AuthorizationContext(options);
            var repository = new RoleEventRepository(context);
            var roleEntity = new RoleEntity
            {
                ContactId = 1,
                AccountId = 2,
                IsDelegation = true,
                IsFavorite = true,
                IsSignatory = true
            };

            // Act
            await repository.CreateRoleAsync(roleEntity);

            // Assert
            var addedRole = await context.RoleEntity.FirstOrDefaultAsync();

            Assert.NotNull(addedRole);
            Assert.Equal(roleEntity.ContactId, addedRole.ContactId);
            Assert.Equal(roleEntity.AccountId, addedRole.AccountId);
            Assert.Equal(roleEntity.IsSignatory, addedRole.IsSignatory);
            Assert.Equal(roleEntity.IsDelegation, addedRole.IsDelegation);
            Assert.Equal(roleEntity.IsFavorite, addedRole.IsFavorite);
        }

        [Fact]
        public async Task UpdateRoleAsync_WithData_ShouldUpdateRole()
        {
            var options = new DbContextOptionsBuilder<AuthorizationContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

            using var context = new AuthorizationContext(options);
            var repository = new RoleEventRepository(context);
            var roleEntity = new RoleEntity
            {
                ContactId = 1,
                AccountId = 2,
                IsDelegation = true,
                IsFavorite = true,
                IsSignatory = true
            };

            await context.RoleEntity.AddAsync(roleEntity);
            await context.SaveChangesAsync();

            var modifiedRoleEntity = new RoleEntity
            {
                ContactId = 1,
                AccountId = 2,
                IsSignatory = false
            };

            // Act
            await repository.UpdateRoleAsync(modifiedRoleEntity);

            // Assert
            var updatedRole = await context.RoleEntity.FirstOrDefaultAsync();

            Assert.NotNull(updatedRole);
            Assert.Equal(modifiedRoleEntity.ContactId, updatedRole.ContactId);
            Assert.Equal(modifiedRoleEntity.AccountId, updatedRole.AccountId);
            Assert.Equal(modifiedRoleEntity.IsSignatory, updatedRole.IsSignatory);
        }

        [Fact]
        public async Task DeleteRoleAsync_WithData_ShouldDeleteRole()
        {
            var options = new DbContextOptionsBuilder<AuthorizationContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

            using var context = new AuthorizationContext(options);
            var repository = new RoleEventRepository(context);
            var roleEntity = new RoleEntity
            {
                ContactId = 1,
                AccountId = 2,
                IsDelegation = true,
                IsFavorite = true,
                IsSignatory = true
            };

            await context.RoleEntity.AddAsync(roleEntity);
            await context.SaveChangesAsync();

            // Act
            await repository.DeleteRoleAsync(contactId: 1, accountId: 2);

            // Assert
            var deletedRole = await context.RoleEntity.FirstOrDefaultAsync(x => x.AccountId == 2 && x.ContactId == 1);

            Assert.Null(deletedRole);
        }
    }
}
