using System;
using System.IO;
using System.Text;

namespace RPXReader
{
    public class ELF32 : ELF
    {
        public Elf32_Ehdr Header
        { protected set; get; }
        public Elf32_Shdr[] SectionHeader
        { protected set; get; }
        public string[] SectionName
        { protected set; get; }
        protected int NameMaxLength;

        public ELF32(string filename)
            : this(filename, true)
        {
            if (SectionHeader.Length != 0)
            {
                FileStream fs = File.Open(filename, FileMode.Open);
                byte[] shstrBytes = new byte[SectionHeader[Header.e_shstrndx].sh_size];
                fs.Position = SectionHeader[Header.e_shstrndx].sh_offset;
                fs.Read(shstrBytes, 0, shstrBytes.Length);
                fs.Close();

                foreach (byte b in shstrBytes)
                {
                    if (b > 127)
                        throw new FormatException("shstrBytes are not ASCII.");
                }

                ReadSectionNames(shstrBytes);
            }
        }

        public ELF32(string filename, bool readSectionHeaders)
            : this()
        {
            FileStream fs = File.Open(filename, FileMode.Open);
            byte[] ehdrBytes = new byte[52];
            fs.Read(ehdrBytes, 0, ehdrBytes.Length);
            fs.Close();

            if (!(ehdrBytes[(byte)EI.MagicNumber0] == 0x7F &&
                ehdrBytes[(byte)EI.MagicNumber1] == 'E' &&
                ehdrBytes[(byte)EI.MagicNumber2] == 'L' &&
                ehdrBytes[(byte)EI.MagicNumber3] == 'F'))
                throw new FormatException("It is not an ELF file.");

            if (ehdrBytes[(byte)EI.FileClass] == (byte)EC.ELF64)
                throw new FormatException("It is an ELF64 file.");
            else if (ehdrBytes[(byte)EI.FileClass] != (byte)EC.ELF32)
                throw new FormatException("It is an invalid ELF class.");

            ValueRead read = new ValueRead(ehdrBytes[(byte)EI.DataEncoding]);
            Header = new Elf32_Ehdr(ehdrBytes, read);

            SectionHeader = new Elf32_Shdr[Header.e_shnum];
            SectionName = new string[Header.e_shnum];

            if (readSectionHeaders && SectionHeader.Length != 0)
            {
                fs = File.Open(filename, FileMode.Open);
                byte[] shdrBytes = new byte[Header.e_shnum * Header.e_shentsize];
                fs.Position = Header.e_shoff;
                fs.Read(shdrBytes, 0, shdrBytes.Length);
                fs.Close();

                for (int i = 0; i < Header.e_shnum; i++)
                {
                    SectionHeader[i] = new Elf32_Shdr(shdrBytes, (uint)(Header.e_shentsize * i), read);
                    SectionName[i] = "";
                }
            }
        }

        protected ELF32()
        {
            Header = new Elf32_Ehdr();
            SectionHeader = null;
            SectionName = null;
            NameMaxLength = 0;
        }

        protected void ReadSectionNames(byte[] source)
        {
            for (int i = 0; i < SectionHeader.Length; i++)
            {
                SectionName[i] = ValueRead.ASCIIString(source, SectionHeader[i].sh_name);
                if (NameMaxLength < SectionName[i].Length)
                    NameMaxLength = SectionName[i].Length;
            }
        }

        public override string ToString()
        {
            StringBuilder strBuilder = new StringBuilder();

            strBuilder.AppendLine(Header.ToString());

            if (SectionHeader.Length != 0)
            {
                strBuilder.AppendLine("Section Headers:");
                strBuilder.AppendLine("  [Nr] " + "Name".PadRight(NameMaxLength, ' ') +
                    " Type          Address  Offset   Size     EntSize Flags Link Info Align");
                for (int i = 0; i < SectionHeader.Length; i++)
                {
                    strBuilder.AppendLine("  [" + i.ToString().PadLeft(2, ' ') + "] " +
                        SectionName[i].PadRight(NameMaxLength, ' ') + " " +
                        SectionHeader[i].ToString());
                }
            }

            return strBuilder.ToString();
        }
    }
}
