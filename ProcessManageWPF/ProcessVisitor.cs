using System.ComponentModel;
using ProcessManageCore.Entity;
using ProcessManageCore.Singleton;

namespace ProcessManageWPF
{
    public class ProcessVisitor : INotifyPropertyChanged
    {
        public Process process;

        public event PropertyChangedEventHandler PropertyChanged;

        public string BasicInfo => $"{process.PID}: {process.name}";

        public int PID
        {
            get => process?.PID ?? -1;
            set
            {
                process = ProcessTable.GetProcess(value);
                PropertyChanged?.Invoke(this, null);
            }
        }

        public string Name
        {
            get => process?.name ?? "";
            set
            {
                if (process != null) process.name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name"));
            }
        }

        public ProcessVisitor()
        {
        }

        public ProcessVisitor(Process p)
        {
            process = p;
        }

        public void SetProcess(Process p)
        {
            process = p;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
        }
    }
}
