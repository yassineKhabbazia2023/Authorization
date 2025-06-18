using Xunit;
using Pulse.Authorization.Core.Models.Subscriptions;
using Pulse.Authorization.Core.Exceptions;
using System.Collections.Generic;
using System.Linq;

namespace Pulse.Authorization.Core.Tests.Models.Subscriptions
{
    public class ProductCodesSubscriptionsTests
    {
        [Fact]
        public void Constructor_CreatesInstanceWithEmptyCollections()
        {
            // Arrange & Act
            var subscription = new ProductCodesSubscriptions();

            // Assert
            Assert.NotNull(subscription);
            Assert.NotNull(subscription.CollabAuthorizationIds);
            Assert.Empty(subscription.CollabAuthorizationIds);
            Assert.NotNull(subscription.ClientAuthorizationIds);
            Assert.Empty(subscription.ClientAuthorizationIds);
            Assert.NotNull(subscription.UnexistedCodes);
            Assert.Empty(subscription.UnexistedCodes);
        }

        [Fact]
        public void CollabAuthorizationIds_Field_CanBeSetAndRetrieved()
        {
            // Arrange
            var subscription = new ProductCodesSubscriptions();
            var expectedIds = new List<int> { 1, 2, 3 };

            // Act
            subscription.CollabAuthorizationIds = expectedIds;

            // Assert
            Assert.Equal(expectedIds, subscription.CollabAuthorizationIds);
        }

        [Fact]
        public void ClientAuthorizationIds_Field_CanBeSetAndRetrieved()
        {
            // Arrange
            var subscription = new ProductCodesSubscriptions();
            var expectedIds = new List<int> { 4, 5, 6 };

            // Act
            subscription.ClientAuthorizationIds = expectedIds;

            // Assert
            Assert.Equal(expectedIds, subscription.ClientAuthorizationIds);
        }

        [Fact]
        public void UnexistedCodes_Field_CanBeSetAndRetrieved()
        {
            // Arrange
            var subscription = new ProductCodesSubscriptions();
            var expectedCodes = new List<string> { "CODE1", "CODE2", "CODE3" };

            // Act
            subscription.UnexistedCodes = expectedCodes;

            // Assert
            Assert.Equal(expectedCodes, subscription.UnexistedCodes);
        }

        [Fact]
        public void AllFields_InitializeAsEmptyCollections()
        {
            // Arrange & Act
            var subscription = new ProductCodesSubscriptions();

            // Assert
            Assert.NotNull(subscription.CollabAuthorizationIds);
            Assert.Empty(subscription.CollabAuthorizationIds);
            Assert.NotNull(subscription.ClientAuthorizationIds);
            Assert.Empty(subscription.ClientAuthorizationIds);
            Assert.NotNull(subscription.UnexistedCodes);
            Assert.Empty(subscription.UnexistedCodes);
        }

        [Fact]
        public void BuildErrors_WithEmptyUnexistedCodes_ReturnsEmptyEnumerable()
        {
            // Arrange
            var subscription = new ProductCodesSubscriptions();

            // Act
            var errors = subscription.BuildErrors();

            // Assert
            Assert.NotNull(errors);
            Assert.Empty(errors);
        }

        [Fact]
        public void BuildErrors_WithSingleUnexistedCode_ReturnsSingleError()
        {
            // Arrange
            const string code = "INVALID_CODE";
            var subscription = new ProductCodesSubscriptions
            {
                UnexistedCodes = new List<string> { code }
            };

            // Act
            var errors = subscription.BuildErrors().ToList();

            // Assert
            Assert.Single(errors);
            Assert.Contains(code, errors[0]);
        }

        [Fact]
        public void BuildErrors_WithMultipleUnexistedCodes_ReturnsMultipleErrors()
        {
            // Arrange
            var codes = new List<string> { "CODE1", "CODE2", "CODE3" };
            var subscription = new ProductCodesSubscriptions
            {
                UnexistedCodes = codes
            };

            // Act
            var errors = subscription.BuildErrors().ToList();

            // Assert
            Assert.Equal(codes.Count, errors.Count);
            foreach (var code in codes)
            {
                Assert.Contains(errors, error => error.Contains(code));
            }
        }

        [Fact]
        public void BuildErrors_UsesCorrectErrorMessageFormat()
        {
            // Arrange
            const string code = "INVALID_CODE";
            var subscription = new ProductCodesSubscriptions
            {
                UnexistedCodes = new List<string> { code }
            };
            var expectedError = string.Format(Errors.NotFoundPermissionMessage, code);

            // Act
            var errors = subscription.BuildErrors().ToList();

            // Assert
            Assert.Single(errors);
            Assert.Equal(expectedError, errors[0]);
        }

