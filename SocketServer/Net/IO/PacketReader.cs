using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SocketServer.Net.IO {

    /// <summary>
    /// TCP 로부터 패킷한번에 정상적으로 들어오지 않고 
    /// 잘려서 들어왔을때 해결
    /// 참고) 
    /// 패킷 구조는 
    /// byte[0] = PACKET_BEGIN
    /// byte[1] = (byte)길이
    /// byte[2] = (byte)(길이 << 8)
    /// byte[3:] 부터는 길이만큼의 내용
    /// </summary>
    class PacketReader {
        readonly Socket socket;

        public PacketReader(Socket socket) {
            this.socket = socket;
        }

        /// <summary>
        /// 헤더를 읽고 메세지 크기를 반환함
        /// 헤더에 뭔가 더 들어가면 구조체를 반환하게 할 것
        /// </summary>
        /// <param name="buffer">첫 읽은 4바이트 이상의 버퍼</param>
        /// <returns>메세지의 길이</returns>
        int DecodeHeader(Span<byte> buffer) {
            const int maxPacketSize = CPacket.MESSAGE_BUFFER_SIZE - CPacket.HEADER_SIZE;

            if (buffer[0] != CPacket.PACKET_BEGIN) {
                throw new Exception($"packet header[0] error {buffer[0]}");
            }

            int len = 0;
            len += buffer[1];
            len += buffer[2] << 8;

            if (len <= 0) {
                throw new Exception($"packet length error {len}");
            }

            if (len > maxPacketSize) {
                throw new Exception($"message size({ len }) > messageBuffer size({ maxPacketSize })");
            }

            return len;
        }


        async ValueTask<bool> ReceiveNext(Memory<byte> buf, int position, int length) {
            // 처음 HEADER를 받을떄 최대 header 까지만 받게
            while (position < length) {
                var headerBuffer = buf.Slice(position, length - position);
                int len = await socket.ReceiveAsync(headerBuffer, SocketFlags.None);

                if (len <= 0) {
                    // 클라이언트에서 연결을 끊었을때
                    return false;
                }

                position += len;
            }

            return true;
        } 

        public async Task<CPacket> ReceiveAsync() {
            var buf = CPacketBufferManager.Obtain();
            if (false == await ReceiveNext(buf, 0, CPacket.HEADER_SIZE)) {
                return null;
            }

            // 처음 HEADER를 받을떄 최대 header 까지만 받게
            // 총 읽을 길이 (메세지 길이 + 헤더 길이)
            int messageSize = DecodeHeader(buf.Span);
            messageSize += CPacket.HEADER_SIZE;


            if (false == await ReceiveNext(buf, CPacket.HEADER_SIZE, messageSize)) {
                return null;
            }
      
            return new CPacket(buf);
        }
    }
}
