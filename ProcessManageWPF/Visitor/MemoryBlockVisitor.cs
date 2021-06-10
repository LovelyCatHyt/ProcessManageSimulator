using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Media;
using ProcessManageCore.Entity;
using ProcessManageCore.Singleton;

namespace ProcessManageWPF.Visitor
{
    public class MemoryBlockVisitor: INotifyPropertyChanged
    {
        public MemoryBlock block;

        public int Start => block?.startPos ?? 0;

        public int Length => block?.length ?? 0;

        public Process OccupiedProcess => ProcessTable.GetProcess(block?.occupyingProcessPID ?? 0);

        public bool Occupied => block?.occupied ?? false;

        public string ToolTip => block != null ? $"Start: {block.startPos}, Length: {block.length}" : "";

        public MemoryBlockVisitor(MemoryBlock block)
        {
            this.block = block;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
