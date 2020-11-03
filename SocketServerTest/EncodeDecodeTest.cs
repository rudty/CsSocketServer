using System;
using System.Collections.Generic;
using System.Text;
using SocketServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SocketServer.Net.IO;

namespace SocketServerTest {
    [TestClass]
    public class EncodeDecodeTest {
        const int value1 = 499791;
        const int value2 = 8899671;

        const string str1 = "hello world";
        const string str2 = "good day";

        CPacketInputStream PacketToInputStream(CPacket p) {
            return new CPacketInputStream(p.Buffer.Slice(Consts.HEADER_SIZE));
        }

        [TestMethod]
        public void CheckInt1() {
            CPacket p = new CPacket();
            p.Push(value1);

            var stream = PacketToInputStream(p);
            var o = stream.NextInt();
            Assert.AreEqual(value1, o);
        }


        [TestMethod]
        public void CheckInt2() {
            CPacket p = new CPacket();
            p.Push(value1);
            p.Push(value2);

            var stream = PacketToInputStream(p);
            var o = stream.NextInt();
            Assert.AreEqual(value1, o);

            o = stream.NextInt();
            Assert.AreEqual(value2, o);
        }

        [TestMethod]
        public void CheckString1() {
            CPacket p = new CPacket();
            p.Push(str1);

            var stream = PacketToInputStream(p);
            var o = stream.NextString();
            Assert.AreEqual(str1, o);

        }

        [TestMethod]
        public void CheckString2() {
            CPacket p = new CPacket();
            p.Push(str1);
            p.Push(str2);

            var stream = PacketToInputStream(p);
            var o = stream.NextString();
            Assert.AreEqual(str1, o);

            o = stream.NextString();
            Assert.AreEqual(str2, o);
        }

        [TestMethod]
        public void CheckIntString1() {
            var p = CPacket.New
                .Push(value1)
                .Push(str1)
                .Push(value2)
                .Push(str2);

            var stream = PacketToInputStream(p);
            int i_out = stream.NextInt();
            Assert.AreEqual(value1, i_out);

            string s_out = stream.NextString();
            Assert.AreEqual(str1, s_out);

            i_out = stream.NextInt();
            Assert.AreEqual(value2, i_out);

            s_out = stream.NextString();
            Assert.AreEqual(str2, s_out);
        }

        class TestObject {
            public int a;
            public string b;
        }

        [TestMethod]
        public void CheckObject() {
            var p = CPacket.New;
            TestObject t = new TestObject();
            t.a = value1;
            t.b = str1;

            p.Push(t);

            var stream = PacketToInputStream(p);
            int i_out = stream.NextInt();
            Assert.AreEqual(value1, i_out);

            string s_out = stream.NextString();
            Assert.AreEqual(str1, s_out);
        }


        class NestedTestObject {
            public int x;
            public TestObject o = new TestObject();
        }

        [TestMethod]
        public void CheckNestedObject() {
            NestedTestObject o = new NestedTestObject();
            o.x = value1;
            o.o.a = value2;
            o.o.b = str1;

            var p = CPacket.New
                    .Push(o);

            var stream = PacketToInputStream(p);

            int i_out = stream.NextInt();
            Assert.AreEqual(value1, i_out);

            i_out = stream.NextInt();
            Assert.AreEqual(value2, i_out);

            string s_out = stream.NextString();
            Assert.AreEqual(str1, s_out);
        }

        [TestMethod]
        public void CheckObjectIO() {
            var o = new NestedTestObject {
                x = value1,
                o = new TestObject {
                    a = value2,
                    b = str1,
                }
            };

            var p = CPacket.New
                    .Push(o);

            var stream = PacketToInputStream(p);

            var o_out = stream.Next<NestedTestObject>();
            Assert.AreEqual(o_out.x, value1);
            Assert.AreEqual(o_out.o.a, value2);
            Assert.AreEqual(o_out.o.b, str1);

        }
    }
}
