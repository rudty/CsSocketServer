using System;
using System.Collections.Generic;
using System.Text;

namespace SocketServer {
    class CPacketBufferManager {
        private static object mutex = new object();
        private static Stack<CPacket> pool = new Stack<CPacket>();
        private static int poolCapacity = 0;

        public static void initialize(int capacity) {
            poolCapacity = capacity;
            allocate();
        }

        private static void allocate() {
            for (int i = 0; i < poolCapacity; i++) {
                pool.Push(new CPacket());
            }
        }

        public static CPacket pop() {
            lock (mutex) {
                if (pool.Count <= 0) {
                    allocate();
                }
                return pool.Pop();
            }
        }

        public static void push(CPacket packet) {
            lock (mutex) {
                pool.Push(packet);
            }
        }
    }
}
