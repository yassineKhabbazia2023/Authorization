// <copyright file="MapAuthorizationEntityToModelTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Net;
using System.Reflection.Metadata;
using AutoFixture;
using Newtonsoft.Json;
using Pulse.Authorization.Core.Constants;
using Pulse.Authorization.Core.Models;
using Pulse.Authorization.Core.Requests;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Mappers;

namespace Pulse.Authorization.Infrastructure.Tests.Mappers;

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
    public void MapToNavigationRequest_ShouldReturnNavigationRequestModel()
    {
        // Arrange
        var resourceEntityFirst = _fixture.Build<ResourceEntity>()
                            .With(r => r.Category, GlobalConstants.ResourceTypeUnitaire)
                            .Create();
        var resourceEntitySecond = _fixture.Build<ResourceEntity>()
                            .With(r => r.Category, GlobalConstants.ResourceTypeGlobale)
                            .Create();

        var resourceEntity = new List<ResourceEntity>()
        {
            resourceEntityFirst,
            resourceEntitySecond
        };

        var expectedNavigationRequest = new NavigationRequest
        {
            UnitView = new List<Navigation>()
            {
                new Navigation()
                {
                    Name = resourceEntityFirst.Name,
                    Label = resourceEntityFirst.Label,
                    Childrens = resourceEntityFirst.InverseParent.Select(r => r.MapToNavigation())
                }
            },
            OverView = new List<Navigation>()
            {
                new Navigation()
                {
                    Name = resourceEntitySecond.Name,
                    Label = resourceEntitySecond.Label,
                    Childrens = resourceEntitySecond.InverseParent.Select(r => r.MapToNavigation())
                }
            }
        };

        // Act
        var navigationRequestModel = MapAuthorizationEntityToModel.MapToNavigationRequest(resourceEntity);

        // Assert
        var navigationRequestJson = JsonConvert.SerializeObject(expectedNavigationRequest);
        var navigationReceivedJson = JsonConvert.SerializeObject(navigationRequestModel);

        Assert.Equal(navigationRequestJson, navigationReceivedJson);
    }

    [Fact]
    public void MapToAction_ShouldReturnAction()
    {
        // Arrange
        var actionEntity = _fixture.Create<ActionEntity>();

        var expectedAction = new Core.Models.Action
        {
            ActionId = actionEntity.ActionId,
            Code = actionEntity.Code,
            Name = actionEntity.Name,
        };

        // Act
        var actionResult = MapAuthorizationEntityToModel.MapToAction(actionEntity);

        // Assert
        var actionExpectJson = JsonConvert.SerializeObject(expectedAction);
        var actionResultJson = JsonConvert.SerializeObject(actionResult);
        Assert.Equal(actionExpectJson, actionResultJson);
    }

    [Fact]
    public void MapToContact_ShouldReturnContact()
    {
        // Arrange
        var contactEntity = _fixture.Create<ContactEntity>();

        var expectedContact = new Contact
        {
            ContactId = contactEntity.ContactId,
            ContactGlobalUniqueId = contactEntity.ContactGlobalUniqueId,
            CreationDate = contactEntity.CreationDate,
            Email = contactEntity.Email,
            FirstName = contactEntity.FirstName,
            LastName = contactEntity.LastName,
            Status = contactEntity.Status,
            Type = contactEntity.Type,
            PersonaName = contactEntity.PersonaName,
        };

        // Act
        var actionResult = MapContactEntityToModel.MapToContact(contactEntity);

        // Assert
        var actionExpectJson = JsonConvert.SerializeObject(expectedContact);
        var actionResultJson = JsonConvert.SerializeObject(actionResult);
        Assert.Equal(actionExpectJson, actionResultJson);
    }
}
