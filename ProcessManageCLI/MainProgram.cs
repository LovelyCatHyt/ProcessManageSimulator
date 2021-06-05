using System.Drawing;
using Console = Colorful.Console;
using System.IO;
using System.Threading;
using ProcessManageCore.Singleton;
using Newtonsoft.Json;

namespace ProcessManageCLI
{
    public class MainProgram
    {
        static void Main(string[] args)
        {
            Config cfg;
            try
            {
                cfg = JsonConvert.DeserializeObject<Config>(File.ReadAllText(string.IsNullOrEmpty(args[0]) ? "config.json" : args[0]));
                Console.WriteLine("Config loaded.", Color.LawnGreen);
            }
            catch (FileNotFoundException)
            {
                cfg = new Config { cpuCount = 4, memorySize = 1024 };
                Console.WriteLine("Config file not found! use default.", Color.Coral);
                File.WriteAllText("config.json", JsonConvert.SerializeObject(cfg));
            }
            OS os = new OS(cfg.cpuCount, cfg.memorySize);
            OSRunner runner = new OSRunner(os);
            Thread simulateThread = new Thread(() => runner.StartRunning());
            simulateThread.IsBackground = true;
            simulateThread.Start();
            Console.ReadLine();
        }
    }
}
