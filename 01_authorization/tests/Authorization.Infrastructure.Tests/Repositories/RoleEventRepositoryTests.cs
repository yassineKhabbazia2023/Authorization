// <copyright file="RoleEventRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Pulse.Authorization.Core.Exceptions;
using Pulse.Authorization.Core.Models.Subscriptions;
using Pulse.Authorization.Infrastructure.Context;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Enum;
using Pulse.Authorization.Infrastructure.Repositories;

namespace Pulse.Authorization.Infrastructure.Tests.Repositories;

public class RoleEventRepositoryTests
{
    private Fixture _fixture;

    public RoleEventRepositoryTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

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
            PersonaName = "None",
            IsActive = true
        };
        context.ContactEntity.Add(contact);

        var account = new AccountEntity
        {
            AccountId = 1,
            AccountGlobalUniqueId = Guid.NewGuid(),
            AccountNumber = "number",
            LegalName = "legal",
            Status = "Invited",
            AccountType = null,
            IsActive = true,
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
            PersonaName = "None",
            IsActive = true
        };
        context.ContactEntity.Add(contact);

        var account = new AccountEntity
        {
            AccountId = 1,
            AccountGlobalUniqueId = Guid.NewGuid(),
            AccountNumber = "number",
            LegalName = "legal",
            Status = "Invited",
            AccountType = null,
            IsActive = true,
            CreationDate = DateTime.UtcNow,
        };
        context.AccountEntity.Add(account);
        await context.SaveChangesAsync();

        var repository = new RoleEventRepository(context);

        var result = await repository.DoesRoleExistAsync(new RoleEntity { AccountId = 1, ContactId = 1 });

        Assert.False(result);
    }

    [Fact]
    public async Task DoesRoleExistAsync_WithNoExistingContact_ShouldThrowInvalidOperationException()
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
            PersonaName = "None",
            IsActive = true
        };
        context.ContactEntity.Add(contact);

        var account = new AccountEntity
        {
            AccountId = 1,
            AccountGlobalUniqueId = Guid.NewGuid(),
            AccountNumber = "number",
            LegalName = "legal",
            IsActive = true,
            AccountType = null,
            Status = "Invited",
            CreationDate = DateTime.UtcNow,
        };
        context.AccountEntity.Add(account);
        await context.SaveChangesAsync();

        var repository = new RoleEventRepository(context);

        var action = async () => await repository.DoesRoleExistAsync(new RoleEntity { AccountId = account.AccountId, ContactId = 3 });

        var result = await action.Should().ThrowAsync<Pulse.ExceptionMiddleware.Exceptions.InvalidOperationException>();

        result.Which.Code.Should().Be(Errors.NotFoundContactCode);
        result.WithMessage("Le contact avec l'identifiant 3 est introuvable");
    }

    [Fact]
    public async Task DoesRoleExistAsync_WithNoExistingAccount_ShouldThrowInvalidOperationException()
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
            PersonaName = "None",
            IsActive = true
        };
        context.ContactEntity.Add(contact);

        var account = new AccountEntity
        {
            AccountId = 1,
            AccountGlobalUniqueId = Guid.NewGuid(),
            AccountNumber = "number",
            LegalName = "legal",
            IsActive = true,
            AccountType = null,
            Status = "Invited",
            CreationDate = DateTime.UtcNow,
        };
        context.AccountEntity.Add(account);
        await context.SaveChangesAsync();

        var repository = new RoleEventRepository(context);

        var action = async () => await repository.DoesRoleExistAsync(new RoleEntity { AccountId = 3, ContactId = contact.ContactId });

        var result = await action.Should().ThrowAsync<Pulse.ExceptionMiddleware.Exceptions.InvalidOperationException>();

        result.Which.Code.Should().Be(Errors.NotFoundAccountCode);
        result.WithMessage("L'identifiant de l'entité saisi 3 est introuvable");
    }

    [Fact]
    public void RetrieveContactsHavingRole_WithNoContacts_ShouldReturnEmptyLists()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AuthorizationContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new AuthorizationContext(options);
        var repository = new RoleEventRepository(context);

        var contactIds = new List<int>();
        var accountId = 1;

        // Act
        var result = repository.RetrieveContactsHavingRole(contactIds, accountId);

        // Assert
        result.Should().NotBeNull();
        result.ExistedContactRoles.Should().BeEmpty();
        result.UnexistedContactRoles.Should().BeEmpty();
    }

    [Fact]
    public void RetrieveContactsHavingRole_WithNoMatchingRoles_ShouldReturnAllContactsAsUnexisted()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AuthorizationContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new AuthorizationContext(options);
        var repository = new RoleEventRepository(context);

        var contactIds = new List<int> { 1, 2, 3 };
        var accountId = 1;

        // Act
        var result = repository.RetrieveContactsHavingRole(contactIds, accountId);

        // Assert
        result.Should().NotBeNull();
        result.ExistedContactRoles.Should().BeEmpty();
        result.UnexistedContactRoles.Should().BeEquivalentTo(contactIds);
    }

    [Fact]
    public void RetrieveContactsHavingRole_WithSomeMatchingRoles_ShouldReturnCorrectlySeparatedLists()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AuthorizationContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new AuthorizationContext(options);
        var repository = new RoleEventRepository(context);

        var accountId = 1;
        var existingContactIds = new List<int> { 1, 2 };
        var nonExistingContactIds = new List<int> { 3, 4 };
        var allContactIds = existingContactIds.Concat(nonExistingContactIds).ToList();

        // Add roles for existing contacts
        foreach (var contactId in existingContactIds)
        {
            context.RoleEntity.Add(new RoleEntity
            {
                ContactId = contactId,
                AccountId = accountId
            });
        }
        context.SaveChanges();

        // Act
        var result = repository.RetrieveContactsHavingRole(allContactIds, accountId);

        // Assert
        result.Should().NotBeNull();
        result.ExistedContactRoles.Should().BeEquivalentTo(existingContactIds);
        result.UnexistedContactRoles.Should().BeEquivalentTo(nonExistingContactIds);
    }

    [Fact]
    public void RetrieveContactsHavingRole_WithAllMatchingRoles_ShouldReturnAllContactsAsExisted()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AuthorizationContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new AuthorizationContext(options);
        var repository = new RoleEventRepository(context);

        var accountId = 1;
        var contactIds = new List<int> { 1, 2, 3 };

        // Add roles for all contacts
        foreach (var contactId in contactIds)
        {
            context.RoleEntity.Add(new RoleEntity
            {
                ContactId = contactId,
                AccountId = accountId
            });
        }
        context.SaveChanges();

        // Act
        var result = repository.RetrieveContactsHavingRole(contactIds, accountId);

        // Assert
        result.Should().NotBeNull();
        result.ExistedContactRoles.Should().BeEquivalentTo(contactIds);
        result.UnexistedContactRoles.Should().BeEmpty();
    }

    [Fact]
    public void RetrieveContactsHavingRole_WithDifferentAccountId_ShouldNotReturnThoseContacts()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AuthorizationContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new AuthorizationContext(options);
        var repository = new RoleEventRepository(context);

        var accountId1 = 1;
        var accountId2 = 2;
        var contactIds = new List<int> { 1, 2, 3 };

        // Add roles for account 1
        foreach (var contactId in contactIds)
        {
            context.RoleEntity.Add(new RoleEntity
            {
                ContactId = contactId,
                AccountId = accountId1
            });
        }
        context.SaveChanges();

        // Act - query for account 2
        var result = repository.RetrieveContactsHavingRole(contactIds, accountId2);

        // Assert
        result.Should().NotBeNull();
        result.ExistedContactRoles.Should().BeEmpty();
        result.UnexistedContactRoles.Should().BeEquivalentTo(contactIds);
    }

    [Fact]
    public void BuildErrors_ShouldReturnCorrectErrorMessages()
    {
        // Arrange
        var accountId = 1;
        var unexistedContactIds = new List<int> { 1, 2 };
        var subscription = new ContactRolesSubscription(accountId)
        {
            UnexistedContactRoles = unexistedContactIds
        };

        // Act
        var errors = subscription.BuildErrors().ToList();

        // Assert
        errors.Should().HaveCount(2);
        errors[0].Should().Be(string.Format(Errors.NotFoundRoleMessage, accountId, unexistedContactIds[0]));
        errors[1].Should().Be(string.Format(Errors.NotFoundRoleMessage, accountId, unexistedContactIds[1]));
    }


    [Fact]
    public async Task GetRole_WithExistingRole_ShouldReturnRole()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AuthorizationContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new AuthorizationContext(options);
        var repository = new RoleEventRepository(context);

        var existingRole = new RoleEntity
        {
            ContactId = 1,
            AccountId = 2,
            IsDelegation = true,
            IsFavorite = false,
            IsSignatory = true
        };

        await context.RoleEntity.AddAsync(existingRole);
        await context.SaveChangesAsync();

        var inputRole = new RoleEntity
        {
            ContactId = 1,
            AccountId = 2
        };

        // Act
        var result = await repository.GetRole(inputRole);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(existingRole.ContactId, result.ContactId);
        Assert.Equal(existingRole.AccountId, result.AccountId);
        Assert.Equal(existingRole.IsSignatory, result.IsSignatory);
        Assert.Equal(existingRole.IsDelegation, result.IsDelegation);
        Assert.Equal(existingRole.IsFavorite, result.IsFavorite);
    }

    [Fact]
    public async Task GetRole_WithNonExistingRole_ShouldReturnNull()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AuthorizationContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new AuthorizationContext(options);
        var repository = new RoleEventRepository(context);

        var inputRole = new RoleEntity
        {
            ContactId = 1,
            AccountId = 2
        };

        // Act
        var result = await repository.GetRole(inputRole);

        // Assert
        Assert.Null(result);
    }

    public static IEnumerable<object[]> AccountAndContact()
    {
        yield return new object[] { 1, 2 };
        yield return new object[] { 2, 1 };
        yield return new object[] { 2, 2 };
    }

    [Fact]
    public async Task DeleteAuthorizations_Nominal()
    {
        var options = new DbContextOptionsBuilder<AuthorizationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using var context = new AuthorizationContext(options);

        var contactAuths = _fixture.Build<ContactAuthorizationEntity>()
            .With(c => c.ContactId, 1)
            .With(c => c.AccountId, 1)
            .CreateMany(5);
        context.ContactAuthorizationEntity.AddRange(contactAuths);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new RoleEventRepository(context);
        await repository.DeleteContactAuthorizations(1, 1);

        var result = context.ContactAuthorizationEntity.Where(c => c.ContactId == 1 && c.AccountId == 1).ToList();

        Assert.Empty(result);
    }
}
