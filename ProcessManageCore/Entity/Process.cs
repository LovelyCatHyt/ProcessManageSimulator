using System.Collections.Generic;
using System.Linq;
using ProcessManageCore.Singleton;

namespace ProcessManageCore.Entity
{
    public enum ProcessType
    {
        System,
        Kernel,
        User
    }

    public enum ProcessState
    {
        /// <summary>
        /// 就绪
        /// </summary>
        Ready,
        /// <summary>
        /// 等待内存分配
        /// </summary>
        WaitForMemory,
        /// <summary>
        /// 挂起
        /// </summary>
        HangUp,
        /// <summary>
        /// 运行
        /// </summary>
        Running
    }

    public class Process
    {
        public readonly ProcessType type;
        /// <summary>
        /// 进程 PID 保证全局唯一
        /// </summary>
        public readonly int PID;
        /// <summary>
        /// 优先级, 越小优先级越高
        /// </summary>
        public int priority;
        /// <summary>
        /// 进程名
        /// </summary>
        public string name;
        /// <summary>
        /// 需求运行时间 <para>单位? 一个模拟器要什么单位🐕</para>
        /// </summary>
        public int requiredTime;
        /// <summary>
        /// 需要的内存
        /// </summary>
        public int requiredMemory;
        /// <summary>
        /// 进程状态
        /// </summary>
        public ProcessState state;
        /// <summary>
        /// true 则为独立进程
        /// </summary>
        public readonly bool isIndependent;
        /// <summary>
        /// 前驱进程(列表)
        /// </summary>
        public readonly List<int> preProcessList;
        /// <summary>
        /// 后继进程(列表)
        /// </summary>
        public readonly List<int> subsequenceProcessList;
        public bool DependenceClear { get; private set; }


        public Process(ProcessType type, int pid, string name, int requiredTime, int requiredMemory, ProcessState state, bool isIndependent, int[] preProcessList, int[] subsequenceProcessList)
        {
            this.type = type;
            PID = pid;
            this.name = name;
            this.requiredTime = requiredTime;
            this.requiredMemory = requiredMemory;
            this.state = state;
            this.isIndependent = isIndependent;
            this.preProcessList = new List<int>(preProcessList);
            DependenceClear = preProcessList.Length == 0;
            this.subsequenceProcessList = new List<int>(subsequenceProcessList);
        }

        public virtual void OnRunning()
        {
            state = ProcessState.Running;
        }

        public virtual void OnKilled()
        {
            // ?
        }

        public virtual void OnHangup()
        {
            state = ProcessState.HangUp;
        }

        public virtual void OnReady()
        {
            state = ProcessState.Ready;
        }

        public virtual void OnWaitForMemory()
        {
            state = ProcessState.WaitForMemory;
        }

        public virtual void OnFinished()
        {
            foreach (var i in subsequenceProcessList)
            {
                var subProcess = ProcessTable.GetProcess(i);
                subProcess.PreProcessFinish(PID);
            }
        }

        /// <summary>
        /// 设置进程依赖关系
        /// </summary>
        /// <param name="pre"></param>
        /// <param name="sub"></param>
        public static void SetProcessDependence(Process pre, Process sub)
        {
            pre.subsequenceProcessList.Add(sub.PID);
            sub.preProcessList.Add(pre.PID);
            sub.DependenceClear = false;
        }

        /// <summary>
        /// 一个前驱进程正常结束
        /// </summary>
        /// <param name="pid"></param>
        protected void PreProcessFinish(int pid)
        {
            // TODO: pre and subsequence process
            preProcessList.Remove(pid);
            DependenceClear = preProcessList.Count == 0;
        }

        public override string ToString() =>
            $"[{PID:D4}] {name}" +
            $": state: {state}, " +
            $"requiredTime: {requiredTime}, " +
            $"isIndependent: {isIndependent}, " +
            $"priority: {priority}, " +
            $"preList: {preProcessList.Aggregate("", (s, i) => s + i)}, " +
            $"subList: {subsequenceProcessList.Aggregate("", (s, i) => s + i)}";
    }
}
