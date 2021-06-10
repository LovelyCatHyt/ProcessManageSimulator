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
using ProcessManageWPF.Visitor;
using Timer = System.Timers.Timer;

namespace ProcessManageWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public readonly OS os;
        public static MainWindow Instance;

        private readonly Timer _simulateTimer;
        private int _simulateStep = 1;
        private NewProcessDialog _newProcessWindow;
        private Dictionary<Process, ProcessVisitor> _visitorDictionary;

        public MainWindow()
        {
            InitializeComponent();

            Instance = this;
            Closed += (sender, args) => _newProcessWindow?.Close();
            os = new OS(App.cfg.cpuCount, App.cfg.memorySize);

            for (var index = 0; index < os.cpuList.Length; index++)
            {
                var cpu = os.cpuList[index];
                cpuListView.Items.Add(new CPUVisitor{cpu = cpu, index = index});
            }

            _visitorDictionary = new Dictionary<Process, ProcessVisitor>();

            _simulateTimer = new Timer(250);
            _simulateTimer.Elapsed += Update;
            _simulateTimer.Enabled = false;
            OnTextFPSChanged(this, null);
            OnTextStepPerFrameChanged(this, null);
            OSInfoUpdate();
        }

        private void Update(object o, ElapsedEventArgs e)
        {
            os.Update(_simulateStep); 
            OSInfoUpdate();
        }

        /// <summary>
        /// 将界面更新事件加入到 UI 线程中
        /// </summary>
        private void OSInfoUpdate()
        {
            Dispatcher.Invoke(OSInfoUpdateNow, DispatcherPriority.Normal);
        }

        /// <summary>
        /// 操作系统信息更新
        /// </summary>
        private void OSInfoUpdateNow()
        {
            var cachedSelected = processList.SelectedItem;
            // 更新内存占用
            memSegment.MemoryBlocks = os.AllMemoryBlocks.Select(x=>new MemoryBlockVisitor(x));

            // 更新CPU进度条
            foreach (CPUVisitor visitor in cpuListView.Items)
            {
                visitor.NotifyPropertyChange();
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
                    _visitorDictionary.Remove(processVisitor.process);
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
            ResetProcessList(readyList, os.ReadyList);
            // 挂起队列
            ResetProcessList(hangupList, os.HangupList);
            // 后备队列
            ResetProcessList(waitForMemoryList, os.WaitForMemoryList);
            processList.SelectedItem ??= cachedSelected;
        }

        private void ResetProcessList(ListView listView, Process[] list)
        {
            listView.ItemsSource = list.Select(x => _visitorDictionary[x]);
        }

        private void AddProcess(Process p)
        {
            os.AddNewProcess(p);
            var processVisitor = new ProcessVisitor(p);
            processList.Items.Add(processVisitor);
            _visitorDictionary.Add(p, processVisitor);
            processVisitor.ownerDictionary = _visitorDictionary;
            OSInfoUpdateNow();
        }

        private void Btn_AddProcess(object sender, RoutedEventArgs e)
        {
            if (_newProcessWindow == null)
            {
                _newProcessWindow = new NewProcessDialog(this);
                _newProcessWindow.addNewProcess += AddProcess;
                _newProcessWindow.Show();
                _newProcessWindow.Closed += (o, args) => _newProcessWindow = null;
            }
            else
            {
                _newProcessWindow.Activate();
                // _newProcessWindow.Topmost = true;
                // newProcessWindow.Activate();
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
            if (_simulateTimer != null) _simulateTimer.Interval = 1000 / fps;
        }

        private void OnTextStepPerFrameChanged(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(textStepPerFrame.Text, out _simulateStep))
            {
                _simulateStep = 1;
            }
            _simulateStep = Math.Clamp(_simulateStep, 1, 1000);
            textStepPerFrame.Text = _simulateStep.ToString();
        }

        private void BtnRunStop_OnClick(object sender, RoutedEventArgs e)
        {
            if (_simulateTimer.Enabled)
            {
                _simulateTimer.Enabled = false;
                btnRunStop.Content = "继续";
            }
            else
            {
                _simulateTimer.Enabled = true;
                btnRunStop.Content = "暂停";
            }
        }

        private void BtnStepRun_OnClick(object sender, RoutedEventArgs e)
        {
            Update(null, null);
        }

        private void BtnKillProcess_OnClick(object sender, RoutedEventArgs e)
        {
            if (processList.SelectedItems.Count >0)
            {
                foreach (ProcessVisitor item in processList.SelectedItems)
                {
                    os.KillProcess(item.PID);
                    item.NotifyPropertyChange();
                }

                OSInfoUpdateNow();
            }
        }
        }
}
