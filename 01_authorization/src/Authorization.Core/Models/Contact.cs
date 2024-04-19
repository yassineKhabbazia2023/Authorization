// <copyright file="Contact.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Authorization.Core.Models;

public class Contact
{
    public int ContactId { get; set; }

    public Guid? ContactGlobalUniqueId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Email { get; set; }

    public string? Type { get; set; }

    public string? Status { get; set; }

    public string? PersonaName { get; set; }

    public DateTime? CreationDate { get; set; }
}
