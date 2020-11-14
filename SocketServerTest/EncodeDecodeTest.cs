using Microsoft.VisualStudio.TestTools.UnitTesting;
using SocketServer.Core;
using SocketServer.Net.IO;

namespace SocketServerTest {
    [TestClass]
    public class EncodeDecodeTest {
        const int value1 = 499791;
        const int value2 = 8899671;

        const string str1 = "hello world";
        const string str2 = "good day";

        [TestMethod]
        public void CheckInt1() {
            CPacket p = new CPacket();
            p.Add(value1);

            p.MoveToFirst();
            var o = p.NextInt();
            Assert.AreEqual(value1, o);
        }


        [TestMethod]
        public void CheckInt2() {
            CPacket p = new CPacket();
            p.Add(value1);
            p.Add(value2);

            p.MoveToFirst();
            var o = p.NextInt();
            Assert.AreEqual(value1, o);

            o = p.NextInt();
            Assert.AreEqual(value2, o);
        }

        [TestMethod]
        public void CheckString1() {
            CPacket p = new CPacket();
            p.Add(str1);

            p.MoveToFirst();
            var o = p.NextString();
            Assert.AreEqual(str1, o);

        }

        [TestMethod]
        public void CheckString2() {
            CPacket p = new CPacket();
            p.Add(str1);
            p.Add(str2);

            p.MoveToFirst();
            var o = p.NextString();
            Assert.AreEqual(str1, o);

            o = p.NextString();
            Assert.AreEqual(str2, o);
        }

        [TestMethod]
        public void CheckIntString1() {
            var p = CPacket.New
                .Add(value1)
                .Add(str1)
                .Add(value2)
                .Add(str2);

            p.MoveToFirst();
            int i_out = p.NextInt();
            Assert.AreEqual(value1, i_out);

            string s_out = p.NextString();
            Assert.AreEqual(str1, s_out);

            i_out = p.NextInt();
            Assert.AreEqual(value2, i_out);

            s_out = p.NextString();
            Assert.AreEqual(str2, s_out);
        }
    }
}
