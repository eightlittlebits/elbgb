using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using elbgb_core;

namespace elbgb_test
{
    class Program
    {
        static XmlSerializer TestSerializer = new XmlSerializer(typeof(List<Test>), new XmlRootAttribute("Tests"));

        static void Main(string[] args)
        {
            string manifest = string.Empty;
            TestStatus showResults = TestStatus.None;
            int frameLimit = 60 * 60;
            
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--show" when i < args.Length - 1:
                        Enum.TryParse(args[++i], true, out showResults);
                        break;

                    case "--timeout" when i < args.Length - 1:
                        int secondsToRun = int.Parse(args[++i]);
                        frameLimit = 60 * secondsToRun;
                        break;

                    default:
                        manifest = args[i];
                        break;
                }
            }

            string manifestPath = Path.GetFullPath(manifest);
            string testPath = Path.GetDirectoryName(manifestPath);

            if (!File.Exists(manifestPath))
            {
                Console.WriteLine($"Manifest file {manifest} not found.");
                return;
            }

            List<Test> tests = LoadTestManifest(manifestPath);

            RunTests(testPath, tests, showResults, frameLimit);

            SaveTestManifest(manifestPath, tests);

            if (Debugger.IsAttached)
                Console.ReadLine();
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

        private static void RunTests(string testPath, List<Test> tests, TestStatus showResults, int frameLimit)
        {
            var startTime = DateTime.Now;
            Console.WriteLine($"Testing Started: {startTime}\n");

            object progressLock = new object();
            int testCount = tests.Count;
            int completedTests = 0;

            Parallel.ForEach(tests, test =>
            {
                try
                {
                    string hash = ExecuteRom(testPath, test, frameLimit);

                    if (hash == test.Hash)
                    {
                        test.Result = test.Status;
                    }
                    else if (test.Status == TestStatus.Passing)
                    {
                        test.Result = TestStatus.Failing;
                    }
                    else
                    {
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

            TimeSpan testDuration = (endTime - startTime);

            Console.WriteLine($"{tests.Count} tests completed in {testDuration.TotalSeconds}\n");

            List<Test> results = tests.Where(x => x.Status != x.Result || showResults.HasFlag(x.Result)).ToList();

            if (results.Count(x => x.Result == TestStatus.Inconclusive) > 0)
                PrintResults(results, TestStatus.Inconclusive, ConsoleColor.Gray);

            if (results.Count(x => x.Result == TestStatus.Failing) > 0)
                PrintResults(results, TestStatus.Failing, ConsoleColor.Red);

            if (results.Count(x => x.Result == TestStatus.Passing) > 0)
                PrintResults(results, TestStatus.Passing, ConsoleColor.Green);

            PrintResultCounts(tests);

            GenerateResultFile(Path.Combine(testPath, "results.html"), tests, testDuration);
        }

        private static string ExecuteRom(string testPathRoot, Test test, int frameLimit)
        {
            string hash = string.Empty;

            using (var framesink = new TestVideoFrameSink())
            {
                var gameboy = new GameBoy(framesink, new NullInputSource());

                string testPath = Path.Combine(testPathRoot, test.Name);

                gameboy.LoadRom(File.ReadAllBytes(testPath));

                int framesRun = 0;
                
                while (framesRun++ < frameLimit && hash != test.Hash)
                {
                    gameboy.RunFrame();
                    hash = framesink.HashFrame();
                }

                using (var stream = new FileStream(Path.ChangeExtension(testPath, "png"), FileMode.Create))
                {
                    framesink.SaveFrameAsPng(stream);
                }
            }

            return hash;
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

        private static void PrintResultCounts(IEnumerable<Test> tests)
        {
            int initialPassing = tests.Count(x => x.Status == TestStatus.Passing);
            int initialFailing = tests.Count(x => x.Status == TestStatus.Failing);
            int initialInconclusive = tests.Count(x => x.Status == TestStatus.Inconclusive);

            int passingCount = tests.Count(x => x.Result == TestStatus.Passing);
            int failingCount = tests.Count(x => x.Result == TestStatus.Failing);
            int inconclusiveCount = tests.Count(x => x.Result == TestStatus.Inconclusive);

            int diffPassing = passingCount - initialPassing;
            int diffFailing = failingCount - initialFailing;
            int diffInconslusive = inconclusiveCount - initialInconclusive;

            Console.WriteLine("Results:");
            Console.WriteLine($"\tInconclusive:\t{inconclusiveCount} ({diffInconslusive:+#;-#;0})");
            Console.WriteLine($"\tFailing:\t{failingCount} ({diffFailing:+#;-#;0})");
            Console.WriteLine($"\tPassing:\t{passingCount} ({diffPassing:+#;-#;0})");
        }

        private static void GenerateResultFile(string filename, List<Test> tests, TimeSpan duration)
        {
            ResultTable resultTableGenerator = new ResultTable(tests, duration);
            string pageContent = resultTableGenerator.TransformText();
            File.WriteAllText(filename, pageContent);
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
