// <copyright file="ProductCodesSubscriptions.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Authorization.Core.Exceptions;

namespace Pulse.Authorization.Core.Models.Subscriptions
{
    public class ProductCodesSubscriptions
    {
        public IEnumerable<int> CollabAuthorizationIds = [];

        public IEnumerable<int> ClientAuthorizationIds = [];

        public IEnumerable<string> UnexistedCodes = [];

        public IEnumerable<string> BuildErrors()
        {
            foreach(string unexistedCode in UnexistedCodes)
            {
                yield return string.Format(Errors.NotFoundPermissionMessage, unexistedCode);
            }
        }
    }
}
