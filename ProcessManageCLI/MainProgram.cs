using System;
using System.IO;
using ProcessManageCore.Singleton;
using Newtonsoft.Json;

namespace ProcessManageCLI
{
    public class MainProgram
    {
        static void Main(string[] args)
        {
            Config cfg = JsonConvert.DeserializeObject<Config>(File.ReadAllText(args[0]));
            OS os = new OS(cfg.cpuCount, cfg.memorySize);
        }
    }
}
