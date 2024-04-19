// <copyright file="AuthorizationControllerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using Pulse.Authorization.Core.Models;
using AutoFixture;
using Kpmg.ExceptionMiddleware.AdvancedException;
using Pulse.Authorization.API;
using Pulse.Authorization.Core.Interfaces;
using Pulse.Authorization.API.Controllers;
using Pulse.Authorization.Core.Requests;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;

namespace Pulse.Authorization.Api.Tests.Controllers;

public class AuthorizationControllerTests : IClassFixture<WebApplicationFactory<Startup>>
{
    private readonly Fixture _fixture;
    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public AuthorizationControllerTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public async Task GetNavigationAsync_Should_Returns_ResourceList()
    {
        // Arrange
        var accountId = 6000;
        var contactId = 3;
        var expected = _fixture.Create<NavigationRequest>();

        var authorizationService = new Mock<IAuthorizationService>(MockBehavior.Strict);
        authorizationService.Setup(service => service.GetNavigationsAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(expected);
        var authorizationController = new AuthorizationController(authorizationService.Object);

        // Act
        var result = await authorizationController.GetNavigationAsync(accountId, contactId);

        // Assert
        Assert.Equal(expected, (result.Result as OkObjectResult)?.Value);
        Assert.Equal(200, (result.Result as OkObjectResult)?.StatusCode);
    }

    [Fact]
    public void GetNavigationAsync_Should_Throw_TechnicalException()
    {
        // Arrange
        var accountId = 6000;
        var contactId = 3;

        var authorizationService = new Mock<IAuthorizationService>();
        var authorizationController = new AuthorizationController(authorizationService.Object);

        // Act
        var act = async () => await authorizationController.GetNavigationAsync(accountId, contactId);

        // Assert
        var exception = Assert.ThrowsAsync<NotFoundException>(act);
    }
}
