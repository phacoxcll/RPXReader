using System;
using System.IO;
using System.Linq;
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
                //strBuilder.AppendLine("  Title: \"" + GetTitleWithRegion(ProductCode) + "\"");
                //strBuilder.AppendLine("  Release date:       " + GetReleaseDate(ProductCode));
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
        //public uint CRCsSum
        //{ private set; get; }

        public RPXNES(string filename, bool readInfo = true)
            : base(filename, false)
        {
            ROM = new ROMInfo();
            Type = VCType.Unknown;
            //CRCsSum = 0;

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
            //long sum = 0;
            for (int i = 0; i < SectionHeader.Length; i++)
            {
                //sum += CRC[i];
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
            //sum -= (long)CRC[2] + CRC[3] + CRC[CRC.Length - 1];
            //CRCsSum = (uint)(sum >> 4);
            MD5 = GetMD5();
        }

        private string GetMD5()
        {
            StringBuilder strBuilder = new StringBuilder();

            if (SectionHeader.Length != 0)
            {
                //strBuilder.Append(CRC[0].ToString("X8"));//NULL
                strBuilder.Append(CRC[1].ToString("X8"));//.syscall
                //strBuilder.Append(CRC[2].ToString("X8"));//.text
                //strBuilder.Append(CRC[3].ToString("X8"));//.rodata
                strBuilder.Append(CRC[4].ToString("X8"));//.data (*)
                strBuilder.Append(CRC[5].ToString("X8"));//.module_id
                //strBuilder.Append(CRC[6].ToString("X8"));//.bss
                strBuilder.Append(CRC[7].ToString("X8"));//.rela.rodata (*)
                strBuilder.Append(CRC[8].ToString("X8"));//.rela.text (*)
                strBuilder.Append(CRC[9].ToString("X8"));//.rela.data (*)
                strBuilder.Append(CRC[10].ToString("X8"));//.fimport_nn_act

                VCType type = GetVCType();
                if (type == VCType.A)
                {
                    //strBuilder.Append(CRC[].ToString("X8"));//.dimport_nn_act
                    strBuilder.Append(CRC[11].ToString("X8"));//.fimport_sysapp
                    strBuilder.Append(CRC[12].ToString("X8"));//.fimport_zlib125
                    strBuilder.Append(CRC[13].ToString("X8"));//.fimport_gx2
                    strBuilder.Append(CRC[14].ToString("X8"));//.fimport_snd_core
                    //strBuilder.Append(CRC[15].ToString("X8"));//.dimport_snd_core
                    strBuilder.Append(CRC[16].ToString("X8"));//.fimport_snd_user
                    //strBuilder.Append(CRC[17].ToString("X8"));//.dimport_snd_user
                    strBuilder.Append(CRC[18].ToString("X8"));//.fimport_nn_save
                    strBuilder.Append(CRC[19].ToString("X8"));//.fimport_vpad
                    strBuilder.Append(CRC[20].ToString("X8"));//.fimport_proc_ui
                    strBuilder.Append(CRC[21].ToString("X8"));//.fimport_padscore
                    strBuilder.Append(CRC[22].ToString("X8"));//.fimport_coreinit
                    strBuilder.Append(CRC[23].ToString("X8"));//.dimport_coreinit
                    strBuilder.Append(CRC[24].ToString("X8"));//.fimport_mic
                    strBuilder.Append(CRC[25].ToString("X8"));//.symtab (*)
                    strBuilder.Append(CRC[26].ToString("X8"));//.strtab
                    strBuilder.Append(CRC[27].ToString("X8"));//.shstrtab
                    //strBuilder.Append(CRC[28].ToString("X8"));//CRCs
                    //strBuilder.Append(CRC[29].ToString("X8"));//RPL Info
                }
                else if (type == VCType.B)
                {
                    //strBuilder.Append(CRC[11].ToString("X8"));//.dimport_nn_act
                    strBuilder.Append(CRC[12].ToString("X8"));//.fimport_sysapp
                    strBuilder.Append(CRC[13].ToString("X8"));//.fimport_zlib125
                    strBuilder.Append(CRC[14].ToString("X8"));//.fimport_gx2
                    strBuilder.Append(CRC[15].ToString("X8"));//.fimport_snd_core
                    //strBuilder.Append(CRC[].ToString("X8"));//.dimport_snd_core
                    strBuilder.Append(CRC[16].ToString("X8"));//.fimport_snd_user
                    //strBuilder.Append(CRC[].ToString("X8"));//.dimport_snd_user
                    strBuilder.Append(CRC[17].ToString("X8"));//.fimport_nn_save
                    strBuilder.Append(CRC[18].ToString("X8"));//.fimport_vpad
                    strBuilder.Append(CRC[19].ToString("X8"));//.fimport_proc_ui
                    strBuilder.Append(CRC[20].ToString("X8"));//.fimport_padscore
                    strBuilder.Append(CRC[21].ToString("X8"));//.fimport_coreinit
                    strBuilder.Append(CRC[22].ToString("X8"));//.dimport_coreinit
                    strBuilder.Append(CRC[23].ToString("X8"));//.fimport_mic
                    strBuilder.Append(CRC[24].ToString("X8"));//.symtab (*)
                    strBuilder.Append(CRC[25].ToString("X8"));//.strtab
                    strBuilder.Append(CRC[26].ToString("X8"));//.shstrtab
                    //strBuilder.Append(CRC[27].ToString("X8"));//CRCs
                    //strBuilder.Append(CRC[28].ToString("X8"));//RPL Info
                }
                else
                {
                    for (int i = 11; i < SectionHeader.Length; i++)
                        strBuilder.Append(CRC[i].ToString("X8"));
                }
            }

            byte[] hash;
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            hash = md5.ComputeHash(Encoding.ASCII.GetBytes(strBuilder.ToString()));

            return BitConverter.ToString(hash).Replace("-", string.Empty);
        }

        public override string ToString()
        {
            StringBuilder strBuilder = new StringBuilder();

            strBuilder.AppendLine("MD5: " + MD5);
            strBuilder.AppendLine("Base title: " + GetBase(MD5));
            strBuilder.AppendLine();
            strBuilder.AppendLine(Header.ToString());
            strBuilder.Append(Info.ToString());
            strBuilder.AppendLine("  mSrcFileName: \"" + SrcFileName + "\"");
            strBuilder.AppendLine("  mTags: ");
            for (int i = 0; i < Tags.Count; i++)
                strBuilder.AppendLine("    " + Tags[i]);
            strBuilder.AppendLine();
            //strBuilder.AppendLine("VC NES:");
            //strBuilder.AppendLine("  Type: " + Type.ToString());
            //strBuilder.AppendLine("  CRCs sum: 0x" + CRCsSum.ToString("X8"));
            //strBuilder.AppendLine();
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

        new public string GetBase(string md5)
        {
            switch (md5)
            {
                case "652E7014F497E1B79FAAB09B6D2400FF": return "Adventure Island (USA) [FBGE]";
                case "F6F59E9A464C34E6571D35BCC97C872C": return "Adventures of Lolo (USA) [FCXE]";
                case "35A39DA573D0BDC417446C2FEE68570D": return "Antarctic Adventure (けっきょく南極大冒険) (JPN) [FC4J]";
                case "B5149296F6D4B9787DCDF0A7DAE00C9F": return "Atlantis no Nazo (アトランチスの謎) (JPN) [FEAJ]";
                case "AAE308A89498F28099DF5A205C25991B": return "Balloon Fight (USA) [FAJE]";
                case "FDEAD24E63C59DFE9B02E08D66EC25CB": return "Balloon Fight (EUR) [FAJP]";
                case "C41B103DAEA1AAF28492EFB4E3E8432C": return "Baseball Simulator 1000 (USA) [FEVE]";
                case "29271881D5A54EC561A4389B97F545AF": return "Bases Loaded (USA) [FC2E]";
                case "7DE08C8FD6DFE27D0B8953D3C7BBA2A9": return "Bases Loaded (燃えろ!!プロ野球) (JPN) [FC2J]";
                case "7A529BFBF4492C583BFDECB0C8F9C5C6": return "Battle City (バトルシティー) (JPN) [FCTJ]";
                case "1732135298D459FF603DCC507531F8BE": return "Blaster Master (USA) [FEBE]";
                case "1693E23D891B9EC7A84B9AB206DB08CA": return "Blaster Master (EUR) [FEBP]";
                case "9DECB2A246E9DFEB6CE4B1BA35E5C2B4": return "Blaster Master (超惑星戦記 メタファイト) (JPN) [FEBJ]";
                case "8B05747621DF5751D837176863FE9A0E": return "Bubble Bobble (JPN) [FBWJ]";
                case "9343C7BB23EE0C6F53A0321161C0ECC3": return "Castlevania (USA) [FB3E]";
                case "EC377F250D53505921B6ABB8669D483E": return "Castlevania (悪魔城ドラキュラ) (JPN) [FB3J]";
                case "81B2FDB2A6756A5F13DE5706A5C8F95A": return "Castlevania II: Simon's Quest (USA) [FB8E]";
                case "33F236D341F2B0FC29A25F47280AAB84": return "Castlevania II: Simon's Quest (ドラキュラⅡ 呪いの封印) (JPN) [FB8J]";
                case "111FC3440F05A9E0B31E49FF3F945477": return "Castlevania II: Simon's Redaction [0005000010105050] (USA) [FB8E - Castlevania II: Simon's Quest]";
                case "71150889F1268AC4A071D4119F5D3C67": return "Castlevania III: Dracula's Curse (USA) [FCHE]";
                case "42F9ABC6AC247C0A43B61728DD89C738": return "Castlevania III: Dracula's Curse (悪魔城伝説) (JPN/EUR) [FCHJ]";
                case "24C8083A1727CAA338C3D61F089FD508": return "Castlevania III: Dracula's Curse DX [0005000010104340] (USA) [FCHE - Castlevania III: Dracula's Curse]";
                case "B99498BF93E6E20DBF1D100FC9C2E28F": return "Castlevania RPG [0005000010105425] (USA) [FB3E - Castlevania]";
                case "A37671BA78D0592B90F64620D286BCE0": return "Championship Lode Runner (チャンピオンシップ・ロードランナー) (JPN) [FEDJ]";
                case "91AD2FEE77DA4716991FD2BCE5C4BEE2": return "City Connection (USA) [FE4E]";
                case "1755FFDD0705B5967423CA46AD6A43CD": return "City Connection (シティコネクション) (JPN) [FE4J]";
                case "21B235C5F222F50D1FE28FCB56E7AD9F": return "Clu Clu Land (クルクルランド) (JPN) [FBQJ]";
                case "E1069114111D7DC7F635D5CD3D28D47A": return "Crash 'n' the Boys: Street Challenge (USA) [FDSE]";
                case "78F6777FB88E0CE6E754043E06ADBB12": return "Crystalis Rebalanced [0005000010105431] (USA) [FBDE - EarthBound Beginnings]";
                case "03A58B4F590D511A1513126C713C0F18": return "Devil World (デビルワールド) (JPN/EUR) [FDMJ]";
                case "E4DC3CB680360C38B7DB194877E4F25E": return "Dig Dug (USA) [FDEE]";
                case "8981B4518CFE10A5F2E2870F2804359B": return "Dig Dug II (USA) [FEWE]";
                case "732A2BED8C969CAE5E0B64DAB1773847": return "Dig Dug II (ディグダグⅡ) (JPN) [FEWJ]";
                case "082B69B4635A9AAFD17A287321F1F4A0": return "Donkey Kong (USA) [FAFE]";
                case "7CD633CDA1E6626A26F72066E7A5D49D": return "Donkey Kong 3 (USA) [FBRE]";
                case "29177D0613AB5F478DB23BA77DCD3D06": return "Donkey Kong 3 (ドンキーコング3) (JPN) [FBRJ]";
                case "1D7F0126C6EE1825065772DA476192CF": return "Donkey Kong Jr. (USA) [FAWE]";
                case "69234809EFAF50DA34F403B7E6CC9657": return "Donkey Kong Jr. Math (USA) [FDDE]";
                case "8B6C563FEE2A605DA70C213136AC35CD": return "Double Dragon (USA) [FB7E]";
                case "0F0490C752D7D7CEFDC9A050E17667B0": return "Double Dragon II: The Revenge (USA) [FC8E]";
                case "DBD0662372951E3E53EB92C094DDF692": return "Double Dragon III: The Sacred Stones (USA) [FE8E]";
                case "6214636C0F49246F87B8A7026A608D5B": return "Downtown Special (ダウンタウンスペシャル くにおくんの時代劇だよ全員集合！) (JPN) [FBZJ]";
                case "760CF151BE14CFDBF997230B375122BB": return "Dr. Mario (USA) [FB5E]";
                case "634CD59B97FB624D9C1B7A9C938E75DE": return "Dr. Mario (EUR) [FB5P]";
                case "9BCF855CD4DFACC88484CE0F56A82F16": return "Dragon Buster (ドラゴンバスター) (JPN) [FDFJ]";
                case "C145D7AC3A474ABA49326896B81C0701": return "Duck Hunt (USA) [FEHE]";
                case "AC4EF67414BDC34832F92F1983D5E95A": return "EarthBound Beginnings (USA) [FBDE]";
                case "39D665DA8A9AB487E9A1769EA71919B9": return "Elevator Action (JPN) [FB9J]";
                case "620B546DE8CC4DB30A591E6EC5C9D85A": return "Esper Dream (エスパードリーム) (JPN) [FD4J]";
                case "31BF54B7220CD024639B4801A9CA1986": return "Excitebike (USA) [FAGE]";
                case "1815486F248B4FDF0E373AD0C314A281": return "Excitebike (エキサイトバイク) (JPN) [FAGJ]";
                case "2B54E0379A220315229136E8492B41AE": return "Exerion (エクセリオン) (JPN) [FERJ]";
                case "D4B1D5D09FD8AC637CEC4426C1308E3A": return "Famicom Tantei Club: Kieta Kōkeisha (ファミコン探偵倶楽部 消えた後継者(前後編)) (JPN) [FCAJ]";
                case "3C27B25FDB0609295590F2FFD6CE8EED": return "Famicom Wars (ファミコンウォーズ) (JPN) [FDWJ]";
                case "A454D8BC4A95602962AB1CE6066F3731": return "Field Combat (フィールドコンバット) (JPN) [FELJ]";
                case "485DF17A9176D8E88B81388C2CE09415": return "Final Fantasy (ファイナルファンタジー) (JPN) [FBTJ]";
                case "ED710F7B303C721F73E3422C4FB18F77": return "Final Fantasy Restored [0005000010105567] (USA) [FAPE - Mega Man 2]";
                case "37EE1547441DF9B0B72B9E598600068B": return "Fire Emblem Gaiden (ファイアーエムブレム 外伝) (JPN) [FCNJ]";
                case "4E78ABB10657F57E69CEBA893BE8CF76": return "Fire Emblem Gaiden Spanish [0005000010102137] (USA) [FBVE - Wario's Woods]";
                case "C7F576214DA517E669743972624C0CEF": return "Flying Dragon: The Secret Scroll (USA) [FCME]";
                case "338D83AFCEBA332B6E965FED14D50DD3": return "Flying Warriors (USA) [FENE]";
                case "88BBC766325103D2A040B82B414BA596": return "Front Line (フロントライン) (JPN) [FEYJ]";
                case "A6DEE3029117AFC73B96714C80144AE9": return "Galaga (USA) [FA6E]";
                case "2B5BD3F2DF6DB9241FA7B8B7E1BCE197": return "Galaga (ギャラガ) (JPN) [FA6J]";
                case "AD46076E63AD942F15876D8486DF1778": return "Ganbare Goemon 2 (がんばれゴエモン２) (JPN) [FD5J]";
                case "9ADFFE130D65161F05BBBBDB6C69C604": return "Ganbare Goemon Gaiden 2 (がんばれゴエモン外伝2 天下の財宝) (JPN) [FDPJ]";
                case "66773447185A6E4F3ABBA2BDF855E4E5": return "Ganbare Goemon Gaiden (がんばれゴエモン外伝  きえた黄金キセル) (JPN) [FEKJ]";
                case "CBBA95368443EEDA33E7943BA58CA406": return "Gargoyle's Quest II: The Demon Darkness (USA) [FC6E]";
                case "7EAA32C861FE117410C8FC5CF3DF4A74": return "Ghosts 'n Goblins (USA) [FBHE]";
                case "9C156AFD8F6071456D08C91F95310E6D": return "Golf (USA) [FBSE]";
                case "4FA40681ECFA74205AE76EE32BB20546": return "Golf (ゴルフ) (JPN) [FBSJ]";
                case "76BE8D12B0B2EC0047490691DC3256B3": return "Gradius (USA) [FATE]";
                case "D620574D4D8F2B7B3A5C8F596DE85537": return "Gradius (グラディウス) (JPN) [FATJ]";
                case "74B24A240B8D90B9D7F334478BB748AE": return "Hanjuku Hero (半熟英雄) (JPN) [FCJJ]";
                case "748D678174190ECB74AE255EC73CF04F": return "Hogan's Alley (USA) [FHAE]";
                case "B003E0915FB16824785D872B181A28C0": return "Ice Hockey (USA) [FBUE]";
                case "2AB613C7B3F5D6A779340C25CF155E60": return "Ice Hockey (アイスホッケー) (JPN) [FBUJ]";
                case "CC8E5F02656DF92C332EFBF802A89276": return "Kid Icarus (USA) [FBBE]";
                case "D1D3FA325510644879600CF59F8CDA61": return "King's Knight (キングスナイト) (JPN) [FEXJ]";
                case "519424BDDB872A802953D81AFA62FDDD": return "Kirby's Adventure (USA) [FADE]";
                case "A3612923D4601E2FA158405E7556615B": return "Kirby's Adventure (星のカービィ　夢の泉の物語) (JPN) [FADJ]";
                case "5A78695BEF5C07A765C42A7F05216A6B": return "Konami's Ping Pong (スマッシュピンポン) (JPN) [FDVJ]";
                case "1E575BDA934D87394F888E48DA3580EB": return "Kung Fu Heroes (スーパーチャイニーズ) (JPN) [FCWJ]";
                case "48247BD34809A62EFCFACB82846E7F73": return "Kung-Fu Heroes (USA) [FCWE]";
                case "6BB5BA44771CF7210D56805997FDCB65": return "Kung-Fu Heroes [0005000010101779] (USA) [FAKE - Punch-Out!!]";
                case "9F5B8376BCA112199A08F4058EF00E44": return "Life Force (EUR) [FC9P]";
                case "4B28C590C944F4C1EFC35374CB7E41DB": return "Life Force (沙羅曼蛇) (USA) [FC9E]";
                case "79C41092AA751D8ACF009242D8174773": return "Mach Rider (USA) [FCQE]";
                case "B477A5A968FE40DDB2C6BCAF8B3546B8": return "MagMax (マグマックス) (JPN) [FEPJ]";
                case "9A21CD99D26DA43DBB5EEE28DE9CB9E6": return "Mappy (マッピー) (JPN) [FA2J]";
                case "4BFCEBA65550A6EB0803794FB69A5BF8": return "Mappy-Land (USA) [FD9E]";
                case "EF91F681A39A33E428252E0ACFBDA452": return "Mappy-Land (EUR) [FD9P]";
                case "54F9D5F67C28AE527FB518D7437884E5": return "Mappy-Land (マッピーランド) (JPN) [FD9J]";
                case "243D0A75C72B57B16FF4BC7DD5033B3B": return "Mario & Yoshi (ヨッシーのたまご) (JPN/EUR) [FAMJ]";
                case "796ED6A64C6272A2A3E05D13084FB63C": return "Mario Bros. [0005000010105781] (USA) [FAAE - Super Mario Bros.]";
                case "15C788E9EF4D1F70DF4A9245ED3A3402": return "Mega Man (USA) [FANE]";
                case "80EA4F2FF7ECA40B60FA0ACBB6E28887": return "Mega Man 2 (USA) [FAPE]";
                case "BD1B0AE0B754808E1E5ABF5AA1BDBD02": return "Mega Man 3 (USA) [FAZE]";
                case "466AA1E1F88A8940F7541D131CB98BC9": return "Mega Man 4 (USA) [FA7E]";
                case "E0ADEE7020D57BA3DBE5B49AF18F756B": return "Mega Man 5 (USA) [FC5E]";
                case "43C8BE82DBBDEC9C9DA851E5B33308A2": return "Mega Man 6 (USA) [FCSE]";
                case "D76476D73DB92502B39782F69CDD4589": return "Mega Man Redux [0005000010105569] (USA) [FANE - Mega Man]";
                case "DD87A1A46D2A9B3191E8CEE140AC89B8": return "Metal Slader Glory (メタルスレイダーグローリー) (JPN) [FETJ]";
                case "94B0756A7320A0D5842FF9491D1CD7C0": return "Metro-Cross (メトロクロス) (JPN) [FEGJ]";
                case "FDD51DEC55D1989CD9ADA5C0993E9260": return "Metroid (メトロイド) (JPN) [FA8J]";
                case "B66332A9B5959722EB97EC5302EDA96B": return "Mighty Bomb Jack (USA) [FCCE]";
                case "9A572F5C0231E15D06D943E03498BFD7": return "Mighty Bomb Jack (EUR) [FCCP]";
                case "B59488839467EF8FBBCF7A3427AABA4F": return "Mighty Bomb Jack (マイティボンジャック) (JPN) [FCCJ]";
                case "F64F8A8766FC77E3FDA36BF676D13495": return "Mighty Final Fight (USA) [FDKE]";
                case "718362732AD7769453876BF5EBA3F6DD": return "Mother (JPN) [FBDJ]";
                case "6234E10EF32289D217593FDA304D074F": return "NES Open Tournament Golf (USA) [FB4E]";
                case "82FE97BECD1D46ACA57335BB54FA08E7": return "NES Open Tournament Golf (EUR) [FB4P]";
                case "1A14709A8410954EAE9B956FC25B7D0A": return "Ninja Gaiden (USA) [FCBE]";
                case "224B251897A97A1113BA73A637654BC7": return "Ninja Gaiden (EUR) [FCBP]";
                case "8CF20064179B92B3B0229A440089DE03": return "Ninja Gaiden II: The Dark Sword of Chaos (USA) [FE5E]";
                case "5D62C60FC6E92CF7858D8600C69208B7": return "Ninja Gaiden III: The Ancient Ship of Doom (USA) [FE6E]";
                case "69C8E99094CC8B6021967BA580B69506": return "Nintendo World Cup (熱血高校ドッジボール部　サッカー編) (JPN) [FCLJ]";
                case "7702E3A48A21550D6760C9DD3A3A6AB5": return "Nintendo Zapper (ワイルドガンマン) (JPN) [FEUJ]";
                case "DBF7362A97A31556C191DF117561A1C1": return "Pac-Land (USA) [FCZE]";
                case "D910041D60C22CC64A5E2999CEBC330C": return "Pinball (EUR) [FBNP]";
                case "760AEA2C140EC115CC45D8151091FB23": return "Pooyan (プーヤン) (JPN) [FDZJ]";
                case "B733339FCC98C0917FCC262E6EF01B9D": return "Princess Tomato in the Salad Kingdom (サラダの国のトマト姫) (JPN) [FCVJ]";
                case "F072DDB0137DC379C31C7D6F70D0D27F": return "Punch-Out!! (USA) [FAKE]";
                case "C807F7E3A88E439ADDB0171A2092342E": return "Renegade (USA) [FB6E]";
                case "F75E5F4ECB8F3D0CB4C9EF10CAE43B92": return "Renegade (EUR) [FB6P]";
                case "368D5EE5B1F1547A17FABBA878867C4E": return "Renegade (熱血硬派くにおくん) (JPN) [FB6J]";
                case "78D2A1DA75B4043EA129B02D4068A92A": return "River City Ransom (USA) [FESE]";
                case "B24C7B164A0FE03951B38A18A207086E": return "River City Ransom (ダウンタウン熱血物語) (JPN) [FESJ]";
                case "48D6B28AEDBC734E79497D82C51C3688": return "Rockman 4: Aratanaru Yabou!! (ロックマン4 新たなる野望!!) (JPN) [FA7J]";
                case "E6F60F9918D48DDC8ABE24D08D9191D8": return "S.C.A.T. (USA) [FDUE]";
                case "5131FEE629B83AA4B5516E590590A515": return "Seicross (セクロス) (JPN) [FD8J]";
                case "8F28EC0DA872DD082B09B4FF9E0BF48D": return "Shadow of the Ninja (USA) [FDTE]";
                case "A13FD9F4E8DC652FC3FCA59DBE61AD8F": return "Shadow of the Ninja (EUR) [FDTP]";
                case "FE2EE3C3DFD98BAD4FB50385938038D3": return "Shin Onigashima (ふぁみこんむかし話 新･鬼ヶ島(前後編)) (JPN) [FA5J]";
                case "D7ADCF2F763C4B30BB8F1D3539CCC122": return "Sky Kid (USA) [FDYE]";
                case "9EFB4BA737FEEA8B8BEA79925B608C7D": return "Sky Kid (スカイキッド) (JPN) [FDYJ]";
                case "D90C058415666733C939672541E316FF": return "Solomon's Key (USA) [FAYE]";
                case "77AFA384370357688BA97265EAF98E30": return "Star Luster (スターラスター) (JPN) [FEJJ]";
                case "266BF976CCE0179D8D358B90426F29F9": return "StarTropics (USA) [FE2E]";
                case "2C4FC4831BFC0259D0879C9E16B7A383": return "Stinger (USA) [FD6E]";
                case "D25EE9954568857085373EDCD73932CD": return "Super Dodge Ball (USA) [FB2E]";
                case "4A3A159B212442A149F332E3FBE318A2": return "Super Dodge Ball (EUR) [FB2P]";
                case "00EE2DE027F727953BC1202FD5F65371": return "Super Dodge Ball (熱血高校ドッジボール部) (JPN) [FB2J]";
                case "71ECAD41A699CB6193A692F86A39104B": return "Super Mario Bros. (USA) [FAAE]";
                case "6CF9ECAEDDB97A81AF79E5B36D6B40EF": return "Super Mario Bros. 2 (USA) [FAHE]";
                case "CA70846A8A7BAE65E7766B119E8CE6F7": return "Super Mario Bros. USA (スーパーマリオＵＳＡ) (JPN/EUR) [FAHJ]";
                case "279A2708233C22CA7C32DEF7993CA0A6": return "Super Mario Bros. 2 (スーパーマリオブラザーズ２) (JPN) [FA9J]";
                case "B6E7AD0553F47246BD8CF87914A903C3": return "Super Mario Bros. 3 (USA) [FABE]";
                case "1851EB4007F15A3385C3DDCF0307B6DC": return "Super Mario Bros. 3 Extended Edition [0005000010104920] (USA) [FABE - Super Mario Bros. 3]";
                case "B84C38A20300DE9AC3B91C80844B453A": return "Super Mario Bros. DX [0005000010105571] (USA) [FAHE - Super Mario Bros. 2]";
                case "A98D6B63ADBBC747A89CD44AA156E6C8": return "Super Mario Bros.: The Lost Levels (USA) [FA9E]";
                case "0C4C564AC9DDA7A6AF0ED54F849C26CB": return "Tatakai no Banka (闘いの挽歌) (JPN) [FDQJ]";
                case "BE0C1399658FA90CA4B8DFCF2302E540": return "Tetris [0005000010105399] (USA) [FC8E - Double Dragon II: The Revenge]";
                case "1C8FE3EFD5624FC12E30D704B69B2183": return "The Adventures of Bayou Billy (USA) [FD2E]";
                case "4D922FCCE3C93C54FECA577D1ACC8418": return "The Legend of Kage (影の伝説) (JPN) [FCKJ]";
                case "E278643AF523971EFDCE7A54D7153DBB": return "The Legend of Zelda (USA) [FBAE]";
                case "95F0D487E7FEF298B7DE7BBE0BB1DDAC": return "The Legend of Zelda (ゼルダの伝説) (JPN) [FBAJ]";
                case "FB9B6B135988572D1004956758CAACE8": return "The Legend of Zelda ESP [0005000010104712] (USA) [FBAE - The Legend of Zelda]";
                case "FF4F13B1D6261E86BF3B23F387BECA04": return "The Mysterious Murasame Castle (謎の村雨城) (JPN) [FC7J]";
                case "6C042EF00917831CF552D5ADAD5FCD25": return "Tower of Babel (バベルの塔) (JPN) [FDBJ]";
                case "2DF7AC485B63FE7B6FD21F11BBA471E5": return "Tsuppari Ōzumō (つっぱり大相撲) (JPN) [FDGJ]";
                case "5A17B8CEBFA0F768EB31D548E05DF389": return "TwinBee (ツインビー) (JPN) [FAUJ]";
                case "6CBEC9A9278BECE06A75237426F953B2": return "Ufouria: The Saga (EUR) [FDAP]";
                case "A13253B77DD7A4940AA2F43C918A9410": return "Urban Champion (アーバンチャンピオン) (JPN) [FBPJ]";
                case "80C0AF4025CDCF7136DF08FEF633433A": return "Valkyrie no Bōken: Toki no Kagi Densetsu (ワルキューレの冒険 時の鍵伝説) (JPN) [FDXJ]";
                case "D2A7E23637ECE00DFC353AA939DD11E7": return "Volleyball (USA) [FCFE]";
                case "1D1C2519CEAD8F6D6D82F675063FD9CC": return "Volleyball (バレーボール) (JPN) [FCFJ]";
                case "39DB6F9311264543FF38932D604B45E9": return "VS. Excitebike (USA) [FEZE]";
                case "42CBD505024A0896296A0F7A2994A98D": return "Wagan Land (ワギャンランド) (JPN) [FCUJ]";
                case "75DAA3DB4C6751ADC86D3BB9F4B45B6A": return "Wagan Land 2 (ワギャンランド2) (JPN) [FEFJ]";
                case "9EDB1800975F91161B6CB36FD76C642F": return "Wai Wai World 2: SOS!! Parsley Jō (ワイワイワールド2 SOS!!パセリ城) (JPN) [FDNJ]";
                case "5B591B198FECDEBA28BE623425BC8E5D": return "Wario's Woods (USA) [FBVE]";
                case "EF1BB1512D0BEB9E77038F35749C89D1": return "Wario's Woods (ワリオの森) (JPN) [FBVJ]";
                case "E13BE22D906AB5BC201096AA8B7A6159": return "Wild Gunman (USA) [FEUE]";
                case "DD311A666C3D9A537D7DBE4C7D8321EF": return "Wild Gunman (EUR) [FEUP]";
                case "23357668FD6555D2927D27395A7CB687": return "Wrecking Crew (USA) [FA4E]";
                case "FB2B51730A349591DDF9D41DF3E80B29": return "Yie Ar Kung-Fu (イー・アル・カンフー) (JPN) [FC3J]";
                case "0C7F6D19C9C47713FEAC81C5C5692A2A": return "Yōkai Dōchūki (妖怪道中記) (JPN) [FDRJ]";
                case "37F781C9CB30F696A5DE95285B16D162": return "Yoshi (USA) [FAME]";
                case "03D98E06FB9B3B42237C6E6530E2C26E": return "Youkai Douchuuki ENG [0005000010105444] (JPN) [FDRJ - Yōkai Dōchūki (妖怪道中記)]";
                case "B53FD09E9590699895EBD3D801CC706C": return "Zelda II: The Adventure of Link (USA) [FBCE]";
                case "C66FC4AB5A045125E94CC93E2F7D2752": return "Zelda II: The Adventure of Link (リンクの冒険) (JPN) [FBCJ]";
                case "4776E7DCF8B235A26B3EDF558D595192": return "Zelda II: The Adventure of Link Easy [0005000010101775] (USA) [FBCE - Zelda II: The Adventure of Link]";
                case "287714C10768CC1819A55A96D31C7236": return "Zoda's Revenge: StarTropics II (USA) [FE3E]";
                default: return "[Unknown]";
            }
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
