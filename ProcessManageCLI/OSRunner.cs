using System.Threading;
using Console = Colorful.Console;
using ProcessManageCore.Singleton;

namespace ProcessManageCLI
{
    /// <summary>
    /// 操作系统运行者, 自动运行并控制进程
    /// </summary>
    public class OSRunner
    {
        public readonly OS os;
        public int step;
        public float frameRate;

        public OSRunner(OS os, int step = 1, float frameRate = 60)
        {
            this.os = os;
            this.step = step;
            this.frameRate = frameRate;
        }

        public void StartRunning()
        {
            while (true)
            {
                Timer timer = new Timer(PrintData);
                os.Update(step);
            }
        }

        private void PrintData(object o)
        {
            var left = Console.CursorLeft;
            var top = Console.CursorTop;
            Console.Write(os);
            Console.SetCursorPosition(left, top);
        }
    }
}
