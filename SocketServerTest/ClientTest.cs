﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SocketServer;
using SocketServer.Net;
using SocketServer.Net.IO;
using System.Threading.Tasks;
using SocketServer.Core;
using SessionServer;
using System.Threading;

namespace SocketServerTest {

    [TestClass]
    public class ClientTest {

        [Timeout(5000)]
        [TestMethod]
        public void TestEcho() {
            using var server = ServerTestHelper.TestServer;
            using var client = ServerTestHelper.TestClient;
            server.AddEventListener("hello", (Request req, Response res) => {
                req.Session.Send(CPacket.NewSend.Add("hi"));
                return Task.CompletedTask;
            });
            client.Send(
                CPacket.NewSend
                    .Add("hello"));
            var res = client.ReceivePacket();
            var body = res.NextString();
            Assert.IsTrue(body == "hi");
        }

        [Timeout(5000)]
        [TestMethod]
        public void TestHello() {
            using var server = ServerTestHelper.TestServer;
            using var client = ServerTestHelper.TestClient;
            server.AddEventListener("hello", (Request req, Response res) => {
                Hello h = req.Packet.Next(Hello.Parser);
                h.Value += 1;
                req.Session.Send(CPacket.NewSend.Add(h));
                return Task.CompletedTask;
            });
            Hello h = new Hello();
            h.Value = 1;
            client.Send(
                CPacket.NewSend
                    .Add("hello")
                    .Add(h));
            var res = client.ReceivePacket();
            var h2 = res.Next(Hello.Parser);
            System.Console.WriteLine(h2.Value);
        }
    }
}
