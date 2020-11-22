using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace SocketServer.Core {
    internal class CPacketMemoryPool {
        private const int BUFFER_COUNT = 5000;
        private static readonly ArrayPool<byte> pool = ArrayPool<byte>.Create(
            CPacket.MESSAGE_BUFFER_SIZE,
            BUFFER_COUNT);

        internal static byte[] Obtain() {
            return pool.Rent(CPacket.MESSAGE_BUFFER_SIZE);
        }

        internal static void Recycle(byte[] b) {
            if (b.Length != CPacket.MESSAGE_BUFFER_SIZE) {
                throw new ArgumentException($"array size must {CPacket.MESSAGE_BUFFER_SIZE}");
            }
            pool.Return(b);
        }
     }
}
