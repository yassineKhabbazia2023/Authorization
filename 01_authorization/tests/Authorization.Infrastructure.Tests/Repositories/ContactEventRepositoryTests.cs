// <copyright file="ContactEventRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Pulse.Authorization.Infrastructure.Context;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Enum;
using Pulse.Authorization.Infrastructure.Repositories;

namespace Pulse.Authorization.Infrastructure.Tests.Repositories;

public class ContactEventRepositoryTests
{
    [Fact]
    public async Task CreateContactAsync_WithContactData_ShouldCreateContact()
    {
        var options = new DbContextOptionsBuilder<AuthorizationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using var context = new AuthorizationContext(options);
        var repository = new ContactEventRepository(context);
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
        await repository.CreateContactAsync(contactEntity);

        // Assert
        var addedContact = await context.ContactEntity.FirstOrDefaultAsync();

        Assert.NotNull(addedContact);
        Assert.Equal(contactEntity.ContactId, addedContact.ContactId);
        Assert.Equal(contactEntity.FirstName, addedContact.FirstName);
        Assert.Equal(contactEntity.LastName, addedContact.LastName);
        Assert.Equal(contactEntity.Email, addedContact.Email);
        Assert.Equal(contactEntity.Status, addedContact.Status);
        Assert.Equal(contactEntity.Type, addedContact.Type);
    }

    [Fact]
    public async Task UpdateContactAsync_WithContactData_ShouldCreateContact()
    {
        var options = new DbContextOptionsBuilder<AuthorizationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using var context = new AuthorizationContext(options);
        var repository = new ContactEventRepository(context);
        var contactEntity = new ContactEntity
        {
            ContactId = 2,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            Status = ContactStatus.Declared.ToString(),
            Type = "Collaborator",
            PersonaName = "None"
        };

        await context.ContactEntity.AddAsync(contactEntity);
        await context.SaveChangesAsync();

        var modifiedContactEntity = new ContactEntity
        {
            ContactId = 2,
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@test.com",
            Status = ContactStatus.Connected.ToString(),
            Type = "Collaborator",
            PersonaName = "None"
        };

        // Act
        await repository.UpdateContactAsync(modifiedContactEntity);

        // Assert
        var updatedContact = await context.ContactEntity.FirstOrDefaultAsync();

        Assert.NotNull(updatedContact);
        Assert.Equal(modifiedContactEntity.ContactId, updatedContact.ContactId);
        Assert.Equal(modifiedContactEntity.FirstName, updatedContact.FirstName);
        Assert.Equal(modifiedContactEntity.LastName, updatedContact.LastName);
        Assert.Equal(modifiedContactEntity.Email, updatedContact.Email);
        Assert.Equal(modifiedContactEntity.Status, updatedContact.Status);
        Assert.Equal(modifiedContactEntity.Type, updatedContact.Type);
        Assert.NotNull(updatedContact.LastUpdateDate);
    }

    [Fact]
    public async Task RevokeContactAsync_WithContactData_ShouldCreateContact()
    {
        var options = new DbContextOptionsBuilder<AuthorizationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using var context = new AuthorizationContext(options);
        var repository = new ContactEventRepository(context);
        var contactEntity = new ContactEntity
        {
            ContactId = 4,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            Status = ContactStatus.Connected.ToString(),
            Type = "Collaborator",
            PersonaName = "None"
        };

        await context.ContactEntity.AddAsync(contactEntity);
        await context.SaveChangesAsync();

        // Act
        await repository.RemoveContactAsync(contactId: 4);

        // Assert
        // ignore query filter to be able to get deleted entity
        var updatedContact = await context.ContactEntity.IgnoreQueryFilters().FirstOrDefaultAsync();

        Assert.NotNull(updatedContact);
        Assert.Equal(ContactStatus.Removed.ToString(), updatedContact.Status);
        Assert.False(updatedContact.IsActive);
        Assert.NotNull(updatedContact.LastUpdateDate);
    }

