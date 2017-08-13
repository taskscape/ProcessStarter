using System;
using System.IO;
using System.Threading;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Console application is running");
            File.WriteAllLines(@"C:\Temp\ConsoleApplication.log", new [] {DateTime.UtcNow.ToLongTimeString() + " " + Environment.GetFolderPath(Environment.SpecialFolder.Desktop)});
            Thread.Sleep(10 * 1000);
        }
    }
}
