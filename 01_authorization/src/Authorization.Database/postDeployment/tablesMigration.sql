IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'CosmosDB')
BEGIN
	CREATE TABLE [dbo].[CosmosDB](
		[id] [nvarchar](200) NULL,
		[AccountName] [nvarchar](200) NULL,
		[AccountId] [nvarchar](200) NULL,
		[ContactLoginName] [nvarchar](200) NULL,
		[AssetId] [nvarchar](200) NULL,
		[Code] [nvarchar](100) NULL
	) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'Transco')
BEGIN
CREATE TABLE [dbo].[Transco](
	[AccessRightCode] [varchar](10) NULL,
	[AuthorizationID] [varchar](10) NULL
) ON [PRIMARY]
 
INSERT INTO Transco (AccessRightCode, AuthorizationId) VALUES ('RIGH02' , 'CLUSER001' )
INSERT INTO Transco (AccessRightCode, AuthorizationId) VALUES ('RIGH02' , 'CLUSER002' )
INSERT INTO Transco (AccessRightCode, AuthorizationId) VALUES ('RIGH02' , 'CLUSER003' )
INSERT INTO Transco (AccessRightCode, AuthorizationId) VALUES ('RIGH02' , 'CLUSER004' )
INSERT INTO Transco (AccessRightCode, AuthorizationId) VALUES ('RIGH02' , 'CLOFF001' )
INSERT INTO Transco (AccessRightCode, AuthorizationId) VALUES ('RIGH02' , 'CLINFO001' )
INSERT INTO Transco (AccessRightCode, AuthorizationId) VALUES ('RIGH02' , 'CLINVO001' )
INSERT INTO Transco (AccessRightCode, AuthorizationId) VALUES ('RIGH11' , 'CLMEG001' )
INSERT INTO Transco (AccessRightCode, AuthorizationId) VALUES ('RIGH33' , 'CLPEN001' )
INSERT INTO Transco (AccessRightCode, AuthorizationId) VALUES ('RIGH31' , 'CLRAPP001' )
INSERT INTO Transco (AccessRightCode, AuthorizationId) VALUES ('RIGH31' , 'CLRAPP002' )
INSERT INTO Transco (AccessRightCode, AuthorizationId) VALUES ('RIGH31' , 'CLKPI0001' )
INSERT INTO Transco (AccessRightCode, AuthorizationId) VALUES ('RIGH07' , 'CLBank001' )
INSERT INTO Transco (AccessRightCode, AuthorizationId) VALUES ('RIGH14' , 'CLSILA001' )
INSERT INTO Transco (AccessRightCode, AuthorizationId) VALUES ('RIGH32' , 'CLEVP001' )
INSERT INTO Transco (AccessRightCode, AuthorizationId) VALUES ('RIGH32' , 'CLEMB001' )
INSERT INTO Transco (AccessRightCode, AuthorizationId) VALUES ('DS0006' , 'CLGED0001' )
INSERT INTO Transco (AccessRightCode, AuthorizationId) VALUES ('DS0005' , 'CLGED0002' )
INSERT INTO Transco (AccessRightCode, AuthorizationId) VALUES ('DS0006', 'COGED0001')
INSERT INTO Transco (AccessRightCode, AuthorizationId) VALUES ('DS0005', 'COGED0002')
INSERT INTO Transco (AccessRightCode, AuthorizationId) VALUES ('RIGH14', 'COLANC001')
INSERT INTO Transco (AccessRightCode, AuthorizationId) VALUES ('RIGH32', 'COLANC001')
INSERT INTO Transco (AccessRightCode, AuthorizationId) VALUES ('RIGH11', 'COLANC001')
INSERT INTO Transco (AccessRightCode, AuthorizationId) VALUES ('RIGH33', 'COLANC001')
INSERT INTO Transco (AccessRightCode, AuthorizationId) VALUES ('RIGH31', 'COKPI0001')
INSERT INTO Transco (AccessRightCode, AuthorizationId) VALUES ('RIGH07', 'COBANK001')
END