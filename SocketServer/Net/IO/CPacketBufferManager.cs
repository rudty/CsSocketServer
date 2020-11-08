using System;
using System.Collections.Generic;

namespace SocketServer.Net.IO {
    public class CPacketBufferManager {
        const int ALLOCATE_BUFFER_COUNT = 5;
        private static object mutex = new object();
        private static readonly Stack<Memory<byte>> pool = new Stack<Memory<byte>>();

        public static Memory<byte> Obtain() {
            const int bufSize = CPacket.MESSAGE_BUFFER_SIZE;
            const int bufCount = ALLOCATE_BUFFER_COUNT;

            lock (mutex) {
                if (pool.Count == 0) {
                    var newBuffer = new byte[bufSize * bufCount];
                    for (int i = 0; i < bufSize * bufCount; i += bufSize) {
                        pool.Push(newBuffer.AsMemory(i, bufSize));
                    }
                }
                return pool.Pop();
            }
        }

        public static void Recycle(Memory<byte> buffer) {
            if (buffer.Length != CPacket.MESSAGE_BUFFER_SIZE) {
                throw new ArgumentException($"${nameof(buffer)} Length must  {CPacket.MESSAGE_BUFFER_SIZE} but {buffer.Length}");
            }
            lock (mutex) {
                pool.Push(buffer); 
            }
        }
    }
}
