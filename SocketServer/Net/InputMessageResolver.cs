using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;

namespace SocketServer.Net {

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
    class InputMessageResolver {
        public delegate void CompletedMessageCallback(byte[] buffer);
        public delegate void MessageDecodeFailCallback(Exception ex, Memory<byte> buffer);

        public event CompletedMessageCallback OnMessageReceive;
        public event MessageDecodeFailCallback OnMessageDecodeFail;

        /// <summary>
        /// 패킷을 읽었을때 전체 메시지 크기
        /// </summary>
        int messageSize = -1;

        /// <summary>
        /// 지금까지 읽은 메세지 크기
        /// </summary>
        int currentPosition = 0;

        /// <summary>
        /// 읽은 메세지를 임시로 저장할 버퍼
        /// </summary>
        readonly byte[] messageBuffer = new byte[Consts.MESSAGE_BUFFER_SIZE];

        void DecodeHeader(Span<byte> buffer) {
            const int minPacketSize = Consts.HEADER_SIZE;
            if (buffer.Length < minPacketSize) {
                throw new Exception($"packet size must > {Consts.HEADER_SIZE} receive:({buffer.Length})");
            }

            if (buffer[0] != Consts.PACKET_BEGIN) {
                throw new Exception($"packet header[0] error {buffer[0]}");
            }

            int len = 0;
            len += buffer[1];
            len += buffer[2] << 8;

            if (len < 0) {
                throw new Exception($"packet length error {len}");
            }

            if (len > messageBuffer.Length) {
                throw new Exception($"message size({len}) > messageBuffer size({messageBuffer.Length})");
            }

            messageSize = len;
        }

        public void OnRawByteReceive(Memory<byte> buffer) {
            try {
                if (messageSize == -1) {
                    DecodeHeader(buffer.Span);
                    buffer = buffer.Slice(Consts.HEADER_SIZE);
                }

                int maxRemainByte = messageBuffer.Length - currentPosition;

                if (currentPosition + buffer.Length > maxRemainByte) {
                    buffer = buffer.Slice(0, maxRemainByte);
                }

                if (buffer.Length > 0) {
                    buffer.CopyTo(messageBuffer.AsMemory(currentPosition));
                    currentPosition += buffer.Length;
                }

                if (currentPosition >= messageSize) {
                    OnMessageReceive(messageBuffer);
                    ClearBuffer();
                }

            } catch (Exception e) {
                Console.WriteLine(e);
                if (buffer.Length > 0) {
                    OnMessageDecodeFail(e, buffer);
                }
            }
        }

        private void ClearBuffer() {
            for (int i = 0; i < Consts.HEADER_SIZE; ++i) {
                messageBuffer[i] = 0;
            }
            currentPosition = 0;
            messageSize = -1;
        }
    }
}
