using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ProcessManageCore;
using ProcessManageCore.Entity;

namespace ProcessManageWPF
{
    /// <summary>
    /// ProcessFullInfo.xaml 的交互逻辑
    /// </summary>
    public partial class ProcessFullInfo : UserControl
    {
        //private readonly ProcessVisitor _hotProcessVisitor;
        public ProcessFullInfo()
        {
            InitializeComponent();
            preProcessList.ItemsSource = preProcessVisitorList;
        }

        public ObservableCollection<ProcessVisitor> preProcessVisitorList = new ObservableCollection<ProcessVisitor>();

        /// <summary>
        /// 从当前的信息生成一个新进程
        /// </summary>
        /// <returns></returns>
        public Process CreateNewFromInfo(Dictionary<string, object> macro = null)
        {
            var temp = ProcessFactory.CreateIndependentProcess(
                comboBoxProcessType.SelectedIndex switch
                {
                    0 => ProcessType.System,
                    1 => ProcessType.Kernel,
                    2 => ProcessType.User,
                    _ => ProcessType.User
                },
                macro == null ? textName.Text : textName.Text.ReplaceMacro(macro),
                int.Parse(textTimeTotal.Text),
                int.Parse(textMemory.Text)
            );

            foreach (var preProcess in preProcessVisitorList)
            {
                Process.SetProcessDependence(preProcess.process, temp);
            }
            return temp;
        }

        /// <summary>
        /// 增加前驱进程的按钮被按下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_OnAddPreProcess(object sender, RoutedEventArgs e)
        {
            var selectorWindow = new ProcessSelectorWindow(MainWindow.Instance.processList.Items.Cast<ProcessVisitor>());
            selectorWindow.SelectConfirmEvent += OnSelectConfirmEvent;
            selectorWindow.ShowDialog();

        }

        private void OnSelectConfirmEvent(IEnumerable<ProcessVisitor> visitors)
        {
            foreach (var visitor in visitors)
            {
                preProcessVisitorList.Add(visitor);
            }
        }

        private void Btn_OnRemovePreProcess(object sender, RoutedEventArgs e)
        {
            var cachedItems = preProcessList.SelectedItems.Cast<ProcessVisitor>();
            for (var i = 0; i < preProcessVisitorList.Count; i++)
            {
                if (cachedItems.Contains(preProcessVisitorList[i]))
                {
                    preProcessVisitorList.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}
