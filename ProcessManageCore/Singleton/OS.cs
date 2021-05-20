using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProcessManageCore.Entity;

namespace ProcessManageCore.Singleton
{
    /// <summary>
    /// 操作系统
    /// </summary>
    public class OS
    {
        public static OS Instance { get; private set; }
        private readonly CPU[] cpuList;
        private readonly MemoryManager memoryMgr;
        private readonly List<Process> readyList = new();

        public OS(int cpuCount, int memorySize)
        {
            cpuList = new CPU[cpuCount];
            for (int i = 0; i < cpuList.Length; i++)
            {
                cpuList[i] = new CPU { deviceID = i };
            }
            memoryMgr = new MemoryManager(memorySize);

            Instance = this;
        }

        /// <summary>
        /// 更新一个时间单位
        /// </summary>
        public void Update()
        {
            foreach (var cpu in cpuList)
            {
                // cpu 占用中
                if (cpu.IsOccupied)
                {
                    cpu.timePhrase++;
                    var process = ProcessTable.GetProcess(cpu.occupyingProcess);
                    process.requiredTime--;
                    if (process.requiredTime == 0)
                    {
                        process.OnFinished();
                        cpu.Release();
                    }
                }

                // cpu 空闲
                if (!cpu.IsOccupied && readyList.Count > 0)
                {
                    RunProcessImmediate(cpu, readyList[0].PID);
                }

                // 当前时间片用尽
                if (cpu.timePhrase == CPU.timeSlice)
                {
                    var current = cpu.occupyingProcess;
                    AddProcess(ProcessTable.GetProcess(current));
                    cpu.Release();
                    RunProcessImmediate(cpu, readyList[0].PID);
                }
            }
            // 更新队列优先级
            readyList.ForEach(p => p.priority = Math.Max(0, p.priority - 1));
        }

        /// <summary>
        /// 更新 count 个时间单位
        /// <para>小于等于 0 的值相当于不运行</para>
        /// </summary>
        /// <param name="count"></param>
        public void Update(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Update();
            }
        }

        /// <summary>
        /// 立即运行进程
        /// </summary>
        public void RunProcessImmediate(CPU cpu, int pid)
        {
            cpu.TryOccupy(pid);
            var index = readyList.FindIndex(p => p.PID == pid);
            readyList[index].OnRunning();
            readyList.RemoveAt(index);
        }

        /// <summary>
        /// 在就绪队列中添加进程
        /// <para>可以是新进程, 也可以是从 cpu 中置换出来的进程</para>
        /// </summary>
        /// <param name="p"></param>
        public void AddProcess(Process p)
        {
            readyList.Add(p);
            p.OnReady();
            readyList.Sort((x, y) => x.priority - y.priority);
        }

        /// <summary>
        /// 杀进程
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public bool KillProcess(int pid)
        {
            var processID = readyList.FindIndex(p => p.PID == pid);
            if (processID < 0) return false;
            var process = readyList[processID];
            var subProcesses = process.subsequenceProcessList;
            process.OnKilled();
            cpuList.First(c => c.occupyingProcess == pid)?.Release();
            memoryMgr.ReleaseMemoryOfProcess(process.PID);
            readyList.RemoveAt(processID);
            // 递归删后继进程, 在这里将子进程和后继进程视为相同的概念
            foreach (var subProcess in subProcesses)
            {
                KillProcess(subProcess);
            }
            return true;
        }

        public override string ToString()
        {
            StringBuilder strBuilder = new();
            strBuilder.AppendLine("CPU list:");
            foreach (var cpu in cpuList)
            {
                strBuilder.AppendFormat("\t{0}\n", cpu);
            }

            strBuilder.AppendLine("Ready process:");
            readyList.ForEach(p => strBuilder.AppendFormat("\t{0}\n", p));
            return strBuilder.ToString();
        }
    }
}
