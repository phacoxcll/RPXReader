using System;
using System.IO;
using System.Text;

namespace RPXReader
{
    public class RPXSNES : RPX
    {
        public enum VCType
        {
            A1,
            A2,
            B1,
            B2,
            Unknown
        }

        public struct ROMInfo
        {
            public int[] HeaderData
            { private set; get; }
            public byte[] Data
            { private set; get; }
            public byte[] FooterData
            { private set; get; }
            public byte[] ExtraData
            { private set; get; }
            public byte[] AdditionalData
            { private set; get; }

            public char FormatCode
            { private set; get; }
            public string ShortID
            { private set; get; }
            public char RegionCode
            { private set; get; }
            public string ProductCode
            { get { return (FormatCode + ShortID + RegionCode).ToUpper(); } }
            public int RawSize
            { private set; get; }

            public int Size
            {
                get
                {
                    if (HeaderData != null)
                        return HeaderData[1];
                    else
                        return 0;
                }
            }
            public int PCMSamplesOffset
            {
                get
                {
                    if (HeaderData != null)
                        return HeaderData[3];
                    else
                        return 0;
                }
            }
            public int PCMFooterOffset
            {
                get
                {
                    if (HeaderData != null)
                        return HeaderData[4];
                    else
                        return 0;
                }
            }
            public int FooterOffset
            {
                get
                {
                    if (HeaderData != null)
                        return HeaderData[5];
                    else
                        return 0;
                }
            }
            public int OffsetX0
            {
                get
                {
                    if (HeaderData != null)
                        return HeaderData[6];
                    else
                        return 0;
                }
            }
            public int OffsetX1
            {
                get
                {
                    if (HeaderData != null)
                        return HeaderData[7];
                    else
                        return 0;
                }
            }
            public int OffsetX2
            {
                get
                {
                    if (HeaderData != null)
                        return HeaderData[8];
                    else
                        return 0;
                }
            }

            public byte EmulationSpeed
            {
                get
                {
                    if (FooterData != null)
                        return FooterData[0];
                    else
                        return 0;
                }
            }
            public int PCMSamplesSize
            { private set; get; }
            public int PCMFooterSize
            { private set; get; }
            public ushort PresetID
            {
                get
                {
                    if (FooterData != null)
                        return (ushort)((FooterData[14] << 8) + FooterData[13]);
                    else
                        return 0;
                }
            }
            public byte Playes
            {
                get
                {
                    if (FooterData != null)
                        return FooterData[15];
                    else
                        return 0;
                }
            }
            public byte SoundVolume
            {
                get
                {
                    if (FooterData != null && FooterData.Length == 19)
                        return FooterData[16];
                    else
                        return 0;
                }
            }
            public byte Type
            {
                get
                {
                    if (FooterData != null && FooterData.Length == 19)
                        return FooterData[17];
                    else
                        return 0;
                }
            }
            public byte EnhancementChip
            {
                get
                {
                    if (FooterData != null && FooterData.Length == 19)
                        return FooterData[18];
                    else
                        return 0;
                }
            }

            public bool ExtendedFooter
            {
                get
                {
                    if (FooterData != null)
                        return FooterData.Length == 19;
                    else
                        return false;
                }
            }

