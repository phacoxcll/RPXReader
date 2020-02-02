using System;
using System.Text;

namespace RPXReader
{
    using Elf32_Addr = UInt32;
    using Elf32_Off = UInt32;
    using Elf32_Half = UInt16;
    using Elf32_Word = UInt32;
    using Elf32_Sword = Int32;

    using Elf64_Addr = UInt64;
    using Elf64_Off = UInt64;
    using Elf64_Half = UInt16;
    using Elf64_Word = UInt32;
    using Elf64_Sword = Int32;
    using Elf64_Xword = UInt64;
    using Elf64_Sxword = Int64;

    #region Enumerators

    /// <summary>ELF Identification.</summary>
    public enum EI : byte
    {
        /// <summary>EI_MAG0</summary>
        MagicNumber0 = 0,
        /// <summary>EI_MAG1</summary>
        MagicNumber1 = 1,
        /// <summary>EI_MAG2</summary>
        MagicNumber2 = 2,
        /// <summary>EI_MAG3</summary>
        MagicNumber3 = 3,
        /// <summary>EI_CLASS</summary>
        FileClass = 4,
        /// <summary>EI_DATA</summary>
        DataEncoding = 5,
        /// <summary>EI_VERSION</summary>
        FileVersion = 6,
        /// <summary>EI_OSABI</summary>
        Target_OS_ABI = 7,
        /// <summary>EI_ABIVERSION</summary>
        ABIVersion = 8,
        /// <summary>EI_PAD</summary>
        StartPadding = 9,
        /// <summary>EI_NIDENT <para>Size of ELF Identification array.</para></summary>
        Size = 16
    }

    /// <summary>ELF Class.</summary>
    public enum EC : byte
    {
        /// <summary>ELFCLASSNONE <para>Invalid class.</para></summary>
        None = 0,
        /// <summary>ELFCLASS32 <para>32-bit objects.</para></summary>
        ELF32 = 1,
        /// <summary>ELFCLASS64 <para>64-bit objects.</para></summary>
        ELF64 = 2
    }

    /// <summary>ELF Data encoding.</summary>
    public enum ED : byte
    {
        /// <summary>ELFDATANONE <para>Invalid data encoding.</para></summary>
        None = 0,
        /// <summary>ELFDATA2LSB <para>Least Significant Bit encoding.</para></summary>
        LittleEndian = 1,
        /// <summary>ELFDATA2MSB <para>Most Significant Bit encoding.</para></summary>
        BigEndian = 2
    }

    /// <summary>ELF Type.</summary>
    public enum ET : ushort
    {
        /// <summary>ET_NONE</summary>
        None = 0,
        /// <summary>ET_REL</summary>
        Relocatable = 1,
        /// <summary>ET_EXEC</summary>
        Executable = 2,
        /// <summary>ET_DYN</summary>
        SharedObject = 3,
        /// <summary>ET_CORE</summary>
        Core = 4,
        /// <summary>ET_LOOS</summary>
        Low_OS_Specific = 0xFE00,
        /// <summary>ET_HIOS</summary>
        High_OS_Specific = 0xFEFF,
        /// <summary>ET_LOPROC</summary>
        Low_Processor_Specific = 0xFF00,
        /// <summary>ET_HIPROC</summary>
        High_Processor_Specific = 0xFFFF
    }

    /// <summary>ELF OS ABI.</summary>
    public enum EO : byte
    {
        /// <summary>ELFOSABI_NONE <para>No extensions or unspecified.</para></summary>
        None = 0,
        /// <summary>ELFOSABI_HPUX <para>Hewlett-Packard HP-UX.</para></summary>
        HPUX = 1,
        /// <summary>ELFOSABI_NETBSD <para>NetBSD.</para></summary>
        NETBSD = 2,
        /// <summary>ELFOSABI_SOLARIS <para>Sun Solaris.</para></summary>
        SOLARIS = 6,
        /// <summary>ELFOSABI_AIX <para>AIX.</para></summary>
        AIX = 7,
        /// <summary>ELFOSABI_IRIX <para>IRIX.</para></summary>
        IRIX = 8,
        /// <summary>ELFOSABI_FREEBSD <para>FreeBSD.</para></summary>
        FREEBSD = 9,
        /// <summary>ELFOSABI_TRU64 <para>Compaq TRU64 UNIX.</para></summary>
        TRU64 = 10,
        /// <summary>ELFOSABI_MODESTO <para>Novell Modesto.</para></summary>
        MODESTO = 11,
        /// <summary>ELFOSABI_OPENBSD <para>Open BSD.</para></summary>
        OPENBSD = 12,
        /// <summary>ELFOSABI_OPENVMS <para>Open VMS.</para></summary>
        OPENVMS = 13,
        /// <summary>ELFOSABI_NSK <para>Hewlett-Packard Non-Stop Kernel.</para></summary>
        NSK = 14,
        /// <summary>ELFOSABI_AROS <para>Amiga Research OS.</para></summary>
        AROS = 15
    }

    /// <summary>Section Header Type.</summary>
    public enum SHT : uint
    {
        /// <summary>SHT_NULL <para>Inactive header, does not have an associated section.</para></summary>
        NULL = 0,
        /// <summary>SHT_PROGBITS <para>Defined by the program.</para></summary>
        PROGBITS = 1,
        /// <summary>SHT_SYMTAB <para>Symbol Table.</para></summary>
        SYMTAB = 2,
        /// <summary>SHT_STRTAB <para>String Table.</para></summary>
        STRTAB = 3,
        /// <summary>SHT_RELA <para>Relocation with explicit addends.</para></summary>
        RELA = 4,
        /// <summary>SHT_HASH <para>Hash Table.</para></summary>
        HASH = 5,
        /// <summary>SHT_DYNAMIC <para>Dynamic Section.</para></summary>
        DYNAMIC = 6,
        /// <summary>SHT_NOTE <para>Note Section.</para></summary>
        NOTE = 7,
        /// <summary>SHT_NOBITS <para>Similar to SHT_PROGBITS but the section has no bits.</para></summary>
        NOBITS = 8,
        /// <summary>SHT_REL <para>Relocation without explicit addends.</para></summary>
        REL = 9,
        /// <summary>SHT_SHLIB <para>Reserved, the section has an unspecified semantics.</para></summary>
        SHLIB = 10,
        /// <summary>SHT_DYNSYM <para>Dynamic Symbol Table.</para></summary>
        DYNSYM = 11,
        /// <summary>SHT_INIT_ARRAY <para>Initialization functions array pointers.</para></summary>
        INIT_ARRAY = 14,
        /// <summary>SHT_FINI_ARRAY <para>Termination functions array pointers.</para></summary>
        FINI_ARRAY = 15,
        /// <summary>SHT_PREINIT_ARRAY <para>Preinitialization functions array pointers.</para></summary>
        PREINIT_ARRAY = 16,
        /// <summary>SHT_GROUP <para>Section group.</para></summary>
        GROUP = 17,
        /// <summary>SHT_SYMTAB_SHNDX <para>Symbol Table Section Header Indexes.</para></summary>
        SYMTAB_SHNDX = 18,
        /// <summary>SHT_LOOS <para>Low OS specific.</para></summary>
        LOOS = 0x60000000,
        /// <summary>SHT_HIOS <para>High OS specific.</para></summary>
        HIOS = 0x6FFFFFFF,
        /// <summary>SHT_LOPROC <para>Low Processor specific.</para></summary>
        LOPROC = 0x70000000,
        /// <summary>SHT_HIPROC <para>High Processor specific.</para></summary>
        HIPROC = 0x7FFFFFFF,
        /// <summary>SHT_LOUSER <para>Low User specific.</para></summary>
        LOUSER = 0x80000000,
        /// <summary>SHT_HIUSER <para>High User specific.</para></summary>
        HIUSER = 0x8FFFFFFF
    }

    /// <summary>Section Header Type RPL.</summary>
    public enum SHT_RPL : uint
    {
        // <summary>SHT_RPL_NULL</summary>
        //NULL = 0x80000000,
        /// <summary>SHT_RPL_EXPORTS</summary>
        EXPORTS = 0x80000001,
        /// <summary>SHT_RPL_IMPORTS</summary>
        IMPORTS = 0x80000002,
        /// <summary>SHT_RPL_CRCS</summary>
        CRCS = 0x80000003,
        /// <summary>SHT_RPL_FILEINFO</summary>
        FILEINFO = 0x80000004
    }

