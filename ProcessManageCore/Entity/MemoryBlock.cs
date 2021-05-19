
using Microsoft.VisualBasic.CompilerServices;
using ProcessManageCore.Exception;

namespace ProcessManageCore.Entity
{
    /// <summary>
    /// 内存块
    /// </summary>
    public class MemoryBlock
    {
        public bool occupied;
        public int occupyingProcessPID;
        public int startPos;
        public int length;

        /// <summary>
        /// 分配内存块
        /// </summary>
        /// <returns></returns>
        public bool Distribute(int requestLength, out MemoryBlock block)
        {
            if (!occupied && length >= requestLength)
            {
                block = new MemoryBlock {length = requestLength, startPos = startPos};
                startPos += requestLength;
                length -= requestLength;
                return true;
            }

            block = null;
            return false;
        }

        /// <summary>
        /// 分配内存块
        /// </summary>
        /// <param name="requestLength"></param>
        /// <returns></returns>
        public MemoryBlock Distribute(int requestLength)
        {
            MemoryBlock block;
            if (Distribute(requestLength, out block))
            {
                return block;
            }
            else
            {
                throw new MemoryRequestTooLargeException();
            }
        }

        /// <summary>
        /// 尝试占用
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public bool TryOccupy(int pid)
        {
            if (!occupied)
            {
                occupyingProcessPID = pid;
                return occupied = true;
            }
            return false;
        }

        /// <summary>
        /// 尝试合并内存块
        /// </summary>
        /// <param name="other">另一个内存块</param>
        /// <returns></returns>
        public bool TryMerge(MemoryBlock other)
        {
            if (startPos + length == other.startPos)
            {
                length += other.length;
                return true;
            }
            return false;
        }
    }
}
