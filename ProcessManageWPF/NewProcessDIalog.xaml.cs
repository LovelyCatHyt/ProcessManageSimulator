using System;
using System.Windows;
using ProcessManageCore.Entity;

namespace ProcessManageWPF
{
    /// <summary>
    /// NewProcessDialog.xaml 的交互逻辑
    /// </summary>
    public partial class NewProcessDialog : Window
    {
        public event Action<Process> addNewProcess;

        public NewProcessDialog()
        {
            InitializeComponent();
        }

        private void Submit(object sender, RoutedEventArgs e)
        {
            addNewProcess?.Invoke(processFullInfo.CreateNewFromInfo());
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