            public ROMInfo(byte[] data)
            {
                if (!(data[0x44] == 'W' &&
                    data[0x45] == 'U' &&
                    data[0x46] == 'P' &&
                    data[0x47] == '-'))
                    throw new FormatException("It is not an VC SNES ROM.");

                HeaderData = new int[12];
                HeaderData[0] = ValueRead.StandardInt32(data, 0x20); //Always 0x00000100
                HeaderData[1] = ValueRead.StandardInt32(data, 0x24); //Size
                HeaderData[2] = ValueRead.StandardInt32(data, 0x28); //ROM offset (Always 0x00000030)
                HeaderData[3] = ValueRead.StandardInt32(data, 0x2C); //PCM samples offset
                HeaderData[4] = ValueRead.StandardInt32(data, 0x30); //PCM footer offset
                HeaderData[5] = ValueRead.StandardInt32(data, 0x34); //Footer offset
                HeaderData[6] = ValueRead.StandardInt32(data, 0x38); //X0 offset
                HeaderData[7] = ValueRead.StandardInt32(data, 0x3C); //X1 offset
                HeaderData[8] = ValueRead.StandardInt32(data, 0x40); //X2 offset
                HeaderData[9] = ValueRead.StandardInt32(data, 0x44); //Always 0x2D505557 ("WUP-")
                HeaderData[10] = ValueRead.StandardInt32(data, 0x48); //Product code
                HeaderData[11] = ValueRead.StandardInt32(data, 0x4C); //Always 0x00000000

                if (data[0x31 + HeaderData[5]] == 0x14 || data[0x31 + HeaderData[5]] == 0x15)
                    FooterData = new byte[19];
                else
                    FooterData = new byte[16];
                FooterData[0] = data[0x20 + HeaderData[5]]; //Emulation speed (Always 0x3C (60 FPS))
                FooterData[1] = data[0x21 + HeaderData[5]]; //ROM size byte 0
                FooterData[2] = data[0x22 + HeaderData[5]]; //ROM size byte 1
                FooterData[3] = data[0x23 + HeaderData[5]]; //ROM size byte 2
                FooterData[4] = data[0x24 + HeaderData[5]]; //ROM size byte 3
                FooterData[5] = data[0x25 + HeaderData[5]]; //PCM samples size byte 0
                FooterData[6] = data[0x26 + HeaderData[5]]; //PCM samples size byte 1
                FooterData[7] = data[0x27 + HeaderData[5]]; //PCM samples size byte 2
                FooterData[8] = data[0x28 + HeaderData[5]]; //PCM samples size byte 3
                FooterData[9] = data[0x29 + HeaderData[5]]; //PCM footer size byte 0
                FooterData[10] = data[0x2A + HeaderData[5]]; //PCM footer size byte 1
                FooterData[11] = data[0x2B + HeaderData[5]]; //PCM footer size byte 2
                FooterData[12] = data[0x2C + HeaderData[5]]; //PCM footer size byte 3
                FooterData[13] = data[0x2D + HeaderData[5]]; //Game preset ID byte 0
                FooterData[14] = data[0x2E + HeaderData[5]]; //Game preset ID byte 1
                FooterData[15] = data[0x2F + HeaderData[5]]; //Amount of players
                if (FooterData.Length == 19)
                {
                    FooterData[16] = data[0x30 + HeaderData[5]]; //Sound volume
                    FooterData[17] = data[0x31 + HeaderData[5]]; //ROM type (0x14 LoROM, 0x15 HiROM)
                    FooterData[18] = data[0x32 + HeaderData[5]]; //Enhancement chip
                                                                 //0x00 Normal
                                                                 //0x02 DSP-1
                                                                 //0x03 S-DD1
                                                                 //0x04 Cx4
                                                                 //0x05 ??? (DSP-2?)
                                                                 //0x06 SA-1
                                                                 //0x07 SA-1
                                                                 //0x08 SA-1
                                                                 //0x09 SA-1
                                                                 //0x0A SA-1
                                                                 //0x0B SA-1
                                                                 //0x0C SuperFX/GSU
                }

                ExtraData = new byte[HeaderData[1] - (HeaderData[5] + FooterData.Length)];
                for (int i = 0; i < ExtraData.Length; i++)
                    ExtraData[i] = data[0x20 + HeaderData[5] + FooterData.Length + i];

                FormatCode = (char)data[0x48];
                ShortID = ValueRead.ASCIIString(data, 0x49, 2);
                RegionCode = (char)data[0x4B];

                RawSize = ValueRead.StandardInt32(data, (ulong)(0x21 + HeaderData[5]));
                PCMSamplesSize = ValueRead.StandardInt32(data, (ulong)(0x25 + HeaderData[5]));
                PCMFooterSize = ValueRead.StandardInt32(data, (ulong)(0x29 + HeaderData[5]));

                Data = new byte[RawSize];
                Array.Copy(data, 0x20 + HeaderData[2], Data, 0, Data.Length);

                AdditionalData = new byte[data.Length - 0x20 - HeaderData[1]];
                Array.Copy(data, 0x20 + HeaderData[1], AdditionalData, 0, AdditionalData.Length);
            }

