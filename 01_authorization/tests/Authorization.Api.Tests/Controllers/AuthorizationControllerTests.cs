// <copyright file="AuthorizationControllerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using Pulse.Authorization.API;
using Pulse.Authorization.API.Controllers;
using Pulse.Authorization.Core.Interfaces;

namespace Pulse.Authorization.Api.Tests.Controllers;

public class AuthorizationControllerTests : IClassFixture<WebApplicationFactory<Startup>>
{
    private readonly Fixture _fixture;

    public AuthorizationControllerTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public async Task GetContactAuthorizationAsync_Should_Returns_AuthorizationCodeList()
    {
        // Arrange
        var accountId = 6000;
        var contactId = 3;
        var expected = _fixture.Create<List<string>>();

        var authorizationService = new Mock<IAuthorizationService>(MockBehavior.Strict);
        authorizationService.Setup(service => service.GetContactAuthorizationAsync(contactId, accountId))
            .ReturnsAsync(expected);
        var authorizationController = new AuthorizationController(authorizationService.Object);

        // Act
        var result = await authorizationController.GetContactAuthorizationAsync(contactId, accountId);

        // Assert
        Assert.Equal(expected, (result.Result as OkObjectResult)?.Value);
        Assert.Equal(200, (result.Result as OkObjectResult)?.StatusCode);
    }

    [Fact]
    public void GetContactAuthorizationAsync_Should_Throw_TechnicalException()
    {
        // Arrange
        var accountId = 6000;
        var contactId = 3;

        var authorizationService = new Mock<IAuthorizationService>();
        var authorizationController = new AuthorizationController(authorizationService.Object);

        // Act
        var act = async () => await authorizationController.GetContactAuthorizationAsync(contactId, accountId);

        // Assert
        var exception = Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async void DeletePermissionAsync_Should_ReturnOk()
    {
        // Arrange
        var accountId = 6000;
        var contactId = 3;

        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService.Setup(c => c.DeleteContactAuthorizationAsync(It.IsAny<int>(), It.IsAny<int>())).Returns(Task.CompletedTask);
        var authorizationController = new AuthorizationController(authorizationService.Object);

        // Act
        var result = await authorizationController.DeleteContactAuthorizationAsync(contactId, accountId);

        // Assert
        Assert.Equal(200, (result as OkResult)?.StatusCode);
    }
}
