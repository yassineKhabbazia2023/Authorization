CREATE TABLE [auth].[Persona] (
    [PersonaId] INT           NOT NULL,
    [Name]       VARCHAR (100) NOT NULL,
    [Type]       VARCHAR (20)  NOT NULL,
    [Description] VARCHAR(500) NULL, 
    CONSTRAINT [PK_Persona] PRIMARY KEY CLUSTERED ([PersonaId] ASC)
);