    /// <summary>Section Header Flag.</summary>
    public enum SHF : uint
    {
        /// <summary>SHF_WRITE</summary>
        WRITE = 0x1,
        /// <summary>SHF_ALLOC</summary>
        ALLOC = 0x2,
        /// <summary>SHF_EXECINSTR</summary>
        EXECINSTR = 0x4,
        /// <summary>SHF_MERGE</summary>
        MERGE = 0x10,
        /// <summary>SHF_STRINGS</summary>
        STRINGS = 0x20,
        /// <summary>SHF_INFO_LINK</summary>
        INFO_LINK = 0x40,
        /// <summary>SHF_LINK_ORDER</summary>
        LINK_ORDER = 0x80,
        /// <summary>SHF_OS_NONCONFORMING <para>Extra OS processing required.</para></summary>
        OS_NONCONFORMING = 0x100,
        /// <summary>SHF_GROUP</summary>
        GROUP = 0x200,
        // <summary>SHF_TLS <para>Thread-Local Storage.</para></summary>
        //TLS = 0x400,
        /// <summary>SHF_MASKOS <para>OS mask.</para></summary>
        MASKOS = 0x0FF00000,
        /// <summary>SHF_MASKPROC <para>Processor mask.</para></summary>
        MASKPROC = 0xF0000000
    }

    /// <summary>Section Header Flag RPL.</summary>
    public enum SHF_RPL : uint
    {
        /// <summary>SHF_RPL_ZLIB</summary>
        ZLIB = 0x08000000
    }

