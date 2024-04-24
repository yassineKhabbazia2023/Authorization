CREATE TABLE [auth].[PersonaAuthorization] (
    [PersonaId] INT NOT NULL,
    [AuthorizationId]   INT NOT NULL,
    CONSTRAINT [PK_PersonaAuthorization] PRIMARY KEY CLUSTERED ([PersonaId] ASC, [AuthorizationId] ASC),
    CONSTRAINT [FK_PersonaAuthorization_Authorization] FOREIGN KEY ([AuthorizationId]) REFERENCES [auth].[Authorization] ([AuthorizationId]),
    CONSTRAINT [FK_PersonaAuthorization_Personna] FOREIGN KEY ([PersonaId]) REFERENCES [auth].[Persona] ([PersonaId])
);

