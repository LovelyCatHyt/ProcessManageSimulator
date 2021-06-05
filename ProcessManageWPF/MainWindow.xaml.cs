using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ProcessManageCore;
using ProcessManageCore.Entity;
using ProcessManageCore.Singleton;
using Timer = System.Timers.Timer;

namespace ProcessManageWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly OS os;

        public MainWindow()
        {
            InitializeComponent();
            os = new OS(App.cfg.cpuCount, App.cfg.memorySize);
            Timer testTimer = new Timer(500);
            testTimer.Elapsed += (o, e) => { os.Update(10); OSInfoUpdate(os); };
            testTimer.Start();
        }

        private void OSInfoUpdate(OS obj)
        {
            Dispatcher.Invoke(() =>
            {
                foreach (var item in processList.Items)
                {
                    ((ProcessUI) item).UpdateProperties();
                }
            }, DispatcherPriority.Normal);
        }

        private void Btn_AddProcess(object sender, RoutedEventArgs e)
        {
            Process p = ProcessFactory.CreateIndependentProcess(ProcessType.System, "Test", 100, 1);
            os.AddNewProcess(p);
            ProcessUI newProcessUi = new ProcessUI(p);
            processList.Items.Add(newProcessUi);
        }

    }
}
