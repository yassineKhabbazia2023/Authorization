// <copyright file="MapAuthorizationEntityToModelTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Pulse.Authorization.Core.Mappers;
using Pulse.Authorization.Infrastructure.Entities;

namespace Pulse.Authorization.Core.Tests.Mappers;

public class MapAuthorizationEntityToModelTests
{
    private readonly Fixture _fixture;

    public MapAuthorizationEntityToModelTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
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
}
