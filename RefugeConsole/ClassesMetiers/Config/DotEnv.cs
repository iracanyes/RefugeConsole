using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace RefugeConsole.ClassesMetiers.Config
{
    internal class DotEnv
    {
        private static readonly ILogger MyLogger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger(nameof(DotEnv));
        private static bool done = false;

        public static void Load(string filePath)
        {
            if (done) return;

            if (!File.Exists(filePath))
                throw new Exception($"Env file not found! Path : {filePath}");

            foreach (var line in File.ReadAllLines(filePath))
            {
                // Ignore comments
                if (line.StartsWith("#"))
                    continue;

                var parts = line.Split('=', StringSplitOptions.RemoveEmptyEntries);


                if (parts.Length < 2)
                    continue;

                if (parts.Length == 2)
                {
                    Environment.SetEnvironmentVariable(parts[0], parts[1]);
                }
                else if (parts.Length > 2)
                {
                    // Remove parenthesis and merge rest of parts to get the actual value
                    Environment.SetEnvironmentVariable(parts[0], string.Join("=", parts.Skip(1)).Trim('"'));
                }
                else
                {
                    throw new NotSupportedException("The environment variable is not supported! Please update ClassesMetiers.Config.DotEnv class");
                }
            }

            done = true;
        }

    }
}
