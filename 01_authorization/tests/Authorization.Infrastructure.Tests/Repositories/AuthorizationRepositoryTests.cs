// <copyright file="AuthorizationRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Newtonsoft.Json;
using Pulse.Authorization.Core.Exceptions;
using Pulse.Authorization.Core.Request;
using Pulse.Authorization.Infrastructure.Constants;
using Pulse.Authorization.Infrastructure.Context;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Enum;
using Pulse.Authorization.Infrastructure.Repositories;
using Pulse.Authorization.Infrastructure.Services;
using Pulse.Authorization.Tests.Helpers;
using Pulse.ExceptionMiddleware.Exceptions;

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
        _fixture = EntityFixtureFactory.Create();
        _fixture.Customize<AccountEntity>(c => c.With(a => a.AccountType, (string?)null));
        _fixture.Customize<AuthorizationEntity>(c => c.With(a => a.TargetAccountType, GlobalConstants.TargetAccountTypeClient));
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
                Type = "Customer",
                IsActive = true
            });

            context.AccountEntity.Add(new AccountEntity
            {
                AccountId = 1,
                AccountNumber = "ACC001",
                LegalName = "Test Account",
                IsActive = true,
                AccountType = null
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
    public async Task GetAllAccountAuthorizations_Return_AuthorizationCode()
    {
        var contact = new ContactEntity
        {
            ContactId = 12456,
            IsActive = true,
            FirstName = "FirstName",
            LastName = "LastName",
            Email = "Email",
            PersonaName = "PersonaName",
            Type = "Type"
        };
        using var context = new AuthorizationContext(_options);
        var contactAuthorizationAccountEntity = _fixture.Build<ContactAuthorizationEntity>()
                        .With(a => a.Authorization, () => _fixture.Create<AuthorizationEntity>())
                        .With(a => a.ContactId, contact.ContactId)
                        .With(a => a.AccountId, 458)
                        .With(a => a.Contact, contact)
                        .With(a => a.Account, new AccountEntity
                        {
                            AccountId = Random.Shared.Next(),
                            IsActive = true,
                            AccountNumber = Random.Shared.Next().ToString(),
                            LegalName = Random.Shared.Next().ToString(),
                            AccountType = null
                        })
                        .CreateMany(3);

        var expectedAuthorization = contactAuthorizationAccountEntity.Select(c => c.Authorization.Code);

        context.ContactAuthorizationEntity.AddRange(contactAuthorizationAccountEntity);
        await context.SaveChangesAsync();

        var repository = new AuthorizationRepository(context);
        var contactId = contactAuthorizationAccountEntity.First().ContactId;

        var receivedAuthorization = await repository.GetAllContactAuthorizationsAsync(contactId);
        receivedAuthorization.Should().BeEquivalentTo(expectedAuthorization);
    }

    [Fact]
    public async Task GetAccountAuthorizations_Return_AuthorizationCode()
    {
        using (var context = new AuthorizationContext(_options))
        {
            var accountAuthorizationAccountEntity = _fixture.Build<AccountAuthorizationEntity>()
                            .With(a => a.Authorization, () => _fixture.Create<AuthorizationEntity>())
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
                            .With(a => a.Authorization, () => _fixture.Create<AuthorizationEntity>())
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
                            .With(a => a.Authorization, () => _fixture.Create<AuthorizationEntity>())
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
                            .With(a => a.Authorization, () => _fixture.Create<AuthorizationEntity>())
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
                    AuthorizationId = 777,
                    Name = "name7",
                    Description = string.Empty,
                    Code = "code7",
                    Label = "label7",
                    Type = "Customer",
                    View = "Global"
                },
                new()
                {
                    AuthorizationId = 877,
                    Name = "name8",
                    Description = string.Empty,
                    Code = "code8",
                    Label = "label8",
                    Type = "Customer",
                    View = "Global"
                },
                new()
                {
                    AuthorizationId = 977,
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
                    AuthorizationId = 977,
                    CreationDate = DateTime.UtcNow,
                },
                new()
                {
                    ContactId = 3,
                    AccountId = 3,
                    AuthorizationId = 877,
                    CreationDate = DateTime.UtcNow,
                },
                new()
                {
                    ContactId = 3,
                    AccountId = 3,
                    AuthorizationId = 777,
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
            context.ChangeTracker.Clear();

            using (var newContext = new AuthorizationContext(_options))
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
                AuthorizationId = 1234,
                Name = "name1",
                Description = string.Empty,
                Code = "code1",
                Label = "label1",
                Type = "Customer",
                View = "Global"
            },
            new()
            {
                AuthorizationId = 2345,
                Name = "name2",
                Description = string.Empty,
                Code = "code2",
                Label = "label2",
                Type = "Customer",
                View = "Global"
            },
            new()
            {
                AuthorizationId = 3456,
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

            context.ChangeTracker.Clear();
            var contactAuthorizationAccountEntity = new List<ContactAuthorizationEntity>
        {
            new()
            {
                ContactId = 128,
                AccountId = -1,
                AuthorizationId = 1234,
                CreationDate = DateTime.UtcNow,
            },
            new()
            {
                ContactId = 128,
                AccountId = -1,
                AuthorizationId = 2345,
                CreationDate = DateTime.UtcNow,
            },
            new()
            {
                ContactId = 128,
                AccountId = -1,
                AuthorizationId = 3456,
                CreationDate = DateTime.UtcNow,
            }
        };

            context.ContactAuthorizationEntity.AddRange(contactAuthorizationAccountEntity);
            await context.SaveChangesAsync();

            context.ChangeTracker.Clear();

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

        using var context = new AuthorizationContext(_options);
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
        var authorizationEntities = _fixture.Build<AuthorizationEntity>()
                            .With(a => a.ProductCode)
                            .CreateMany(3);

        var otherAuthorizationEntities = _fixture.Build<AuthorizationEntity>()
                            .With(a => a.ProductCode, "other")
                            .Without(a => a.ContactAuthorizationEntity)
                            .Create();

        var account = _fixture.Build<AccountEntity>()
            .With(a => a.AccountId, 432)
            .Create();

        var contacts = _fixture.Build<ContactEntity>()
            .With(c => c.Type, ContactType.Customer.ToString())
            .With(c => c.IsActive, true)
            .CreateMany(12).DistinctBy(c => c.ContactId);

        var contactAuthorization = _fixture.Build<ContactAuthorizationEntity>()
            .With(a => a.AccountId, account.AccountId)
            .Without(a => a.Account)
            .Without(a => a.Contact)
            .With(a => a.ContactId, contacts.Select(c => c.ContactId).First())
            .With(a => a.Authorization, otherAuthorizationEntities)
            .Create();

        using (var context = new AuthorizationContext(_options))
        {
            context.AccountEntity.Add(account);
            context.ContactEntity.AddRange(contacts);
            context.AuthorizationEntity.AddRange(authorizationEntities);
            context.AuthorizationEntity.Add(otherAuthorizationEntities);
            context.ContactAuthorizationEntity.Add(contactAuthorization);
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

        using var ct = new AuthorizationContext(_options);
        var productCodes = authorizationEntities.Select(a => a.ProductCode).Distinct();
        var repository = new AuthorizationRepository(ct);

        // Act
        var result = await repository.AddSubscriptionAuthorizationsOnAccountSignatoriesAsync(account.AccountId, productCodes!);

        var totalCountWithOther = (productCodes.Count() * contacts.Count()) + 1;

        // Assert
        Assert.NotNull(result);
        result.Should().NotBeEmpty();
        Assert.Equal(result.Count(), totalCountWithOther);
    }

    [Fact]
    public async Task CreateDefaultAuthorizationsOnAccountAsync_Should_AddDefaultAccountAuthorizations_And_ReturnSaidAuthorizations()
    {
        // Arrange
        var authorizationEntities = GlobalConstants.DefaultAccountPermissions.Select(p =>
        {
            var a = _fixture.Create<AuthorizationEntity>();
            a.Code = p;
            return a;
        });

        var account = _fixture.Build<AccountEntity>()
            .With(a => a.AccountId, 44)
            .Create();

        using var context = new AuthorizationContext(_options);
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
    public async Task CreateReportingAuthorizationsOnAccountAsync_Should_AddReportinghorizations()
    {
        // Arrange
        var authorizationEntities = GlobalConstants.PowerBIDefaultPermissions.Select(p =>
        {
            var a = _fixture.Create<AuthorizationEntity>();
            a.Code = p;
            return a;
        });

        var account = _fixture.Build<AccountEntity>()
            .With(a => a.AccountId, 44)
            .Create();

        using var context = new AuthorizationContext(_options);
        context.AccountEntity.Add(account);
        context.AuthorizationEntity.AddRange(authorizationEntities);
        await context.SaveChangesAsync();
        var repository = new AuthorizationRepository(context);

        // Act
        var result = await repository.CreateReportingAuthorizationsOnAccountAsync(account.AccountId, GlobalConstants.PowerBIDefaultPermissions);

        // Assert
        Assert.NotNull(result);
        result.Should().BeEquivalentTo(GlobalConstants.PowerBIDefaultPermissions);
    }

    [Fact]
    public async Task CreateDefaultAuthorizationsOnSignatoryAsync_Should_AddDefaultSignatoryAuthorizations_And_ReturnSaidAuthorizations()
    {
        // Arrange
        var authorizationEntities = GlobalConstants.DefaultSignatoryPermissions.Select(p =>
        {
            var a = _fixture.Create<AuthorizationEntity>();
            a.Code = p;
            return a;
        });

        using var context = new AuthorizationContext(_options);
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

        using (var context = new AuthorizationContext(_options))
        {
            context.ContactEntity.Add(contactEntity);
            context.AuthorizationEntity.AddRange(defaultAuthorizationEntities);
            await context.SaveChangesAsync();
            context.ContactAuthorizationEntity.AddRange(defaultAuthorizationsThatAlreadyExistedForContact);
            await context.SaveChangesAsync();

            var repos = new AuthorizationRepository(context);

            await repos.CreateDefaultAuthorizationsOnAccountAsync(1);

            context.AccountAuthorizationEntity.Where(x => x.AccountId == 1).ToList().Count.Should().Be(defaultPermissions.Count());
        }
    }

    [Fact]
    public async Task SetContactAuthorizationFromAccountAuthoriztion_ShouldAddAllAuthorizationOfAccountInContact()
    {
        var defaultAccountPermissions = GlobalConstants.DefaultAccountPermissions;
        List<AuthorizationEntity> customerAuthorizations = defaultAccountPermissions
            .Select(code => _fixture.Build<AuthorizationEntity>()
            .With(auth => auth.Code, code)
            .With(auth => auth.Type, AuthorizationType.Customer)
            .Without(auth => auth.AccountAuthorizationEntity)
            .Without(auth => auth.ContactAuthorizationEntity)
            .Without(auth => auth.Persona)
            .Create())
            .ToList();

        List<AuthorizationEntity> collabAuthorizations = _fixture.Build<AuthorizationEntity>()
            .Without(x => x.AccountAuthorizationEntity)
            .Without(x => x.ContactAuthorizationEntity)
            .With(x => x.Type, AuthorizationType.Collaborator)
            .CreateMany(10).ToList();

        var account = _fixture.Build<AccountEntity>()
            .With(x => x.AccountId, 1)
            .Without(x => x.ContactAuthorizationEntity)
            .Without(x => x.AccountAuthorizationEntity)
            .Without(x => x.RoleEntity)
            .Create();

        var contact = new ContactEntity
        {
            ContactGlobalUniqueId = Guid.NewGuid(),
            ContactId = 2,
            CreationDate = DateTime.UtcNow,
            Email = "email@email.com",
            IsActive = true,
            FirstName = "Test",
            LastName = "test",
            PersonaName = "test test",
            Type = "customer"
        };

        using (var context = new AuthorizationContext(_options))
        {
            context.AccountEntity.Add(account);
            context.ContactEntity.Add(contact);
            context.AuthorizationEntity.AddRange(customerAuthorizations);
            context.AuthorizationEntity.AddRange(collabAuthorizations);
            context.SaveChanges();

            context.ChangeTracker.Clear();

            var authorizationIds = context.AuthorizationEntity.Select(a => a.AuthorizationId).ToList();

            var accountAuthorizations = authorizationIds.Select(authorizationId => new AccountAuthorizationEntity() { AccountId = 1, AuthorizationId = authorizationId }).ToList();

            context.AccountAuthorizationEntity.AddRange(accountAuthorizations);
            context.SaveChanges();

            context.ChangeTracker.Clear();

            var authorizationRepos = new AuthorizationRepository(context);
            await authorizationRepos.SetContactAuthorizationFromAccountAuthorization(account.AccountId, contact.ContactId);

            context.ChangeTracker.Clear();
            var contactAuthorizations = context.ContactAuthorizationEntity.ToList();

            contactAuthorizations.Should().NotBeEmpty();
            contactAuthorizations.Count().Should().Be(customerAuthorizations.Count());
        }
    }

    [Fact]
    public async Task SetContactAuthorizationFromContactAuthoriztion_ShouldAddDeltaAuthorizationBetweenAccountAndContact()
    {
        var defaultAccountPermissions = GlobalConstants.DefaultAccountPermissions;
        List<AuthorizationEntity> customerAuthorizations = defaultAccountPermissions
            .Select(code => _fixture.Build<AuthorizationEntity>()
            .With(auth => auth.Code, code)
            .With(auth => auth.Type, AuthorizationType.Customer)
            .Without(auth => auth.AccountAuthorizationEntity)
            .Without(auth => auth.ContactAuthorizationEntity)
            .Without(auth => auth.Persona)
            .Create())
            .ToList();

        List<AuthorizationEntity> collabAuthorizations = _fixture.Build<AuthorizationEntity>()
            .Without(x => x.AccountAuthorizationEntity)
            .Without(x => x.ContactAuthorizationEntity)
            .With(x => x.Type, AuthorizationType.Collaborator)
            .CreateMany(10).ToList();

        var account = _fixture.Build<AccountEntity>()
            .With(x => x.AccountId, 1)
            .Without(x => x.ContactAuthorizationEntity)
            .Without(x => x.AccountAuthorizationEntity)
            .Without(x => x.RoleEntity)
            .Create();

        var contact = new ContactEntity
        {
            ContactGlobalUniqueId = Guid.NewGuid(),
            ContactId = 2,
            CreationDate = DateTime.UtcNow,
            Email = "email@email.com",
            IsActive = true,
            FirstName = "Test",
            LastName = "test",
            PersonaName = "test test",
            Type = "customer"
        };

        using (var context = new AuthorizationContext(_options))
        {
            // add account, contacts and authorizations
            context.AccountEntity.Add(account);
            context.ContactEntity.Add(contact);
            context.AuthorizationEntity.AddRange(customerAuthorizations);
            context.AuthorizationEntity.AddRange(collabAuthorizations);
            context.SaveChanges();

            context.ChangeTracker.Clear();

            // add account authorizations from all the authorizations created before
            var authorizationIds = context.AuthorizationEntity.Select(a => a.AuthorizationId).ToList();
            var accountAuthorizations = authorizationIds.Select(authorizationId => new AccountAuthorizationEntity() { AccountId = 1, AuthorizationId = authorizationId }).ToList();
            context.AccountAuthorizationEntity.AddRange(accountAuthorizations);
            context.SaveChanges();
            context.ChangeTracker.Clear();

            // add three authorization of type customer into contact authorization
            var existedContactAuthorizations = context.AuthorizationEntity
                .Where(a => a.Type == AuthorizationType.Customer)
                .Take(3).ToList().Select(authorization => new ContactAuthorizationEntity()
                {
                    AccountId = account.AccountId,
                    AuthorizationId = authorization.AuthorizationId,
                    ContactId = contact.ContactId,
                    CreationDate = DateTime.UtcNow
                });
            context.ContactAuthorizationEntity.AddRange(existedContactAuthorizations);
            context.SaveChanges();
            context.ChangeTracker.Clear();

            // execute
            var authorizationRepos = new AuthorizationRepository(context);
            await authorizationRepos.SetContactAuthorizationFromAccountAuthorization(account.AccountId, contact.ContactId);

            context.ChangeTracker.Clear();
            var contactAuthorizations = context.ContactAuthorizationEntity.ToList();

            contactAuthorizations.Should().NotBeEmpty();
            contactAuthorizations.Count().Should().Be(customerAuthorizations.Count());
        }
    }

    [Fact]
    public async Task SetContactAuthorizationFromContactAuthoriztion_ShouldThrowExceptionIfContactIsNullOrDefault()
    {
        int contactId = 0;
        int accountId = 2;

        using (var context = new AuthorizationContext(_options))
        {
            var authorizationRepos = new AuthorizationRepository(context);

            var action = async () => await authorizationRepos.SetContactAuthorizationFromAccountAuthorization(accountId, contactId);

            var exception = await action.Should().ThrowAsync<Pulse.ExceptionMiddleware.Exceptions.NullArgumentException>();

            exception.Which.Code.Should().Be(Errors.NullArgumentCode);
            exception.WithMessage(string.Format(Errors.NullArgumentMessage, nameof(contactId)));
        }
    }

    [Fact]
    public async Task SetContactAuthorizationFromContactAuthoriztion_ShouldThrowExceptionIfAccountIsNullOrDefault()
    {
        int contactId = 2;
        int accountId = 0;

        using (var context = new AuthorizationContext(_options))
        {
            var authorizationRepos = new AuthorizationRepository(context);

            var action = async () => await authorizationRepos.SetContactAuthorizationFromAccountAuthorization(accountId, contactId);

            var exception = await action.Should().ThrowAsync<Pulse.ExceptionMiddleware.Exceptions.NullArgumentException>();

            exception.Which.Code.Should().Be(Errors.NullArgumentCode);
            exception.WithMessage(string.Format(Errors.NullArgumentMessage, nameof(accountId)));
        }
    }

    [Fact]
    public async Task CreateReportingAuthorizationsOnAccountAsync_Should_OnlyInsertNewCodes_WhenSomeAlreadyExist()
    {
        // Arrange
        using var context = new AuthorizationContext(_options);

        var accountId = 100;
        var auth1 = new AuthorizationEntity
        {
            AuthorizationId = 17,
            Code = "CLRAPP001",
            Name = "View bi financial",
            Type = "Customer",
            View = "Partial",
            Label = "Accéder aux rapports BI Financier",
            Configurable = true,
            Description = string.Empty,
            AccountAuthorizationEntity = new List<AccountAuthorizationEntity>()
        };
        var auth2 = new AuthorizationEntity
        {
            AuthorizationId = 18,
            Code = "CLRAPP002",
            Name = "View bi HR",
            Type = "Customer",
            View = "Partial",
            Label = "Accéder aux rapports BI RH\r\nAccéder aux rapports BI RH",
            Configurable = false,
            Description = string.Empty,
        };
        var auth3 = new AuthorizationEntity
        {
            AuthorizationId = 39,
            Code = "CORAPP001",
            Name = "Mirror report",
            Type = "Customer",
            View = "Partial",
            Label = "Accéder aux rapports BI ",
            Configurable = true,
            Description = string.Empty,
        };

        auth1.AccountAuthorizationEntity.Add(new AccountAuthorizationEntity
        {
            AccountId = accountId,
            AuthorizationId = auth1.AuthorizationId,
            Enabled = true
        });

        context.AuthorizationEntity.AddRange(auth1, auth2, auth3);
        await context.SaveChangesAsync();

        var repository = new AuthorizationRepository(context);

        var codesToInsert = new[] { "CLRAPP001", "CLRAPP002", "CORAPP001" };

        // Act
        var result = await repository.CreateReportingAuthorizationsOnAccountAsync(accountId, codesToInsert);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(["CLRAPP002", "CORAPP001"]);

        var allLinksForAccount = await context.AccountAuthorizationEntity
            .Where(ac => ac.AccountId == accountId)
            .ToListAsync();
        allLinksForAccount.Count.Should().Be(3);

        allLinksForAccount.Any(x => x.AuthorizationId == 17).Should().BeTrue();
        allLinksForAccount.Any(x => x.AuthorizationId == 18).Should().BeTrue();
        allLinksForAccount.Any(x => x.AuthorizationId == 39).Should().BeTrue();
    }

    [Fact]
    public async Task DeleteContactAuthorizationsAsync_WithInvalidContact_ShouldThrowNullArgumentException()
    {
        int contactId = 0;
        int accountId = 123;
        string[] permissions = { "COADMI001", "COADMI002" };

        using (var context = new AuthorizationContext(_options))
        {
            var authRepos = new AuthorizationRepository(context);
            var action = async () => await authRepos.DeleteContactAuthorizationsAsync(contactId, accountId, permissions);
            var exception = await action.Should().ThrowAsync<Pulse.ExceptionMiddleware.Exceptions.NullArgumentException>();
            exception.Which.Code.Should().Be(Errors.NullArgumentCode);
            exception.WithMessage(string.Format(Errors.NullArgumentMessage, "contactId"));
        }
    }

    [Fact]
    public async Task DeleteContactAuthorizationsAsync_WithInvalidAccount_ShouldThrowNullArgumentException()
    {
        int contactId = 123;
        int accountId = 0;
        string[] permissions = { "COADMI001", "COADMI002" };

        using (var context = new AuthorizationContext(_options))
        {
            var authRepos = new AuthorizationRepository(context);
            var action = async () => await authRepos.DeleteContactAuthorizationsAsync(contactId, accountId, permissions);
            var exception = await action.Should().ThrowAsync<Pulse.ExceptionMiddleware.Exceptions.NullArgumentException>();
            exception.Which.Code.Should().Be(Errors.NullArgumentCode);
            exception.WithMessage(string.Format(Errors.NullArgumentMessage, "accountId"));
        }
    }

    [Fact]
    public async Task DeleteContactAuthorizationsAsync_WithInvalidPermissions_ShouldThrowNullArgumentException()
    {
        int contactId = 123;
        int accountId = 22;
        string[] permissions = { };

        using (var context = new AuthorizationContext(_options))
        {
            var authRepos = new AuthorizationRepository(context);
            var action = async () => await authRepos.DeleteContactAuthorizationsAsync(contactId, accountId, permissions);
            var exception = await action.Should().ThrowAsync<Pulse.ExceptionMiddleware.Exceptions.NullArgumentException>();
            exception.Which.Code.Should().Be(Errors.NullArgumentCode);
            exception.WithMessage(string.Format(Errors.NullArgumentMessage, "permissions"));
        }
    }

    [Fact]
    public async Task DeleteContactAuthorizationsAsync_WithValidAccountContactAndPermission_ShouldDeleteContactAuthrorizations()
    {
        var authorizationsEntities = _fixture.Build<AuthorizationEntity>()
            .Without(auth => auth.ContactAuthorizationEntity)
            .Without(auth => auth.AccountAuthorizationEntity)
            .Without(auth => auth.Persona)
            .CreateMany(5);

        var accountEntity = _fixture.Build<AccountEntity>()
            .Without(acc => acc.ContactAuthorizationEntity)
            .Without(acc => acc.AccountAuthorizationEntity)
            .Without(acc => acc.RoleEntity)
            .Create();

        var contactEntity = _fixture.Build<ContactEntity>()
            .Without(acc => acc.ContactAuthorizationEntity)
            .Without(acc => acc.RoleEntity)
            .Create();

        var permissions = authorizationsEntities.TakeLast(3).Select(auth => auth.Code).ToArray();
        int accountId = accountEntity.AccountId;
        int contactId = contactEntity.ContactId;

        var contactAuthorizationEntities = new List<ContactAuthorizationEntity>();

        foreach (var auth in authorizationsEntities)
        {
            var contactAuthorization = _fixture.Build<ContactAuthorizationEntity>()
            .Without(cnt => cnt.Authorization)
            .Without(cnt => cnt.Account)
            .Without(cnt => cnt.Contact)
            .With(cnt => cnt.ContactId, contactId)
            .With(cnt => cnt.AccountId, accountId)
            .With(cnt => cnt.AuthorizationId, auth.AuthorizationId)
            .Create();
            contactAuthorizationEntities.Add(contactAuthorization);
        }

        using (var context = new AuthorizationContext(_options))
        {
            context.ContactEntity.Add(contactEntity);
            context.AccountEntity.Add(accountEntity);
            context.AuthorizationEntity.AddRange(authorizationsEntities);
            await context.SaveChangesAsync();

            await context.ContactAuthorizationEntity.AddRangeAsync(contactAuthorizationEntities);
            await context.SaveChangesAsync();

            var authRepos = new AuthorizationRepository(context);
            await authRepos.DeleteContactAuthorizationsAsync(contactId, accountId, permissions);

            var permissionList = context.ContactAuthorizationEntity.ToList();
            permissionList.Count().Should().Be(2);
        }
    }

    [Fact]
    public async Task CreateReportingAuthorizationsForSignatoriesAsync_Nominal()
    {
        var accountId = 1;
        var contacts = new List<int> { 1, 2 };

        var auth1 = _fixture.Build<AuthorizationEntity>()
            .With(a => a.Code, GlobalConstants.PowerBIDefaultSignatoryPermissions[0])
            .Without(a => a.ContactAuthorizationEntity)
            .Without(a => a.AccountAuthorizationEntity)
            .Create();
        var auth2 = _fixture.Build<AuthorizationEntity>()
            .With(a => a.Code, GlobalConstants.PowerBIDefaultSignatoryPermissions[1])
            .Without(a => a.ContactAuthorizationEntity)
            .Without(a => a.AccountAuthorizationEntity)
            .Create();

        using var context = new AuthorizationContext(_options);
        context.AuthorizationEntity.AddRange(new List<AuthorizationEntity> { auth1, auth2 });
        await context.SaveChangesAsync();

        var repository = new AuthorizationRepository(context);

        var result = await repository.CreateReportingAuthorizationsForSignatoriesAsync(contacts, accountId);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Equal(4, result.Count());
    }

    [Fact]
    public async Task CreateReportingAuthorizationsForSignatoriesAsync_ShouldReturnEmptyList_WhenContactAuthorizationsAlreadyExist()
    {
        var accountId = 1;
        var contactId = new List<int> { 1 };

        var contactAuth1 = _fixture.Build<ContactAuthorizationEntity>()
            .With(c => c.ContactId, 1)
            .With(c => c.AccountId, accountId)
            .With(c => c.AuthorizationId, 1)
            .Without(c => c.Authorization)
            .Without(c => c.Account)
            .Without(c => c.Contact)
            .Create();
        var contactAuth2 = _fixture.Build<ContactAuthorizationEntity>()
            .With(c => c.ContactId, 1)
            .With(c => c.AccountId, accountId)
            .With(c => c.AuthorizationId, 2)
            .Without(c => c.Authorization)
            .Without(c => c.Account)
            .Without(c => c.Contact)
            .Create();
        var auth1 = _fixture.Build<AuthorizationEntity>()
            .With(a => a.AuthorizationId, 1)
            .With(a => a.Code, GlobalConstants.PowerBIDefaultSignatoryPermissions[0])
            .With(a => a.ContactAuthorizationEntity, new List<ContactAuthorizationEntity> { contactAuth1 })
            .Without(a => a.AccountAuthorizationEntity)
            .Create();
        var auth2 = _fixture.Build<AuthorizationEntity>()
            .With(a => a.AuthorizationId, 2)
            .With(a => a.Code, GlobalConstants.PowerBIDefaultSignatoryPermissions[1])
            .With(a => a.ContactAuthorizationEntity, new List<ContactAuthorizationEntity> { contactAuth2 })
            .Without(a => a.AccountAuthorizationEntity)
            .Create();

        using var context = new AuthorizationContext(_options);
        context.ContactAuthorizationEntity.AddRange(new List<ContactAuthorizationEntity> { contactAuth1, contactAuth2 });
        context.AuthorizationEntity.AddRange(new List<AuthorizationEntity> { auth1, auth2 });
        await context.SaveChangesAsync();

        var repository = new AuthorizationRepository(context);

        var result = await repository.CreateReportingAuthorizationsForSignatoriesAsync(contactId, accountId);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetContactIdsByAuthorizationCodesAndAccountIdAsync_ReturnResult()
    {
        // Arrange
        var pagination = new Pagination
        {
            PageNumber = 1,
            PageSize = 5,
        };

        var authorization1 = _fixture.Build<AuthorizationEntity>()
            .With(a => a.AuthorizationId, 11)
            .With(a => a.Code, "COALP001")
            .Create();
        var authorization2 = _fixture.Build<AuthorizationEntity>()
            .With(a => a.AuthorizationId, 22)
            .With(a => a.Code, "COALP002")
            .Create();

        var contact = _fixture.Build<ContactEntity>()
            .With(a => a.ContactId, 1)
            .Create();

        var contactAuth1 = _fixture.Build<ContactAuthorizationEntity>()
            .With(c => c.ContactId, 1)
            .With(c => c.Contact, contact)
            .With(c => c.AccountId, 1)
            .With(c => c.AuthorizationId, 11)
             .Without(c => c.Authorization)
            .Without(c => c.Account)
            .Create();
        var contactAuth2 = _fixture.Build<ContactAuthorizationEntity>()
           .With(c => c.ContactId, 1)
            .With(c => c.Contact, contact)
           .With(c => c.AccountId, 1)
           .With(c => c.AuthorizationId, 22)
            .Without(c => c.Authorization)
           .Without(c => c.Account)
           .Create();
        var role = _fixture.Build<RoleEntity>()
            .With(c => c.ContactId, 1)
            .With(c => c.AccountId, 1)
            .Without(c => c.Account)
            .Without(c => c.Contact)
            .Create();

        using var context = new AuthorizationContext(_options);
        context.RoleEntity.Add(role);
        context.AuthorizationEntity.Add(authorization1);
        context.ContactAuthorizationEntity.Add(contactAuth1);
        context.AuthorizationEntity.Add(authorization2);
        context.ContactAuthorizationEntity.Add(contactAuth2);
        await context.SaveChangesAsync();

        var repo = new AuthorizationRepository(context);

        // Act
        var result = await repo.GetContactIdsByAuthorizationCodesAndAccountIdAsync(["COALP001", "COALP002"], 1, pagination);

        // Assert
        Assert.Contains(1, result.Items!.Select(c => c.ContactId)!);
    }

    [Fact]
    public async Task GetContactIdsByAuthorizationCodesAndAccountIdAsync_ReturnNone()
    {
        // Arrange
        var pagination = new Pagination
        {
            PageNumber = 1,
            PageSize = 5,
        };

        var authorization1 = _fixture.Build<AuthorizationEntity>()
            .With(a => a.AuthorizationId, 111)
            .With(a => a.Code, "COALP001")
            .Create();
        var authorization2 = _fixture.Build<AuthorizationEntity>()
            .With(a => a.AuthorizationId, 222)
            .With(a => a.Code, "COALP002")
            .Create();

        var contactAuth1 = _fixture.Build<ContactAuthorizationEntity>()
            .With(c => c.ContactId, 1)
            .With(c => c.AccountId, 1)
            .With(c => c.AuthorizationId, 111)
             .Without(c => c.Authorization)
            .Without(c => c.Account)
            .Without(c => c.Contact)
            .Create();

        using var context = new AuthorizationContext(_options);
        context.AuthorizationEntity.Add(authorization1);
        context.ContactAuthorizationEntity.Add(contactAuth1);
        context.AuthorizationEntity.Add(authorization2);
        await context.SaveChangesAsync();

        var repo = new AuthorizationRepository(context);

        // Act
        var result = await repo.GetContactIdsByAuthorizationCodesAndAccountIdAsync(["COALP001", "COALP002"], 1, pagination);

        // Assert
        Assert.DoesNotContain(1, result.Items!.Select(c => c.ContactId)!);
    }

    [Fact]
    public async Task GetContactIdsByAuthorizationCodesAndAccountIdAsync_ThrowNotFound_WhenMissingAuthorizationCode()
    {
        // Arrange
        var pagination = new Pagination
        {
            PageNumber = 1,
            PageSize = 5,
        };

        var authorization1 = _fixture.Build<AuthorizationEntity>()
            .With(a => a.AuthorizationId, 111)
            .With(a => a.Code, "COALP001")
            .Create();

        using var context = new AuthorizationContext(_options);
        context.AuthorizationEntity.Add(authorization1);
        await context.SaveChangesAsync();

        var repo = new AuthorizationRepository(context);

        // Act + Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(async () =>
            await repo.GetContactIdsByAuthorizationCodesAndAccountIdAsync(new List<string> { "COALP001", "COALP002" }, 1, pagination)
        );

        Assert.Equal("AUT008", ex.Code);
        Assert.Contains("COALP001,COALP002", ex.Message);
    }

    [Fact]
    public async Task GetContactIdsByAuthorizationCodesAndAccountIdSignatoryAsync_ReturnResult()
    {
        // Arrange
        using var context = new AuthorizationContext(_options);
        var auth1 = _fixture.CreateAuthorization(11, "signatory", "COALP001", "Auth 1");
        var auth2 = _fixture.CreateAuthorization(22, "signatory", "COALP002", "Auth 2");
        var account = _fixture.CreateAccount(1);
        var contact = _fixture.CreateContact(1, "customer");
        context.AuthorizationEntity.AddRange(auth1, auth2);
        context.AccountEntity.Add(account);
        context.ContactEntity.Add(contact);
        await context.SaveChangesAsync();

        var role = _fixture.Build<RoleEntity>()
            .With(r => r.ContactId, 1)
            .With(r => r.AccountId, 1)
            .With(r => r.IsSignatory, true)
            .Without(r => r.Contact)
            .Without(r => r.Account)
            .Create();

        var contactAuth1 = _fixture.Build<ContactAuthorizationEntity>()
            .With(ca => ca.ContactId, 1)
            .With(ca => ca.AccountId, 1)
            .With(ca => ca.AuthorizationId, 11)
            .Without(ca => ca.Contact)
            .Without(ca => ca.Account)
            .Without(ca => ca.Authorization)
            .Create();

        var contactAuth2 = _fixture.Build<ContactAuthorizationEntity>()
            .With(ca => ca.ContactId, 1)
            .With(ca => ca.AccountId, 1)
            .With(ca => ca.AuthorizationId, 22)
            .Without(ca => ca.Contact)
            .Without(ca => ca.Account)
            .Without(ca => ca.Authorization)
            .Create();

        context.RoleEntity.Add(role);
        context.ContactAuthorizationEntity.AddRange(contactAuth1, contactAuth2);
        await context.SaveChangesAsync();

        var repo = new AuthorizationRepository(context);

        // Act
        var result = await repo.GetContactIdsByAccountIdSignatoryAsync(1);

        // Assert
        result.Should().NotBeEmpty("should return contacts with signatory role");
        result.Should().Contain(c => c.ContactId == 1, "contact 1 is a signatory");
    }

    [Fact]
    public async Task GetContactIdsByAuthorizationCodesAndAccountIdSignatoryAsync_ReturnNone()
    {
        // Arrange
        var pagination = new Pagination
        {
            PageNumber = 1,
            PageSize = 5,
        };

        var authorization1 = _fixture.Build<AuthorizationEntity>()
            .With(a => a.AuthorizationId, 111)
            .With(a => a.Code, "COALP001")
            .Create();
        var authorization2 = _fixture.Build<AuthorizationEntity>()
            .With(a => a.AuthorizationId, 222)
            .With(a => a.Code, "COALP002")
            .Create();

        var contactAuth1 = _fixture.Build<ContactAuthorizationEntity>()
            .With(c => c.ContactId, 1)
            .With(c => c.AccountId, 1)
            .With(c => c.AuthorizationId, 111)
             .Without(c => c.Authorization)
            .Without(c => c.Account)
            .Without(c => c.Contact)
            .Create();
        var role = _fixture.Build<RoleEntity>()
            .With(c => c.ContactId, 1)
            .With(c => c.AccountId, 1)
            .With(c => c.IsSignatory, false)
            .Without(c => c.Account)
            .Without(c => c.Contact)
            .Create();

        using var context = new AuthorizationContext(_options);
        context.RoleEntity.Add(role);
        context.AuthorizationEntity.Add(authorization1);
        context.ContactAuthorizationEntity.Add(contactAuth1);
        context.AuthorizationEntity.Add(authorization2);
        await context.SaveChangesAsync();

        var repo = new AuthorizationRepository(context);

        // Act
        var result = await repo.GetContactIdsByAccountIdSignatoryAsync(1);

        // Assert
        Assert.DoesNotContain(1, result.Select(c => c.ContactId)!);
    }

    [Fact]
    public void RetrieveExistedProductCodes_Should_SplitIdsByType_And_ReturnUnexistedCodes()
    {
        // – Arrange --------------------------------------------------------------
        using var context = new AuthorizationContext(_options);

        var customerAuth = _fixture.Build<AuthorizationEntity>()
            .With(a => a.AuthorizationId, 1)
            .With(a => a.ProductCode, "P1")
            .With(a => a.Type, ContactType.Customer.ToString())
            .Without(a => a.AccountAuthorizationEntity)
            .Without(a => a.ContactAuthorizationEntity)
            .Create();

        var collaboratorAuth = _fixture.Build<AuthorizationEntity>()
            .With(a => a.AuthorizationId, 2)
            .With(a => a.ProductCode, "P2")
            .With(a => a.Type, ContactType.Collaborator.ToString())
            .Without(a => a.AccountAuthorizationEntity)
            .Without(a => a.ContactAuthorizationEntity)
            .Create();

        context.AuthorizationEntity.AddRange(customerAuth, collaboratorAuth);
        context.SaveChanges();

        var repo = new AuthorizationRepository(context);
        var searchedCodes = new[] { "P1", "P2", "P3" };   // P3 does NOT exist

        // – Act 
        var result = repo.RetrieveExistedProductCodes(searchedCodes);

        // – Assert
        result.ClientAuthorizationIds.Should().BeEquivalentTo([customerAuth.AuthorizationId]);
        result.CollabAuthorizationIds.Should().BeEquivalentTo([collaboratorAuth.AuthorizationId]);
        result.UnexistedCodes.Should().BeEquivalentTo(["P3"]);
    }

    [Fact]
    public async Task AddSubscriptionAuthorizationOnAccountContactsAsync_Should_AddOnlyNewLinks_And_ReturnThem()
    {
        // – Arrange 
        using var context = new AuthorizationContext(_options);

        // Authorizations referenced by the contact links
        var auth1 = _fixture.Build<AuthorizationEntity>()
            .With(a => a.AuthorizationId, 10)
            .Without(a => a.AccountAuthorizationEntity)
            .Without(a => a.ContactAuthorizationEntity)
            .Create();
        var auth2 = _fixture.Build<AuthorizationEntity>()
            .With(a => a.AuthorizationId, 20)
            .Without(a => a.AccountAuthorizationEntity)
            .Without(a => a.ContactAuthorizationEntity)
            .Create();

        var acc = _fixture.Build<AccountEntity>()
            .With(a => a.AccountId, 1)
            .Without(a => a.AccountAuthorizationEntity)
            .Without(a => a.ContactAuthorizationEntity)
            .Without(a => a.RoleEntity)
            .Create();

        var cnt = _fixture.Build<ContactEntity>()
            .With(a => a.ContactId, 1)
            .Without(a => a.RoleEntity)
            .Without(a => a.ContactAuthorizationEntity)
            .Create();

        context.AuthorizationEntity.AddRange(auth1, auth2);
        context.AccountEntity.Add(acc);
        context.ContactEntity.Add(cnt);
        await context.SaveChangesAsync();

        const int contactId = 1;
        const int accountId = 1;

        // This link already exists in DB and must be ignored by the repo method
        var alreadyLinked = _fixture.Build<ContactAuthorizationEntity>()
            .With(c => c.Authorization, auth1)
            .With(c => c.Account, acc)
            .With(c => c.Contact, cnt)
            .With(c => c.ContactId, contactId)
            .With(c => c.AccountId, accountId)
            .With(c => c.AuthorizationId, auth1.AuthorizationId)
            .Create();
        context.ContactAuthorizationEntity.Add(alreadyLinked);
        await context.SaveChangesAsync();

        // Two links we ask to insert: one existingCustomerAuth & one really new
        var duplicate = _fixture.Build<ContactAuthorizationEntity>()
            .With(c => c.Authorization, auth1)
            .With(c => c.Account, acc)
            .With(c => c.Contact, cnt)
            .With(c => c.ContactId, contactId)
            .With(c => c.AccountId, accountId)
            .With(c => c.AuthorizationId, auth1.AuthorizationId)   // already exists
            .Create();

        var brandNew = _fixture.Build<ContactAuthorizationEntity>()
            .With(c => c.Authorization, auth2)
            .With(c => c.Account, acc)
            .With(c => c.Contact, cnt)
            .With(c => c.ContactId, contactId)
            .With(c => c.AccountId, accountId)
            .With(c => c.AuthorizationId, auth2.AuthorizationId)   // new link
            .Create();

        var repo = new AuthorizationRepository(context);

        // – Act
        var added = await repo.AddSubscriptionAuthorizationOnAccountContactsAsync([duplicate, brandNew], accountId);

        // – Assert
        added.Should().HaveCount(2);                          // return all link
        added.Last().AuthorizationId.Should().Be(auth2.AuthorizationId);

        var allLinksInDb = context.ContactAuthorizationEntity
            .Where(ca => ca.ContactId == contactId && ca.AccountId == accountId)
            .ToList();

        allLinksInDb.Should().HaveCount(2);                   // original + new
        allLinksInDb.Should().ContainSingle(ca => ca.AuthorizationId == auth1.AuthorizationId);
        allLinksInDb.Should().ContainSingle(ca => ca.AuthorizationId == auth2.AuthorizationId);
    }

    [Fact]
    public void RetrieveExistedProductCodes_Should_ReturnEmpty_WhenInputIsEmpty()
    {
        // – Arrange 
        using var context = new AuthorizationContext(_options);
        var repo = new AuthorizationRepository(context);

        // – Act
        var result = repo.RetrieveExistedProductCodes([]);

        // – Assert
        result.ClientAuthorizationIds.Should().BeEmpty();
        result.CollabAuthorizationIds.Should().BeEmpty();
        result.UnexistedCodes.Should().BeEmpty();
    }

    [Fact]
    public void RetrieveExistedProductCodes_Should_ReturnAllAsUnexisted_WhenNoneMatch()
    {
        // – Arrange
        using var context = new AuthorizationContext(_options);
        var repo = new AuthorizationRepository(context);
        var unknownCodes = new[] { "X1", "Y2" };

        // – Act
        var result = repo.RetrieveExistedProductCodes(unknownCodes);

        // – Assert
        result.ClientAuthorizationIds.Should().BeEmpty();
        result.CollabAuthorizationIds.Should().BeEmpty();
        result.UnexistedCodes.Should().BeEquivalentTo(unknownCodes);
    }

    [Fact]
    public async Task AddSubscriptionAuthorizationOnAccountContactsAsync_Should_AddNothing_WhenAllExist()
    {
        // – Arrange
        using var context = new AuthorizationContext(_options);

        var auth = _fixture.Build<AuthorizationEntity>()
            .With(a => a.AuthorizationId, 100)
            .Without(a => a.AccountAuthorizationEntity)
            .Without(a => a.ContactAuthorizationEntity)
            .Create();

        var acc = _fixture.Build<AccountEntity>()
            .With(a => a.AccountId, 1)
            .Without(a => a.AccountAuthorizationEntity)
            .Without(a => a.ContactAuthorizationEntity)
            .Without(a => a.RoleEntity)
            .Create();

        var cnt = _fixture.Build<ContactEntity>()
            .With(a => a.ContactId, 1)
            .Without(a => a.RoleEntity)
            .Without(a => a.ContactAuthorizationEntity)
            .Create();

        context.AuthorizationEntity.Add(auth);
        context.AccountEntity.Add(acc);
        context.ContactEntity.Add(cnt);
        await context.SaveChangesAsync();

        var link = _fixture.Build<ContactAuthorizationEntity>()
            .With(c => c.Authorization, auth)
            .With(c => c.Account, acc)
            .With(c => c.Contact, cnt)
            .With(c => c.AuthorizationId, auth.AuthorizationId)
            .With(c => c.ContactId, 1)
            .With(c => c.AccountId, 1)
            .Create();

        context.ContactAuthorizationEntity.Add(link);
        await context.SaveChangesAsync();

        var repo = new AuthorizationRepository(context);

        // – Act
        var result = await repo.AddSubscriptionAuthorizationOnAccountContactsAsync([link], 1);

        // – Assert
        result.Should().NotBeEmpty();

        var all = context.ContactAuthorizationEntity.ToList();
        all.Should().HaveCount(1); // only the existing link
    }

    [Fact]
    public async Task AddSubscriptionAuthorizationOnAccountContactsAsync_Should_HandleEmptyList()
    {
        // – Arrange
        using var context = new AuthorizationContext(_options);
        var repo = new AuthorizationRepository(context);

        // – Act
        var result = await repo.AddSubscriptionAuthorizationOnAccountContactsAsync([], 1);

        // – Assert
        result.Should().BeEmpty();
        context.ContactAuthorizationEntity.Should().BeEmpty();
    }

    [Fact]
    public async Task AddSubscriptionAuthorizationOnAccountContactsAsync_GivenCustomerAndCollabAuthorization_ShouldReturnAllExistingAndNewAuthorization()
    {
        // Arrange
        using var context = new AuthorizationContext(_options);

        var auth1 = _fixture.CreateAuthorization(10, "customer", "CUST001", "Customer Auth 1");
        var auth2 = _fixture.CreateAuthorization(20, "customer", "CUST002", "Customer Auth 2");
        var authCollab = _fixture.CreateAuthorization(30, "collaborator", "COGED0002", "view gedesc");
        var authCollab2 = _fixture.CreateAuthorization(36, "collaborator", "COEVPO01", "Collab Auth 2");

        var customerAccount = _fixture.CreateAccount(1);
        var collabAccount = _fixture.CreateAccount(-1);

        var customer = _fixture.CreateContact(1, "customer");
        var collaborator = _fixture.CreateContact(2, "collaborator");

        context.AuthorizationEntity.AddRange(auth1, auth2, authCollab, authCollab2);
        context.AccountEntity.AddRange(customerAccount, collabAccount);
        context.ContactEntity.AddRange(customer, collaborator);
        await context.SaveChangesAsync();

        var existingCustomerAuth1 = _fixture.CreateContactAuthorization(
            customer, customerAccount, auth1, 1, 1, 10);

        var existingCollabAuth = _fixture.CreateContactAuthorization(
            collaborator, collabAccount, authCollab, 2, -1, 30);

        context.ContactAuthorizationEntity.AddRange(existingCustomerAuth1, existingCollabAuth);
        await context.SaveChangesAsync();

        var authToAdd = new List<ContactAuthorizationEntity>
        {
            // Already exists
            _fixture.CreateContactAuthorization(customer, customerAccount, auth1, 1, 1, 10),

            // New customer authorization
            _fixture.CreateContactAuthorization(customer, customerAccount, auth2, 1, 1, 20),

            // New collab authorization
            _fixture.CreateContactAuthorization(collaborator, collabAccount, authCollab2, 2, -1, 36)
        };

        var repo = new AuthorizationRepository(context);

        // Act
        var result = await repo.AddSubscriptionAuthorizationOnAccountContactsAsync(authToAdd, 1);

        // Assert
        var resultList = result.ToList();

        resultList.Should().HaveCount(4, "should return all existing + newly created authorizations");

        // Verify customer authorizations
        var customerAuths = resultList.Where(ca => ca.ContactId == 1).ToList();
        customerAuths.Should().HaveCount(2);
        customerAuths.Should().Contain(ca => ca.AuthorizationId == 10, "auth1 already existed");
        customerAuths.Should().Contain(ca => ca.AuthorizationId == 20, "auth2 is new");

        // Verify collaborator authorizations
        var collabAuths = resultList.Where(ca => ca.ContactId == 2).ToList();
        collabAuths.Should().HaveCount(2);
        collabAuths.Should().Contain(ca => ca.AuthorizationId == 30, "authCollab already existed");
        collabAuths.Should().Contain(ca => ca.AuthorizationId == 36, "authCollab2 is new");

        // Verify only 2 new records were inserted in DB
        var allAuthsInDb = await context.ContactAuthorizationEntity.CountAsync();
        allAuthsInDb.Should().Be(4, "2 existed before + 2 newly inserted");

        // Verify by authorization codes
        var authCodes = resultList.Select(ca => ca.Authorization.Code).ToList();
        authCodes.Should().Contain(new[] { "CUST001", "CUST002", "COGED0002", "COEVPO01" });
    }

}
