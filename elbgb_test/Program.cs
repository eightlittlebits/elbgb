﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Xml.Serialization;
using elbgb_core;
using System.Threading.Tasks;

namespace elbgb_test
{
    class Program
    {
        const string ManifestFilename = "tests.xml";

        static XmlSerializer TestSerializer = new XmlSerializer(typeof(List<Test>), new XmlRootAttribute("Tests"));

        static string GetTestFolder(string testFolder)
        {
            var testPathRoot = Path.Combine(Environment.CurrentDirectory, testFolder);

            if (!testPathRoot.EndsWith("\\"))
            {
                testPathRoot += "\\";
            }

            return testPathRoot;
        }

        static void Main(string[] args)
        {
            string manifest = string.Empty;
            TestStatus showResults = TestStatus.None;


            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--manifest" when i < args.Length - 1:
                        manifest = args[++i];
                        break;

                    case "--show" when i < args.Length - 1:
                        Enum.TryParse(args[++i], true, out showResults);
                        break;

                    //case "--generate":
                    //    GenerateTestXml(manifestPath, "*.gb");
                    //    break;
                }
            }

            string manifestPath = Path.GetFullPath(manifest);
            string testPath = Path.GetDirectoryName(manifestPath);

            if (!File.Exists(manifestPath))
            {
                Console.WriteLine("Manifest file tests.xml not found, please run elbgb_test --generate.");
                return;
            }

            List<Test> tests;
            tests = LoadTestManifest(manifestPath);

            RunTests(testPath, tests, showResults);

            //SaveTestManifest(manifestPath, tests);

