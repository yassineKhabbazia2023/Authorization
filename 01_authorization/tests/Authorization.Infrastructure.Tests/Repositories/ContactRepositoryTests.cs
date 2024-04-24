// <copyright file="ContactRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pulse.Authorization.Infrastructure.Context;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Mappers;
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

            var expectedContact = contactEntity.MapToContact();

            var repository = new ContactRepository(context);

            var receivedContact = await repository.GetContactByIdAsync(contactEntity.ContactId);

            var contactExpectJson = JsonConvert.SerializeObject(expectedContact);
            var contactResultJson = JsonConvert.SerializeObject(receivedContact);
            Assert.Equal(contactExpectJson, contactResultJson);
        }
    }
}
