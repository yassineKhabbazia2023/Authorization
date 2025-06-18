// <copyright file="ContactsSubscription.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Authorization.Core.Exceptions;

namespace Pulse.Authorization.Core.Models.Subscriptions
{
    public class ContactsSubscription
    {
        public IEnumerable<int> ClientContacts { get; set; } = [];

        public IEnumerable<int> CollabContacts { get; set; } = [];

        public IEnumerable<int> UnexistedContacts { get; set; } = [];

        public IEnumerable<string> BuildErrors()
        {
            foreach (var unexistedContact in UnexistedContacts)
            {
                yield return string.Format(Errors.NotFoundContactMessage, unexistedContact);
            }
        }
    }
}
