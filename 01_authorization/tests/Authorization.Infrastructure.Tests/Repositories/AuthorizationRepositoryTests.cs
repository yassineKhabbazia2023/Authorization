// <copyright file="AuthorizationRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pulse.Authorization.Core.Constants;
using Pulse.Authorization.Core.Models;
using Pulse.Authorization.Core.Requests;
using Pulse.Authorization.Infrastructure.Context;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Mappers;
using Pulse.Authorization.Infrastructure.Repositories;

namespace Pulse.Authorization.Infrastructure.Tests.Repositories;

public class AuthorizationRepositoryTests
{
    private readonly DbContextOptions<AuthorizationContext> _options;
    private readonly Fixture _fixture;

    public AuthorizationRepositoryTests()
    {
        _options = new DbContextOptionsBuilder<AuthorizationContext>()
                            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                            .Options;
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public async Task GetContactAndAccountAuthorizations_Return_AuthorizationCode()
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

            var expectedAuthorization = contactAuthorizationAccountEntity.Select(c => c.Authorization.Code);

            context.ContactAuthorization.AddRange(contactAuthorizationAccountEntity);
            await context.SaveChangesAsync();

            var repository = new AuthorizationRepository(context);

            var contactId = contactAuthorizationAccountEntity.First().ContactId;
            var accountId = contactAuthorizationAccountEntity.First().AccountId;

            var receivedAuthorization = await repository.GetContactAccountAuthorizationsAsync(contactId, accountId);

            var authExpectJson = JsonConvert.SerializeObject(expectedAuthorization);
            var authResultJson = JsonConvert.SerializeObject(receivedAuthorization);
            Assert.Equal(authExpectJson, authResultJson);
            Assert.NotNull(receivedAuthorization);
        }
    }

    [Fact]
    public async Task GetAccountAuthorizations_Return_AuthorizationCode()
    {
        using (var context = new AuthorizationContext(_options))
        {
            var accountAuthorizationAccountEntity = _fixture.Build<AccountAuthorization>()
                            .With(a => a.Authorization)
                            .With(a => a.AccountId, 456)
                            .Without(a => a.Account)
                            .CreateMany(3);

            var expectedAuthorization = accountAuthorizationAccountEntity.Select(c => c.Authorization.Code);

            context.AccountAuthorization.AddRange(accountAuthorizationAccountEntity);
            await context.SaveChangesAsync();

            var repository = new AuthorizationRepository(context);

            var accountId = accountAuthorizationAccountEntity.First().AccountId;

            var receivedAuthorization = await repository.GetAccountAuthorizationAsync(accountId);

            var authExpectJson = JsonConvert.SerializeObject(expectedAuthorization);
            var authResultJson = JsonConvert.SerializeObject(receivedAuthorization);
            Assert.Equal(authExpectJson, authResultJson);
            Assert.NotNull(receivedAuthorization);
        }
    }

    [Fact]
    public async Task GetContactAndAccountAuthorizationsAsync_Return_Empty()
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

            var repository = new AuthorizationRepository(context);

            var contactId = contactAuthorizationAccountEntity.First().ContactId;
            var accountId = contactAuthorizationAccountEntity.First().AccountId;

            var receivedAuthorization = await repository.GetContactAccountAuthorizationsAsync(999, 888);

            Assert.Empty(receivedAuthorization);
        }
    }

    [Fact]
    public async Task GetContactAuthorizationsAsync_Return_AuthorizationCode()
    {
        using (var context = new AuthorizationContext(_options))
        {
            var contactAuthorizationAccountEntity = _fixture.Build<ContactAuthorization>()
                            .With(a => a.Authorization)
                            .With(a => a.ContactId, 123)
                            .Without(a => a.Contact)
                            .Without(a => a.Account)
                            .CreateMany(3);

            var expectedAuthorization = contactAuthorizationAccountEntity.Select(c => c.Authorization.Code);

            context.ContactAuthorization.AddRange(contactAuthorizationAccountEntity);
            await context.SaveChangesAsync();

            var repository = new AuthorizationRepository(context);

            var contactId = contactAuthorizationAccountEntity.First().ContactId;
            var accountId = contactAuthorizationAccountEntity.First().AccountId;

            var receivedAuthorization = await repository.GetContactAuthorizationAsync(contactId);

            var authExpectJson = JsonConvert.SerializeObject(expectedAuthorization);
            var authResultJson = JsonConvert.SerializeObject(receivedAuthorization);
            Assert.Equal(authExpectJson, authResultJson);
            Assert.NotNull(receivedAuthorization);
        }
    }
}
