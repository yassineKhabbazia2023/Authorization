// <copyright file="SubscriptionEventRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Pulse.Authorization.Infrastructure.Context;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Interfaces;
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
        var authorizationRepository = new Mock<IAuthorizationRepository>(MockBehavior.Strict);
        var expectedResult = _fixture.CreateMany<AccountAuthorizationEntity>();
        authorizationRepository.Setup(r => r.AddSubscriptionAuthorizationsOnAccountAsync(It.IsAny<int>(), It.IsAny<List<string>>()))
        .ReturnsAsync(expectedResult);
        var repository = new SubscriptionEventRepository(authorizationRepository.Object);

        // Act
        var result = await repository.AddSubscriptionAuthorizationsOnAccountAsync(It.IsAny<int>(), It.IsAny<List<string>>());

        // Assert
        Assert.Equivalent(expectedResult, result);
    }

    [Fact]
    public async Task AddSubscriptionAuthorizationsOnContactAsync_ShouldAddContactAuthorizationEntity()
    {
        // Arrange
        var logger = new Mock<ILogger<SubscriptionEventRepository>>();

        var options = new DbContextOptionsBuilder<AuthorizationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        var authorizationRepository = new Mock<IAuthorizationRepository>(MockBehavior.Strict);
        var expectedResult = _fixture.CreateMany<ContactAuthorizationEntity>();
        authorizationRepository.Setup(r => r.AddSubscriptionAuthorizationsOnAccountSignatoriesAsync(It.IsAny<int>(), It.IsAny<List<string>>()))
        .ReturnsAsync(expectedResult);
        var repository = new SubscriptionEventRepository(authorizationRepository.Object);

        // Act
        var result = await repository.AddSubscriptionAuthorizationsOnAccountSignatoriesAsync(It.IsAny<int>(), It.IsAny<List<string>>());

        // Assert
        Assert.Equivalent(expectedResult, result);
    }
}
