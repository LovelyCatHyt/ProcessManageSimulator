using System;
using System.Drawing;
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

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            try
            {
                if (e.Args.Length>0)
                {
                    cfg = JsonConvert.DeserializeObject<Config>(File.ReadAllText(string.IsNullOrEmpty(e.Args[0]) ? "config.json" : e.Args[0]));
                }
                else
                {
                    cfg = Config.defaultConfig;
                }
                // Console.WriteLine("Config loaded.", Color.LawnGreen);
            }
            catch (FileNotFoundException)
            {
                cfg = Config.defaultConfig;
            }
            File.WriteAllText("config.json", JsonConvert.SerializeObject(cfg));
        }
    }
}
