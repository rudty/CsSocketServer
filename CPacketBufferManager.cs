using System;
using System.Collections.Generic;

namespace SocketServer {
    class CPacketBufferManager {
        private static object mutex = new object();

        private static Stack<Memory<byte>> pool = new Stack<Memory<byte>>();

        private const int DEFAULT_BUFFER_SIZE = 1024;
        private const int DEFAULT_ALLOCAT_BUFFER_COUNT = 5;

        internal static Memory<byte> Obtain() {
            lock (mutex) {
                if (pool.Count == 0) {
                    var newBuffer = new byte[DEFAULT_BUFFER_SIZE * DEFAULT_ALLOCAT_BUFFER_COUNT];
                    for (int i = 0; i < DEFAULT_ALLOCAT_BUFFER_COUNT * DEFAULT_BUFFER_SIZE; i += DEFAULT_BUFFER_SIZE) {
                        pool.Push(newBuffer.AsMemory(i, DEFAULT_BUFFER_SIZE));
                    }
                }
                return pool.Pop();
            }
        }

        internal static void Recycle(Memory<byte> buffer) {
            if (buffer.Length != DEFAULT_BUFFER_SIZE) {
                throw new ArgumentException($"${nameof(buffer)} Length must  {DEFAULT_BUFFER_SIZE} but {buffer.Length}");
            }
            lock (mutex) {
                pool.Push(buffer); 
            }
        }
    }
}
