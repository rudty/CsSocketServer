using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.IO;
using SocketServer.Core;
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
    class CPacketStreamReader {
        readonly Stream stream;

        public CPacketStreamReader(Stream stream) {
            this.stream = stream;
        }

        /// <summary>
        /// 헤더를 읽고 메세지 크기를 반환함
        /// 헤더에 뭔가 더 들어가면 구조체를 반환하게 할 것
        /// </summary>
        /// <param name="buffer">첫 읽은 4바이트 이상의 버퍼</param>
        /// <returns>메세지의 길이</returns>
        int DecodeHeader(byte[] buffer) {
            const int maxPacketSize = CPacket.MESSAGE_BUFFER_SIZE - CPacket.HEADER_SIZE;

            if (buffer[0] != CPacket.PACKET_BEGIN) {
                throw new PacketDecodeFailException(
                    buffer,
                    0,
                    CPacket.HEADER_SIZE,
                $"packet header[0] error {buffer[0]}"); ;
            }

            int len = 0;
            len += buffer[1];
            len += buffer[2] << 8;

            if (len <= 0) {
                throw new PacketDecodeFailException(
                    buffer,
                    0,
                    CPacket.HEADER_SIZE,
                    $"packet length error {len}");
            }

            if (len > maxPacketSize) {
                throw new PacketDecodeFailException(
                    buffer,
                    0,
                    CPacket.HEADER_SIZE,
                    $"message size({ len }) > messageBuffer size({ maxPacketSize })");
            }

            return len;
        }


        /// <summary>
        /// 클라이언트로부터 패킷을 받습니다
        /// length 까지 받으며 받지 못한다면 계속 받기를 시도합니다
        /// </summary>
        /// <param name="packet">저장할 버퍼</param>
        /// <param name="length">길이</param>
        /// <returns>정상적으로 읽기를 완료하였음</returns>
        async ValueTask<bool> ReceiveNext(CPacket packet, int length) {
            int position = 0;
            int readLength = length;
            while (position < length) {
                int len = await stream.ReadAsync(packet, readLength);
                if (len <= 0) {
                    // 클라이언트에서 연결을 끊었을때
                    return false;
                }

                position += len;
                readLength -= len;
            }
            return true;
        }


        /// <summary>
        /// 클라이언트로부터 패킷을 읽습니다
        /// 패킷은 헤더 + 본문으로 이루어져
        /// 두번 읽게 됩니다
        /// </summary>
        /// <returns>읽은 완전한 패킷</returns>
        public async Task<CPacket> ReceiveAsync() {
            CPacket packet = CPacket.NewReceive;

            if (false == await ReceiveNext(packet, CPacket.HEADER_SIZE)) {
                return null;
            }

            // 처음 HEADER를 받을떄 최대 header 까지만 받게
            // 총 읽을 길이 (메세지 길이 + 헤더 길이)
            int messageSize = DecodeHeader(packet.Buffer);


            if (false == await ReceiveNext(packet, messageSize)) {
                return null;
            }

            packet.Position = CPacket.HEADER_SIZE;
            return packet;
        }
    }
}
