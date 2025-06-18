declare @authorizationId INT = (select authorizationId from auth.[Authorization] where Code = 'COPEN001')
IF(@authorizationId != null)
BEGIN 
	Update auth.[Authorization] set ProductCode = 'pennylaneaccess' where AuthorizationId = @authorizationId
END

