IF (SELECT count(*) FROM [auth].[Authorization] WHERE Code = 'CLGEDPEN01') = 0
BEGIN
INSERT INTO auth.[Authorization] (
	Name, Description, Code, Label, Category, Configurable, [View], [ProductCode], [Type])
	VALUES
	('Access Rydge Conseil', '', 'CLGEDPEN01', 'Accéder à l’espace documentaire Rydge Conseil', 'CLTGEDESC', '1', 'Partial', 'pennylaneaccess', 'customer')
END