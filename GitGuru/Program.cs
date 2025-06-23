using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace GitGuru
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                ShowHelp();
                return;
            }

            var revisions = new List<string>();
            bool dryRun = false;

            // Parse command line arguments
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "--file":
                    case "-f":
                        if (i + 1 < args.Length)
                        {
                            revisions = FileHelper.LoadRevisionsFromFile(args[i + 1]);
                            i++; // Skip the file path argument
                        }
                        else
                        {
                            Console.WriteLine("❌ --file option requires a file path");
                            Environment.Exit(1);
                        }
                        break;
                    case "--dry-run":
                        dryRun = true;
                        break;
                    case "--help":
                    case "-h":
                        ShowHelp();
                        return;
                    default:
                        if (!args[i].StartsWith("--"))
                        {
                            revisions.Add(args[i]);
                        }
                        break;
                }
            }

            if (revisions.Count == 0)
            {
                Console.WriteLine("❌ No revisions provided!");
                ShowHelp();
                return;
            }

            if (dryRun)
            {
                Console.WriteLine("🔍 DRY RUN MODE - Commands that would be executed:");
                Console.WriteLine("git reset --hard");
                Console.WriteLine("git pull");
                foreach (var rev in revisions)
                {
                    Console.WriteLine($"git cherry-pick {rev}");
                    Console.WriteLine("git push");
                }
                return;
            }

            // Run the tool
            var tool = new GitCherryPickTool();
            await tool.RunAsync(revisions);
        }

        static void ShowHelp()
        {
            Console.WriteLine(@"
                                Git Cherry-Pick Automation Tool

                                Usage:
                                  GitCherryPickTool.exe [options] [revisions...]
                                  GitCherryPickTool.exe --file <file_path>

                                Options:
                                  --file, -f <path>    Load revisions from file (JSON or text)
                                  --dry-run           Show what would be done without executing
                                  --help, -h          Show this help message

                                Examples:
                                  GitCherryPickTool.exe abc123 def456 ghi789
                                  GitCherryPickTool.exe --file revisions.txt
                                  GitCherryPickTool.exe --file revisions.json
                                  GitCherryPickTool.exe --dry-run abc123 def456

                                File formats:
                                  Text file (one revision per line):
                                    abc123
                                    def456
                                    ghi789

                                  JSON file:
                                    [""abc123"", ""def456"", ""ghi789""]
                                                                            ");
        }
    }
}
