using System;
using NUnit.Framework;
using ProcessManageCore.Singleton;

namespace ProcessManageCore.Test
{
    public class OSTest
    {
        [Test]
        public void TestInit()
        {
            OS os = new OS(4, 1024);
            Console.Write(os);
        }
    }
}