using System;
using System.Text;

namespace RPXReader
{
    public class ValueRead
    {
        public bool IsSameEncoding { private set; get; }

        public ValueRead(byte fileDataEncoding)
        {
            ED fileEncoding = ED.None;
            if (fileDataEncoding == (byte)ED.LittleEndian)
                fileEncoding = ED.LittleEndian;
            else if (fileDataEncoding == (byte)ED.BigEndian)
                fileEncoding = ED.BigEndian;

            ED encodingOfThisMachine = ED.BigEndian;
            if (BitConverter.IsLittleEndian)
                encodingOfThisMachine = ED.LittleEndian;

            if (fileEncoding == encodingOfThisMachine)
                IsSameEncoding = true;
            else
                IsSameEncoding = false;
        }

        public uint Elf32_Addr(byte[] value, uint startIndex)
        {
            return UInt32(value, startIndex);
        }

        public uint Elf32_Off(byte[] value, uint startIndex)
        {
            return UInt32(value, startIndex);
        }

        public ushort Elf32_Half(byte[] value, uint startIndex)
        {
            return UInt16(value, startIndex);
        }

        public uint Elf32_Word(byte[] value, uint startIndex)
        {
            return UInt32(value, startIndex);
        }

        public int Elf32_Sword(byte[] value, uint startIndex)
        {
            return Int32(value, startIndex);
        }


        public ulong Elf64_Addr(byte[] value, ulong startIndex)
        {
            return UInt64(value, startIndex);
        }

        public ulong Elf64_Off(byte[] value, ulong startIndex)
        {
            return UInt64(value, startIndex);
        }

        public ushort Elf64_Half(byte[] value, ulong startIndex)
        {
            return UInt16(value, startIndex);
        }

        public uint Elf64_Word(byte[] value, ulong startIndex)
        {
            return UInt32(value, startIndex);
        }

        public int Elf64_Sword(byte[] value, ulong startIndex)
        {
            return Int32(value, startIndex);
        }

        public ulong Elf64_Xword(byte[] value, ulong startIndex)
        {
            return UInt64(value, startIndex);
        }

        public long Elf64_Sxword(byte[] value, ulong startIndex)
        {
            return Int64(value, startIndex);
        }


        public short Int16(byte[] value, ulong startIndex)
        {
            if (IsSameEncoding)
                return StandardInt16(value, startIndex);
            else
                return ReverseInt16(value, startIndex);
        }

        public int Int32(byte[] value, ulong startIndex)
        {
            if (IsSameEncoding)
                return StandardInt32(value, startIndex);
            else
                return ReverseInt32(value, startIndex);
        }

        public long Int64(byte[] value, ulong startIndex)
        {
            if (IsSameEncoding)
                return StandardInt64(value, startIndex);
            else
                return ReverseInt64(value, startIndex);
        }

        public ushort UInt16(byte[] value, ulong startIndex)
        {
            if (IsSameEncoding)
                return StandardUInt16(value, startIndex);
            else
                return ReverseUInt16(value, startIndex);
        }

        public uint UInt32(byte[] value, ulong startIndex)
        {
            if (IsSameEncoding)
                return StandardUInt32(value, startIndex);
            else
                return ReverseUInt32(value, startIndex);
        }

        public ulong UInt64(byte[] value, ulong startIndex)
        {
            if (IsSameEncoding)
                return StandardUInt64(value, startIndex);
            else
                return ReverseUInt64(value, startIndex);
        }

        #region static methods

        public static string ASCIIString(byte[] value, ulong startIndex)
        {
            ulong stringLength = 0;
            while (value[startIndex + stringLength] != 0) stringLength++;
            return Encoding.ASCII.GetString(value, (int)startIndex, (int)stringLength);
        }

        public static string ASCIIString(byte[] value, ulong startIndex, int count)
        {
            return Encoding.ASCII.GetString(value, (int)startIndex, count);
        }

