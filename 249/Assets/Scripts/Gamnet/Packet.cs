using System;
using System.Text;

namespace Gamnet
{
    public class Packet
    {
        public const int OFFSET_LENGTH = 0;
        public const int OFFSET_MSGSEQ = 2;
        public const int OFFSET_MSGID = 6;
        public const int OFFSET_RELIABLE = 10;
        public const int OFFSET_RESERVED = 11;
        public const int HEADER_SIZE = 12;

        public readonly Buffer buffer;

        public ushort Length
        {
            set
            {
                byte[] bytes = BitConverter.GetBytes(value);
                buffer.ms.Position = OFFSET_LENGTH;
                buffer.ms.Write(bytes, 0, bytes.Length);
            }
            get
            {
                return BitConverter.ToUInt16(buffer.ms.GetBuffer(), OFFSET_LENGTH);
            }
        }

        public uint Seq
        {
            set
            {
                byte[] bytes = BitConverter.GetBytes(value);
                buffer.ms.Position = OFFSET_MSGSEQ;
                buffer.ms.Write(bytes, 0, bytes.Length);
            }
            get
            {
                return BitConverter.ToUInt32(buffer.ms.GetBuffer(), OFFSET_MSGSEQ);
            }
        }

        public uint Id
        {
            set
            {
                byte[] bytes = BitConverter.GetBytes(value);
                buffer.ms.Position = OFFSET_MSGID;
                buffer.ms.Write(bytes, 0, bytes.Length);
            }
            get
            {
                return BitConverter.ToUInt32(buffer.ms.GetBuffer(), OFFSET_MSGID);
            }
        }

        public bool IsReliable
        {
            set
            {
                byte[] bytes = BitConverter.GetBytes(value);
                buffer.ms.Position = OFFSET_RELIABLE;
                buffer.ms.Write(bytes, 0, bytes.Length);
            }
            get
            {
                return BitConverter.ToBoolean(buffer.ms.GetBuffer(), OFFSET_RELIABLE);
            }
        }

        public Packet()
        {
            this.buffer = new Buffer();
            Clear();
        }

        public Packet(Buffer buffer)
        {
            this.buffer = buffer;
        }

        public void Clear()
        {
            buffer.write_index = HEADER_SIZE;
            buffer.read_index = 0;
            buffer.ms.Position = 0;
            Length = HEADER_SIZE;
        }

        public void Write(byte[] src)
        {
            buffer.ms.Position = buffer.write_index;
            buffer.ms.Write(src, 0, src.Length);
            buffer.write_index += src.Length;
            Length = (ushort)(buffer.write_index);
        }

        public void Write(Boolean src) { byte[] bytes = BitConverter.GetBytes(src); Write(bytes); }
        public void Write(Int16 src) { byte[] bytes = BitConverter.GetBytes(src); Write(bytes); }
        public void Write(UInt16 src) { byte[] bytes = BitConverter.GetBytes(src); Write(bytes); }
        public void Write(Int32 src) { byte[] bytes = BitConverter.GetBytes(src); Write(bytes); }
        public void Write(UInt32 src) { byte[] bytes = BitConverter.GetBytes(src); Write(bytes); }
        public void Write(Int64 src) { byte[] bytes = BitConverter.GetBytes(src); Write(bytes); }
        public void Write(UInt64 src) { byte[] bytes = BitConverter.GetBytes(src); Write(bytes); }
        public void Write(string src) { byte[] bytes = Encoding.UTF8.GetBytes(src); Write(bytes); }

        public int BodySize()
        {
            return Length - HEADER_SIZE;
        }

        public byte[] ToByteArray()
        {
            return buffer.ms.GetBuffer();
        }
    }
}