    public enum Machine : ushort
    {
        /// <summary>EM_NONE <para>No machine.</para></summary>
        None = 0,
        /// <summary>EM_M32 <para>AT&T WE 32100.</para></summary>
        M32 = 1,
        /// <summary>EM_SPARC <para>SPARC.</para></summary>
        SPARC = 2,
        /// <summary>EM_386 <para>Intel 386.</para></summary>
        Intel386 = 3,
        /// <summary>EM_68K <para>Motorola 68000.</para></summary>
        M68K = 4,
        /// <summary>EM_88K <para>Motorola 88000.</para></summary>
        M88K = 5,
        /// <summary>EM_IAMCU <para>Intel MCU.</para></summary>
        Intel486 = 6,
        /// <summary>EM_860 <para>Intel 80860.</para></summary>
        Intel860 = 7,
        /// <summary>EM_MIPS <para>MIPS R3000.</para></summary>
        MIPS = 8,
        /// <summary>EM_S370 <para>IBM System/370.</para></summary>
        S370 = 9,
        /// <summary>EM_MIPS_RS3_LE <para>MIPS RS3000 Little-endian.</para></summary>
        MIPSRS3LE = 10,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_11 = 11,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_12 = 12,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_13 = 13,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_14 = 14,
        /// <summary>EM_PARISC <para>Hewlett-Packard PA-RISC.</para></summary>
        PARISC = 15,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_16 = 16,
        /// <summary>EM_VPP500 <para>Fujitsu VPP500.</para></summary>
        VPP500 = 17,
        /// <summary>EM_SPARC32PLUS <para>Enhanced instruction set SPARC.</para></summary>
        SPARC32Plus = 18,
        /// <summary>EM_960 <para>Intel 80960.</para></summary>
        Intel960 = 19,
        /// <summary>EM_PPC <para>PowerPC.</para></summary>
        PowerPC = 20,
        /// <summary>EM_PPC64 <para>PowerPC64.</para></summary>
        PowerPC64 = 21,
        /// <summary>EM_S390 <para>IBM System/390.</para></summary>
        S390 = 22,
        /// <summary>EM_SPU <para>IBM SPU/SPC.</para></summary>
        SPU = 23,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_24 = 24,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_25 = 25,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_26 = 26,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_27 = 27,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_28 = 28,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_29 = 29,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_30 = 30,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_31 = 31,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_32 = 32,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_33 = 33,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_34 = 34,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_35 = 35,
        /// <summary>EM_V800 <para>NEC V800.</para></summary>
        V800 = 36,
        /// <summary>EM_FR20 <para>Fujitsu FR20.</para></summary>
        FR20 = 37,
        /// <summary>EM_RH32 <para>TRW RH-32.</para></summary>
        RH32 = 38,
        /// <summary>EM_RCE <para>Motorola RCE.</para></summary>
        RCE = 39,
        /// <summary>EM_ARM <para>ARM.</para></summary>
        ARM = 40,
        /// <summary>EM_ALPHA <para>DEC Alpha.</para></summary>
        Alpha = 41,
        /// <summary>EM_SH <para>Hitachi SH.</para></summary>
        SuperH = 42,
        /// <summary>EM_SPARCV9 <para>SPARC V9.</para></summary>
        SPARCv9 = 43,
        /// <summary>EM_TRICORE <para>Siemens TriCore.</para></summary>
        TriCore = 44,
        /// <summary>EM_ARC <para>Argonaut RISC Core.</para></summary>
        ARC = 45,
        /// <summary>EM_H8_300 <para>Hitachi H8/300.</para></summary>
        H8300 = 46,
        /// <summary>EM_H8_300H <para>Hitachi H8/300H.</para></summary>
        H8300H = 47,
        /// <summary>EM_H8S <para>Hitachi H8S.</para></summary>
        H8S = 48,
        /// <summary>EM_H8_500 <para>Hitachi H8/500.</para></summary>
        H8500 = 49,
        /// <summary>EM_IA_64 <para>Intel IA-64 processor architecture.</para></summary>
        IA64 = 50,
        /// <summary>EM_MIPS_X <para>Stanford MIPS-X.</para></summary>
        MIPSX = 51,
        /// <summary>EM_COLDFIRE <para>Motorola ColdFire.</para></summary>
        ColdFire = 52,
        /// <summary>EM_68HC12 <para>Motorola M68HC12.</para></summary>
        M68HC12 = 53,
        /// <summary>EM_MMA <para>Fujitsu MMA Multimedia Accelerator.</para></summary>
        MMA = 54,
        /// <summary>EM_PCP <para>Siemens PCP.</para></summary>
        PCP = 55,
        /// <summary>EM_NCPU <para>Sony nCPU embedded RISC processor.</para></summary>
        NCPU = 56,
        /// <summary>EM_NDR1 <para>Denso NDR1 microprocessor.</para></summary>
        NDR1 = 57,
        /// <summary>EM_STARCORE <para>Motorola Star*Core processor.</para></summary>
        StarCore = 58,
        /// <summary>EM_ME16 <para>Toyota ME16 processor.</para></summary>
        ME16 = 59,
        /// <summary>EM_ST100 <para>STMicroelectronics ST100 processor.</para></summary>
        ST100 = 60,
        /// <summary>EM_TINYJ <para>Advanced Logic Corp. TinyJ embedded processor family.</para></summary>
        TinyJ = 61,
        /// <summary>EM_X86_64 <para>AMD x86-64 architecture.</para></summary>
        AMD64 = 62,
        /// <summary>EM_PDSP <para>Sony DSP Processor.</para></summary>
        PDSP = 63,
        /// <summary>EM_PDP10 <para>Digital Equipment Corp. PDP-10.</para></summary>
        PDP10 = 64,
        /// <summary>EM_PDP11 <para>Digital Equipment Corp. PDP-11.</para></summary>
        PDP11 = 65,
        /// <summary>EM_FX66 <para>Siemens FX66 microcontroller.</para></summary>
        FX66 = 66,
        /// <summary>EM_ST9PLUS <para>STMicroelectronics ST9+ 8/16 bit microcontroller.</para></summary>
        ST9PLUS = 67,
        /// <summary>EM_ST7 <para>STMicroelectronics ST7 8-bit microcontroller.</para></summary>
        ST7 = 68,
        /// <summary>EM_68HC16 <para>Motorola MC68HC16 Microcontroller.</para></summary>
        M68HC16 = 69,
        /// <summary>EM_68HC11 <para>Motorola MC68HC11 Microcontroller.</para></summary>
        M68HC11 = 70,
        /// <summary>EM_68HC08 <para>Motorola MC68HC08 Microcontroller.</para></summary>
        M68HC08 = 71,
        /// <summary>EM_68HC05 <para>Motorola MC68HC05 Microcontroller.</para></summary>
        M68HC05 = 72,
        /// <summary>EM_SVX <para>Silicon Graphics SVx.</para></summary>
        SVX = 73,
        /// <summary>EM_ST19 <para>STMicroelectronics ST19 8-bit microcontroller.</para></summary>
        ST19 = 74,
        /// <summary>EM_VAX <para>Digital VAX.</para></summary>
        VAX = 75,
        /// <summary>EM_CRIS <para>Axis Communications 32-bit embedded processor.</para></summary>
        CRIS = 76,
        /// <summary>EM_JAVELIN <para>Infineon Technologies 32-bit embedded processor.</para></summary>
        Javelin = 77,
        /// <summary>EM_FIREPATH <para>Element 14 64-bit DSP Processor.</para></summary>
        FirePath = 78,
        /// <summary>EM_ZSP <para>LSI Logic 16-bit DSP Processor.</para></summary>
        ZSP = 79,
        /// <summary>EM_MMIX <para>Donald Knuth's educational 64-bit processor.</para></summary>
        MMIX = 80,
        /// <summary>EM_HUANY <para>Harvard University machine-independent object files.</para></summary>
        HUANY = 81,
        /// <summary>EM_PRISM <para>SiTera Prism.</para></summary>
        PRISM = 82,
        /// <summary>EM_AVR <para>Atmel AVR 8-bit microcontroller.</para></summary>
        AVR = 83,
        /// <summary>EM_FR30 <para>Fujitsu FR30.</para></summary>
        FR30 = 84,
        /// <summary>EM_D10V <para>Mitsubishi D10V.</para></summary>
        D10V = 85,
        /// <summary>EM_D30V <para>Mitsubishi D30V.</para></summary>
        D30V = 86,
        /// <summary>EM_V850 <para>NEC v850.</para></summary>
        V850 = 87,
        /// <summary>EM_M32R <para>Mitsubishi M32R.</para></summary>
        M32R = 88,
        /// <summary>EM_MN10300 <para>Matsushita MN10300.</para></summary>
        MN10300 = 89,
        /// <summary>EM_MN10200 <para>Matsushita MN10200.</para></summary>
        MN10200 = 90,
        /// <summary>EM_PJ <para>picoJava.</para></summary>
        PicoJava = 91,
        /// <summary>EM_OPENRISC <para>OpenRISC 32-bit embedded processor.</para></summary>
        OpenRISC = 92,
        /// <summary>EM_ARC_COMPACT <para>ARC International ARCompact processor.</para></summary>
        ARCompact = 93,
        /// <summary>EM_XTENSA <para>Tensilica Xtensa Architecture.</para></summary>
        Xtensa = 94,
        /// <summary>EM_VIDEOCORE <para>Alphamosaic VideoCore processor.</para></summary>
        VideoCore = 95,
        /// <summary>EM_TMM_GPP <para>Thompson Multimedia General Purpose Processor.</para></summary>
        TMMGPP = 96,
        /// <summary>EM_NS32K <para>National Semiconductor 32000 series.</para></summary>
        NS32K = 97,
        /// <summary>EM_TPC <para>Tenor Network TPC processor.</para></summary>
        TPC = 98,
        /// <summary>EM_SNP1K <para>Trebia SNP 1000 processor.</para></summary>
        SNP1k = 99,
        /// <summary>EM_ST200 <para>STMicroelectronics (www.st.com) ST200.</para></summary>
        ST200 = 100,
        /// <summary>EM_IP2K <para>Ubicom IP2xxx microcontroller family.</para></summary>
        IP2K = 101,
        /// <summary>EM_MAX <para>MAX Processor.</para></summary>
        MAX = 102,
        /// <summary>EM_CR <para>National Semiconductor CompactRISC microprocessor.</para></summary>
        CompactRISC = 103,
        /// <summary>EM_F2MC16 <para>Fujitsu F2MC16.</para></summary>
        F2MC16 = 104,
        /// <summary>EM_MSP430 <para>Texas Instruments embedded microcontroller msp430.</para></summary>
        MSP430 = 105,
        /// <summary>EM_BLACKFIN <para>Analog Devices Blackfin (DSP) processor.</para></summary>
        Blackfin = 106,
        /// <summary>EM_SE_C33 <para>S1C33 Family of Seiko Epson processors.</para></summary>
        S1C33 = 107,
        /// <summary>EM_SEP <para>Sharp embedded microprocessor.</para></summary>
        SEP = 108,
        /// <summary>EM_ARCA <para>Arca RISC Microprocessor.</para></summary>
        ArcaRISC = 109,
        /// <summary>EM_UNICORE <para>Microprocessor series from PKU-Unity Ltd. and MPRC of Peking University.</para></summary>
        UNICORE = 110,
        /// <summary>EM_EXCESS <para>eXcess: 16/32/64-bit configurable embedded CPU.</para></summary>
        Excess = 111,
        /// <summary>EM_DXP <para>Icera Semiconductor Inc. Deep Execution Processor.</para></summary>
        DXP = 112,
        /// <summary>EM_ALTERA_NIOS2 <para>Altera Nios II soft-core processor.</para></summary>
        AlteraNios2 = 113,
        /// <summary>EM_CRX <para>National Semiconductor CompactRISC CRX.</para></summary>
        CRX = 114,
        /// <summary>EM_XGATE <para>Motorola XGATE embedded processor.</para></summary>
        XGATE = 115,
        /// <summary>EM_C166 <para>Infineon C16x/XC16x processor.</para></summary>
        C166 = 116,
        /// <summary>EM_M16C <para>Renesas M16C series microprocessors.</para></summary>
        M16C = 117,
        /// <summary>EM_DSPIC30F <para>Microchip Technology dsPIC30F Digital Signal.</para></summary>
        DSPIC30F = 118,
        /// <summary>EM_CE <para>Freescale Communication Engine RISC core.</para></summary>
        EngineRISC = 119,
        /// <summary>EM_M32C <para>Renesas M32C series microprocessors.</para></summary>
        M32C = 120,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_121 = 121,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_122 = 122,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_123 = 123,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_124 = 124,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_125 = 125,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_126 = 126,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_127 = 127,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_128 = 128,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_129 = 129,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_130 = 130,
        /// <summary>EM_TSK3000 <para>Altium TSK3000 core.</para></summary>
        TSK3000 = 131,
        /// <summary>EM_RS08 <para>Freescale RS08 embedded processor.</para></summary>
        RS08 = 132,
        /// <summary>EM_SHARC <para>Analog Devices SHARC family of 32-bit DSP processors.</para></summary>
        SHARC = 133,
        /// <summary>EM_ECOG2 <para>Cyan Technology eCOG2 microprocessor.</para></summary>
        ECOG2 = 134,
        /// <summary>EM_SCORE7 <para>Sunplus S+core7 RISC processor.</para></summary>
        Score7 = 135,
        /// <summary>EM_DSP24 <para>New Japan Radio (NJR) 24-bit DSP Processor.</para></summary>
        DSP24 = 136,
        /// <summary>EM_VIDEOCORE3 <para>Broadcom VideoCore III processor.</para></summary>
        VideoCore3 = 137,
        /// <summary>EM_LATTICEMICO32 <para>RISC processor for Lattice FPGA architecture.</para></summary>
        LatticeMico32 = 138,
        /// <summary>EM_SE_C17 <para>Seiko Epson C17 family.</para></summary>
        SeikoEpsonC17 = 139,
        /// <summary>EM_TI_C6000 <para>The Texas Instruments TMS320C6000 DSP family.</para></summary>
        TIC6000 = 140,
        /// <summary>EM_TI_C2000 <para>The Texas Instruments TMS320C2000 DSP family.</para></summary>
        TIC2000 = 141,
        /// <summary>EM_TI_C5500 <para>The Texas Instruments TMS320C55x DSP family.</para></summary>
        TIC5500 = 142,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_143 = 143,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_144 = 144,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_145 = 145,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_146 = 146,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_147 = 147,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_148 = 148,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_149 = 149,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_150 = 150,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_151 = 151,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_152 = 152,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_153 = 153,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_154 = 154,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_155 = 155,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_156 = 156,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_157 = 157,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_158 = 158,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_159 = 159,
        /// <summary>EM_MMDSP_PLUS <para>STMicroelectronics 64bit VLIW Data Signal Processor.</para></summary>
        MMDSPPlus = 160,
        /// <summary>EM_CYPRESS_M8C <para>Cypress M8C microprocessor.</para></summary>
        CypressM8C = 161,
        /// <summary>EM_R32C <para>Renesas R32C series microprocessors.</para></summary>
        R32C = 162,
        /// <summary>EM_TRIMEDIA <para>NXP Semiconductors TriMedia architecture family.</para></summary>
        TriMedia = 163,
        /// <summary>EM_HEXAGON <para>Qualcomm Hexagon processor.</para></summary>
        Hexagon = 164,
        /// <summary>EM_8051 <para>Intel 8051 and variants.</para></summary>
        Intel8051 = 165,
        /// <summary>EM_STXP7X <para>STMicroelectronics STxP7x family of configurable and extensible RISC processors.</para></summary>
        STxP7x = 166,
        /// <summary>EM_NDS32 <para>Andes Technology compact code size embedded RISC processor family.</para></summary>
        NDS32 = 167,
        /// <summary>EM_ECOG1 <para>Cyan Technology eCOG1X family.</para></summary>
        ECOG1 = 168,
        /// <summary>EM_MAXQ30 <para>Dallas Semiconductor MAXQ30 Core Micro-controllers.</para></summary>
        MAXQ30 = 169,
        /// <summary>EM_XIMO16 <para>New Japan Radio (NJR) 16-bit DSP Processor.</para></summary>
        XIMO16 = 170,
        /// <summary>EM_MANIK <para>M2000 Reconfigurable RISC Microprocessor.</para></summary>
        MANIK = 171,
        /// <summary>EM_CRAYNV2 <para>Cray Inc. NV2 vector architecture.</para></summary>
        CrayNV2 = 172,
        /// <summary>EM_RX <para>Renesas RX family.</para></summary>
        RX = 173,
        /// <summary>EM_METAG <para>Imagination Technologies META processor architecture.</para></summary>
        METAG = 174,
        /// <summary>EM_MCST_ELBRUS <para>MCST Elbrus general purpose hardware architecture.</para></summary>
        MCSTElbrus = 175,
        /// <summary>EM_ECOG16 <para>Cyan Technology eCOG16 family.</para></summary>
        ECOG16 = 176,
        /// <summary>EM_CR16 <para>National Semiconductor CompactRISC CR16 16-bit microprocessor.</para></summary>
        CR16 = 177,
        /// <summary>EM_ETPU <para>Freescale Extended Time Processing Unit.</para></summary>
        ETPU = 178,
        /// <summary>EM_SLE9X <para>Infineon Technologies SLE9X core.</para></summary>
        SLE9X = 179,
        /// <summary>EM_L10M <para>Intel L10M.</para></summary>
        L10M = 180,
        /// <summary>EM_K10M <para>Intel K10M.</para></summary>
        K10M = 181,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_182 = 182,
        /// <summary>EM_AARCH64 <para>ARM AArch64.</para></summary>
        AArch64 = 183,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_184 = 184,
        /// <summary>EM_AVR32 <para>Atmel Corporation 32-bit microprocessor family.</para></summary>
        AVR32 = 185,
        /// <summary>EM_STM8 <para>STMicroeletronics STM8 8-bit microcontroller.</para></summary>
        STM8 = 186,
        /// <summary>EM_TILE64 <para>Tilera TILE64 multicore architecture family.</para></summary>
        TILE64 = 187,
        /// <summary>EM_TILEPRO <para>Tilera TILEPro multicore architecture family.</para></summary>
        TILEPro = 188,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_189 = 189,
        /// <summary>EM_CUDA <para>NVIDIA CUDA architecture.</para></summary>
        CUDA = 190,
        /// <summary>EM_TILEGX <para>Tilera TILE-Gx multicore architecture family.</para></summary>
        TILEGx = 191,
        /// <summary>EM_CLOUDSHIELD <para>CloudShield architecture family.</para></summary>
        CloudShield = 192,
        /// <summary>EM_COREA_1ST <para>KIPO-KAIST Core-A 1st generation processor family.</para></summary>
        CoreA1st = 193,
        /// <summary>EM_COREA_2ND <para>KIPO-KAIST Core-A 2nd generation processor family.</para></summary>
        CoreA2nd = 194,
        /// <summary>EM_ARC_COMPACT2 <para>Synopsys ARCompact V2.</para></summary>
        ARCompact2 = 195,
        /// <summary>EM_OPEN8 <para>Open8 8-bit RISC soft processor core.</para></summary>
        Open8 = 196,
        /// <summary>EM_RL78 <para>Renesas RL78 family.</para></summary>
        RL78 = 197,
        /// <summary>EM_VIDEOCORE5 <para>Broadcom VideoCore V processor.</para></summary>
        VideoCore5 = 198,
        /// <summary>EM_78KOR <para>Renesas 78KOR family.</para></summary>
        R78KOR = 199,
        /// <summary>EM_56800EX <para>Freescale 56800EX Digital Signal Controller (DSC).</para></summary>
        F56800EX = 200,
        /// <summary>EM_BA1 <para>Beyond BA1 CPU architecture.</para></summary>
        BeyondBA1 = 201,
        /// <summary>EM_BA2 <para>Beyond BA2 CPU architecture.</para></summary>
        BeyondBA2 = 202,
        /// <summary>EM_XCORE <para>XMOS xCORE processor family.</para></summary>
        XCORE = 203,
        /// <summary>EM_MCHP_PIC <para>Microchip 8-bit PIC(r) family.</para></summary>
        MicrochipPIC = 204,
        /// <summary>EM_INTEL205 <para>Reserved by Intel.</para></summary>
        EM_205 = 205,
        /// <summary>EM_INTEL206 <para>Reserved by Intel.</para></summary>
        EM_206 = 206,
        /// <summary>EM_INTEL207 <para>Reserved by Intel.</para></summary>
        EM_207 = 207,
        /// <summary>EM_INTEL208 <para>Reserved by Intel.</para></summary>
        EM_208 = 208,
        /// <summary>EM_INTEL209 <para>Reserved by Intel.</para></summary>
        EM_209 = 209,
        /// <summary>EM_KM32 <para>KM211 KM32 32-bit processor.</para></summary>
        KM32 = 210,
        /// <summary>EM_KMX32 <para>KM211 KMX32 32-bit processor.</para></summary>
        KMX32 = 211,
        /// <summary>EM_KMX16 <para>KM211 KMX16 16-bit processor.</para></summary>
        KMX16 = 212,
        /// <summary>EM_KMX8 <para>KM211 KMX8 8-bit processor.</para></summary>
        KMX8 = 213,
        /// <summary>EM_KVARC <para>KM211 KVARC processor.</para></summary>
        KVARC = 214,
        /// <summary>EM_CDP <para>Paneve CDP architecture family.</para></summary>
        PaneveCDP = 215,
        /// <summary>EM_COGE <para>Cognitive Smart Memory Processor.</para></summary>
        Cognitive = 216,
        /// <summary>EM_COOL <para>iCelero CoolEngine.</para></summary>
        CoolEngine = 217,
        /// <summary>EM_NORC <para>Nanoradio Optimized RISC.</para></summary>
        Nanoradio = 218,
        /// <summary>EM_CSR_KALIMBA <para>CSR Kalimba architecture family.</para></summary>
        CSRKalimba = 219,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_220 = 220,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_221 = 221,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_222 = 222,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_223 = 223,
        /// <summary>EM_AMDGPU <para>AMD GPU architecture.</para></summary>
        AMDGPU = 224,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_225 = 225,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_226 = 226,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_227 = 227,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_228 = 228,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_229 = 229,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_230 = 230,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_231 = 231,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_232 = 232,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_233 = 233,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_234 = 234,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_235 = 235,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_236 = 236,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_237 = 237,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_238 = 238,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_239 = 239,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_240 = 240,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_241 = 241,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_242 = 242,
        /// <summary>EM_RISCV <para>RISC-V.</para></summary>
        RISCV = 243,
        /// <summary>EM_LANAI <para>Lanai 32-bit processor.</para></summary>
        Lanai = 244,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_245 = 245,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_246 = 246,
        /// <summary>EM_BPF <para>Linux kernel bpf virtual machine.</para></summary>
        BPF = 247
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_248 = 248,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_249 = 249,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_250 = 250,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_251 = 251,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_252 = 252,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_253 = 253,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_254 = 254,
        // <summary>Reserved <para>Reserved for future use.</para></summary>
        //EM_255 = 255,
    }

