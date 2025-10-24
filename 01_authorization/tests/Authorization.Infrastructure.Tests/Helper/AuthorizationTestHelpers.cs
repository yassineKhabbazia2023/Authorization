using AutoFixture;
using Pulse.Authorization.Infrastructure.Entities;

namespace Pulse.Authorization.Tests.Helpers;

public static class AuthorizationTestHelpers
{
    public static AuthorizationEntity CreateAuthorization(
        this IFixture fixture,
        int id,
        string type,
        string code,
        string label)
    {
        return fixture.Build<AuthorizationEntity>()
            .With(a => a.AuthorizationId, id)
            .With(a => a.Type, type)
            .With(a => a.Code, code)
            .With(a => a.Label, label)
            .Without(a => a.AccountAuthorizationEntity)
            .Without(a => a.ContactAuthorizationEntity)
            .Create();
    }

    public static AccountEntity CreateAccount(this IFixture fixture, int id)
    {
        return fixture.Build<AccountEntity>()
            .With(a => a.AccountId, id)
            .Without(a => a.AccountAuthorizationEntity)
            .Without(a => a.ContactAuthorizationEntity)
            .Without(a => a.RoleEntity)
            .Create();
    }

    public static ContactEntity CreateContact(this IFixture fixture, int id, string type)
    {
        return fixture.Build<ContactEntity>()
        .With(c => c.ContactId, id)
        .With(c => c.Type, type)
        .With(c => c.ContactGlobalUniqueId, Guid.NewGuid())
        .With(c => c.CreationDate, DateTime.UtcNow)
        .With(c => c.IsActive, true)
        .With(c => c.Email, $"email{id}@test.com")
        .With(c => c.FirstName, $"FirstName{id}")
        .With(c => c.LastName, $"LastName{id}")
        .With(c => c.PersonaName, $"Persona{id}")
        .Without(c => c.RoleEntity)
        .Without(c => c.ContactAuthorizationEntity)
        .Create();
    }

    public static ContactAuthorizationEntity CreateContactAuthorization(
        this IFixture fixture,
        ContactEntity contact,
        AccountEntity account,
        AuthorizationEntity authorization,
        int contactId,
        int accountId,
        int authorizationId)
    {
        return fixture.Build<ContactAuthorizationEntity>()
            .With(ca => ca.Contact, contact)
            .With(ca => ca.Account, account)
            .With(ca => ca.Authorization, authorization)
            .With(ca => ca.ContactId, contactId)
            .With(ca => ca.AccountId, accountId)
            .With(ca => ca.AuthorizationId, authorizationId)
            .Create();
    }
}
