using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using ProcessManageCore.Entity;
using ProcessManageCore.Singleton;

namespace ProcessManageWPF
{
    public class ProcessVisitor : INotifyPropertyChanged
    {
        public Process process;

        public event PropertyChangedEventHandler PropertyChanged;

        public string BasicInfo => $"{process.PID}: {process.name}";

        public int PID
        {
            get => process?.PID ?? -1;
            set
            {
                process = ProcessTable.GetProcess(value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
            }
        }

        public ProcessType ProcessType => process?.type ?? ProcessType.System;

        /// <summary>
        /// 进程名
        /// </summary>
        public string Name
        {
            get => process?.name ?? "";
            set
            {
                if (process != null) process.name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }

        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority => process?.priority ?? 0;

        /// <summary>
        /// 进度
        /// </summary>
        public double Progress
        {
            get
            {
                if (process != null)
                {
                    return 1 - (double)process.remainedTime / process.totalTime;
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                if (process != null) process.remainedTime = (int)((1 - value) * process.totalTime);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Progress)));
            }
        }

        /// <summary>
        /// 总时间
        /// </summary>
        public int TotalTime
        {
            get => process?.totalTime ?? 0;
            set
            {
                if (process != null)
                {
                    var elapsedTime = process.totalTime - process.remainedTime;
                    process.totalTime = Math.Max(value, elapsedTime);
                    process.remainedTime = process.totalTime - elapsedTime;
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalTime)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Progress)));
            }
        }

        /// <summary>
        /// 运行的时间
        /// </summary>
        public int ElapsedTime
        {
            get => process != null ? process.totalTime - process.remainedTime : 0;
            set
            {
                if (process != null)
                {
                    value = process.totalTime - value;
                    process.remainedTime = Math.Clamp(value, 0, process.totalTime);
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ElapsedTime)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Progress)));
            }
        }

        /// <summary>
        /// 内存使用
        /// </summary>
        public int MemoryUsage
        {
            get => process?.requiredMemory ?? 0;
            set
            {
                if (process != null) { process.requiredMemory = Math.Max(value, 0); }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MemoryUsage)));
            }
        }

        public ProcessState State => process?.state ?? ProcessState.Ready;

        public ProcessVisitor()
        {
        }

        public ProcessVisitor(Process p)
        {
            process = p;
        }

        public void NotifyPropertyChange()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
        }

        public void SetProcess(Process p)
        {
            process = p;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
        }
    }

    public class StateColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ProcessState state)
            {
                return new SolidColorBrush(state switch
                {
                    ProcessState.Ready => Color.FromRgb(0, 200, 100),
                    ProcessState.WaitForMemory => Color.FromRgb(200, 30, 0),
                    ProcessState.HangUp => Color.FromRgb(64, 64, 64),
                    ProcessState.Running => Color.FromRgb(60, 100, 255),
                    _ => Color.FromRgb(0, 0, 0)
                });
            }
            return new SolidColorBrush(Color.FromRgb(255, 255, 255));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ProcessState.Ready;
        }
    }

    public class StateStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((ProcessState)value).ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ProcessState.Ready;
        }
    }

    public class ProcessTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((ProcessType)value).ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (ProcessType)value;
        }
    }
}
