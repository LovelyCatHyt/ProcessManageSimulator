using System;
using NUnit.Framework;
using ProcessManageCore.Entity;
using ProcessManageCore.Singleton;

namespace ProcessManageCore.Test
{
    public class OSTest
    {
        private OS os;
        [SetUp]
        public void TestSetUp()
        {
            os = new OS(4, 1024);
        }

        [Test]
        public void TestInit()
        {
            OS os = new OS(4, 1024);
            Console.Write(os);
        }

        [Test]
        public void TestWaitForMemoryList()
        {
            os.CleanUp();
            os.AddNewProcess(ProcessFactory.CreateProcess(ProcessType.System, "Test1", 1, 1024, true, new int[0],
                new int[0]));
            os.AddNewProcess(ProcessFactory.CreateProcess(ProcessType.System, "Test1", 1, 1024, true, new int[0],
                new int[0]));
            os.Update(3);
        }

        [Test]
        public void TestTooLargeMemoryRequest()
        {
            os.CleanUp();
            Assert.Catch<ArgumentOutOfRangeException>(() => os.AddNewProcess(ProcessFactory.CreateProcess(ProcessType.System, "Test1", 1, 1025,
                true, new int[0],
                new int[0])));
        }
    }
}