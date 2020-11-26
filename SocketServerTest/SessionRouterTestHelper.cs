using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using SocketServer.Net.IO;
using SocketServerTest;
using SocketServer.Core;
using SessionServer;
namespace SolutionTest {
    [TestClass]
    public class SessionRouterTestHelper {
        [ThreadStatic]
        public static SessionRouter Router;

        [ThreadStatic]
        public static int Port;

        [ThreadStatic]
        public static LocalTestClient Client;

        [TestInitialize]
        public void TestInitialize() {
            Random r = new Random();
            int port = r.Next(20000, 30000);
            Environment.SetEnvironmentVariable("PORT", port.ToString());
            Router = new SessionRouter();
            Port = port;
            Client = new LocalTestClient(SessionRouterTestHelper.Port);
        }

        [TestCleanup]
        public void TestCleanup() {
            Router.Dispose();
            Client.Dispose();
        }
    }
}
