using SocketServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SocketServer.Net.IO;

namespace SocketServerTest {

    [TestClass]
    public class ClientTest {

        [TestMethod]
        public void TestEcho() {
            using var server = ServerTestHelper.TestServer;
            using var client = ServerTestHelper.TestClient;
            server.UserMessageListener += (Session session, string message, CPacketInputStream packetInputStream) => {
                session.Send(CPacket.New.Push(message));
            };
            client.Send(
                CPacket.New
                    .Push((byte)1)
                    .Push("hello"));
            var res = client.ReceiveAny();
            var istream = new CPacketInputStream(res);
            var body = istream.NextString();
            Assert.IsTrue(body == "hello");
        }

        [TestMethod]
        public void TestLogin() {

        }

    }
}
