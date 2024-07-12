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
            ContactId = 1,
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
            ContactId = 1,
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
            ContactId = 1,
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
        await repository.RemoveContactAsync(contactId: 1);

        // Assert
        var updatedContact = await context.ContactEntity.FirstOrDefaultAsync();

        Assert.NotNull(updatedContact);
        Assert.Equal(ContactStatus.Removed.ToString(), updatedContact.Status);
        Assert.NotNull(updatedContact.LastUpdateDate);
    }
}
