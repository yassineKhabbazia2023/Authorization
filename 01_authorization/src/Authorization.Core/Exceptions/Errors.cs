// <copyright file="Errors.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Authorization.Core.Exceptions
{
    public static class Errors
    {
        public static readonly string NotFoundAccountCode = "AUT001";
        public static readonly string NotFoundAccountMessage = "L'identifiant de l'entité saisi est erroné";

        public static readonly string NotFoundContactCode = "AUT002";
        public static readonly string NotFoundContactMessage = "Le contact avec l'identifiant {0} est introuvable";

        public static readonly string NotFoundContactTypeCode = "AUT003";
        public static readonly string NotFoundContactTypeMessage = "Le contact avec l'identifiant {0} et de type {1} n'est pas reconnu";

        public static readonly string NotFoundContactAccountAuthCode = "AUT004";
        public static readonly string NotFoundContactAccountAuthMessage = "Le contact avec l'identifiant {0} ne possède pas ces authorizations: {1} sur l'entité {2}";

        public static readonly string NotFoundServiceBusNamespaceCode = "AUT005";
        public static readonly string NotFoundServiceBusNamespaceMessage = "Le namespace du service bus doit être renseignée ";

        public static readonly string NotFoundManagedIdentityClientIdCode = "AUT006";
        public static readonly string NotFoundManagedIdentityClientIdMessage = "Le client ID de l'entité managée doit être renseigné";

        public static readonly string NotFoundRoleCode = "AUT007";
        public static readonly string NotFoundRoleMessage = "Le role avec l'identifiant {0} est introuvable";

        public static readonly string NotFoundPermissionCode = "AUT008";
        public static readonly string NotFoundPermissionMessage = "La permission avec l'identifiant {0} est introuvable.";

        public static readonly string NotConfigurablePermissionCode = "AUT009";
        public static readonly string NotConfigurablePermissionMessage = "La permission avec l'identifiant {0} n'est pas configurable.";
    }
}
