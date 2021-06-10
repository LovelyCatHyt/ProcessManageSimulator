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
                ProcessType.Kernel => 100,
                ProcessType.User => 200,
                _ => 20
            };
            ProcessTable.AddProcess(newProcess);
            return newProcess;
        }

        public static Process CreateIndependentProcess(ProcessType type, string name, int requiredTime,
            int requiredMemory) =>
            CreateProcess(type, name, requiredTime, requiredMemory, true, new int[0], new int[0]);
    }
}
