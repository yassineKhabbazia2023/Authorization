// <copyright file="RoleEventRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Pulse.Authorization.Infrastructure.Context;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Enum;
using Pulse.Authorization.Infrastructure.Repositories;

namespace Pulse.Authorization.Infrastructure.Tests.Repositories;

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
        Assert.NotNull(updatedRole.LastUpdateDate);
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

    [Fact]
    public async Task DeleteContactRolesAsync_ShouldDeleteAllRolesForContact()
    {
        var options = new DbContextOptionsBuilder<AuthorizationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using var context = new AuthorizationContext(options);
        var repository = new RoleEventRepository(context);
        var role1 = new RoleEntity
        {
            ContactId = 1,
            AccountId = 2,
            IsDelegation = true,
            IsFavorite = true,
            IsSignatory = true
        };
        var role2 = new RoleEntity
        {
            ContactId = 1,
            AccountId = 4,
            IsDelegation = false,
            IsFavorite = false,
            IsSignatory = true
        };

        await context.RoleEntity.AddRangeAsync(new List<RoleEntity> { role1, role2 });
        await context.SaveChangesAsync();

        await repository.DeleteContactRolesAsync(1);

        var result = await context.RoleEntity.Where(r => r.ContactId == 1).ToListAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task DoesRoleExistAsync_WithExistingRole_ShouldReturnTrue()
    {
        var options = new DbContextOptionsBuilder<AuthorizationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using var context = new AuthorizationContext(options);

        var contact = new ContactEntity
        {
            ContactId = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            Status = ContactStatus.Declared.ToString(),
            Type = "Collaborator",
            PersonaName = "None"
        };
        context.ContactEntity.Add(contact);

        var account = new AccountEntity
        {
            AccountId = 1,
            AccountGlobalUniqueId = Guid.NewGuid(),
            AccountNumber = "number",
            LegalName = "legal",
            Status = "Invited",
            CreationDate = DateTime.UtcNow,
        };
        context.AccountEntity.Add(account);

        var role = new RoleEntity
        {
            AccountId = 1,
            ContactId = 1,
        };
        context.RoleEntity.Add(role);
        await context.SaveChangesAsync();

        var repository = new RoleEventRepository(context);

        var result = await repository.DoesRoleExistAsync(role);

        Assert.True(result);
    }

    [Fact]
    public async Task DoesRoleExistAsync_WithNoExistingRole_ShouldReturnFalse()
    {
        var options = new DbContextOptionsBuilder<AuthorizationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using var context = new AuthorizationContext(options);

        var contact = new ContactEntity
        {
            ContactId = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            Status = ContactStatus.Declared.ToString(),
            Type = "Collaborator",
            PersonaName = "None"
        };
        context.ContactEntity.Add(contact);

        var account = new AccountEntity
        {
            AccountId = 1,
            AccountGlobalUniqueId = Guid.NewGuid(),
            AccountNumber = "number",
            LegalName = "legal",
            Status = "Invited",
            CreationDate = DateTime.UtcNow,
        };
        context.AccountEntity.Add(account);
        await context.SaveChangesAsync();

        var repository = new RoleEventRepository(context);

        var result = await repository.DoesRoleExistAsync(new RoleEntity { AccountId = 1, ContactId = 1 });

        Assert.False(result);
    }

    [Theory]
    [MemberData(nameof(AccountAndContact))]
    public async Task DoesRoleExistAsync_WithNoExistingContactOrAccount_ShouldThrowInvalidOperationException(int accountId, int contactId)
    {
        var options = new DbContextOptionsBuilder<AuthorizationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using var context = new AuthorizationContext(options);

        var contact = new ContactEntity
        {
            ContactId = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            Status = ContactStatus.Declared.ToString(),
            Type = "Collaborator",
            PersonaName = "None"
        };
        context.ContactEntity.Add(contact);

        var account = new AccountEntity
        {
            AccountId = 1,
            AccountGlobalUniqueId = Guid.NewGuid(),
            AccountNumber = "number",
            LegalName = "legal",
            Status = "Invited",
            CreationDate = DateTime.UtcNow,
        };
        context.AccountEntity.Add(account);
        await context.SaveChangesAsync();

        var repository = new RoleEventRepository(context);

        var result = await Assert.ThrowsAsync<InvalidOperationException>(async () => await repository.DoesRoleExistAsync(new RoleEntity { AccountId = accountId, ContactId = contactId }));

        Assert.Equal("L'entité ou le contact n'existe pas", result.Message);
    }

    public static IEnumerable<object[]> AccountAndContact()
    {
        yield return new object[] { 1, 2 };
        yield return new object[] { 2, 1 };
        yield return new object[] { 2, 2 };
    }
}
