using System;
using System.Diagnostics.CodeAnalysis;
using ProcessManageCore.Entity;
using ProcessManageCore.Singleton;

namespace ProcessManageCore
{
    class Program
    {
        [SuppressMessage("ReSharper.DPA", "DPA0001: Memory allocation issues")]
        static void Main(string[] args)
        {
            OS os = new OS(4, 1024);
            bool keepAlive = true;
            var p = ProcessFactory.CreateProcess(ProcessType.System, "system", Int32.MaxValue, 512, true, new int[0], new int[0]);
            os.AddNewProcess(p);
            Console.WriteLine(os);
            while (keepAlive)
            {
                switch (Console.ReadKey(true).Key)
                {
                    case ConsoleKey.Enter:
                        os.Update();
                        Console.WriteLine(os);
                        break;
                    case ConsoleKey.Add:
                        Console.Write("\rprocess name: ");
                        var name = Console.ReadLine();
                        Console.Write("time: ");
                        var time = int.Parse(Console.ReadLine());
                        Console.Write("memory: ");
                        var memory = int.Parse(Console.ReadLine());

                        os.AddNewProcess(ProcessFactory.CreateProcess(ProcessType.Kernel, name, time, memory, true, new int[0], new int[0]));
                        Console.WriteLine(os);
                        break;
                    case ConsoleKey.S:
                        Console.Write("Skip counts: ");
                        os.Update(int.Parse(Console.ReadLine()));
                        Console.WriteLine(os);
                        break;
                    case ConsoleKey.Q:
                        keepAlive = false;
                        break;
                }
            }
            Console.Write("--------------\nBye~\n--------------");
        }
    }
}
