-- DELETE FROM dbo.TrackingPoint 
SELECT  *
FROM    dbo.TrackingPoint
ORDER BY Timestamp ASC;

-- This one will select all "final" values in msec for sending data to the server
WITH    ServerTiming ( CorrelationId, TimeInMsec, RowNum )
          AS ( SELECT   CorrelationId, counter, ROW_NUMBER() OVER ( PARTITION BY CorrelationId ORDER BY TrackingPoint ASC )
               FROM     dbo.TrackingPoint
               WHERE    TrackingPoint LIKE 'Server: MSMQ%'
                        AND AdditionalData LIKE 'after%'
                        OR TrackingPoint LIKE 'MSMQSensorTask: After%'
             )
    SELECT TOP ( 1000 )
            CorrelationId, MAX(CASE WHEN RowNum = 1 THEN TimeInMsec
                               END) AS 'SendFromClient_MSMQ in msec', MAX(CASE WHEN RowNum = 2 THEN TimeInMsec
                                                                          END) AS 'ProcessFromMSMQ_Server in msec '
    FROM    ServerTiming
    GROUP BY CorrelationId;

-- This one selects the time it takes from the server to write a value until it is read by the client
-- we have to be careful as not all messages will be read - so we have to remove the empty records
WITH    Timing ( CorrelationId, TrackingPoint, RowNum, TimeStamp )
          AS ( SELECT   CorrelationId, TrackingPoint, ROW_NUMBER() OVER ( PARTITION BY CorrelationId ORDER BY TrackingPoint DESC ) AS RowNum, Timestamp
               FROM     dbo.TrackingPoint
               WHERE    TrackingPoint LIKE 'MSMQSensorTask: GetVal%'
                        OR TrackingPoint LIKE 'Server: Actuator_S%'
               GROUP BY CorrelationId, TrackingPoint, Timestamp
             )
    SELECT TOP ( 1000 )
            CorrelationId, DATEDIFF(MILLISECOND, MAX(CASE WHEN RowNum = 1 THEN Timestamp
                                                     END), MAX(CASE WHEN RowNum = 2 THEN Timestamp
                                                               END)) AS 'TotalTimeToReceive in msec'
    FROM    Timing A
    WHERE   EXISTS ( SELECT 1
                     FROM   Timing
                     WHERE  RowNum = 2
                            AND A.CorrelationId = CorrelationId )
    GROUP BY A.CorrelationId