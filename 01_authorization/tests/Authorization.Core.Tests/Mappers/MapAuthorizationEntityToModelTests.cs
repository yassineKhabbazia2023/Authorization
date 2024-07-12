// <copyright file="MapAuthorizationEntityToModelTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Newtonsoft.Json;
using Pulse.Authorization.Core.Mappers;
using Pulse.Authorization.Core.Models;
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
