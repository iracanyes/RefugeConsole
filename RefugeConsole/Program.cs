using Microsoft.Extensions.Logging;
using RefugeConsole.ClassesMetiers.Config;
using RefugeConsole.CouchePresentation.View;
using System;

namespace RefugeConsole
{
    internal class Program
    {
        private static readonly ILogger MyLogger = LoggerFactory.Create( builder => builder.AddConsole()).CreateLogger<Program>();
        public static void Main(string[] args)
        {
            Console.WriteLine("\tBienvenue au refuge\t\n================================");
            
            // Load environment variables file
            LoadEnvVars();

            MenuView.Display();

        }

        private static void LoadEnvVars()
        {
            try
            {
                // Load environment variables file 
                var root = Directory.GetCurrentDirectory();
                var dotEnvFile = Path.Combine(root, ".env");
                DotEnv.Load(dotEnvFile);
            }
            catch (Exception ex) {
                MyLogger.LogError("Error while loading environmnent file .env. Reason : {0}", ex.Message);
            }
        }
    }
}