            public override string ToString()
            {
                StringBuilder strBuilder = new StringBuilder();

                strBuilder.AppendLine("ROM info:");
                strBuilder.AppendLine("  Title: \"" + GetTitleWithRegion(ProductCode) + "\"");
                strBuilder.AppendLine("  Release date:       " + GetReleaseDate(ProductCode));
                strBuilder.AppendLine("  Product code:       " + ProductCode);
                strBuilder.AppendLine("  Preset ID:          0x" + PresetID.ToString("X4"));
                strBuilder.AppendLine("  Size:               " + Size.ToString() + " (bytes)");
                strBuilder.AppendLine("  PCM samples size:   " + PCMSamplesSize.ToString() + " (bytes)");
                strBuilder.AppendLine("  PCM footer size:    " + PCMFooterSize.ToString() + " (bytes)");
                if (Type == 0x14)
                    strBuilder.AppendLine("  Type:               0x14 (LoROM)");
                else if (Type == 0x15)
                    strBuilder.AppendLine("  Type:               0x15 (HiROM)");
                else
                    strBuilder.AppendLine("  Type:               0x" + Type.ToString("X2"));
                //strBuilder.AppendLine("  Header data 0:      0x" + HeaderData[0].ToString("X8") + "(Always 0x00000100)");
                strBuilder.AppendLine("  Full size:          " + HeaderData[1].ToString() + " (bytes)");
                strBuilder.AppendLine("  ROM offset:         0x" + HeaderData[2].ToString("X8"));
                strBuilder.AppendLine("  PCM samples offset: 0x" + HeaderData[3].ToString("X8"));
                strBuilder.AppendLine("  PCM footer offset:  0x" + HeaderData[4].ToString("X8"));
                strBuilder.AppendLine("  Footer offset:      0x" + HeaderData[5].ToString("X8"));
                strBuilder.AppendLine("  X0 offset:          0x" + HeaderData[6].ToString("X8"));
                strBuilder.AppendLine("  X1 offset:          0x" + HeaderData[7].ToString("X8"));
                strBuilder.AppendLine("  X2 offset:          0x" + HeaderData[8].ToString("X8"));
                //strBuilder.AppendLine("  Header data 9:      0x" + HeaderData[9].ToString("X8") + "(Always 0x2D505557)");
                //strBuilder.AppendLine("  Product code:       0x" + HeaderData[10].ToString("X8"));
                //strBuilder.AppendLine("  Header data 11:     0x" + HeaderData[11].ToString("X8") + "(Always 0x00000000)");

                strBuilder.AppendLine("  Emulation speed:    " + FooterData[0].ToString() + " (FPS)");
                //strBuilder.AppendLine("  ROM size byte 0:         0x" + FooterData[1].ToString("X2"));
                //strBuilder.AppendLine("  ROM size byte 1:         0x" + FooterData[2].ToString("X2"));
                //strBuilder.AppendLine("  ROM size byte 2:         0x" + FooterData[3].ToString("X2"));
                //strBuilder.AppendLine("  ROM size byte 3:         0x" + FooterData[4].ToString("X2"));
                //strBuilder.AppendLine("  PCM samples size byte 0: 0x" + FooterData[5].ToString("X2"));
                //strBuilder.AppendLine("  PCM samples size byte 1: 0x" + FooterData[6].ToString("X2"));
                //strBuilder.AppendLine("  PCM samples size byte 2: 0x" + FooterData[7].ToString("X2"));
                //strBuilder.AppendLine("  PCM samples size byte 3: 0x" + FooterData[8].ToString("X2"));
                //strBuilder.AppendLine("  PCM footer size byte 0:  0x" + FooterData[9].ToString("X2"));
                //strBuilder.AppendLine("  PCM footer size byte 1:  0x" + FooterData[10].ToString("X2"));
                //strBuilder.AppendLine("  PCM footer size byte 2:  0x" + FooterData[11].ToString("X2"));
                //strBuilder.AppendLine("  PCM footer size byte 3:  0x" + FooterData[12].ToString("X2"));
                //strBuilder.AppendLine("  Game preset ID byte 0:   0x" + FooterData[13].ToString("X2"));
                //strBuilder.AppendLine("  Game preset ID byte 1:   0x" + FooterData[14].ToString("X2"));
                strBuilder.AppendLine("  Number of players:  " + FooterData[15].ToString());

                strBuilder.AppendLine("  Extra data size:    " + ExtraData.Length.ToString() + " (bytes)");
                strBuilder.Append("  Extra data:");
                int length = ExtraData.Length > 64 ? 64 : ExtraData.Length;
                for (int i = 0; i < length; i++)
                    strBuilder.Append(" " + ExtraData[i].ToString("X2"));
                if (ExtraData.Length != length)
                    strBuilder.AppendLine("...");
                else
                    strBuilder.AppendLine();

                return strBuilder.ToString();
            }
        }

        public ROMInfo ROM
        { private set; get; }
        public VCType Type
        { private set; get; }
        public uint CRCsSum
        { private set; get; }

        public RPXSNES(string filename, bool readInfo = true)
            : base(filename, readInfo)
        {
            ROM = new ROMInfo();
            Type = VCType.Unknown;
            CRCsSum = 0;

            if (SectionHeader.Length == 0)
                throw new FormatException("It is not an RPX file.");

            Type = GetVCType();
            if (Type == VCType.Unknown)
                throw new FormatException("It is not an RPX SNES file.");

            if (readInfo)
                ReadInfo(filename);
        }

