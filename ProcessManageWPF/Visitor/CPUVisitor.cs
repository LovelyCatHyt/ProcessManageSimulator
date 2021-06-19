using System.ComponentModel;
using ProcessManageCore.Entity;
using ProcessManageCore.Singleton;

namespace ProcessManageWPF.Visitor
{
    public class CPUVisitor : INotifyPropertyChanged
    {
        public CPU cpu;
        public int index;

        public string Title => $"CPU {index}";

        public double Progress => cpu.IsOccupied ? ((double)cpu.timePhrase + 1) / CPU.timeSlice : 0;

        public string ProcessInfo
        {
            get
            {
                var process = ProcessTable.GetProcess(cpu.occupyingProcess);
                return cpu.IsOccupied ? $"{process.PID}: {process.name}" : "空闲";
            }
        }

        public void NotifyPropertyChange()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
        }

        public event PropertyChangedEventHandler PropertyChanged;

    }
}
