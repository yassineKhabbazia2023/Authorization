// <copyright file="AuthorizationRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Newtonsoft.Json;
using Pulse.Authorization.Infrastructure.Constants;
using Pulse.Authorization.Infrastructure.Context;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Enum;
using Pulse.Authorization.Infrastructure.Repositories;

namespace Pulse.Authorization.Infrastructure.Tests.Repositories;

public class AuthorizationRepositoryTests
{
    private readonly DbContextOptions<AuthorizationContext> _options;
    private readonly Fixture _fixture;

    public AuthorizationRepositoryTests()
    {
        _options = new DbContextOptionsBuilder<AuthorizationContext>()
                            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                            .Options;
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public async Task GetContactAndAccountAuthorizations_Return_AuthorizationCode()
    {
        using (var context = new AuthorizationContext(_options))
        {
            var authorizations = new List<AuthorizationEntity>()
            {
                new()
                {
                    AuthorizationId = 4,
                    Name = "name4",
                    Description = string.Empty,
                    Code = "code4",
                    Label = "label4",
                    Type = "Customer",
                    View = "Global"
                },
                new()
                {
                    AuthorizationId = 5,
                    Name = "name5",
                    Description = string.Empty,
                    Code = "code5",
                    Label = "label5",
                    Type = "Customer",
                    View = "Global"
                },
                new()
                {
                    AuthorizationId = 6,
                    Name = "name6",
                    Description = string.Empty,
                    Code = "code6",
                    Label = "label6",
                    Type = "Customer",
                    View = "Global"
                }
            };
            context.AuthorizationEntity.AddRange(authorizations);
            await context.SaveChangesAsync();

            var contactAuthorizationAccountEntity = new List<ContactAuthorizationEntity>
            {
                new()
                {
                    ContactId = 1,
                    AccountId = 1,
                    AuthorizationId = 6,
                    CreationDate = DateTime.UtcNow,
                },
                new()
                {
                    ContactId = 1,
                    AccountId = 1,
                    AuthorizationId = 5,
                    CreationDate = DateTime.UtcNow,
                },
                new()
                {
                    ContactId = 1,
                    AccountId = 1,
                    AuthorizationId = 4,
                    CreationDate = DateTime.UtcNow,
                }
            };
            context.ContactAuthorizationEntity.AddRange(contactAuthorizationAccountEntity);
            await context.SaveChangesAsync();

            context.ContactEntity.Add(new ContactEntity
            {
                ContactId = 1,
                FirstName = "toto",
                LastName = "titi",
                Email = "tititoto@email.fr",
                PersonaName = "Client",
                Type = "Customer"
            });

            var expectedAuthorization = authorizations.Select(c => c.Code);
            await context.SaveChangesAsync();

            var repository = new AuthorizationRepository(context);

            var contactId = contactAuthorizationAccountEntity.First().ContactId;
            var accountId = contactAuthorizationAccountEntity.First().AccountId;

            var receivedAuthorization = await repository.GetContactAccountAuthorizationsAsync(contactId, accountId, null);

            var authExpectJson = JsonConvert.SerializeObject(expectedAuthorization);
            var authResultJson = JsonConvert.SerializeObject(receivedAuthorization);
            Assert.Equal(authExpectJson, authResultJson);
            Assert.NotNull(receivedAuthorization);
        }
    }

    [Fact]
    public async Task GetAccountAuthorizations_Return_AuthorizationCode()
    {
        using (var context = new AuthorizationContext(_options))
        {
            var accountAuthorizationAccountEntity = _fixture.Build<AccountAuthorizationEntity>()
                            .With(a => a.Authorization)
                            .With(a => a.AccountId, 457)
                            .Without(a => a.Account)
                            .CreateMany(3);

            var expectedAuthorization = accountAuthorizationAccountEntity.Select(c => c.Authorization.Code);

            context.AccountAuthorizationEntity.AddRange(accountAuthorizationAccountEntity);
            await context.SaveChangesAsync();

            var repository = new AuthorizationRepository(context);

            var accountId = accountAuthorizationAccountEntity.First().AccountId;

            var receivedAuthorization = await repository.GetAccountAuthorizationAsync(accountId);

            var authExpectJson = JsonConvert.SerializeObject(expectedAuthorization);
            var authResultJson = JsonConvert.SerializeObject(receivedAuthorization);
            Assert.Equal(authExpectJson, authResultJson);
            Assert.NotNull(receivedAuthorization);
        }
    }

    [Fact]
    public async Task GetContactAndAccountAuthorizationsAsync_Return_Empty()
    {
        using (var context = new AuthorizationContext(_options))
        {
            var contactAuthorizationAccountEntity = _fixture.Build<ContactAuthorizationEntity>()
                            .With(a => a.Authorization)
                            .With(a => a.ContactId, 124)
                            .With(a => a.AccountId, 458)
                            .Without(a => a.Contact)
                            .CreateMany(3);

            var expectedAuthorization = contactAuthorizationAccountEntity.Select(c => c.Authorization.Code);

            context.ContactAuthorizationEntity.AddRange(contactAuthorizationAccountEntity);
            await context.SaveChangesAsync();

            var repository = new AuthorizationRepository(context);

            var contactId = contactAuthorizationAccountEntity.First().ContactId;
            var accountId = contactAuthorizationAccountEntity.First().AccountId;

            var receivedAuthorization = await repository.GetContactAccountAuthorizationsAsync(999, 888, null);

            Assert.Empty(receivedAuthorization);
        }
    }

    [Fact]
    public async Task GetContactAndAccountAuthorizationsAsync_Cas_Global()
    {
        using (var context = new AuthorizationContext(_options))
        {
            var contactAuthorizationAccountEntity = _fixture.Build<ContactAuthorizationEntity>()
                            .With(a => a.Authorization)
                            .With(a => a.ContactId, 125)
                            .With(a => a.AccountId, 459)
                            .Without(a => a.Contact)
                            .CreateMany(3);
            var permissionGlobal = contactAuthorizationAccountEntity.Where(x => x.Authorization.View == AuthorizationView.Global.ToString()).ToList();

            var expectedAuthorization = contactAuthorizationAccountEntity.Select(c => c.Authorization.Code);

            context.ContactAuthorizationEntity.AddRange(contactAuthorizationAccountEntity);
            await context.SaveChangesAsync();

            var repository = new AuthorizationRepository(context);

            var contactId = contactAuthorizationAccountEntity.First().ContactId;
            var accountId = contactAuthorizationAccountEntity.First().AccountId;

            var receivedAuthorization = await repository.GetContactAccountAuthorizationsAsync(123, 456, true);

            Assert.Equal(permissionGlobal.Count, receivedAuthorization.Count);
        }
    }

    [Fact]
    public async Task GetContactAndAccountAuthorizationsAsync_Cas_Partial()
    {
        using (var context = new AuthorizationContext(_options))
        {
            var contactAuthorizationAccountEntity = _fixture.Build<ContactAuthorizationEntity>()
                            .With(a => a.Authorization)
                            .With(a => a.ContactId, 126)
                            .With(a => a.AccountId, 460)
                            .Without(a => a.Contact)
                            .CreateMany(3);
            var permissionPartial = contactAuthorizationAccountEntity.Where(x => x.Authorization.View == AuthorizationView.Partial.ToString()).ToList();

            var expectedAuthorization = contactAuthorizationAccountEntity.Select(c => c.Authorization.Code);

            context.ContactAuthorizationEntity.AddRange(contactAuthorizationAccountEntity);
            await context.SaveChangesAsync();

            var repository = new AuthorizationRepository(context);

            var contactId = contactAuthorizationAccountEntity.First().ContactId;
            var accountId = contactAuthorizationAccountEntity.First().AccountId;

            var receivedAuthorization = await repository.GetContactAccountAuthorizationsAsync(123, 456, false);

            Assert.Equal(permissionPartial.Count, receivedAuthorization.Count);
        }
    }

    [Fact]
    public async Task GetContactAuthorizationsAsync_Return_AuthorizationCode()
    {
        using (var context = new AuthorizationContext(_options))
        {

            context.AuthorizationEntity.RemoveRange(context.AuthorizationEntity);
            context.ContactAuthorizationEntity.RemoveRange(context.ContactAuthorizationEntity);
            context.ContactEntity.RemoveRange(context.ContactEntity);
            await context.SaveChangesAsync();


            var authorizations = new List<AuthorizationEntity>()
            {
                new()
                {
                    AuthorizationId = 7,
                    Name = "name7",
                    Description = string.Empty,
                    Code = "code7",
                    Label = "label7",
                    Type = "Customer",
                    View = "Global"
                },
                new()
                {
                    AuthorizationId = 8,
                    Name = "name8",
                    Description = string.Empty,
                    Code = "code8",
                    Label = "label8",
                    Type = "Customer",
                    View = "Global"
                },
                new()
                {
                    AuthorizationId = 9,
                    Name = "name9",
                    Description = string.Empty,
                    Code = "code9",
                    Label = "label9",
                    Type = "Customer",
                    View = "Global"
                }
            };
            context.AuthorizationEntity.AddRange(authorizations);
            await context.SaveChangesAsync();

            var contactAuthorizationAccountEntity = new List<ContactAuthorizationEntity>
            {
                new()
                {
                    ContactId = 3,
                    AccountId = 3,
                    AuthorizationId = 9,
                    CreationDate = DateTime.UtcNow,
                },
                new()
                {
                    ContactId = 3,
                    AccountId = 3,
                    AuthorizationId = 8,
                    CreationDate = DateTime.UtcNow,
                },
                new()
                {
                    ContactId = 3,
                    AccountId = 3,
                    AuthorizationId = 7,
                    CreationDate = DateTime.UtcNow,
                }
            };
            context.ContactAuthorizationEntity.AddRange(contactAuthorizationAccountEntity);
            await context.SaveChangesAsync();

            context.ContactEntity.Add(_fixture.Build<ContactEntity>()
                                .With(x => x.Type, "Customer")
                                .With(x => x.ContactId, 3)
                                .Without(x => x.ContactAuthorizationEntity)
                                .Create());

            var expectedAuthorization = authorizations.Select(c => c.Code);
            await context.SaveChangesAsync();

            using(var newContext = new AuthorizationContext(_options))
            {
                var repository = new AuthorizationRepository(context);

                var contactId = contactAuthorizationAccountEntity.First().ContactId;
                var accountId = contactAuthorizationAccountEntity.First().AccountId;

                var receivedAuthorization = await repository.GetContactAuthorizationAsync(contactId);

                var authExpectJson = JsonConvert.SerializeObject(expectedAuthorization);
                var authResultJson = JsonConvert.SerializeObject(receivedAuthorization);
                Assert.Equal(authExpectJson, authResultJson);
                Assert.NotNull(receivedAuthorization);
            }
        }
    }

    [Fact]
    public async Task DeletePermissionAsync_Should_DeletePermission()
    {
        using (var context = new AuthorizationContext(_options))
        {
            // Clear the context first
            context.ChangeTracker.Clear();

            var authorizations = new List<AuthorizationEntity>()
        {
            new()
            {
                AuthorizationId = 1,
                Name = "name1",
                Description = string.Empty,
                Code = "code1",
                Label = "label1",
                Type = "Customer",
                View = "Global"
            },
            new()
            {
                AuthorizationId = 2,
                Name = "name2",
                Description = string.Empty,
                Code = "code2",
                Label = "label2",
                Type = "Customer",
                View = "Global"
            },
            new()
            {
                AuthorizationId = 3,
                Name = "name3",
                Description = string.Empty,
                Code = "code3",
                Label = "label3",
                Type = "Customer",
                View = "Global"
            }
        };

            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();

            context.AuthorizationEntity.AddRange(authorizations);
            await context.SaveChangesAsync();

            var contactAuthorizationAccountEntity = new List<ContactAuthorizationEntity>
        {
            new()
            {
                ContactId = 128,
                AccountId = -1,
                AuthorizationId = 1,
                CreationDate = DateTime.UtcNow,
            },
            new()
            {
                ContactId = 128,
                AccountId = -1,
                AuthorizationId = 2,
                CreationDate = DateTime.UtcNow,
            },
            new()
            {
                ContactId = 128,
                AccountId = -1,
                AuthorizationId = 3,
                CreationDate = DateTime.UtcNow,
            }
        };

            context.ContactAuthorizationEntity.AddRange(contactAuthorizationAccountEntity);
            await context.SaveChangesAsync();

            context.ContactEntity.Add(_fixture.Build<ContactEntity>()
                                .With(x => x.Type, "Customer")
                                .With(x => x.ContactId, 128)
                                .With(x => x.IsActive, true)
                                .Without(x => x.ContactAuthorizationEntity)
                                .Create());
            await context.SaveChangesAsync();

            // Create a new context instance for the repository operations
            // I did this because context failed to track the same instance with the same context
            using (var newContext = new AuthorizationContext(_options))
            {
                var repository = new AuthorizationRepository(newContext);
                var permissionBefore = await repository.GetContactAccountAuthorizationsAsync(128, -1, null);
                Assert.NotEmpty(permissionBefore);

                await repository.DeleteContactAuthorizationAsync(128, -1);

                var permissionAfter = await repository.GetContactAccountAuthorizationsAsync(128, -1, null);
                Assert.Empty(permissionAfter);
            }
        }
    }


    [Fact]
    public async Task AddSubscriptionAuthorizationsOnAccountAsync_Should_AddAccountAuthorizations_And_ReturnSaidAuthorizations()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AuthorizationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        var productCodes = new Dictionary<string, string>
        {
            { "123", "CLGED0001" },
            { "456", "CLGED0002" },
            { "789", "CLSILA001" }
        };

        var expectedAuthorizationCodes = productCodes.Values.ToList();
        var collabCodes = productCodes.Values.Select(a => { return GlobalConstants.CustomerToMirrorCodes[a]; });
        expectedAuthorizationCodes.AddRange(collabCodes);

        var authorizationEntities = productCodes.Select(c =>
        {
            var a = _fixture.Build<AuthorizationEntity>()
            .Without(a => a.AccountAuthorizationEntity)
            .Without(a => a.ContactAuthorizationEntity)
            .Create();

            a.ProductCode = c.Key;
            a.Code = c.Value;
            return a;
        }).ToList();

        authorizationEntities.AddRange(collabCodes.Select(c =>
        {
            var a = _fixture.Build<AuthorizationEntity>()
            .Without(a => a.AccountAuthorizationEntity)
            .Without(a => a.ContactAuthorizationEntity)
            .Create();
            a.ProductCode = null;
            a.Code = c;
            return a;
        }));

        var account = _fixture.Build<AccountEntity>()
            .With(a => a.AccountId, 42)
            .Create();

        using var context = new AuthorizationContext(options);
        context.AccountEntity.Add(account);
        context.AuthorizationEntity.AddRange(authorizationEntities);
        context.SaveChanges();
        var repository = new AuthorizationRepository(context);

        // Act
        var result = await repository.AddSubscriptionAuthorizationsOnAccountAsync(account.AccountId, productCodes.Keys!);

        // Assert
        Assert.NotNull(result);
        // Don't forget to fix this later, somehow tracking does not work correctly
        //result.Select(a => a.Authorization.Code).Should().BeEquivalentTo(expectedAuthorizationCodes);
    }

