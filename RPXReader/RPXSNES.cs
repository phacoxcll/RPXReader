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
                //strBuilder.AppendLine("  Title: \"" + GetTitleWithRegion(ProductCode) + "\"");
                //strBuilder.AppendLine("  Release date:       " + GetReleaseDate(ProductCode));
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
        //public uint CRCsSum
        //{ private set; get; }

        public RPXSNES(string filename, bool readInfo = true)
            : base(filename, readInfo)
        {
            ROM = new ROMInfo();
            Type = VCType.Unknown;
            //CRCsSum = 0;

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
                if (type == VCType.A1)
                {
                    //strBuilder.Append("N/A\t");//.dimport_nn_act
                    strBuilder.Append(CRC[11].ToString("X8") + "\t");//.fimport_sysapp
                    strBuilder.Append(CRC[12].ToString("X8") + "\t");//.fimport_zlib125
                    strBuilder.Append(CRC[13].ToString("X8") + "\t");//.fimport_gx2
                    strBuilder.Append(CRC[14].ToString("X8") + "\t");//.fimport_snd_core
                    //strBuilder.Append(CRC[15].ToString("X8") + "\t");//.dimport_snd_core
                    strBuilder.Append(CRC[23].ToString("X8") + "\t");//.fimport_snd_user
                    //strBuilder.Append(CRC[24].ToString("X8") + "\t");//.dimport_snd_user
                    strBuilder.Append(CRC[16].ToString("X8") + "\t");//.fimport_nn_save
                    strBuilder.Append(CRC[17].ToString("X8") + "\t");//.fimport_vpad
                    strBuilder.Append(CRC[18].ToString("X8") + "\t");//.fimport_proc_ui
                    strBuilder.Append(CRC[19].ToString("X8") + "\t");//.fimport_padscore
                    strBuilder.Append(CRC[20].ToString("X8") + "\t");//.fimport_coreinit
                    strBuilder.Append(CRC[20].ToString("X8") + "\t");//.dimport_coreinit
                    //strBuilder.Append(CRC[22].ToString("X8") + "\t");//.fimport_mic
                    strBuilder.Append(CRC[25].ToString("X8") + "\t");//.symtab
                    strBuilder.Append(CRC[26].ToString("X8") + "\t");//.strtab
                    strBuilder.Append(CRC[27].ToString("X8") + "\t");//.shstrtab
                    //strBuilder.Append(CRC[28].ToString("X8") + "\t");//CRCs
                    //strBuilder.Append(CRC[29].ToString("X8") + "\t");//RPL Info
                }
                else if (type == VCType.A2)
                {
                    //strBuilder.Append("N/A\t");//.dimport_nn_act
                    strBuilder.Append(CRC[11].ToString("X8") + "\t");//.fimport_sysapp
                    strBuilder.Append(CRC[12].ToString("X8") + "\t");//.fimport_zlib125
                    strBuilder.Append(CRC[13].ToString("X8") + "\t");//.fimport_gx2
                    strBuilder.Append(CRC[14].ToString("X8") + "\t");//.fimport_snd_core
                    //strBuilder.Append(CRC[15].ToString("X8") + "\t");//.dimport_snd_core
                    strBuilder.Append(CRC[22].ToString("X8") + "\t");//.fimport_snd_user
                    //strBuilder.Append(CRC[23].ToString("X8") + "\t");//.dimport_snd_user
                    strBuilder.Append(CRC[16].ToString("X8") + "\t");//.fimport_nn_save
                    strBuilder.Append(CRC[17].ToString("X8") + "\t");//.fimport_vpad
                    strBuilder.Append(CRC[18].ToString("X8") + "\t");//.fimport_proc_ui
                    strBuilder.Append(CRC[19].ToString("X8") + "\t");//.fimport_padscore
                    strBuilder.Append(CRC[20].ToString("X8") + "\t");//.fimport_coreinit
                    strBuilder.Append(CRC[21].ToString("X8") + "\t");//.dimport_coreinit
                    //strBuilder.Append("N/A\t");//.fimport_mic
                    strBuilder.Append(CRC[24].ToString("X8") + "\t");//.symtab
                    strBuilder.Append(CRC[25].ToString("X8") + "\t");//.strtab
                    strBuilder.Append(CRC[26].ToString("X8") + "\t");//.shstrtab
                    //strBuilder.Append(CRC[27].ToString("X8") + "\t");//CRCs
                    //strBuilder.Append(CRC[28].ToString("X8") + "\t");//RPL Info
                }
                else if (type == VCType.B1)
                {
                    //strBuilder.Append(CRC[11].ToString("X8") + "\t");//.dimport_nn_act
                    strBuilder.Append(CRC[12].ToString("X8") + "\t");//.fimport_sysapp
                    strBuilder.Append(CRC[13].ToString("X8") + "\t");//.fimport_zlib125
                    strBuilder.Append(CRC[14].ToString("X8") + "\t");//.fimport_gx2
                    strBuilder.Append(CRC[15].ToString("X8") + "\t");//.fimport_snd_core
                    //strBuilder.Append("N/A\t");//.dimport_snd_core
                    strBuilder.Append(CRC[22].ToString("X8") + "\t");//.fimport_snd_user
                    //strBuilder.Append("N/A\t");//.dimport_snd_user
                    strBuilder.Append(CRC[16].ToString("X8") + "\t");//.fimport_nn_save
                    strBuilder.Append(CRC[17].ToString("X8") + "\t");//.fimport_vpad
                    strBuilder.Append(CRC[18].ToString("X8") + "\t");//.fimport_proc_ui
                    strBuilder.Append(CRC[19].ToString("X8") + "\t");//.fimport_padscore
                    strBuilder.Append(CRC[20].ToString("X8") + "\t");//.fimport_coreinit
                    strBuilder.Append(CRC[21].ToString("X8") + "\t");//.dimport_coreinit
                    //strBuilder.Append("N/A\t");//.fimport_mic
                    strBuilder.Append(CRC[23].ToString("X8") + "\t");//.symtab
                    strBuilder.Append(CRC[24].ToString("X8") + "\t");//.strtab
                    strBuilder.Append(CRC[25].ToString("X8") + "\t");//.shstrtab
                    //strBuilder.Append(CRC[26].ToString("X8") + "\t");//CRCs
                    //strBuilder.Append(CRC[27].ToString("X8") + "\t");//RPL Info
                }
                else if (type == VCType.B2)
                {
                    //strBuilder.Append(CRC[11].ToString("X8") + "\t");//.dimport_nn_act
                    strBuilder.Append(CRC[12].ToString("X8") + "\t");//.fimport_sysapp
                    strBuilder.Append(CRC[13].ToString("X8") + "\t");//.fimport_zlib125
                    strBuilder.Append(CRC[14].ToString("X8") + "\t");//.fimport_gx2
                    strBuilder.Append(CRC[15].ToString("X8") + "\t");//.fimport_snd_core
                    //strBuilder.Append("N/A\t");//.dimport_snd_core
                    strBuilder.Append(CRC[16].ToString("X8") + "\t");//.fimport_snd_user
                    //strBuilder.Append("N/A\t");//.dimport_snd_user
                    strBuilder.Append(CRC[17].ToString("X8") + "\t");//.fimport_nn_save
                    strBuilder.Append(CRC[18].ToString("X8") + "\t");//.fimport_vpad
                    strBuilder.Append(CRC[19].ToString("X8") + "\t");//.fimport_proc_ui
                    strBuilder.Append(CRC[20].ToString("X8") + "\t");//.fimport_padscore
                    strBuilder.Append(CRC[21].ToString("X8") + "\t");//.fimport_coreinit
                    strBuilder.Append(CRC[22].ToString("X8") + "\t");//.dimport_coreinit
                    //strBuilder.Append("N/A\t");//.fimport_mic
                    strBuilder.Append(CRC[23].ToString("X8") + "\t");//.symtab
                    strBuilder.Append(CRC[24].ToString("X8") + "\t");//.strtab
                    strBuilder.Append(CRC[25].ToString("X8") + "\t");//.shstrtab
                    //strBuilder.Append(CRC[26].ToString("X8") + "\t");//CRCs
                    //strBuilder.Append(CRC[27].ToString("X8") + "\t");//RPL Info
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
            //strBuilder.AppendLine("VC SNES:");
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
                case "915D172266F7021DFF5E898EA50F3FA7": return "Albert Odyssey (アルバートオデッセイ) (JPN) [JC6J]";
                case "6A88FCBA4D9B34300FD4226FEF88D9DB": return "Axelay (USA) [JCZE]";
                case "A67408778421C1F90CBC4C874C015E38": return "Axelay (アクスレイ) (JPN) [JCZJ]";
                case "092693D5BD8C3EEA42A6DE8B3A943225": return "Bahamut Lagoon (バハムート ラグーン) (JPN) [JCAJ]";
                case "832B7E4A10FFAC3A468249AFF6E00D2A": return "Battletoads in Battlemaniacs [0005000010165001] (USA) [JANE - Kirby's Dream Land 3]";
                case "A325638B95AF1BB840EED9734D83ECFD": return "Brawl Brothers (USA) [JBVE]";
                case "D0458DE8BF36F5267B6654CF026785A1": return "Breath of Fire (USA) [JCVE]";
                case "FDAF4B61D826CA769DAD6F77B9FD389F": return "Breath of Fire II (USA) [JBEE]";
                case "6D8C097C782F57646FBC1B659AD3AEE3": return "Breath of Fire II: The Fated Child (ブレス オブ ファイアⅡ 使命の子) (JPN) [JBEJ]";
                case "7E81CB3BE61EBD933E5D4098E3F45F4C": return "Castlevania: Dracula X (USA) [JCDE]";
                case "10EC7115C7E01E36B0662138FC4A62B7": return "Clock Tower (クロックタワー) (JPN) [JBWJ]";
                case "72B0B6413F9D8F0B6F4DD4DAB1118E1E": return "Contra III: The Alien Wars (USA) [JA5E]";
                case "415AC74E8CB6C60A503EE845D8DA164F": return "Contra III: The Alien Wars (魂斗羅スピリッツ) (JPN) [JA5J]";
                case "49D2EE2FE3E6171BF225F58413A3AF38": return "Contra III: The Alien Wars Restoration [0005000010105291] (USA) [JA5E - Contra III: The Alien Wars]";
                case "7472383D43638CEFD2240D4BB5BB180C": return "Cosmo Gang the Puzzle (コズモギャング ザ パズル) (JPN) [JC4J]";
                case "AE79C3B751B447597D741C8B011C0E38": return "Cybernator (USA) [JBNE]";
                case "D70379AFCAB4940094C1A84AD6400F39": return "Cybernator (重装機兵ヴァルケン) (JPN) [JBNJ]";
                case "7B27B2959EEC9757B89507561D29BB82": return "Darius Twin (ダライアスツイン) (JPN) [JDJJ]";
                case "021A8BBFEE5C2C9CFC7FB3F17AE94CBD": return "Donkey Kong Country (USA) [JACE]";
                case "DB1FA053670EBC7283AB0B72A3C062FF": return "Donkey Kong Country (EUR) [JACP]";
                case "FBD6930C5D583316AA0AFC72A00FC8AF": return "Donkey Kong Country (スーパードンキーコング) (JPN) [JACJ]";
                case "C49F28EC143EC5CB0CC886FDEF71C812": return "Donkey Kong Country 2: Diddy's Kong Quest (USA) [JAGE]";
                case "60D7F84DBF839A30D223C099CCF713D3": return "Donkey Kong Country 2: The Lost Levels [0005000010105462] (USA) [JAGE - Donkey Kong Country 2: Diddy's Kong Quest]";
                case "C50A962D9789E4BB281628F93E93C00E": return "Donkey Kong Country 3 - Tag Team Trouble [0005000010105463] (USA) [JCXE - Donkey Kong Country 3: Dixie Kong's Double Trouble!]";
                case "114AD835F65EF43088F17A3ADEA55D71": return "Donkey Kong Country 3: Dixie Kong's Double Trouble! (USA) [JCXE]";
                case "78403FE0FECA15B9ECEABF7E9A7721D8": return "Donkey Kong Country 3: Dixie Kong's Double Trouble! (スーパードンキーコング3 謎のクレミス島) (JPN) [JCXJ]";
                case "2EB8BC6C766BFB74E8D8D8A6E1B170F8": return "Donkey Kong Country Easy Edition [0005000010105085] (USA) [JACE - Donkey Kong Country]";
                case "B897546E83B72A6B61175DAE17330183": return "EarthBound (USA) [JBBE]";
                case "2C03ABE178589000AB0752FD1890DF8E": return "EarthBound (EUR) [JBBP]";
                case "AD541ACFCF57AE2F21AEFC7DFAA5E6A7": return "Earthbound New Controls Mod [0005000010102538] (USA) [JBBE - EarthBound]";
                case "28D379E95CBD8B2D934E273E69BC9AE9": return "Earthworm Jim [0005000010166000] (USA) [JACE - Donkey Kong Country]";
                case "A2B579F5153EF83ECFC77EAAC1026703": return "Famicom Bunko: Hajimari no Mori (はじまりの森) (JPN) [JBHJ]";
                case "96F074A866686879F02CF95B4A71BEF0": return "Famicom Tantei Club Part II: Ushiro ni Tatsu Shōjo (ファミコン探偵倶楽部 PARTⅡ うしろに立つ少女) (JPN) [JA6J]";
                case "F67F5B0AF38D9DD502842DFFFC416D53": return "Final Fantasy IV (ファイナルファンタジーIV) (JPN) [JBZJ]";
                case "90F6FEEBEAB81D2C4B00042BDFDBD69F": return "Final Fantasy Mystic Quest (ファイナルファンタジーUSA ミスティッククエスト) (JPN) [JB7J]";
                case "00BE40B07EAC39EA13991ADCB7BABFE1": return "Final Fantasy V (ファイナルファンタジーV) (JPN) [JB6J]";
                case "93D24D3EBFD486E0A74A821530D9D44E": return "Final Fantasy VI (ファイナルファンタジーⅥ) (JPN) [JBYJ]";
                case "622751C5BCCF64AD043EE04C36EF972B": return "Final Fight (USA) [JA8E]";
                case "3B06EC8817D43100D41384BBE9707C45": return "Final Fight 2 (USA) [JBLE]";
                case "7BF4A263E0F00EFDA645B91EFBB853CB": return "Final Fight 2 (ファイナルファイト2) (JPN) [JBLJ]";
                case "FE0442746D1A67C19A8C6048279B5ABD": return "Final Fight 3 (USA) [JBUE]";
                case "135F86DDC5E6DBDBDAFDB548F650BFAE": return "Final Fight 3 (ファイナルファイト タフ) (JPN) [JBUJ]";
                case "A5ACF94B0CCA1ECFFEC8B7690634C9FC": return "Fire Emblem: Monshō no Nazo (ファイアーエムブレム　紋章の謎) (JPN) [JAHJ]";
                case "FF32005A539A916B9E578D091E19496D": return "Fire Emblem: Seisen no Keifu (ファイアーエムブレム　聖戦の系譜) (JPN) [JAFJ]";
                case "E33D50EE7E85F96E51A6FAD81D9286D1": return "Fire Emblem: Thracia 776 (ファイアーエムブレム  トラキア776) (JPN) [JBFJ]";
                case "05C12D9DA331FF19273027250799C242": return "Fire Fighting (ファイヤー･ファイティング) (JPN) [JENJ]";
                case "FDDC47914AFD910937F525A0C3089DB3": return "F-Zero (USA) [JARE]";
                case "486090B7963343FEC0012BFA94F1368F": return "F-Zero (JPN) [JARJ]";
                case "DFAF0A7C4C0766C16919BF20993E09E6": return "Gakkou de atta Kowai Hanashi (学校であった怖い話) (JPN) [JCSJ]";
                case "14EA892BC23973736674D1CAB537C879": return "Ganbare Goemon 2: Kiteretsu Shōgun Magginesu (JPN) [JAUJ]";
                case "12CCE8EDF9BA2DAAEEF2F3721621D4D6": return "Ganbare Goemon 3: Shishijūrokubē no Karakuri Manji Gatame (がんばれゴエモン3 獅子重禄兵衛のからくり卍固め) (JPN) [JAXJ]";
                case "0F1729E2D5B015D25B532CA16CBE7022": return "Genghis Khan II: Clan of the Gray Wolf (USA) [JC7E]";
                case "553545C8CBDDAB4A7FC4B6B229D985CD": return "Genghis Khan II: Clan of the Gray Wolf (スーパー蒼き狼と白き牝鹿 元朝秘史) (JPN) [JC7J]";
                case "AB5D8E0F8E962C03F785974232AFB788": return "Gussun Oyoyo (すーぱーぐっすんおよよ) (JPN) [JC2J]";
                case "3276E139FC4C38B850788FB12277C6ED": return "Harvest Moon (USA) [JBKE]";
                case "FE171282AC06764E17D1F1FB24336A24": return "Harvest Moon (EUR) [JBKP]";
                case "1FE542F7A233F20CBBD110D21C45EE3D": return "Heisei Shin Onigashima Part 1 (平成 新･鬼ヶ島 前編) (JPN) [JCMJ]";
                case "1126C89C7A37C32870768780BBA24DAE": return "Heisei Shin Onigashima Part 2 (平成 新･鬼ヶ島 後編) (JPN) [JCQJ]";
                case "0AE7ABD139DB22EDE02940251D042374": return "Heracles no Eikō III: Kamigami no Chinmoku (ヘラクレスの栄光Ⅲ 神々の沈黙) (JPN) [JA2J]";
                case "7C331EE3C908D8DCE6F145DD4875CB3C": return "Heracles no Eikō IV: Kamigami kara no Okurimono (ヘラクレスの栄光Ⅳ 神々からの贈り物) (JPN) [JCYJ]";
                case "040230DC8B3828421F27427283A6E607": return "Idol Janshi Suchie-Pai (美少女雀士スーチーパイ) (JPN) [JDEJ]";
                case "4EDD4C685A506FF56233DE08B2648598": return "Kai: Tsukikomori (晦－つきこもり) (JPN) [JEMJ]";
                case "17B12079775AC63B14AD8E2B917C6FF2": return "Kamaitachi no Yoru (かまいたちの夜) (JPN) [JAZJ]";
                case "526430390B07F9F1A9C5DEE4A37507AD": return "Kirby Super Star (USA) [JAEE]";
                case "0CF8CDB51E3891CD82749B56AFA8FB67": return "Kirby's Dream Course (USA) [JASE]";
                case "DBF25474B1022AA2C26A3310D5D49EE3": return "Kirby's Dream Course (カービィボウル) (JPN) [JASJ]";
                case "546CAB1678DD3989832EA57CCAEEF7EF": return "Kirby's Dream Land 3 (USA) [JANE]";
                case "45DEAA621CDB68083086490F33858096": return "Kirby's Dream Land 3 (星のカービィ3) (JPN) [JANJ]";
                case "EFCE161ED1E4B1A3F9B4322B08BD38B9": return "Kirby's Star Stacker (カービィのきらきらきっず) (JPN) [JAWJ]";
                case "FD0B2EC0632BA670E946DE563868DDA3": return "Kunio-kun no Dodge Ball da yo: Zenin Shūgo (くにおくんのドッジボールだよ 全員集合！) (JPN) [JCKJ]";
                case "9D891C11D4CC4D139584C6E24799D11B": return "Last Bible III (ラストバイブルⅢ) (JPN) [JCHJ]";
                case "AE759744951CAA05CC7492512B6968CC": return "Live A Live (ライブ・ア・ライブ) (JPN) [JC5J]";
                case "4AE80453D608119B23189C7F52DDE525": return "Majin Tensei (魔神転生) (JPN) [JC9J]";
                case "C09ADB685F61D866990058382281000A": return "Majin Tensei II: Spiral Nemesis (魔神転生Ⅱ SPIRAL NEMESIS) (JPN) [JDFJ]";
                case "FCDA10363AF8E662068C97E8D80466FE": return "Mario's Super Picross (EUR) [JAQP]";
                case "57735FF0077AE360697C1A92D6A0D1A9": return "Mario's Super Picross (マリオのスーパーピクロス) (JPN) [JAQJ]";
                case "888F40AB5EAB218D2DDDF1DEF4416467": return "Marvelous: Mōhitotsu no Takarajima (マーヴェラス ～もうひとつの宝島～) (JPN) [JCCJ]";
                case "0575986B78DEB4E528E2F743C96E713D": return "Mega Man X (USA) [JBAE]";
                case "0FB19459B10998F3F8212B112E7AB015": return "Mega Man X2 (USA) [JBTE]";
                case "5BC9710236E1B9B9E382492D4F63A660": return "Mega Man X2 ESP [0005000010105694] (USA) [JCPE - Mega Man X3]";
                case "BCA2EA307085B1362AFA637CAB7D6C2C": return "Mega Man X3 (USA) [JCPE]";
                case "65ACAA38F684833FC484DEBA0536A5E4": return "Metal Marines (USA) [JC3E]";
                case "ED8F9459C7E5BBD43336EC1AB8E067E1": return "Metal Marines (ミリティア) (JPN) [JC3J]";
                case "59FC6F684270196C279F6ECA3F2D9E45": return "Metal Slader Glory (メタルスレイダーグローリー ディレクターズカット) (JPN) [JDLJ]";
                case "60E5E8D1FBDD8648931EC110F0B8A85C": return "Mother 2 (JPN) [JBBJ]";
                case "A451A70324A5C7EC5DA48DEF64821C70": return "Natsume Championship Wrestling (USA) [JCUE]";
                case "7EAFAAFA05455379F18F4C94CE430BB6": return "Nobunaga's Ambition (USA) [JCLE]";
                case "B80F9A650D48AB605F0C26DC24F5F253": return "Ogre Battle: The March of the Black Queen (伝説のオウガバトル) (JPN) [JB2J]";
                case "1C449D010DD6B77EC48F4BCC19F6559C": return "Otogirisō (弟切草) (JPN) [JCEJ]";
                case "F03A3375B43B747189F0B40EC15876A9": return "Pac-Attack (USA) [JC4E]";
                case "706DA98743B233A73F39578353A6DC04": return "Pac-Attack (EUR) [JC4P]";
                case "0127F8F5AAE58DF1F2DAF465FDF903A5": return "Pac-Man 2: The New Adventures (USA) [JDKE]";
                case "E8F5D225FFA1646D4B53BA41E489E1D8": return "Panel de Pon (パネルでポン) (JPN) [JA3J]";
                case "EB03D7ADEC34288D19D6925FBC1941D5": return "Pilotwings (USA) [JA7E]";
                case "D8BFD1D939BFC59368927A5307AEAD13": return "Pop'n TwinBee (Pop'nツインビー) (JPN/EUR) [JCBJ]";
                case "D1D240E53CE2E152A20B93A6EFE22C4C": return "Pop'n TwinBee: Rainbow Bell Adventures (ツインビー レインボーベルアドベンチャー) (JPN/EUR) [JCFJ]";
                case "A1C2ECFA71D199E4DB4BA18CBEAA5981": return "Power Instinct (豪血寺一族) (JPN) [JEPJ]";
                case "A267E33B72B97C4D36BAC60F7BC78392": return "Rival Turf! (USA) [JDBE]";
                case "C84C4685F6F7C76AD4F19B75D9C6EDE3": return "Rockman X2 (ロックマンX2) (JPN) [JBTJ]";
                case "BF90BAB27FD0D7C65B666CB30DE5EAD3": return "Rockman X3 (ロックマンX3) (JPN) [JCPJ]";
                case "B8044BBBAE16BD345D44922A242C9674": return "Romance of the Three Kingdoms IV: Wall of Fire (USA) [JBCE]";
                case "3932248346EA780BA9E3BA5D39274404": return "Romance of the Three Kingdoms IV: Wall of Fire (三國志Ⅳ) (JPN) [JBCJ]";
                case "6EF46DD0FF06E3D6A08FFF2CB617DCF1": return "Romancing SaGa (ロマンシング サ・ガ) (JPN) [JB3J]";
                case "69ACEC05E14F2F2E032C382AA38DF786": return "Romancing SaGa 2 (ロマンシング サ・ガ2) (JPN) [JB5J]";
                case "C917FC9A2451ECA33A001E9CF2C86899": return "Romancing SaGa 3 (ロマンシング サ･ガ3) (JPN) [JB9J]";
                case "1C7F3408AAB1B71CD9ED30E902F58310": return "Rushing Beat (ラッシング･ビート) (JPN) [JDBJ]";
                case "54632249197DB1C7A385ABF655C304B7": return "Secret of Mana (聖剣伝説2) (JPN) [JBXJ]";
                case "4F810D7FCC414E88D986CF5DAA1000BB": return "Shin Megami Tensei (真･女神転生) (JPN) [JA4J]";
                case "61189177AD8BC3F243661A56967EB542": return "Shin Megami Tensei If… (真･女神転生if…) (JPN) [JBSJ]";
                case "D7C8301F835F47FEA201AF55D588D50F": return "Shin Megami Tensei II (真･女神転生Ⅱ) (JPN) [JBRJ]";
                case "985FD5A4D4E58A9F73E5C05264E60DD8": return "Space Invaders: The Original Game (JPN) [JDHJ]";
                case "92E3ADE70C424072624441067EDCAB1D": return "Street Fighter Alpha 2 (USA) [JCGE]";
                case "8F3595632A0AE9A2FA30EB5EE4E57801": return "Street Fighter II' Turbo: Hyper Fighting (USA) [JAYE]";
                case "3BC47A115A88A879C9D91B7C00FC71AA": return "Street Fighter II' Turbo: Hyper Fighting (ストリートファイターⅡ ターボ ハイパー ファイティング) (JPN) [JAYJ]";
                case "84C3BE501632326C66B773E8F4FE3932": return "Street Fighter II: The World Warrior (USA) [JAME]";
                case "F399B716512562E21E1825B3D63E1E9C": return "Street Fighter II: The World Warrior (ストリートファイターⅡ ザ ワールド ウォーリアー) (JPN) [JAMJ]";
                case "B97275FE1F9FD319A08E6DB46B5080E0": return "Street Fighter Zero 2 (ストリートファイターZERO2) (JPN) [JCGJ]";
                case "4FCF29344E4FD7B9CA4A25B66927215E": return "Super Castlevania IV (USA) [JA9E]";
                case "75D02F4BEA96E18D1811FCB3EE532099": return "Super E.D.F.: Earth Defense Force (USA) [JDAE]";
                case "C917D40F0A53557ADFC65E7C2F3CFB27": return "Super E.D.F.: Earth Defense Force (JPN) [JDAJ]";
                case "2F9BA3454597F4B3B974824B953B6E09": return "Super Famicom Wars (スーパーファミコンウォーズ) (JPN) [JBGJ]";
                case "82B5119769B6CF340F985F50BB39F0FA": return "Super Ghouls 'n Ghosts (USA) [JATE]";
                case "96CAEEB0A880D5C18FFCBCB232259636": return "Super Mario Kart (USA) [JAKE]";
                case "27EE11866E8C3A09186C1BB079FDD695": return "Super Mario Kart (スーパーマリオカート) (JPN) [JAKJ]";
                case "8FE994D4D89BA8495D533D00CEF1A782": return "Super Mario Kart Victory Drunk Plus! [0005000010105435] (USA) [JAKE - Super Mario Kart]";
                case "5C32A356EAF94C858BA036573ACD3937": return "Super Mario RPG: Legend of the Seven Stars (USA) [JABE]";
                case "4ADB32888932642CE437F825695699DD": return "Super Mario World (USA) [JAAE]";
                case "86DC50E8C3005838E19D8D71D366563F": return "Super Metroid (USA) [JAJE]";
                case "6032B2AFB767E316B5B5E82F5D6983E6": return "Super Metroid (EUR) [JAJP]";
                case "8AB8E8227AE45D0BADCD2FC508414995": return "Super Metroid ESP [0005000010104717] (USA) [JAJE - Super Metroid]";
                case "D0FE6B74798DC7EAFA5A0DC1116F8786": return "Super Ninja Boy (スーパーチャイニーズワールド) (JPN) [JCJJ]";
                case "D44B157058599E4FE9E59C9129CED707": return "Super Nobunaga's Ambition (SUPER 信長の野望・全国版) (JPN) [JCLJ]";
                case "B80B2FFEBAE5181E2C54D1DE25B71711": return "Super Punch-Out!! (USA) [JB8E]";
                case "BB00EC211C83D14E3D5894110D9AFAFD": return "Super Punch-Out!! (EUR) [JB8P]";
                case "A2ADC5251CE3DB241EA319DCBF7A61AD": return "Super Punch-Out!! (スーパーパンチアウト!!) (JPN) [JB8J]";
                case "4A31937EAC317BCE5EE3FB7D3F35C444": return "Super Street Fighter II: The New Challengers (USA) [JAVE]";
                case "1E3F1E4C267ED6A52A33E6D001955177": return "Super Street Fighter II: The New Challengers (スーパーストリートファイターⅡ ザ ニューチャレンジャーズ) (JPN) [JAVJ]";
                case "8911302BF5C231A3F73AB3337B1898C1": return "Sutte Hakkun (すってはっくん) (JPN) [JCRJ]";
                case "948345CB5F7E6E3CAB4ECD4DC396DD29": return "Tactics Ogre: Let Us Cling Together (タクティクスオウガ) (JPN) [JB4J]";
                case "41D93BB5C0BEAB706FCDA925A12CA12F": return "Taikō Risshiden (太閤立志伝) (JPN) [JC8J]";
                case "0241FF4171A8A49B4FE060D3DAD2C8F1": return "The Ignition Factor (USA) [JENE]";
                case "FD440F163DE6EAA15FFB8A56BDC55122": return "The Legend of the Mystical Ninja (USA) [JALE]";
                case "632A491D5F9BE599B37FD8D152D8F3AB": return "The Legend of the Mystical Ninja (EUR) [JALP]";
                case "02614A8F3FF551389DAD3C330AD6AF2A": return "The Legend of Zelda: A Link to the Past (USA) [JADE]";
                case "A58BF87413318F37DD0AFA63DD2D1DEB": return "The Legend of Zelda: A Link to the Past (EUR) [JADP]";
                case "A2AE21A915BEC833A0129FE947FEB1DB": return "Treasure of the Rudras (ルドラの秘宝) (JPN) [JDGJ]";
                case "99A89AA68ED1090ED42CCEAEAFA49570": return "Uncharted Waters II: New Horizons (大航海時代Ⅱ) (JPN) [JBQJ]";
                case "CD5005E33EA525F536C007071056582C": return "Uncharted Waters: New Horizons (USA) [JBQE]";
                case "B8214A362CDFAD65875366D2257C9782": return "Vegas Stakes (USA) [JBJE]";
                case "9352919722880ED6626AF472859A2D19": return "Wagan Land (スーパーワギャンランド) (JPN) [JBMJ]";
                case "3F7E0EC3DA5DC0BBB97B3100386AFEA2": return "Wild Guns (USA) [JCTE]";
                case "3175C4D9FA26C042000646036637E3C4": return "Wrecking Crew '98 (レッキングクルー'98) (JPN) [JLCJ]";
                default: return "[Unknown]";
            }
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
