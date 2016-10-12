print('Performance Test IronPython SQL')

from System import Console
import collections
import sys
import clr
clr.AddReference('System.Data')

from System.Data.SqlClient import SqlConnection, SqlParameter
from System.Data import ConnectionState
from System import DateTime
from System import Convert
from decimal import *

conn_string = 'data source=localhost; initial catalog=EXPERIMENTS; trusted_connection=True'
connection = SqlConnection(conn_string)
baseDataConnection = SqlConnection(conn_string)

try:
    connection.Open()
    baseDataConnection.Open()

    insertCmd = connection.CreateCommand()

    # maximum iterations per loop and in total
    maxIterationsPerLoop = 500
    maxLoops = 5000

    resultDict = collections.defaultdict(list)
            

    loopCount = 0

    while loopCount < maxLoops:

        print 'Loop: {0}'.format(loopCount+1)

        iterationCount = 0
        now = DateTime.Now
        while iterationCount < maxIterationsPerLoop:
            insertCmd.CommandText = "INSERT  INTO dbo.PerformanceTest ( StringCol ) VALUES  ( 'ABC' )"
            insertCmd.ExecuteNonQuery()
            iterationCount += 1

        resultDict["FixedString"].append(DateTime.Now.Subtract(now).Milliseconds)



        iterationCount = 0
        now = DateTime.Now
        while iterationCount < maxIterationsPerLoop:

            # fixed INT
            insertCmd.CommandText = "INSERT  INTO dbo.PerformanceTest ( IntCol ) VALUES  ( 12333 )"
            insertCmd.ExecuteNonQuery()
            iterationCount += 1

        resultDict["FixedInt"].append(DateTime.Now.Subtract(now).Milliseconds)



        iterationCount = 0
        now = DateTime.Now
        while iterationCount < maxIterationsPerLoop:
            insertCmd.CommandText = "INSERT  INTO dbo.PerformanceTest ( IntCol ) VALUES  ( CONVERT(INT, '12333') )"
            insertCmd.ExecuteNonQuery()
            iterationCount += 1

        resultDict["DynamicInt"].append(DateTime.Now.Subtract(now).Milliseconds)

        iterationCount = 0
        now = DateTime.Now
        while iterationCount < maxIterationsPerLoop:
            insertCmd.CommandText = "INSERT  INTO dbo.PerformanceTest ( BigIntCol ) VALUES  ( 12333 )"
            insertCmd.ExecuteNonQuery()
            iterationCount += 1

        resultDict["FixedBigInt"].append(DateTime.Now.Subtract(now).Milliseconds)


        iterationCount = 0
        now = DateTime.Now
        while iterationCount < maxIterationsPerLoop:
            insertCmd.CommandText = "INSERT  INTO dbo.PerformanceTest ( BigIntCol ) VALUES  ( CONVERT(BIGINT, '12333') )"
            insertCmd.ExecuteNonQuery()
            iterationCount += 1

        resultDict["DynamicBigInt"].append(DateTime.Now.Subtract(now).Milliseconds)

        iterationCount = 0
        now = DateTime.Now
        while iterationCount < maxIterationsPerLoop:
            insertCmd.CommandText = "INSERT  INTO dbo.PerformanceTest ( FloatCol ) VALUES  ( 1233.2323 )"
            insertCmd.ExecuteNonQuery()
            iterationCount += 1

        resultDict["FixedFloat"].append(DateTime.Now.Subtract(now).Milliseconds)


        iterationCount = 0
        now = DateTime.Now
        while iterationCount < maxIterationsPerLoop:
            insertCmd.CommandText = "INSERT  INTO dbo.PerformanceTest ( FloatCol ) VALUES  ( CONVERT(FLOAT, '1233.2323') )"
            insertCmd.ExecuteNonQuery()
            iterationCount += 1

        resultDict["DynamicFloat"].append(DateTime.Now.Subtract(now).Milliseconds)


        iterationCount = 0
        now = DateTime.Now
        while iterationCount < maxIterationsPerLoop:
            insertCmd.CommandText = "INSERT  INTO dbo.PerformanceTest ( NumericCol ) VALUES  ( 123411.3123 )"
            insertCmd.ExecuteNonQuery()
            iterationCount += 1

        resultDict["FixedNumeric"].append(DateTime.Now.Subtract(now).Milliseconds)


        iterationCount = 0
        now = DateTime.Now
        while iterationCount < maxIterationsPerLoop:
            insertCmd.CommandText = "INSERT  INTO dbo.PerformanceTest ( NumericCol ) VALUES  ( CONVERT(NUMERIC, '123411.3123') )"
            insertCmd.ExecuteNonQuery()
            iterationCount += 1

        resultDict["DynamicNumeric"].append(DateTime.Now.Subtract(now).Milliseconds)

        
        iterationCount = 0
        now = DateTime.Now
        while iterationCount < maxIterationsPerLoop:
            insertCmd.CommandText = "INSERT  INTO dbo.PerformanceTest ( BitCol ) VALUES  ( 1 )"
            insertCmd.ExecuteNonQuery()
            iterationCount += 1

        resultDict["FixedBit"].append(DateTime.Now.Subtract(now).Milliseconds)


        iterationCount = 0
        now = DateTime.Now
        while iterationCount < maxIterationsPerLoop:
            insertCmd.CommandText = "INSERT  INTO dbo.PerformanceTest ( BitCol ) VALUES  ( CONVERT(BIT, '1') )"
            insertCmd.ExecuteNonQuery()
            iterationCount += 1

        resultDict["DynamicBit"].append(DateTime.Now.Subtract(now).Milliseconds)


        # Pure INSERT / SELECT combination
        iterationCount = 0
        now = DateTime.Now
        insertCmd.CommandText = "INSERT INTO dbo.PerformanceTestTransform ( IntCol ) SELECT TOP ( 1000 ) IntCol + 2 FROM dbo.PerformanceTestBaseData"
        insertCmd.ExecuteNonQuery()
        resultDict["SelectBasedFixedTransformInt"].append(DateTime.Now.Subtract(now).Milliseconds)

        iterationCount = 0
        now = DateTime.Now
        selectCmd = baseDataConnection.CreateCommand()
        selectCmd.CommandText = 'SELECT TOP ( 1000 ) IntCol FROM dbo.PerformanceTestBaseData'
        reader = selectCmd.ExecuteReader()
        while reader.Read():
            value = reader['IntCol'] + 2
            insertCmd.CommandText = "INSERT  INTO dbo.PerformanceTestTransform ( IntCol ) VALUES  ({0:d})".format(value)
            # print insertCmd.CommandText
            insertCmd.ExecuteNonQuery()
        reader.Close()
        resultDict["FixedTransformInt"].append(DateTime.Now.Subtract(now).Milliseconds)

        iterationCount = 0
        now = DateTime.Now
        # selectCmd = baseDataConnection.CreateCommand()
        selectCmd.CommandText = 'SELECT TOP ( 1000 ) CONVERT(VARCHAR(20), IntCol) FROM dbo.PerformanceTestBaseData'
        reader = selectCmd.ExecuteReader()
        while reader.Read():
            value = Convert.ToInt32(reader[0]) + 2
            insertCmd.CommandText = "INSERT  INTO dbo.PerformanceTestTransform ( IntCol ) VALUES  (CONVERT(INT, {0:d}))".format(value)
            # print insertCmd.CommandText
            insertCmd.ExecuteNonQuery()
        reader.Close()
        resultDict["DynamicTransformInt"].append(DateTime.Now.Subtract(now).Milliseconds)



        # Pure INSERT / SELECT combination
        iterationCount = 0
        now = DateTime.Now
        insertCmd.CommandText = "INSERT INTO dbo.PerformanceTestTransform ( NumericCol ) SELECT TOP ( 1000 ) NumericCol + 2 FROM dbo.PerformanceTestBaseData"
        insertCmd.ExecuteNonQuery()
        resultDict["SelectBasedFixedTransformNumeric"].append(DateTime.Now.Subtract(now).Milliseconds)

        iterationCount = 0
        now = DateTime.Now
        # selectCmd = baseDataConnection.CreateCommand()
        selectCmd.CommandText = 'SELECT TOP ( 1000 ) NumericCol FROM dbo.PerformanceTestBaseData'
        reader = selectCmd.ExecuteReader()
        while reader.Read():
            value = Convert.ToDecimal(reader['NumericCol'])
            insertCmd.CommandText = "INSERT  INTO dbo.PerformanceTestTransform ( NumericCol ) VALUES  ({0})".format(value)
            insertCmd.CommandText = insertCmd.CommandText.Replace(',', '.')
            # print insertCmd.CommandText
            insertCmd.ExecuteNonQuery()
        reader.Close()
        resultDict["FixedTransformNumeric"].append(DateTime.Now.Subtract(now).Milliseconds)

        iterationCount = 0
        now = DateTime.Now
        # selectCmd = baseDataConnection.CreateCommand()
        selectCmd.CommandText = 'SELECT TOP ( 1000 ) CONVERT(VARCHAR(30), NumericCol) FROM dbo.PerformanceTestBaseData'
        reader = selectCmd.ExecuteReader()
        while reader.Read():
            value = Convert.ToDecimal(reader[0].Replace(',', '.')) + 2
            insertCmd.CommandText = "INSERT  INTO dbo.PerformanceTestTransform ( NumericCol ) VALUES  (CONVERT(NUMERIC, '{0}'))".format(value)
            # print insertCmd.CommandText
            insertCmd.ExecuteNonQuery()
        reader.Close()
        resultDict["DynamicTransformNumeric"].append(DateTime.Now.Subtract(now).Milliseconds)

        # next iteration
        loopCount += 1

        for key, list in resultDict.items():
            print key, list[-1], 'msec'


    print "Dumping results"
    print resultDict.Keys
    with open("testresult_" + DateTime.Now.ToShortDateString() + ".txt", "w") as text_file:
        for key, list in resultDict.items():
            for value in list:
                text_file.write("{0}\t {1}\n".format(key, value))
                print key, value, 'msec'

    print 'All tests ran without problems'

except:
    type, value = sys.exc_info() [:2]
    print type
    print value

finally:
    if (connection.State == ConnectionState.Open):
        connection.Close()
    if (baseDataConnection.State == ConnectionState.Open):
        baseDataConnection.Close()

print 'Press ENTER to continue'
Console.ReadLine()

