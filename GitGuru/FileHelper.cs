using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GitGuru
{
    public static class FileHelper
    {
        public static List<string> LoadRevisionsFromFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"❌ File not found: {filePath}");
                    Environment.Exit(1);
                }

                var content = File.ReadAllText(filePath).Trim();

                // Try to parse as JSON first
                try
                {
                    var jsonRevisions = JsonSerializer.Deserialize<List<string>>(content);
                    if (jsonRevisions != null)
                    {
                        return jsonRevisions;
                    }
                }
                catch (JsonException)
                {
                    // If not JSON, treat as line-separated text
                    var revisions = content.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                                          .Select(line => line.Trim())
                                          .Where(line => !string.IsNullOrEmpty(line))
                                          .ToList();
                    return revisions;
                }

                Console.WriteLine("❌ Could not parse file content");
                Environment.Exit(1);
                return new List<string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error reading file: {ex.Message}");
                Environment.Exit(1);
                return new List<string>();
            }
        }
    }
}