        private VCType GetVCType()
        {
            if (SectionName.Length == 30 &&
                SectionName[11] == ".fimport_sysapp" &&
                SectionName[12] == ".fimport_zlib125.rpl" &&
                SectionName[13] == ".fimport_gx2" &&
                SectionName[14] == ".fimport_snd_core.rpl" &&
                SectionName[15] == ".dimport_snd_core.rpl" &&
                SectionName[16] == ".fimport_nn_save" &&
                SectionName[17] == ".fimport_vpad.rpl" &&
                SectionName[18] == ".fimport_proc_ui" &&
                SectionName[19] == ".fimport_padscore" &&
                SectionName[20] == ".fimport_coreinit" &&
                SectionName[21] == ".dimport_coreinit" &&
                SectionName[22] == ".fimport_mic.rpl" && //Not in type A2
                SectionName[23] == ".fimport_snd_user.rpl" &&
                SectionName[24] == ".dimport_snd_user.rpl" &&
                SectionName[25] == ".symtab" &&
                SectionName[26] == ".strtab" &&
                SectionName[27] == ".shstrtab")
                return VCType.A1;
            else if (SectionName.Length == 29 &&
                SectionName[11] == ".fimport_sysapp" &&
                SectionName[12] == ".fimport_zlib125.rpl" &&
                SectionName[13] == ".fimport_gx2" &&
                SectionName[14] == ".fimport_snd_core.rpl" &&
                SectionName[15] == ".dimport_snd_core.rpl" &&
                SectionName[16] == ".fimport_nn_save" &&
                SectionName[17] == ".fimport_vpad.rpl" &&
                SectionName[18] == ".fimport_proc_ui" &&
                SectionName[19] == ".fimport_padscore" &&
                SectionName[20] == ".fimport_coreinit" &&
                SectionName[21] == ".dimport_coreinit" &&
                SectionName[22] == ".fimport_snd_user.rpl" &&
                SectionName[23] == ".dimport_snd_user.rpl" &&
                SectionName[24] == ".symtab" &&
                SectionName[25] == ".strtab" &&
                SectionName[26] == ".shstrtab")
                return VCType.A2;
            else if (SectionName.Length == 28 &&
                SectionName[11] == ".dimport_nn_act" &&
                SectionName[12] == ".fimport_sysapp" &&
                SectionName[13] == ".fimport_zlib125" &&
                SectionName[14] == ".fimport_gx2" &&
                SectionName[15] == ".fimport_snd_core" &&
                SectionName[16] == ".fimport_nn_save" && //Index 17 in type B2
                SectionName[17] == ".fimport_vpad" &&    //Index 18 in type B2
                SectionName[18] == ".fimport_proc_ui" && //Index 19 in type B2
                SectionName[19] == ".fimport_padscore" &&//Index 20 in type B2
                SectionName[20] == ".fimport_coreinit" &&//Index 21 in type B2
                SectionName[21] == ".dimport_coreinit" &&//Index 22 in type B2
                SectionName[22] == ".fimport_snd_user" &&//Index 16 in type B2
                SectionName[23] == ".symtab" &&
                SectionName[24] == ".strtab" &&
                SectionName[25] == ".shstrtab")
                return VCType.B1;
            else if (SectionName.Length == 28 &&
                SectionName[11] == ".dimport_nn_act" &&
                SectionName[12] == ".fimport_sysapp" &&
                SectionName[13] == ".fimport_zlib125" &&
                SectionName[14] == ".fimport_gx2" &&
                SectionName[15] == ".fimport_snd_core" &&
                SectionName[16] == ".fimport_snd_user" &&//Index 22 in type B1
                SectionName[17] == ".fimport_nn_save" && //Index 16 in type B1
                SectionName[18] == ".fimport_vpad" &&    //Index 17 in type B1
                SectionName[19] == ".fimport_proc_ui" && //Index 18 in type B1
                SectionName[20] == ".fimport_padscore" &&//Index 19 in type B1
                SectionName[21] == ".fimport_coreinit" &&//Index 20 in type B1
                SectionName[22] == ".dimport_coreinit" &&//Index 21 in type B1             
                SectionName[23] == ".symtab" &&
                SectionName[24] == ".strtab" &&
                SectionName[25] == ".shstrtab")
                return VCType.B2;
            else
                return VCType.Unknown;
        }

        protected override void ReadInfo(string filename)
        {
            base.ReadInfo(filename);
            long sum = 0;
            for (int i = 0; i < SectionHeader.Length; i++)
            {
                sum += CRC[i];
                if (SectionHeader[i].sh_offset != 0 &&
                    SectionHeader[i].sh_addr == 0x10000000 &&
                    SectionName[i] == ".rodata")
                {
                    FileStream fs = File.Open(filename, FileMode.Open);
                    byte[] sectionData = new byte[SectionHeader[i].sh_size];
                    fs.Position = SectionHeader[i].sh_offset;
                    fs.Read(sectionData, 0, sectionData.Length);
                    fs.Close();

                    if ((SectionHeader[i].sh_flags & (uint)SHF_RPL.ZLIB) == (uint)SHF_RPL.ZLIB)
                        sectionData = Decompress(sectionData);

                    ROM = new ROMInfo(sectionData);
                }
            }
            sum -= (long)CRC[2] + CRC[3] + CRC[CRC.Length - 1];
            CRCsSum = (uint)(sum >> 4);
        }

