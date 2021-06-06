using System;
using System.Windows.Controls;
using ProcessManageCore.Entity;
using ProcessManageCore.Singleton;

namespace ProcessManageWPF
{
    /// <summary>
    /// CPUUI.xaml 的交互逻辑
    /// </summary>
    public partial class CPUUI : UserControl
    {
        private CPU cpu;
        private double _progress;
        private int id;

        public double Progress
        {
            get => _progress;
            set
            {
                UpdateUI();
                value = Math.Clamp(value, 0, 1);
                _progress = value;
            }
        }

        public CPUUI(CPU cpu, int id)
        {
            InitializeComponent();
            this.cpu = cpu;
            this.id = id;
            labelName.Content = $"CPU: {id}";
            labelProcess.Content = "";
        }

        public void Update()
        {
            Progress = (double)(cpu.timePhrase + 1) / CPU.timeSlice;
        }

        private void UpdateUI()
        {
            if (cpu.IsOccupied)
            {
                progressBarContent.Width = progressBackGround.ActualWidth * _progress;
                var process = ProcessTable.GetProcess(cpu.occupyingProcess);
                labelProcess.Content = $"{process.PID}: {process.name}";
            }
            else
            {
                progressBarContent.Width = 0;
                labelProcess.Content = "";
            }
        }
    }
}
