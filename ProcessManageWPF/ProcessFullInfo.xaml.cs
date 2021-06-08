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
            // _hotProcessVisitor = (ProcessVisitor)FindResource("hotProcess");
        }

        //public void ApplyData(Process p)
        //{
        //    _hotProcessVisitor.SetProcess(p);
        //    textPid.Text = p.PID.ToString();
        //    // textName.Text = p.name;
            
        //    comboBoxProcessType.SelectedIndex = p.type switch
        //    {
        //        ProcessType.System => 0,
        //        ProcessType.Kernel => 1,
        //        ProcessType.User => 2,
        //        _ => -1
        //    };
        //    textTimeTotal.Text = p.totalTime.ToString();
        //    textTimeElapsed.Text = (p.totalTime - p.remainedTime).ToString();
        //    textState.Text = p.state switch
        //    {
        //        ProcessState.Ready => "就绪",
        //        ProcessState.WaitForMemory => "等待内存",
        //        ProcessState.HangUp => "挂起",
        //        ProcessState.Running => "运行中",
        //        _ => "无效状态"
        //    };
        //    ((SolidColorBrush)textState.Background).Color = p.state switch
        //    {
        //        ProcessState.Ready => Color.FromRgb(0, 200, 100),
        //        ProcessState.WaitForMemory => Color.FromRgb(200, 30, 0),
        //        ProcessState.HangUp => Color.FromRgb(64, 64, 64),
        //        ProcessState.Running => Color.FromRgb(60, 100, 255),
        //        _ => Color.FromRgb(0, 0, 0)
        //    };
        //    textMemory.Text = p.requiredMemory.ToString();
        //    textMemBlock.Text = "??";
        //}

        //public void ClearData()
        //{
        //    textPid.Text = "";
        //    // textName.Text = "";
        //    comboBoxProcessType.SelectedIndex = -1;
        //    textTimeTotal.Text = "0";
        //    textTimeElapsed.Text = "0";
        //    textState.Text = "-";
        //    ((SolidColorBrush)textState.Background).Color = Color.FromRgb(255, 255, 255);
        //    textMemory.Text = "0";
        //    textMemBlock.Text = "??";
        //}

        /// <summary>
        /// 从当前的信息生成一个新进程
        /// </summary>
        /// <returns></returns>
        public Process CreateNewFromInfo()
        {
            Process temp = ProcessFactory.CreateIndependentProcess(
                comboBoxProcessType.SelectedIndex switch
                {
                    0 => ProcessType.System,
                    1 => ProcessType.Kernel,
                    2 => ProcessType.User,
                    _ => ProcessType.User
                }, 
                textName.Text, 
                int.Parse(textTimeTotal.Text), 
                int.Parse(textMemory.Text)
            );
            return temp;
        }
    }
}