    [Fact]
    public async Task DoesContactExistAsync_WithExistingContact_ShouldReturnTrue()
    {
        var options = new DbContextOptionsBuilder<AuthorizationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using var context = new AuthorizationContext(options);

        var contact = new ContactEntity
        {
            ContactId = 5,
            ContactGlobalUniqueId = Guid.NewGuid(),
            FirstName = "Jean",
            LastName = "Pierre",
            Email = "jp@email.fr",
            Status = ContactStatus.Invited.ToString(),
            Type = "Customer",
            PersonaName = "Client",
            CreationDate = DateTime.UtcNow,
        };
        context.ContactEntity.Add(contact);
        await context.SaveChangesAsync();

        var repository = new ContactEventRepository(context);

        var result = await repository.DoesContactExistAsync(5);

        Assert.True(result);
    }

    [Fact]
    public async Task DoesContactExistAsync_WithNoExistingContact_ShouldReturnFalse()
    {
        var options = new DbContextOptionsBuilder<AuthorizationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using var context = new AuthorizationContext(options);

        var repository = new ContactEventRepository(context);

        var result = await repository.DoesContactExistAsync(1);

        Assert.False(result);
    }

    [Fact]
    public async Task RemoveContactAuthorizationsAsync_ShouldDeleteContactAuthorization()
    {
        var options = new DbContextOptionsBuilder<AuthorizationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using var context = new AuthorizationContext(options);

        var contactAuthorization = new ContactAuthorizationEntity
        {
            ContactId = 6,
            AccountId = 7,
            AuthorizationId = 1,
            CreationDate = DateTime.UtcNow,
        };
        context.ContactAuthorizationEntity.Add(contactAuthorization);
        await context.SaveChangesAsync();

        var repository = new ContactEventRepository(context);

        await repository.RemoveContactAuthorizationsAsync(6);

        var result = await context.ContactAuthorizationEntity.FirstOrDefaultAsync(ca => ca.ContactId == 6);

        Assert.Null(result);
    }

    [Fact]
    public async Task IsContactClientAsync_WithClientContact_ShouldReturnTrue()
    {
        var options = new DbContextOptionsBuilder<AuthorizationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using var context = new AuthorizationContext(options);

        var contact = new ContactEntity
        {
            ContactId = 8,
            ContactGlobalUniqueId = Guid.NewGuid(),
            FirstName = "Client",
            LastName = "User",
            Email = "cuser@email.fr",
            IsActive = true,
            Type = "Customer",
            PersonaName = "Client",
        };
        context.ContactEntity.Add(contact);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new ContactEventRepository(context);

        var result = await repository.IsContactClientAsync(8);

        Assert.True(result);
    }

    [Fact]
    public async Task IsContactClientAsync_WithCollabContact_ShouldReturnFalse()
    {
        var options = new DbContextOptionsBuilder<AuthorizationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using var context = new AuthorizationContext(options);

        var contact = new ContactEntity
        {
            ContactId = 9,
            ContactGlobalUniqueId = Guid.NewGuid(),
            FirstName = "Collab",
            LastName = "User",
            Email = "colluser@email.fr",
            IsActive = true,
            Type = "Collaborator",
            PersonaName = "Partner",
        };
        context.ContactEntity.Add(contact);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new ContactEventRepository(context);

        var result = await repository.IsContactClientAsync(9);

        Assert.False(result);
    }

    [Fact]
    public async Task IsContactClientAsync_WhenContactNotExist_ShouldReturnFalse()
    {
        var options = new DbContextOptionsBuilder<AuthorizationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using var context = new AuthorizationContext(options);

        var repository = new ContactEventRepository(context);

        var result = await repository.IsContactClientAsync(1);

        Assert.False(result);
    }

    [Fact]
    public async Task IsContactClientAsync_WhenContactNotActive_ShouldReturnFalse()
    {
        var options = new DbContextOptionsBuilder<AuthorizationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using var context = new AuthorizationContext(options);

        var contact = new ContactEntity
        {
            ContactId = 10,
            ContactGlobalUniqueId = Guid.NewGuid(),
            FirstName = "Client",
            LastName = "User",
            Email = "cuser@email.fr",
            IsActive = false,
            Type = "Customer",
            PersonaName = "Client",
        };
        context.ContactEntity.Add(contact);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new ContactEventRepository(context);

        var result = await repository.IsContactClientAsync(10);

        Assert.False(result);
    }
}
