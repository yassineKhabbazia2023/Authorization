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
