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
        public bool CPUAllFree => cpuList.All(x => !x.IsOccupied);
        /// <summary>
        /// 经历的总运行周期数, 每次 Update 算一个刻
        /// </summary>
        public long ElapsedPeriod { get; private set; }
        public event Action<OS> updateEvent;
        public readonly CPU[] cpuList;
        public Process[] ReadyList => readyList.ToArray();
        public Process[] HangupList => hangupList.ToArray();
        public Process[] WaitForMemoryList => waitForMemoryList.ToArray();
        public MemoryBlock[] AllMemoryBlocks => memoryMgr.AllBlocks;
        //public MemoryBlock[] MemoryBlocks=>

        private readonly MemoryManager memoryMgr;
        /// <summary>
        /// 就绪队列
        /// </summary>
        private readonly List<Process> readyList = new();
        /// <summary>
        /// 因申请内存过多而等待内存分配的队列
        /// </summary>
        private readonly List<Process> waitForMemoryList = new();
        /// <summary>
        /// 挂起队列, 进入挂起队列的同时会尝试分配内存
        /// </summary>
        private readonly List<Process> hangupList = new();
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
                    process.remainedTime--;
                    if (process.remainedTime <= 0)
                    {
                        process.OnFinished();
                        KillProcess(process.PID);
                    }
                }

                // 将可能空出分配给后备队列
                TryAllocateWaitList();
                // 检查挂起队列是否有可以解挂的进程, 以及是否有应该挂起的进程
                CheckUnhang();
                CheckHangup();
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
            ElapsedPeriod++;
            updateEvent?.Invoke(this);
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
        private void TryAllocateWaitList()
        {
            if (waitForMemoryList.Count > 0)
            {
                for (int i = 0; i < waitForMemoryList.Count; i++)
                {
                    var p = waitForMemoryList[i];
                    if (waitForMemoryList.All(x => !p.preProcessList.Contains(x.PID)) && memoryMgr.RequestMemory(p.requiredMemory, p.PID) != null)
                    {
                        AddProcessToReadyList(waitForMemoryList[i]);
                        waitForMemoryList.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        /// <summary>
        /// 检查挂起队列, 将可以解挂的进程移到就绪队列
        /// </summary>
        private void CheckUnhang()
        {
            for (int i = 0; i < hangupList.Count; i++)
            {
                if (hangupList[i].DependenceClear)
                {
                    AddProcessToReadyList(hangupList[i]);
                    hangupList.RemoveAt(i);
                    i--;
                }
            }
        }

        private void CheckHangup()
        {
            for (int i = 0; i < readyList.Count; i++)
            {
                if (!readyList[i].DependenceClear)
                {
                    readyList[i].OnHangup();
                    hangupList.Add(readyList[i]);
                    readyList.RemoveAt(i);
                    i--;
                }
            }
        }

        /// <summary>
        /// 将任意状态的进程挂起
        /// </summary>
        /// <param name="p"></param>
        public void Hangup(Process p)
        {
            switch (p.state)
            {
                case ProcessState.Running:
                    cpuList.FirstOrDefault(x => x.occupyingProcess == p.PID)?.Release();
                    hangupList.Add(p);
                    break;
            }
            p.ForceHangup();
            // p.ForceHangup();
        }

        /// <summary>
        /// 无视前驱进程强制解挂
        /// </summary>
        /// <param name="p"></param>
        public void Unhang(Process p)
        {
            p.ForceUnhang();
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
            if (p.DependenceClear)
            {
                if (memoryMgr.RequestMemory(p.requiredMemory, p.PID) != null)
                {
                    AddProcessToReadyList(p);
                }
                else
                {
                    waitForMemoryList.Add(p);
                    p.OnWaitForMemory();
                }
            }
            else
            {
                if (waitForMemoryList.Any(x => p.preProcessList.Contains(x.PID)))
                {
                    waitForMemoryList.Add(p);
                    p.OnWaitForMemory();
                }
                else
                {
                    hangupList.Add(p);
                    p.OnHangup();
                }
            }
        }

        private void AddProcessToReadyList(Process p)
        {
            readyList.Add(p);
            p.OnReady();
            var sorted = readyList.OrderBy(x => x.priority).ToArray();
            readyList.Clear();
            // readyList.Capacity= sorted.Count();
            readyList.AddRange(sorted);
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
        /// 杀进程
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public bool KillProcess(int pid)
        {
            if (KillInReady(pid)) return true;
            if (KillInHangup(pid)) return true;
            if (KillInWaitForMemory(pid)) return true;
            return KillProcessInCPU(pid);
        }

        private bool KillInWaitForMemory(int pid)
        {
            var processID = waitForMemoryList.FindIndex(p => p.PID == pid);
            if (processID < 0)
            {
                // 不在就绪队列
                return false;
            }
            var process = waitForMemoryList[processID];
            var subProcesses = process.subsequenceProcessList;
            process.OnKilled();
            // memoryMgr.ReleaseMemoryOfProcess(process.PID);
            waitForMemoryList.RemoveAt(processID);
            // 递归删后继进程, 在这里将子进程和后继进程视为相同的概念
            foreach (var subProcess in subProcesses)
            {
                KillProcess(subProcess);
            }

            return true;
        }

        private bool KillInHangup(int pid)
        {
            var processID = hangupList.FindIndex(p => p.PID == pid);
            if (processID < 0)
            {
                // 不在就绪队列
                return false;
            }
            var process = hangupList[processID];
            var subProcesses = process.subsequenceProcessList;
            process.OnKilled();
            memoryMgr.ReleaseMemoryOfProcess(process.PID);
            hangupList.RemoveAt(processID);
            // 递归删后继进程, 在这里将子进程和后继进程视为相同的概念
            foreach (var subProcess in subProcesses)
            {
                KillProcess(subProcess);
            }

            return true;
        }

        private bool KillInReady(int pid)
        {
            var processID = readyList.FindIndex(p => p.PID == pid);
            if (processID < 0)
            {
                // 不在就绪队列
                return false;
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
            strBuilder.AppendFormat("Elapsed period: {0}\n", ElapsedPeriod);
            strBuilder.AppendFormat("Memory: {0:D4}/{1:D4}: {2:F1}%\n", used, total, (float)used / total * 100);
            strBuilder.AppendLine("CPU list:");
            foreach (var cpu in cpuList)
            {
                strBuilder.AppendFormat("\t{0}\n", cpu);
            }

            strBuilder.AppendLine("Ready process:");
            readyList.ForEach(p => strBuilder.AppendFormat("\t{0}\n", p));
            strBuilder.AppendLine("WaitForMemory process:");
            waitForMemoryList.ForEach(p => strBuilder.AppendFormat("\t{0}\n", p));
            strBuilder.AppendLine("HangUp process:");
            hangupList.ForEach(p => strBuilder.AppendFormat("\t{0}\n", p));
            return strBuilder.ToString();
        }
    }
}
