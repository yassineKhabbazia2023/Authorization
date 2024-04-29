// <copyright file="ConfigurationControllerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using AutoFixture;
using Pulse.Authorization.API;
using Pulse.Authorization.Core.Interfaces;
using Pulse.Authorization.API.Controllers;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Pulse.Authorization.Core.Models;

namespace Pulse.Authorization.Api.Tests.Controllers;

public class ConfigurationControllerTests : IClassFixture<WebApplicationFactory<Startup>>
{
    private readonly Fixture _fixture;

    public ConfigurationControllerTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public async Task GetContactAccountConfigurationAsync_Should_Returns_ConfigurationList()
    {
        // Arrange
        var accountId = 6000;
        var contactId = 3;
        var expected = _fixture.Create<List<Configuration>>();

        var configurationService = new Mock<IConfigurationService>(MockBehavior.Strict);
        configurationService.Setup(service => service.GetContactAccountConfigurationAsync(contactId, accountId))
            .ReturnsAsync(expected);
        var configurationController = new ConfigurationController(configurationService.Object);

        // Act
        var result = await configurationController.GetContactAccountConfigurationAsync(contactId, accountId);

        // Assert
        Assert.Equal(expected, (result.Result as OkObjectResult)?.Value);
        Assert.Equal(200, (result.Result as OkObjectResult)?.StatusCode);
    }

    [Fact]
    public void GetContactAccountConfigurationAsync_Should_Throw_TechnicalException()
    {
        // Arrange
        var accountId = 6000;
        var contactId = 3;

        var configurationService = new Mock<IConfigurationService>();
        var configurationController = new ConfigurationController(configurationService.Object);

        // Act
        var act = async () => await configurationController.GetContactAccountConfigurationAsync(contactId, accountId);

        // Assert
        var exception = Assert.ThrowsAsync<NotFoundException>(act);
    }
}