        [Fact]
        public void BuildErrors_WithDuplicateUnexistedCodes_ReturnsErrorForEachOccurrence()
        {
            // Arrange
            const string duplicateCode = "DUPLICATE_CODE";
            var subscription = new ProductCodesSubscriptions
            {
                UnexistedCodes = new List<string> { duplicateCode, duplicateCode, duplicateCode }
            };

            // Act
            var errors = subscription.BuildErrors().ToList();

            // Assert
            Assert.Equal(3, errors.Count);
            Assert.All(errors, error => Assert.Contains(duplicateCode, error));
        }

        [Fact]
        public void BuildErrors_IsLazyEvaluated()
        {
            // Arrange
            var subscription = new ProductCodesSubscriptions
            {
                UnexistedCodes = new List<string> { "CODE1", "CODE2", "CODE3" }
            };

            // Act
            var errors = subscription.BuildErrors(); // This should not execute the loop yet

            // Change the UnexistedCodes after getting the enumerable
            subscription.UnexistedCodes = new List<string> { "NEW_CODE" };

            var errorsList = errors.ToList(); // This should execute with the new values

            // Assert
            Assert.Single(errorsList);
            Assert.Contains("NEW_CODE", errorsList[0]);
        }

        [Fact]
        public void BuildErrors_DoesNotDependOnAuthorizationIds()
        {
            // Arrange
            var subscription = new ProductCodesSubscriptions
            {
                CollabAuthorizationIds = new List<int> { 1, 2, 3 },
                ClientAuthorizationIds = new List<int> { 4, 5, 6 },
                UnexistedCodes = new List<string> { "TEST_CODE" }
            };

            // Act
            var errors = subscription.BuildErrors().ToList();

            // Assert
            Assert.Single(errors);
            Assert.Contains("TEST_CODE", errors[0]);
        }

        [Fact]
        public void Fields_CanBeSetToEmptyCollections()
        {
            // Arrange
            var subscription = new ProductCodesSubscriptions();

            // Act
            subscription.CollabAuthorizationIds = new List<int>();
            subscription.ClientAuthorizationIds = new List<int>();
            subscription.UnexistedCodes = new List<string>();

            // Assert
            Assert.Empty(subscription.CollabAuthorizationIds);
            Assert.Empty(subscription.ClientAuthorizationIds);
            Assert.Empty(subscription.UnexistedCodes);
        }

        [Fact]
        public void Fields_CanBeSetToNull()
        {
            // Arrange
            var subscription = new ProductCodesSubscriptions();

            // Act
            subscription.CollabAuthorizationIds = null;
            subscription.ClientAuthorizationIds = null;
            subscription.UnexistedCodes = null;

            // Assert
            Assert.Null(subscription.CollabAuthorizationIds);
            Assert.Null(subscription.ClientAuthorizationIds);
            Assert.Null(subscription.UnexistedCodes);
        }

        [Fact]
        public void Fields_PreserveOrderOfElements()
        {
            // Arrange
            var subscription = new ProductCodesSubscriptions();
            var orderedCollabIds = new List<int> { 5, 1, 9, 3, 7 };
            var orderedClientIds = new List<int> { 8, 2, 6, 4 };
            var orderedCodes = new List<string> { "ZCODE", "ACODE", "MCODE" };

            // Act
            subscription.CollabAuthorizationIds = orderedCollabIds;
            subscription.ClientAuthorizationIds = orderedClientIds;
            subscription.UnexistedCodes = orderedCodes;

            // Assert
            Assert.True(subscription.CollabAuthorizationIds.SequenceEqual(orderedCollabIds));
            Assert.True(subscription.ClientAuthorizationIds.SequenceEqual(orderedClientIds));
            Assert.True(subscription.UnexistedCodes.SequenceEqual(orderedCodes));
        }

        [Fact]
        public void Fields_CanHandleDuplicateValues()
        {
            // Arrange
            var subscription = new ProductCodesSubscriptions();
            var collabIdsWithDuplicates = new List<int> { 1, 2, 2, 3, 1 };
            var clientIdsWithDuplicates = new List<int> { 4, 5, 5, 6, 4 };
            var codesWithDuplicates = new List<string> { "CODE1", "CODE2", "CODE2", "CODE3", "CODE1" };

            // Act
            subscription.CollabAuthorizationIds = collabIdsWithDuplicates;
            subscription.ClientAuthorizationIds = clientIdsWithDuplicates;
            subscription.UnexistedCodes = codesWithDuplicates;

            // Assert
            Assert.Equal(collabIdsWithDuplicates, subscription.CollabAuthorizationIds);
            Assert.Equal(5, subscription.CollabAuthorizationIds.Count());
            Assert.Equal(clientIdsWithDuplicates, subscription.ClientAuthorizationIds);
            Assert.Equal(5, subscription.ClientAuthorizationIds.Count());
            Assert.Equal(codesWithDuplicates, subscription.UnexistedCodes);
            Assert.Equal(5, subscription.UnexistedCodes.Count());
        }

