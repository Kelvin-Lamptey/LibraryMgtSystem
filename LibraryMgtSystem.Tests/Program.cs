using System;

namespace LibraryMgtSystem.Tests
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Running LibraryMgtSystem Tests...");
            Console.WriteLine();

            int failed = LibraryTests.RunAll();
            Environment.ExitCode = failed > 0 ? 1 : 0;

            if (!Console.IsInputRedirected)
            {
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey(true);
            }
        }
    }
}
