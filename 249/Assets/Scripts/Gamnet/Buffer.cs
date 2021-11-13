using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gamnet
{
    public class Buffer
    {
        public const int MAX_BUFFER_SIZE = Int16.MaxValue;
        public System.IO.MemoryStream ms;
        public int write_index = 0;
        public int read_index = 0;

        public Buffer()
        {
            ms = new System.IO.MemoryStream();
        }

        public Buffer(Buffer src)
        {
            ms = new System.IO.MemoryStream();
            Copy(src);
        }

        ~Buffer()
        {
            ms.Close();
        }

        public void Copy(byte[] src)
        {
            write_index = 0;
            read_index = 0;
            ms.Position = 0;
            ms.Write(src, 0, src.Length);
            write_index += src.Length;
        }

        public void Copy(Buffer src)
        {
            write_index = src.Size();
            read_index = 0;
            ms.Position = 0;
            ms.Write(src.ms.GetBuffer(), src.read_index, (int)src.Size());
        }

        public void Append(byte[] src)
        {
            ms.Position = write_index;
            ms.Write(src, 0, src.Length);
            write_index += src.Length;
        }

        public void Append(byte[] src, int offset, int length)
        {
            ms.Position = write_index;
            ms.Write(src, offset, length);
            write_index += length;
        }

        public void Clear()
        {
            write_index = 0;
            read_index = 0;
            ms.Position = 0;
        }

        public int Size()
        {
            return write_index - read_index;
        }

        public bool Remove(int size)
        {
            if (write_index < read_index + size)
            {
                return false;
            }
            read_index += size;
            return true;
        }

        public byte[] ToByteArray()
        {
            return ms.GetBuffer();
        }
    }
}
