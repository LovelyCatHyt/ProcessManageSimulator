using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ProcessManageCore.Entity;
using ProcessManageWPF.Visitor;

namespace ProcessManageWPF
{
    /// <summary>
    /// ProcessSelectorWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ProcessSelectorWindow : Window
    {
        // private IEnumerable<ProcessVisitor> processes;

        public event Action<IEnumerable<ProcessVisitor>> SelectConfirmEvent;

        public ProcessSelectorWindow(IEnumerable<ProcessVisitor> toSelectList)
        {
            InitializeComponent();
            processList.ItemsSource = toSelectList;
        }

        private void Btn_Confirm(object sender, RoutedEventArgs e)
        {
            SelectConfirmEvent?.Invoke(processList.SelectedItems.Cast<ProcessVisitor>());
            Close();
        }

        private void Btn_Cancel(object sender, RoutedEventArgs e)
        {
            SelectConfirmEvent?.Invoke(new List<ProcessVisitor>());
            Close();
        }
    }
}
