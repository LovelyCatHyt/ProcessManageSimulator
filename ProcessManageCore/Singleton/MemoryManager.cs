using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProcessManageCore.Entity;

namespace ProcessManageCore.Singleton
{
    public class MemoryManager
    {
        public readonly int totalLength;
        public int UsedSpace => _occupiedBlocks.Sum(b => b.length);
        public int RemainedSpace => _availableBlocks.Sum(b => b.length);
        public int MaxBlock => _availableBlocks.Count > 0 ? _availableBlocks.Max(b => b.length) : 0;
        public MemoryBlock[] AllBlocks
        {
            get
            {
                int aIndex = 0;
                int oIndex = 0;

                int total = _occupiedBlocks.Count + _availableBlocks.Count;
                MemoryBlock[] tempArray = new MemoryBlock[total];
                var i = 0;
                for (; aIndex< _availableBlocks.Count; i++)
                {
                    tempArray[i] = _availableBlocks[aIndex];
                    aIndex++;
                }
                for(;oIndex< _occupiedBlocks.Count; i++)
                {
                    tempArray[i] = _occupiedBlocks[oIndex];
                    oIndex++;
                }
                return tempArray.OrderBy(x=>x.startPos).ToArray();
            }
        }

        private readonly List<MemoryBlock> _availableBlocks = new();
        private readonly List<MemoryBlock> _occupiedBlocks = new();
        private readonly Dictionary<int, List<MemoryBlock>> _occupyDictionary = new();
        private int _current = 0;

        public MemoryManager(int totalLength)
        {
            this.totalLength = totalLength;
            _availableBlocks.Add(new MemoryBlock { length = totalLength, startPos = 0, occupied = false });
        }

        /// <summary>
        /// 申请内存
        /// </summary>
        /// <param name="length"></param>
        /// <param name="process"></param>
        /// <returns></returns>
        public MemoryBlock RequestMemory(int length, int process)
        {
            int visited = 0;
            int index = _availableBlocks.Count > 0 ? _current % _availableBlocks.Count : 0;
            // 首次适应算法
            while (visited < _availableBlocks.Count)
            {
                if (_availableBlocks[index].Distribute(length, out var tempBlock))
                {
                    // 找到了
                    _occupiedBlocks.Add(tempBlock);
                    if (_availableBlocks[index].length == 0) _availableBlocks.RemoveAt(index);
                    _current = index;
                    // 可占用
                    if (tempBlock.TryOccupy(process))
                    {
                        // 记录占用的内存块到对应的表中
                        _occupyDictionary.TryAdd(process, new List<MemoryBlock>());
                        _occupyDictionary[process].Add(tempBlock);
                        return tempBlock;
                    }
                }

                visited++;
                index++;
                index %= _availableBlocks.Count;
            }
            return null;
        }

        /// <summary>
        /// 从一个列表中根据起始地址寻找指定内存块在该列表中的索引
        /// </summary>
        /// <param name="blockList">必须是升序的列表</param>
        /// <param name="startPos"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static bool FindBlockByStart(List<MemoryBlock> blockList, int startPos, out int index)
        {
            int min = 0;
            int max = blockList.Capacity - 1;
            index = (min + max) / 2;
            while (blockList[index].startPos != startPos)
            {
                if (blockList[index].startPos < startPos)
                {
                    max = index + 1;
                }
                else
                {
                    min = index;
                }

                if (min == max - 1) return false;
                index = (min + max) / 2;
            }
            return true;
        }

        public static int FindBlockByStart(List<MemoryBlock> blockList, int startPos)
        {
            if (FindBlockByStart(blockList, startPos, out var index))
            {
                return index;
            }
            else
            {
                throw new ArgumentException();
            }
        }

        /// <summary>
        /// 释放进程的内存
        /// </summary>
        /// <param name="process"></param>
        public void ReleaseMemoryOfProcess(int process)
        {
            if(!_occupyDictionary.TryGetValue(process, out var list)) return;
            // I know it's slow, but who cares? XD
            // TODO-Performance: _occupiedBlocks use hashset
            _occupiedBlocks.RemoveAll(x => list.Contains(x));
            list.ForEach(x => x.Release());
            // _occupiedBlocks.Except(list);
            _availableBlocks.AddRange(list);
            _availableBlocks.Sort((x, y) => x.startPos - y.startPos);
            for (int i = 0; i < _availableBlocks.Count - 1; i++)
            {
                if (_availableBlocks[i].TryMerge(_availableBlocks[i + 1]))
                {
                    // 移除被 merge 的内存块
                    _availableBlocks.RemoveAt(i + 1);
                    // 返回一步
                    i--;
                }
            }

            _occupyDictionary.Remove(process);
        }
    }
}