        public override string ToString()
        {
            StringBuilder strBuilder = new StringBuilder();

            strBuilder.AppendLine(Header.ToString());
            strBuilder.Append(Info.ToString());
            strBuilder.AppendLine("  mSrcFileName: \"" + SrcFileName + "\"");
            strBuilder.AppendLine("  mTags: ");
            for (int i = 0; i < Tags.Count; i++)
                strBuilder.AppendLine("    " + Tags[i]);
            strBuilder.AppendLine();
            strBuilder.AppendLine("VC SNES:");
            strBuilder.AppendLine("  Type: " + Type.ToString());
            strBuilder.AppendLine("  CRCs sum: 0x" + CRCsSum.ToString("X8"));
            strBuilder.AppendLine();
            strBuilder.Append(ROM.ToString());
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

        public string GetROMFileName()
        {
            string title = GetTitleWithRegion(ROM.ProductCode);
            char[] invalid = Path.GetInvalidFileNameChars();

            foreach (char i in invalid)
                title = title.Replace(i.ToString(), "");

            return title + " [" + ROM.ProductCode + "].sfc";
        }

        public static string GetTitleWithRegion(string productCode)
        {
            return GetTitle(productCode) + " (" + GetRegion(productCode) + ")";
        }

        public static string GetRegion(string productCode)
        {
            return productCode[3] == 'J' ? "JPN" :
                (productCode[3] == 'E' ? "USA" :
                (productCode[3] == 'P' ? "EUR" :
                (productCode[3] == 'F' ? "FRA" :
                (productCode[3] == 'D' ? "DEU" :
                productCode[3].ToString()))));
        }

        public static string GetTitle(string productCode)
        {
            switch (productCode)
            {
                case "JA2J": return "Heracles no Eikō III: Kamigami no Chinmoku";
                case "JA3J": return "Panel de Pon";
                case "JA4J": return "Shin Megami Tensei";
                case "JA5E": return "Contra III: The Alien Wars";
                case "JA5J": return "Contra III: The Alien Wars";
                case "JA5P": return "Contra III: The Alien Wars";
                case "JA6J": return "Famicom Tantei Club Part II: Ushiro ni Tatsu Shōjo";
                case "JA7E": return "Pilotwings";
                case "JA7J": return "Pilotwings";
                case "JA7P": return "Pilotwings";
                case "JA8E": return "Final Fight";
                case "JA8J": return "Final Fight";
                case "JA8P": return "Final Fight";
                case "JA9E": return "Super Castlevania IV";
                case "JA9J": return "Super Castlevania IV";
                case "JA9P": return "Super Castlevania IV";
                case "JAAE": return "Super Mario World";
                case "JAAJ": return "Super Mario World";
                case "JABE": return "Super Mario RPG: Legend of the Seven Stars";
                case "JABJ": return "Super Mario RPG";
                case "JABP": return "Super Mario RPG: Legend of the Seven Stars";
                case "JACE": return "Donkey Kong Country";
                case "JACJ": return "Donkey Kong Country";
                case "JACP": return "Donkey Kong Country";
                case "JADD": return "The Legend of Zelda: A Link to the Past";
                case "JADE": return "The Legend of Zelda: A Link to the Past";
                case "JADF": return "The Legend of Zelda: A Link to the Past";
                case "JADJ": return "The Legend of Zelda: A Link to the Past";
                case "JADP": return "The Legend of Zelda: A Link to the Past";
                case "JAEE": return "Kirby Super Star";
                case "JAEJ": return "Kirby Super Star";
                case "JAFJ": return "Fire Emblem: Seisen no Keifu";
                case "JAGE": return "Donkey Kong Country 2: Diddy's Kong Quest";
                case "JAGJ": return "Donkey Kong Country 2: Diddy's Kong Quest";
                case "JAGP": return "Donkey Kong Country 2: Diddy's Kong Quest";
                case "JAHJ": return "Fire Emblem: Monshō no Nazo";
                case "JAJE": return "Super Metroid";
                case "JAJJ": return "Super Metroid";
                case "JAJP": return "Super Metroid";
                case "JAKE": return "Super Mario Kart";
                case "JAKJ": return "Super Mario Kart";
                case "JALE": return "The Legend of the Mystical Ninja";
                case "JALJ": return "The Legend of the Mystical Ninja";
                case "JALP": return "The Legend of the Mystical Ninja";
                case "JAME": return "Street Fighter II: The World Warrior";
                case "JAMJ": return "Street Fighter II: The World Warrior";
                case "JANE": return "Kirby's Dream Land 3";
                case "JANJ": return "Kirby's Dream Land 3";
                case "JAQJ": return "Mario's Super Picross";
                case "JAQP": return "Mario's Super Picross";
                case "JARE": return "F-Zero";
                case "JARJ": return "F-Zero";
                case "JASE": return "Kirby's Dream Course";
                case "JASJ": return "Kirby's Dream Course";
                case "JASP": return "Kirby's Dream Course";
                case "JATE": return "Super Ghouls 'n Ghosts";
                case "JATJ": return "Super Ghouls 'n Ghosts";
                case "JAUJ": return "Ganbare Goemon 2: Kiteretsu Shōgun Magginesu";
                case "JAVE": return "Super Street Fighter II: The New Challengers";
                case "JAVJ": return "Super Street Fighter II: The New Challengers";
                case "JAWJ": return "Kirby's Star Stacker";
                case "JAXJ": return "Ganbare Goemon 3: Shishijūrokubē no Karakuri Manji Gatame";
                case "JAYE": return "Street Fighter II' Turbo: Hyper Fighting";
                case "JAYJ": return "Street Fighter II' Turbo: Hyper Fighting";
                case "JAZJ": return "Kamaitachi no Yoru";
                case "JB2J": return "Ogre Battle: The March of the Black Queen";
                case "JB3J": return "Romancing SaGa";
                case "JB4J": return "Tactics Ogre: Let Us Cling Together";
                case "JB5J": return "Romancing SaGa 2";
                case "JB6J": return "Final Fantasy V";
                case "JB7J": return "Final Fantasy Mystic Quest";
                case "JB8E": return "Super Punch-Out!!";
                case "JB8J": return "Super Punch-Out!!";
                case "JB8P": return "Super Punch-Out!!";
                case "JB9J": return "Romancing SaGa 3";
                case "JBAE": return "Mega Man X";
                case "JBAJ": return "Rockman X";
                case "JBAP": return "Mega Man X";
                case "JBBE": return "EarthBound";
                case "JBBJ": return "Mother 2";
                case "JBBP": return "EarthBound";
                case "JBCE": return "Romance of the Three Kingdoms IV: Wall of Fire";
                case "JBCJ": return "Romance of the Three Kingdoms IV: Wall of Fire";
                case "JBCP": return "Romance of the Three Kingdoms IV: Wall of Fire";
                case "JBEE": return "Breath of Fire II";
                case "JBEJ": return "Breath of Fire II: The Fated Child";
                case "JBEP": return "Breath of Fire II";
                case "JBFJ": return "Fire Emblem: Thracia 776";
                case "JBGJ": return "Super Famicom Wars";
                case "JBHJ": return "Famicom Bunko: Hajimari no Mori";
                case "JBJE": return "Vegas Stakes";
                case "JBJP": return "Vegas Stakes";
                case "JBKE": return "Harvest Moon";
                case "JBKP": return "Harvest Moon";
                case "JBLE": return "Final Fight 2";
                case "JBLJ": return "Final Fight 2";
                case "JBLP": return "Final Fight 2";
                case "JBMJ": return "Wagan Land";
                case "JBNE": return "Cybernator";
                case "JBNJ": return "Cybernator";
                case "JBNP": return "Cybernator";
                case "JBQE": return "Uncharted Waters: New Horizons";
                case "JBQJ": return "Uncharted Waters II: New Horizons";
                case "JBQP": return "Uncharted Waters: New Horizons";
                case "JBRJ": return "Shin Megami Tensei II";
                case "JBSJ": return "Shin Megami Tensei If...";
                case "JBTE": return "Mega Man X2";
                case "JBTJ": return "Rockman X2";
                case "JBTP": return "Mega Man X2";
                case "JBUE": return "Final Fight 3";
                case "JBUJ": return "Final Fight 3";
                case "JBUP": return "Final Fight 3";
                case "JBVE": return "Brawl Brothers";
                case "JBVJ": return "Rushing Beat Ran";
                case "JBVP": return "Brawl Brothers";
                case "JBWJ": return "Clock Tower";
                case "JBXJ": return "Secret of Mana";
                case "JBYJ": return "Final Fantasy VI";
                case "JBZJ": return "Final Fantasy IV";
                case "JC2J": return "Gussun Oyoyo";
                case "JC3E": return "Metal Marines";
                case "JC3J": return "Metal Marines";
                case "JC4E": return "Pac-Attack";
                case "JC4J": return "Cosmo Gang the Puzzle";
                case "JC4P": return "Pac-Attack";
                case "JC5J": return "Live A Live";
                case "JC6J": return "Albert Odyssey";
                case "JC7E": return "Genghis Khan II: Clan of the Gray Wolf";
                case "JC7J": return "Genghis Khan II: Clan of the Gray Wolf";
                case "JC8J": return "Taikō Risshiden";
                case "JC9J": return "Majin Tensei";
                case "JCAJ": return "Bahamut Lagoon";
                case "JCBJ": return "Pop'n TwinBee";
                case "JCBP": return "Pop'n TwinBee";
                case "JCCJ": return "Marvelous: Mōhitotsu no Takarajima";
                case "JCDE": return "Castlevania: Dracula X";
                case "JCDJ": return "Castlevania: Dracula X";
                case "JCDP": return "Castlevania: Dracula X";
                case "JCEJ": return "Otogirisō";
                case "JCFJ": return "Pop'n TwinBee: Rainbow Bell Adventures";
                case "JCFP": return "Pop'n TwinBee: Rainbow Bell Adventures";
                case "JCGE": return "Street Fighter Alpha 2";
                case "JCGJ": return "Street Fighter Zero 2";
                case "JCGP": return "Street Fighter Alpha 2";
                case "JCHJ": return "Last Bible III";
                case "JCJJ": return "Super Ninja Boy";
                case "JCKJ": return "Kunio-kun no Dodge Ball da yo: Zenin Shūgo";
                case "JCLE": return "Nobunaga's Ambition";
                case "JCLJ": return "Super Nobunaga's Ambition";
                case "JCLP": return "Nobunaga's Ambition";
                case "JCMJ": return "Heisei Shin Onigashima Part 1";
                case "JCNE": return "Mega Man 7";
                case "JCNJ": return "Rockman 7: Shukumei no Taiketsu!";
                case "JCNP": return "Mega Man 7";
                case "JCPE": return "Mega Man X3";
                case "JCPJ": return "Rockman X3";
                case "JCPP": return "Mega Man X3";
                case "JCQJ": return "Heisei Shin Onigashima Part 2";
                case "JCRJ": return "Sutte Hakkun";
                case "JCSJ": return "Gakkou de atta Kowai Hanashi";
                case "JCTE": return "Wild Guns";
                case "JCTP": return "Wild Guns";
                case "JCUE": return "Natsume Championship Wrestling";
                case "JCUP": return "Natsume Championship Wrestling";
                case "JCVE": return "Breath of Fire";
                case "JCVJ": return "Breath of Fire";
                case "JCVP": return "Breath of Fire";
                case "JCWE": return "Demon's Crest";
                case "JCWJ": return "Demon's Crest";
                case "JCWP": return "Demon's Crest";
                case "JCXE": return "Donkey Kong Country 3: Dixie Kong's Double Trouble!";
                case "JCXJ": return "Donkey Kong Country 3: Dixie Kong's Double Trouble!";
                case "JCXP": return "Donkey Kong Country 3: Dixie Kong's Double Trouble!";
                case "JCYJ": return "Heracles no Eikō IV: Kamigami kara no Okurimono";
                case "JCZE": return "Axelay";
                case "JCZJ": return "Axelay";
                case "JCZP": return "Axelay";
                case "JDAE": return "Super E.D.F.: Earth Defense Force";
                case "JDAJ": return "Super E.D.F.: Earth Defense Force";
                case "JDBE": return "Rival Turf!";
                case "JDBJ": return "Rushing Beat";
                case "JDEJ": return "Idol Janshi Suchie-Pai";
                case "JDFJ": return "Majin Tensei II: Spiral Nemesis";
                case "JDGJ": return "Treasure of the Rudras";
                case "JDHJ": return "Space Invaders: The Original Game";
                case "JDJJ": return "Darius Twin";
                case "JDKE": return "Pac-Man 2: The New Adventures";
                case "JDKP": return "Pac-Man 2: The New Adventures";
                case "JDLJ": return "Metal Slader Glory";
                case "JEMJ": return "Kai: Tsukikomori";
                case "JENE": return "The Ignition Factor";
                case "JENJ": return "Fire Fighting";
                case "JEPJ": return "Power Instinct";
                case "JLCJ": return "Wrecking Crew '98";
                default: return "[Unknown]";
            }
        }

        public static string GetReleaseDate(string productCode)
        {
            switch (productCode)
            {
                case "JA2J": return "2013-05-22";
                case "JA3J": return "2013-05-29";
                case "JA4J": return "2013-07-03";
                case "JA5E": return "2013-11-28";
                case "JA5J": return "2013-11-27";
                case "JA5P": return "2014-01-09";
                case "JA6J": return "2013-07-31";
                case "JA7E": return "2013-07-04";
                case "JA7J": return "2013-05-29";
                case "JA7P": return "2013-07-04";
                case "JA8E": return "2013-10-03";
                case "JA8J": return "2014-08-06";
                case "JA8P": return "2013-10-03";
                case "JA9E": return "2013-10-31";
                case "JA9J": return "2013-09-11";
                case "JA9P": return "2013-10-31";
                case "JAAE": return "2013-04-26";
                case "JAAJ": return "2013-04-27";
                case "JABE": return "2016-06-30";
                case "JABJ": return "2015-08-05";
                case "JABP": return "2015-12-24";
                case "JACE": return "2015-02-26";
                case "JACJ": return "2014-11-26";
                case "JACP": return "2014-10-16";
                case "JADD": return "2013-12-12";
                case "JADE": return "2014-01-30";
                case "JADF": return "2013-12-12";
                case "JADJ": return "2014-02-12";
                case "JADP": return "2013-12-12";
                case "JAEE": return "2013-05-23";
                case "JAEJ": return "2013-05-01";
                case "JAFJ": return "2013-04-27";
                case "JAGE": return "2015-02-26";
                case "JAGJ": return "2014-11-26";
                case "JAGP": return "2014-10-23";
                case "JAHJ": return "2013-04-27";
                case "JAJE": return "2013-05-15";
                case "JAJJ": return "2013-05-15";
                case "JAJP": return "2013-05-16";
                case "JAKE": return "2014-03-27";
                case "JAKJ": return "2013-06-19";
                case "JALE": return "2013-12-05";
                case "JALJ": return "2013-09-04";
                case "JALP": return "2014-01-16";
                case "JAME": return "2013-08-22";
                case "JAMJ": return "2014-06-25";
                case "JANE": return "2013-05-23";
                case "JANJ": return "2013-05-08";
                case "JAQJ": return "2013-04-27";
                case "JAQP": return "2013-04-27";
                case "JARE": return "2013-04-26";
                case "JARJ": return "2013-04-27";
                case "JASE": return "2013-05-23";
                case "JASJ": return "2013-05-08";
                case "JASP": return "2013-05-23";
                case "JATE": return "2013-05-16";
                case "JATJ": return "2013-04-27";
                case "JAUJ": return "2013-09-25";
                case "JAVE": return "2013-08-22";
                case "JAVJ": return "2014-06-25";
                case "JAWJ": return "2013-05-08";
                case "JAXJ": return "2013-10-16";
                case "JAYE": return "2013-08-22";
                case "JAYJ": return "2014-06-25";
                case "JAZJ": return "2013-08-07";
                case "JB2J": return "2013-11-20";
                case "JB3J": return "2013-12-18";
                case "JB4J": return "2014-03-12";
                case "JB5J": return "2014-01-22";
                case "JB6J": return "2014-03-26";
                case "JB7J": return "2014-04-16";
                case "JB8E": return "2013-12-26";
                case "JB8J": return "2014-04-09";
                case "JB8P": return "2014-06-12";
                case "JB9J": return "2014-02-26";
                case "JBAE": return "2013-05-30";
                case "JBAJ": return "2013-05-22";
                case "JBAP": return "2013-09-19";
                case "JBBE": return "2013-07-18";
                case "JBBJ": return "2013-04-27";
                case "JBBP": return "2013-07-18";
                case "JBCE": return "2013-08-08";
                case "JBCJ": return "2013-07-24";
                case "JBCP": return "2013-08-08";
                case "JBEE": return "2013-09-05";
                case "JBEJ": return "2013-07-10";
                case "JBEP": return "2013-09-05";
                case "JBFJ": return "2013-07-10";
                case "JBGJ": return "2013-10-02";
                case "JBHJ": return "2013-08-21";
                case "JBJE": return "2013-06-27";
                case "JBJP": return "2013-06-27";
                case "JBKE": return "2013-08-01";
                case "JBKP": return "2013-08-01";
                case "JBLE": return "2013-10-03";
                case "JBLJ": return "2014-08-27";
                case "JBLP": return "2013-10-03";
                case "JBMJ": return "2013-09-18";
                case "JBNE": return "2014-08-07";
                case "JBNJ": return "2015-01-14";
                case "JBNP": return "2015-02-26";
                case "JBQE": return "2013-11-14";
                case "JBQJ": return "2013-10-30";
                case "JBQP": return "2013-11-28";
                case "JBRJ": return "2013-09-25";
                case "JBSJ": return "2013-10-16";
                case "JBTE": return "2014-01-02";
                case "JBTJ": return "2013-10-09";
                case "JBTP": return "2013-11-14";
                case "JBUE": return "2013-10-03";
                case "JBUJ": return "2014-10-29";
                case "JBUP": return "2013-10-03";
                case "JBVE": return "2013-11-21";
                case "JBVJ": return "2013-11-27";
                case "JBVP": return "2013-11-21";
                case "JBWJ": return "2013-11-06";
                case "JBXJ": return "2013-06-26";
                case "JBYJ": return "2013-06-26";
                case "JBZJ": return "2014-02-19";
                case "JC2J": return "2015-08-19";
                case "JC3E": return "2015-05-07";
                case "JC3J": return "2015-03-04";
                case "JC4E": return "2015-06-04";
                case "JC4J": return "2015-04-28";
                case "JC4P": return "2015-02-26";
                case "JC5J": return "2015-06-24";
                case "JC6J": return "2015-07-29";
                case "JC7E": return "2015-08-20";
                case "JC7J": return "2015-07-22";
                case "JC8J": return "2015-05-20";
                case "JC9J": return "2015-07-15";
                case "JCAJ": return "2014-02-05";
                case "JCBJ": return "2014-05-07";
                case "JCBP": return "2014-07-10";
                case "JCCJ": return "2014-02-12";
                case "JCDE": return "2014-10-02";
                case "JCDJ": return "2014-04-23";
                case "JCDP": return "2014-11-13";
                case "JCEJ": return "2014-07-30";
                case "JCFJ": return "2014-05-21";
                case "JCFP": return "2014-07-31";
                case "JCGE": return "2014-05-22";
                case "JCGJ": return "2014-08-20";
                case "JCGP": return "2014-10-02";
                case "JCHJ": return "2015-01-28";
                case "JCJJ": return "2014-10-01";
                case "JCKJ": return "2014-07-16";
                case "JCLE": return "2014-09-04";
                case "JCLJ": return "2014-10-29";
                case "JCLP": return "2014-09-25";
                case "JCMJ": return "2014-09-24";
                case "JCNE": return "2014-09-12";
                case "JCNJ": return "2014-08-06";
                case "JCNP": return "2014-11-06";
                case "JCPE": return "2014-08-28";
                case "JCPJ": return "2014-10-08";
                case "JCPP": return "2014-11-06";
                case "JCQJ": return "2014-09-24";
                case "JCRJ": return "2015-02-04";
                case "JCSJ": return "2014-08-27";
                case "JCTE": return "2014-09-18";
                case "JCTP": return "2014-11-20";
                case "JCUE": return "2014-12-18";
                case "JCUP": return "2014-11-20";
                case "JCVE": return "2015-02-12";
                case "JCVJ": return "2014-09-10";
                case "JCVP": return "2014-11-27";
                case "JCWE": return "2014-10-30";
                case "JCWJ": return "2015-07-08";
                case "JCWP": return "2015-01-15";
                case "JCXE": return "2015-02-26";
                case "JCXJ": return "2014-11-26";
                case "JCXP": return "2014-10-30";
                case "JCYJ": return "2015-02-10";
                case "JCZE": return "2015-02-19";
                case "JCZJ": return "2015-02-25";
                case "JCZP": return "2015-01-15";
                case "JDAE": return "2015-05-21";
                case "JDAJ": return "2015-08-26";
                case "JDBE": return "2015-05-28";
                case "JDBJ": return "2015-09-30";
                case "JDEJ": return "2017-03-29";
                case "JDFJ": return "2016-06-01";
                case "JDGJ": return "2015-12-02";
                case "JDHJ": return "2016-10-12";
                case "JDJJ": return "2016-10-12";
                case "JDKE": return "2016-03-03";
                case "JDKP": return "2015-10-08";
                case "JDLJ": return "2015-12-09";
                case "JEMJ": return "2016-09-07";
                case "JENE": return "2015-09-24";
                case "JENJ": return "2017-03-29";
                case "JEPJ": return "2015-11-11";
                case "JLCJ": return "2016-09-28";
                default: return "[Unknown]";
            }
        }
    }
}
