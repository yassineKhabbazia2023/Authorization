// <copyright file="MapToContactEntityTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Back.Events.IntegrationEvents.EventsData;
using Pulse.Authorization.Infrastructure.Entities;
using Pulse.Authorization.Infrastructure.Mappers.EventMappers;

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
            Assert.Equal(updatedContact.ContactId, existingContact.ContactId);
            Assert.Equal(updatedContact.FirstName, existingContact.FirstName);
            Assert.Equal(updatedContact.LastName, existingContact.LastName);
            Assert.Equal(updatedContact.Email, existingContact.Email);
            Assert.Equal(updatedContact.Status, existingContact.Status);
            Assert.Equal(updatedContact.Type, existingContact.Type);
            Assert.NotNull(existingContact.LastUpdateDate);
        }
    }
}
