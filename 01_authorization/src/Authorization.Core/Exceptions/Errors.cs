// <copyright file="Errors.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Authorization.Core.Exceptions
{
    public static class Errors
    {
        public static readonly string NotFoundAccountCode = "AUT001";
        public static readonly string NotFoundAccountMessage = "L'entité avec l'identifiant {0} ne possède aucune ressource";

        public static readonly string NotFoundContactCode = "AUT002";
        public static readonly string NotFoundContactMessage = "Le contact avec l'identifiant {0} est introuvable";

        public static readonly string NotFoundContactTypeCode = "AUT003";
        public static readonly string NotFoundContactTypeMessage = "Le contact avec l'identifiant {0} et de type {1} n'est pas reconnu";
    }
}
