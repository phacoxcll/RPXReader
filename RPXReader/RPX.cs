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
        public bool IsPadded
        { protected set; get; }
        public uint[] CRC
        { protected set; get; }
        public string MD5
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
            IsPadded = true;
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
                    if (SectionHeader[i].sh_offset % 0x40 != 0)
                        IsPadded = false;
                }
            }

            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            fs = File.Open(filename, FileMode.Open);
            byte[] hash = md5.ComputeHash(fs);
            fs.Close();
            MD5 = BitConverter.ToString(hash).Replace("-", string.Empty);
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

            strBuilder.AppendLine("MD5: " + MD5);
            strBuilder.AppendLine("Base title: " + GetBase(MD5));
            strBuilder.AppendLine();
            strBuilder.AppendLine(Header.ToString());
            strBuilder.AppendLine("Is padded: " + IsPadded.ToString() + "\n");
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

        public string GetBase(string md5)
        {
            switch (md5)
            {
                case "7F3BABACCC23E78F8BBD970090E881FE": return "Advance Wars: Dual Strike (USA)";
                case "22373D0FFD3E9CC0EFEBF68555A81609": return "ALIEN CRUSH (JPN)";
                case "A2EA5CCD9DC30F3138831214A474C4DB": return "ALIEN CRUSH (USA)";
                case "3C488C84808310979170366603D07D42": return "Animal Crossing: Wild World (USA)";
                case "1D08A7A10D622D58AF087F18DC67B0AE": return "Antarctic Adventure (JPN)";
                case "D2D43DAAD6578CC40905D2330C06F5E7": return "Blazing Lazers (JPN)";
                case "A4ABE0E2AC2E25D6DF5E2DCA87A14756": return "BOMBERMAN PANIC BOMBER (USA)";
                case "3C85EC1EAD0561EFC37FD1AC0D83504E": return "BOMBERMAN PANICBOMBER (JPN)";
                case "C3CDBB3D382ADC22AB8BBF53EB0F36A4": return "BONK 3 Bonk's Big Adventure (JPN)";
                case "53C94D17754681425AEC2ED928499151": return "Bonk's Adventure (JPN)";
                case "02485C7C1249ED165760450806FDDF23": return "Bonk's Adventure (USA)";
                case "BA389B2CDDFFF991490A879A40D044E9": return "Brain Age: Train Your Brain in Minutes a Day! (USA)";
                case "493644F6FB3DF3305F167FBB828B6663": return "Castlevania (JPN)";
                case "75ABE24D149D79CCE34188882D97E925": return "Cho-Aniki (JPN)";
                case "350F45093EAAD4085E59D4DF983DC5C2": return "Chocobo Land (チョコボランド) (JPN)";
                case "85F7A8FB827428C54FB7C91D438CCB60": return "ChuChu Rocket! (チューチューロケット！) (JPN)";
                case "46E603E233F976A7C2840247C9969A23": return "Contra (JPN)";
                case "B28D5D74C19053097C7228671B3587A1": return "CONTRA ADVANCE THE ALIEN WARS EX (USA)";
                case "99B986617A2886338F1CE0C9A003FE6F": return "Custom Robo V2 (カスタムロボV2) (JPN) [SVN2244]";
                case "AF70AB57FC8D38397B3D0731200A7D43": return "Densetsu no Stafy 2 (伝説のスタフィー2) (JPN)";
                case "118DCEDBE45F2BF2475D00EE9BD90D8A": return "Densetsu no Stafy 3 (伝説のスタフィー3) (JPN)";
                case "7EC4DCD6D1CC539BF1915D30306A7E19": return "DETANA TWIN BEE (JPN)";
                case "32E61DCDFC5B070A4342E979A3A11F82": return "DETANA TWIN BEE (USA)";
                case "4BE950E85C1A17F61FAA2734C689104E": return "Devil's Crush (JPN)";
                case "C9E5205579E2ECF1DF43676A516B6B25": return "DIGITAL CHAMP (JPN)";
                case "8EE2376F36EA84B3A327CC53AC4A89BE": return "DK: Jungle Climber (USA)";
                case "684DCB210D804EAE95E94438396DA5C0": return "Donkey Kong 64 (USA) [SVN1680]";
                case "E43DEA04321E86F699E4222F7AF31245": return "Donkey Kong 64 (ドンキーコング64) (JPN) [SVN1690]";
                case "24B854F6E5B4DD6EE2558A9E3EA893D2": return "DOUBLE DUNGEONS (JPN)";
                case "B1F9DE02808D3DA8D713CA3BC54F13ED": return "Dr Kawashima's Brain Training How Old Is Your Brain (Original Free DL Version) [0005000010101974] (EUR)";
                case "2F1FAB231F2C27553A13AAE3F3E327B9": return "Drill Dozer (EUR)";
                case "769C56B037FCD816DB82E2309C178F19": return "Dungeon Explorer (JPN)";
                case "4E66AD4E541B2F2D27A0FE14C2D1CA6F": return "DUNGEON EXPLORER (USA)";
                case "B46E9A02D4C6E874F16EC631B73C0667": return "Excitebike 64 (USA) [SVN2404]";
                case "A26FDDBC92089D8412FBAFF83BD6B97C": return "Final Fantasy I & II: Dawn of Souls (ファイナルファンタジーI・II アドバンス) (JPN)";
                case "115420A5C027406887B1EA2477253D69": return "Final Fantasy IV Advance (ファイナルファンタジーIV アドバンス) (JPN)";
                case "1E202CD00740CC4DDC2472E5E0428EF9": return "FINAL FANTASY Tactics Advance (USA)";
                case "76828C65C7999EBFD94C6F155C49E6F3": return "Final Fantasy VI Advance (ファイナルファンタジーVI アドバンス) (JPN)";
                case "9E64937568D2064118C38801A425436D": return "FINAL SOLDIER (USA)";
                case "925EE78E3C2303B6F07871917E11C558": return "Fire Emblem: Fūin no Tsurugi (ファイアーエムブレム 封印の剣) (JPN)";
                case "DC993A9F2DC2E1A3DB922DA5FDB81389": return "Fire Emblem: Shadow Dragon (USA)";
                case "56F9DC51E8601758FA2CC4BA4B0F5AC7": return "F-Zero Climax (F-ZERO クライマックス) (JPN)";
                case "E25564A67F4A77A3C149FCF5F5D12626": return "F-Zero X (USA) [SVN2428]";
                case "3643D706099B14BD58B8D89E9C86BAA7": return "Game & Watch Gallery 4 (USA)";
                case "B2C1F779253691B1FE7A02E57FEF06BE": return "GLADIUS (JPN)";
                case "2026FECCF5DB3B8E274DB50A6F1A0356": return "GLADIUS2 (JPN)";
                case "32ED02E8D50C5FBCB544799926BECEB7": return "Goemon (JPN)";
                case "88A56E9A711860142F9CEB2208A9AC65": return "Golden Sun (USA)";
                case "088E172AC8997EC71800F02FF1B863E0": return "GRADIUS (JPN)";
                case "FC2B845BF1E932E65CA82896B02EB196": return "HYPER SPORTS 2 (JPN)";
                case "CDB6F703E5AA230990B6805A2DD67556": return "IMAGEFIGHT (JPN)";
                case "2C8C572FB6250F18FC8E94467D2AC3D9": return "Kirby & The Amazing Mirror (USA)";
                case "0504BB7FA5645DC572CAA5F6C9442F84": return "Kirby 64: The Crystal Shards (USA) [SVN1790]";
                case "0CBA7338B08940811CEFBD22FB5CF5E6": return "Kirby 64: The Crystal Shards (星のカービィ64) (JPN) [SVN1778]";
                case "97567625B8994F6891665BC757A553A8": return "Kirby Nightmare in Dream Land (USA)";
                case "450411EF51B86892C1436C93A1CA4A74": return "Kirby: Mass Attack (USA)";
                case "D845C0936950F234DFD89FF67AB59F61": return "Kirby: Squeak Squad (USA)";
                case "E6D6D3C99E7043A072141DFF4A82F37F": return "KLONOA 2 Dream Champ Tournament (USA)";
                case "7394C5249B55F47137B4E616E7BF27BB": return "Konami's Soccer (JPN)";
                case "4A296BD23E995BC753849F652D52AD3D": return "Langrisser (JPN)";
                case "F270AEFA213A9382E3CCB9C0A8A110FB": return "Magical Tree (JPN)";
                case "453E93CB2371B78EFB67D76C767971CA": return "Magical Vacation (マジカルバケーション) (JPN)";
                case "B16B93BCBDA2FA19DD5EA022106095A1": return "Mario & Luigi Superstar Saga (USA)";
                case "9256B4F9032E7B2424245F68712FE566": return "Mario & Luigi Superstar Saga (マリオ&ルイージRPG) (JPN)";
                case "FD84152B0B8ADA981C79761E6B71A548": return "Mario & Luigi: Partners in Time (USA)";
                case "3AD23BB2FAB7E737493CC4B42F9950D2": return "Mario Golf (USA) [SVN1955]";
                case "310CBA1F5941748E4959F4B265786083": return "Mario Golf (マリオゴルフ64) (JPN) [SVN1946]";
                case "C18CFF826476AD1BC0DC680151DA49CF": return "Mario Hoops 3-on-3 (USA)";
                case "4F54CD7D01B100B354B48BD8D0B0362F": return "Mario Kart 64 (USA) [SVN2043]";
                case "EC43ACDCB2841DB712F6EBE14ED4985F": return "Mario Kart DS (USA)";
                case "7E08E6AB014A10F8C89390AF3E00C84E": return "Mario Kart Super Circuit (USA)";
                case "AE4E6272A58AD3961CECD83F5A134D8B": return "Mario Party 2 (USA) [SVN2234]";
                case "073D4FF1BD54D04D5B2BE7BBECD5924B": return "Mario Party Advance (USA)";
                case "FA9F170184A1B73CBB0478A31601CFE4": return "Mario Party DS (USA)";
                case "5B763F6918AC0F297181DB0E16F01D1E": return "Mario Tennis (USA) [SVN1918]";
                case "0D360940B387C63E8F7BC9E28CDD2336": return "Mario Tennis (マリオテニス64) (JPN) [SVN1897]";
                case "E84B99D829AA00D88B90EE1036A6F4F5": return "Mario vs. Donkey Kong 2: March of the Minis (USA)";
                case "98C58C75EDF38BE7B804504BA2DF174E": return "Medabots AX Metabee (USA)";
                case "927F5E19A7621F567FEBB6E8B27C78A3": return "Medabots Metabee (USA)";
                case "A39582BB47A848E03BC9698C91240A76": return "MEGA MAN & BASS (USA)";
                case "2C7808389698F24DDD7B90CDBFA3DDA1": return "MEGA MAN BATTLE NETWORK (USA)";
                case "C1AAE59EF220CFA673165B8BC8B0F304": return "MEGA MAN BATTLE NETWORK 2 (USA)";
                case "7F5F7E6B75E04B6870895176517D631C": return "MEGA MAN BATTLE NETWORK 3 WHITE (USA)";
                case "30FF8D4C5B5F93C76BFE584A52DC1207": return "MEGA MAN BATTLE NETWORK 4 RED SUN (USA)";
                case "45C08A9CEC1445A7000AE93DF554F184": return "MEGA MAN BATTLE NETWORK 5 TEAM PROTOMAN (USA)";
                case "D3F586C8B7BBE1F5883CCE2B2A34ABD6": return "MEGA MAN BATTLE NETWORK 6 CYBEAST GREGAR (USA)";
                case "6A9A331395CFA3CB374458B800C01691": return "MEGA MAN ZERO 2 (USA)";
                case "C942370B6FDFD3FC1C6C6B9DDDB463A2": return "MEGA MAN ZERO 3 (USA)";
                case "FE0E4BE4D692773606F176EC4DC945C7": return "Metroid Prime Hunters (USA)";
                case "244A1C881C73191D453F4D36437F05D2": return "Metroid Zero Mission (USA)";
                case "314A96ABB5AC588A23E98887C84EEF41": return "MOTHER3 (JPN)";
                case "1B0EB1FB39FEE2C32F93C58A52098473": return "MOTOROADER (JPN)";
                case "C673BB127C1D4B66B9C62B4A4727764C": return "Napoleon (ナポレオン) (JPN)";
                case "B60341F590607AF4EE8761010EA12991": return "New Adventure Island (JPN)";
                case "3D64C31F8E811768924A48655B0D9889": return "New Super Mario Bros. (EUR)";
                case "DC24416E5F56DF1CC39A239A7564A5E2": return "Ogre Battle 64: Person of Lordly Caliber (USA) [SVN2395]";
                case "148A185E38B31003FF4C2B1FA95CFAAE": return "ONIMUSHA TACTICS (USA)";
                case "23A14F6B2014EF2A954EB50E528A9315": return "PAC-MAN Collection (USA)";
                case "4A0E7EA0F4FD4B642CAD9298758E8A18": return "Paper Mario (USA) [SVN1743]";
                case "56C233F6E8897280FD3D2BE5BCB19B3D": return "PARODIUS (JPN)";
                case "4C756D3E530EAE2A293BDE2F87AC8E25": return "Phoenix Wright: Ace Attorney: Justice for All (逆転裁判 2) (JPN)";
                case "D76A124B65797675645D86ED724D8C15": return "Phoenix Wright: Ace Attorney: Trials and Tribulations (逆転裁判 3) (JPN)";
                case "CA78E72EA13F5DBE2027751EF57CC0D1": return "Picross 3D (USA)";
                case "D3D6DE8CF4D3F5A073D1CF868DF75FC3": return "Pocky & Rocky with Becky (USA)";
                case "35C54BCD092F2F2E27C6CAFE4A71F64B": return "Pokémon Mystery Dungeon Red Rescue Team (USA)";
                case "4AFE2CF3576F4F9A63949B6329DAEF1D": return "Pokémon Pinball Ruby & Sapphire (USA)";
                case "C5E56250D853F7B0793944B5BCAD11EB": return "Pokémon Ranger (EUR)";
                case "39725670132AF14346EBC64107EC67C1": return "Pokémon Ranger (USA)";
                case "776AB8F1BE792DC939AE4A32D2B8ED1D": return "Pokémon Ranger (ポケモンレンジャー) (JPN)";
                case "E79DD2B0AB341988929708966C6CC196": return "Pokémon Ranger: Guardian Signs (USA)";
                case "B98B4998698963E97794500B91E12957": return "Pokémon Ranger: Guardian Signs (ポケモンレンジャー 光の軌跡) (JPN)";
                case "C63034942320C942E77B1A3CFDF2B5DD": return "Pokémon Snap (USA) [SVN2195]";
                case "EAC9FBDEB5846A150617B36EDD7EF746": return "Polarium Advance (USA)";
                case "64A987B8DF16FA924CD5C79164DD20F0": return "Power Golf (JPN)";
                case "4FA37E5BDEAF53E0F2DD213005039EEE": return "QUARTH (JPN)";
                case "D47834C458FC2A9F2B6350F9CD48A9D4": return "Rayman Advance (USA)";
                case "094B992083AB1A216447388B6019A7AF": return "ROAD FIGHTER (JPN)";
                case "942D6FA63F0632CC5590957228B31762": return "Rockman EXE 4.5 Real Operation (ロックマン エグゼ 4.5  リアルオペレーション) (JPN)";
                case "4036B7380505C6A737BE38388662DBCB": return "R-TYPE (JPN)";
                case "DB1454667CA81B61CFF3F33582FB9C61": return "R-TYPE (USA)";
                case "CBEDEC88492B1166F1D4B1E122440BA0": return "SALAMANDER (JPN)";
                case "237D9EEB2FB3554390E8C087B4332A83": return "Shockman (JPN)";
                case "C9A65E0470E353B2567FD5ED44F40B6E": return "Sin & Punishment Successor of the Earth (罪と罰 地球の継承者) (USA) [SVN1991]";
                case "90D9938829429BB438BC4757F74FB027": return "SKYJAGUAR (JPN)";
                case "42B10F3173E11E41EEA4CF3CF758AB44": return "Sonic Advance 3 (ソニック アドバンス 3) (JPN)";
                case "9C6646B7B5286FD043097712507A9EB5": return "Space Manbow (JPN)";
                case "AC08D2E2D6FED4B86B0E83F754B53A87": return "Star Fox 64 (USA) [SVN1970]";
                case "6CA05D9DE6B0CFB11202D29309A845A7": return "Star Fox Command (USA)";
                case "9A9B5A6D4594560D5F5DFE2132924D71": return "Style Savvy (USA)";
                case "040681E5369FAE0AEAC2036D71EA48EC": return "Super Mario 64 DS (USA)";
                case "7770677C17BCFD123554218234D15AC8": return "Super Mario Advance (USA)";
                case "FE9A2AF1A365A2E50062D24323B40DF8": return "Super Mario Advance 4 Super Mario Bros. 3 (USA)";
                case "FAD484794EF512A686D2A6463DC7F69F": return "SUPER STAR SOLDIER (JPN)";
                case "F652958982373922AEA58ACFD7E72296": return "syubibinman (JPN)";
                case "FF38D45A2885922C36CF9E712390F6BE": return "The Legend of Zelda: Majora's Mask (USA) [SVN2190]";
                case "AEA4E4AA0A2CD61A7090516B0D6FB7A6": return "The Legend of Zelda: Majora's Mask (ゼルダの伝説 ムジュラの仮面) (JPN) [SVN2170]";
                case "8C8B4AEC06EB886047C8DEE0A3AD2DB1": return "The Legend of Zelda: Ocarina of Time (USA) [SVN1696]";
                case "0EED857929A9A1A8F9ED35C1C015E0C9": return "The Legend of Zelda: Phantom Hourglass (USA)";
                case "2313EA840515591992EEBF98D0C63C1C": return "The Legend of Zelda: Spirit Tracks (USA)";
                case "A2E61EBB79F1E1DD8D084BAE79939E7B": return "TwinBee (JPN)";
                case "C70CC509E2CAA12DC60C5D3282A6E1C9": return "VICTORY RUN (JPN)";
                case "44E86F2249CFA110C6ED6C2665532A75": return "VIGILANTE (JPN)";
                case "151AD8D112C3C3E94E9E7AE1E705CC4C": return "VIGILANTE (USA)";
                case "4FDE286A38CF13E7F4D6879A271D570E": return "Wallaby (JPN)";
                case "8F8513484E7A808BE1FFD117BA4F8BEE": return "Wario Land 4 (EUR)";
                case "A144F99CACC9349C91D864062D13C96E": return "Wario Land 4 (USA)";
                case "E08B85EF7DA75B0A4906453EE1552D7C": return "Wario: Master of Disguise (USA)";
                case "E902D3CE53523772336C1182553CE442": return "WarioWare: Touched! (USA)";
                case "54A13A3165E131B1B203255A6FCC3243": return "Wave Race 64 (USA) [SVN2136]";
                case "69CD8B6A46401786A650B59395F0CB7E": return "Wave Race 64 (ウエーブレース) (JPN) [SVN2109]";
                case "CCFFA3F69F241CDAD8A02C0528EC0F07": return "WORLD SPORTS COMPETITION (JPN)";
                case "D92DD9F314A7617166F8E89958BFBCB8": return "Yie Ar Kung-Fu II The Emperor Yie-Gah (JPN)";
                case "38FF681798C3456C605D432496C33CCD": return "Yoshi's Island Super Mario Advance 3 (USA)";
                case "07513BF26B2BA31AF04092B5EABE722A": return "Yoshi's Island Super Mario Advance 3 (スーパーマリオアドバンス3) (JPN)";
                case "68A7C3C0EC80CEC4B7EC237654028BD7": return "Yoshi's Story (USA) [SVN2079]";
                default: return "[Unknown]";
            }
        }

        public static void Decompress(string source, string destination)
        {
            RPX rpx = new RPX(source);

            byte[][] sectionBytes = new byte[rpx.Header.e_shnum][];

            FileStream src = File.Open(source, FileMode.Open);

            for (int i = 0; i < rpx.SectionHeader.Length; i++)
            {
                if (rpx.CRC[i] != 0)
                {
                    src.Position = rpx.SectionHeader[i].sh_offset;
                    byte[] srcBytes = new byte[rpx.SectionHeader[i].sh_size];
                    src.Read(srcBytes, 0, srcBytes.Length);
                    if ((rpx.SectionHeader[i].sh_flags & (uint)SHF_RPL.ZLIB) == (uint)SHF_RPL.ZLIB)
                    {
                        sectionBytes[i] = Decompress(srcBytes);
                        rpx.SectionHeader[i].sh_flags &= ~(uint)SHF_RPL.ZLIB;
                        rpx.SectionHeader[i].sh_size = (uint)sectionBytes[i].Length;
                        rpx.CRC[i] = Security.ComputeCRC32(sectionBytes[i], 0, sectionBytes[i].Length);
                    }
                    else
                        sectionBytes[i] = srcBytes;
                }
                else
                    sectionBytes[i] = new byte[0];
            }

            src.Close();

            List<KeyValuePair<int, Elf32_Shdr>> shList = new List<KeyValuePair<int, Elf32_Shdr>>();

            for (int i = 0; i < rpx.SectionHeader.Length; i++)
                shList.Add(new KeyValuePair<int, Elf32_Shdr>(i, rpx.SectionHeader[i]));
            shList.Sort((pair1, pair2) => Elf32_Shdr.CompareByOffset(pair1.Value, pair2.Value));

            byte[] paddingBytes = new byte[0x40 + rpx.Header.e_shnum * 0x2C];

            FileStream dest = File.Open(destination, FileMode.Create);
            dest.Write(paddingBytes, 0, paddingBytes.Length);

            for (int i = 0; i < shList.Count; i++)
            {
                if (rpx.CRC[shList[i].Key] != 0)
                {
                    rpx.SectionHeader[shList[i].Key].sh_offset = (uint)dest.Position;
                    dest.Write(sectionBytes[shList[i].Key], 0, sectionBytes[shList[i].Key].Length);

                    if (rpx.IsPadded)
                    {
                        int padding = GetPhysicalSectionSize(sectionBytes[shList[i].Key].Length) - sectionBytes[shList[i].Key].Length;
                        paddingBytes = new byte[padding];
                        dest.Write(paddingBytes, 0, paddingBytes.Length);
                    }
                }
                else if ((rpx.SectionHeader[shList[i].Key].sh_type & (uint)SHT_RPL.CRCS) == (uint)SHT_RPL.CRCS)
                    rpx.SectionHeader[shList[i].Key].sh_offset = (uint)(0x40 + rpx.Header.e_shnum * 0x28);
            }

            dest.Position = 0;
            dest.Write(rpx.Header.ToArray(), 0, 0x34);

            dest.Position = 0x40;
            for (int i = 0; i < rpx.SectionHeader.Length; i++)
                dest.Write(rpx.SectionHeader[i].ToArray(rpx.Header.e_ident[(byte)EI.DataEncoding]), 0, 0x28);

            for (int i = 0; i < rpx.CRC.Length; i++)
            {
                dest.WriteByte((byte)(rpx.CRC[i] >> 24));
                dest.WriteByte((byte)((rpx.CRC[i] >> 16) & 0xFF));
                dest.WriteByte((byte)((rpx.CRC[i] >> 8) & 0xFF));
                dest.WriteByte((byte)(rpx.CRC[i] & 0xFF));
            }

            dest.Close();

            /*List<KeyValuePair<int, Elf32_Shdr>> shList = new List<KeyValuePair<int, Elf32_Shdr>>();
            List<KeyValuePair<int, Elf32_Shdr>> shNew = new List<KeyValuePair<int, Elf32_Shdr>>();

            for (int i = 0; i < rpx.SectionHeader.Length; i++)
                shList.Add(new KeyValuePair<int, Elf32_Shdr>(i, rpx.SectionHeader[i]));
            shList.Sort((pair1, pair2) => Elf32_Shdr.CompareByOffset(pair1.Value, pair2.Value));

            FileStream src = File.Open(source, FileMode.Open);
            FileStream dest = File.Open(destination, FileMode.Create);

            byte[] srcBytes = new byte[0x40 + rpx.Header.e_shnum * 0x28];
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

            src.Close();
            dest.Close();*/
        }

        public static void Compress(string source, string destination)
        {
            RPX rpx = new RPX(source);

            byte[][] sectionBytes = new byte[rpx.Header.e_shnum][];

            FileStream src = File.Open(source, FileMode.Open);

            for (int i = 0; i < rpx.SectionHeader.Length; i++)
            {
                if (rpx.CRC[i] != 0)
                {
                    src.Position = rpx.SectionHeader[i].sh_offset;
                    byte[] srcBytes = new byte[rpx.SectionHeader[i].sh_size];
                    src.Read(srcBytes, 0, srcBytes.Length);

                    if ((rpx.SectionHeader[i].sh_type & (uint)SHT_RPL.FILEINFO) == (uint)SHT_RPL.FILEINFO ||
                        (rpx.SectionHeader[i].sh_flags & (uint)SHF_RPL.ZLIB) == (uint)SHF_RPL.ZLIB)
                    {
                        sectionBytes[i] = srcBytes;
                    }
                    else
                    { 
                        sectionBytes[i] = Compress(srcBytes);
 
                        if (sectionBytes[i].Length < srcBytes.Length)
                        {
                            rpx.SectionHeader[i].sh_flags |= (uint)SHF_RPL.ZLIB;
                            rpx.SectionHeader[i].sh_size = (uint)sectionBytes[i].Length;
                        }
                        else
                        {
                            sectionBytes[i] = srcBytes;
                        }
                    }
                }
                else
                    sectionBytes[i] = new byte[0];
            }

            src.Close();

            List<KeyValuePair<int, Elf32_Shdr>> shList = new List<KeyValuePair<int, Elf32_Shdr>>();

            for (int i = 0; i < rpx.SectionHeader.Length; i++)
                shList.Add(new KeyValuePair<int, Elf32_Shdr>(i, rpx.SectionHeader[i]));
            shList.Sort((pair1, pair2) => Elf32_Shdr.CompareByOffset(pair1.Value, pair2.Value));

            byte[] paddingBytes = new byte[0x40 + rpx.Header.e_shnum * 0x2C];

            FileStream dest = File.Open(destination, FileMode.Create);
            dest.Write(paddingBytes, 0, paddingBytes.Length);

            for (int i = 0; i < shList.Count; i++)
            {
                if (rpx.CRC[shList[i].Key] != 0)
                {
                    rpx.SectionHeader[shList[i].Key].sh_offset = (uint)dest.Position;
                    dest.Write(sectionBytes[shList[i].Key], 0, sectionBytes[shList[i].Key].Length);

                    if (rpx.IsPadded)
                    {
                        int padding = GetPhysicalSectionSize(sectionBytes[shList[i].Key].Length) - sectionBytes[shList[i].Key].Length;
                        paddingBytes = new byte[padding];
                        dest.Write(paddingBytes, 0, paddingBytes.Length);
                    }
                }
            }

            dest.Position = 0;
            dest.Write(rpx.Header.ToArray(), 0, 0x34);

            dest.Position = 0x40;
            for (int i = 0; i < rpx.SectionHeader.Length; i++)
                dest.Write(rpx.SectionHeader[i].ToArray(rpx.Header.e_ident[(byte)EI.DataEncoding]), 0, 0x28);

            for (int i = 0; i < rpx.CRC.Length; i++)
            {
                dest.WriteByte((byte)(rpx.CRC[i] >> 24));
                dest.WriteByte((byte)((rpx.CRC[i] >> 16) & 0xFF));
                dest.WriteByte((byte)((rpx.CRC[i] >> 8) & 0xFF));
                dest.WriteByte((byte)(rpx.CRC[i] & 0xFF));
            }

            dest.Close();

            /*int shSize = rpx.Header.e_shnum * 0x2C; // 0x2C = rpx.Header.e_shentsize + 4 bytes of CRC32            
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

            dest.Close();*/
        }

        protected static int GetPhysicalSectionSize(int size)
        {
            return size % 0x40 == 0 ? size : size / 0x40 * 0x40 + 0x40;
        }
    }
}
