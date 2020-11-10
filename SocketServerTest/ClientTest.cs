using Microsoft.VisualStudio.TestTools.UnitTesting;
using SocketServer;
using SocketServer.Net;
using SocketServer.Net.IO;
using System.Threading.Tasks;

namespace SocketServerTest {

    [TestClass]
    public class ClientTest {

        [TestMethod]
        public void TestEcho() {
            using var server = ServerTestHelper.TestServer;
            using var client = ServerTestHelper.TestClient;
            server.AddEventListener("hello", (Session session, CPacket p) => {
                session.Send(CPacket.New.Add("hi"));
                return Task.CompletedTask;
            });
            client.Send(
                CPacket.New
                    .Add((byte)1)
                    .Add("hello"));
            var res = client.ReceivePacket();
            var body = res.NextString();
            Assert.IsTrue(body == "hi");
        }
    }
}
