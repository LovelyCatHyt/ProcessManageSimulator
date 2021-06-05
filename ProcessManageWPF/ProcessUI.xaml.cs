using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Process = ProcessManageCore.Entity.Process;

namespace ProcessManageWPF
{
    /// <summary>
    /// ProcessUI.xaml 的交互逻辑
    /// </summary>
    public partial class ProcessUI : UserControl
    {
        private double _progress;

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
        /// <summary>
        /// 进程需要的总时间
        /// </summary>
        public readonly int totalTime;

        public ProcessUI(Process bindingProcess = null)
        {
            InitializeComponent();

            this.bindingProcess = bindingProcess;
            Debug.Assert(bindingProcess != null, nameof(this.bindingProcess) + " != null");
            totalTime = bindingProcess.requiredTime;
            pid.Content = bindingProcess.PID;
            label.Content = bindingProcess.name;
        }

        public void UpdateProperties()
        {
            Progress = 1 - (double)bindingProcess.requiredTime / totalTime;
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
