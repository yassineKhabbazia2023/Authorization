// <copyright file="ConfigurationServiceTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Moq;
using Pulse.Authorization.Core.Exceptions;
using Pulse.Authorization.Core.Interfaces;
using Pulse.Authorization.Core.Models;
using Pulse.Authorization.Core.Services;

namespace Pulse.Authorization.Core.Tests.Services;

public class ConfigurationServiceTests
{
    private readonly Mock<IConfigurationRepository> _configurationRepository;

    private readonly Fixture _fixture;

    public ConfigurationServiceTests()
    {
        _configurationRepository = new Mock<IConfigurationRepository>(MockBehavior.Strict);
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public async Task GetContactConfigurationAsync_Should_ReturnsConfigurationList()
    {
        // Arrange
        var accountId = 123;
        var contactId = 456;
        var menuCodeMocked = _fixture.Create<List<Configuration>>();
        var contactMocked = _fixture.Build<Contact>()
                                    .With(c => c.ContactId, contactId)
                                    .Create();
        _configurationRepository.Setup(repository => repository.GetContactConfigurationAsync(contactId, accountId))
            .ReturnsAsync(menuCodeMocked);

        var contactRepository = new Mock<IContactRepository>(MockBehavior.Strict);
        contactRepository.Setup(repository => repository.GetContactByIdAsync(contactId))
            .ReturnsAsync(contactMocked);

        var configurationService = new ConfigurationService(_configurationRepository.Object, contactRepository.Object);

        // Act
        var resources = await configurationService.GetContactConfigurationAsync(contactId, accountId);

        // Assert
        Assert.Equal(menuCodeMocked, resources);
    }

    [Fact]
    public async Task GetNavigationAsync_Should_Throw_NotFoundException()
    {
        // Arrange
        var accountId = 123;
        var contactId = 234;
        var resourceMocked = _fixture.Create<List<Configuration>>();
        _configurationRepository.Setup(repository => repository.GetContactConfigurationAsync(contactId, accountId))
            .ReturnsAsync(resourceMocked);

        var contactRepository = new Mock<IContactRepository>(MockBehavior.Strict);
        contactRepository.Setup(repository => repository.GetContactByIdAsync(contactId))
            .ReturnsAsync((Contact)null!);

        var configurationService = new ConfigurationService(_configurationRepository.Object, contactRepository.Object);

        // Act
        var act = async () => await configurationService.GetContactConfigurationAsync(contactId, accountId);

        // Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(act);
        Assert.Equal(Errors.NotFoundContactCode, exception.Code);
        Assert.Equal(string.Format(Errors.NotFoundContactMessage, 234), exception.Message);
    }
}
