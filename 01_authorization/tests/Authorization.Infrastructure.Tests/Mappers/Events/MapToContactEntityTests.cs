// <copyright file="MapToContactEntityTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Back.Events.IntegrationEvents.EventsData;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Mappers.EventMappers;
using Xunit;

namespace Pulse.Authorization.Infrastructure.Tests.Mappers.Events
{
    public class MapToContactEntityTests
    {
        [Fact]
        public void ToContactEntity_MapsCorrectly()
        {
            // Arrange
            var source = new ContactStateEventData
            {
                ContactId = 100,
                FirstName = "Test",
                LastName = "User",
                Email = "test.user@example.com",
                Status = "Active",
                Type = "Test Type",
            };

            // Act
            var result = source.ToContactEntity();

            // Assert
            Assert.Equal(source.ContactId, result.ContactId);
            Assert.Equal(source.FirstName, result.FirstName);
            Assert.Equal(source.LastName, result.LastName);
            Assert.Equal(source.Email, result.Email);
            Assert.Equal(source.Status, result.Status);
            Assert.Equal(source.Type, result.Type);
        }

        [Fact]
        public void ToContactEntity_MapsToDestinationCorrectly()
        {
            // Arrange
            Guid contactGlobalUniqueId = Guid.NewGuid();

            var updatedContact = new ContactEntity
            {
                ContactId = 100,
                FirstName = "Modified Test",
                LastName = "Modified User",
                Email = "test.user@example.com",
                Status = "active",
                Type = "Test Type",
            };

            var existingContact = new ContactEntity
            {
                ContactId = 100,
                FirstName = "Test",
                LastName = "User",
                Email = "test.user@example.com",
                Status = "declared",
                Type = "Test Type",
            };

            // Act
            updatedContact.ToContactEntity(existingContact);

            // Assert
            Assert.Equal(updatedContact.ContactId, updatedContact.ContactId);
            Assert.Equal(updatedContact.FirstName, updatedContact.FirstName);
            Assert.Equal(updatedContact.LastName, updatedContact.LastName);
            Assert.Equal(updatedContact.Email, updatedContact.Email);
            Assert.Equal(updatedContact.Status, updatedContact.Status);
            Assert.Equal(updatedContact.Type, updatedContact.Type);
        }
    }
}
