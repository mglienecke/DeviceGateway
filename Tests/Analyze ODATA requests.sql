SELECT  CONVERT(INT, SUBSTRING(CorrelationId, CHARINDEX('?$top=', CorrelationId) + 6, 20)) AS 'RequestRecords', Counter AS 'msec'
FROM    dbo.TrackingPoint
WHERE   TrackingPoint LIKE 'ODATA service%'
ORDER BY Timestamp ASC;

WITH    ClientMeasures ( CorrelationId, RowNum, Counter )
          AS ( SELECT   CONVERT(INT, CorrelationId) , ROW_NUMBER() OVER ( PARTITION BY CorrelationId ORDER BY CorrelationId ASC ) AS "RowNum", Counter
               FROM     dbo.TrackingPoint
               WHERE    TrackingPoint LIKE 'ODATA client%'
             )
    SELECT  CorrelationId, MAX(CASE WHEN RowNum = 1 THEN Counter
                               END) AS 'msec', MAX(CASE WHEN RowNum = 2 THEN Counter
                                                   END) AS 'CPU', MAX(CASE WHEN RowNum = 3 THEN Counter
                                                                      END) AS 'RAM'
    FROM    ClientMeasures
    GROUP BY CorrelationId
    ORDER BY CorrelationId ASC 


-- DELETE FROM dbo.TrackingPoint