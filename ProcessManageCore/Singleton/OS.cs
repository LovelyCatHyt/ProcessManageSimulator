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
        /// <summary>
        /// 就绪队列
        /// </summary>
        private readonly List<Process> readyList = new();
        /// <summary>
        /// 因申请内存过多而等待内存分配的队列
        /// </summary>
        private readonly List<Process> waitForMemoryList = new();

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
                        KillProcess(process.PID);
                    }
                }

                // 将可能空出分配给后备队列
                TryAllocateWaitList();

                // cpu 空闲
                if (!cpu.IsOccupied && readyList.Count > 0)
                {
                    RunProcessImmediate(cpu, readyList[0].PID);
                }

                // 当前时间片用尽
                if (cpu.timePhrase == CPU.timeSlice)
                {
                    var current = cpu.occupyingProcess;
                    AddProcessToReadyList(ProcessTable.GetProcess(current));
                    cpu.Release();
                    if (readyList.Count > 0) RunProcessImmediate(cpu, readyList[0].PID);
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
        /// 尝试给等待内存队列分配内存
        /// </summary>
        public void TryAllocateWaitList()
        {
            if (waitForMemoryList.Count > 0)
            {
                for (int i = 0; i < waitForMemoryList.Count; i++)
                {
                    var p = waitForMemoryList[i];
                    if (memoryMgr.RequestMemory(p.requiredMemory, p.PID) != null)
                    {
                        AddProcessToReadyList(waitForMemoryList[i]);
                        waitForMemoryList.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        /// <summary>
        /// 在就绪队列中添加新进程, 并试图分配内存
        /// <para></para>
        /// <para>内存不足则放到等待内存分配的位置</para>
        /// </summary>
        /// <param name="p"></param>
        public void AddNewProcess(Process p)
        {
            if (p.requiredMemory > memoryMgr.totalLength)
            {
                throw new ArgumentOutOfRangeException(nameof(p.requiredMemory), "Requiring memory more than the total memory size");
            }
            if (memoryMgr.RequestMemory(p.requiredMemory, p.PID) != null)
            {
                AddProcessToReadyList(p);
            }
            else
            {
                waitForMemoryList.Add(p);
            }
        }

        private void AddProcessToReadyList(Process p)
        {
            readyList.Add(p);
            p.OnReady();
            readyList.Sort((x, y) => x.priority - y.priority);
        }

        /// <summary>
        /// 关机, 其实就是杀掉所有进程
        /// </summary>
        public void CleanUp()
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < readyList.Count; i++)
            {
                KillProcess(readyList[i].PID);
            }

            waitForMemoryList.Clear();

            foreach (var cpu in cpuList)
            {
                if (cpu.IsOccupied) memoryMgr.ReleaseMemoryOfProcess(cpu.occupyingProcess);
                cpu.Release();
            }
        }

        /// <summary>
        /// 在就绪队列中杀进程
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public bool KillProcess(int pid)
        {
            var processID = readyList.FindIndex(p => p.PID == pid);
            if (processID < 0)
            {
                // 不在就绪队列
                return KillProcessInCPU(pid);
            }
            var process = readyList[processID];
            var subProcesses = process.subsequenceProcessList;
            process.OnKilled();
            memoryMgr.ReleaseMemoryOfProcess(process.PID);
            readyList.RemoveAt(processID);
            // 递归删后继进程, 在这里将子进程和后继进程视为相同的概念
            foreach (var subProcess in subProcesses)
            {
                KillProcess(subProcess);
            }
            return true;
        }

        /// <summary>
        /// 在 CPU 中杀进程
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public bool KillProcessInCPU(int pid)
        {
            bool found = false;
            var process = ProcessTable.GetProcess(pid);
            foreach (var cpu in cpuList)
            {
                if (cpu.IsOccupied)
                {
                    if (pid == cpu.occupyingProcess)
                    {
                        found = true;
                        process.OnKilled();
                        memoryMgr.ReleaseMemoryOfProcess(pid);
                        cpu.Release();
                    }
                }
            }
            return found;
        }

        public override string ToString()
        {
            StringBuilder strBuilder = new();
            var used = memoryMgr.UsedSpace;
            var total = memoryMgr.totalLength;
            strBuilder.AppendFormat("Memory: {0:D4}/{1:D4}: {2:F1}%\n", used, total, (float)used / total * 100);
            strBuilder.AppendLine("CPU list:");
            foreach (var cpu in cpuList)
            {
                strBuilder.AppendFormat("\t{0}\n", cpu);
            }

            strBuilder.AppendLine("Ready process:");
            readyList.ForEach(p => strBuilder.AppendFormat("\t{0}\n", p));
            strBuilder.AppendLine("waitForMemory process:");
            waitForMemoryList.ForEach(p => strBuilder.AppendFormat("\t{0}\n", p));
            return strBuilder.ToString();
        }
    }
}
