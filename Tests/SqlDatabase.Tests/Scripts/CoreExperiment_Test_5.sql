USE Experiments

DECLARE @COUNTER INT ,
    @OuterLoop INT;
    
DECLARE @MaxLoops INT ,
    @MaxIterationsPerLoop INT;

DECLARE @starttime DATETIME2
DECLARE @duration DATETIME2


SET @OuterLoop = 0;
SET @MaxLoops = 5000;
SET @MaxIterationsPerLoop = 500;

SET NOCOUNT ON;

-- Delete the results table if existent
IF EXISTS ( SELECT  *
            FROM    tempdb..sysobjects
            WHERE   id = OBJECT_ID('tempdb..#IterationResult') ) 
    DROP TABLE #IterationResult
    
-- create the table for the results    
CREATE TABLE #IterationResult
    (
      TestPart VARCHAR(50) ,
      TestRun INT ,
      TestCreated DATETIME2 ,
      Duration INT
    )

-- perform 100 iterations
WHILE @OuterLoop < @MaxLoops 
    BEGIN
    
        PRINT 'Loop: ' + CONVERT(VARCHAR(20), @OuterLoop + 1);
        
        SET @COUNTER = 0;
        SET @starttime = GETDATE();




		/*
		The dynamic insertion tests
		*/

		-- of dynmic insert of string
        WHILE @COUNTER < @MaxIterationsPerLoop 
            BEGIN
                INSERT  INTO dbo.PerformanceTest ( StringCol )
                VALUES  ( 'ABC' )
	        
                SET @COUNTER = @COUNTER + 1;
	        
            END
            
        -- write some result
        INSERT  INTO #IterationResult ( TestPart, TestRun, TestCreated, Duration )
        VALUES  ( 'FixedString', @OuterLoop, GETDATE(), DATEDIFF(ms, @starttime, GETDATE()) )
	    
	    -- for the loop control
        SET @COUNTER = 0;
        SET @starttime = GETDATE();
        
        -- of dynmic insert of string
        WHILE @COUNTER < @MaxIterationsPerLoop 
            BEGIN
                INSERT  INTO dbo.PerformanceTest ( IntCol )
                VALUES  ( 12 )
	        
                SET @COUNTER = @COUNTER + 1;
	        
            END
            
        -- write some result
        INSERT  INTO #IterationResult ( TestPart, TestRun, TestCreated, Duration )
        VALUES  ( 'FixedInt', @OuterLoop, GETDATE(), DATEDIFF(ms, @starttime, GETDATE()) )
                

	    -- for the loop control
        SET @COUNTER = 0;
        SET @starttime = GETDATE();
        
        -- of dynmic insert of string
        WHILE @COUNTER < @MaxIterationsPerLoop 
            BEGIN
                INSERT  INTO dbo.PerformanceTest ( IntCol )
                VALUES  ( CONVERT(INT, '121') )
	        
                SET @COUNTER = @COUNTER + 1;
	        
            END
            
        -- write some result
        INSERT  INTO #IterationResult ( TestPart, TEstRun, TestCreated, Duration )
        VALUES  ( 'DynamicInt', @OuterLoop, GETDATE(), DATEDIFF(ms, @starttime, GETDATE()) )
	    
	     -- for the loop control
        SET @COUNTER = 0;
        SET @starttime = GETDATE();
        
        -- of dynmic insert of string
        WHILE @COUNTER < @MaxIterationsPerLoop 
            BEGIN
                INSERT  INTO dbo.PerformanceTest ( BigintCol )
                VALUES  ( 123 )
	        
                SET @COUNTER = @COUNTER + 1;
	        
            END
            
        -- write some result
        INSERT  INTO #IterationResult ( TestPart, TestRun, TestCreated, Duration )
        VALUES  ( 'FixedBigInt', @OuterLoop, GETDATE(), DATEDIFF(ms, @starttime, GETDATE()) )
                
	     -- for the loop control
        SET @COUNTER = 0;
        SET @starttime = GETDATE();
        
        -- of dynmic insert of string
        WHILE @COUNTER < @MaxIterationsPerLoop 
            BEGIN
                INSERT  INTO dbo.PerformanceTest ( BigintCol )
                VALUES  ( CONVERT(BIGINT, '1212') )
	        
                SET @COUNTER = @COUNTER + 1;
	        
            END
            
        -- write some result
        INSERT  INTO #IterationResult ( TestPart, TestRun, TestCreated, Duration )
        VALUES  ( 'DynamicBigInt', @OuterLoop, GETDATE(), DATEDIFF(ms, @starttime, GETDATE()) )


	     -- for the loop control
        SET @COUNTER = 0;
        SET @starttime = GETDATE();
        
        -- of dynmic insert of string
        WHILE @COUNTER < @MaxIterationsPerLoop 
            BEGIN
                INSERT  INTO dbo.PerformanceTest ( FloatCol )
                VALUES  ( 123.433 )
	        
                SET @COUNTER = @COUNTER + 1;
	        
            END
            
        -- write some result
        INSERT  INTO #IterationResult ( TestPart, TestRun, TestCreated, Duration )
        VALUES  ( 'FixedFloat', @OuterLoop, GETDATE(), DATEDIFF(ms, @starttime, GETDATE()) )
                

	     -- for the loop control
        SET @COUNTER = 0;
        SET @starttime = GETDATE();
        
        -- of dynmic insert of string
        WHILE @COUNTER < @MaxIterationsPerLoop 
            BEGIN
                INSERT  INTO dbo.PerformanceTest ( FloatCol )
                VALUES  ( CONVERT(FLOAT, '12123.12') )
	        
                SET @COUNTER = @COUNTER + 1;
	        
            END
            
        -- write some result
        INSERT  INTO #IterationResult ( TestPart, TEstRun, TestCreated, Duration )
        VALUES  ( 'DynamicFloat', @OuterLoop, GETDATE(), DATEDIFF(ms, @starttime, GETDATE()) )
                
	    
	     -- for the loop control
        SET @COUNTER = 0;
        SET @starttime = GETDATE();
        
        -- of dynmic insert of string
        WHILE @COUNTER < @MaxIterationsPerLoop 
            BEGIN
                INSERT  INTO dbo.PerformanceTest ( BitCol )
                VALUES  ( 1 )
	        
                SET @COUNTER = @COUNTER + 1;
	        
            END
            
        -- write some result
        INSERT  INTO #IterationResult ( TestPart, TestRun, TestCreated, Duration )
        VALUES  ( 'FixedBit', @OuterLoop, GETDATE(), DATEDIFF(ms, @starttime, GETDATE()) )
                

	     -- for the loop control
        SET @COUNTER = 0;
        SET @starttime = GETDATE();
        
        -- of dynmic insert of string
        WHILE @COUNTER < @MaxIterationsPerLoop 
            BEGIN
                INSERT  INTO dbo.PerformanceTest ( BitCol )
                VALUES  ( CONVERT(BIT, '1') )
	        
                SET @COUNTER = @COUNTER + 1;
	        
            END
            
        -- write some result
        INSERT  INTO #IterationResult ( TestPart, TestRun, TestCreated, Duration )
        VALUES  ( 'DynamicBit', @OuterLoop, GETDATE(), DATEDIFF(ms, @starttime, GETDATE()) )
                
	    
	     -- for the loop control
        SET @COUNTER = 0;
        SET @starttime = GETDATE();
        
        -- of dynmic insert of string
        WHILE @COUNTER < @MaxIterationsPerLoop 
            BEGIN
                INSERT  INTO dbo.PerformanceTest ( NumericCol )
                VALUES  ( 123411.3123 )
	        
                SET @COUNTER = @COUNTER + 1;
	        
            END
            
        -- write some result
        INSERT  INTO #IterationResult ( TestPart, TestRun, TestCreated, Duration )
        VALUES  ( 'FixedNumeric', @OuterLoop, GETDATE(), DATEDIFF(ms, @starttime, GETDATE()) )
                

	     -- for the loop control
        SET @COUNTER = 0;
        SET @starttime = GETDATE();
        
        -- of dynmic insert of string
        WHILE @COUNTER < @MaxIterationsPerLoop 
            BEGIN
                INSERT  INTO dbo.PerformanceTest ( NumericCol )
                VALUES  ( CONVERT(NUMERIC, '123411.3123') )
	        
                SET @COUNTER = @COUNTER + 1;
	        
            END
            
        -- write some result
        INSERT  INTO #IterationResult ( TestPart, TEstRun, TestCreated, Duration )
        VALUES  ( 'DynamicNumeric', @OuterLoop, GETDATE(), DATEDIFF(ms, @starttime, GETDATE()) )
               
			   
		
		
		/*
		Retrieve values from the dynamic tables vs. fixed retrievals, multiply (or change them) and write them back - native or via conversion
		*/	   

        SET @starttime = GETDATE();

        INSERT  INTO dbo.PerformanceTestTransform ( IntCol )
                SELECT TOP ( 1000 )
                        IntCol + 2
                FROM    dbo.PerformanceTestBaseData
        INSERT  INTO #IterationResult ( TestPart, TEstRun, TestCreated, Duration )
        VALUES  ( 'SelectBasedFixedTransformInt', @OuterLoop, GETDATE(), DATEDIFF(ms, @starttime, GETDATE()) )


        SET @starttime = GETDATE();

        DECLARE @IntValue INT;
        DECLARE IntCursor CURSOR
        FOR
            SELECT TOP ( 1000 )
                    IntCol
            FROM    dbo.PerformanceTestBaseData
        OPEN IntCursor;
        FETCH NEXT FROM IntCursor INTO @IntValue;
        WHILE @@FETCH_STATUS = 0 
            BEGIN
                INSERT  INTO dbo.PerformanceTestTransform ( IntCol )
                VALUES  ( @IntValue + 2 )

                FETCH NEXT FROM IntCursor INTO @IntValue;
            END      
        CLOSE IntCursor
        DEALLOCATE IntCursor
        
        -- write some result
        INSERT  INTO #IterationResult ( TestPart, TEstRun, TestCreated, Duration )
        VALUES  ( 'FixedTransformInt', @OuterLoop, GETDATE(), DATEDIFF(ms, @starttime, GETDATE()) )


		-- insert INT which are converted first

        SET @starttime = GETDATE();

        DECLARE @DynamicIntValue VARCHAR(20)
        DECLARE IntCursor CURSOR
        FOR
            SELECT TOP ( 1000 )
                    CONVERT(VARCHAR(20), IntCol)
            FROM    dbo.PerformanceTestBaseData
        OPEN IntCursor;
        FETCH NEXT FROM IntCursor INTO @DynamicIntValue
        WHILE @@FETCH_STATUS = 0 
            BEGIN
                INSERT  INTO dbo.PerformanceTestTransform ( IntCol )
                VALUES  ( CONVERT(INT, @DynamicIntValue) + 2 )

                FETCH NEXT FROM IntCursor INTO @DynamicIntValue
            END      
        CLOSE IntCursor
        DEALLOCATE IntCursor
        
        -- write some result
        INSERT  INTO #IterationResult ( TestPart, TEstRun, TestCreated, Duration )
        VALUES  ( 'DyanmicTransformInt', @OuterLoop, GETDATE(), DATEDIFF(ms, @starttime, GETDATE()) )




        SET @starttime = GETDATE();

        INSERT  INTO dbo.PerformanceTestTransform ( NumericCol )
                SELECT TOP ( 1000 )
                        NumericCol + 2
                FROM    dbo.PerformanceTestBaseData
        INSERT  INTO #IterationResult ( TestPart, TestRun, TestCreated, Duration )
        VALUES  ( 'SelectBasedFixedTransformNumeric', @OuterLoop, GETDATE(), DATEDIFF(ms, @starttime, GETDATE()) )


        SET @starttime = GETDATE();

        DECLARE @NumericValue NUMERIC;
        DECLARE NumericCursor CURSOR
        FOR
            SELECT TOP ( 1000 )
                    NumericCol
            FROM    dbo.PerformanceTestBaseData
        OPEN NumericCursor;
        FETCH NEXT FROM NumericCursor INTO @NumericValue
        WHILE @@FETCH_STATUS = 0 
            BEGIN
                INSERT  INTO dbo.PerformanceTestTransform ( NumericCol )
                VALUES  ( @NumericValue + 2 )

                FETCH NEXT FROM NumericCursor INTO @NumericValue;
            END      
        CLOSE NumericCursor
        DEALLOCATE NumericCursor
        
        -- write some result
        INSERT  INTO #IterationResult ( TestPart, TestRun, TestCreated, Duration )
        VALUES  ( 'FixedTransformNumeric', @OuterLoop, GETDATE(), DATEDIFF(ms, @starttime, GETDATE()) )


		-- insert INT which are converted first

        SET @starttime = GETDATE();

        DECLARE @DynamicNumericValue VARCHAR(30)
        DECLARE NumericCursor CURSOR
        FOR
            SELECT TOP ( 1000 )
                    CONVERT(VARCHAR(30), NumericCol)
            FROM    dbo.PerformanceTestBaseData
        OPEN NumericCursor;
        FETCH NEXT FROM NumericCursor INTO @DynamicNumericValue
        WHILE @@FETCH_STATUS = 0 
            BEGIN
                INSERT  INTO dbo.PerformanceTestTransform ( NumericCol )
                VALUES  ( CONVERT(NUMERIC, @DynamicNumericValue) + 2 )

                FETCH NEXT FROM NumericCursor INTO @DynamicNumericValue
            END      
        CLOSE NumericCursor
        DEALLOCATE NumericCursor
        
        -- write some result
        INSERT  INTO #IterationResult ( TestPart, TestRun, TestCreated, Duration )
        VALUES  ( 'DyanmicTransformNumeric', @OuterLoop, GETDATE(), DATEDIFF(ms, @starttime, GETDATE()) )








			   
			    
		-- dump the results for one test run
        --SELECT  TestPart, TestRun, TestCreated, CONVERT(VARCHAR(20), Duration) AS 'Duration in msec'
        --FROM    #IterationResult
        --WHERE   TestRun = @OuterLoop

	    -- next iteration
        SET @OuterLoop = @OuterLoop + 1;
    END

-- test results 
SELECT  TestPart, TestCreated, CONVERT(VARCHAR(20), Duration) AS 'Duration in msec'
FROM    #IterationResult
ORDER BY TestPart DESC, TestCreated ASC


SET NOCOUNT OFF;
