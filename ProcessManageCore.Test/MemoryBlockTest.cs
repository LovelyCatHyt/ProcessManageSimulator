using NUnit.Framework;
using ProcessManageCore.Entity;
using ProcessManageCore.Exception;
using ProcessManageCore.Singleton;

namespace ProcessManageCore.Test
{
    class MemoryBlockTest
    {
        [Test]
        public void TestDistribute()
        {
            MemoryBlock block = new MemoryBlock { length = 1024, occupied = false, startPos = 0 };
            var block2 = block.Distribute(256);
            Assert.AreEqual(256, block.startPos);
            Assert.AreEqual(0, block2.startPos);
            Assert.AreEqual(1024 - 256, block.length);
        }

        [Test]
        public void TestAllocate()
        {
            MemoryManager mgr = new MemoryManager(1024);
            mgr.RequestMemory(512, 0);
            Assert.NotNull(mgr.RequestMemory(256, 0));
            Assert.IsNull(mgr.RequestMemory(512, 0));
            mgr.RequestMemory(256, 0);
            Assert.IsNull(mgr.RequestMemory(0, 0));
            Assert.IsNull(mgr.RequestMemory(1, 0));
        }

        [Test]
        public void TestGC()
        {
            MemoryManager mgr = new MemoryManager(1024);
            // 交替占用
            mgr.RequestMemory(128, 0);
            mgr.RequestMemory(128, 1);
            mgr.RequestMemory(128, 0);
            mgr.RequestMemory(128, 1);

            Assert.AreEqual(512, mgr.RemainedSpace);
            mgr.ReleaseMemoryOfProcess(1);
            // 检查是否正确归还
            Assert.AreEqual(512 + 256, mgr.RemainedSpace);
            // 检查是否正确合并
            Assert.AreEqual(512 + 128, mgr.MaxBlock);
            mgr.ReleaseMemoryOfProcess(0);
            Assert.AreEqual(1024, mgr.RemainedSpace);
            Assert.AreEqual(1024, mgr.MaxBlock);
        }
    }
}
