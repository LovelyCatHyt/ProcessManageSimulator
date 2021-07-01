using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Media;
using ProcessManageCore.Entity;
using ProcessManageCore.Exception;
using ProcessManageCore.Singleton;

namespace ProcessManageWPF.Visitor
{
    public class MemoryBlockVisitor: INotifyPropertyChanged
    {
        public MemoryBlock block;

        public int Start => block?.startPos ?? 0;

        public int Length => block?.length ?? 0;

        public Process OccupiedProcess
        {
            get
            {
                try
                {
                    return ProcessTable.GetProcess(block?.occupyingProcessPID ?? 0);
                }
                catch (ProcessNotFoundException)
                {
                    return null;
                }
            }
        }

        public bool Occupied => block?.occupied ?? false;

        public string ToolTip => block != null ? $"Start: {block.startPos}, Length: {block.length}, PID: {block.occupyingProcessPID}" : "";

        public MemoryBlockVisitor(MemoryBlock block)
        {
            this.block = block;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
