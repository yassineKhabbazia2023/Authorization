// <copyright file="AuthorizationService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Authorization.Core.Interfaces;
using Pulse.Authorization.Core.Models;
using Action = Pulse.Authorization.Core.Models.Action;

namespace Pulse.Authorization.Core.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly IAuthorizationRepository _authorizationRepository;

        public AuthorizationService(IAuthorizationRepository authorizationRepository)
        {
            _authorizationRepository = authorizationRepository;
        }

        public async Task<IReadOnlyCollection<Resource?>> GetAuthorizationsAsync(int accountId)
        {
            var result = new List<Resource>
            {
                new()
                {
                    ResourceId = 1,
                    ResourceName = "Souscriptions",
                    Actions = new List<Action>
                    {
                        new()
                        {
                            ActionId = 1,
                            Code = "OFF001",
                            Name = "Consulter les souscriptions"
                        }
                    },
                    Childrens = new List<Resource>
                    {
                        new()
                        {
                            ResourceId = 20,
                            ResourceName = "Offres",
                            ParentResourceId = 1,
                            Actions = new List<Action>
                            {
                                new()
                                {
                                    ActionId = 2,
                                    Code = "OFF002",
                                    Name = "Consulter les Offres"
                                },
                                new()
                                {
                                    ActionId = 3,
                                    Code = "OFF003",
                                    Name = "Modifier une Offre"
                                },
                                new()
                                {
                                    ActionId = 4,
                                    Code = "OFF004",
                                    Name = "Supprimer une Offre"
                                }
                            },
                            Childrens = new List<Resource>
                            {
                                new()
                                {
                                    ResourceId = 40,
                                    ResourceName = "Services",
                                    ParentResourceId = 20,
                                    Actions = new List<Action>
                                    {
                                        new()
                                        {
                                            ActionId = 5,
                                            Code = "OFF005",
                                            Name = "Consulter les Services"
                                        },
                                        new()
                                        {
                                            ActionId = 6,
                                            Code = "OFF006",
                                            Name = "Modifier un Service"
                                        },
                                        new()
                                        {
                                         ActionId = 7,
                                         Code = "OFF007",
                                         Name = "Supprimer un service",
                                        }
                                    },
                                }
                            }
                        }
                    }
                },
                new()
                {
                    ResourceId = 2,
                    ResourceName = "Validation",
                    Actions = new List<Action>
                    {
                        new()
                        {
                            ActionId = 8,
                            Code = "OFF008",
                            Name = "Consulter les demandes"
                        },
                        new()
                        {
                            ActionId = 9,
                            Code = "OFF009",
                            Name = "Rejeter une demande"
                        },
                        new()
                        {
                         ActionId = 10,
                         Code = "OFF010",
                         Name = "Valider une demande",
                        }
                    },
                    Childrens = new List<Resource>
                    {
                        new()
                        {
                            ResourceId = 21,
                            ResourceName = "Clients",
                            ParentResourceId = 2,
                            Actions = new List<Action>
                            {
                                new()
                                {
                                    ActionId = 8,
                                    Code = "OFF011",
                                    Name = "Consulter les Clients"
                                },
                            },
                        }
                    }
                }

            };
            //return await _authorizationRepository.GetAuthorizationsAsync(accountId);
            return result;
        }
    }
}