    #endregion

    public struct Elf32_Ehdr
    {
        public byte[] e_ident;
        public Elf32_Half e_type;
        public Elf32_Half e_machine;
        public Elf32_Word e_version;
        public Elf32_Addr e_entry;
        public Elf32_Off e_phoff;
        public Elf32_Off e_shoff;
        public Elf32_Word e_flags;
        public Elf32_Half e_ehsize;
        public Elf32_Half e_phentsize;
        public Elf32_Half e_phnum;
        public Elf32_Half e_shentsize;
        public Elf32_Half e_shnum;
        public Elf32_Half e_shstrndx;

        public Elf32_Ehdr(byte[] data, ValueRead read)
        {
            e_ident = new byte[(byte)EI.Size];
            for (int i = 0; i < (byte)EI.Size; i++)
                e_ident[i] = data[i];
            e_type = read.Elf32_Half(data, 0x10);
            e_machine = read.Elf32_Half(data, 0x12);
            e_version = read.Elf32_Word(data, 0x14);
            e_entry = read.Elf32_Addr(data, 0x18);
            e_phoff = read.Elf32_Off(data, 0x1C);
            e_shoff = read.Elf32_Off(data, 0x20);
            e_flags = read.Elf32_Word(data, 0x24);
            e_ehsize = read.Elf32_Half(data, 0x28);
            e_phentsize = read.Elf32_Half(data, 0x2A);
            e_phnum = read.Elf32_Half(data, 0x2C);
            e_shentsize = read.Elf32_Half(data, 0x2E);
            e_shnum = read.Elf32_Half(data, 0x30);
            e_shstrndx = read.Elf32_Half(data, 0x32);
        }

