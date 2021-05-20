using System;
using System.Collections.Generic;
using System.Text;

namespace ProcessManageCore.Entity
{
    /// <summary>
    /// 处理器
    /// </summary>
    public class CPU
    {
        /// <summary>
        /// 时间片长度
        /// </summary>
        public static int timeSlice = 4;

        /// <summary>
        /// 时间片位置
        /// </summary>
        public int timePhrase;
        public bool IsOccupied { get; private set; } = false;
        public int occupyingProcess;
        public int deviceID;

        /// <summary>
        /// 占用锁
        /// </summary>
        private readonly object occupyLock = new();

        /// <summary>
        /// 尝试占用
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public bool TryOccupy(int pid)
        {
            lock (occupyLock)
            {
                if (!IsOccupied)
                {
                    occupyingProcess = pid;
                    IsOccupied = true;
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 释放 CPU
        /// </summary>
        public void Release()
        {
            lock (occupyLock)
            {
                IsOccupied = false;
                occupyingProcess = 0;
                timePhrase = 0;
            }
        }

        public override string ToString() => $"id: {deviceID}, occupied: {IsOccupied}, process: {(IsOccupied ? occupyingProcess.ToString() : "None")}, timePhrase: {timePhrase}";
    }
}