        public static ushort StandardUInt16(byte[] value, ulong startIndex)
        {
            return (ushort)(
                (value[startIndex + 1] << 8) |
                 value[startIndex]);
        }

        public static uint StandardUInt32(byte[] value, ulong startIndex)
        {
            return (uint)(
                  (value[startIndex + 3] << 24) |
                  (value[startIndex + 2] << 16) |
                  (value[startIndex + 1] << 8) |
                   value[startIndex]);
        }

        public static ulong StandardUInt64(byte[] value, ulong startIndex)
        {
            return
                ((ulong)value[startIndex + 7] << 56) |
                ((ulong)value[startIndex + 6] << 48) |
                ((ulong)value[startIndex + 5] << 40) |
                ((ulong)value[startIndex + 4] << 32) |
                ((ulong)value[startIndex + 3] << 24) |
                ((ulong)value[startIndex + 2] << 16) |
                ((ulong)value[startIndex + 1] << 8) |
                       value[startIndex];
        }

        public static short StandardInt16(byte[] value, ulong startIndex)
        {
            return (short)(
                (value[startIndex + 1] << 8) |
                 value[startIndex]);
        }

        public static int StandardInt32(byte[] value, ulong startIndex)
        {
            return
                  (value[startIndex + 3] << 24) |
                  (value[startIndex + 2] << 16) |
                  (value[startIndex + 1] << 8) |
                   value[startIndex];
        }

        public static long StandardInt64(byte[] value, ulong startIndex)
        {
            return
                ((long)value[startIndex + 7] << 56) |
                ((long)value[startIndex + 6] << 48) |
                ((long)value[startIndex + 5] << 40) |
                ((long)value[startIndex + 4] << 32) |
                ((long)value[startIndex + 3] << 24) |
                ((long)value[startIndex + 2] << 16) |
                ((long)value[startIndex + 1] << 8) |
                       value[startIndex];
        }

        public static ushort ReverseUInt16(byte[] value, ulong startIndex)
        {
            return (ushort)(
                (value[startIndex] << 8) |
                 value[startIndex + 1]);
        }

        public static uint ReverseUInt32(byte[] value, ulong startIndex)
        {
            return
                  ((uint)value[startIndex] << 24) |
                  ((uint)value[startIndex + 1] << 16) |
                  ((uint)value[startIndex + 2] << 8) |
                         value[startIndex + 3];
        }

        public static ulong ReverseUInt64(byte[] value, ulong startIndex)
        {
            return
                ((ulong)value[startIndex] << 56) |
                ((ulong)value[startIndex + 1] << 48) |
                ((ulong)value[startIndex + 2] << 40) |
                ((ulong)value[startIndex + 3] << 32) |
                ((ulong)value[startIndex + 4] << 24) |
                ((ulong)value[startIndex + 5] << 16) |
                ((ulong)value[startIndex + 6] << 8) |
                        value[startIndex + 7];
        }

        public static short ReverseInt16(byte[] value, ulong startIndex)
        {
            return (short)(
                (value[startIndex] << 8) |
                 value[startIndex + 1]);
        }

        public static int ReverseInt32(byte[] value, ulong startIndex)
        {
            return
                  (value[startIndex] << 24) |
                  (value[startIndex + 1] << 16) |
                  (value[startIndex + 2] << 8) |
                   value[startIndex + 3];
        }

        public static long ReverseInt64(byte[] value, ulong startIndex)
        {
            return
                ((long)value[startIndex] << 56) |
                ((long)value[startIndex + 1] << 48) |
                ((long)value[startIndex + 2] << 40) |
                ((long)value[startIndex + 3] << 32) |
                ((long)value[startIndex + 4] << 24) |
                ((long)value[startIndex + 5] << 16) |
                ((long)value[startIndex + 6] << 8) |
                       value[startIndex + 7];
        }

        #endregion
    }
}
