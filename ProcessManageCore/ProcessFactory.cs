using System;
using System.Collections.Generic;
using System.Text;
using ProcessManageCore.Entity;
using ProcessManageCore.Singleton;

namespace ProcessManageCore
{
    public static class ProcessFactory
    {
        public static Process CreateProcess(ProcessType type, string name, int requiredTime, bool isIndependent, int[] preProcessList, int[] subsequenceProcessList)
        {
            // TODO: Use Object Pool maybe
            Process newProcess = new Process(type, 
                ProcessTable.GetNewAvailablePID(type),
                name,
                requiredTime,
                ProcessState.Ready,
                isIndependent,
                preProcessList,
                subsequenceProcessList);
            ProcessTable.AddProcess(newProcess);
            return newProcess;
        }
    }
}
