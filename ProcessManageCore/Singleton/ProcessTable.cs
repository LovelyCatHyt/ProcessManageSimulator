using System;
using System.Collections.Generic;
using System.Text;
using ProcessManageCore.Entity;
using ProcessManageCore.Exception;

namespace ProcessManageCore.Singleton
{
    public static class ProcessTable
    {
        private static readonly Dictionary<ProcessType, int> pidPhraseTable = new()
        {
            { ProcessType.System, 0 },
            { ProcessType.Kernel, 100 },
            { ProcessType.User, 1000 }
        };
        private static readonly Dictionary<int, Process> _processesDictionary = new();
        private static readonly HashSet<int> _usedPIDSet = new();

        public static void ResetPIDTable()
        {
            pidPhraseTable[ProcessType.System] = 0;
            pidPhraseTable[ProcessType.Kernel] = 100;
            pidPhraseTable[ProcessType.User] = 1000;
        }

        public static int GetNewAvailablePID(ProcessType type)
        {
            var ret = pidPhraseTable[type];
            pidPhraseTable[type] = ret + 1;
            return ret;
        }

        public static void AddProcess(Process p)
        {
            if (!_usedPIDSet.Contains(p.PID))
            {
                _processesDictionary.Add(p.PID, p);
                _usedPIDSet.Add(p.PID);
            }
        }

        public static void RemoveProcess(int pid)
        {
            bool hasProcess = false;
            try
            {
                _processesDictionary.Remove(pid);
                hasProcess = _usedPIDSet.Remove(pid);
            }
            catch (ArgumentNullException)
            {
                throw new ProcessNotFoundException();
            }

            if (!hasProcess) throw new ProcessNotFoundException();
        }

        /// <summary>
        /// 根据 PID 获取一个进程
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public static Process GetProcess(int pid)
        {
            try
            {
                return _processesDictionary[pid];
            }
            catch (KeyNotFoundException)
            {
                throw new ProcessNotFoundException();
            }
        }
    }
}
