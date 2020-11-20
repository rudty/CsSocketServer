using SocketServer.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer.Net.IO {
    public static class CPacketReadExtension {
        /// <summary>
        /// Stream 으로부터 length 만큼 읽어 packet 에 저장합니다
        /// stream 의 상황에 따라 length 와 실제로 읽은 수가 일치하지는 않을 수도 있습니다 
        /// </summary>
        /// <param name="packet">패킷</param>
        /// <param name="stream">데이터를 읽을 stream</param>
        /// <param name="length">길이</param>
        /// <returns></returns>
        public static async Task<int> ReadFromAsync(this CPacket packet, Stream stream, int length) {
            var s = packet.Buffer;
            int position = packet.Position;
            if (position + length > s.Length) {
                throw new CPacketOverflowException($"position + length > buffer.length {position + length} > {s.Length}");
            }
            int len = await stream.ReadAsync(s.Buffer, s.Offset, length);
            packet.Position += len;
            return len;

        }
    }
}
