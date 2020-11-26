using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SessionServer;
using SocketServer;
using SocketServer.Core;
using SocketServer.Net;
using SocketServer.Net.IO;
using SocketServerTest;

namespace SolutionTest {
    [TestClass]
    public class SessionRouterTest: SessionRouterTestHelper  {

        [Timeout(6000)]
        [TestMethod]
        public void TestEcho() {
            Hello h = new Hello();
            h.Value = 8;
            Client.Send(CPacket.NewSend
                            .Add("hello")
                            .Add(h));
            var receive = Client.ReceivePacket();

            var resHello = receive.Next(Hello.Parser);
            Assert.AreEqual(resHello.Value, 16);
        }
    }
}
