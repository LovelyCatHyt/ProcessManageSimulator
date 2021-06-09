using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using ProcessManageCore.Entity;
using ProcessManageCore.Singleton;

namespace ProcessManageWPF
{
    /// <summary>
    /// NewProcessDialog.xaml 的交互逻辑
    /// </summary>
    public partial class NewProcessDialog : Window
    {
        private Dictionary<string, object> _macros = new Dictionary<string, object>();
        private int _duplicatedCount;
        public event Action<Process> addNewProcess;

        public NewProcessDialog(Window owner)
        {
            InitializeComponent();
            _macros.Add("DupCount", _duplicatedCount);
            btnSubmit.Focus();
            processFullInfo.textName.Text = App.cfg.GetStringCache("NewProcess.Name");
            processFullInfo.textTimeTotal.Text = App.cfg.GetIntCache("NewProcess.Memory").ToString();
            processFullInfo.textMemory.Text = App.cfg.GetIntCache("NewProcess.TotalTime").ToString();
            processFullInfo.comboBoxProcessType.SelectedIndex = App.cfg.GetIntCache("NewProcess.ProcessType");
            Owner = owner;
        }

        private void Submit(object sender, RoutedEventArgs e)
        {
            var process = processFullInfo.CreateNewFromInfo(_macros);
            addNewProcess?.Invoke(process);
            App.cfg.SetStringCache("NewProcess.Name", processFullInfo.textName.Text);
            App.cfg.SetIntCache("NewProcess.Memory",
                int.TryParse(processFullInfo.textMemory.Text, out var temp) ? temp : 0);
            App.cfg.SetIntCache("NewProcess.TotalTime",
                int.TryParse(processFullInfo.textTimeTotal.Text, out temp) ? temp : 0);
            App.cfg.SetIntCache("NewProcess.ProcessType", processFullInfo.comboBoxProcessType.SelectedIndex);
            // 计数
            if (processFullInfo.textName.Text == App.cfg.GetStringCache("NewProcess.Name"))
            {
                _duplicatedCount++;
            }
            else
            {
                _duplicatedCount = 0;
            }
            _macros["DupCount"] = _duplicatedCount;
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter: Submit(sender, null); break;
            }
        }
    }
}