        public byte[] ToArray()
        {
            byte[] array = new byte[0x34];

            Array.Copy(e_ident, array, 0x10);
            if (e_ident[(byte)EI.DataEncoding] == (byte)ED.LittleEndian)
            {
                array[0x10] = (byte)(e_type & 0xFF);
                array[0x11] = (byte)(e_type >> 8);
                array[0x12] = (byte)(e_machine & 0xFF);
                array[0x13] = (byte)(e_machine >> 8);
                array[0x14] = (byte)(e_version & 0xFF);
                array[0x15] = (byte)((e_version >> 8) & 0xFF);
                array[0x16] = (byte)((e_version >> 16) & 0xFF);
                array[0x17] = (byte)(e_version >> 24);
                array[0x18] = (byte)(e_entry & 0xFF);
                array[0x19] = (byte)((e_entry >> 8) & 0xFF);
                array[0x1A] = (byte)((e_entry >> 16) & 0xFF);
                array[0x1B] = (byte)(e_entry >> 24);
                array[0x1C] = (byte)(e_phoff & 0xFF);
                array[0x1D] = (byte)((e_phoff >> 8) & 0xFF);
                array[0x1E] = (byte)((e_phoff >> 16) & 0xFF);
                array[0x1F] = (byte)(e_phoff >> 24);
                array[0x20] = (byte)(e_shoff & 0xFF);
                array[0x21] = (byte)((e_shoff >> 8) & 0xFF);
                array[0x22] = (byte)((e_shoff >> 16) & 0xFF);
                array[0x23] = (byte)(e_shoff >> 24);
                array[0x24] = (byte)(e_flags & 0xFF);
                array[0x25] = (byte)((e_flags >> 8) & 0xFF);
                array[0x26] = (byte)((e_flags >> 16) & 0xFF);
                array[0x27] = (byte)(e_flags >> 24);
                array[0x28] = (byte)(e_ehsize & 0xFF);
                array[0x29] = (byte)(e_ehsize >> 8);
                array[0x2A] = (byte)(e_phentsize & 0xFF);
                array[0x2B] = (byte)(e_phentsize >> 8);
                array[0x2C] = (byte)(e_phnum & 0xFF);
                array[0x2D] = (byte)(e_phnum >> 8);
                array[0x2E] = (byte)(e_shentsize & 0xFF);
                array[0x2F] = (byte)(e_shentsize >> 8);
                array[0x30] = (byte)(e_shnum & 0xFF);
                array[0x31] = (byte)(e_shnum >> 8);
                array[0x32] = (byte)(e_shstrndx & 0xFF);
                array[0x33] = (byte)(e_shstrndx >> 8);
            }
            else if (e_ident[(byte)EI.DataEncoding] == (byte)ED.BigEndian)
            {
                array[0x10] = (byte)(e_type >> 8);
                array[0x11] = (byte)(e_type & 0xFF);
                array[0x12] = (byte)(e_machine >> 8);
                array[0x13] = (byte)(e_machine & 0xFF);
                array[0x14] = (byte)(e_version >> 24);
                array[0x15] = (byte)((e_version >> 16) & 0xFF);
                array[0x16] = (byte)((e_version >> 8) & 0xFF);
                array[0x17] = (byte)(e_version & 0xFF);
                array[0x18] = (byte)(e_entry >> 24);
                array[0x19] = (byte)((e_entry >> 16) & 0xFF);
                array[0x1A] = (byte)((e_entry >> 8) & 0xFF);
                array[0x1B] = (byte)(e_entry & 0xFF);
                array[0x1C] = (byte)(e_phoff >> 24);
                array[0x1D] = (byte)((e_phoff >> 16) & 0xFF);
                array[0x1E] = (byte)((e_phoff >> 8) & 0xFF);
                array[0x1F] = (byte)(e_phoff & 0xFF);
                array[0x20] = (byte)(e_shoff >> 24);
                array[0x21] = (byte)((e_shoff >> 16) & 0xFF);
                array[0x22] = (byte)((e_shoff >> 8) & 0xFF);
                array[0x23] = (byte)(e_shoff & 0xFF);
                array[0x24] = (byte)(e_flags >> 24);
                array[0x25] = (byte)((e_flags >> 16) & 0xFF);
                array[0x26] = (byte)((e_flags >> 8) & 0xFF);
                array[0x27] = (byte)(e_flags & 0xFF);
                array[0x28] = (byte)(e_ehsize >> 8);
                array[0x29] = (byte)(e_ehsize & 0xFF);
                array[0x2A] = (byte)(e_phentsize >> 8);
                array[0x2B] = (byte)(e_phentsize & 0xFF);
                array[0x2C] = (byte)(e_phnum >> 8);
                array[0x2D] = (byte)(e_phnum & 0xFF);
                array[0x2E] = (byte)(e_shentsize >> 8);
                array[0x2F] = (byte)(e_shentsize & 0xFF);
                array[0x30] = (byte)(e_shnum >> 8);
                array[0x31] = (byte)(e_shnum & 0xFF);
                array[0x32] = (byte)(e_shstrndx >> 8);
                array[0x33] = (byte)(e_shstrndx & 0xFF);
            }

            return array;
        }

