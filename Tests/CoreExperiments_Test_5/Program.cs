using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CoreExperiments_Test_5
{
    class Program
    {
        static void Main(string[] args)
        {
            const int MaxIterationsPerLoop = 500;
            const int MaxLoops = 5000;

            Dictionary<string, List<int>> ResultDict = new Dictionary<string, List<int>>();
            ResultDict.Add("FixedString", new List<int>());
            ResultDict.Add("FixedInt", new List<int>());
            ResultDict.Add("DynamicInt", new List<int>());
            ResultDict.Add("FixedBigInt", new List<int>());
            ResultDict.Add("DynamicBigInt", new List<int>());
            ResultDict.Add("FixedFloat", new List<int>());
            ResultDict.Add("DynamicFloat", new List<int>());
            ResultDict.Add("FixedNumeric", new List<int>());
            ResultDict.Add("DynamicNumeric", new List<int>());
            ResultDict.Add("FixedBit", new List<int>());
            ResultDict.Add("DynamicBit", new List<int>());
            ResultDict.Add("SelectBasedFixedTransformInt", new List<int>());
            ResultDict.Add("FixedTransformInt", new List<int>());
            ResultDict.Add("DynamicTransformInt", new List<int>());
            ResultDict.Add("SelectBasedFixedTransformNumeric", new List<int>());
            ResultDict.Add("FixedTransformNumeric", new List<int>());
            ResultDict.Add("DynamicTransformNumeric", new List<int>());

            SqlConnection connection = new SqlConnection("data source=localhost; initial catalog=EXPERIMENTS; trusted_connection=True");
            SqlConnection basedDataConnection = new SqlConnection("data source=localhost; initial catalog=EXPERIMENTS; trusted_connection=True");

            DateTime now;
            try
            {
                connection.Open();
                basedDataConnection.Open();

                SqlCommand insertCmd = connection.CreateCommand();
                SqlCommand selectCmd = basedDataConnection.CreateCommand();

                for (int loopCount = 0; loopCount < MaxLoops; loopCount++)
                {
                    Console.WriteLine("Loop: {0}", loopCount+1);

                    Stopwatch watch = new Stopwatch();

                    // Direct String
                    watch.Restart();
                    for (int i = 0; i < MaxIterationsPerLoop; i++)
                    {
                        insertCmd.CommandText = "INSERT  INTO dbo.PerformanceTest ( StringCol ) VALUES  ( 'ABC' )";
                        insertCmd.ExecuteNonQuery();
                    }
                    ResultDict["FixedString"].Add(Convert.ToInt32(watch.ElapsedMilliseconds));

                    // Direct INT
                    watch.Restart();
                    for (int i = 0; i < MaxIterationsPerLoop; i++)
                    {
                        insertCmd.CommandText = "INSERT  INTO dbo.PerformanceTest ( IntCol ) VALUES  ( 12333 )";
                        insertCmd.ExecuteNonQuery();
                    }
                    ResultDict["FixedInt"].Add(Convert.ToInt32(watch.ElapsedMilliseconds));


                    // Direct INT
                    watch.Restart();
                    for (int i = 0; i < MaxIterationsPerLoop; i++)
                    {
                        insertCmd.CommandText = "INSERT  INTO dbo.PerformanceTest ( IntCol ) VALUES  ( CONVERT(INT, '12333') )";
                        insertCmd.ExecuteNonQuery();
                    }
                    ResultDict["DynamicInt"].Add(Convert.ToInt32(watch.ElapsedMilliseconds));

                    // Direct INT
                    watch.Restart();
                    for (int i = 0; i < MaxIterationsPerLoop; i++)
                    {
                        insertCmd.CommandText = "INSERT  INTO dbo.PerformanceTest ( BigIntCol ) VALUES  ( 12333 )";
                        insertCmd.ExecuteNonQuery();
                    }
                    ResultDict["FixedBigInt"].Add(Convert.ToInt32(watch.ElapsedMilliseconds));

                    // Direct INT
                    watch.Restart();
                    for (int i = 0; i < MaxIterationsPerLoop; i++)
                    {
                        insertCmd.CommandText = "INSERT  INTO dbo.PerformanceTest ( BigIntCol ) VALUES  ( CONVERT(BIGINT, '12333') )";
                        insertCmd.ExecuteNonQuery();
                    }
                    ResultDict["DynamicBigInt"].Add(Convert.ToInt32(watch.ElapsedMilliseconds));

                    // Direct INT
                    watch.Restart();
                    for (int i = 0; i < MaxIterationsPerLoop; i++)
                    {
                        insertCmd.CommandText = "INSERT  INTO dbo.PerformanceTest ( FloatCol ) VALUES  ( 1233.2323 )";
                        insertCmd.ExecuteNonQuery();
                    }
                    ResultDict["FixedFloat"].Add(Convert.ToInt32(watch.ElapsedMilliseconds));

                    // Direct INT
                    watch.Restart();
                    for (int i = 0; i < MaxIterationsPerLoop; i++)
                    {
                        insertCmd.CommandText = "INSERT  INTO dbo.PerformanceTest ( FloatCol ) VALUES  ( CONVERT(FLOAT, '1233.2323') )";
                        insertCmd.ExecuteNonQuery();
                    }
                    ResultDict["DynamicFloat"].Add(Convert.ToInt32(watch.ElapsedMilliseconds));

                    // Direct INT
                    watch.Restart();
                    for (int i = 0; i < MaxIterationsPerLoop; i++)
                    {
                        insertCmd.CommandText = "INSERT  INTO dbo.PerformanceTest ( NumericCol ) VALUES  ( 123411.3123 )";
                        insertCmd.ExecuteNonQuery();
                    }
                    ResultDict["FixedNumeric"].Add(Convert.ToInt32(watch.ElapsedMilliseconds));


                    // Direct INT
                    watch.Restart();
                    for (int i = 0; i < MaxIterationsPerLoop; i++)
                    {
                        insertCmd.CommandText = "INSERT  INTO dbo.PerformanceTest ( NumericCol ) VALUES  ( CONVERT(NUMERIC, '123411.3123') )";
                        insertCmd.ExecuteNonQuery();
                    }
                    ResultDict["DynamicNumeric"].Add(Convert.ToInt32(watch.ElapsedMilliseconds));


                    // Direct INT
                    watch.Restart();
                    for (int i = 0; i < MaxIterationsPerLoop; i++)
                    {
                        insertCmd.CommandText = "INSERT  INTO dbo.PerformanceTest ( BitCol ) VALUES  ( 1 )";
                        insertCmd.ExecuteNonQuery();
                    }
                    ResultDict["FixedBit"].Add(Convert.ToInt32(watch.ElapsedMilliseconds));


                    // Direct INT
                    watch.Restart();
                    for (int i = 0; i < MaxIterationsPerLoop; i++)
                    {
                        insertCmd.CommandText = "INSERT  INTO dbo.PerformanceTest ( BitCol ) VALUES  ( CONVERT(BIT, '1') )";
                        insertCmd.ExecuteNonQuery();
                    }
                    ResultDict["DynamicBit"].Add(Convert.ToInt32(watch.ElapsedMilliseconds));


                    // 
                    watch.Restart();
                    insertCmd.CommandText = "INSERT INTO dbo.PerformanceTestTransform ( IntCol ) SELECT TOP ( 1000 ) IntCol + 2 FROM dbo.PerformanceTestBaseData";
                    insertCmd.ExecuteNonQuery();
                    ResultDict["SelectBasedFixedTransformInt"].Add(Convert.ToInt32(watch.ElapsedMilliseconds));

                    //
                    watch.Restart();
                    selectCmd.CommandText = "SELECT TOP ( 1000 ) IntCol FROM dbo.PerformanceTestBaseData";
                    SqlDataReader reader = selectCmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Int32 val = ((int)reader["IntCol"]) + 2;
                        insertCmd.CommandText = string.Format("INSERT  INTO dbo.PerformanceTestTransform ( IntCol ) VALUES  ({0})", val);
                        insertCmd.ExecuteNonQuery();
                    }
                    reader.Close();
                    ResultDict["FixedTransformInt"].Add(Convert.ToInt32(watch.ElapsedMilliseconds));


                    // 
                    watch.Restart();
                    selectCmd.CommandText = "SELECT TOP ( 1000 ) CONVERT(VARCHAR(20), IntCol) FROM dbo.PerformanceTestBaseData";
                    reader = selectCmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Int32 val = Convert.ToInt32(reader[0]) + 2;
                        insertCmd.CommandText = string.Format("INSERT  INTO dbo.PerformanceTestTransform ( IntCol ) VALUES  (CONVERT(INT, {0}))", val);
                        insertCmd.ExecuteNonQuery();
                    }
                    reader.Close();
                    ResultDict["DynamicTransformInt"].Add(Convert.ToInt32(watch.ElapsedMilliseconds));




                    // 
                    watch.Restart();
                    insertCmd.CommandText = "INSERT INTO dbo.PerformanceTestTransform ( NumericCol ) SELECT TOP ( 1000 ) NumericCol + 2 FROM dbo.PerformanceTestBaseData";
                    insertCmd.ExecuteNonQuery();
                    ResultDict["SelectBasedFixedTransformNumeric"].Add(Convert.ToInt32(watch.ElapsedMilliseconds));


                    //
                    watch.Restart();
                    selectCmd.CommandText = "SELECT TOP ( 1000 ) NumericCol FROM dbo.PerformanceTestBaseData";
                    reader = selectCmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Decimal val = ((decimal)reader["NumericCol"]) + 2;
                        insertCmd.CommandText = string.Format("INSERT  INTO dbo.PerformanceTestTransform ( NumericCol ) VALUES  ({0})", val);
                        insertCmd.CommandText = insertCmd.CommandText.Replace(',', '.');
                        insertCmd.ExecuteNonQuery();
                    }
                    reader.Close();
                    ResultDict["FixedTransformNumeric"].Add(Convert.ToInt32(watch.ElapsedMilliseconds));


                    // 
                    watch.Restart();
                    selectCmd.CommandText = "SELECT TOP ( 1000 ) CONVERT(VARCHAR(30), NumericCol) FROM dbo.PerformanceTestBaseData";
                    reader = selectCmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Decimal val = Convert.ToDecimal(reader[0].ToString().Replace(',', '.')) + 2;
                        insertCmd.CommandText = string.Format("INSERT  INTO dbo.PerformanceTestTransform ( NumericCol ) VALUES  (CONVERT(NUMERIC, '{0}'))", val);
                        insertCmd.ExecuteNonQuery();
                    }
                    reader.Close();
                    ResultDict["DynamicTransformNumeric"].Add(Convert.ToInt32(watch.ElapsedMilliseconds));

                    // Dump der Ergebnisse
                    foreach (string key in ResultDict.Keys)
                    {
                        Console.WriteLine("{0}\t{1} msec", key, ResultDict[key].Last());
                    }
                }

                Console.WriteLine("Dumping results");

                using (StreamWriter sw = new StreamWriter("testresult_" + DateTime.Now.ToShortDateString() + ".txt"))
                {
                    foreach (string key in ResultDict.Keys)
                    {
                        foreach (int val in ResultDict[key])
                        {
                            sw.WriteLine("{0}\t{1}", key, val);
                            Console.WriteLine("{0}\t{1} msec", key, val);
                        }
                    }
                }

                Console.WriteLine("All tests ran without problem");
            }
            catch (Exception x)
            {
                Console.WriteLine("Error running test: {0}", x.Message);
                throw;
            }

            finally
            {
                if (connection != null)
                {
                    connection.Close();
                }

                if (basedDataConnection != null)
                {
                    basedDataConnection.Close();
                }
            }
        }
    }
}
