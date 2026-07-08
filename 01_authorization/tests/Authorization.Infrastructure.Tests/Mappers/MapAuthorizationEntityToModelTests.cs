// <copyright file="MapAuthorizationEntityToModelTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Pulse.Authorization.Core.Mappers;
using Pulse.Authorization.Core.Models.Subscriptions;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Tests.Helpers;

namespace Pulse.Authorization.Core.Tests.Mappers;

public class MapAuthorizationEntityToModelTests
{
    private readonly Fixture _fixture;

    public MapAuthorizationEntityToModelTests()
    {
        _fixture = EntityFixtureFactory.Create();
    }

    [Fact]
    public void MapAuthorizationToAction_ShouldReturnActionModel()
    {
        var authorization = _fixture.Create<AuthorizationEntity>();

        var result = authorization.MapAuthorizationToAction();

        Assert.NotNull(result);
        Assert.Equal(authorization.AuthorizationId, result.ActionId);
        Assert.Equal(authorization.Name, result.Name);
        Assert.Equal(authorization.Code, result.Code);
        Assert.Equal(authorization.Label, result.Label);
        Assert.False(result.Enabled);
    }

    [Fact]
    public void MapAuthorizationToAction_WithNullSource_ShouldReturnActionModel()
    {
        var result = MapAuthorizationEntityToModel.MapAuthorizationToAction(null!);

        Assert.Null(result);
    }

    [Fact]
    public void MapAuthorizationToConfiguration_ShouldReturnConfiguration()
    {
        var category = "cat";
        var authorizations = new List<AuthorizationEntity>
        {
            new()
            {
                AuthorizationId = 1,
                Name = "name1",
                Code = "code1",
                Label = "label1",
                Category = category
            },
            new()
            {
                AuthorizationId = 2,
                Name = "name2",
                Code = "code2",
                Label = "label2",
                Category = category
            }
        };

        var result = authorizations.MapAuthorizationToConfiguration();

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(category, result.First().Category);
        Assert.Equal(authorizations.Count, result.First().Actions.Count());
    }

    [Fact]
    public void MapAuthorizationToConfiguration_WithNullSource_ShouldReturnEmptyList()
    {
        var result = MapAuthorizationEntityToModel.MapAuthorizationToConfiguration(null!);

        Assert.Empty(result);
    }

    [Fact]
    public void MapToContactAuthorizationEntities_WithValidInput_ShouldReturnCorrectEntities()
    {
        // Arrange
        var subscription = new ContactAuthorizationSubscription
        {
            AccountId = 123,
            ContactIds = new[] { 1, 2 },
            AuthorizationIds = new[] { 10, 20 }
        };

        // Act
        var result = subscription.MapToContactAuthorizationEntities().ToList();

        // Assert
        Assert.Equal(4, result.Count); // 2 contacts × 2 authorizations = 4 entities

        // Verify all combinations are created
        Assert.Contains(result, e => e.ContactId == 1 && e.AuthorizationId == 10);
        Assert.Contains(result, e => e.ContactId == 1 && e.AuthorizationId == 20);
        Assert.Contains(result, e => e.ContactId == 2 && e.AuthorizationId == 10);
        Assert.Contains(result, e => e.ContactId == 2 && e.AuthorizationId == 20);

        // Verify all entities have correct AccountId
        Assert.All(result, entity => Assert.Equal(123, entity.AccountId));

        // Verify all entities have CreationDate set to approximately now
        var now = DateTime.UtcNow;
        Assert.All(result, entity =>
        {
            Assert.True(Math.Abs((entity.CreationDate - now).TotalSeconds) < 1,
                "CreationDate should be set to current UTC time");
        });
    }

    [Fact]
    public void MapToContactAuthorizationEntities_WithSingleContactAndAuthorization_ShouldReturnSingleEntity()
    {
        // Arrange
        var subscription = new ContactAuthorizationSubscription
        {
            AccountId = 456,
            ContactIds = new[] { 5 },
            AuthorizationIds = new[] { 15 }
        };

        // Act
        var result = subscription.MapToContactAuthorizationEntities().ToList();

        // Assert
        Assert.Single(result);
        var entity = result.First();
        Assert.Equal(456, entity.AccountId);
        Assert.Equal(5, entity.ContactId);
        Assert.Equal(15, entity.AuthorizationId);
        Assert.True(entity.CreationDate <= DateTime.UtcNow);
    }

