using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SocketServer;

namespace SocketServerTest {
    [TestClass]
    public class MessageTaskRunnerTest {
        [TestMethod]
        public void TestQueue() {
            MessageTaskRunner r = new MessageTaskRunner();
            r.Add(() => Console.WriteLine("a"));
            r.Add(() => Console.WriteLine("b"));
            r.Add(() => Console.WriteLine("c"));
            r.Add(() => Console.WriteLine("d"));
            r.Wait();
        }
    }
}
