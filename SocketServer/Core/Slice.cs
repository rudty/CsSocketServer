using System;

namespace SocketServer.Core {

    /// <summary>
    /// ProtoBuf 와 C# 내부 에서 버퍼로 사용할 수 있는 서브 배열 클래스
    /// 원래는 매우 큰 byte[] 를 잘라서 Memory<byte> 로 사용했지만 
    /// 이 형식은 현재 ProtoBuf 에서 지원하지 않아
    /// Memory 와 유사한 클래스를 제작함
    /// </summary>
    public class Slice {
        public byte[] Buffer { get; private set; }
        public int Offset { get; private set; }
        public int Length { get; private set; }


        /// <summary>
        /// 이 byte[] 로 Slice를 제작함
        /// Slice 객체는 byte 배열의 offset 부터 length 까지만 사용할 것
        /// </summary>
        /// <param name="buffer">byte 배열</param>
        /// <param name="offset">사용할 시작점</param>
        /// <param name="length">offset 부터 몇개를 사용할 것인지</param>
        public Slice(byte[] buffer, int offset, int length) {
            this.Buffer = buffer;
            this.Offset = offset;
            this.Length = length;
            
            if (offset < 0) {
                throw new ArgumentException($"Offset < 0 Offset:({offset})");
            }

            if (length <= 0) {
                throw new ArgumentException($"Length <= 0 Length:({length})");
            }

            if (buffer.Length < (offset + length)) {
                throw new ArgumentException($"Buffer.Length <= Offset + Length Buffer.Length:{buffer.Length}, Offset:{offset}, Length{length}");
            }
        }


        /// <summary>
        /// 해당 Slice 보다 같거나 작은 범위의 Slice를 만듭니다
        /// </summary>
        /// <param name="s">예전 슬라이스</param>
        /// <param name="subOffset">새로 지정할 offset</param>
        /// <param name="subLength">새로 지정할 길이</param>
        public Slice(Slice s, int subOffset, int subLength)
            : this(s.Buffer, subOffset, subLength) {
            if (s.Offset > subOffset) {
                throw new ArgumentException($"old Slice offset > new Slice offset old offset:{s.Offset}, new offset:{subOffset}");
            }

            if (s.Length < subLength) {
                throw new ArgumentException($"old Slice length < new Slice length old length:{s.Length}, new length:{subLength}");
            }
        }

        /// <summary>
        /// 이 byte[] 를 모두 사용해서 Slice 를 만듭니다
        /// </summary>
        /// <param name="b">사용할 버퍼</param>
        public Slice(byte[] b)
            :this(b, 0, b.Length) {
        }

         public byte this[int i] {
            get { 
                if (Offset + i >= Length) {
                    throw new IndexOutOfRangeException($"Length:{Length}");
                }
                return Buffer[Offset + i]; 
            }
            set {
                if (Offset + i >= Length) {
                    throw new IndexOutOfRangeException($"Length:{Length}");
                }
                Buffer[Offset + i] = value;
            }
        }

        public Slice this[Range range] {
            get {
                return new Slice(this, range.Start.Value, range.End.Value);
            }
        }


    }
}
