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

        [Test]
        public void TestProcessDependencies()
        {
            os.CleanUp();
            Process a;
            Process b;
            Process c;
            os.AddNewProcess(a = ProcessFactory.CreateIndependentProcess(ProcessType.System, "TestA", 1, 1));
            os.AddNewProcess(b = ProcessFactory.CreateIndependentProcess(ProcessType.System, "TestB", 1, 1));
            os.AddNewProcess(c = ProcessFactory.CreateIndependentProcess(ProcessType.System, "TestC", 1, 1));
            Process.SetProcessDependence(b, a);
            Process.SetProcessDependence(c, b);
            os.Update();
            Console.WriteLine(os);
            Assert.True(!os.CPUAllFree);
            os.Update();
            Console.WriteLine(os);
            Assert.True(!os.CPUAllFree);
            os.Update();
            Console.WriteLine(os);
            Assert.True(!os.CPUAllFree);
            os.Update();
            Console.WriteLine(os);
            Assert.True(os.CPUAllFree);
        }
    }
}