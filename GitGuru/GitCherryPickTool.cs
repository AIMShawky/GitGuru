using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitGuru
{
    public class GitCherryPickTool
    {
        private readonly List<string> _errorRevisions = new();
        private readonly List<string> _successfulRevisions = new();

        public async Task<(bool Success, string Output)> RunCommandAsync(string command, string arguments = "")
        {
            try
            {
                using var process = new Process();
                process.StartInfo.FileName = command;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;

                process.Start();

                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                var fullOutput = output + error;
                return (process.ExitCode == 0, fullOutput);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<bool> GitResetHardAsync()
        {
            Console.WriteLine("🔄 Running git reset --hard...");
            var (success, output) = await RunCommandAsync("git", "reset --hard");

            if (success)
            {
                Console.WriteLine("✅ Git reset completed successfully");
                return true;
            }
            else
            {
                Console.WriteLine($"❌ Git reset failed: {output}");
                return false;
            }
        }

        public async Task<bool> GitPullAsync()
        {
            Console.WriteLine("🔄 Running git pull...");
            var (success, output) = await RunCommandAsync("git", "pull");

            if (success)
            {
                Console.WriteLine("✅ Git pull completed successfully");
                return true;
            }
            else
            {
                Console.WriteLine($"❌ Git pull failed: {output}");
                return false;
            }
        }

        public async Task<bool> CherryPickRevisionAsync(string revision)
        {
            Console.WriteLine($"🍒 Cherry-picking revision: {revision}");
            var (success, output) = await RunCommandAsync("git", $"cherry-pick {revision}");

            if (success)
            {
                Console.WriteLine($"✅ Successfully cherry-picked: {revision}");
                return true;
            }
            else
            {
                Console.WriteLine($"❌ Cherry-pick failed for: {revision}");
                Console.WriteLine($"Error: {output}");

                // Abort the cherry-pick to clean up
                Console.WriteLine("🔄 Aborting cherry-pick...");
                var (abortSuccess, abortOutput) = await RunCommandAsync("git", "cherry-pick --abort");
                if (!abortSuccess)
                {
                    Console.WriteLine($"⚠️ Warning: Failed to abort cherry-pick: {abortOutput}");
                }

                return false;
            }
        }

        public async Task<bool> GitPushAsync()
        {
            Console.WriteLine("🚀 Pushing changes...");
            var (success, output) = await RunCommandAsync("git", "push");

            if (success)
            {
                Console.WriteLine("✅ Push completed successfully");
                return true;
            }
            else
            {
                Console.WriteLine($"❌ Push failed: {output}");
                return false;
            }
        }

        public async Task<bool> CheckGitRepositoryAsync()
        {
            var (success, _) = await RunCommandAsync("git", "rev-parse --git-dir");
            return success;
        }

        public async Task ProcessRevisionsAsync(List<string> revisions)
        {
            Console.WriteLine($"\n📋 Processing {revisions.Count} revisions...");

            for (int i = 0; i < revisions.Count; i++)
            {
                var revision = revisions[i];
                Console.WriteLine($"\n--- Processing revision {i + 1}/{revisions.Count} ---");

                if (await CherryPickRevisionAsync(revision))
                {
                    // Cherry-pick successful, now push
                    if (await GitPushAsync())
                    {
                        _successfulRevisions.Add(revision);
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ Cherry-pick succeeded but push failed for: {revision}");
                        _errorRevisions.Add(revision);
                    }
                }
                else
                {
                    // Cherry-pick failed, add to error list
                    _errorRevisions.Add(revision);
                }

                Console.WriteLine(new string('-', 50));
            }
        }

        public void PrintSummary()
        {
            Console.WriteLine("\n" + new string('=', 60));
            Console.WriteLine("📊 CHERRY-PICK SUMMARY");
            Console.WriteLine(new string('=', 60));

            if (_errorRevisions.Count == 0)
            {
                Console.WriteLine("🎉 SUCCESS: All revisions were cherry-picked successfully!");
                Console.WriteLine($"✅ Total successful revisions: {_successfulRevisions.Count}");
            }
            else
            {
                Console.WriteLine("⚠️ COMPLETED WITH ERRORS");
                Console.WriteLine($"✅ Successful revisions: {_successfulRevisions.Count}");
                Console.WriteLine($"❌ Failed revisions: {_errorRevisions.Count}");

                Console.WriteLine("\n🔴 Failed Revisions:");
                foreach (var revision in _errorRevisions)
                {
                    Console.WriteLine($"  - {revision}");
                }
            }

            if (_successfulRevisions.Count > 0)
            {
                Console.WriteLine("\n🟢 Successful Revisions:");
                foreach (var revision in _successfulRevisions)
                {
                    Console.WriteLine($"  - {revision}");
                }
            }
        }

        public async Task RunAsync(List<string> revisions)
        {
            Console.WriteLine("🚀 Starting Git Cherry-Pick Automation Tool");
            Console.WriteLine(new string('=', 60));

            // Check if we're in a git repository
            if (!await CheckGitRepositoryAsync())
            {
                Console.WriteLine("❌ Error: Not in a git repository!");
                Environment.Exit(1);
            }

            // Step 1: Git reset --hard
            if (!await GitResetHardAsync())
            {
                Console.WriteLine("❌ Failed to reset repository. Exiting.");
                Environment.Exit(1);
            }

            // Step 2: Git pull
            if (!await GitPullAsync())
            {
                Console.WriteLine("❌ Failed to pull latest changes. Exiting.");
                Environment.Exit(1);
            }

            // Step 3: Process all revisions
            await ProcessRevisionsAsync(revisions);

            // Step 4: Print summary
            PrintSummary();
        }
    }
}
