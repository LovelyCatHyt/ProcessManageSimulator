﻿using System;
using ProcessManageCore.Entity;
using ProcessManageCore.Singleton;

namespace ProcessManageCore
{
    class Program
    {
        static void Main(string[] args)
        {
            OS os = new OS(4, 1024);
            bool keepAlive = true;
            var p = ProcessFactory.CreateProcess(ProcessType.System, "system", Int32.MaxValue, true, new int[0], new int[0]);
            os.AddProcess(p);
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
                        os.AddProcess(ProcessFactory.CreateProcess(ProcessType.Kernel, name, time, true, new int[0], new int[0]));
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
