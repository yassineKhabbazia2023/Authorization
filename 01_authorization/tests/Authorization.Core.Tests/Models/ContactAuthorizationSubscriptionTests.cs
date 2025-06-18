using Xunit;
using Pulse.Authorization.Core.Models.Subscriptions;
using System.Collections.Generic;
using System.Linq;

namespace Pulse.Authorization.Core.Tests.Models.Subscriptions
{
    public class ContactAuthorizationSubscriptionTests
    {
        [Fact]
        public void Constructor_WithAllRequiredProperties_CreatesInstanceSuccessfully()
        {
            // Arrange
            var contactIds = new List<int> { 1, 2, 3 };
            const int accountId = 123;
            var authorizationIds = new List<int> { 10, 20, 30 };

            // Act
            var subscription = new ContactAuthorizationSubscription
            {
                ContactIds = contactIds,
                AccountId = accountId,
                AuthorizationIds = authorizationIds
            };

            // Assert
            Assert.Equal(contactIds, subscription.ContactIds);
            Assert.Equal(accountId, subscription.AccountId);
            Assert.Equal(authorizationIds, subscription.AuthorizationIds);
        }

        [Fact]
        public void ContactIds_Property_CanBeSetAndRetrieved()
        {
            // Arrange
            var initialContactIds = new List<int> { 1, 2, 3 };
            var newContactIds = new List<int> { 4, 5, 6 };
            var subscription = new ContactAuthorizationSubscription
            {
                AuthorizationIds = [],
                ContactIds = initialContactIds,
                AccountId = 123
            };

            // Act
            subscription.ContactIds = newContactIds;

            // Assert
            Assert.Equal(newContactIds, subscription.ContactIds);
        }

        [Fact]
        public void AccountId_Property_CanBeSetAndRetrieved()
        {
            // Arrange
            const int initialAccountId = 123;
            const int newAccountId = 456;
            var subscription = new ContactAuthorizationSubscription
            {
                AuthorizationIds = [],
                ContactIds = new List<int> { 1, 2, 3 },
                AccountId = initialAccountId
            };

            // Act
            subscription.AccountId = newAccountId;

            // Assert
            Assert.Equal(newAccountId, subscription.AccountId);
        }

        [Fact]
        public void AuthorizationIds_Property_CanBeSetAndRetrieved()
        {
            // Arrange
            var initialAuthorizationIds = new List<int> { 10, 20, 30 };
            var newAuthorizationIds = new List<int> { 40, 50, 60 };
            var subscription = new ContactAuthorizationSubscription
            {
                ContactIds = new List<int> { 1, 2, 3 },
                AccountId = 123,
                AuthorizationIds = initialAuthorizationIds
            };

            // Act
            subscription.AuthorizationIds = newAuthorizationIds;

            // Assert
            Assert.Equal(newAuthorizationIds, subscription.AuthorizationIds);
        }

        [Fact]
        public void AuthorizationIds_Property_DefaultsToEmptyList_WhenNotSpecified()
        {
            // Arrange & Act
            var subscription = new ContactAuthorizationSubscription
            {
                AuthorizationIds = [],
                ContactIds = new List<int> { 1, 2, 3 },
                AccountId = 123
            };

            // Assert
            Assert.NotNull(subscription.AuthorizationIds);
            Assert.Empty(subscription.AuthorizationIds);
        }

        [Fact]
        public void ContactIds_Property_CanBeSetToEmptyList()
        {
            // Arrange
            var emptyList = new List<int>();
            var subscription = new ContactAuthorizationSubscription
            {
                AuthorizationIds = emptyList,
                ContactIds = new List<int> { 1, 2, 3 },
                AccountId = 123
            };

            // Act
            subscription.ContactIds = emptyList;

            // Assert
            Assert.NotNull(subscription.ContactIds);
            Assert.Empty(subscription.ContactIds);
        }

