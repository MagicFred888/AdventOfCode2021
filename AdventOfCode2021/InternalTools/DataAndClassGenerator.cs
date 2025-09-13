using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace AdventOfCode2021.InternalTools
{
    public static partial class DataAndClassGenerator
    {
        public static bool CreateLevel(int year, int dayToCreate)
        {
            // Ask user to confirm
            Console.WriteLine("Automatic level creation and puzzle extraction");
            Console.WriteLine("^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^");
            Console.WriteLine("");
            Console.Write($"Create level for Day {dayToCreate:0} ? [y/n]: ");
            if (!(Console.ReadLine() ?? "").Equals("y", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            // Create HttpClient with a handler that supports cookies
            HttpClient client;
            bool extractPuzzle = false;
            string sessionCookieFilePath = @"..\..\..\..\session.txt";
            if (File.Exists(sessionCookieFilePath))
            {
                // Client with session to be able to extract our puzzle
                CookieContainer cookieContainer = new();
                cookieContainer.Add(new Cookie("session", File.ReadAllText(sessionCookieFilePath))
                {
                    Domain = ".adventofcode.com"
                });
                var handler = new HttpClientHandler { CookieContainer = cookieContainer };
                client = new(handler);
                extractPuzzle = true;
            }
            else
            {
                // Simple client
                client = new HttpClient();
            }

            // Connect to web page and extract HTML code
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Extracting day title...");
            string url = $"https://adventofcode.com/{year}/day/{dayToCreate}";
            HttpResponseMessage response = client.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();
            string htmlContent = response.Content.ReadAsStringAsync().Result;

            // Get title
            string title;
            Console.WriteLine("");
            Match titleMatch = TitleExtractionRegex().Match(htmlContent);
            if (titleMatch.Success)
            {
                // Automatic
                title = titleMatch.Groups["title"].Value;
                Console.WriteLine($"Title has been found: {title}");
            }
            else
            {
                // Manual
                Console.WriteLine("Unable to find day title... :-//");
                Console.WriteLine("");
                Console.Write($"Please input title manually: ");
                title = Console.ReadLine() ?? "";
                if (string.IsNullOrEmpty(title))
                {
                    return false;
                }
            }

            // Create data folder with challenge and sample if not exists
            string newDataFolder = @$"..\..\..\Data\Day{dayToCreate:00}\";
            if (!Directory.Exists(newDataFolder))
            {
                // Puzzle input
                string puzzleInput = string.Empty;
                if (extractPuzzle)
                {
                    Console.WriteLine("");
                    Console.WriteLine("Extracting puzzle data...");
                    url = $"https://adventofcode.com/{year}/day/{dayToCreate}/input";
                    response = client.GetAsync(url).Result;
                    response.EnsureSuccessStatusCode();
                    puzzleInput = response.Content.ReadAsStringAsync().Result.TrimEnd('\n');
                    Console.WriteLine("");
                    int nbrOfLines = puzzleInput.Split('\n').Length;
                    Console.WriteLine($"Puzzle data have been extracted: {nbrOfLines} line{(nbrOfLines > 1 ? "s" : "")}");
                }

                // Directory and challenge file
                Directory.CreateDirectory(newDataFolder);
                File.WriteAllText(Path.Combine(newDataFolder, "Challenge.txt"), puzzleInput, Encoding.UTF8);

                // Sample files
                Enumerable.Range(1, 9).ToList()
                    .ConvertAll(i => Path.Combine(newDataFolder, $"Sample{i}_X_X.txt"))
                    .ForEach(p => { if (!File.Exists(p)) { File.Create(p); } });

                // Dispose
                client.Dispose();
            }

            // Create new solver Day{dayToSolve:00}
            string newSolverFile = @$"..\..\..\Solver\Day{dayToCreate:00}.cs";

            // Read and fix
            List<string> solverFile = GetResourceContent("DayXX.txt").Split("\n").ToList().ConvertAll(l => l.Trim('\r'));
            for (int i = 0; i < solverFile.Count; i++)
            {
                if (solverFile[i].Contains("DayXX"))
                {
                    solverFile[i] = solverFile[i].Replace("DayXX", $"Day{dayToCreate:00}");
                }
                if (solverFile[i].Contains("\"XXX\""))
                {
                    solverFile[i] = solverFile[i].Replace("\"XXX\"", $"\"{title}\"");
                }
            }

            // Write
            File.WriteAllLines(newSolverFile, solverFile, Encoding.UTF8);

            // Inform user
            Console.WriteLine("");
            Console.WriteLine("New solver created. Press any key to exit.");
            Console.ReadKey();

            // Done
            return true;
        }

        public static string GetResourceContent(string resourceName)
        {
            // Read resource content
            Assembly assembly = Assembly.GetExecutingAssembly();
            List<string> targetResources = new(assembly.GetManifestResourceNames());
            targetResources = targetResources.FindAll(c => c.Contains(resourceName, StringComparison.CurrentCultureIgnoreCase));
            if (targetResources.Count != 1) return string.Empty;
            using Stream stream = assembly.GetManifestResourceStream(targetResources[0])!;
            using StreamReader reader = new(stream);
            return reader.ReadToEnd();
        }

        [GeneratedRegex(@"<article class=""day-desc""><h2>--- Day (?<day>\d+): (?<title>.+?) ---</h2>")]
        private static partial Regex TitleExtractionRegex();
    }
}