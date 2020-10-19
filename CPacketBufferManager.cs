using System;
using System.Collections.Generic;
using System.Text;

namespace SocketServer {
    class CPacketBufferManager {
        private static object mutex = new object();

        private static Stack<byte[]> pool = new Stack<byte[]>();

        private const int DEFAULT_BUFFER_SIZE = 1024;

        internal static byte[] Obtain() {
            lock (mutex) {
                if (pool.Count == 0) {
                    return new byte[DEFAULT_BUFFER_SIZE];
                }
                return pool.Pop();
            }
        }

        internal static void Recycle(byte[] buffer) {
            if (buffer.Length != DEFAULT_BUFFER_SIZE) {
                throw new ArgumentException($"${nameof(buffer)} Length must  {DEFAULT_BUFFER_SIZE} but {buffer.Length}");
            }
            lock (mutex) {
                pool.Push(buffer);
            }
        }
    }
}
