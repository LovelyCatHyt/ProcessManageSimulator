using System;
using System.Collections.Generic;
using System.Text;
using ProcessManageCore.Entity;
using ProcessManageCore.Singleton;

namespace ProcessManageCore
{
    public static class ProcessFactory
    {
        public static Process CreateProcess(ProcessType type, string name, int requiredTime, int requiredMemory, bool isIndependent, int[] preProcessList, int[] subsequenceProcessList)
        {
            // TODO: Use Object Pool maybe
            Process newProcess = new Process(type,
                ProcessTable.GetNewAvailablePID(type),
                name,
                requiredTime,
                requiredMemory,
                ProcessState.Ready,
                isIndependent,
                preProcessList,
                subsequenceProcessList);
            newProcess.priority = newProcess.type switch
            {
                ProcessType.System => 0,
                ProcessType.Kernel => 10,
                ProcessType.User => 20,
                _ => 20
            };
            ProcessTable.AddProcess(newProcess);
            return newProcess;
        }
    }
}