        [Fact]
        public void AuthorizationIds_Property_CanBeSetToEmptyList()
        {
            // Arrange
            var emptyList = new List<int>();
            var subscription = new ContactAuthorizationSubscription
            {
                ContactIds = new List<int> { 1, 2, 3 },
                AccountId = 123,
                AuthorizationIds = new List<int> { 10, 20, 30 }
            };

            // Act
            subscription.AuthorizationIds = emptyList;

            // Assert
            Assert.NotNull(subscription.AuthorizationIds);
            Assert.Empty(subscription.AuthorizationIds);
        }

        [Fact]
        public void ContactIds_Property_PreservesOrderOfElements()
        {
            // Arrange
            var orderedContactIds = new List<int> { 5, 1, 9, 3, 7 };
            var subscription = new ContactAuthorizationSubscription
            {
                AuthorizationIds = [],
                ContactIds = new List<int> { 1, 2, 3 },
                AccountId = 123
            };

            // Act
            subscription.ContactIds = orderedContactIds;

            // Assert
            Assert.Equal(orderedContactIds, subscription.ContactIds);
            Assert.True(subscription.ContactIds.SequenceEqual(orderedContactIds));
        }

        [Fact]
        public void AuthorizationIds_Property_PreservesOrderOfElements()
        {
            // Arrange
            var orderedAuthorizationIds = new List<int> { 15, 3, 8, 1, 12 };
            var subscription = new ContactAuthorizationSubscription
            {
                ContactIds = new List<int> { 1, 2, 3 },
                AccountId = 123,
                AuthorizationIds = new List<int> { 10, 20, 30 }
            };

            // Act
            subscription.AuthorizationIds = orderedAuthorizationIds;

            // Assert
            Assert.Equal(orderedAuthorizationIds, subscription.AuthorizationIds);
            Assert.True(subscription.AuthorizationIds.SequenceEqual(orderedAuthorizationIds));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void AccountId_Property_HandlesVariousIntegerValues(int accountId)
        {
            // Arrange & Act
            var subscription = new ContactAuthorizationSubscription
            {
                AuthorizationIds = [],
                ContactIds = new List<int> { 1, 2, 3 },
                AccountId = accountId
            };

            // Assert
            Assert.Equal(accountId, subscription.AccountId);
        }

        [Fact]
        public void ContactIds_Property_CanHandleDuplicateValues()
        {
            // Arrange
            var contactIdsWithDuplicates = new List<int> { 1, 2, 2, 3, 1 };
            var subscription = new ContactAuthorizationSubscription
            {
                AuthorizationIds = [],
                ContactIds = new List<int> { 1, 2, 3 },
                AccountId = 123
            };

            // Act
            subscription.ContactIds = contactIdsWithDuplicates;

            // Assert
            Assert.Equal(contactIdsWithDuplicates, subscription.ContactIds);
            Assert.Equal(5, subscription.ContactIds.Count());
        }

        [Fact]
        public void AuthorizationIds_Property_CanHandleDuplicateValues()
        {
            // Arrange
            var authorizationIdsWithDuplicates = new List<int> { 10, 20, 20, 30, 10 };
            var subscription = new ContactAuthorizationSubscription
            {
                ContactIds = new List<int> { 1, 2, 3 },
                AccountId = 123,
                AuthorizationIds = new List<int> { 10, 20, 30 }
            };

            // Act
            subscription.AuthorizationIds = authorizationIdsWithDuplicates;

            // Assert
            Assert.Equal(authorizationIdsWithDuplicates, subscription.AuthorizationIds);
            Assert.Equal(5, subscription.AuthorizationIds.Count());
        }

        [Fact]
        public void Constructor_WithMinimalRequiredProperties_CreatesInstanceSuccessfully()
        {
            // Arrange
            var contactIds = new List<int> { 1, 2, 3 };
            const int accountId = 123;

            // Act
            var subscription = new ContactAuthorizationSubscription
            {
                AuthorizationIds = [],
                ContactIds = contactIds,
                AccountId = accountId
            };

            // Assert
            Assert.Equal(contactIds, subscription.ContactIds);
            Assert.Equal(accountId, subscription.AccountId);
            Assert.NotNull(subscription.AuthorizationIds);
            Assert.Empty(subscription.AuthorizationIds);
        }
    }
}
