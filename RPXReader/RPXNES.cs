using System;
using System.IO;
using System.Text;

namespace RPXReader
{
    public class RPXNES : RPX
    {
        public enum VCType
        {
            A,
            B,
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
            public string ShortId
            { private set; get; }
            public char RegionCode
            { private set; get; }
            public string ProductCode
            { get { return (FormatCode + ShortId + RegionCode).ToUpper(); } }
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
            public int FooterOffset
            {
                get
                {
                    if (HeaderData != null)
                        return HeaderData[3];
                    else
                        return 0;
                }
            }
            public int OffsetX0
            {
                get
                {
                    if (HeaderData != null)
                        return HeaderData[4];
                    else
                        return 0;
                }
            }
            public int OffsetX1
            {
                get
                {
                    if (HeaderData != null)
                        return HeaderData[5];
                    else
                        return 0;
                }
            }
            public int OffsetX2
            {
                get
                {
                    if (HeaderData != null)
                        return HeaderData[6];
                    else
                        return 0;
                }
            }
            public int OffsetX3
            {
                get
                {
                    if (HeaderData != null)
                        return HeaderData[7];
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
            public ushort PresetID
            {
                get
                {
                    if (FooterData != null)
                        return (ushort)((FooterData[9] << 8) + FooterData[8]);
                    else
                        return 0;
                }
            }
            public byte Playes
            {
                get
                {
                    if (FooterData != null)
                        return FooterData[10];
                    else
                        return 0;
                }
            }

            public bool IsFDS
            { private set; get; }

            public ROMInfo(byte[] data)
            {
                if (!(data[0x40] == 'W' &&
                    data[0x41] == 'U' &&
                    data[0x42] == 'P' &&
                    data[0x43] == '-'))
                    throw new FormatException("It is not an VC NES ROM.");

                HeaderData = new int[12];
                HeaderData[0] = ValueRead.StandardInt32(data, 0x20); //Always 0x00000000
                HeaderData[1] = ValueRead.StandardInt32(data, 0x24); //Size
                HeaderData[2] = ValueRead.StandardInt32(data, 0x28); //ROM offset (Always 0x00000030)
                HeaderData[3] = ValueRead.StandardInt32(data, 0x2C); //Footer offset
                HeaderData[4] = ValueRead.StandardInt32(data, 0x30); //X0 offset
                HeaderData[5] = ValueRead.StandardInt32(data, 0x34); //X1 offset
                HeaderData[6] = ValueRead.StandardInt32(data, 0x38); //X2 offset
                HeaderData[7] = ValueRead.StandardInt32(data, 0x3C); //X3 offset
                HeaderData[8] = ValueRead.StandardInt32(data, 0x40); //Always 0x2D505557 ("WUP-")
                HeaderData[9] = ValueRead.StandardInt32(data, 0x44); //Product code
                HeaderData[10] = ValueRead.StandardInt32(data, 0x48); //Always 0x00000000
                HeaderData[11] = ValueRead.StandardInt32(data, 0x4C); //Always 0x00000000

                FooterData = new byte[11];
                FooterData[0] = data[0x20 + HeaderData[3]]; //Emulation speed (Always 0x3C (60 FPS))
                FooterData[1] = data[0x21 + HeaderData[3]]; //
                FooterData[2] = data[0x22 + HeaderData[3]]; //
                FooterData[3] = data[0x23 + HeaderData[3]]; //
                FooterData[4] = data[0x24 + HeaderData[3]]; //
                FooterData[5] = data[0x25 + HeaderData[3]]; //
                FooterData[6] = data[0x26 + HeaderData[3]]; //
                FooterData[7] = data[0x27 + HeaderData[3]]; //
                FooterData[8] = data[0x28 + HeaderData[3]]; //Game preset ID byte 0
                FooterData[9] = data[0x29 + HeaderData[3]]; //Game preset ID byte 1
                FooterData[10] = data[0x2A + HeaderData[3]]; //Amount of players

                ExtraData = new byte[HeaderData[1] - (HeaderData[3] + 11)];
                for (int i = 0; i < ExtraData.Length; i++)
                    ExtraData[i] = data[0x2B + HeaderData[3] + i];

                FormatCode = (char)data[0x44];
                ShortId = ValueRead.ASCIIString(data, 0x45, 2);
                RegionCode = (char)data[0x47];

                if (data[0x20 + HeaderData[2]] == 'N' &&
                    data[0x21 + HeaderData[2]] == 'E' &&
                    data[0x22 + HeaderData[2]] == 'S')
                    IsFDS = false;
                else
                    IsFDS = true;

                if (IsFDS)
                {
                    RawSize = HeaderData[3] - HeaderData[2];
                    Data = new byte[RawSize];
                }
                else
                {
                    RawSize = data[0x24 + HeaderData[2]] * 16384 + data[0x25 + HeaderData[2]] * 8192;
                    Data = new byte[HeaderData[3] - HeaderData[2]];
                }
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
                strBuilder.AppendLine("  FDS ROM:            " + IsFDS.ToString());
                strBuilder.AppendLine("  Size:               " + Size.ToString() + " (bytes)");
                //strBuilder.AppendLine("  Header data 0:      0x" + HeaderData[0].ToString("X8") + "(Always 0x00000000)");
                strBuilder.AppendLine("  Full size:          " + HeaderData[1].ToString() + " (bytes)");
                strBuilder.AppendLine("  ROM offset:         0x" + HeaderData[2].ToString("X8"));
                strBuilder.AppendLine("  Footer offset:      0x" + HeaderData[3].ToString("X8"));
                strBuilder.AppendLine("  X0 offset:          0x" + HeaderData[4].ToString("X8"));
                strBuilder.AppendLine("  X1 offset:          0x" + HeaderData[5].ToString("X8"));
                strBuilder.AppendLine("  X2 offset:          0x" + HeaderData[6].ToString("X8"));
                strBuilder.AppendLine("  X3 offset:          0x" + HeaderData[7].ToString("X8"));
                //strBuilder.AppendLine("  Header data 8:      0x" + HeaderData[8].ToString("X8") + "(Always 0x2D505557)");
                //strBuilder.AppendLine("  Product code:       0x" + HeaderData[9].ToString("X8"));
                //strBuilder.AppendLine("  Header data 10:     0x" + HeaderData[10].ToString("X8") + "(Always 0x00000000)");
                //strBuilder.AppendLine("  Header data 11:     0x" + HeaderData[11].ToString("X8") + "(Always 0x00000000)");

                strBuilder.AppendLine("  Emulation speed:    " + FooterData[0].ToString() + " (FPS)");
                strBuilder.AppendLine("  Footer byte 1:      0x" + FooterData[1].ToString("X2"));
                strBuilder.AppendLine("  Footer byte 2:      0x" + FooterData[2].ToString("X2"));
                strBuilder.AppendLine("  Footer byte 3:      0x" + FooterData[3].ToString("X2"));
                strBuilder.AppendLine("  Footer byte 4:      0x" + FooterData[4].ToString("X2"));
                strBuilder.AppendLine("  Footer byte 5:      0x" + FooterData[5].ToString("X2"));
                strBuilder.AppendLine("  Footer byte 6:      0x" + FooterData[6].ToString("X2"));
                strBuilder.AppendLine("  Footer byte 7:      0x" + FooterData[7].ToString("X2"));
                //strBuilder.AppendLine("  Game preset ID byte 0: 0x" + FooterData[8].ToString("X2"));
                //strBuilder.AppendLine("  Game preset ID byte 1: 0x" + FooterData[9].ToString("X2"));
                strBuilder.AppendLine("  Number of players:  " + FooterData[10].ToString());

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

        public RPXNES(string filename, bool readInfo = true)
            : base(filename, false)
        {
            ROM = new ROMInfo();
            Type = VCType.Unknown;
            CRCsSum = 0;

            if (SectionHeader.Length == 0)
                throw new FormatException("It is not an RPX file.");

            Type = GetVCType();
            if (Type == VCType.Unknown)
                throw new FormatException("It is not an RPX NES file.");

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
                SectionName[16] == ".fimport_snd_user.rpl" &&
                SectionName[17] == ".dimport_snd_user.rpl" &&
                SectionName[18] == ".fimport_nn_save" &&
                SectionName[19] == ".fimport_vpad.rpl" &&
                SectionName[20] == ".fimport_proc_ui" &&
                SectionName[21] == ".fimport_padscore" &&
                SectionName[22] == ".fimport_coreinit" &&
                SectionName[23] == ".dimport_coreinit" &&
                SectionName[24] == ".fimport_mic.rpl" &&
                SectionName[25] == ".symtab" &&
                SectionName[26] == ".strtab" &&
                SectionName[27] == ".shstrtab")
                return VCType.A;
            else if (SectionName.Length == 29 &&
                SectionName[11] == ".dimport_nn_act" &&
                SectionName[12] == ".fimport_sysapp" &&
                SectionName[13] == ".fimport_zlib125" &&
                SectionName[14] == ".fimport_gx2" &&
                SectionName[15] == ".fimport_snd_core" &&
                SectionName[16] == ".fimport_snd_user" &&
                SectionName[17] == ".fimport_nn_save" &&
                SectionName[18] == ".fimport_vpad" &&
                SectionName[19] == ".fimport_proc_ui" &&
                SectionName[20] == ".fimport_padscore" &&
                SectionName[21] == ".fimport_coreinit" &&
                SectionName[22] == ".dimport_coreinit" &&
                SectionName[23] == ".fimport_mic" &&
                SectionName[24] == ".symtab" &&
                SectionName[25] == ".strtab" &&
                SectionName[26] == ".shstrtab")
                return VCType.B;
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
            strBuilder.AppendLine("VC NES:");
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

            if (ROM.IsFDS)
                return title + " [" + ROM.ProductCode + "].fds";
            else
                return title + " [" + ROM.ProductCode + "].nes";
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
                productCode[3].ToString()));
        }

