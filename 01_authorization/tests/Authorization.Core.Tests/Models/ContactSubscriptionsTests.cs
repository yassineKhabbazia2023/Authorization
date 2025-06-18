using Pulse.Authorization.Core.Models.Subscriptions;
using Pulse.Authorization.Core.Exceptions;

namespace Pulse.Authorization.Core.Tests.Models.Subscriptions
{
    public class ContactsSubscriptionTests
    {
        [Fact]
        public void Constructor_CreatesInstanceWithEmptyCollections()
        {
            // Arrange & Act
            var subscription = new ContactsSubscription();

            // Assert
            Assert.NotNull(subscription);
            Assert.NotNull(subscription.ClientContacts);
            Assert.Empty(subscription.ClientContacts);
            Assert.NotNull(subscription.CollabContacts);
            Assert.Empty(subscription.CollabContacts);
            Assert.NotNull(subscription.UnexistedContacts);
            Assert.Empty(subscription.UnexistedContacts);
        }

        [Fact]
        public void ClientContacts_Property_CanBeSetAndRetrieved()
        {
            // Arrange
            var subscription = new ContactsSubscription();
            var expectedContacts = new List<int> { 1, 2, 3 };

            // Act
            subscription.ClientContacts = expectedContacts;

            // Assert
            Assert.Equal(expectedContacts, subscription.ClientContacts);
        }

        [Fact]
        public void CollabContacts_Property_CanBeSetAndRetrieved()
        {
            // Arrange
            var subscription = new ContactsSubscription();
            var expectedContacts = new List<int> { 4, 5, 6 };

            // Act
            subscription.CollabContacts = expectedContacts;

            // Assert
            Assert.Equal(expectedContacts, subscription.CollabContacts);
        }

        [Fact]
        public void UnexistedContacts_Property_CanBeSetAndRetrieved()
        {
            // Arrange
            var subscription = new ContactsSubscription();
            var expectedContacts = new List<int> { 7, 8, 9 };

            // Act
            subscription.UnexistedContacts = expectedContacts;

            // Assert
            Assert.Equal(expectedContacts, subscription.UnexistedContacts);
        }

        [Fact]
        public void AllProperties_InitializeAsEmptyCollections()
        {
            // Arrange & Act
            var subscription = new ContactsSubscription();

            // Assert
            Assert.NotNull(subscription.ClientContacts);
            Assert.Empty(subscription.ClientContacts);
            Assert.NotNull(subscription.CollabContacts);
            Assert.Empty(subscription.CollabContacts);
            Assert.NotNull(subscription.UnexistedContacts);
            Assert.Empty(subscription.UnexistedContacts);
        }

        [Fact]
        public void BuildErrors_WithEmptyUnexistedContacts_ReturnsEmptyEnumerable()
        {
            // Arrange
            var subscription = new ContactsSubscription();

            // Act
            var errors = subscription.BuildErrors();

            // Assert
            Assert.NotNull(errors);
            Assert.Empty(errors);
        }

        [Fact]
        public void BuildErrors_WithSingleUnexistedContact_ReturnsSingleError()
        {
            // Arrange
            const int contactId = 123;
            var subscription = new ContactsSubscription
            {
                UnexistedContacts = new List<int> { contactId }
            };

            // Act
            var errors = subscription.BuildErrors().ToList();

            // Assert
            Assert.Single(errors);
            Assert.Contains(contactId.ToString(), errors[0]);
        }

        [Fact]
        public void BuildErrors_WithMultipleUnexistedContacts_ReturnsMultipleErrors()
        {
            // Arrange
            var contactIds = new List<int> { 123, 456, 789 };
            var subscription = new ContactsSubscription
            {
                UnexistedContacts = contactIds
            };

            // Act
            var errors = subscription.BuildErrors().ToList();

            // Assert
            Assert.Equal(contactIds.Count, errors.Count);
            foreach (var contactId in contactIds)
            {
                Assert.Contains(errors, error => error.Contains(contactId.ToString()));
            }
        }

        [Fact]
        public void BuildErrors_UsesCorrectErrorMessageFormat()
        {
            // Arrange
            const int contactId = 123;
            var subscription = new ContactsSubscription
            {
                UnexistedContacts = new List<int> { contactId }
            };
            var expectedError = string.Format(Errors.NotFoundContactMessage, contactId);

            // Act
            var errors = subscription.BuildErrors().ToList();

            // Assert
            Assert.Single(errors);
            Assert.Equal(expectedError, errors[0]);
        }

        [Fact]
        public void BuildErrors_WithDuplicateUnexistedContacts_ReturnsErrorForEachOccurrence()
        {
            // Arrange
            const int duplicateContactId = 123;
            var subscription = new ContactsSubscription
            {
                UnexistedContacts = new List<int> { duplicateContactId, duplicateContactId, duplicateContactId }
            };

            // Act
            var errors = subscription.BuildErrors().ToList();

            // Assert
            Assert.Equal(3, errors.Count);
            Assert.All(errors, error => Assert.Contains(duplicateContactId.ToString(), error));
        }

        [Fact]
        public void BuildErrors_IsLazyEvaluated()
        {
            // Arrange
            var subscription = new ContactsSubscription
            {
                UnexistedContacts = new List<int> { 123, 456, 789 }
            };

            // Act
            var errors = subscription.BuildErrors(); // This should not execute the loop yet

            // Change the UnexistedContacts after getting the enumerable
            subscription.UnexistedContacts = new List<int> { 999 };

            var errorsList = errors.ToList(); // This should execute with the new values

            // Assert
            Assert.Single(errorsList);
            Assert.Contains("999", errorsList[0]);
        }

