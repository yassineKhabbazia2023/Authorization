// <copyright file="ContactRoleSubscriptionTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Authorization.Core.Models.Subscriptions;
using Pulse.Authorization.Core.Exceptions;

namespace Pulse.Authorization.Core.Tests.Models.Subscriptions
{
    public class ContactRolesSubscriptionTests
    {
        [Fact]
        public void Constructor_WithValidAccountId_CreatesInstanceSuccessfully()
        {
            // Arrange
            const int accountId = 123;

            // Act
            var subscription = new ContactRolesSubscription(accountId);

            // Assert
            Assert.NotNull(subscription);
            Assert.NotNull(subscription.ExistedContactRoles);
            Assert.Empty(subscription.ExistedContactRoles);
            Assert.NotNull(subscription.UnexistedContactRoles);
            Assert.Empty(subscription.UnexistedContactRoles);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void Constructor_WithVariousAccountIds_CreatesInstanceSuccessfully(int accountId)
        {
            // Arrange & Act
            var subscription = new ContactRolesSubscription(accountId);

            // Assert
            Assert.NotNull(subscription);
        }

        [Fact]
        public void ExistedContactRoles_Property_CanBeSetAndRetrieved()
        {
            // Arrange
            var subscription = new ContactRolesSubscription(123);
            var expectedRoles = new List<int> { 1, 2, 3 };

            // Act
            subscription.ExistedContactRoles = expectedRoles;

            // Assert
            Assert.Equal(expectedRoles, subscription.ExistedContactRoles);
        }

        [Fact]
        public void UnexistedContactRoles_Property_CanBeSetAndRetrieved()
        {
            // Arrange
            var subscription = new ContactRolesSubscription(123);
            var expectedRoles = new List<int> { 4, 5, 6 };

            // Act
            subscription.UnexistedContactRoles = expectedRoles;

            // Assert
            Assert.Equal(expectedRoles, subscription.UnexistedContactRoles);
        }

        [Fact]
        public void ExistedContactRoles_Property_InitializesAsEmptyList()
        {
            // Arrange & Act
            var subscription = new ContactRolesSubscription(123);

            // Assert
            Assert.NotNull(subscription.ExistedContactRoles);
            Assert.Empty(subscription.ExistedContactRoles);
        }

        [Fact]
        public void UnexistedContactRoles_Property_InitializesAsEmptyList()
        {
            // Arrange & Act
            var subscription = new ContactRolesSubscription(123);

            // Assert
            Assert.NotNull(subscription.UnexistedContactRoles);
            Assert.Empty(subscription.UnexistedContactRoles);
        }

        [Fact]
        public void BuildErrors_WithEmptyUnexistedContactRoles_ReturnsEmptyEnumerable()
        {
            // Arrange
            var subscription = new ContactRolesSubscription(123);

            // Act
            var errors = subscription.BuildErrors();

            // Assert
            Assert.NotNull(errors);
            Assert.Empty(errors);
        }

        [Fact]
        public void BuildErrors_WithSingleUnexistedContactRole_ReturnsSingleError()
        {
            // Arrange
            const int accountId = 123;
            const int roleId = 456;
            var subscription = new ContactRolesSubscription(accountId)
            {
                UnexistedContactRoles = new List<int> { roleId }
            };

            // Act
            var errors = subscription.BuildErrors().ToList();

            // Assert
            Assert.Single(errors);
            Assert.Contains(accountId.ToString(), errors[0]);
            Assert.Contains(roleId.ToString(), errors[0]);
        }

        [Fact]
        public void BuildErrors_WithMultipleUnexistedContactRoles_ReturnsMultipleErrors()
        {
            // Arrange
            const int accountId = 123;
            var roleIds = new List<int> { 456, 789, 101 };
            var subscription = new ContactRolesSubscription(accountId)
            {
                UnexistedContactRoles = roleIds
            };

            // Act
            var errors = subscription.BuildErrors().ToList();

            // Assert
            Assert.Equal(roleIds.Count, errors.Count);
            foreach (var roleId in roleIds)
            {
                Assert.Contains(errors, error => error.Contains(accountId.ToString()) && error.Contains(roleId.ToString()));
            }
        }

        [Fact]
        public void BuildErrors_UsesCorrectErrorMessageFormat()
        {
            // Arrange
            const int accountId = 123;
            const int roleId = 456;
            var subscription = new ContactRolesSubscription(accountId)
            {
                UnexistedContactRoles = new List<int> { roleId }
            };
            var expectedError = string.Format(Errors.NotFoundRoleMessage, accountId, roleId);

            // Act
            var errors = subscription.BuildErrors().ToList();

            // Assert
            Assert.Single(errors);
            Assert.Equal(expectedError, errors[0]);
        }

        [Fact]
        public void BuildErrors_WithDuplicateUnexistedContactRoles_ReturnsErrorForEachOccurrence()
        {
            // Arrange
            const int accountId = 123;
            const int duplicateRoleId = 456;
            var subscription = new ContactRolesSubscription(accountId)
            {
                UnexistedContactRoles = new List<int> { duplicateRoleId, duplicateRoleId, duplicateRoleId }
            };

            // Act
            var errors = subscription.BuildErrors().ToList();

            // Assert
            Assert.Equal(3, errors.Count);
            Assert.All(errors, error =>
            {
                Assert.Contains(accountId.ToString(), error);
                Assert.Contains(duplicateRoleId.ToString(), error);
            });
        }

        [Fact]
        public void BuildErrors_IsLazyEvaluated()
        {
            // Arrange
            const int accountId = 123;
            var subscription = new ContactRolesSubscription(accountId)
            {
                UnexistedContactRoles = new List<int> { 456, 789, 101 }
            };

            // Act
            var errors = subscription.BuildErrors(); // This should not execute the loop yet

            // Change the UnexistedContactRoles after getting the enumerable
            subscription.UnexistedContactRoles = new List<int> { 999 };

            var errorsList = errors.ToList(); // This should execute with the new values

            // Assert
            Assert.Single(errorsList);
            Assert.Contains("999", errorsList[0]);
        }

        [Fact]
        public void BuildErrors_DoesNotDependOnExistedContactRoles()
        {
            // Arrange
            const int accountId = 123;
            var subscription = new ContactRolesSubscription(accountId)
            {
                ExistedContactRoles = new List<int> { 1, 2, 3 },
                UnexistedContactRoles = new List<int> { 456 }
            };

            // Act
            var errors = subscription.BuildErrors().ToList();

            // Assert
            Assert.Single(errors);
            Assert.Contains("456", errors[0]);
        }

        [Fact]
        public void Properties_CanBeSetToEmptyCollections()
        {
            // Arrange
            var subscription = new ContactRolesSubscription(123);

            // Act
            subscription.ExistedContactRoles = new List<int>();
            subscription.UnexistedContactRoles = new List<int>();

            // Assert
            Assert.Empty(subscription.ExistedContactRoles);
            Assert.Empty(subscription.UnexistedContactRoles);
        }

        [Fact]
        public void Properties_CanBeSetToNull()
        {
            // Arrange
            var subscription = new ContactRolesSubscription(123);

            // Act
            subscription.ExistedContactRoles = null;
            subscription.UnexistedContactRoles = null;

            // Assert
            Assert.Null(subscription.ExistedContactRoles);
            Assert.Null(subscription.UnexistedContactRoles);
        }

        [Fact]
        public void Properties_PreserveOrderOfElements()
        {
            // Arrange
            var subscription = new ContactRolesSubscription(123);
            var orderedExistedRoles = new List<int> { 5, 1, 9, 3, 7 };
            var orderedUnexistedRoles = new List<int> { 8, 2, 6, 4 };

            // Act
            subscription.ExistedContactRoles = orderedExistedRoles;
            subscription.UnexistedContactRoles = orderedUnexistedRoles;

            // Assert
            Assert.True(subscription.ExistedContactRoles.SequenceEqual(orderedExistedRoles));
            Assert.True(subscription.UnexistedContactRoles.SequenceEqual(orderedUnexistedRoles));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void BuildErrors_WithDifferentAccountIds_IncludesCorrectAccountIdInErrorMessage(int accountId)
        {
            // Arrange
            const int roleId = 456;
            var subscription = new ContactRolesSubscription(accountId)
            {
                UnexistedContactRoles = new List<int> { roleId }
            };

            // Act
            var errors = subscription.BuildErrors().ToList();

            // Assert
            Assert.Single(errors);
            Assert.Contains(accountId.ToString(), errors[0]);
        }
    }
}
