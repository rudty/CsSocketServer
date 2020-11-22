using SocketServer.Core;
using System.IO;
using System.Threading.Tasks;

namespace SocketServer.Net.IO {
    public static class StreamCPacketReadWriteExtension {
        /// <summary>
        /// Stream 으로부터 length 만큼 읽어 packet 에 저장합니다
        /// stream 의 상황에 따라 length 와 실제로 읽은 수가 일치하지는 않을 수도 있습니다 
        /// </summary>
        /// <param name="packet">패킷</param>
        /// <param name="stream">데이터를 읽을 stream</param>
        /// <param name="length">길이</param>
        /// <returns></returns>
        public static async ValueTask<int> ReadAsync(this Stream stream, CPacket packet, int length) {
            var s = packet.Buffer;
            int position = packet.Position;
            if (position + length > s.Length) {
                throw new CPacketOverflowException($"position + length > buffer.length {position + length} > {s.Length}");
            }
            int len = await stream.ReadAsync(s, position, length);
            packet.Position += len;
            return len;
        }

        /// <summary>
        /// Stream CPacket 추가
        /// </summary>
        /// <param name="stream">데이터를 쓸 Stream</param>
        /// <param name="packet">보낼 패킷</param>
        /// <returns></returns>
        public static Task WriteAsync(this Stream stream, CPacket packet) {
            var s = packet.Buffer;
            int position = packet.Position;
            return stream.WriteAsync(s, 0, position);

        }
    }
}
