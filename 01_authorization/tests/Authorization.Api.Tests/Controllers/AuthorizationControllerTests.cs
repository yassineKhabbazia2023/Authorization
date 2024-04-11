// <copyright file="AccountControllerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using Pulse.Authorization.Core.Models;
using AutoFixture;
using Microsoft.AspNetCore.JsonPatch;
using FluentAssertions;
using Kpmg.ExceptionMiddleware.AdvancedException;
using Pulse.Authorization.API;
using Pulse.Authorization.Core.Interfaces;
using Pulse.Authorization.Core.Models.Paging;

namespace Account.Api.Tests.Controllers
{
    public class AuthorizationControllerTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly Mock<IAuthorizationService> _authorizationService;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public AuthorizationControllerTests()
        {
            _authorizationService = new Mock<IAuthorizationService>(MockBehavior.Strict);
        }
    }
}
