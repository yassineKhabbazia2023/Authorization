// <copyright file="AuthorizationControllerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Text;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Azure.Amqp.Transaction;
using Moq;
using Pulse.Authorization.API;
using Pulse.Authorization.API.Controllers;
using Pulse.Authorization.Core.Exceptions;
using Pulse.Authorization.Core.Interfaces;
using Pulse.Authorization.Core.Models;
using Pulse.Authorization.Core.Models.Utils;
using Pulse.Authorization.Core.Request;
using Pulse.ExceptionMiddleware.Exceptions;

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
    public async Task GetAllContactAuthorizationsAsync_Should_Returns_AuthorizationCodeList()
    {
        // Arrange
        var contactId = 3;
        var expected = _fixture.Create<List<string>>();

        var authorizationService = new Mock<IAuthorizationService>(MockBehavior.Strict);
        authorizationService.Setup(service => service.GetAllContactAuthorizationsAsync(contactId))
            .ReturnsAsync(expected);
        var authorizationController = new AuthorizationController(authorizationService.Object);

        // Act
        var result = await authorizationController.GetAllContactAuthorizationsAsync(contactId);

        // Assert
        result.Result.Should().BeEquivalentTo(new OkObjectResult(expected));
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
    public async Task DeletePermissionAsyncShouldReturnOkAsync()
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

    [Fact]
    public async Task GetContactIdsByAuthorizationCodes_ShouldReturnOk()
    {
        // Arrange
        List<Contact> contacts = new List<Contact>()
        {
            new Contact()
            {
                ContactId = 1,
                FirstName = "test",
                LastName = "test",
                Email = "test@email.com",
            }
        };

        var expected = new Paging<Contact>
        {
            CurrentPage = 1,
            Items = contacts,
            TotalItems = 1,
            TotalPage = 1
        };

        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService.Setup(c => c.GetContactIdsByAuthorizationCodesAndAccountIdAsync(It.IsAny<List<string>>(), It.IsAny<int>(), It.IsAny<Pagination?>()))
            .ReturnsAsync(expected);
        var authorizationController = new AuthorizationController(authorizationService.Object);

        // Act
        var result = await authorizationController.GetContactIdsByAuthorizationCodesAndAccountId(["CORAPP001"], 1, null);

        // Assert
        var resultValue = result.Result as OkObjectResult;
        Assert.Equal(200, resultValue?.StatusCode);
        Assert.Equivalent(expected, resultValue.Value);
    }

    [Fact]
    public async Task GetContactIdsByAuthorizationCodes_ShouldReturnNotFoundException()
    {
        // Arrange
        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService.Setup(c => c.GetContactIdsByAuthorizationCodesAndAccountIdAsync(It.IsAny<List<string>>(), It.IsAny<int>(), It.IsAny<Pagination?>()))
            .ThrowsAsync(new NotFoundException(" ", " "));
        var authorizationController = new AuthorizationController(authorizationService.Object);

        // Act
        var result = async () => await authorizationController.GetContactIdsByAuthorizationCodesAndAccountId(["CORAPP001"], 1, null);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(result);
    }

    [Fact]
    public async Task GetContactIdsByAuthorizationCodesAndAccountIdSignatoryAsync_ShouldReturnOk()
    {
        // Arrange
        List<Contact> expected = new List<Contact>()
        {
            new Contact()
            {
                ContactId = 1,
                FirstName = "test",
                LastName = "test",
                Email = "test@email.com",
            }
        };

        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService.Setup(c => c.GetContactIdsByAccountIdSignatoryAsync(It.IsAny<int>()))
            .ReturnsAsync(expected);
        var authorizationController = new AuthorizationController(authorizationService.Object);

        // Act
        var result = await authorizationController.GetContactIdsByAccountIdSignatoryAsync(1);

        // Assert
        var resultValue = result.Result as OkObjectResult;
        Assert.Equal(200, resultValue?.StatusCode);
        Assert.Equivalent(expected, resultValue.Value);
    }

    [Fact]
    public async Task GetContactIdsByAuthorizationCodesAndAccountIdSignatoryAsync_ShouldReturnNotFoundException()
    {
        // Arrange
        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService.Setup(c => c.GetContactIdsByAccountIdSignatoryAsync(It.IsAny<int>()))
            .ThrowsAsync(new NotFoundException(" ", " "));
        var authorizationController = new AuthorizationController(authorizationService.Object);

        // Act
        var result = async () => await authorizationController.GetContactIdsByAccountIdSignatoryAsync(1);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(result);
    }

    [Fact]
    public async Task SetPermissionForContactEmailAsync_ReturnsOk_WhenFileIsValid()
    {
        // Arrange
        var permission = "COGED0002";
        var content = "email@example.com";
        var fileName = "emails.csv";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var formFile = new FormFile(stream, 0, stream.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/csv"
        };
        var authServiceMock = new Mock<IAuthorizationService>();
        var controller = new AuthorizationController(authServiceMock.Object);
        authServiceMock
            .Setup(x => x.SetPermissionForContactEmailAsync(permission, formFile))
            .Returns(Task.CompletedTask);

        // Act
        var result = await controller.SetPermissionForContactEmailAsync(permission, formFile);

        // Assert
        Assert.IsType<OkResult>(result);
        authServiceMock.Verify(x => x.SetPermissionForContactEmailAsync(permission, formFile), Times.Once);
    }

    [Fact]
    public async Task SetPermissionForContactEmailAsync_ThrowsBadRequest_WhenFileIsNull()
    {
        // Arrange
        var permission = "COGED0002";
        var authServiceMock = new Mock<IAuthorizationService>();
        var controller = new AuthorizationController(authServiceMock.Object);

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await controller.SetPermissionForContactEmailAsync(permission, null!));
    }

    [Fact]
    public async Task SetPermissionForContactEmailAsync_ThrowsBadRequest_WhenFileIsEmpty()
    {
        // Arrange
        var permission = "admin";
        var emptyStream = new MemoryStream();
        var emptyFile = new FormFile(emptyStream, 0, 0, "file", "empty.csv");
        var authServiceMock = new Mock<IAuthorizationService>();
        var controller = new AuthorizationController(authServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await controller.SetPermissionForContactEmailAsync(permission, emptyFile));
    }
}