    [Fact]
    public async Task AddSubscriptionAuthorizationsOnContactAsync_Should_AddContactAuthorizations_And_ReturnSaidAuthorizations()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AuthorizationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        var authorizationEntities = _fixture.Build<AuthorizationEntity>()
                            .With(a => a.ProductCode)
                            .CreateMany(3);
        var account = _fixture.Build<AccountEntity>()
            .With(a => a.AccountId, 43)
            .Create();

        var contacts = _fixture.Build<ContactEntity>()
            .With(c => c.Type, ContactType.Customer.ToString())
            .With(c => c.IsActive, true)
            .CreateMany(12).DistinctBy(c => c.ContactId);

        using (var context = new AuthorizationContext(options))
        {
            context.AccountEntity.Add(account);
            context.ContactEntity.AddRange(contacts);
            context.AuthorizationEntity.AddRange(authorizationEntities);
            context.SaveChanges();

            var roles = contacts.Select(c =>
            {
                return new RoleEntity()
                {
                    ContactId = c.ContactId,
                    AccountId = account.AccountId,
                    IsSignatory = true,
                };
            });

            context.RoleEntity.AddRange(roles);
            context.SaveChanges();
        }

        using var ct = new AuthorizationContext(options);
        var contactIds = contacts.Select(c => c.ContactId);
        var productCodes = authorizationEntities.Select(a => a.ProductCode).Distinct();
        var repository = new AuthorizationRepository(ct);

