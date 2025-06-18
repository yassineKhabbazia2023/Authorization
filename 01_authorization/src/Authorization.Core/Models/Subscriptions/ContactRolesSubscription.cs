// <copyright file="ContactRolesSubscription.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Authorization.Core.Exceptions;

namespace Pulse.Authorization.Core.Models.Subscriptions
{
    public class ContactRolesSubscription
    {
        private int _accountId;

        public ContactRolesSubscription(int accountId)
        {
            _accountId = accountId;
        }

        public IEnumerable<int> ExistedContactRoles { get; set; } = [];

        public IEnumerable<int> UnexistedContactRoles { get; set; } = [];

        public IEnumerable<string> BuildErrors()
        {
            foreach(var unexistedContactRole in UnexistedContactRoles)
            {
                yield return string.Format(Errors.NotFoundRoleMessage, _accountId, unexistedContactRole);
            }
        }
    }
}
