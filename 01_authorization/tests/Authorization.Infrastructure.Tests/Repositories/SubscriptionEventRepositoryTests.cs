// <copyright file="SubscriptionEventRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Pulse.Authorization.Core.Interfaces;
using Pulse.Authorization.Core.Models.Subscriptions;
using Pulse.Authorization.Infrastructure.Context;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Interfaces;
using Pulse.Authorization.Infrastructure.Providers.Interfaces;
using Pulse.Authorization.Infrastructure.Repositories;
using Pulse.Authorization.Tests.Helpers;

namespace Pulse.Authorization.Infrastructure.Tests.Repositories;

public class SubscriptionEventRepositoryTests
{
    private readonly Fixture _fixture;
    private readonly Mock<IAuthorizationRepository> _authRepoMock;
    private readonly Mock<IContactRepository> _contactRepoMock;
    private readonly Mock<IRoleEventRepository> _roleEventRepoMock;
    private readonly SubscriptionEventRepository _repository;


    public SubscriptionEventRepositoryTests()
    {
        _fixture = EntityFixtureFactory.Create();
        _authRepoMock = new Mock<IAuthorizationRepository>();
        _contactRepoMock = new Mock<IContactRepository>();
        _roleEventRepoMock = new Mock<IRoleEventRepository>();

        _repository = new SubscriptionEventRepository(
            _authRepoMock.Object,
            _roleEventRepoMock.Object,
            _contactRepoMock.Object);
    }

    [Fact]
    public async Task AddSubscriptionAuthorizationsOnAccountAsync_ShouldAddAccountAuthorizationEntity()
    {
        // Arrange
        var authorizationRepository = new Mock<IAuthorizationRepository>(MockBehavior.Strict);
        var roleEventRepository = new Mock<IRoleEventRepository>();
        var contactRepository = new Mock<IContactRepository>();

        var options = new DbContextOptionsBuilder<AuthorizationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        var expectedResult = _fixture.CreateMany<AccountAuthorizationEntity>();
        authorizationRepository.Setup(r => r.AddSubscriptionAuthorizationsOnAccountAsync(It.IsAny<int>(), It.IsAny<List<string>>()))
        .ReturnsAsync(expectedResult);

        var repository = new SubscriptionEventRepository(authorizationRepository.Object, roleEventRepository.Object, contactRepository.Object);

        // Act
        var result = await repository.AddSubscriptionAuthorizationsOnAccountAsync(It.IsAny<int>(), It.IsAny<List<string>>());

        // Assert
        Assert.Equivalent(expectedResult, result);
    }

    [Fact]
    public async Task AddSubscriptionAuthorizationsOnContactAsync_ShouldAddContactAuthorizationEntity()
    {
        var authorizationRepository = new Mock<IAuthorizationRepository>(MockBehavior.Strict);
        var roleEventRepository = new Mock<IRoleEventRepository>();
        var contactRepository = new Mock<IContactRepository>();

        var options = new DbContextOptionsBuilder<AuthorizationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        var expectedResult = _fixture.CreateMany<ContactAuthorizationEntity>();
        authorizationRepository.Setup(r => r.AddSubscriptionAuthorizationsOnAccountSignatoriesAsync(It.IsAny<int>(), It.IsAny<List<string>>()))
        .ReturnsAsync(expectedResult);
        var repository = new SubscriptionEventRepository(authorizationRepository.Object, roleEventRepository.Object, contactRepository.Object);

        // Act
        var result = await repository.AddSubscriptionAuthorizationsOnAccountSignatoriesAsync(It.IsAny<int>(), It.IsAny<List<string>>());

        // Assert
        Assert.Equivalent(expectedResult, result);
    }


    [Fact]
    public async Task AddSubscriptionAuthorizationsForContacts_ShouldProcessCollabContacts_WithValidData()
    {
        // Arrange
        var contactIds = new List<int> { 1 };
        var accountId = 1;
        var productCodes = new List<string> { "P1" };
        var collabContacts = new List<int> { 1 };
        var collabAuthIds = new List<int> { 101, 102 };

        _contactRepoMock.Setup(x => x.RetrieveExistedContacts(contactIds))
            .Returns(new ContactsSubscription { CollabContacts = collabContacts });

        _roleEventRepoMock.Setup(x => x.RetrieveContactsHavingRole(contactIds, accountId))
            .Returns(new ContactRolesSubscription(accountId));

        _authRepoMock.Setup(x => x.RetrieveExistedProductCodes(productCodes))
            .Returns(new ProductCodesSubscriptions { CollabAuthorizationIds = collabAuthIds });

        var expectedEntities = _fixture.CreateMany<ContactAuthorizationEntity>();
        _authRepoMock.Setup(x => x.AddSubscriptionAuthorizationOnAccountContactsAsync(It.IsAny<IEnumerable<ContactAuthorizationEntity>>(), It.IsAny<int>()))
            .ReturnsAsync(expectedEntities);

        // Act
        var result = await _repository.AddSubscriptionAuthorizationsForContacts(contactIds, accountId, productCodes);

        // Assert
        Assert.Equal(expectedEntities, result.Item1);
        _authRepoMock.Verify(x => x.AddSubscriptionAuthorizationOnAccountContactsAsync(
            It.Is<IEnumerable<ContactAuthorizationEntity>>(entities =>
                entities.All(e => collabContacts.Contains(e.ContactId) &&
                                e.AccountId == accountId || e.AccountId == -1)), It.IsAny<int>()));
    }