    [Fact]
    public void MapToContactAuthorizationEntities_WithMultipleContactsAndAuthorizations_ShouldCreateAllCombinations()
    {
        // Arrange
        var subscription = new ContactAuthorizationSubscription
        {
            AccountId = 789,
            ContactIds = new[] { 1, 2, 3 },
            AuthorizationIds = new[] { 10, 20, 30, 40 }
        };

        // Act
        var result = subscription.MapToContactAuthorizationEntities().ToList();

        // Assert
        Assert.Equal(12, result.Count); // 3 contacts × 4 authorizations = 12 entities

        // Verify each contact has all authorizations
        foreach (var contactId in subscription.ContactIds)
        {
            foreach (var authId in subscription.AuthorizationIds)
            {
                Assert.Contains(result, e => e.ContactId == contactId && e.AuthorizationId == authId);
            }
        }
    }

    [Fact]
    public void MapToContactAuthorizationEntities_WithEmptyContactIds_ShouldReturnEmptyCollection()
    {
        // Arrange
        var subscription = new ContactAuthorizationSubscription
        {
            AccountId = 999,
            ContactIds = new int[0],
            AuthorizationIds = new[] { 10, 20 }
        };

        // Act
        var result = subscription.MapToContactAuthorizationEntities().ToList();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void MapToContactAuthorizationEntities_WithEmptyAuthorizationIds_ShouldReturnEmptyCollection()
    {
        // Arrange
        var subscription = new ContactAuthorizationSubscription
        {
            AccountId = 999,
            ContactIds = new[] { 1, 2 },
            AuthorizationIds = new int[0]
        };

        // Act
        var result = subscription.MapToContactAuthorizationEntities().ToList();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void MapToContactAuthorizationEntities_WithBothEmptyCollections_ShouldReturnEmptyCollection()
    {
        // Arrange
        var subscription = new ContactAuthorizationSubscription
        {
            AccountId = 999,
            ContactIds = new int[0],
            AuthorizationIds = new int[0]
        };

        // Act
        var result = subscription.MapToContactAuthorizationEntities().ToList();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void MapToContactAuthorizationEntities_ShouldSetCreationDateToUtcNow()
    {
        // Arrange
        var beforeExecution = DateTime.UtcNow;
        var subscription = new ContactAuthorizationSubscription
        {
            AccountId = 123,
            ContactIds = new[] { 1 },
            AuthorizationIds = new[] { 10 }
        };

        // Act
        var result = subscription.MapToContactAuthorizationEntities().ToList();
        var afterExecution = DateTime.UtcNow;

        // Assert
        var entity = result.First();
        Assert.True(entity.CreationDate >= beforeExecution && entity.CreationDate <= afterExecution,
            "CreationDate should be set to the current UTC time during execution");
    }

    [Fact]
    public void MapToContactAuthorizationEntities_WithNegativeIds_ShouldHandleCorrectly()
    {
        // Arrange
        var subscription = new ContactAuthorizationSubscription
        {
            AccountId = -1,
            ContactIds = new[] { -5, -10 },
            AuthorizationIds = new[] { -15, -20 }
        };

        // Act
        var result = subscription.MapToContactAuthorizationEntities().ToList();

        // Assert
        Assert.Equal(4, result.Count);
        Assert.All(result, entity => Assert.Equal(-1, entity.AccountId));
        Assert.Contains(result, e => e.ContactId == -5 && e.AuthorizationId == -15);
        Assert.Contains(result, e => e.ContactId == -5 && e.AuthorizationId == -20);
        Assert.Contains(result, e => e.ContactId == -10 && e.AuthorizationId == -15);
        Assert.Contains(result, e => e.ContactId == -10 && e.AuthorizationId == -20);
    }

    [Fact]
    public void MapToContactAuthorizationEntities_WithDuplicateIds_ShouldCreateDuplicateEntities()
    {
        // Arrange
        var subscription = new ContactAuthorizationSubscription
        {
            AccountId = 123,
            ContactIds = new[] { 1, 1 }, // Duplicate contact ID
            AuthorizationIds = new[] { 10, 10 } // Duplicate authorization ID
        };

        // Act
        var result = subscription.MapToContactAuthorizationEntities().ToList();

        // Assert
        Assert.Equal(4, result.Count); // 2 contacts × 2 authorizations = 4 entities (including duplicates)

        // Should have 4 identical entities
        Assert.All(result, entity =>
        {
            Assert.Equal(123, entity.AccountId);
            Assert.Equal(1, entity.ContactId);
            Assert.Equal(10, entity.AuthorizationId);
        });
    }
}
