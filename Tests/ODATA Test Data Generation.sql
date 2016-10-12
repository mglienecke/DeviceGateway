DELETE FROM dbo.DbSensorData
DELETE FROM dbo.TrackingPoint

SET NOCOUNT ON

DECLARE @counter INT
DECLARE @SensorId INT
DECLARE @iteration INT

-- Transactions / sec
SET @SensorId = 780

SET @counter = 0
SET @iteration = 0

-- make a loop with 1,000,000 entries
WHILE @counter < 1000000
BEGIN
	INSERT INTO dbo.DbSensorData ( SensorId, TakenWhen, Value, CorrelationId )
	VALUES  ( @SensorId, SYSDATETIME(), CONVERT(VARCHAR(10), @counter), CONVERT(VARCHAR(10), @counter))
	SET @counter = @counter + 1

	IF (@counter % 10000) = 0
	BEGIN
		SET @iteration = @iteration + 1
		PRINT CONVERT(VARCHAR(10), @iteration) + ' iterations done'
	END
END

SELECT * FROM dbo.DbSensorData