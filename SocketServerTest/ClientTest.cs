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
            server.AddEventListener("hello", (Session session, CPacket p) => {
                session.Send(CPacket.New.Add("hi"));
            });
            client.Send(
                CPacket.New
                    .Add((byte)1)
                    .Add("hello"));
            var res = client.ReceivePacket();
            var body = res.NextString();
            Assert.IsTrue(body == "hi");
        }

        [TestMethod]
        public void TestLogin() {

        }

    }
}
