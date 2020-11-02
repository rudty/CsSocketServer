using System;
using System.Collections.Generic;
using System.Text;
using SocketServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;

namespace SocketServerTest {

    [TestClass]
    public class ClientTest {

        [TestMethod]
        public void TestEcho() {
            using var server = ServerTestHelper.TestServer;
            using var client = ServerTestHelper.TestClient;
            server.UserMessageListener += (Session session, string message, PacketInputStream packetInputStream) => {
                session.Send(CPacket.New.Push(message));
            };
            client.Send(
                CPacket.New
                    .Push((byte)1)
                    .Push("hello"));
            var res = client.ReceiveAny();
            var istream = new PacketInputStream(res);
            var body = istream.NextString();
            Assert.IsTrue(body == "hello"); 
        }

    }
}
