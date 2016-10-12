-- DELETE FROM dbo.TrackingPoint 
SELECT  *
FROM    dbo.TrackingPoint
ORDER BY Timestamp ASC;

-- This one will select all "final" values in msec for sending data to the server
WITH    ServerTiming ( CorrelationId, TimeInMsec, RowNum )
          AS ( SELECT   CorrelationId, counter, ROW_NUMBER() OVER ( PARTITION BY CorrelationId ORDER BY TrackingPoint ASC )
               FROM     dbo.TrackingPoint
               WHERE    TrackingPoint LIKE 'Server: HTTP%'
                        AND AdditionalData LIKE 'after%'
                        OR TrackingPoint LIKE 'HttpSensorTask: After%'
             )
    SELECT TOP ( 1000 )
            CorrelationId, MAX(CASE WHEN RowNum = 1 THEN TimeInMsec
                               END) AS 'SendFromClient_HTTP in msec', MAX(CASE WHEN RowNum = 2 THEN TimeInMsec
                                                                          END) AS 'ProcessFromHTTP_Server in msec '
    FROM    ServerTiming
    GROUP BY CorrelationId;

-- This one selects the time it takes from the server to write a value until it is read by the client
-- we have to be careful as not all messages will be read - so we have to remove the empty records
SELECT  TOP (1000) CorrelationId, Counter AS 'TotalTimeToReceive in msec'
FROM    dbo.TrackingPoint
WHERE   TrackingPoint LIKE 'HttpSensorTask: GetVal%'

SELECT  TOP (1000) CorrelationId, Counter AS 'ServerTimeToRetrieve in msec'
FROM    dbo.TrackingPoint
WHERE   TrackingPoint LIKE 'Server: HTTP req%' AND AdditionalData LIKE 'after pro%'