    [Fact]
    public async Task AddSubscriptionAuthorizationsForContacts_ShouldProcessClientContacts_WithValidData()
    {
        // Arrange
        var contactIds = new List<int> { 2 };
        var accountId = 1;
        var productCodes = new List<string> { "P2" };
        var clientContacts = new List<int> { 2 };
        var clientAuthIds = new List<int> { 201, 202 };

        _contactRepoMock.Setup(x => x.RetrieveExistedContacts(contactIds))
            .Returns(new ContactsSubscription { ClientContacts = clientContacts });

        _roleEventRepoMock.Setup(x => x.RetrieveContactsHavingRole(contactIds, accountId))
            .Returns(new ContactRolesSubscription(accountId));

        _authRepoMock.Setup(x => x.RetrieveExistedProductCodes(productCodes))
            .Returns(new ProductCodesSubscriptions { ClientAuthorizationIds = clientAuthIds });

        var expectedEntities = _fixture.CreateMany<ContactAuthorizationEntity>();
        _authRepoMock.Setup(x => x.AddSubscriptionAuthorizationOnAccountContactsAsync(It.IsAny<IEnumerable<ContactAuthorizationEntity>>(), It.IsAny<int>()))
            .ReturnsAsync(expectedEntities);

        // Act
        var result = await _repository.AddSubscriptionAuthorizationsForContacts(contactIds, accountId, productCodes);

        // Assert
        Assert.Equal(expectedEntities, result.Item1);
        _authRepoMock.Verify(x => x.AddSubscriptionAuthorizationOnAccountContactsAsync(
            It.Is<IEnumerable<ContactAuthorizationEntity>>(entities =>
                entities.All(e => clientContacts.Contains(e.ContactId) &&
                                e.AccountId == accountId)), It.IsAny<int>()));
    }

    [Fact]
    public async Task AddSubscriptionAuthorizationsForContacts_ShouldNotCallAdd_WhenNoValidCombinations()
    {
        // Arrange
        var contactIds = new List<int> { 1 };
        var accountId = 1;
        var productCodes = new List<string> { "P1" };

        // Contacts exist but no matching auth IDs
        _contactRepoMock.Setup(x => x.RetrieveExistedContacts(contactIds))
            .Returns(new ContactsSubscription { CollabContacts = contactIds });

        _authRepoMock.Setup(x => x.RetrieveExistedProductCodes(productCodes))
            .Returns(new ProductCodesSubscriptions()); // Empty auth IDs

        _roleEventRepoMock.Setup(x => x.RetrieveContactsHavingRole(contactIds, accountId)).Returns(new ContactRolesSubscription(accountId) { ExistedContactRoles = [], UnexistedContactRoles = new List<int> { 1 } });
        // Act
        var result = await _repository.AddSubscriptionAuthorizationsForContacts(contactIds, accountId, productCodes);

        // Assert
        Assert.Empty(result.Item1);
        _authRepoMock.Verify(x => x.AddSubscriptionAuthorizationOnAccountContactsAsync(It.IsAny<IEnumerable<ContactAuthorizationEntity>>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task AddSubscriptionAuthorizationsForContacts_ShouldHandleMixedValidAndInvalidData()
    {
        // Arrange
        var contactIds = new List<int> { 1, 2, 99 };
        var accountId = 1;
        var productCodes = new List<string> { "P1", "INVALID" };
        var collabContacts = new List<int> { 1 };
        var clientContacts = new List<int> { 2 };
        var collabAuthIds = new List<int> { 101 };
        var clientAuthIds = new List<int> { 201 };

        _contactRepoMock.Setup(x => x.RetrieveExistedContacts(contactIds))
            .Returns(new ContactsSubscription
            {
                CollabContacts = collabContacts,
                ClientContacts = clientContacts,
                UnexistedContacts = new List<int> { 99 }
            });

        _roleEventRepoMock.Setup(x => x.RetrieveContactsHavingRole(contactIds, accountId))
            .Returns(new ContactRolesSubscription(accountId));

        _authRepoMock.Setup(x => x.RetrieveExistedProductCodes(productCodes))
            .Returns(new ProductCodesSubscriptions
            {
                CollabAuthorizationIds = collabAuthIds,
                ClientAuthorizationIds = clientAuthIds,
                UnexistedCodes = new List<string> { "INVALID" }
            });

        var expectedEntities = _fixture.CreateMany<ContactAuthorizationEntity>(2); // 1 collab + 1 client
        _authRepoMock.Setup(x => x.AddSubscriptionAuthorizationOnAccountContactsAsync(It.IsAny<IEnumerable<ContactAuthorizationEntity>>(), It.IsAny<int>()))
            .ReturnsAsync(expectedEntities);

        // Act
        var result = await _repository.AddSubscriptionAuthorizationsForContacts(contactIds, accountId, productCodes);

        // Assert
        Assert.Equal(2, result.Item1.Count());
    }

}
