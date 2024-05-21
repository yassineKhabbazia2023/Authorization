// <copyright file="ContactEventRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Pulse.Authorization.Infrastructure.Context;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Repositories;

namespace Pulse.Authorization.Infrastructure.Tests.Repositories;

public class SubscriptionEventRepositoryTests
{
    private readonly Fixture _fixture;

    public SubscriptionEventRepositoryTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public async Task AddSubscriptionAuthorizationsOnAccountAsync_ShouldAddAccountAuthorizationEntity()
    {
        // Arrange
        var logger = new Mock<ILogger<SubscriptionEventRepository>>();

        var options = new DbContextOptionsBuilder<AuthorizationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        var authorizationEntities = _fixture.Build<AuthorizationEntity>()
                            .With(a => a.ProductCode)
                            .CreateMany(3);
        var account = _fixture.Build<AccountEntity>()
            .With(a => a.AccountId, 42)
            .Create();

        using var context = new AuthorizationContext(options);
        var productCodes = authorizationEntities.Select(a => a.ProductCode);

        context.AccountEntity.Add(account);
        context.AuthorizationEntity.AddRange(authorizationEntities);
        context.SaveChanges();
        var repository = new SubscriptionEventRepository(context, logger.Object);

        // Act
        await repository.AddSubscriptionAuthorizationsOnAccountAsync(account.AccountId, productCodes!);

        // Assert
        var addedAuthorization = await context.AccountAuthorizationEntity.FirstOrDefaultAsync();

        Assert.NotNull(addedAuthorization);
    }

    [Fact]
    public async Task AddSubscriptionAuthorizationsOnContactAsync_ShouldAddContactAuthorizationEntity()
    {
        // Arrange
        var logger = new Mock<ILogger<SubscriptionEventRepository>>();

        var options = new DbContextOptionsBuilder<AuthorizationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        var authorizationEntities = _fixture.Build<AuthorizationEntity>()
                            .With(a => a.ProductCode)
                            .CreateMany(3);
        var account = _fixture.Build<AccountEntity>()
            .With(a => a.AccountId, 42)
            .Create();

        var contacts = _fixture.Build<ContactEntity>()
            .With(c => c.ContactId)
            .With(c => c.Type, "Customer")
            .CreateMany(10);

        var roles = new List<RoleEntity>();
        foreach(var c in contacts)
        {
            roles.Add(new RoleEntity
            {
                AccountId = account.AccountId,
                ContactId = c.ContactId,
                IsSignatory = true
            });
        }

        using var context = new AuthorizationContext(options);
        var productCodes = authorizationEntities.Select(a => a.ProductCode);

        context.AccountEntity.Add(account);
        context.AuthorizationEntity.AddRange(authorizationEntities);
        context.ContactEntity.AddRange(contacts);
        context.RoleEntity.AddRange(roles);
        context.SaveChanges();
        var repository = new SubscriptionEventRepository(context, logger.Object);

        // Act
        await repository.AddSubscriptionAuthorizationsOnContactAsync(account.AccountId, contacts.Select(c => c.ContactId), productCodes!);

        // Assert
        var addedAuthorization = await context.ContactAuthorizationEntity.FirstOrDefaultAsync();

        Assert.NotNull(addedAuthorization);
    }
}
