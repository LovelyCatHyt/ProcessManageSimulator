using System;
using System.Globalization;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
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
        private Timer simulateTimer;
        private int simulateStep = 1;

        public MainWindow()
        {
            InitializeComponent();
            os = new OS(App.cfg.cpuCount, App.cfg.memorySize);

            for (var index = 0; index < os.cpuList.Length; index++)
            {
                var cpu = os.cpuList[index];
                cpuPanel.Children.Add(new CPUUI(cpu, index));
            }
            simulateTimer = new Timer(250);
            simulateTimer.Elapsed += Update;
            simulateTimer.Start();
        }

        private void Update(object o, ElapsedEventArgs e)
        {
            os.Update(simulateStep); OSInfoUpdate(os);
        }

        private void OSInfoUpdate(OS obj)
        {
            Dispatcher.Invoke(() =>
            {
                // 更新进程进度条
                foreach (var item in processList.Items)
                {
                    ((ProcessUI)item).UpdateProperties();
                }
                for (var i = 0; i < processList.Items.Count; i++)
                {
                    var processUi = (ProcessUI)processList.Items[i];
                    if (processUi.Killed)
                    {
                        processList.Items.RemoveAt(i);
                        i--;
                    }
                }

                // 更新进程信息
                if (processList.SelectedItem != null)
                {
                    processFullInfo.ApplyData(((ProcessUI)processList.SelectedItem).bindingProcess);
                }
                else
                {
                    processFullInfo.ClearData();
                }

                // 更新CPU进度条
                foreach (UIElement cpuPanelChild in cpuPanel.Children)
                {
                    ((CPUUI)cpuPanelChild).Update();
                }

                // 已运行时间片数目
                labelTimeCount.Content = os.ElapsedPeriod;
            }, DispatcherPriority.Normal);
        }

        private void AddProcess(Process p)
        {
            os.AddNewProcess(p);
            ProcessUI pUI = new ProcessUI(p);
            processList.Items.Add(pUI);
        }

        private void Btn_AddProcess(object sender, RoutedEventArgs e)
        {
            NewProcessDialog window = new NewProcessDialog();
            window.addNewProcess += AddProcess;
            window.Show();
        }

        private void ProcessList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var processUi = (ProcessUI)e.AddedItems[0];
                processFullInfo.ApplyData(processUi.bindingProcess);
            }
        }

        private void OnTextFPSChanged(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(textFPS.Text, out var fps))
            {
                fps = 1;
            }
            fps = Math.Clamp(fps, 0.0625, 120);
            textFPS.Text = fps.ToString(CultureInfo.CurrentCulture);
            if (simulateTimer != null) simulateTimer.Interval = 1000 / fps;
        }

        private void OnTextStepPerFrameChanged(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(textStepPerFrame.Text, out simulateStep))
            {
                simulateStep = 1;
            }
            simulateStep = Math.Clamp(simulateStep, 1, 1000);
            textStepPerFrame.Text = simulateStep.ToString();
        }

        private void BtnRunStop_OnClick(object sender, RoutedEventArgs e)
        {
            if (simulateTimer.Enabled)
            {
                simulateTimer.Enabled = false;
                btnRunStop.Content = "继续";
            }
            else
            {
                simulateTimer.Enabled = true;
                btnRunStop.Content = "暂停";
            }
        }

        private void BtnStepRun_OnClick(object sender, RoutedEventArgs e)
        {
            Update(null, null);
        }
    }
}
