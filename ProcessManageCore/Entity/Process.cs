using System.Linq;

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
        public readonly int[] preProcessList;
        /// <summary>
        /// 后继进程(列表)
        /// </summary>
        public readonly int[] subsequenceProcessList;

        public Process(ProcessType type, int pid, string name, int requiredTime, ProcessState state, bool isIndependent, int[] preProcessList, int[] subsequenceProcessList)
        {
            this.type = type;
            PID = pid;
            this.name = name;
            this.requiredTime = requiredTime;
            this.state = state;
            this.isIndependent = isIndependent;
            this.preProcessList = preProcessList;
            this.subsequenceProcessList = subsequenceProcessList;
        }

        public void OnRunning()
        {

        }

        public void OnKilled()
        {

        }

        public void OnReady()
        {

        }

        public void OnFinished()
        {

        }

        public override string ToString() =>
            $"[{PID:D4}] {name}" +
            $": state: {state}, " +
            $"requiredTime: {requiredTime}, " +
            $"isIndependent: {isIndependent}, " +
            $"priority: {priority} " +
            $"preList: {preProcessList.Aggregate("", (s, i) => s + i)}, " +
            $"subList: {subsequenceProcessList.Aggregate("", (s, i) => s + i)}";
    }
}