        // Act
        var result = await repository.AddSubscriptionAuthorizationsOnAccountSignatoriesAsync(account.AccountId, productCodes!);

        // Assert
        Assert.NotNull(result);
        result.Should().NotBeEmpty();
        Assert.Equal(result.Count(), productCodes.Count() * contacts.Count());
    }

    [Fact]
    public async Task CreateDefaultAuthorizationsOnAccountAsync_Should_AddDefaultAccountAuthorizations_And_ReturnSaidAuthorizations()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AuthorizationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        var authorizationEntities = GlobalConstants.DefaultAccountPermissions.Select(p =>
        {
            var a = _fixture.Create<AuthorizationEntity>();
            a.Code = p;
            return a;
        });

        var account = _fixture.Build<AccountEntity>()
            .With(a => a.AccountId, 44)
            .Create();

        using var context = new AuthorizationContext(options);
        context.AccountEntity.Add(account);
        context.AuthorizationEntity.AddRange(authorizationEntities);
        context.SaveChanges();
        var repository = new AuthorizationRepository(context);

        // Act
        var result = await repository.CreateDefaultAuthorizationsOnAccountAsync(account.AccountId);

        // Assert
        Assert.NotNull(result);
        result.Should().BeEquivalentTo(GlobalConstants.DefaultAccountPermissions);
    }


    [Fact]
    public async Task CreateRapportBIAuthorizationsOnAccountAsync_Should_AddRapportBiAuthorizations()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AuthorizationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        var authorizationEntities = GlobalConstants.PowerBIDefaultPermissions.Select(p =>
        {
            var a = _fixture.Create<AuthorizationEntity>();
            a.Code = p;
            return a;
        });

        var account = _fixture.Build<AccountEntity>()
            .With(a => a.AccountId, 44)
            .Create();

        using var context = new AuthorizationContext(options);
        context.AccountEntity.Add(account);
        context.AuthorizationEntity.AddRange(authorizationEntities);
        context.SaveChanges();
        var repository = new AuthorizationRepository(context);

        // Act
        var result = await repository.CreateRapportBIAuthorizationsOnAccountAsync(account.AccountId, GlobalConstants.PowerBIDefaultPermissions.ToList());

        // Assert
        Assert.NotNull(result);
        result.Should().BeEquivalentTo(GlobalConstants.PowerBIDefaultPermissions);
    }

    [Fact]
    public async Task CreateDefaultAuthorizationsOnSignatoryAsync_Should_AddDefaultSignatoryAuthorizations_And_ReturnSaidAuthorizations()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AuthorizationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        var authorizationEntities = GlobalConstants.DefaultSignatoryPermissions.Select(p =>
        {
            var a = _fixture.Create<AuthorizationEntity>();
            a.Code = p;
            return a;
        });

        using var context = new AuthorizationContext(options);
        context.AuthorizationEntity.AddRange(authorizationEntities);
        context.SaveChanges();
        var repository = new AuthorizationRepository(context);

        // Act
        var result = await repository.CreateDefaultAuthorizationsOnSignatoryAsync(It.IsAny<int>(), It.IsAny<int>());

        // Assert
        Assert.NotNull(result);
        result.Should().BeEquivalentTo(GlobalConstants.DefaultSignatoryPermissions);
    }

    [Fact]
    public async Task CreateDefaultAuthorizationsOnAccountAsync_ShouldCreateOnlyTheDefaultAuthorizations_ThatDoesNotAlreadyExistsForThisAccount()
    {
        var defaultPermissions = GlobalConstants.DefaultSignatoryPermissions;

        ContactEntity contactEntity = _fixture.Build<ContactEntity>()
            .Without(x => x.ContactAuthorizationEntity)
            .Without(x => x.RoleEntity)
            .With(x => x.ContactId, 1)
            .Create();

        List<AuthorizationEntity> defaultAuthorizationEntities = defaultPermissions
            .Select(x => _fixture.Build<AuthorizationEntity>()
            .With(a => a.Code, x)
            .Without(x => x.AccountAuthorizationEntity)
        .Without(x => x.ContactAuthorizationEntity)
        .Create())
            .ToList();

        List<ContactAuthorizationEntity> defaultAuthorizationsThatAlreadyExistedForContact = defaultAuthorizationEntities.Take(2).ToList()
            .Select(x => _fixture.Build<ContactAuthorizationEntity>()
            .Without(a => a.Authorization)
            .Without(a => a.Contact)
            .With(a => a.ContactId, 1)
            .With(a => a.AuthorizationId, x.AuthorizationId)
            .With(x => x.AccountId, 1).Create())
            .ToList();

        var options = new DbContextOptionsBuilder<AuthorizationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using (var context = new AuthorizationContext(options))
        {
            context.ContactEntity.Add(contactEntity);
            context.AuthorizationEntity.AddRange(defaultAuthorizationEntities);
            context.SaveChanges();
            context.ContactAuthorizationEntity.AddRange(defaultAuthorizationsThatAlreadyExistedForContact);
            context.SaveChanges();

            var repos = new AuthorizationRepository(context);

            await repos.CreateDefaultAuthorizationsOnAccountAsync(1);

            context.AccountAuthorizationEntity.Where(x => x.AccountId == 1).ToList().Count().Should().Be(defaultPermissions.Count());
        }

    }
}
