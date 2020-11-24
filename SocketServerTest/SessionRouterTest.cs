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
    public class SessionRouterTest {

        [Timeout(6000)]
        [TestMethod]
        public void TestEcho() {
            Random r = new Random();
            int port = r.Next(20000, 30000);
            var server = new SessionRouter();
            //server.ListenAndServe("127.0.0.1", port);
            Task.Run(() => server.ListenAndServe("127.0.0.1", port));
            Thread.Sleep(1000);
            var client = new LocalTestClient(port);

            Hello h = new Hello();
            h.Value = 8;
            client.Send(CPacket.NewSend
                            .Add("hello")
                            .Add(h));


            var receive = client.ReceivePacket();

            var resHello = receive.Next(Hello.Parser);
            Assert.AreEqual(resHello.Value, 16);
        }
    }
}
