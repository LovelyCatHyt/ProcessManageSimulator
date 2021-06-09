using System.IO;
using Newtonsoft.Json;
using System.Windows;

namespace ProcessManageWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Config cfg;

        /// <summary>
        /// 保存配置文件
        /// </summary>
        public static void SaveConfig()
        {
            File.WriteAllText(string.IsNullOrEmpty(cfg.cfgPath) ? Config.defaultConfig.cfgPath : cfg.cfgPath, JsonConvert.SerializeObject(cfg));
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            try
            {
                if (e.Args.Length > 0)
                {
                    cfg = JsonConvert.DeserializeObject<Config>(File.ReadAllText(e.Args[0]));
                }
                else
                {
                    cfg = JsonConvert.DeserializeObject<Config>(File.ReadAllText(Config.defaultConfig.cfgPath));
                }
                // Console.WriteLine("Config loaded.", Color.LawnGreen);
            }
            catch (FileNotFoundException)
            {
                cfg = Config.defaultConfig;
            }

            SaveConfig();
        }
    }
}
