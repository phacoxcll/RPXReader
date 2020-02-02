using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RPXReader
{
    public class RPX /*and RPL*/ : ELF32
    {
        public struct FileInfo
        {
            public uint magic_version;
            public uint mRegBytes_Text;
            public uint mRegBytes_TextAlign;
            public uint mRegBytes_Data;
            public uint mRegBytes_DataAlign;
            public uint mRegBytes_LoaderInfo;
            public uint mRegBytes_LoaderInfoAlign;
            public uint mRegBytes_Temp;
            public uint mTrampAdj;
            public uint mSDABase;
            public uint mSDA2Base;
            public uint mSizeCoreStacks;
            public uint mSrcFileNameOffset;
            public uint mFlags;
            public uint mSysHeapBytes;
            public uint mTagsOffset;

            public FileInfo(byte[] data, uint startIndex, ValueRead read)
            {
                magic_version = read.UInt32(data, startIndex + 0x00);
                mRegBytes_Text = read.UInt32(data, startIndex + 0x04);
                mRegBytes_TextAlign = read.UInt32(data, startIndex + 0x08);
                mRegBytes_Data = read.UInt32(data, startIndex + 0x0c);
                mRegBytes_DataAlign = read.UInt32(data, startIndex + 0x10);
                mRegBytes_LoaderInfo = read.UInt32(data, startIndex + 0x14);
                mRegBytes_LoaderInfoAlign = read.UInt32(data, startIndex + 0x18);
                mRegBytes_Temp = read.UInt32(data, startIndex + 0x1c);
                mTrampAdj = read.UInt32(data, startIndex + 0x20);
                mSDABase = read.UInt32(data, startIndex + 0x24);
                mSDA2Base = read.UInt32(data, startIndex + 0x28);
                mSizeCoreStacks = read.UInt32(data, startIndex + 0x2c);
                mSrcFileNameOffset = read.UInt32(data, startIndex + 0x30);
                mFlags = read.UInt32(data, startIndex + 0x34);
                mSysHeapBytes = read.UInt32(data, startIndex + 0x38);
                mTagsOffset = read.UInt32(data, startIndex + 0x3c);
            }

            public override string ToString()
            {
                StringBuilder strBuilder = new StringBuilder();

                strBuilder.AppendLine("RPL file info:");
                strBuilder.AppendLine("  magic_version:             0x" + magic_version.ToString("X8"));
                strBuilder.AppendLine("  mRegBytes_Text:            0x" + mRegBytes_Text.ToString("X8"));
                strBuilder.AppendLine("  mRegBytes_TextAlign:       0x" + mRegBytes_TextAlign.ToString("X8"));
                strBuilder.AppendLine("  mRegBytes_Data:            0x" + mRegBytes_Data.ToString("X8"));
                strBuilder.AppendLine("  mRegBytes_DataAlign:       0x" + mRegBytes_DataAlign.ToString("X8"));
                strBuilder.AppendLine("  mRegBytes_LoaderInfo:      0x" + mRegBytes_LoaderInfo.ToString("X8"));
                strBuilder.AppendLine("  mRegBytes_LoaderInfoAlign: 0x" + mRegBytes_LoaderInfoAlign.ToString("X8"));
                strBuilder.AppendLine("  mRegBytes_Temp:            0x" + mRegBytes_Temp.ToString("X8"));
                strBuilder.AppendLine("  mTrampAdj:                 0x" + mTrampAdj.ToString("X8"));
                strBuilder.AppendLine("  mSDABase:                  0x" + mSDABase.ToString("X8"));
                strBuilder.AppendLine("  mSDA2Base:                 0x" + mSDA2Base.ToString("X8"));
                strBuilder.AppendLine("  mSizeCoreStacks:           0x" + mSizeCoreStacks.ToString("X8"));
                strBuilder.AppendLine("  mSrcFileNameOffset:        0x" + mSrcFileNameOffset.ToString("X8"));
                strBuilder.AppendLine("  mFlags:                    0x" + mFlags.ToString("X8"));
                strBuilder.AppendLine("  mSysHeapBytes:             0x" + mSysHeapBytes.ToString("X8"));
                strBuilder.AppendLine("  mTagsOffset:               0x" + mTagsOffset.ToString("X8"));

                return strBuilder.ToString();
            }
        }

        public FileInfo Info
        { protected set; get; }
        public uint[] CRC
        { protected set; get; }
        public string SrcFileName
        { protected set; get; }
        public List<string> Tags
        { protected set; get; }

        public RPX(string filename, bool readInfo = true)
            : base(filename, true)
        {
            if (!(Header.e_ident[(byte)EI.Target_OS_ABI] == 0xCA &&
                Header.e_ident[(byte)EI.ABIVersion] == 0xFE &&
                (Header.e_type == 0xFE01 || Header.e_type == 0x0002)))
                throw new FormatException("It is not an RPX or RPL file.");

            Info = new FileInfo();
            CRC = new uint[Header.e_shnum];
            SrcFileName = "";
            Tags = new List<string>();

            if (SectionHeader.Length != 0)
            {
                FileStream fs = File.Open(filename, FileMode.Open);
                byte[] shstrBytes = new byte[SectionHeader[Header.e_shstrndx].sh_size];
                fs.Position = SectionHeader[Header.e_shstrndx].sh_offset;
                fs.Read(shstrBytes, 0, shstrBytes.Length);
                fs.Close();

                SectionName = new string[Header.e_shnum];
                if ((SectionHeader[Header.e_shstrndx].sh_flags & (uint)SHF_RPL.ZLIB) == (uint)SHF_RPL.ZLIB)
                    shstrBytes = Decompress(shstrBytes);

                foreach (byte b in shstrBytes)
                {
                    if (b > 127)
                        throw new FormatException("shstrBytes are not ASCII.");
                }

                ReadSectionNames(shstrBytes);

                if (readInfo)
                    ReadInfo(filename);
                else
                    for (int i = 0; i < Header.e_shnum; i++)
                        CRC[i] = 0;
            }
        }

        protected virtual void ReadInfo(string filename)
        {
            ValueRead read = new ValueRead(Header.e_ident[(byte)EI.DataEncoding]);
            FileStream fs;

            for (int i = 0; i < Header.e_shnum; i++)
            {
                if (SectionHeader[i].sh_offset != 0)
                {
                    if ((SectionHeader[i].sh_type & (uint)SHT_RPL.FILEINFO) == (uint)SHT_RPL.FILEINFO)
                    {
                        fs = File.Open(filename, FileMode.Open);
                        byte[] sectiondata = new byte[SectionHeader[i].sh_size];
                        fs.Position = SectionHeader[i].sh_offset;
                        fs.Read(sectiondata, 0, sectiondata.Length);
                        fs.Close();

                        Info = new RPX.FileInfo(sectiondata, 0, read);
                        if (Info.mSrcFileNameOffset != 0)
                            SrcFileName = ValueRead.ASCIIString(sectiondata, Info.mSrcFileNameOffset);
                        if (Info.mTagsOffset != 0)
                        {
                            ulong offset = Info.mTagsOffset;
                            string tag = ValueRead.ASCIIString(sectiondata, offset);
                            while (tag.Length != 0)
                            {
                                Tags.Add(tag);
                                offset += (ulong)tag.Length + 1;
                                tag = ValueRead.ASCIIString(sectiondata, offset);
                            }
                        }
                    }
                    else if ((SectionHeader[i].sh_type & (uint)SHT_RPL.CRCS) == (uint)SHT_RPL.CRCS)
                    {
                        fs = File.Open(filename, FileMode.Open);
                        byte[] sectiondata = new byte[SectionHeader[i].sh_size];
                        fs.Position = SectionHeader[i].sh_offset;
                        fs.Read(sectiondata, 0, sectiondata.Length);
                        fs.Close();

                        for (int j = 0; j < Header.e_shnum; j++)
                            CRC[j] = read.UInt32(sectiondata, (ulong)(j * 4));
                    }
                }
            }
        }

        protected static byte[] Decompress(byte[] source)
        {
            uint decompressedSize = ValueRead.ReverseUInt32(source, 0);
            uint adler32 = ValueRead.ReverseUInt32(source, (ulong)(source.Length - 4));
            byte[] decompressedData = Security.ZlibDecompress(source, 4, source.Length - 4);
            uint adler32Calculated = Security.Adler32(decompressedData, 0, decompressedData.Length);

            if (decompressedSize != decompressedData.Length)
                throw new FormatException("Decompressed size does not match.");

            if (adler32 != adler32Calculated)
                throw new FormatException("Adler-32 does not match.");

            return decompressedData;
        }

        protected static byte[] Compress(byte[] source)
        {
            byte[] sizeBytes = BitConverter.GetBytes(source.Length);
            byte[] compressedData = Security.ZlibCompress(source, 0, source.Length);
            byte[] result = new byte[compressedData.Length + 4];
            result[0] = sizeBytes[3];
            result[1] = sizeBytes[2];
            result[2] = sizeBytes[1];
            result[3] = sizeBytes[0];
            Array.Copy(compressedData, 0, result, 4, compressedData.Length);
            return result;
        }

        public override string ToString()
        {
            StringBuilder strBuilder = new StringBuilder();

            strBuilder.AppendLine(Header.ToString());
            strBuilder.Append(Info.ToString());
            if (SrcFileName.Length != 0)
                strBuilder.AppendLine("  mSrcFileName: \"" + SrcFileName + "\"");
            if (Tags.Count != 0)
            {
                strBuilder.AppendLine("  mTags: ");
                for (int i = 0; i < Tags.Count; i++)
                    strBuilder.AppendLine("    " + Tags[i]);
            }
            strBuilder.AppendLine();

            if (SectionHeader.Length != 0)
            {
                strBuilder.AppendLine("Section Headers:");
                strBuilder.AppendLine("  [Nr] " + "Name".PadRight(NameMaxLength, ' ') +
                    " Type          Address  Offset   Size     EntSize Flags Link Info Align CRC32");
                for (int i = 0; i < SectionHeader.Length; i++)
                {
                    strBuilder.AppendLine("  [" + i.ToString().PadLeft(2, ' ') + "] " +
                        SectionName[i].PadRight(NameMaxLength, ' ') + " " +
                        SectionHeader[i].ToString() + " " +
                        CRC[i].ToString("X8"));
                }
            }

            return strBuilder.ToString();
        }

        public static void Decompress(string source, string destination)
        {
            RPX rpx = new RPX(source);

            int shSize = rpx.Header.e_shnum * 0x2C; // 0x2C = rpx.Header.e_shentsize + 4 bytes of CRC32            
            int sectionsOffset = GetPhysicalSectionSize(shSize) + 0x40;

            List<KeyValuePair<int, Elf32_Shdr>> shList = new List<KeyValuePair<int, Elf32_Shdr>>();
            List<KeyValuePair<int, Elf32_Shdr>> shNew = new List<KeyValuePair<int, Elf32_Shdr>>();

            for (int i = 0; i < rpx.SectionHeader.Length; i++)
                shList.Add(new KeyValuePair<int, Elf32_Shdr>(i, rpx.SectionHeader[i]));
            shList.Sort((pair1, pair2) => Elf32_Shdr.CompareByOffset(pair1.Value, pair2.Value));

            FileStream src = File.Open(source, FileMode.Open);
            FileStream dest = File.Open(destination, FileMode.Create);

            byte[] srcBytes = new byte[sectionsOffset];
            src.Read(srcBytes, 0, srcBytes.Length);
            dest.Write(srcBytes, 0, srcBytes.Length);

            for (int i = 0; i < shList.Count; i++)
            {
                int key = shList[i].Key;
                Elf32_Shdr shdr = new Elf32_Shdr(shList[i].Value);
                if (shList[i].Value.sh_offset >= sectionsOffset)
                {
                    int padding = 0;
                    if ((shList[i].Value.sh_flags & (uint)SHF_RPL.ZLIB) == (uint)SHF_RPL.ZLIB)
                    {
                        shdr.sh_offset = (uint)dest.Position;
                        srcBytes = new byte[shList[i].Value.sh_size];
                        src.Position = shList[i].Value.sh_offset;
                        src.Read(srcBytes, 0, srcBytes.Length);
                        byte[] decompressBytes = Decompress(srcBytes);
                        rpx.CRC[shList[i].Key] = Security.ComputeCRC32(decompressBytes, 0, decompressBytes.Length);
                        shdr.sh_flags &= ~(uint)SHF_RPL.ZLIB;
                        shdr.sh_size = (uint)decompressBytes.Length;
                        padding = GetPhysicalSectionSize(decompressBytes.Length) - decompressBytes.Length;
                        dest.Write(decompressBytes, 0, decompressBytes.Length);
                    }
                    else
                    {
                        shdr.sh_offset = (uint)dest.Position;
                        srcBytes = new byte[shList[i].Value.sh_size];
                        src.Position = shList[i].Value.sh_offset;
                        src.Read(srcBytes, 0, srcBytes.Length);
                        rpx.CRC[shList[i].Key] = Security.ComputeCRC32(srcBytes, 0, srcBytes.Length);
                        padding = GetPhysicalSectionSize(srcBytes.Length) - srcBytes.Length;
                        dest.Write(srcBytes, 0, srcBytes.Length);
                    }
                    byte[] paddingBytes = new byte[padding];
                    dest.Write(paddingBytes, 0, paddingBytes.Length);
                }
                shNew.Add(new KeyValuePair<int, Elf32_Shdr>(key, shdr));
            }

            src.Close();

            dest.Position = 0x40;
            shNew.Sort((pair1, pair2) => pair1.Key.CompareTo(pair2.Key));
            for (int i = 0; i < shNew.Count; i++)
                dest.Write(shNew[i].Value.ToArray(rpx.Header.e_ident[(byte)EI.DataEncoding]), 0, 0x28);

            for (int i = 0; i < rpx.CRC.Length; i++)
            {
                dest.WriteByte((byte)(rpx.CRC[i] >> 24));
                dest.WriteByte((byte)((rpx.CRC[i] >> 16) & 0xFF));
                dest.WriteByte((byte)((rpx.CRC[i] >> 8) & 0xFF));
                dest.WriteByte((byte)(rpx.CRC[i] & 0xFF));
            }

            dest.Close();
        }

        public static void Compress(string source, string destination)
        {
            RPX rpx = new RPX(source);

            int shSize = rpx.Header.e_shnum * 0x2C; // 0x2C = rpx.Header.e_shentsize + 4 bytes of CRC32            
            int sectionsOffset = GetPhysicalSectionSize(shSize) + 0x40;

            List<KeyValuePair<int, Elf32_Shdr>> shList = new List<KeyValuePair<int, Elf32_Shdr>>();
            List<KeyValuePair<int, Elf32_Shdr>> shNew = new List<KeyValuePair<int, Elf32_Shdr>>();

            for (int i = 0; i < rpx.SectionHeader.Length; i++)
                shList.Add(new KeyValuePair<int, Elf32_Shdr>(i, rpx.SectionHeader[i]));
            shList.Sort((pair1, pair2) => Elf32_Shdr.CompareByOffset(pair1.Value, pair2.Value));

            FileStream src = File.Open(source, FileMode.Open);
            FileStream dest = File.Open(destination, FileMode.Create);

            byte[] srcBytes = new byte[sectionsOffset];
            src.Read(srcBytes, 0, srcBytes.Length);
            dest.Write(srcBytes, 0, srcBytes.Length);

            for (int i = 0; i < shList.Count; i++)
            {
                int key = shList[i].Key;
                Elf32_Shdr shdr = new Elf32_Shdr(shList[i].Value);
                if (shList[i].Value.sh_offset >= sectionsOffset)
                {
                    int padding = 0;
                    if ((shList[i].Value.sh_type & (uint)SHT_RPL.FILEINFO) == (uint)SHT_RPL.FILEINFO ||
                        (shList[i].Value.sh_flags & (uint)SHF_RPL.ZLIB) == (uint)SHF_RPL.ZLIB)
                    {
                        shdr.sh_offset = (uint)dest.Position;
                        srcBytes = new byte[shList[i].Value.sh_size];
                        src.Position = shList[i].Value.sh_offset;
                        src.Read(srcBytes, 0, srcBytes.Length);
                        if ((shList[i].Value.sh_type & (uint)SHT_RPL.FILEINFO) == (uint)SHT_RPL.FILEINFO)
                            rpx.CRC[shList[i].Key] = Security.ComputeCRC32(srcBytes, 0, srcBytes.Length);
                        padding = GetPhysicalSectionSize(srcBytes.Length) - srcBytes.Length;
                        dest.Write(srcBytes, 0, srcBytes.Length);
                    }
                    else
                    {
                        shdr.sh_offset = (uint)dest.Position;
                        srcBytes = new byte[shList[i].Value.sh_size];
                        src.Position = shList[i].Value.sh_offset;
                        src.Read(srcBytes, 0, srcBytes.Length);
                        byte[] compressBytes = Compress(srcBytes);
                        rpx.CRC[shList[i].Key] = Security.ComputeCRC32(srcBytes, 0, srcBytes.Length);
                        if (compressBytes.Length < srcBytes.Length)
                        {
                            shdr.sh_flags |= (uint)SHF_RPL.ZLIB;
                            shdr.sh_size = (uint)compressBytes.Length;
                            padding = GetPhysicalSectionSize(compressBytes.Length) - compressBytes.Length;
                            dest.Write(compressBytes, 0, compressBytes.Length);
                        }
                        else
                        {
                            padding = GetPhysicalSectionSize(srcBytes.Length) - srcBytes.Length;
                            dest.Write(srcBytes, 0, srcBytes.Length);
                        }
                    }
                    byte[] paddingBytes = new byte[padding];
                    dest.Write(paddingBytes, 0, paddingBytes.Length);
                }
                shNew.Add(new KeyValuePair<int, Elf32_Shdr>(key, shdr));
            }

            src.Close();

            dest.Position = 0x40;
            shNew.Sort((pair1, pair2) => pair1.Key.CompareTo(pair2.Key));
            for (int i = 0; i < shNew.Count; i++)
                dest.Write(shNew[i].Value.ToArray(rpx.Header.e_ident[(byte)EI.DataEncoding]), 0, 0x28);

            for (int i = 0; i < rpx.CRC.Length; i++)
            {
                dest.WriteByte((byte)(rpx.CRC[i] >> 24));
                dest.WriteByte((byte)((rpx.CRC[i] >> 16) & 0xFF));
                dest.WriteByte((byte)((rpx.CRC[i] >> 8) & 0xFF));
                dest.WriteByte((byte)(rpx.CRC[i] & 0xFF));
            }

            dest.Close();
        }

        protected static int GetPhysicalSectionSize(int size)
        {
            return size % 0x40 == 0 ? size : size / 0x40 * 0x40 + 0x40;
        }
    }
}
