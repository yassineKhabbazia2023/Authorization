// <copyright file="ContactRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Pulse.Authorization.Infrastructure.Context;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Enum;
using Pulse.Authorization.Infrastructure.Repositories;

namespace Pulse.Authorization.Infrastructure.Tests.Repositories;

public class ContactRepositoryTests
{
    private readonly DbContextOptions<AuthorizationContext> _options;
    private readonly Fixture _fixture;

    public ContactRepositoryTests()
    {
        _options = new DbContextOptionsBuilder<AuthorizationContext>()
                            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                            .Options;
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public async Task GetContactByIdAsync_Return_Contact()
    {
        using (var context = new AuthorizationContext(_options))
        {
            var contactEntity = _fixture.Create<ContactEntity>();

            context.ContactEntity.AddRange(contactEntity);
            await context.SaveChangesAsync();

            var expectedContact = contactEntity;

            var repository = new ContactRepository(context);

            var receivedContact = await repository.GetContactByIdAsync(contactEntity.ContactId);

            Assert.Equivalent(expectedContact.ContactId, receivedContact.ContactId);
        }
    }

    [Fact]
    public async Task GetSignatoriesAsync_Nominal()
    {
        var accountId = 1;

        using var context = new AuthorizationContext(_options);

        var contactProperties = _fixture.Build<string>();
        context.RoleEntity.AddRange(
            new RoleEntity
            {
                AccountId = accountId,
                IsSignatory = true,
                Contact = new ContactEntity
                {
                    ContactId = 101,
                    Email = contactProperties.Create(),
                    FirstName = contactProperties.Create(),
                    LastName = contactProperties.Create(),
                    PersonaName = contactProperties.Create(),
                    Type = ContactType.Customer.ToString(),
                }
            },
            new RoleEntity
            {
                AccountId = accountId,
                IsSignatory = true,
                Contact = new ContactEntity
                {
                    ContactId = 102,
                    Email = contactProperties.Create(),
                    FirstName = contactProperties.Create(),
                    LastName = contactProperties.Create(),
                    PersonaName = contactProperties.Create(),
                    Type = ContactType.Customer.ToString(),
                }
            });
        await context.SaveChangesAsync();

        var repository = new ContactRepository(context);

        var result = await repository.GetSignatoriesAsync(accountId);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Equal(2, result.Count());
        Assert.Equal(101, result.First());
        Assert.Equal(102, result.ElementAt(1));
    }

    [Theory]
    [MemberData(nameof(InvalidContacts))]
    public async Task GetSignatoriesAsync_ShouldReturnEmptyList_WhenNoClientSignatoryOnAccount(int accountId, bool isSignatory,ContactEntity contact)
    {
        using var context = new AuthorizationContext(_options);

        var contactProperties = _fixture.Build<string>();
        context.RoleEntity.AddRange(
            new RoleEntity
            {
                AccountId = accountId,
                IsSignatory = isSignatory,
                Contact = contact
            });
        await context.SaveChangesAsync();

        var repository = new ContactRepository(context);

        var result = await repository.GetSignatoriesAsync(1);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void RetrieveExistedContacts_ShouldReturnCorrectSubscriptions()
    {
        using var context = new AuthorizationContext(_options);

        // Arrange: Create contacts
        var customerContact = new ContactEntity
        {
            ContactId = 1,
            Email = "dibemark@rydge.com",
            PersonaName = "Marc",
            FirstName = "Marc",
            LastName = "DIBEH",
            Type = ContactType.Customer.ToString()
        };
        var collaboratorContact = new ContactEntity
        {
            ContactId = 2,
            Email = "hakouna@rydge.com",
            PersonaName = "Marc",
            FirstName = "Marc",
            LastName = "DIBEH",
            Type = ContactType.Collaborator.ToString()
        };
        var unrelatedContact = new ContactEntity
        {
            Email = "matata@rydge.com",
            PersonaName = "Marc",
            FirstName = "Marc",
            LastName = "DIBEH",
            ContactId = 3,
            Type = "Other"
        };

        context.ContactEntity.AddRange(customerContact, collaboratorContact, unrelatedContact);
        context.SaveChanges();

        var repository = new ContactRepository(context);
        var inputIds = new List<int> { 1, 2, 4 }; // 4 is non-existent

        // Act
        var result = repository.RetrieveExistedContacts(inputIds);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(1, result.ClientContacts);
        Assert.Contains(2, result.CollabContacts);
        Assert.Contains(4, result.UnexistedContacts);
        Assert.DoesNotContain(3, result.ClientContacts); // type is "Other", not Customer or Collaborator
    }

    [Fact]
    public void RetrieveExistedContacts_AllCustomers_ReturnsOnlyClientContacts()
    {
        using var context = new AuthorizationContext(_options);

        var customer1 = new ContactEntity
        {
            ContactId = 10,
            Email = "c1@example.com",
            PersonaName = "Client1",
            FirstName = "John",
            LastName = "Doe",
            Type = ContactType.Customer.ToString()
        };
        var customer2 = new ContactEntity
        {
            ContactId = 11,
            Email = "c2@example.com",
            PersonaName = "Client2",
            FirstName = "Jane",
            LastName = "Smith",
            Type = ContactType.Customer.ToString()
        };

        context.ContactEntity.AddRange(customer1, customer2);
        context.SaveChanges();

        var repository = new ContactRepository(context);
        var inputIds = new List<int> { 10, 11 };

        var result = repository.RetrieveExistedContacts(inputIds);

        Assert.NotNull(result);
        Assert.Equal(new[] { 10, 11 }, result.ClientContacts);
        Assert.Empty(result.CollabContacts);
        Assert.Empty(result.UnexistedContacts);
    }

    [Fact]
    public void RetrieveExistedContacts_AllCollaborators_ReturnsOnlyCollabContacts()
    {
        using var context = new AuthorizationContext(_options);

        var collab1 = new ContactEntity
        {
            ContactId = 20,
            Email = "collab1@example.com",
            PersonaName = "Collab1",
            FirstName = "Alan",
            LastName = "Turing",
            Type = ContactType.Collaborator.ToString()
        };
        var collab2 = new ContactEntity
        {
            ContactId = 21,
            Email = "collab2@example.com",
            PersonaName = "Collab2",
            FirstName = "Grace",
            LastName = "Hopper",
            Type = ContactType.Collaborator.ToString()
        };

        context.ContactEntity.AddRange(collab1, collab2);
        context.SaveChanges();

        var repository = new ContactRepository(context);
        var inputIds = new List<int> { 20, 21 };

        var result = repository.RetrieveExistedContacts(inputIds);

        Assert.NotNull(result);
        Assert.Empty(result.ClientContacts);
        Assert.Equal(new[] { 20, 21 }, result.CollabContacts);
        Assert.Empty(result.UnexistedContacts);
    }


    [Fact]
    public void RetrieveExistedContacts_NoMatchingContacts_ReturnsAllAsUnexisted()
    {
        using var context = new AuthorizationContext(_options);

        var repository = new ContactRepository(context);
        var inputIds = new List<int> { 100, 200 };

        var result = repository.RetrieveExistedContacts(inputIds);

        Assert.NotNull(result);
        Assert.Empty(result.ClientContacts);
        Assert.Empty(result.CollabContacts);
        Assert.Equal(new[] { 100, 200 }, result.UnexistedContacts);
    }

    [Fact]
    public void RetrieveExistedContacts_EmptyInput_ReturnsEmptyResults()
    {
        using var context = new AuthorizationContext(_options);

        var repository = new ContactRepository(context);
        var result = repository.RetrieveExistedContacts(new List<int>());

        Assert.NotNull(result);
        Assert.Empty(result.ClientContacts);
        Assert.Empty(result.CollabContacts);
        Assert.Empty(result.UnexistedContacts);
    }


    public static TheoryData<int, bool, ContactEntity> InvalidContacts =>
        new TheoryData<int, bool, ContactEntity>
        {
            {
                1,
                true,
                new ContactEntity
                {
                    ContactId = 104,
                    Email = "email",
                    FirstName = "fistName",
                    LastName = "lastName",
                    PersonaName = "personaName",
                    Type = ContactType.Collaborator.ToString(),
                }
            },
            {
                1,
                false,
                new ContactEntity
                {
                    ContactId = 104,
                    Email = "email",
                    FirstName = "fistName",
                    LastName = "lastName",
                    PersonaName = "personaName",
                    Type = ContactType.Customer.ToString(),
                }
            },
            {
                2,
                true,
                new ContactEntity
                {
                    ContactId = 104,
                    Email = "email",
                    FirstName = "fistName",
                    LastName = "lastName",
                    PersonaName = "personaName",
                    Type = ContactType.Customer.ToString(),
                }
            },
        };
}
