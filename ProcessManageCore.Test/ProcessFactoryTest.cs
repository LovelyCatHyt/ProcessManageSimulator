using NUnit.Framework;
using ProcessManageCore.Entity;
using ProcessManageCore.Exception;
using ProcessManageCore.Singleton;

namespace ProcessManageCore.Test
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
            // nothing to setup YET
        }

        [Test]
        public void TestProcessCreate()
        {
            var p= ProcessFactory.CreateProcess(ProcessType.Kernel, "Kernel", 0x7fffffff, true, null, null);
            Assert.AreEqual(100, p.PID);
        }

        [Test]
        public void TestProcessNotFound()
        {
            Assert.Catch<ProcessNotFoundException>(() => ProcessTable.GetProcess(233));
            Assert.Catch<ProcessNotFoundException>(() => ProcessTable.RemoveProcess(233));
        }
    }
}