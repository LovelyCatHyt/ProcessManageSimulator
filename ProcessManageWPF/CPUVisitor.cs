using System.ComponentModel;
using ProcessManageCore.Entity;

namespace ProcessManageWPF
{
    public class CPUVisitor : INotifyPropertyChanged
    {
        public CPU cpu;
        public int index;

        public string Title => $"CPU {index}";

        public double Progress => cpu.IsOccupied ? ((double)cpu.timePhrase + 1) / CPU.timeSlice : 0;

        public void NotifyPropertyChange()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
        }

        public event PropertyChangedEventHandler PropertyChanged;

    }
}
