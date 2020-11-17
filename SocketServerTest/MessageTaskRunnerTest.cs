using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SocketServer.TaskRunner;

namespace SocketServerTest {
    [TestClass]
    public class MessageTaskRunnerTest {
        [TestMethod]
        public async Task TestQueue() {
            SingleTaskRunner r = new SingleTaskRunner();
            r.Add(() => Console.WriteLine("a"));
            r.Add(() => Console.WriteLine("b"));
            r.Add(() => Console.WriteLine("c"));
            r.Add(() => Console.WriteLine("d"));
            await r.Wait();
            r.Add(() => Console.WriteLine("e"));
            r.Add(() => Console.WriteLine("f"));
            await r.Wait();
        }
    }
}
