// <copyright file="MapToRoleEntityTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Mappers.EventMappers;
using Pulse.Back.Events.IntegrationEvents.EventsData;

namespace Pulse.Authorization.Infrastructure.Tests.Mappers.Events;

public class MapToRoleEntityTests
{
    [Fact]
    public void ToRoleEntity_RoleCreatedEvent_MapsCorrectly()
    {
        // Arrange
        var source = new RoleCreatedEventData
        {
            ContactId = 100,
            AccountId = 1,
            IsDelegation = true,
            IsFavorite = true,
            IsSignatory = true
        };

        // Act
        var result = source.ToRoleEntity();

        // Assert
        Assert.Equal(source.ContactId, result.ContactId);
        Assert.Equal(source.AccountId, result.AccountId);
        Assert.Equal(source.IsSignatory, result.IsSignatory);
        Assert.Equal(source.IsDelegation, result.IsDelegation);
        Assert.Equal(source.IsFavorite, result.IsFavorite);
    }

    [Fact]
    public void ToRoleEntity_WithNullRoleCreatedEventData_ShouldReturnNull()
    {
        RoleCreatedEventData data = null!;

        var result = MapToRoleEntity.ToRoleEntity(data);

        Assert.Null(result);
    }

    [Fact]
    public void ToRoleEntity_RoleUpdatedEvent_MapsCorrectly()
    {
        // Arrange
        var source = new RoleUpdatedEventData
        {
            ContactId = 100,
            AccountId = 1,
            IsSignatory = true
        };

        // Act
        var result = source.ToRoleEntity();

        // Assert
        Assert.Equal(source.ContactId, result.ContactId);
        Assert.Equal(source.AccountId, result.AccountId);
        Assert.Equal(source.IsSignatory, result.IsSignatory);
    }

    [Fact]
    public void ToRoleEntity_WithNullRoleUpdatedEvent_ShouldReturnNull()
    {
        RoleUpdatedEventData data = null!;

        var result = MapToRoleEntity.ToRoleEntity(data);

        Assert.Null(result);
    }

    [Fact]
    public void ToRoleEntity_MapsToDestinationCorrectly()
    {
        // Arrange
        var updatedRole = new RoleEntity
        {
            ContactId = 100,
            AccountId = 1,
            IsSignatory = false,
            IsDelegation = true,
            IsFavorite = true
        };

        var existingRole = new RoleEntity
        {
            ContactId = 100,
            AccountId = 1,
            IsSignatory = true,
            IsDelegation = true,
            IsFavorite = true
        };

        // Act
        updatedRole.ToRoleEntity(existingRole);

        // Assert
        Assert.Equal(updatedRole.ContactId, existingRole.ContactId);
        Assert.Equal(updatedRole.AccountId, existingRole.AccountId);
        Assert.Equal(updatedRole.IsSignatory, existingRole.IsSignatory);
        Assert.NotNull(existingRole.LastUpdateDate);
    }

    [Fact]
    public void ToRoleEntity_WithNullSource_ShouldReturn()
    {
        var role = new RoleEntity
        {
            ContactId = 100,
            AccountId = 1,
            IsSignatory = false,
            IsDelegation = true,
            IsFavorite = true
        };

        role.ToRoleEntity(null!);

        Assert.Equal(100, role.ContactId);
        Assert.Equal(1, role.AccountId);
        Assert.False(role.IsSignatory);
        Assert.True(role.IsDelegation);
        Assert.True(role.IsFavorite);
    }
}