        [Fact]
        public void BuildErrors_DoesNotDependOnClientContactsOrCollabContacts()
        {
            // Arrange
            var subscription = new ContactsSubscription
            {
                ClientContacts = new List<int> { 1, 2, 3 },
                CollabContacts = new List<int> { 4, 5, 6 },
                UnexistedContacts = new List<int> { 123 }
            };

            // Act
            var errors = subscription.BuildErrors().ToList();

            // Assert
            Assert.Single(errors);
            Assert.Contains("123", errors[0]);
        }

        [Fact]
        public void Properties_CanBeSetToEmptyCollections()
        {
            // Arrange
            var subscription = new ContactsSubscription();

            // Act
            subscription.ClientContacts = new List<int>();
            subscription.CollabContacts = new List<int>();
            subscription.UnexistedContacts = new List<int>();

            // Assert
            Assert.Empty(subscription.ClientContacts);
            Assert.Empty(subscription.CollabContacts);
            Assert.Empty(subscription.UnexistedContacts);
        }

        [Fact]
        public void Properties_CanBeSetToNull()
        {
            // Arrange
            var subscription = new ContactsSubscription();

            // Act
            subscription.ClientContacts = null;
            subscription.CollabContacts = null;
            subscription.UnexistedContacts = null;

            // Assert
            Assert.Null(subscription.ClientContacts);
            Assert.Null(subscription.CollabContacts);
            Assert.Null(subscription.UnexistedContacts);
        }

        [Fact]
        public void Properties_PreserveOrderOfElements()
        {
            // Arrange
            var subscription = new ContactsSubscription();
            var orderedClientContacts = new List<int> { 5, 1, 9, 3, 7 };
            var orderedCollabContacts = new List<int> { 8, 2, 6, 4 };
            var orderedUnexistedContacts = new List<int> { 10, 11, 12 };

            // Act
            subscription.ClientContacts = orderedClientContacts;
            subscription.CollabContacts = orderedCollabContacts;
            subscription.UnexistedContacts = orderedUnexistedContacts;

            // Assert
            Assert.True(subscription.ClientContacts.SequenceEqual(orderedClientContacts));
            Assert.True(subscription.CollabContacts.SequenceEqual(orderedCollabContacts));
            Assert.True(subscription.UnexistedContacts.SequenceEqual(orderedUnexistedContacts));
        }

        [Fact]
        public void Properties_CanHandleDuplicateValues()
        {
            // Arrange
            var subscription = new ContactsSubscription();
            var clientContactsWithDuplicates = new List<int> { 1, 2, 2, 3, 1 };
            var collabContactsWithDuplicates = new List<int> { 4, 5, 5, 6, 4 };
            var unexistedContactsWithDuplicates = new List<int> { 7, 8, 8, 9, 7 };

            // Act
            subscription.ClientContacts = clientContactsWithDuplicates;
            subscription.CollabContacts = collabContactsWithDuplicates;
            subscription.UnexistedContacts = unexistedContactsWithDuplicates;

            // Assert
            Assert.Equal(clientContactsWithDuplicates, subscription.ClientContacts);
            Assert.Equal(5, subscription.ClientContacts.Count());
            Assert.Equal(collabContactsWithDuplicates, subscription.CollabContacts);
            Assert.Equal(5, subscription.CollabContacts.Count());
            Assert.Equal(unexistedContactsWithDuplicates, subscription.UnexistedContacts);
            Assert.Equal(5, subscription.UnexistedContacts.Count());
        }

        [Fact]
        public void AllProperties_CanBeSetSimultaneously()
        {
            // Arrange
            var subscription = new ContactsSubscription();
            var clientContacts = new List<int> { 1, 2, 3 };
            var collabContacts = new List<int> { 4, 5, 6 };
            var unexistedContacts = new List<int> { 7, 8, 9 };

            // Act
            subscription.ClientContacts = clientContacts;
            subscription.CollabContacts = collabContacts;
            subscription.UnexistedContacts = unexistedContacts;

            // Assert
            Assert.Equal(clientContacts, subscription.ClientContacts);
            Assert.Equal(collabContacts, subscription.CollabContacts);
            Assert.Equal(unexistedContacts, subscription.UnexistedContacts);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void BuildErrors_WithDifferentContactIds_IncludesCorrectContactIdInErrorMessage(int contactId)
        {
            // Arrange
            var subscription = new ContactsSubscription
            {
                UnexistedContacts = new List<int> { contactId }
            };

            // Act
            var errors = subscription.BuildErrors().ToList();

            // Assert
            Assert.Single(errors);
            Assert.Contains(contactId.ToString(), errors[0]);
        }

        [Fact]
        public void BuildErrors_WithMixedPositiveAndNegativeContactIds_HandlesAllCorrectly()
        {
            // Arrange
            var mixedContactIds = new List<int> { -1, 0, 1, int.MaxValue, int.MinValue };
            var subscription = new ContactsSubscription
            {
                UnexistedContacts = mixedContactIds
            };

            // Act
            var errors = subscription.BuildErrors().ToList();

            // Assert
            Assert.Equal(mixedContactIds.Count, errors.Count);
            foreach (var contactId in mixedContactIds)
            {
                Assert.Contains(errors, error => error.Contains(contactId.ToString()));
            }
        }

        [Fact]
        public void BuildErrors_ReturnsIEnumerableOfStrings()
        {
            // Arrange
            var subscription = new ContactsSubscription
            {
                UnexistedContacts = new List<int> { 123 }
            };

            // Act
            var errors = subscription.BuildErrors();

            // Assert
            Assert.IsAssignableFrom<IEnumerable<string>>(errors);
            Assert.All(errors, error => Assert.IsType<string>(error));
        }
    }
}
