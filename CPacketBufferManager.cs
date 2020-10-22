using System;
using System.Collections.Generic;

namespace SocketServer {
    class CPacketBufferManager {
        private static object mutex = new object();
        private static Stack<Memory<byte>> pool = new Stack<Memory<byte>>();

        internal static Memory<byte> Obtain() {
            const int bufSize = Consts.MESSAGE_BUFFER_SIZE;
            const int bufCount = Consts.ALLOCATE_BUFFER_COUNT;

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

        internal static void Recycle(Memory<byte> buffer) {
            if (buffer.Length != Consts.MESSAGE_BUFFER_SIZE) {
                throw new ArgumentException($"${nameof(buffer)} Length must  {Consts.MESSAGE_BUFFER_SIZE} but {buffer.Length}");
            }
            lock (mutex) {
                pool.Push(buffer); 
            }
        }
    }
}
