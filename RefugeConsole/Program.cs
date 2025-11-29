using Microsoft.Extensions.Logging;
using System;

namespace RefugeConsole
{
    internal class Program
    {
        private static readonly ILogger MyLogger = LoggerFactory.Create( builder => builder.AddConsole()).CreateLogger<Program>();
        public static void Main(string[] args)
        {
            Console.WriteLine("\tBienvenue au refuge\t\n================================");
        }
    }
}
