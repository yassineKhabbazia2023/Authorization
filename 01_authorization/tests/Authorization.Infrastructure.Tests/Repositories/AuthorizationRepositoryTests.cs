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
    public async Task GetNavigationsAsync_Return_Navigation()
    {
        using (var context = new AuthorizationContext(_options))
        {
            var accountEntity = _fixture.Build<AccountEntity>()
                            .Without(a => a.AccountResourceEntity)
                            .Without(a => a.AuthorizationEntity)
                            .Create();
            var contactEntity = _fixture.Build<ContactEntity>()
                            .Without(a => a.AuthorizationEntity)
                            .Create();
            var authorizationEntityFirst = _fixture.Build<AuthorizationEntity>()
                                              .With(auth => auth.Account, accountEntity)
                                              .With(auth => auth.Contact, contactEntity)
                                              .Without(auth => auth.Action)
                                              .Create();
            var authorizationEntitySecond = _fixture.Build<AuthorizationEntity>()
                                             .With(auth => auth.Account, accountEntity)
                                             .With(auth => auth.Contact, contactEntity)
                                             .Without(auth => auth.Action)
                                             .Create();
            var actionEntityFirst = _fixture.Build<ActionEntity>()
                                       .With(a => a.AuthorizationEntity, new List<AuthorizationEntity>() { authorizationEntityFirst })
                                       .Without(a => a.Resource)
                                       .Without(a => a.PersonnaActionEntity)
                                       .Create();
            var actionEntitySecond = _fixture.Build<ActionEntity>()
                                       .With(a => a.AuthorizationEntity, new List<AuthorizationEntity>() { authorizationEntitySecond })
                                       .Without(a => a.Resource)
                                       .Without(a => a.PersonnaActionEntity)
                                       .Create();

            var resourceEntityFirst = _fixture.Build<ResourceEntity>()
                            .With(r => r.ActionEntity, new List<ActionEntity>() { actionEntityFirst })
                            .With(r => r.Category, GlobalConstants.ResourceTypeUnitaire)
                            .With(r => r.Type, contactEntity.Type)
                            .Without(a => a.Parent)
                            .Without(a => a.InverseParent)
                            .Without(a => a.ActionEntity)
                            .Without(a => a.AccountResourceEntity)
                            .Create();
            var resourceEntitySecond = _fixture.Build<ResourceEntity>()
                            .With(r => r.ActionEntity, new List<ActionEntity>() { actionEntitySecond })
                            .With(r => r.Category, GlobalConstants.ResourceTypeGlobale)
                            .With(r => r.Type, contactEntity.Type)
                            .Without(a => a.Parent)
                            .Without(a => a.InverseParent)
                            .Without(a => a.ActionEntity)
                            .Without(a => a.AccountResourceEntity)
                            .Create();

            var accountResourceEntityFirst = _fixture.Build<AccountResourceEntity>()
                                        .With(a => a.Resource, resourceEntityFirst)
                                        .With(a => a.Account, accountEntity)
                                        .Create();

            var accountResourceEntitySecond = _fixture.Build<AccountResourceEntity>()
                                        .With(a => a.Resource, resourceEntitySecond)
                                        .With(a => a.Account, accountEntity)
                                        .Create();

            var accountResourceEntity = new List<AccountResourceEntity>()
            {
                accountResourceEntityFirst,
                accountResourceEntitySecond
            };

            context.AccountResourceEntity.AddRange(accountResourceEntity);
            await context.SaveChangesAsync();

            var expectedNavigationRequest = new NavigationRequest
            {
                UnitView = new List<Navigation>() { resourceEntityFirst.MapToNavigation() },
                OverView = new List<Navigation>() { resourceEntitySecond.MapToNavigation() }
            };

            var repository = new AuthorizationRepository(context);

            var receivedNavigationRequest = await repository.GetNavigationsAsync(accountEntity.AccountId, contactEntity.MapToContact());

            var navigationExpectJson = JsonConvert.SerializeObject(expectedNavigationRequest);
            var navigationResultJson = JsonConvert.SerializeObject(receivedNavigationRequest);
            Assert.Equal(navigationExpectJson, navigationResultJson);
            Assert.NotNull(receivedNavigationRequest);

        }
    }
}