        public override string ToString()
        {
            StringBuilder strBuilder = new StringBuilder();

            strBuilder.AppendLine("ELF Header:");
            strBuilder.AppendLine("  Magic:  " + ELFHeaderString.Magic(e_ident));
            strBuilder.AppendLine("  Class:                             " + ELFHeaderString.Class(e_ident));
            strBuilder.AppendLine("  Data:                              " + ELFHeaderString.Data(e_ident));
            strBuilder.AppendLine("  Header Version:                    " + ELFHeaderString.HeaderVersion(e_ident));
            if (e_ident[(byte)EI.Target_OS_ABI] == 0xCA && e_ident[(byte)EI.ABIVersion] == 0xFE)
            {
                strBuilder.AppendLine("  OS/ABI:                            0xCA (Wii U CAFE OS)");
                strBuilder.AppendLine("  ABI Version:                       0xFE (Wii U CAFE OS)");
                if (e_type == 0xFE01)
                    strBuilder.AppendLine("  Type:                              0xFE01 (Wii U software)");
                else
                    strBuilder.AppendLine("  Type:                              " + ELFHeaderString.Type(e_type));
            }
            else
            {
                strBuilder.AppendLine("  OS/ABI:                            " + ELFHeaderString.OSABI(e_ident));
                strBuilder.AppendLine("  ABI Version:                       " + ELFHeaderString.ABIVersion(e_ident));
                strBuilder.AppendLine("  Type:                              " + ELFHeaderString.Type(e_type));
            }
            strBuilder.AppendLine("  Machine:                           " + ELFHeaderString.Machine(e_machine));
            strBuilder.AppendLine("  File Version:                      " + ELFHeaderString.FileVersion(e_version));
            strBuilder.AppendLine("  Entry point address:               " + ELFHeaderString.EntryPointAddress(e_entry));
            strBuilder.AppendLine("  Start of program headers:          " + ELFHeaderString.StartOfProgramHeaders(e_phoff));
            strBuilder.AppendLine("  Start of section headers:          " + ELFHeaderString.StartOfSectionHeaders(e_shoff));
            strBuilder.AppendLine("  Flags:                             " + ELFHeaderString.Flags(e_flags));
            strBuilder.AppendLine("  Size of this header:               " + ELFHeaderString.SizeOfThisHeader(e_ehsize));
            strBuilder.AppendLine("  Size of program headers:           " + ELFHeaderString.SizeOfProgramHeaders(e_phentsize));
            strBuilder.AppendLine("  Number of program headers:         " + ELFHeaderString.NumberOfProgramHeaders(e_phnum));
            strBuilder.AppendLine("  Size of section headers:           " + ELFHeaderString.SizeOfSectionHeaders(e_shentsize));
            strBuilder.AppendLine("  Number of section headers:         " + ELFHeaderString.NumberOfSectionHeaders(e_shnum));
            strBuilder.AppendLine("  Section header string table index: " + ELFHeaderString.SectionHeaderStringTableIndex(e_shstrndx));

            return strBuilder.ToString();
        }
    }

    public struct Elf64_Ehdr
    {
        public byte[] e_ident;
        public Elf64_Half e_type;
        public Elf64_Half e_machine;
        public Elf64_Word e_version;
        public Elf64_Addr e_entry;
        public Elf64_Off e_phoff;
        public Elf64_Off e_shoff;
        public Elf64_Word e_flags;
        public Elf64_Half e_ehsize;
        public Elf64_Half e_phentsize;
        public Elf64_Half e_phnum;
        public Elf64_Half e_shentsize;
        public Elf64_Half e_shnum;
        public Elf64_Half e_shstrndx;

        public Elf64_Ehdr(byte[] data, ValueRead read)
        {
            e_ident = new byte[(byte)EI.Size];
            for (int i = 0; i < (byte)EI.Size; i++)
                e_ident[i] = data[i];
            e_type = read.Elf64_Half(data, 0x10);
            e_machine = read.Elf64_Half(data, 0x12);
            e_version = read.Elf64_Word(data, 0x14);
            e_entry = read.Elf64_Addr(data, 0x18);
            e_phoff = read.Elf64_Off(data, 0x20);
            e_shoff = read.Elf64_Off(data, 0x28);
            e_flags = read.Elf64_Word(data, 0x30);
            e_ehsize = read.Elf64_Half(data, 0x34);
            e_phentsize = read.Elf64_Half(data, 0x36);
            e_phnum = read.Elf64_Half(data, 0x38);
            e_shentsize = read.Elf64_Half(data, 0x3A);
            e_shnum = read.Elf64_Half(data, 0x3C);
            e_shstrndx = read.Elf64_Half(data, 0x3E);
        }

        public override string ToString()
        {
            StringBuilder strBuilder = new StringBuilder();

            strBuilder.AppendLine("ELF Header:");
            strBuilder.AppendLine("  Magic:   " + ELFHeaderString.Magic(e_ident));
            strBuilder.AppendLine("  Class:                             " + ELFHeaderString.Class(e_ident));
            strBuilder.AppendLine("  Data:                              " + ELFHeaderString.Data(e_ident));
            strBuilder.AppendLine("  Header Version:                    " + ELFHeaderString.HeaderVersion(e_ident));
            strBuilder.AppendLine("  OS/ABI:                            " + ELFHeaderString.OSABI(e_ident));
            strBuilder.AppendLine("  ABI Version:                       " + ELFHeaderString.ABIVersion(e_ident));
            strBuilder.AppendLine("  Type:                              " + ELFHeaderString.Type(e_type));
            strBuilder.AppendLine("  Machine:                           " + ELFHeaderString.Machine(e_machine));
            strBuilder.AppendLine("  File Version:                      " + ELFHeaderString.FileVersion(e_version));
            strBuilder.AppendLine("  Entry point address:               " + ELFHeaderString.EntryPointAddress(e_entry));
            strBuilder.AppendLine("  Start of program headers:          " + ELFHeaderString.StartOfProgramHeaders(e_phoff));
            strBuilder.AppendLine("  Start of section headers:          " + ELFHeaderString.StartOfSectionHeaders(e_shoff));
            strBuilder.AppendLine("  Flags:                             " + ELFHeaderString.Flags(e_flags));
            strBuilder.AppendLine("  Size of this header:               " + ELFHeaderString.SizeOfThisHeader(e_ehsize));
            strBuilder.AppendLine("  Size of program headers:           " + ELFHeaderString.SizeOfProgramHeaders(e_phentsize));
            strBuilder.AppendLine("  Number of program headers:         " + ELFHeaderString.NumberOfProgramHeaders(e_phnum));
            strBuilder.AppendLine("  Size of section headers:           " + ELFHeaderString.SizeOfSectionHeaders(e_shentsize));
            strBuilder.AppendLine("  Number of section headers:         " + ELFHeaderString.NumberOfSectionHeaders(e_shnum));
            strBuilder.AppendLine("  Section header string table index: " + ELFHeaderString.SectionHeaderStringTableIndex(e_shstrndx));

            return strBuilder.ToString();
        }
    }

    public struct Elf32_Shdr
    {
        public Elf32_Word sh_name;
        public Elf32_Word sh_type;
        public Elf32_Word sh_flags;
        public Elf32_Addr sh_addr;
        public Elf32_Off sh_offset;
        public Elf32_Word sh_size;
        public Elf32_Word sh_link;
        public Elf32_Word sh_info;
        public Elf32_Word sh_addralign;
        public Elf32_Word sh_entsize;

        public Elf32_Shdr(byte[] data, Elf32_Addr startIndex, ValueRead read)
        {
            sh_name = read.Elf32_Word(data, startIndex + 0x00);
            sh_type = read.Elf32_Word(data, startIndex + 0x04);
            sh_flags = read.Elf32_Word(data, startIndex + 0x08);
            sh_addr = read.Elf32_Addr(data, startIndex + 0x0C);
            sh_offset = read.Elf32_Off(data, startIndex + 0x10);
            sh_size = read.Elf32_Word(data, startIndex + 0x14);
            sh_link = read.Elf32_Word(data, startIndex + 0x18);
            sh_info = read.Elf32_Word(data, startIndex + 0x1C);
            sh_addralign = read.Elf32_Word(data, startIndex + 0x20);
            sh_entsize = read.Elf32_Word(data, startIndex + 0x24);
        }

        public Elf32_Shdr(Elf32_Shdr shdr)
        {
            sh_name = shdr.sh_name;
            sh_type = shdr.sh_type;
            sh_flags = shdr.sh_flags;
            sh_addr = shdr.sh_addr;
            sh_offset = shdr.sh_offset;
            sh_size = shdr.sh_size;
            sh_link = shdr.sh_link;
            sh_info = shdr.sh_info;
            sh_addralign = shdr.sh_addralign;
            sh_entsize = shdr.sh_entsize;
        }