            if (Debugger.IsAttached)
                Console.ReadLine();
        }

        private static void RunTests(string testPath, List<Test> tests, TestStatus showResults)
        {
            int initialPassing = tests.Count(x => x.Status == TestStatus.Passing);
            int initialFailing = tests.Count(x => x.Status == TestStatus.Failing);
            int initialInconclusive = tests.Count(x => x.Status == TestStatus.Inconclusive);

            var startTime = DateTime.Now;
            Console.WriteLine($"Testing Started: {startTime}\n");

            object progressLock = new object();
            int testCount = tests.Count;
            int completedTests = 0;

            Parallel.ForEach(tests, test =>
            {
                try
                {
                    // if we have a passing test then run framecount, otherwise run (roughly) a minute
                    const int MinuteFrameCount = 60 * 60;

                    if (test.Status != TestStatus.Passing)
                    {
                        test.FrameCount = MinuteFrameCount;
                    }

                    (int frameCount, string hash) = ExecuteRom(testPath, test);

                    if (hash == test.Hash)
                    {
                        test.Result = test.Status;
                    }
                    else if (hash != test.Hash && test.Status == TestStatus.Passing)
                    {
                        test.Result = TestStatus.Failing;
                    }
                    else
                    {
                        test.FrameCount = frameCount;
                        test.Hash = hash;
                        test.Result = TestStatus.Inconclusive;
                    }
                }
                catch (Exception)
                {
                    test.Result = TestStatus.Failing;
                }
                finally
                {
                    lock (progressLock)
                    {
                        UpdateProgress(testCount, ++completedTests);
                    }
                }
            });

            var endTime = DateTime.Now;
            Console.WriteLine($"\n\nTesting Finished: {endTime}\n");

            Console.WriteLine($"{tests.Count} tests completed in {(endTime - startTime).TotalSeconds}\n");

            int resultPassing = tests.Count(x => x.Result == TestStatus.Passing);
            int resultFailing = tests.Count(x => x.Result == TestStatus.Failing);
            int resultInconclusive = tests.Count(x => x.Result == TestStatus.Inconclusive);

            IEnumerable<Test> results;

            results = tests.Where(x => x.Status != x.Result || showResults.HasFlag(x.Result));

            if (results.Count(x => x.Result == TestStatus.Inconclusive) > 0)
                PrintResults(results, TestStatus.Inconclusive, ConsoleColor.Gray);

            if (results.Count(x => x.Result == TestStatus.Failing) > 0)
                PrintResults(results, TestStatus.Failing, ConsoleColor.Red);

            if (results.Count(x => x.Result == TestStatus.Passing) > 0)
                PrintResults(results, TestStatus.Passing, ConsoleColor.Green);

            int diffPassing = resultPassing - initialPassing;
            int diffFailing = resultFailing - initialFailing;
            int diffInconslusive = resultInconclusive - initialInconclusive;

            Console.WriteLine("Results:");
            Console.WriteLine($"\tInconclusive:\t{resultInconclusive} ({diffInconslusive:+#;-#;0})");
            Console.WriteLine($"\tFailing:\t{resultFailing} ({diffFailing:+#;-#;0})");
            Console.WriteLine($"\tPassing:\t{resultPassing} ({diffPassing:+#;-#;0})");
        }

        private static void UpdateProgress(int testCount, int completedTests)
        {
            const int progressStars = 40;

            var slope = (double)progressStars / testCount;
            var output = slope * completedTests;

            var stars = (int)Math.Floor(output);
            var space = progressStars - stars;

            Console.Write("\r[");

            Console.Write(new string('*', stars));
            Console.Write(new string(' ', space));

            Console.Write($"] ({completedTests}/{testCount})");
        }

        private static void PrintResults(IEnumerable<Test> tests, TestStatus status, ConsoleColor colour)
        {
            ConsoleColor currentColour = Console.ForegroundColor;

            Console.ForegroundColor = colour;

            Console.WriteLine($"{status}:");

            foreach (var test in tests.Where(x => x.Result == status))
            {
                Console.WriteLine("\t" + test.Name);
            }

            Console.ForegroundColor = currentColour;
            Console.WriteLine();
        }

        private static List<Test> LoadTestManifest(string manifestPath)
        {
            List<Test> tests;
            using (var manifest = new StreamReader(manifestPath))
            {
                tests = (List<Test>)TestSerializer.Deserialize(manifest);
            }

            return tests;
        }
        private static void SaveTestManifest(string manifestPath, List<Test> tests)
        {
            using (var manifest = new StreamWriter(manifestPath))
            {
                TestSerializer.Serialize(manifest, tests);
            }
        }

        //private static void GenerateTestXml(string manifestPath, string searchPattern)
        //{
        //    List<Test> tests = new List<Test>();

        //    string testPath = Path.GetDirectoryName(manifestPath) + "\\";

        //    foreach (var filename in Directory.EnumerateFiles(testPath, searchPattern, SearchOption.AllDirectories))
        //    {
        //        // remove the base from the file path
        //        var relativePath = filename.Remove(0, testPath.Length);

        //        tests.Add(new Test { Name = relativePath, FrameCount = 0, Hash = string.Empty, Status = TestStatus.Inconclusive });
        //    }

        //    SaveTestManifest(manifestPath, tests);
        //}

        private static (int frameCount, string hash) ExecuteRom(string testPathRoot, Test test)
        {
            Dictionary<string, int> frameHash = new Dictionary<string, int>();

            var framesink = new TestVideoFrameSink();
            var gameboy = new GameBoy(framesink, new NullInputSource());

            string testPath = Path.Combine(testPathRoot, test.Name);

            gameboy.LoadRom(File.ReadAllBytes(testPath));

            int framesRun = 0;
            string hash;

            while (framesRun++ < test.FrameCount)
            {
                gameboy.RunFrame();

                hash = framesink.HashFrame();
                if (!frameHash.ContainsKey(hash))
                {
                    frameHash.Add(hash, framesRun);
                }
            }

            using (var stream = new FileStream(Path.ChangeExtension(testPath, "png"), FileMode.Create))
            {
                framesink.SaveFrame(stream);
            }

            hash = framesink.HashFrame();
            return (frameHash[hash], hash);
        }
    }

    class NullInputSource : IInputSource
    {
        public GBCoreInput PollInput()
        {
            return default(GBCoreInput);
        }
    }
}