        public static string GetTitle(string productCode)
        {
            switch (productCode)
            {
                case "FA2J": return "Mappy";
                case "FA4E": return "Wrecking Crew";
                case "FA4J": return "Wrecking Crew";
                case "FA4P": return "Wrecking Crew";
                case "FA5J": return "Shin Onigashima";
                case "FA6E": return "Galaga";
                case "FA6J": return "Galaga";
                case "FA6P": return "Galaga";
                case "FA7E": return "Mega Man 4";
                case "FA7J": return "Rockman 4: Aratanaru Yabou!!";
                case "FA7P": return "Mega Man 4";
                case "FA8E": return "Metroid";
                case "FA8J": return "Metroid";
                case "FA8P": return "Metroid";
                case "FA9E": return "Super Mario Bros.: The Lost Levels";
                case "FA9J": return "Super Mario Bros. 2";
                case "FA9P": return "Super Mario Bros.: The Lost Levels";
                case "FAAE": return "Super Mario Bros.";
                case "FAAJ": return "Super Mario Bros.";
                case "FABE": return "Super Mario Bros. 3";
                case "FABJ": return "Super Mario Bros. 3";
                case "FACE": return "Ice Climber";
                case "FACJ": return "Ice Climber";
                case "FADE": return "Kirby's Adventure";
                case "FADJ": return "Kirby's Adventure";
                case "FADP": return "Kirby's Adventure";
                case "FAEE": return "Mario Bros.";
                case "FAEJ": return "Mario Bros.";
                case "FAFE": return "Donkey Kong";
                case "FAFJ": return "Donkey Kong";
                case "FAGE": return "Excitebike";
                case "FAGJ": return "Excitebike";
                case "FAGP": return "Excitebike";
                case "FAHE": return "Super Mario Bros. 2";
                case "FAHJ": return "Super Mario Bros. USA";
                case "FAHP": return "Super Mario Bros. 2";
                case "FAJE": return "Balloon Fight";
                case "FAJJ": return "Balloon Fight";
                case "FAJP": return "Balloon Fight";
                case "FAKE": return "Punch-Out!! Featuring Mr. Dream";
                case "FAKJ": return "Punch-Out!!";
                case "FALE": return "Xevious";
                case "FALJ": return "Xevious";
                case "FAME": return "Yoshi";
                case "FAMJ": return "Yoshi no Tamago";
                case "FAMP": return "Mario & Yoshi";
                case "FANE": return "Mega Man";
                case "FANJ": return "Rockman";
                case "FAPE": return "Mega Man 2";
                case "FAPJ": return "Rockman 2: Dr. Wily no Nazo";
                case "FAQJ": return "Downtown Nekketsu Kōshinkyoku: Soreyuke Daiundōkai";
                case "FARE": return "Pac-Man";
                case "FARJ": return "Pac-Man";
                case "FASE": return "Spelunker";
                case "FASJ": return "Spelunker";
                case "FATE": return "Gradius";
                case "FATJ": return "Gradius";
                case "FAUJ": return "TwinBee";
                case "FAVE": return "Lode Runner";
                case "FAVJ": return "Lode Runner";
                case "FAWE": return "Donkey Kong Jr.";
                case "FAWJ": return "Donkey Kong Jr.";
                case "FAXJ": return "Ninja JaJaMaru‑kun";
                case "FAYE": return "Solomon's Key";
                case "FAYJ": return "Solomon's Key";
                case "FAZE": return "Mega Man 3";
                case "FAZJ": return "Rockman 3: Dr. Wily no Saigo!?";
                case "FAZP": return "Mega Man 3";
                case "FB2E": return "Super Dodge Ball";
                case "FB2J": return "Super Dodge Ball";
                case "FB2P": return "Super Dodge Ball";
                case "FB3E": return "Castlevania";
                case "FB3J": return "Castlevania";
                case "FB3P": return "Castlevania";
                case "FB4E": return "NES Open Tournament Golf";
                case "FB4J": return "Mario Open Golf";
                case "FB4P": return "NES Open Tournament Golf";
                case "FB5E": return "Dr. Mario";
                case "FB5J": return "Dr. Mario";
                case "FB5P": return "Dr. Mario";
                case "FB6E": return "Renegade";
                case "FB6J": return "Renegade";
                case "FB6P": return "Renegade";
                case "FB7E": return "Double Dragon";
                case "FB7J": return "Double Dragon";
                case "FB7P": return "Double Dragon";
                case "FB8E": return "Castlevania II: Simon's Quest";
                case "FB8J": return "Castlevania II: Simon's Quest";
                case "FB8P": return "Castlevania II: Simon's Quest";
                case "FB9J": return "Elevator Action";
                case "FBAE": return "The Legend of Zelda";
                case "FBAJ": return "The Legend of Zelda";
                case "FBAP": return "The Legend of Zelda";
                case "FBBE": return "Kid Icarus";
                case "FBBJ": return "Kid Icarus";
                case "FBBP": return "Kid Icarus";
                case "FBCE": return "Zelda II: The Adventure of Link";
                case "FBCJ": return "Zelda II: The Adventure of Link";
                case "FBCP": return "Zelda II: The Adventure of Link";
                case "FBDE": return "EarthBound Beginnings";
                case "FBDJ": return "Mother";
                case "FBDP": return "EarthBound Beginnings";
                case "FBEJ": return "Ikki";
                case "FBGE": return "Adventure Island";
                case "FBGJ": return "Adventure Island";
                case "FBGP": return "Adventure Island";
                case "FBHE": return "Ghosts 'n Goblins";
                case "FBHJ": return "Ghosts 'n Goblins";
                case "FBHP": return "Ghosts 'n Goblins";
                case "FBKJ": return "The Tower of Druaga";
                case "FBLE": return "Baseball";
                case "FBLJ": return "Baseball";
                case "FBLP": return "Baseball";
                case "FBME": return "Tennis";
                case "FBMJ": return "Tennis";
                case "FBMP": return "Tennis";
                case "FBNE": return "Pinball";
                case "FBNJ": return "Pinball";
                case "FBNP": return "Pinball";
                case "FBPE": return "Urban Champion";
                case "FBPJ": return "Urban Champion";
                case "FBPP": return "Urban Champion";
                case "FBQE": return "Clu Clu Land";
                case "FBQJ": return "Clu Clu Land";
                case "FBQP": return "Clu Clu Land";
                case "FBRE": return "Donkey Kong 3";
                case "FBRJ": return "Donkey Kong 3";
                case "FBRP": return "Donkey Kong 3";
                case "FBSE": return "Golf";
                case "FBSJ": return "Golf";
                case "FBSP": return "Golf";
                case "FBTJ": return "Final Fantasy";
                case "FBUE": return "Ice Hockey";
                case "FBUJ": return "Ice Hockey";
                case "FBUP": return "Ice Hockey";
                case "FBVE": return "Wario's Woods";
                case "FBVJ": return "Wario's Woods";
                case "FBVP": return "Wario's Woods";
                case "FBWJ": return "Bubble Bobble";
                case "FBXJ": return "Final Fantasy 2";
                case "FBYJ": return "Final Fantasy 3";
                case "FBZJ": return "Downtown Special: Kunio-kun no Jidaigeki da yo Zen'in Shūgō!";
                case "FC2E": return "Bases Loaded";
                case "FC2J": return "Bases Loaded";
                case "FC3J": return "Yie Ar Kung-Fu";
                case "FC4J": return "Antarctic Adventure";
                case "FC5E": return "Mega Man 5";
                case "FC5J": return "Rockman 5: Blues no Wana!?";
                case "FC5P": return "Mega Man 5";
                case "FC6E": return "Gargoyle's Quest II: The Demon Darkness";
                case "FC6J": return "Gargoyle's Quest II: The Demon Darkness";
                case "FC6P": return "Gargoyle's Quest II: The Demon Darkness";
                case "FC7J": return "The Mysterious Murasame Castle";
                case "FC8E": return "Double Dragon II: The Revenge";
                case "FC8J": return "Double Dragon II: The Revenge";
                case "FC8P": return "Double Dragon II: The Revenge";
                case "FC9E": return "Life Force";
                case "FC9J": return "Salamander";
                case "FC9P": return "Life Force";
                case "FCAJ": return "Famicom Tantei Club: Kieta Kōkeisha";
                case "FCBE": return "Ninja Gaiden";
                case "FCBJ": return "Ninja Gaiden";
                case "FCBP": return "Ninja Gaiden";
                case "FCCE": return "Mighty Bomb Jack";
                case "FCCJ": return "Mighty Bomb Jack";
                case "FCCP": return "Mighty Bomb Jack";
                case "FCDJ": return "Ganbare Goemon! Karakuri Dōchū";
                case "FCEE": return "Super C";
                case "FCEJ": return "Super Contra";
                case "FCEP": return "Super C";
                case "FCFE": return "Volleyball";
                case "FCFJ": return "Volleyball";
                case "FCFP": return "Volleyball";
                case "FCGJ": return "Fire Emblem: Ankoku Ryū to Hikari no Tsurugi";
                case "FCHE": return "Castlevania III: Dracula's Curse";
                case "FCHJ": return "Castlevania III: Dracula's Curse";
                case "FCHP": return "Castlevania III: Dracula's Curse";
                case "FCJJ": return "Hanjuku Hero";
                case "FCKJ": return "The Legend of Kage";
                case "FCLJ": return "Nintendo World Cup";
                case "FCME": return "Flying Dragon: The Secret Scroll";
                case "FCMJ": return "Flying Dragon: The Secret Scroll";
                case "FCMP": return "Flying Dragon: The Secret Scroll";
                case "FCNJ": return "Fire Emblem Gaiden";
                case "FCPE": return "Soccer";
                case "FCPJ": return "Soccer";
                case "FCPP": return "Soccer";
                case "FCQE": return "Mach Rider";
                case "FCQJ": return "Mach Rider";
                case "FCQP": return "Mach Rider";
                case "FCRJ": return "Ike Ike! Nekketsu Hockey-bu: Subete Koronde Dairantou";
                case "FCSE": return "Mega Man 6";
                case "FCSJ": return "Rockman 6: Shijō Saidai no Tatakai!!";
                case "FCSP": return "Mega Man 6";
                case "FCTJ": return "Battle City";
                case "FCUJ": return "Wagan Land";
                case "FCVJ": return "Princess Tomato in the Salad Kingdom";
                case "FCWE": return "Kung-Fu Heroes";
                case "FCWJ": return "Kung Fu Heroes";
                case "FCWP": return "Kung Fu Heroes";
                case "FCXE": return "Adventures of Lolo";
                case "FCXJ": return "Adventures of Lolo";
                case "FCXP": return "Adventures of Lolo";
                case "FCYJ": return "Joy Mech Fight";
                case "FCZE": return "Pac-Land";
                case "FCZJ": return "Pac-Land";
                case "FCZP": return "Pac-Land";
                case "FD2E": return "The Adventures of Bayou Billy";
                case "FD2J": return "The Adventures of Bayou Billy";
                case "FD2P": return "The Adventures of Bayou Billy";
                case "FD3J": return "Getsu Fūma Den";
                case "FD4J": return "Esper Dream";
                case "FD5J": return "Ganbare Goemon 2: Kiteretsu Shōgun Magginesu";
                case "FD6E": return "Stinger";
                case "FD6J": return "Stinger";
                case "FD7J": return "Sugoro Quest - The Quest of Dice Heros";
                case "FD8J": return "Seicross";
                case "FD9E": return "Mappy-Land";
                case "FD9J": return "Mappy-Land";
                case "FD9P": return "Mappy-Land";
                case "FDAE": return "Ufouria: The Saga";
                case "FDAJ": return "Ufouria: The Saga";
                case "FDAP": return "Ufouria: The Saga";
                case "FDBJ": return "Tower of Babel";
                case "FDCJ": return "Mendel Palace";
                case "FDDE": return "Donkey Kong Jr. Math";
                case "FDDJ": return "Donkey Kong Jr. Math";
                case "FDDP": return "Donkey Kong Jr. Math";
                case "FDEE": return "Dig Dug";
                case "FDEJ": return "Dig Dug";
                case "FDEP": return "Dig Dug";
                case "FDFJ": return "Dragon Buster";
                case "FDGJ": return "Tsuppari Ōzumō";
                case "FDHJ": return "Nuts and Milk";
                case "FDJJ": return "Gomoku Narabe Renju";
                case "FDKE": return "Mighty Final Fight";
                case "FDKJ": return "Mighty Final Fight";
                case "FDKP": return "Mighty Final Fight";
                case "FDLE": return "Street Fighter 2010: The Final Fight";
                case "FDLJ": return "Street Fighter 2010: The Final Fight";
                case "FDLP": return "Street Fighter 2010: The Final Fight";
                case "FDMJ": return "Devil World";
                case "FDMP": return "Devil World";
                case "FDNJ": return "Wai Wai World 2: SOS!! Parsley Jō";
                case "FDPJ": return "Ganbare Goemon Gaiden 2: Tenka no Zaihō";
                case "FDQJ": return "Trojan";
                case "FDRJ": return "Yokai Dochuki";
                case "FDSE": return "Crash 'n' the Boys: Street Challenge";
                case "FDSJ": return "Crash 'n' the Boys: Street Challenge";
                case "FDSP": return "Crash 'n' the Boys: Street Challenge";
                case "FDTE": return "Shadow of the Ninja";
                case "FDTP": return "Shadow of the Ninja";
                case "FDUE": return "S.C.A.T.";
                case "FDUP": return "S.C.A.T.";
                case "FDVJ": return "Konami's Ping Pong";
                case "FDWJ": return "Famicom Wars";
                case "FDXJ": return "Valkyrie no Bōken: Toki no Kagi Densetsu";
                case "FDYE": return "Sky Kid";
                case "FDYJ": return "Sky Kid";
                case "FDZJ": return "Pooyan";
                case "FE2E": return "StarTropics";
                case "FE2P": return "StarTropics";
                case "FE3E": return "Zoda's Revenge: StarTropics II";
                case "FE3P": return "Zoda's Revenge: StarTropics II";
                case "FE4E": return "City Connection";
                case "FE4J": return "City Connection";
                case "FE5E": return "Ninja Gaiden II: The Dark Sword of Chaos";
                case "FE5P": return "Ninja Gaiden II: The Dark Sword of Chaos";
                case "FE6E": return "Ninja Gaiden III: The Ancient Ship of Doom";
                case "FE6P": return "Ninja Gaiden III: The Ancient Ship of Doom";
                case "FE7E": return "Tecmo Bowl";
                case "FE7P": return "Tecmo Bowl";
                case "FE8E": return "Double Dragon III: The Sacred Stones";
                case "FE8P": return "Double Dragon III: The Sacred Stones";
                case "FEAJ": return "Atlantis no Nazo";
                case "FEBE": return "Blaster Master";
                case "FEBJ": return "Blaster Master";
                case "FEBP": return "Blaster Master";
                case "FECJ": return "Bio Miracle Bokutte Upa";
                case "FEDJ": return "Championship Lode Runner";
                case "FEEJ": return "Tōkaidō Gojūsan-tsugi";
                case "FEFJ": return "Wagan Land 2";
                case "FEGJ": return "Metro-Cross";
                case "FEHE": return "Duck Hunt";
                case "FEHJ": return "Duck Hunt";
                case "FEHP": return "Duck Hunt";
                case "FEJJ": return "Star Luster";
                case "FEKJ": return "Ganbare Goemon Gaiden: Kieta Ōgon Kiseru";
                case "FELJ": return "Field Combat";
                case "FEME": return "Little Ninja Brothers";
                case "FEMJ": return "Little Ninja Brothers";
                case "FEMP": return "Little Ninja Brothers";
                case "FENE": return "Flying Warriors";
                case "FENJ": return "Flying Dragon 2: Dragon's Wings";
                case "FENP": return "Flying Warriors";
                case "FEPJ": return "MagMax";
                case "FEQJ": return "Formation Z";
                case "FERJ": return "Exerion";
                case "FESE": return "River City Ransom";
                case "FESJ": return "River City Ransom";
                case "FESP": return "Street Gangs";
                case "FETJ": return "Metal Slader Glory";
                case "FEUE": return "Wild Gunman";
                case "FEUJ": return "Nintendo Zapper";
                case "FEUP": return "Wild Gunman";
                case "FEVE": return "Baseball Simulator 1000";
                case "FEVJ": return "Baseball Simulator 1000";
                case "FEWE": return "Dig Dug II";
                case "FEWJ": return "Dig Dug II";
                case "FEWP": return "Dig Dug II";
                case "FEXJ": return "King's Knight";
                case "FEYJ": return "Front Line";
                case "FEZE": return "VS. Excitebike";
                case "FEZJ": return "VS. Excitebike";
                case "FHAE": return "Hogan's Alley";
                case "FHAJ": return "Hogan's Alley";
                case "FHAP": return "Hogan's Alley";
                default: return "[Unknown]";
            }
        }