        public byte[] ToArray(byte dataEncoding)
        {
            byte[] array = new byte[0x28];

            if (dataEncoding == (byte)ED.LittleEndian)
            {
                array[0x00] = (byte)(sh_name & 0xFF);
                array[0x01] = (byte)((sh_name >> 8) & 0xFF);
                array[0x02] = (byte)((sh_name >> 16) & 0xFF);
                array[0x03] = (byte)(sh_name >> 24);
                array[0x04] = (byte)(sh_type & 0xFF);
                array[0x05] = (byte)((sh_type >> 8) & 0xFF);
                array[0x06] = (byte)((sh_type >> 16) & 0xFF);
                array[0x07] = (byte)(sh_type >> 24);
                array[0x08] = (byte)(sh_flags & 0xFF);
                array[0x09] = (byte)((sh_flags >> 8) & 0xFF);
                array[0x0A] = (byte)((sh_flags >> 16) & 0xFF);
                array[0x0B] = (byte)(sh_flags >> 24);
                array[0x0C] = (byte)(sh_addr & 0xFF);
                array[0x0D] = (byte)((sh_addr >> 8) & 0xFF);
                array[0x0E] = (byte)((sh_addr >> 16) & 0xFF);
                array[0x0F] = (byte)(sh_addr >> 24);
                array[0x10] = (byte)(sh_offset & 0xFF);
                array[0x11] = (byte)((sh_offset >> 8) & 0xFF);
                array[0x12] = (byte)((sh_offset >> 16) & 0xFF);
                array[0x13] = (byte)(sh_offset >> 24);
                array[0x14] = (byte)(sh_size & 0xFF);
                array[0x15] = (byte)((sh_size >> 8) & 0xFF);
                array[0x16] = (byte)((sh_size >> 16) & 0xFF);
                array[0x17] = (byte)(sh_size >> 24);
                array[0x18] = (byte)(sh_link & 0xFF);
                array[0x19] = (byte)((sh_link >> 8) & 0xFF);
                array[0x1A] = (byte)((sh_link >> 16) & 0xFF);
                array[0x1B] = (byte)(sh_link >> 24);
                array[0x1C] = (byte)(sh_info & 0xFF);
                array[0x1D] = (byte)((sh_info >> 8) & 0xFF);
                array[0x1E] = (byte)((sh_info >> 16) & 0xFF);
                array[0x1F] = (byte)(sh_info >> 24);
                array[0x20] = (byte)(sh_addralign & 0xFF);
                array[0x21] = (byte)((sh_addralign >> 8) & 0xFF);
                array[0x22] = (byte)((sh_addralign >> 16) & 0xFF);
                array[0x23] = (byte)(sh_addralign >> 24);
                array[0x24] = (byte)(sh_entsize & 0xFF);
                array[0x25] = (byte)((sh_entsize >> 8) & 0xFF);
                array[0x26] = (byte)((sh_entsize >> 16) & 0xFF);
                array[0x27] = (byte)(sh_entsize >> 24);
            }
            else if (dataEncoding == (byte)ED.BigEndian)
            {
                array[0x00] = (byte)(sh_name >> 24);
                array[0x01] = (byte)((sh_name >> 16) & 0xFF);
                array[0x02] = (byte)((sh_name >> 8) & 0xFF);
                array[0x03] = (byte)(sh_name & 0xFF);
                array[0x04] = (byte)(sh_type >> 24);
                array[0x05] = (byte)((sh_type >> 16) & 0xFF);
                array[0x06] = (byte)((sh_type >> 8) & 0xFF);
                array[0x07] = (byte)(sh_type & 0xFF);
                array[0x08] = (byte)(sh_flags >> 24);
                array[0x09] = (byte)((sh_flags >> 16) & 0xFF);
                array[0x0A] = (byte)((sh_flags >> 8) & 0xFF);
                array[0x0B] = (byte)(sh_flags & 0xFF);
                array[0x0C] = (byte)(sh_addr >> 24);
                array[0x0D] = (byte)((sh_addr >> 16) & 0xFF);
                array[0x0E] = (byte)((sh_addr >> 8) & 0xFF);
                array[0x0F] = (byte)(sh_addr & 0xFF);
                array[0x10] = (byte)(sh_offset >> 24);
                array[0x11] = (byte)((sh_offset >> 16) & 0xFF);
                array[0x12] = (byte)((sh_offset >> 8) & 0xFF);
                array[0x13] = (byte)(sh_offset & 0xFF);
                array[0x14] = (byte)(sh_size >> 24);
                array[0x15] = (byte)((sh_size >> 16) & 0xFF);
                array[0x16] = (byte)((sh_size >> 8) & 0xFF);
                array[0x17] = (byte)(sh_size & 0xFF);
                array[0x18] = (byte)(sh_link >> 24);
                array[0x19] = (byte)((sh_link >> 16) & 0xFF);
                array[0x1A] = (byte)((sh_link >> 8) & 0xFF);
                array[0x1B] = (byte)(sh_link & 0xFF);
                array[0x1C] = (byte)(sh_info >> 24);
                array[0x1D] = (byte)((sh_info >> 16) & 0xFF);
                array[0x1E] = (byte)((sh_info >> 8) & 0xFF);
                array[0x1F] = (byte)(sh_info & 0xFF);
                array[0x20] = (byte)(sh_addralign >> 24);
                array[0x21] = (byte)((sh_addralign >> 16) & 0xFF);
                array[0x22] = (byte)((sh_addralign >> 8) & 0xFF);
                array[0x23] = (byte)(sh_addralign & 0xFF);
                array[0x24] = (byte)(sh_entsize >> 24);
                array[0x25] = (byte)((sh_entsize >> 16) & 0xFF);
                array[0x26] = (byte)((sh_entsize >> 8) & 0xFF);
                array[0x27] = (byte)(sh_entsize & 0xFF);
            }

            return array;
        }

        public override string ToString()
        {
            StringBuilder strBuilder = new StringBuilder();

            //strBuilder.Append(SectionHeaderString.Name(sh_name) + " ");
            strBuilder.Append(SectionHeaderString.Type(sh_type) + " ");
            strBuilder.Append(SectionHeaderString.Address(sh_addr) + " ");
            strBuilder.Append(SectionHeaderString.Offset(sh_offset) + " ");
            strBuilder.Append(SectionHeaderString.Size(sh_size) + " ");
            strBuilder.Append(SectionHeaderString.EntriesSize(sh_entsize) + " ");
            strBuilder.Append(SectionHeaderString.Flags(sh_flags) + " ");
            strBuilder.Append(SectionHeaderString.Link(sh_link) + " ");
            strBuilder.Append(SectionHeaderString.Info(sh_info) + " ");
            strBuilder.Append(SectionHeaderString.Alignment(sh_addralign));

            return strBuilder.ToString();
        }

        public static int CompareByOffset(Elf32_Shdr sh1, Elf32_Shdr sh2)
        {
            return sh1.sh_offset.CompareTo(sh2.sh_offset);
        }
    }

    public struct Elf64_Shdr
    {
        public Elf64_Word sh_name;
        public Elf64_Word sh_type;
        public Elf64_Xword sh_flags;
        public Elf64_Addr sh_addr;
        public Elf64_Off sh_offset;
        public Elf64_Xword sh_size;
        public Elf64_Word sh_link;
        public Elf64_Word sh_info;
        public Elf64_Xword sh_addralign;
        public Elf64_Xword sh_entsize;

        public Elf64_Shdr(byte[] data, Elf64_Addr startIndex, ValueRead read)
        {
            sh_name = read.Elf64_Word(data, startIndex + 0x00);
            sh_type = read.Elf64_Word(data, startIndex + 0x04);
            sh_flags = read.Elf64_Xword(data, startIndex + 0x08);
            sh_addr = read.Elf64_Addr(data, startIndex + 0x10);
            sh_offset = read.Elf64_Off(data, startIndex + 0x18);
            sh_size = read.Elf64_Xword(data, startIndex + 0x20);
            sh_link = read.Elf64_Word(data, startIndex + 0x28);
            sh_info = read.Elf64_Word(data, startIndex + 0x2C);
            sh_addralign = read.Elf64_Xword(data, startIndex + 0x30);
            sh_entsize = read.Elf64_Xword(data, startIndex + 0x38);
        }

        public override string ToString()
        {
            StringBuilder strBuilder = new StringBuilder();

            //strBuilder.Append(SectionHeaderString.Name(sh_name) + " ");
            strBuilder.Append(SectionHeaderString.Type(sh_type) + " ");
            strBuilder.Append(SectionHeaderString.Address(sh_addr) + " ");
            strBuilder.Append(SectionHeaderString.Offset(sh_offset) + " ");
            strBuilder.Append(SectionHeaderString.Size(sh_size) + " ");
            strBuilder.Append(SectionHeaderString.EntriesSize(sh_entsize) + " ");
            strBuilder.Append(SectionHeaderString.Flags(sh_flags) + " ");
            strBuilder.Append(SectionHeaderString.Link(sh_link) + " ");
            strBuilder.Append(SectionHeaderString.Info(sh_info) + " ");
            strBuilder.Append(SectionHeaderString.Alignment(sh_addralign));

            return strBuilder.ToString();
        }
    }

