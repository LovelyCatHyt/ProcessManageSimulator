namespace ProcessManageWPF
{
    public class Config
    {
        public int cpuCount { get; set; }
        public int memorySize { get; set; }
        public static Config defaultConfig = new Config() { cpuCount = 4, memorySize = 1024};
    }
}
