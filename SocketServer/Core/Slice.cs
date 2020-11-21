using System;

namespace SocketServer.Core {

    /// <summary>
    /// ProtoBuf 와 C# 내부 에서 버퍼로 사용할 수 있는 서브 배열 클래스
    /// 원래는 매우 큰 T[] 를 잘라서 Memory<T> 로 사용했지만 
    /// 이 형식은 현재 ProtoBuf 에서 지원하지 않아
    /// Memory 와 유사한 클래스를 제작함
    /// </summary>
    public class Slice<T> {
        public T[] Buffer { get; internal set; }
        public int Offset { get; private set; }
        public int Length { get; private set; }

        public bool IsEmpty {
            get {
                return Buffer != null;
            }
        }

        /// <summary>
        /// 이 T[] 로 Slice를 제작함
        /// Slice 객체는 T 배열의 offset 부터 length 까지만 사용할 것
        /// </summary>
        /// <param name="buffer">T 배열</param>
        /// <param name="offset">사용할 시작점</param>
        /// <param name="length">offset 부터 몇개를 사용할 것인지</param>
        public Slice(T[] buffer, int offset, int length) {
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
        public Slice(Slice<T> s, int subOffset, int subLength)
            : this(s.Buffer, subOffset, subLength) {
            if (s.Offset > subOffset) {
                throw new ArgumentException($"old Slice offset > new Slice offset old offset:{s.Offset}, new offset:{subOffset}");
            }

            if (s.Length < subLength) {
                throw new ArgumentException($"old Slice length < new Slice length old length:{s.Length}, new length:{subLength}");
            }
        }

        /// <summary>
        /// 이 T[] 를 모두 사용해서 Slice 를 만듭니다
        /// </summary>
        /// <param name="b">사용할 버퍼</param>
        public Slice(T[] b)
            :this(b, 0, b.Length) {
        }

         public T this[int i] {
            get { 
                return Buffer[Offset + i]; 
            }
            set {
                 Buffer[Offset + i] = value;
            }
        }

        public Slice<T> this[Range range] {
            get {
                return new Slice<T>(this, Offset + range.Start.Value, range.End.Value);
            }
        }

        public Memory<T> AsMemory() {
            return Buffer.AsMemory(Offset, Length);
        }

        public Memory<T> AsMemory(int start, int count) {
            if (count > Length) {
                throw new ArgumentException($"AsMemory() count > slice.length count:{count}, slice.length:{Length}");
            }
            return Buffer.AsMemory(Offset + start, count);
        }
    }
}
