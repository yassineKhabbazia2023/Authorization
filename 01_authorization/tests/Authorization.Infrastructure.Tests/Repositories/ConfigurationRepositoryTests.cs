// <copyright file="ConfigurationRepositoryTests.cs" company="Pulse">
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

public class ConfigurationRepositoryTests
{
    private readonly DbContextOptions<AuthorizationContext> _options;
    private readonly Fixture _fixture;

    public ConfigurationRepositoryTests()
    {
        _options = new DbContextOptionsBuilder<AuthorizationContext>()
                            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                            .Options;
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public async Task GetContactConfigurationAsync_Return_Authorization()
    {
        using (var context = new AuthorizationContext(_options))
        {
            var contactAuthorizationAccountEntity = _fixture.Build<ContactAuthorization>()
                            .With(a => a.Authorization)
                            .With(a => a.ContactId, 123)
                            .With(a => a.AccountId, 456)
                            .Without(a => a.Contact)
                            .Without(a => a.Account)
                            .CreateMany(3);

            var expectedAuthorization = contactAuthorizationAccountEntity.Select(c => c.Authorization).MapAuthorizationToConfiguration();

            context.ContactAuthorization.AddRange(contactAuthorizationAccountEntity);
            await context.SaveChangesAsync();

            var repository = new ConfigurationRepository(context);

            var contactId = contactAuthorizationAccountEntity.First().ContactId;
            var accountId = contactAuthorizationAccountEntity.First().AccountId;

            var receivedAuthorization = await repository.GetContactAccountConfigurationAsync(contactId, accountId);

            var authExpectJson = JsonConvert.SerializeObject(expectedAuthorization);
            var authResultJson = JsonConvert.SerializeObject(receivedAuthorization);
            Assert.Equal(authExpectJson, authResultJson);
            Assert.NotNull(receivedAuthorization);
        }
    }

    [Fact]
    public async Task GetContactConfigurationAsyncAsync_Return_Empty()
    {
        using (var context = new AuthorizationContext(_options))
        {
            var contactAuthorizationAccountEntity = _fixture.Build<ContactAuthorization>()
                            .With(a => a.Authorization)
                            .With(a => a.ContactId, 123)
                            .With(a => a.AccountId, 456)
                            .Without(a => a.Contact)
                            .CreateMany(3);

            var expectedAuthorization = contactAuthorizationAccountEntity.Select(c => c.Authorization.Code);

            context.ContactAuthorization.AddRange(contactAuthorizationAccountEntity);
            await context.SaveChangesAsync();

            var repository = new ConfigurationRepository(context);

            var contactId = contactAuthorizationAccountEntity.First().ContactId;
            var accountId = contactAuthorizationAccountEntity.First().AccountId;

            var receivedAuthorization = await repository.GetContactAccountConfigurationAsync(999, 888);

            Assert.Empty(receivedAuthorization);
        }
    }
}
