using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;

namespace TrxToJson
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: TrxToJson <input-trx-file> <output-json-file>");
                return;
            }

            string inputFile = args[0];
            string outputFile = args[1];

            try
            {
                if (!File.Exists(inputFile))
                {
                    Console.WriteLine($"Input file not found: {inputFile}");
                    Environment.Exit(1);
                }

                // Load the TRX file
                XDocument doc = XDocument.Load(inputFile);
                XNamespace ns = doc.Root.GetDefaultNamespace();

                // Extract test results
                var testResults = new
                {
                    total = 0,
                    passed = 0,
                    failed = 0,
                    skipped = 0,
                    time = 0.0,
                    testCases = new List<object>()
                };

                // Get all test results
                var unitTestResults = doc.Descendants(ns + "UnitTestResult").ToList();

                int total = unitTestResults.Count;
                int passed = unitTestResults.Count(r => r.Attribute("outcome")?.Value == "Passed");
                int failed = unitTestResults.Count(r => r.Attribute("outcome")?.Value == "Failed");
                int skipped = unitTestResults.Count(r => r.Attribute("outcome")?.Value == "NotExecuted");

                double totalTime = 0;
                if (unitTestResults.Any())
                {
                    totalTime = unitTestResults
                        .Where(r => r.Attribute("duration") != null)
                        .Sum(r =>
                        {
                            if (TimeSpan.TryParse(r.Attribute("duration")?.Value, out TimeSpan ts))
                                return ts.TotalSeconds;
                            return 0;
                        });
                }

                var testCases = new List<object>();
                foreach (var result in unitTestResults)
                {
                    string testName = result.Attribute("testName")?.Value ?? "Unknown Test";
                    string outcome = result.Attribute("outcome")?.Value ?? "Unknown";

                    double duration = 0;
                    if (TimeSpan.TryParse(result.Attribute("duration")?.Value, out TimeSpan ts))
                        duration = ts.TotalSeconds;

                    string errorMessage = "";
                    string errorStackTrace = "";

                    var output = result.Element(ns + "Output");
                    if (output != null)
                    {
                        var errorInfo = output.Element(ns + "ErrorInfo");
                        if (errorInfo != null)
                        {
                            errorMessage = errorInfo.Element(ns + "Message")?.Value ?? "";
                            errorStackTrace = errorInfo.Element(ns + "StackTrace")?.Value ?? "";
                        }
                    }

                    testCases.Add(new
                    {
                        name = testName,
                        result = outcome,
                        time = duration,
                        errorMessage,
                        errorStackTrace
                    });
                }

                // Create the final result object
                var finalResult = new
                {
                    total,
                    passed,
                    failed,
                    skipped,
                    time = totalTime,
                    testCases
                };

                // Serialize to JSON
                string json = JsonSerializer.Serialize(finalResult, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                // Write to output file
                File.WriteAllText(outputFile, json);
                Console.WriteLine($"Successfully converted {inputFile} to {outputFile}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        }
    }
}