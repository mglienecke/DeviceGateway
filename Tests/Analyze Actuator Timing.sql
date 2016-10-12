-- DELETE FROM dbo.TrackingPoint

-- Timinig analysis for Actuator writing

-- Overall roundtrip timinig. Numerical correlation ids are for server receive -> actuator send -> actuator receive
-- NULL correlation ID for server batch processing
-- others are for client batch processing (e.g. 1 - 20)
--
-- THIS IS NEEDED TO FIGURE OUT THE OVERALL TIMING FOR A ROUNDTRIP
WITH    Timing ( CorrelationId, StartTime, EndTime )
          AS ( SELECT   CorrelationId, MIN(Timestamp) OVER ( PARTITION BY CorrelationId ) AS Mi, MAX(Timestamp) OVER ( PARTITION BY CorrelationId ) AS Ma
               FROM     dbo.TrackingPoint
             )
    SELECT  CorrelationId, DATEDIFF(MICROSECOND, MIN(StartTime), MAX(EndTime)) AS 'Duration Roundtrip µsec'
    FROM    Timing
    WHERE   CorrelationId IS NOT NULL AND ISNUMERIC(CorrelationId) = 0
    GROUP BY CorrelationId;


-- Get only the internal measures within the server between receiving and sending the value to the actuator
--
-- THIS IS NEEDED TO FIGURE OUT THE TIMING FOR AN ACTUATOR
WITH    ActuatorMeasures ( CorrelationId, RowNum, TrackingPoint, Timestamp )
          AS ( SELECT   CorrelationId, ROW_NUMBER() OVER ( PARTITION BY CorrelationId ORDER BY TrackingPoint DESC) AS "RowNum", TrackingPoint, Timestamp
               FROM     dbo.TrackingPoint
               WHERE    ISNUMERIC(CorrelationId) = 1
             )
    SELECT  CorrelationId, MAX(CASE WHEN RowNum = 1 THEN Timestamp
                               END) AS 'SensorReceive', MAX(CASE WHEN RowNum = 2 THEN Timestamp
                                                            END) AS 'ActuatorSend', MAX(CASE WHEN RowNum = 3 THEN Timestamp
                                                                                        END) AS 'DeviceActuatorReceive',
            DATEDIFF(MICROSECOND, MAX(CASE WHEN RowNum = 1 THEN Timestamp
                                      END), MAX(CASE WHEN RowNum = 2 THEN Timestamp
                                                END)) AS 'TimeToSend µsec', 
												            DATEDIFF(MICROSECOND, MAX(CASE WHEN RowNum = 2 THEN Timestamp
                                      END), MAX(CASE WHEN RowNum = 3 THEN Timestamp
                                                END)) AS 'TimeToReceive µsec'
    FROM    ActuatorMeasures
    GROUP BY CorrelationId
    ORDER BY CorrelationId ASC;
    

	
-- List all data records in groups for the correlation ids
	WITH    ActuatorMeasures ( CorrelationId, RowNum, TrackingPoint, AdditionalData, Timestamp )
          AS ( SELECT   CorrelationId, ROW_NUMBER() OVER ( PARTITION BY CorrelationId ORDER BY TrackingPoint DESC) AS "RowNum", TrackingPoint, AdditionalData, Timestamp
               FROM     dbo.TrackingPoint
               WHERE    ISNUMERIC(CorrelationId) = 1
             )
    SELECT  CorrelationId, RowNum, TrackingPoint, AdditionalData, Timestamp
    FROM    ActuatorMeasures
    ORDER BY CorrelationId ASC;
    

	-- Get only the internal measures within the server between receiving and sending the value to the actuator
	-- RowNum = 1 -> Server -> Sensor Receive
	-- RowNum = 2 -> Server -> Actuator Send
	-- RowNum = 3 -> Device -> Actuator Receive 
WITH    ActuatorMeasures ( CorrelationId, RowNum, TrackingPoint, Timestamp )
          AS ( SELECT   CorrelationId, ROW_NUMBER() OVER ( PARTITION BY CorrelationId ORDER BY TrackingPoint DESC) AS "RowNum", TrackingPoint, Timestamp
               FROM     dbo.TrackingPoint
               WHERE    ISNUMERIC(CorrelationId) = 1
             )
    SELECT  CorrelationId, MAX(CASE WHEN RowNum = 1 THEN Timestamp
                               END) AS 'SensorReceive', MAX(CASE WHEN RowNum = 2 THEN Timestamp
                                                            END) AS 'ActuatorSend', MAX(CASE WHEN RowNum = 3 THEN Timestamp
                                                                                        END) AS 'DeviceActuatorReceive'
            
    FROM    ActuatorMeasures
    GROUP BY CorrelationId
    ORDER BY CorrelationId ASC;
    

-- test the various data tuples
--SELECT  *
--FROM    dbo.TrackingPoint
--WHERE   CorrelationId = '1 - 20'
--SELECT  *
--FROM    dbo.TrackingPoint
--WHERE   CorrelationId = '900'
--SELECT  *
--FROM    dbo.TrackingPoint
--WHERE   CorrelationId IS NULL 
SELECT * FROM dbo.TrackingPoint WHERE CorrelationId = '1540 - 1540'