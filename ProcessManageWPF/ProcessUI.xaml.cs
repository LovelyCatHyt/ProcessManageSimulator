using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Media;
using ProcessManageCore.Entity;
using Process = ProcessManageCore.Entity.Process;

namespace ProcessManageWPF
{
    /// <summary>
    /// ProcessUI.xaml 的交互逻辑
    /// </summary>
    public partial class ProcessUI : UserControl
    {
        private double _progress;

        public bool Killed { get; private set; }
        /// <summary>
        /// 当前进度, 从0到1
        /// </summary>
        public double Progress
        {
            get => _progress;
            set
            {
                value = Math.Clamp(value, 0, 1);
                UpdateProgressBar(value);
                _progress = value;
            }
        }

        /// <summary>
        /// 这个UI元素所绑定的进程
        /// </summary>
        public readonly Process bindingProcess;
        
        public ProcessUI(Process bindingProcess = null)
        {
            InitializeComponent();

            this.bindingProcess = bindingProcess;
            Debug.Assert(bindingProcess != null, nameof(this.bindingProcess) + " != null");
            bindingProcess.killedEvent += OnProcessKilled;
            labelName.Content = bindingProcess.name;
            pid.Content = bindingProcess.PID;
            // labelName.Content = bindingProcess.name;
        }
        
        private void OnProcessKilled(Process obj)
        {
            Killed = true;
        }

        public void UpdateProperties()
        {
            Progress = 1 - (double)bindingProcess.remainedTime / bindingProcess.totalTime;
            var colorBrush = ((SolidColorBrush)progressBarContent.Fill);
            colorBrush.Color = bindingProcess.state switch
            {
                ProcessState.Ready => Color.FromRgb(0, 200, 100),
                ProcessState.WaitForMemory => Color.FromRgb(200, 30, 0),
                ProcessState.HangUp => Color.FromRgb(64, 64, 64),
                ProcessState.Running => Color.FromRgb(60, 100, 255),
                _ => Color.FromRgb(0, 0, 0)
            };
        }

        private void OnLayoutUpdated(object sender, EventArgs e)
        {
            UpdateProgressBar();
        }

        private void UpdateProgressBar()
        {
            UpdateProgressBar(Progress);
        }

        private void UpdateProgressBar(double value)
        {
            progressBarContent.Width = progressBackGround.ActualWidth * value;
        }
    }
}