    public static class ELFHeaderString
    {
        public static string Magic(byte[] e_ident)
        {
            StringBuilder strBuilder = new StringBuilder();
            foreach (byte b in e_ident)
                strBuilder.Append(" " + b.ToString("X2"));
            return strBuilder.ToString();
        }

        public static string Class(byte[] e_ident)
        {
            if (Enum.IsDefined(typeof(EC), e_ident[(byte)EI.FileClass]))
                return ((EC)e_ident[(byte)EI.FileClass]).ToString();
            else
                return "Invalid";
        }

        public static string Data(byte[] e_ident)
        {
            if (Enum.IsDefined(typeof(ED), e_ident[(byte)EI.DataEncoding]))
                return ((ED)e_ident[(byte)EI.DataEncoding]).ToString();
            else
                return "Invalid";
        }

        public static string HeaderVersion(byte[] e_ident)
        {
            return e_ident[(byte)EI.FileVersion].ToString();
        }

        public static string OSABI(byte[] e_ident)
        {
            if (Enum.IsDefined(typeof(EO), e_ident[(byte)EI.Target_OS_ABI]))
                return ((EO)e_ident[(byte)EI.Target_OS_ABI]).ToString();
            else
                return e_ident[(byte)EI.Target_OS_ABI].ToString();
        }

        public static string ABIVersion(byte[] e_ident)
        {
            return e_ident[(byte)EI.ABIVersion].ToString();
        }

        public static string Type(ushort e_type)
        {
            if (Enum.IsDefined(typeof(ET), e_type))
                return ((ET)e_type).ToString();
            else
                return "0x" + e_type.ToString("X4");
        }

        public static string Machine(ushort e_machine)
        {
            if (Enum.IsDefined(typeof(Machine), e_machine))
                return ((Machine)e_machine).ToString();
            else
                return "0x" + e_machine.ToString("X");
        }

        public static string FileVersion(uint e_version)
        {
            return "0x" + e_version.ToString("X");
        }

        public static string EntryPointAddress(ulong e_entry)
        {
            return "0x" + e_entry.ToString("X");
        }

        public static string StartOfProgramHeaders(ulong e_phoff)
        {
            return e_phoff.ToString() + " (bytes into file)";
        }

        public static string StartOfSectionHeaders(ulong e_shoff)
        {
            return e_shoff.ToString() + " (bytes into file)";
        }

        public static string Flags(uint e_flags)
        {
            return "0x" + e_flags.ToString("X8");
        }

        public static string SizeOfThisHeader(ushort e_ehsize)
        {
            return e_ehsize.ToString() + " (bytes)";
        }

        public static string SizeOfProgramHeaders(ushort e_phentsize)
        {
            return e_phentsize.ToString() + " (bytes)";
        }

        public static string NumberOfProgramHeaders(ushort e_phnum)
        {
            return e_phnum.ToString();
        }

        public static string SizeOfSectionHeaders(ushort e_shentsize)
        {
            return e_shentsize.ToString() + " (bytes)";
        }

        public static string NumberOfSectionHeaders(ushort e_shnum)
        {
            return e_shnum.ToString();
        }

        public static string SectionHeaderStringTableIndex(ushort e_shstrndx)
        {
            return e_shstrndx.ToString();
        }
    }

    public static class SectionHeaderString
    {
        public static string Name(uint sh_name)
        {
            return sh_name.ToString("X8");
        }

        public static string Type(uint sh_type)
        {
            if (sh_type >= (uint)SHT.LOOS && sh_type <= (uint)SHT.HIOS)
                return "OS".PadRight(13, ' ');
            else if (sh_type >= (uint)SHT.LOPROC && sh_type <= (uint)SHT.HIPROC)
                return "PROC".PadRight(13, ' ');
            else if (sh_type >= (uint)SHT.LOUSER && sh_type <= (uint)SHT.HIUSER)
                return "USER".PadRight(13, ' ');
            else if (Enum.IsDefined(typeof(SHT), sh_type))
                return ((SHT)sh_type).ToString().PadRight(13, ' ');
            else
                return sh_type.ToString("X").PadLeft(13, ' ');
        }

        public static string Address(ulong sh_addr)
        {
            return sh_addr.ToString("X").PadLeft(8, ' ');
        }

        public static string Offset(ulong sh_offset)
        {
            return sh_offset.ToString("X").PadLeft(8, ' ');
        }

        public static string Size(ulong sh_size)
        {
            return sh_size.ToString("X").PadLeft(8, ' ');
        }

        public static string EntriesSize(ulong sh_entsize)
        {
            return sh_entsize.ToString("X").PadLeft(7, ' ');
        }

        public static string Flags(ulong sh_flags)
        {
            StringBuilder strBuilder = new StringBuilder();

            if ((sh_flags & (uint)SHF.WRITE) == (uint)SHF.WRITE)
                strBuilder.Append("W");

            if ((sh_flags & (uint)SHF.ALLOC) == (uint)SHF.ALLOC)
                strBuilder.Append("A");

            if ((sh_flags & (uint)SHF.EXECINSTR) == (uint)SHF.EXECINSTR)
                strBuilder.Append("X");

            if ((sh_flags & 0x8) == 0x8)
                strBuilder.Append("x");

            if ((sh_flags & (uint)SHF.MERGE) == (uint)SHF.MERGE)
                strBuilder.Append("M");

            if ((sh_flags & (uint)SHF.STRINGS) == (uint)SHF.STRINGS)
                strBuilder.Append("S");

            if ((sh_flags & (uint)SHF.INFO_LINK) == (uint)SHF.INFO_LINK)
                strBuilder.Append("I");

            if ((sh_flags & (uint)SHF.LINK_ORDER) == (uint)SHF.LINK_ORDER)
                strBuilder.Append("L");

            if ((sh_flags & (uint)SHF.OS_NONCONFORMING) == (uint)SHF.OS_NONCONFORMING)
                strBuilder.Append("O");

            if ((sh_flags & (uint)SHF.GROUP) == (uint)SHF.GROUP)
                strBuilder.Append("G");

            if ((sh_flags & 0x400) == 0x400)
                strBuilder.Append("T");

            if ((sh_flags & 0x800) == 0x800)
                strBuilder.Append("x");

            if ((sh_flags & 0x1000) == 0x1000)
                strBuilder.Append("x");

            if ((sh_flags & 0x2000) == 0x2000)
                strBuilder.Append("x");

            if ((sh_flags & 0x4000) == 0x4000)
                strBuilder.Append("x");

            if ((sh_flags & 0x8000) == 0x8000)
                strBuilder.Append("x");

            if ((sh_flags & 0x1000) == 0x10000)
                strBuilder.Append("x");

            if ((sh_flags & 0x2000) == 0x20000)
                strBuilder.Append("x");

            if ((sh_flags & 0x4000) == 0x40000)
                strBuilder.Append("x");

            if ((sh_flags & 0x8000) == 0x80000)
                strBuilder.Append("x");

            if ((sh_flags & (uint)SHF.MASKOS) == (uint)SHF.MASKOS)
                strBuilder.Append("o");

            if ((sh_flags & (uint)SHF.MASKPROC) == (uint)SHF.MASKPROC)
                strBuilder.Append("p");

            return strBuilder.ToString().PadLeft(5, ' ');
        }

        public static string Link(uint sh_link)
        {
            return sh_link.ToString("X").PadLeft(4, ' ');
        }

        public static string Info(uint sh_info)
        {
            return sh_info.ToString("X").PadLeft(4, ' ');
        }

        public static string Alignment(ulong sh_addralign)
        {
            return sh_addralign.ToString("X").PadLeft(5, ' ');
        }
    }

    public abstract class ELF
    {
        public static ELF Open(string filename)
        {
            ELF file = null;

            try
            {
                file = new RPXNES(filename);
                return file;
            }
            catch
            {
            }

            try
            {
                file = new RPXSNES(filename);
                return file;
            }
            catch
            {
            }

            try
            {
                file = new RPX(filename);
                return file;
            }
            catch
            {
            }

            try
            {
                file = new ELF32(filename);
                return file;
            }
            catch
            {
            }

            try
            {
                file = new ELF64(filename);
                return file;
            }
            catch
            {
            }

            return file;
        }
    }
}
