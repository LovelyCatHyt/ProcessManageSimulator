using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        private NewProcessDialog newProcessWindow;
        public MainWindow()
        {
            InitializeComponent();

            Closed += (sender, args) => newProcessWindow?.Close();
            os = new OS(App.cfg.cpuCount, App.cfg.memorySize);

            for (var index = 0; index < os.cpuList.Length; index++)
            {
                var cpu = os.cpuList[index];
                cpuPanel.Children.Add(new CPUUI(cpu, index));
            }
            simulateTimer = new Timer(250);
            simulateTimer.Elapsed += Update;
            simulateTimer.Enabled = false;
            OnTextFPSChanged(this, null);
            OnTextStepPerFrameChanged(this, null);
            OSInfoUpdate();
        }

        private void Update(object o, ElapsedEventArgs e)
        {
            os.Update(simulateStep); 
            OSInfoUpdate();
        }

        private void OSInfoUpdate()
        {
            Dispatcher.Invoke(() =>
            {
                // 更新CPU进度条
                foreach (UIElement cpuPanelChild in cpuPanel.Children)
                {
                    ((CPUUI)cpuPanelChild).Update();
                }

                // 已运行时间片数目
                labelTimeCount.Content = os.ElapsedPeriod;
                
                // 移除已经终止的进程, 同时更新进程访问器
                for (var i = 0; i < processList.Items.Count; i++)
                {
                    var processVisitor = (ProcessVisitor) processList.Items[i];
                    if (processVisitor.State == ProcessState.Killed)
                    {
                        processList.Items.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        processVisitor.NotifyPropertyChange();
                    }
                }

                // 缓存元素列表为易于 Linq 的形式
                var visitors = processList.Items.Cast<ProcessVisitor>() as ProcessVisitor[] ?? processList.Items.Cast<ProcessVisitor>().ToArray();

                // 就绪队列
                ResetProcessList(readyList, visitors.Where(x=>os.ReadyList.Contains(x.process)));
                // 挂起队列
                ResetProcessList(hangupList, visitors.Where(x => os.HangupList.Contains(x.process)));
                // 后备队列
                ResetProcessList(waitForMemoryList, visitors.Where(x => os.WaitForMemoryList.Contains(x.process)));
            }, DispatcherPriority.Normal);
        }

        private void ResetProcessList(ListView listView, IEnumerable<ProcessVisitor> list)
        {
            listView.Items.Clear();
            foreach (var process in list)
            {
                // ProcessVisitor v = new ProcessVisitor(process);
                listView.Items.Add(process);
            }
        }

        private void AddProcess(Process p)
        {
            os.AddNewProcess(p);
            processList.Items.Add(new ProcessVisitor(p));
            OSInfoUpdate();
        }

        private void Btn_AddProcess(object sender, RoutedEventArgs e)
        {
            if (newProcessWindow == null)
            {
                newProcessWindow = new NewProcessDialog(this);
                newProcessWindow.addNewProcess += AddProcess;
                newProcessWindow.Show();
                newProcessWindow.Closed += (o, args) => newProcessWindow = null;
            }
            else
            {
                newProcessWindow.Activate();
                newProcessWindow.Topmost = true;
                // newProcessWindow.Activate();
            }
        }

        private void ProcessList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (processList.SelectedItem != null)
            //{
            //    var processVisitor = (ProcessVisitor)processList.SelectedItem;
            //    processFullInfo.ApplyData(processVisitor.process);
            //    var focus = (ProcessVisitor)FindResource("focusProcess");
            //    focus.SetProcess(processVisitor.process);
            //}
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