        [Fact]
        public void AllFields_CanBeSetSimultaneously()
        {
            // Arrange
            var subscription = new ProductCodesSubscriptions();
            var collabIds = new List<int> { 1, 2, 3 };
            var clientIds = new List<int> { 4, 5, 6 };
            var codes = new List<string> { "CODE1", "CODE2", "CODE3" };

            // Act
            subscription.CollabAuthorizationIds = collabIds;
            subscription.ClientAuthorizationIds = clientIds;
            subscription.UnexistedCodes = codes;

            // Assert
            Assert.Equal(collabIds, subscription.CollabAuthorizationIds);
            Assert.Equal(clientIds, subscription.ClientAuthorizationIds);
            Assert.Equal(codes, subscription.UnexistedCodes);
        }

        [Theory]
        [InlineData("")]
        [InlineData("SIMPLE_CODE")]
        [InlineData("CODE_WITH_NUMBERS_123")]
        [InlineData("code_with_lowercase")]
        [InlineData("CODE WITH SPACES")]
        [InlineData("CODE!@#$%^&*()")]
        public void BuildErrors_WithDifferentCodeFormats_IncludesCorrectCodeInErrorMessage(string code)
        {
            // Arrange
            var subscription = new ProductCodesSubscriptions
            {
                UnexistedCodes = new List<string> { code }
            };

            // Act
            var errors = subscription.BuildErrors().ToList();

            // Assert
            Assert.Single(errors);
            Assert.Contains(code, errors[0]);
        }

        [Fact]
        public void BuildErrors_WithEmptyStringCode_HandlesCorrectly()
        {
            // Arrange
            var subscription = new ProductCodesSubscriptions
            {
                UnexistedCodes = new List<string> { "" }
            };

            // Act
            var errors = subscription.BuildErrors().ToList();

            // Assert
            Assert.Single(errors);
            Assert.Contains("", errors[0]);
        }

        [Fact]
        public void BuildErrors_WithWhitespaceCode_HandlesCorrectly()
        {
            // Arrange
            const string whitespaceCode = "   ";
            var subscription = new ProductCodesSubscriptions
            {
                UnexistedCodes = new List<string> { whitespaceCode }
            };

            // Act
            var errors = subscription.BuildErrors().ToList();

            // Assert
            Assert.Single(errors);
            Assert.Contains(whitespaceCode, errors[0]);
        }

        [Fact]
        public void BuildErrors_WithMixedCaseAndSpecialCharacterCodes_HandlesAllCorrectly()
        {
            // Arrange
            var mixedCodes = new List<string>
            {
                "UPPERCASE",
                "lowercase",
                "MiXeD_CaSe",
                "123NUMERIC",
                "SPECIAL!@#",
                "",
                "   "
            };
            var subscription = new ProductCodesSubscriptions
            {
                UnexistedCodes = mixedCodes
            };

            // Act
            var errors = subscription.BuildErrors().ToList();

            // Assert
            Assert.Equal(mixedCodes.Count, errors.Count);
            foreach (var code in mixedCodes)
            {
                Assert.Contains(errors, error => error.Contains(code));
            }
        }

        [Fact]
        public void BuildErrors_ReturnsIEnumerableOfStrings()
        {
            // Arrange
            var subscription = new ProductCodesSubscriptions
            {
                UnexistedCodes = new List<string> { "TEST_CODE" }
            };

            // Act
            var errors = subscription.BuildErrors();

            // Assert
            Assert.IsAssignableFrom<IEnumerable<string>>(errors);
            Assert.All(errors, error => Assert.IsType<string>(error));
        }

        [Fact]
        public void AuthorizationIds_CanHandleIntegerEdgeCases()
        {
            // Arrange
            var subscription = new ProductCodesSubscriptions();
            var edgeCaseIds = new List<int> { 0, -1, int.MaxValue, int.MinValue };

            // Act
            subscription.CollabAuthorizationIds = edgeCaseIds;
            subscription.ClientAuthorizationIds = edgeCaseIds;

            // Assert
            Assert.Equal(edgeCaseIds, subscription.CollabAuthorizationIds);
            Assert.Equal(edgeCaseIds, subscription.ClientAuthorizationIds);
        }
    }
}