        public static string GetReleaseDate(string productCode)
        {
            switch (productCode)
            {
                case "FA2J": return "2013-05-15";
                case "FA4E": return "2013-06-20";
                case "FA4J": return "2013-06-19";
                case "FA4P": return "2013-06-20";
                case "FA5J": return "2013-09-18";
                case "FA6E": return "2013-08-15";
                case "FA6J": return "2013-07-24";
                case "FA6P": return "2013-08-08";
                case "FA7E": return "2013-06-11";
                case "FA7J": return "2013-06-12";
                case "FA7P": return "2013-06-11";
                case "FA8E": return "2013-07-11";
                case "FA8J": return "2013-08-14";
                case "FA8P": return "2013-07-11";
                case "FA9E": return "2014-03-13";
                case "FA9J": return "2013-08-08";
                case "FA9P": return "2014-01-23";
                case "FAAE": return "2013-09-12";
                case "FAAJ": return "2013-06-05";
                case "FABE": return "2013-12-26";
                case "FABJ": return "2013-12-25";
                case "FACE": return "2013-04-26";
                case "FACJ": return "2013-04-27";
                case "FADE": return "2013-04-17";
                case "FADJ": return "2013-04-17";
                case "FADP": return "2013-04-18";
                case "FAEE": return "2013-06-20";
                case "FAEJ": return "2013-05-29";
                case "FAFE": return "2013-07-15";
                case "FAFJ": return "2013-07-17";
                case "FAGE": return "2013-04-26";
                case "FAGJ": return "2013-04-27";
                case "FAGP": return "2013-04-27";
                case "FAHE": return "2013-05-16";
                case "FAHJ": return "2014-03-19";
                case "FAHP": return "2013-05-16";
                case "FAJE": return "2013-01-23";
                case "FAJJ": return "2013-04-27";
                case "FAJP": return "2013-06-27";
                case "FAKE": return "2013-03-20";
                case "FAKJ": return "2013-06-05";
                case "FALE": return "2013-05-09";
                case "FALJ": return "2013-04-27";
                case "FAME": return "2013-06-12";
                case "FAMJ": return "2013-06-12";
                case "FAMP": return "2013-06-13";
                case "FANE": return "2013-05-02";
                case "FANJ": return "2013-06-12";
                case "FAPE": return "2013-06-11";
                case "FAPJ": return "2013-06-12";
                case "FAQJ": return "2013-04-27";
                case "FARE": return "2013-05-02";
                case "FARJ": return "2013-05-15";
                case "FASE": return "2013-06-06";
                case "FASJ": return "2013-04-27";
                case "FATE": return "2013-09-26";
                case "FATJ": return "2013-10-02";
                case "FAUJ": return "2013-09-04";
                case "FAVE": return "2014-12-04";
                case "FAVJ": return "2014-09-17";
                case "FAWE": return "2013-04-26";
                case "FAWJ": return "2013-07-17";
                case "FAXJ": return "2013-06-19";
                case "FAYE": return "2013-05-09";
                case "FAYJ": return "2013-05-01";
                case "FAZE": return "2013-06-11";
                case "FAZJ": return "2013-05-01";
                case "FAZP": return "2013-06-11";
                case "FB2E": return "2014-06-18";
                case "FB2J": return "2013-12-18";
                case "FB2P": return "2014-03-13";
                case "FB3E": return "2013-12-19";
                case "FB3J": return "2013-12-04";
                case "FB3P": return "2014-03-20";
                case "FB4E": return "2014-03-06";
                case "FB4J": return "2014-01-15";
                case "FB4P": return "2014-02-06";
                case "FB5E": return "2014-03-27";
                case "FB5J": return "2014-02-26";
                case "FB5P": return "2014-02-13";
                case "FB6E": return "2014-02-27";
                case "FB6J": return "2014-01-15";
                case "FB6P": return "2014-03-06";
                case "FB7E": return "2013-12-12";
                case "FB7J": return "2014-01-22";
                case "FB7P": return "2014-03-13";
                case "FB8E": return "2014-01-16";
                case "FB8J": return "2014-03-05";
                case "FB8P": return "2014-05-01";
                case "FB9J": return "2014-02-19";
                case "FBAE": return "2013-08-29";
                case "FBAJ": return "2013-08-28";
                case "FBAP": return "2013-08-29";
                case "FBBE": return "2013-07-25";
                case "FBBJ": return "2013-08-14";
                case "FBBP": return "2013-07-11";
                case "FBCE": return "2013-09-12";
                case "FBCJ": return "2013-09-11";
                case "FBCP": return "2013-09-26";
                case "FBDE": return "2015-06-14";
                case "FBDJ": return "2015-06-15";
                case "FBDP": return "2015-06-15";
                case "FBEJ": return "2013-05-22";
                case "FBGE": return "2014-09-11";
                case "FBGJ": return "2014-09-24";
                case "FBGP": return "2014-07-03";
                case "FBHE": return "2013-05-30";
                case "FBHJ": return "2013-07-03";
                case "FBHP": return "2013-05-30";
                case "FBKJ": return "2013-08-21";
                case "FBLE": return "2013-10-24";
                case "FBLJ": return "2013-10-23";
                case "FBLP": return "2013-10-24";
                case "FBME": return "2013-10-10";
                case "FBMJ": return "2013-10-30";
                case "FBMP": return "2013-10-10";
                case "FBNE": return "2013-10-24";
                case "FBNJ": return "2013-10-23";
                case "FBNP": return "2013-10-24";
                case "FBPE": return "2013-10-17";
                case "FBPJ": return "2013-10-23";
                case "FBPP": return "2013-10-17";
                case "FBQE": return "2013-10-17";
                case "FBQJ": return "2013-11-20";
                case "FBQP": return "2013-10-17";
                case "FBRE": return "2013-09-26";
                case "FBRJ": return "2013-11-06";
                case "FBRP": return "2013-10-24";
                case "FBSE": return "2013-10-10";
                case "FBSJ": return "2013-11-13";
                case "FBSP": return "2013-10-10";
                case "FBTJ": return "2013-11-13";
                case "FBUE": return "2014-02-20";
                case "FBUJ": return "2013-12-11";
                case "FBUP": return "2014-02-20";
                case "FBVE": return "2013-11-07";
                case "FBVJ": return "2014-01-29";
                case "FBVP": return "2014-02-27";
                case "FBWJ": return "2014-01-29";
                case "FBXJ": return "2013-12-11";
                case "FBYJ": return "2014-01-08";
                case "FBZJ": return "2013-12-04";
                case "FC2E": return "2014-07-10";
                case "FC2J": return "2014-10-22";
                case "FC3J": return "2014-09-17";
                case "FC4J": return "2014-06-19";
                case "FC5E": return "2014-08-07";
                case "FC5J": return "2014-04-23";
                case "FC5P": return "2014-07-24";
                case "FC6E": return "2014-10-30";
                case "FC6J": return "2014-05-21";
                case "FC6P": return "2014-09-04";
                case "FC7J": return "2014-07-30";
                case "FC8E": return "2014-08-14";
                case "FC8J": return "2016-09-14";
                case "FC8P": return "2014-08-21";
                case "FC9E": return "2014-08-21";
                case "FC9J": return "2014-10-08";
                case "FC9P": return "2014-09-18";
                case "FCAJ": return "2014-05-28";
                case "FCBE": return "2014-02-06";
                case "FCBJ": return "2014-03-26";
                case "FCBP": return "2014-03-27";
                case "FCCE": return "2014-01-23";
                case "FCCJ": return "2014-02-05";
                case "FCCP": return "2014-03-27";
                case "FCDJ": return "2014-07-02";
                case "FCEE": return "2014-02-13";
                case "FCEJ": return "2014-03-05";
                case "FCEP": return "2014-06-19";
                case "FCFE": return "2014-03-20";
                case "FCFJ": return "2014-03-12";
                case "FCFP": return "2014-05-08";
                case "FCGJ": return "2014-06-04";
                case "FCHE": return "2014-06-26";
                case "FCHJ": return "2014-04-16";
                case "FCHP": return "2014-09-04";
                case "FCJJ": return "2014-04-09";
                case "FCKJ": return "2014-06-04";
                case "FCLJ": return "2014-03-19";
                case "FCME": return "2016-07-07";
                case "FCMJ": return "2014-09-10";
                case "FCMP": return "2015-03-05";
                case "FCNJ": return "2014-08-20";
                case "FCPE": return "2014-06-12";
                case "FCPJ": return "2014-12-03";
                case "FCPP": return "2014-06-12";
                case "FCQE": return "2014-05-01";
                case "FCQJ": return "2014-10-01";
                case "FCQP": return "2014-08-07";
                case "FCRJ": return "2014-06-19";
                case "FCSE": return "2014-08-21";
                case "FCSJ": return "2014-05-14";
                case "FCSP": return "2014-07-24";
                case "FCTJ": return "2014-07-09";
                case "FCUJ": return "2014-05-07";
                case "FCVJ": return "2014-05-14";
                case "FCWE": return "2017-01-26";
                case "FCWJ": return "2014-07-23";
                case "FCWP": return "2015-03-05";
                case "FCXE": return "2014-05-15";
                case "FCXJ": return "2014-09-03";
                case "FCXP": return "2014-08-21";
                case "FCYJ": return "2014-05-28";
                case "FCZE": return "2014-06-10";
                case "FCZJ": return "2014-06-11";
                case "FCZP": return "2014-06-12";
                case "FD2E": return "2016-01-07";
                case "FD2J": return "2016-09-21";
                case "FD2P": return "2015-11-26";
                case "FD3J": return "2015-04-08";
                case "FD4J": return "2015-07-22";
                case "FD5J": return "2015-03-11";
                case "FD6E": return "2015-11-05";
                case "FD6J": return "2015-10-28";
                case "FD7J": return "2015-06-03";
                case "FD8J": return "2015-07-01";
                case "FD9E": return "2015-02-05";
                case "FD9J": return "2015-05-13";
                case "FD9P": return "2015-02-12";
                case "FDAE": return "2014-07-24";
                case "FDAJ": return "2015-01-28";
                case "FDAP": return "2014-10-09";
                case "FDBJ": return "2014-10-22";
                case "FDCJ": return "2014-07-02";
                case "FDDE": return "2014-08-28";
                case "FDDJ": return "2015-04-15";
                case "FDDP": return "2015-01-22";
                case "FDEE": return "2015-02-05";
                case "FDEJ": return "2014-10-15";
                case "FDEP": return "2015-01-08";
                case "FDFJ": return "2015-01-21";
                case "FDGJ": return "2014-10-08";
                case "FDHJ": return "2014-11-19";
                case "FDJJ": return "2014-10-29";
                case "FDKE": return "2014-11-27";
                case "FDKJ": return "2015-02-10";
                case "FDKP": return "2014-12-18";
                case "FDLE": return "2015-01-15";
                case "FDLJ": return "2014-10-15";
                case "FDLP": return "2014-12-18";
                case "FDMJ": return "2014-11-05";
                case "FDMP": return "2014-10-30";
                case "FDNJ": return "2015-09-02";
                case "FDPJ": return "2015-05-20";
                case "FDQJ": return "2016-08-31";
                case "FDRJ": return "2015-02-25";
                case "FDSE": return "2014-12-11";
                case "FDSJ": return "2014-11-12";
                case "FDSP": return "2014-12-04";
                case "FDTE": return "2015-01-29";
                case "FDTP": return "2014-12-04";
                case "FDUE": return "2015-01-22";
                case "FDUP": return "2014-12-04";
                case "FDVJ": return "2015-03-18";
                case "FDWJ": return "2014-12-03";
                case "FDXJ": return "2015-02-04";
                case "FDYE": return "2015-03-05";
                case "FDYJ": return "2015-03-04";
                case "FDZJ": return "2015-06-10";
                case "FE2E": return "2016-05-26";
                case "FE2P": return "2015-09-03";
                case "FE3E": return "2016-05-26";
                case "FE3P": return "2015-09-03";
                case "FE4E": return "2016-03-17";
                case "FE4J": return "2017-03-29";
                case "FE5E": return "2016-02-18";
                case "FE5P": return "2015-11-26";
                case "FE6E": return "2016-02-18";
                case "FE6P": return "2015-11-26";
                case "FE7E": return "2015-09-10";
                case "FE7P": return "2015-09-17";
                case "FE8E": return "2016-02-18";
                case "FE8P": return "2015-11-26";
                case "FEAJ": return "2015-04-22";
                case "FEBE": return "2015-07-16";
                case "FEBJ": return "2015-05-27";
                case "FEBP": return "2015-02-12";
                case "FECJ": return "2015-07-15";
                case "FEDJ": return "2015-07-08";
                case "FEEJ": return "2015-10-07";
                case "FEFJ": return "2015-11-11";
                case "FEGJ": return "2015-11-25";
                case "FEHE": return "2014-12-25";
                case "FEHJ": return "2014-12-24";
                case "FEHP": return "2014-12-25";
                case "FEJJ": return "2015-11-04";
                case "FEKJ": return "2015-10-21";
                case "FELJ": return "2015-11-25";
                case "FEME": return "2017-01-26";
                case "FEMJ": return "2016-10-19";
                case "FEMP": return "2015-05-28";
                case "FENE": return "2017-01-26";
                case "FENJ": return "2016-09-21";
                case "FENP": return "2015-05-28";
                case "FEPJ": return "2016-09-14";
                case "FEQJ": return "2017-03-29";
                case "FERJ": return "2015-09-16";
                case "FESE": return "2015-10-01";
                case "FESJ": return "2015-08-05";
                case "FESP": return "2015-04-23";
                case "FETJ": return "2015-07-01";
                case "FEUE": return "2016-01-07";
                case "FEUJ": return "2016-06-22";
                case "FEUP": return "2015-10-22";
                case "FEVE": return "2016-07-07";
                case "FEVJ": return "2016-10-26";
                case "FEWE": return "2016-03-17";
                case "FEWJ": return "2016-09-07";
                case "FEWP": return "2015-10-08";
                case "FEXJ": return "2016-07-06";
                case "FEYJ": return "2016-06-29";
                case "FEZE": return "2015-08-31";
                case "FEZJ": return "2015-09-16";
                case "FHAE": return "2016-01-07";
                case "FHAJ": return "2016-06-22";
                case "FHAP": return "2015-10-22";
                default: return "[Unknown]";
            }
        }
    }
}
