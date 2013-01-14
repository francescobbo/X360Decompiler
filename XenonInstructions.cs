using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace X360Decompiler
{
    public class XenonInstructions
    {
        public const int OP_UNKNOWN = 0x1;
        public const int OP_ENDBLOCK = 0x2;
        public const int OP_SUBTABLE = 0x4;

        static private State decompiler;
        public XenonInstructions(State state)
        {
            decompiler = state;
        }

        public enum Mnemonics
        {
	        /* This OpCode was unused on the Xenon. We can use it to jump to HLE functions */
	        PPC_OP_HLE_CALL = 1,
	        PPC_OP_TDI,
	        PPC_OP_TWI,
	        PPC_OP_MULLI,
	        PPC_OP_SUBFIC,
	        PPC_OP_CMPLI,
	        PPC_OP_CMPI,
	        PPC_OP_ADDIC,
	        PPC_OP_ADDIC_UP,
	        PPC_OP_ADDI,
	        PPC_OP_ADDIS,
	        PPC_OP_BC,
	        PPC_OP_SC,
	        PPC_OP_B,
	        PPC_OP_RLWIMI,
	        PPC_OP_RLWINM,
	        PPC_OP_RLWNM,
	        PPC_OP_ORI,
	        PPC_OP_ORIS,
	        PPC_OP_XORI,
	        PPC_OP_XORIS,
	        PPC_OP_ANDI_UP,
	        PPC_OP_ANDIS_UP,
	        PPC_OP_LWZ,
	        PPC_OP_LWZU,
	        PPC_OP_LBZ,
	        PPC_OP_LBZU,
	        PPC_OP_STW,
	        PPC_OP_STWU,
	        PPC_OP_STB,
	        PPC_OP_STBU,
	        PPC_OP_LHZ,
	        PPC_OP_LHZU,
	        PPC_OP_LHA,
	        PPC_OP_LHAU,
	        PPC_OP_STH,
	        PPC_OP_STHU,
	        PPC_OP_LMW,
	        PPC_OP_STMW,
	        PPC_OP_LFS,
	        PPC_OP_LFSU,
	        PPC_OP_LFD,
	        PPC_OP_LFDU,
	        PPC_OP_STFS,
	        PPC_OP_STFSU,
	        PPC_OP_STFD,
	        PPC_OP_STFDU,
	        PPC_OP_STD,

	        /* Opcode 19 sub operations */
	        PPC_OP_BCCTR,
	        PPC_OP_BCLR,
	        PPC_OP_CRAND,
	        PPC_OP_CRANDC,
	        PPC_OP_CREQV,
	        PPC_OP_CRNAND,
	        PPC_OP_CRNOR,
	        PPC_OP_CROR,
	        PPC_OP_CRORC,
	        PPC_OP_CRXOR,
	        PPC_OP_ISYNC,
	        PPC_OP_MCRF,
	        PPC_OP_RFID,

	        /* Opcode 30 sub operations */
	        PPC_OP_RLDCL,
	        PPC_OP_RLDCR,
	        PPC_OP_RLDIC,
	        PPC_OP_RLDICL,
	        PPC_OP_RLDICR,
	        PPC_OP_RLDIMI,

	        /* Opcode 31 sub operations */
	        PPC_OP_ADD,
	        PPC_OP_ADDC,
	        PPC_OP_ADDE,
	        PPC_OP_ADDME,
	        PPC_OP_ADDZE,
	        PPC_OP_AND,
	        PPC_OP_ANDC,
	        PPC_OP_CMP,
	        PPC_OP_CMPL,
	        PPC_OP_CNTLZD,
	        PPC_OP_CNTLZW,
	        PPC_OP_DCBA,
	        PPC_OP_DCBF,
	        PPC_OP_DCBI,
	        PPC_OP_DCBST,
	        PPC_OP_DCBT,
	        PPC_OP_DCBTST,
	        PPC_OP_DCBZ,
	        PPC_OP_DIVD,
	        PPC_OP_DIVDU,
	        PPC_OP_DIVW,
	        PPC_OP_DIVWU,
	        PPC_OP_ECIWX,
	        PPC_OP_ECOWX,
	        PPC_OP_EIEIO,
	        PPC_OP_EQV,
	        PPC_OP_EXTSB,
	        PPC_OP_EXTSH,
	        PPC_OP_EXTSW,
	        PPC_OP_ICBI,
	        PPC_OP_LBZUX,
	        PPC_OP_LBZX,
	        PPC_OP_LDARX,
	        PPC_OP_LDUX,
	        PPC_OP_LDX,
	        PPC_OP_LFDUX,
	        PPC_OP_LFDX,
	        PPC_OP_LFSUX,
	        PPC_OP_LFSX,
	        PPC_OP_LHAUX,
	        PPC_OP_LHAX,
	        PPC_OP_LHBRX,
	        PPC_OP_LHZUX,
	        PPC_OP_LHZX,
	        PPC_OP_LSWI,
	        PPC_OP_LSWX,
	        PPC_OP_LWARX,
	        PPC_OP_LWAUX,
	        PPC_OP_LWAX,
	        PPC_OP_LWBRX,
	        PPC_OP_LWZUX,
	        PPC_OP_LWZX,
	        PPC_OP_MCRXR,
	        PPC_OP_MFCR,
	        PPC_OP_MFMSR,
	        PPC_OP_MFSPR,
	        PPC_OP_MFSR,
	        PPC_OP_MFSRIN,
	        PPC_OP_MFTB,
	        PPC_OP_MTCRF,
	        PPC_OP_MTMSR,
	        PPC_OP_MTMSRD,
	        PPC_OP_MTSPR,
	        PPC_OP_MTSR,
	        PPC_OP_MTSRIN,
	        PPC_OP_MULHD,
	        PPC_OP_MULHDU,
	        PPC_OP_MULHW,
	        PPC_OP_MULHWU,
	        PPC_OP_MULLD,
	        PPC_OP_MULLW,
	        PPC_OP_NAND,
	        PPC_OP_NEG,
	        PPC_OP_NOR,
	        PPC_OP_OR,
	        PPC_OP_ORC,
	        PPC_OP_SLBIA,
	        PPC_OP_SLBIE,
	        PPC_OP_SLBMFEE,
	        PPC_OP_SLBMFEV,
	        PPC_OP_SLBMTE,
	        PPC_OP_SLD,
	        PPC_OP_SLW,
	        PPC_OP_SRAD,
	        PPC_OP_SRADI,
	        PPC_OP_SRADI1,
	        PPC_OP_SRAW,
	        PPC_OP_SRAWI,
	        PPC_OP_SRD,
	        PPC_OP_SRW,
	        PPC_OP_STBUX,
	        PPC_OP_STBX,
	        PPC_OP_STDCX_UP,
	        PPC_OP_STDUX,
	        PPC_OP_STDX,
	        PPC_OP_STFDUX,
	        PPC_OP_STFDX,
	        PPC_OP_STFIWX,
	        PPC_OP_STFSUX,
	        PPC_OP_STFSX,
	        PPC_OP_STHBRX,
	        PPC_OP_STHUX,
	        PPC_OP_STHX,
	        PPC_OP_STSWI,
	        PPC_OP_STSWX,
	        PPC_OP_STWBRX,
	        PPC_OP_STWCX_UP,
	        PPC_OP_STWUX,
	        PPC_OP_STWX,
	        PPC_OP_SUBF,
	        PPC_OP_SUBFC,
	        PPC_OP_SUBFE,
	        PPC_OP_SUBFME,
	        PPC_OP_SUBFZE,
	        PPC_OP_SYNC,
	        PPC_OP_TD,
	        PPC_OP_TLBIA,
	        PPC_OP_TLBIE,
	        PPC_OP_TLBIEL,
	        PPC_OP_TLBSYNC,
	        PPC_OP_TW,
	        PPC_OP_XOR,

	        /* Opcode 58 sub operations */
	        PPC_OP_LD,
	        PPC_OP_LDU,
	        PPC_OP_LWA,

	        /* Opcode 59 sub operations */
	        PPC_OP_FADDS,
	        PPC_OP_FDIVS,
	        PPC_OP_FMADDS,
	        PPC_OP_FMSUBS,
	        PPC_OP_FMULS,
	        PPC_OP_FNMADDS,
	        PPC_OP_FNMSUBS,
	        PPC_OP_FRES,
	        PPC_OP_FSQRTS,
	        PPC_OP_FSUBS,
	        /* 202 is used up */

	        /* Opcode 63 sub operations */
	        PPC_OP_FABS,
	        PPC_OP_FADD,
	        PPC_OP_FCFID,
	        PPC_OP_FCMPO,
	        PPC_OP_FCMPU,
	        PPC_OP_FCTID,
	        PPC_OP_FCTIDZ,
	        PPC_OP_FCTIW,
	        PPC_OP_FCTIWZ,
	        PPC_OP_FDIV,
	        PPC_OP_FMADD,
	        PPC_OP_FMR,
	        PPC_OP_FMSUB,
	        PPC_OP_FMUL,
	        PPC_OP_FNABS,
	        PPC_OP_FNEG,
	        PPC_OP_FNMADD,
	        PPC_OP_FNMSUB,
	        PPC_OP_FRSP,
	        PPC_OP_FRSQRTE,
	        PPC_OP_FSEL,
	        PPC_OP_FSQRT,
	        PPC_OP_FSUB,
	        PPC_OP_MCRFS,
	        PPC_OP_MFFS,
	        PPC_OP_MTFSB0,
	        PPC_OP_MTFSB1,
	        PPC_OP_MTFSF,
	        PPC_OP_MTFSFI,

	        /* Update this if you add new instructions */
	        PPC_OP_INVALID,
	        PPC_OP_LAST
        };

        public struct OpCodeInfo
        {
            public OpCodeInfo(int opcode, Mnemonics mnemonic, String name, int flags)
            {
                OpCode = opcode;
                Id = mnemonic;
                Name = name;
                Flags = flags;
                CEquivalent = null;
            }

            public OpCodeInfo(int opcode, Mnemonics mnemonic, String name, int flags, CTranslator translator)
            {
                OpCode = opcode;
                Id = mnemonic;
                Name = name;
                Flags = flags;
                CEquivalent = translator;
            }

	        public int OpCode;
	        public Mnemonics Id;
	        public String Name;
	        public int Flags;
            
            public delegate List<CStatement> CTranslator(uint pc, uint instruction);
            public CTranslator CEquivalent;
        }

        OpCodeInfo InvalidOp = new OpCodeInfo(0, Mnemonics.PPC_OP_INVALID, "InvInstr", OP_UNKNOWN);

        OpCodeInfo[] BaseTable = new OpCodeInfo[]
        {
	        new OpCodeInfo(4,   Mnemonics.PPC_OP_INVALID, "", OP_SUBTABLE | (0 << 24)),
	        new OpCodeInfo(19,  Mnemonics.PPC_OP_INVALID, "", OP_SUBTABLE | (1 << 24)),
	        new OpCodeInfo(30,  Mnemonics.PPC_OP_INVALID, "", OP_SUBTABLE | (2 << 24)),
	        new OpCodeInfo(31,  Mnemonics.PPC_OP_INVALID, "", OP_SUBTABLE | (3 << 24)),
	        new OpCodeInfo(58,  Mnemonics.PPC_OP_INVALID, "", OP_SUBTABLE | (4 << 24)),
	        new OpCodeInfo(59,  Mnemonics.PPC_OP_INVALID, "", OP_SUBTABLE | (5 << 24)),
	        new OpCodeInfo(63,  Mnemonics.PPC_OP_INVALID, "", OP_SUBTABLE | (6 << 24)),

	        new OpCodeInfo(1,   Mnemonics.PPC_OP_HLE_CALL, "HLECall", OP_ENDBLOCK),
	        new OpCodeInfo(3,   Mnemonics.PPC_OP_TWI, "twi", OP_ENDBLOCK, Twi_C),
	        new OpCodeInfo(7,   Mnemonics.PPC_OP_MULLI, "mulli", 0),
	        new OpCodeInfo(8,   Mnemonics.PPC_OP_SUBFIC, "subfic", 0),

	        new OpCodeInfo(10,  Mnemonics.PPC_OP_CMPLI, "cmpli", 0, Cmpli_C),
	        new OpCodeInfo(11,  Mnemonics.PPC_OP_CMPI, "cmpi", 0, Cmpi_C),
	        new OpCodeInfo(12,  Mnemonics.PPC_OP_ADDIC, "addic", 0),
	        new OpCodeInfo(13,  Mnemonics.PPC_OP_ADDIC_UP, "addic.", 0),
	        new OpCodeInfo(14,  Mnemonics.PPC_OP_ADDI, "addi", 0, Addi_C),
	        new OpCodeInfo(15,  Mnemonics.PPC_OP_ADDIS, "addis", 0, Addis_C),
	        new OpCodeInfo(16,  Mnemonics.PPC_OP_BC, "bc", OP_ENDBLOCK, Bc_C),
	        new OpCodeInfo(18,  Mnemonics.PPC_OP_B, "b", OP_ENDBLOCK, B_C),
	        new OpCodeInfo(17,  Mnemonics.PPC_OP_SC, "sc", OP_ENDBLOCK),

	        new OpCodeInfo(20,  Mnemonics.PPC_OP_RLWIMI, "rlwimi", 0, Rlwimi_C),
	        new OpCodeInfo(21,  Mnemonics.PPC_OP_RLWINM, "rlwinm", 0, Rlwinm_C),
	        new OpCodeInfo(23,  Mnemonics.PPC_OP_RLWNM, "rlwnm", 0),
	        new OpCodeInfo(24,  Mnemonics.PPC_OP_ORI, "ori", 0, Ori_C),
	        new OpCodeInfo(25,  Mnemonics.PPC_OP_ORIS, "oris", 0),
	        new OpCodeInfo(26,  Mnemonics.PPC_OP_XORI, "xori", 0),
	        new OpCodeInfo(27,  Mnemonics.PPC_OP_XORIS, "xoris", 0),
	        new OpCodeInfo(28,  Mnemonics.PPC_OP_ANDI_UP, "andi_rc", 0),
	        new OpCodeInfo(29,  Mnemonics.PPC_OP_ANDIS_UP, "andis_rc", 0),

	        new OpCodeInfo(32,  Mnemonics.PPC_OP_LWZ, "lwz", 0, Lwz_C),
	        new OpCodeInfo(33,  Mnemonics.PPC_OP_LWZU, "lwzu", 0),
	        new OpCodeInfo(34,  Mnemonics.PPC_OP_LBZ, "lbz", 0, Lbz_C),
	        new OpCodeInfo(35,  Mnemonics.PPC_OP_LBZU, "lbzu", 0),
	        new OpCodeInfo(36,  Mnemonics.PPC_OP_STW, "stw", 0, Stw_C),
	        new OpCodeInfo(37,  Mnemonics.PPC_OP_STWU, "stwu", 0, Stwu_C),
	        new OpCodeInfo(38,  Mnemonics.PPC_OP_STB, "stb", 0, Stb_C),
	        new OpCodeInfo(39,  Mnemonics.PPC_OP_STBU, "stbu", 0),

	        new OpCodeInfo(40,  Mnemonics.PPC_OP_LHZ, "lhz", 0, Lhz_C),
	        new OpCodeInfo(41,  Mnemonics.PPC_OP_LHZU, "lhzu", 0),
	        new OpCodeInfo(42,  Mnemonics.PPC_OP_LHA, "lha", 0),
	        new OpCodeInfo(43,  Mnemonics.PPC_OP_LHAU, "lhau", 0),
	        new OpCodeInfo(44,  Mnemonics.PPC_OP_STH, "sth", 0, Sth_C),
	        new OpCodeInfo(45,  Mnemonics.PPC_OP_STHU, "sthu", 0),
	        new OpCodeInfo(46,  Mnemonics.PPC_OP_LMW, "lmw", 0),
	        new OpCodeInfo(47,  Mnemonics.PPC_OP_STMW, "stmw", 0),
	        new OpCodeInfo(48,  Mnemonics.PPC_OP_LFS, "lfs", 0),
	        new OpCodeInfo(49,  Mnemonics.PPC_OP_LFSU, "lfsu", 0),

	        new OpCodeInfo(50,  Mnemonics.PPC_OP_LFD, "lfd", 0),
	        new OpCodeInfo(51,  Mnemonics.PPC_OP_LFDU, "lfdu", 0),
	        new OpCodeInfo(52,  Mnemonics.PPC_OP_STFS, "stfs", 0),
	        new OpCodeInfo(53,  Mnemonics.PPC_OP_STFSU, "stfsu", 0),
	        new OpCodeInfo(54,  Mnemonics.PPC_OP_STFD, "stfd", 0),
	        new OpCodeInfo(55,  Mnemonics.PPC_OP_STFDU, "stfdu", 0),

	        new OpCodeInfo(62,  Mnemonics.PPC_OP_STD, "std", 0, Std_C),

	        /* Unused opcodes */
	        new OpCodeInfo(0,   Mnemonics.PPC_OP_INVALID, "InvInstr", OP_UNKNOWN),
	        new OpCodeInfo(2,   Mnemonics.PPC_OP_INVALID, "InvInstr", OP_UNKNOWN),
	        new OpCodeInfo(5,   Mnemonics.PPC_OP_INVALID, "InvInstr", OP_UNKNOWN),
	        new OpCodeInfo(6,   Mnemonics.PPC_OP_INVALID, "InvInstr", OP_UNKNOWN),
	        new OpCodeInfo(9,   Mnemonics.PPC_OP_INVALID, "InvInstr", OP_UNKNOWN),
	        new OpCodeInfo(22,  Mnemonics.PPC_OP_INVALID, "InvInstr", OP_UNKNOWN),
	        new OpCodeInfo(56,  Mnemonics.PPC_OP_INVALID, "InvInstr", OP_UNKNOWN),
	        new OpCodeInfo(57,  Mnemonics.PPC_OP_INVALID, "InvInstr", OP_UNKNOWN),
	        new OpCodeInfo(60,  Mnemonics.PPC_OP_INVALID, "InvInstr", OP_UNKNOWN),
	        new OpCodeInfo(61,  Mnemonics.PPC_OP_INVALID, "InvInstr", OP_UNKNOWN),
        };

        /* VMX128 specific table */
        OpCodeInfo[] Table4 = new OpCodeInfo[]
        {
	        new OpCodeInfo(0,   0, "", OP_UNKNOWN)
        };

        OpCodeInfo[] Table19 = new OpCodeInfo[]
        {
	        new OpCodeInfo(0,   Mnemonics.PPC_OP_MCRF, "mcrf", 0),
	        new OpCodeInfo(16,  Mnemonics.PPC_OP_BCLR, "bclr", OP_ENDBLOCK, Bclr_C),
	        new OpCodeInfo(18,  Mnemonics.PPC_OP_RFID, "rfid", OP_ENDBLOCK),
	        new OpCodeInfo(33,  Mnemonics.PPC_OP_CRNOR, "crnor", 0),
	        new OpCodeInfo(129, Mnemonics.PPC_OP_CRANDC, "crandc", 0),
	        new OpCodeInfo(150, Mnemonics.PPC_OP_ISYNC, "isync", 0),
	        new OpCodeInfo(193, Mnemonics.PPC_OP_CRXOR, "crxor", 0),
	        new OpCodeInfo(225, Mnemonics.PPC_OP_CRNAND, "crnand", 0),
	        new OpCodeInfo(257, Mnemonics.PPC_OP_CRAND, "crand", 0),
	        new OpCodeInfo(289, Mnemonics.PPC_OP_CREQV, "creqv", 0),
	        new OpCodeInfo(417, Mnemonics.PPC_OP_CRORC, "crorc", 0),
	        new OpCodeInfo(449, Mnemonics.PPC_OP_CROR, "cror", 0),
	        new OpCodeInfo(528, Mnemonics.PPC_OP_BCCTR, "bcctr", OP_ENDBLOCK, Bcctr_C),
        };

        OpCodeInfo[] Table30 = new OpCodeInfo[]
        {
	        /* The last bit has a special meaning in the first 4 pairs. Each pair maps to the same OpCode */
	        new OpCodeInfo(0,   Mnemonics.PPC_OP_RLDICL, "rldicl", 0),
	        new OpCodeInfo(1,   Mnemonics.PPC_OP_RLDICL, "rldicl", 0),
	        new OpCodeInfo(2,   Mnemonics.PPC_OP_RLDICR, "rldicr", 0),
	        new OpCodeInfo(3,   Mnemonics.PPC_OP_RLDICR, "rldicr", 0),
	        new OpCodeInfo(4,   Mnemonics.PPC_OP_RLDIC, "rldic", 0),
	        new OpCodeInfo(5,   Mnemonics.PPC_OP_RLDIC, "rldic", 0),
	        new OpCodeInfo(6,   Mnemonics.PPC_OP_RLDIMI, "rldimi", 0),
	        new OpCodeInfo(7,   Mnemonics.PPC_OP_RLDIMI, "rldimi", 0),

	        new OpCodeInfo(8,   Mnemonics.PPC_OP_RLDCL, "rldcl", 0),
	        new OpCodeInfo(9,   Mnemonics.PPC_OP_RLDCR, "rldcr", 0)
        };

        OpCodeInfo[] Table31 = new OpCodeInfo[]
        {
	        new OpCodeInfo(0,   Mnemonics.PPC_OP_CMP, "cmp", 0, Cmp_C),
	        new OpCodeInfo(4,   Mnemonics.PPC_OP_TW, "tw", OP_ENDBLOCK),
	        new OpCodeInfo(9,   Mnemonics.PPC_OP_MULHDU, "mulhdu", 0),
	        new OpCodeInfo(19,  Mnemonics.PPC_OP_MFCR, "mfcr", 0),
	        new OpCodeInfo(20,  Mnemonics.PPC_OP_LWARX, "lwarx", 0),
	        new OpCodeInfo(21,  Mnemonics.PPC_OP_LDX, "ldx", 0),
	        new OpCodeInfo(23,  Mnemonics.PPC_OP_LWZX, "lwzx", 0),
	        new OpCodeInfo(24,  Mnemonics.PPC_OP_SLW, "slw", 0),
	        new OpCodeInfo(26,  Mnemonics.PPC_OP_CNTLZW, "cntlzwx", 0),
	        new OpCodeInfo(27,  Mnemonics.PPC_OP_SLD, "sld", 0),
	        new OpCodeInfo(28,  Mnemonics.PPC_OP_AND, "and", 0, And_C),
	        new OpCodeInfo(32,  Mnemonics.PPC_OP_CMPL, "cmpl", 0, Cmpl_C),
	        new OpCodeInfo(53,  Mnemonics.PPC_OP_LDUX, "ldux", 0),
	        new OpCodeInfo(54,  Mnemonics.PPC_OP_DCBST, "dcbst", 0),
	        new OpCodeInfo(55,  Mnemonics.PPC_OP_LWZUX, "lwzux", 0),
	        new OpCodeInfo(58,  Mnemonics.PPC_OP_CNTLZD, "cntlzd", 0),
	        new OpCodeInfo(60,  Mnemonics.PPC_OP_ANDC, "andc", 0, Andc_C),
	        new OpCodeInfo(68,  Mnemonics.PPC_OP_TD, "td", OP_ENDBLOCK),
	        new OpCodeInfo(73,  Mnemonics.PPC_OP_MULHD, "mulhd", 0),
	        new OpCodeInfo(83,  Mnemonics.PPC_OP_MFMSR, "mfmsr", 0),
	        new OpCodeInfo(84,  Mnemonics.PPC_OP_LDARX, "ldarx", 0),
	        new OpCodeInfo(86,  Mnemonics.PPC_OP_DCBF, "dcbf", 0),
	        new OpCodeInfo(87,  Mnemonics.PPC_OP_LBZX, "lbzx", 0, Lbzx_C),

	        new OpCodeInfo(119, Mnemonics.PPC_OP_LBZUX, "lbzux", 0),
	        new OpCodeInfo(124, Mnemonics.PPC_OP_NOR, "nor", 0),
	        new OpCodeInfo(144, Mnemonics.PPC_OP_MTCRF, "mtcrf", 0),
	        new OpCodeInfo(146, Mnemonics.PPC_OP_MTMSR, "mtmsr", 0),
	        new OpCodeInfo(149, Mnemonics.PPC_OP_STDX, "stdx", 0),
	        new OpCodeInfo(150, Mnemonics.PPC_OP_STWCX_UP, "stwcx.", 0),
	        new OpCodeInfo(151, Mnemonics.PPC_OP_STWX, "stwx", 0),
	        new OpCodeInfo(178, Mnemonics.PPC_OP_MTMSRD, "mtmsrd", 0),
	        new OpCodeInfo(181, Mnemonics.PPC_OP_STDUX, "stdux", 0),
	        new OpCodeInfo(183, Mnemonics.PPC_OP_STWUX, "stwux", 0),

	        new OpCodeInfo(210, Mnemonics.PPC_OP_MTSR, "mtsr", 0),
	        new OpCodeInfo(214, Mnemonics.PPC_OP_STDCX_UP, "stdcx.", 0),
	        new OpCodeInfo(215, Mnemonics.PPC_OP_STBX, "stbx", 0),
	        new OpCodeInfo(242, Mnemonics.PPC_OP_MTSRIN, "mtsrin", 0),
	        new OpCodeInfo(246, Mnemonics.PPC_OP_DCBTST, "dcbtst", 0),
	        new OpCodeInfo(247, Mnemonics.PPC_OP_STBUX, "stbux", 0),
	        new OpCodeInfo(274, Mnemonics.PPC_OP_TLBIEL, "tlbiel", 0),
	        new OpCodeInfo(278, Mnemonics.PPC_OP_DCBT, "dcbt", 0),
	        new OpCodeInfo(279, Mnemonics.PPC_OP_LHZX, "lhzx", 0),
	        new OpCodeInfo(284, Mnemonics.PPC_OP_EQV, "eqv", 0),

	        new OpCodeInfo(306, Mnemonics.PPC_OP_TLBIE, "tlbie", 0),
	        new OpCodeInfo(310, Mnemonics.PPC_OP_ECIWX, "eciwx", 0),
	        new OpCodeInfo(311, Mnemonics.PPC_OP_LHZUX, "lhzux", 0),
	        new OpCodeInfo(316, Mnemonics.PPC_OP_XOR, "xor", 0),
	        new OpCodeInfo(339, Mnemonics.PPC_OP_MFSPR, "mfspr", 0, Mfspr_C),
	        new OpCodeInfo(341, Mnemonics.PPC_OP_LWAX, "lwax", 0),
	        new OpCodeInfo(343, Mnemonics.PPC_OP_LHAX, "lhax", 0),
	        new OpCodeInfo(370, Mnemonics.PPC_OP_TLBIA, "tlbia", 0),
	        new OpCodeInfo(371, Mnemonics.PPC_OP_MFTB, "mftb", 0),
	        new OpCodeInfo(373, Mnemonics.PPC_OP_LWAUX, "lwaux", 0),
	        new OpCodeInfo(375, Mnemonics.PPC_OP_LHAUX, "lhaux", 0),

	        new OpCodeInfo(402, Mnemonics.PPC_OP_SLBMTE, "slbmte", 0),
	        new OpCodeInfo(407, Mnemonics.PPC_OP_STHX, "sthx", 0),
	        new OpCodeInfo(412, Mnemonics.PPC_OP_ORC, "orc", 0),
	        new OpCodeInfo(434, Mnemonics.PPC_OP_SLBIE, "slbie", 0),
	        new OpCodeInfo(438, Mnemonics.PPC_OP_ECOWX, "ecowx", 0),
	        new OpCodeInfo(439, Mnemonics.PPC_OP_STHUX, "sthux", 0),
	        new OpCodeInfo(444, Mnemonics.PPC_OP_OR, "or", 0, Or_C),
	        new OpCodeInfo(467, Mnemonics.PPC_OP_MTSPR, "mtspr", 0, Mtspr_C),
	        new OpCodeInfo(470, Mnemonics.PPC_OP_DCBI, "dcbi", 0),	// Is this present on the Xenon?
	        new OpCodeInfo(476, Mnemonics.PPC_OP_NAND, "nand", 0),
	        new OpCodeInfo(498, Mnemonics.PPC_OP_SLBIA, "slbia", 0),

	        new OpCodeInfo(512, Mnemonics.PPC_OP_MCRXR, "mcrxr", 0),
	        new OpCodeInfo(533, Mnemonics.PPC_OP_LSWX, "lswx", 0),
	        new OpCodeInfo(534, Mnemonics.PPC_OP_LWBRX, "lwbrx", 0),
	        new OpCodeInfo(535, Mnemonics.PPC_OP_LFSX, "lfsx", 0),
	        new OpCodeInfo(536, Mnemonics.PPC_OP_SRW, "srw", 0),
	        new OpCodeInfo(539, Mnemonics.PPC_OP_SRD, "srd", 0),
	        new OpCodeInfo(566, Mnemonics.PPC_OP_TLBSYNC, "tlbsync", 0),
	        new OpCodeInfo(567, Mnemonics.PPC_OP_LFSUX, "lfsux", 0),
	        new OpCodeInfo(595, Mnemonics.PPC_OP_MFSR, "mfsr", 0),
	        new OpCodeInfo(597, Mnemonics.PPC_OP_LSWI, "lswi", 0),
	        new OpCodeInfo(598, Mnemonics.PPC_OP_SYNC, "sync", 0),
	        new OpCodeInfo(599, Mnemonics.PPC_OP_LFDX, "lfdx", 0),

	        new OpCodeInfo(631, Mnemonics.PPC_OP_LFDUX, "lfdux", 0),
	        new OpCodeInfo(659, Mnemonics.PPC_OP_MFSRIN, "mfsrin", 0),
	        new OpCodeInfo(661, Mnemonics.PPC_OP_STSWX, "stswx", 0),
	        new OpCodeInfo(662, Mnemonics.PPC_OP_STWBRX, "stwbrx", 0),
	        new OpCodeInfo(663, Mnemonics.PPC_OP_STFSX, "stfsx", 0),
	        new OpCodeInfo(695, Mnemonics.PPC_OP_STFSUX, "stfsux", 0),

	        new OpCodeInfo(725, Mnemonics.PPC_OP_STSWI, "stswi", 0),
	        new OpCodeInfo(727, Mnemonics.PPC_OP_STFDX, "stfdx", 0),
	        new OpCodeInfo(758, Mnemonics.PPC_OP_DCBA, "dcba", 0),	// Is this present on the Xenon?
	        new OpCodeInfo(759, Mnemonics.PPC_OP_STFDUX, "stfdux", 0),
	        new OpCodeInfo(790, Mnemonics.PPC_OP_LHBRX, "lhbrx", 0),
	        new OpCodeInfo(792, Mnemonics.PPC_OP_SRAW, "sraw", 0),
	        new OpCodeInfo(794, Mnemonics.PPC_OP_SRAD, "srad", 0),

	        new OpCodeInfo(824, Mnemonics.PPC_OP_SRAWI, "srawi", 0, Srawi_C),
	        /* The next two maps to the same opcode. The last bit has a special meaning */
	        new OpCodeInfo(826, Mnemonics.PPC_OP_SRADI, "sradi", 0),
	        new OpCodeInfo(827, Mnemonics.PPC_OP_SRADI, "sradi", 0),
	        new OpCodeInfo(851, Mnemonics.PPC_OP_SLBMFEV, "slbmfev", 0),
	        new OpCodeInfo(854, Mnemonics.PPC_OP_EIEIO, "eieio", 0),

	        new OpCodeInfo(915, Mnemonics.PPC_OP_SLBMFEE, "slbmfee", 0),
	        new OpCodeInfo(918, Mnemonics.PPC_OP_STHBRX, "sthbrx", 0),
	        new OpCodeInfo(922, Mnemonics.PPC_OP_EXTSH, "extsh", 0),
	        new OpCodeInfo(954, Mnemonics.PPC_OP_EXTSB, "extsb", 0),
	        new OpCodeInfo(982, Mnemonics.PPC_OP_ICBI, "icbi", 0),
	        new OpCodeInfo(983, Mnemonics.PPC_OP_STFIWX, "stfiwx", 0),
	        new OpCodeInfo(986, Mnemonics.PPC_OP_EXTSW, "extsw", 0),

	        new OpCodeInfo(1014, Mnemonics.PPC_OP_DCBZ, "dcbz", 0),
        };

        OpCodeInfo[] Table31_2 = new OpCodeInfo[]
        {
	        new OpCodeInfo(8, Mnemonics.PPC_OP_SUBFC, "subfc", 0),
	        new OpCodeInfo(10, Mnemonics.PPC_OP_ADDC, "addc", 0),
	        new OpCodeInfo(11, Mnemonics.PPC_OP_MULHWU, "mulhwu", 0),
	        new OpCodeInfo(40, Mnemonics.PPC_OP_SUBF, "subf", 0, Subf_C),
	        new OpCodeInfo(75, Mnemonics.PPC_OP_MULHW, "mulhw", 0),

	        new OpCodeInfo(104, Mnemonics.PPC_OP_NEG, "negx", 0),
	        new OpCodeInfo(136, Mnemonics.PPC_OP_SUBFE, "subfe", 0),
	        new OpCodeInfo(138, Mnemonics.PPC_OP_ADDE, "adde", 0),
	
	        new OpCodeInfo(200, Mnemonics.PPC_OP_SUBFZE, "subfze", 0),
	        new OpCodeInfo(202, Mnemonics.PPC_OP_ADDZE, "addze", 0),
	        new OpCodeInfo(232, Mnemonics.PPC_OP_SUBFME, "subfme", 0),
	        new OpCodeInfo(233, Mnemonics.PPC_OP_MULLD, "mulld", 0),
	        new OpCodeInfo(234, Mnemonics.PPC_OP_ADDME, "addme", 0),
	        new OpCodeInfo(235, Mnemonics.PPC_OP_MULLW, "mullw", 0),
	        new OpCodeInfo(266, Mnemonics.PPC_OP_ADD, "add", 0, Add_C),
	
	        new OpCodeInfo(457, Mnemonics.PPC_OP_DIVDU, "divdu", 0),
	        new OpCodeInfo(459, Mnemonics.PPC_OP_DIVWU, "divwu", 0),
	        new OpCodeInfo(489, Mnemonics.PPC_OP_DIVD, "divd", 0),
	        new OpCodeInfo(491, Mnemonics.PPC_OP_DIVW, "divw", 0),
        };

        OpCodeInfo[] Table58 = new OpCodeInfo[]
        {
	        new OpCodeInfo(0, Mnemonics.PPC_OP_LD, "ld", 0, Ld_C),
	        new OpCodeInfo(1, Mnemonics.PPC_OP_LDU, "ldu", 0),
	        new OpCodeInfo(2, Mnemonics.PPC_OP_LWA, "lwa", 0)
        };

        OpCodeInfo[] Table59 = new OpCodeInfo[]
        {
	        new OpCodeInfo(18, Mnemonics.PPC_OP_FDIVS, "fdivs", 0),
	        new OpCodeInfo(20, Mnemonics.PPC_OP_FSUBS, "fsubs", 0),
	        new OpCodeInfo(21, Mnemonics.PPC_OP_FADDS, "fadds", 0),
	        new OpCodeInfo(22, Mnemonics.PPC_OP_FSQRTS, "fsqrts", 0),
	        new OpCodeInfo(24, Mnemonics.PPC_OP_FRES, "fres", 0),
	        new OpCodeInfo(25, Mnemonics.PPC_OP_FMULS, "fmuls", 0),
	        new OpCodeInfo(28, Mnemonics.PPC_OP_FMSUBS, "fmsubs", 0),
	        new OpCodeInfo(29, Mnemonics.PPC_OP_FMADDS, "fmadds", 0),
	        new OpCodeInfo(30, Mnemonics.PPC_OP_FNMSUBS, "fnmsubs", 0),
	        new OpCodeInfo(31, Mnemonics.PPC_OP_FNMADDS, "fnmadds", 0),
        };

        OpCodeInfo[] Table63 = new OpCodeInfo[]
        {
	        new OpCodeInfo(0,   Mnemonics.PPC_OP_FCMPU, "fcmpu", 0),
	        new OpCodeInfo(12,  Mnemonics.PPC_OP_FRSP, "frsp", 0),
	        new OpCodeInfo(14,  Mnemonics.PPC_OP_FCTIW, "fctiw", 0),
	        new OpCodeInfo(15,  Mnemonics.PPC_OP_FCTIWZ, "fctiwz", 0),
	        new OpCodeInfo(32,  Mnemonics.PPC_OP_FCMPO, "fcmpo", 0),
	        new OpCodeInfo(38,  Mnemonics.PPC_OP_MTFSB1, "mtfsb1", 0),
	        new OpCodeInfo(40,  Mnemonics.PPC_OP_FNEG, "fneg", 0),
	        new OpCodeInfo(64,  Mnemonics.PPC_OP_MCRFS, "mcrfs", 0),
	        new OpCodeInfo(70,  Mnemonics.PPC_OP_MTFSB0, "mtfsb0", 0),
	        new OpCodeInfo(72,  Mnemonics.PPC_OP_FMR, "fmr", 0),
	        new OpCodeInfo(134, Mnemonics.PPC_OP_MTFSFI, "mtfsfi", 0),
	        new OpCodeInfo(136, Mnemonics.PPC_OP_FNABS, "fnabs", 0),
	        new OpCodeInfo(264, Mnemonics.PPC_OP_FABS, "fabs", 0),
	        new OpCodeInfo(583, Mnemonics.PPC_OP_MFFS, "mffs", 0),
	        new OpCodeInfo(711, Mnemonics.PPC_OP_MTFSF, "mtfsf", 0),
	        new OpCodeInfo(814, Mnemonics.PPC_OP_FCTID, "fctid", 0),
	        new OpCodeInfo(815, Mnemonics.PPC_OP_FCTIDZ, "fctidz", 0),
	        new OpCodeInfo(846, Mnemonics.PPC_OP_FCFID, "fcfid", 0),
        };

        OpCodeInfo[] Table63_2 = new OpCodeInfo[]
        {
	        new OpCodeInfo(18, Mnemonics.PPC_OP_FDIV, "fdiv", 0),
	        new OpCodeInfo(20, Mnemonics.PPC_OP_FSUB, "fsub", 0),
	        new OpCodeInfo(21, Mnemonics.PPC_OP_FADD, "fadd", 0),
	        new OpCodeInfo(22, Mnemonics.PPC_OP_FSQRT, "fsqrt", 0),
	        new OpCodeInfo(23, Mnemonics.PPC_OP_FSEL, "fsel", 0),
	        new OpCodeInfo(25, Mnemonics.PPC_OP_FMUL, "fmul", 0),
	        new OpCodeInfo(26, Mnemonics.PPC_OP_FRSQRTE, "frsqrte", 0),
	        new OpCodeInfo(28, Mnemonics.PPC_OP_FMSUB, "fmsub", 0),
	        new OpCodeInfo(29, Mnemonics.PPC_OP_FMADD, "fmadd", 0),
	        new OpCodeInfo(30, Mnemonics.PPC_OP_FNMSUB, "fnmsub", 0),
	        new OpCodeInfo(31, Mnemonics.PPC_OP_FNMADD, "fnmadd", 0),
        };

        static OpCodeInfo[] RealTableBase = new OpCodeInfo[64];
        static OpCodeInfo[] RealTable4 = new OpCodeInfo[1024];
        static OpCodeInfo[] RealTable19 = new OpCodeInfo[1024];
        static OpCodeInfo[] RealTable30 = new OpCodeInfo[16];
        static OpCodeInfo[] RealTable31 = new OpCodeInfo[1024];
        static OpCodeInfo[] RealTable58 = new OpCodeInfo[4];
        static OpCodeInfo[] RealTable59 = new OpCodeInfo[32];
        static OpCodeInfo[] RealTable63 = new OpCodeInfo[1024];

        static OpCodeInfo[][] SubTables = new OpCodeInfo[][]
        {
	        RealTable4, RealTable19, RealTable30, RealTable31, RealTable58, RealTable59, RealTable63,
        };

        public void SetupTables()
        {
            RealTableBase = new OpCodeInfo[64];

	        for (int i = 0; i < 64; i++)
		        RealTableBase[BaseTable[i].OpCode] = BaseTable[i];

	        /* Set all the entries to OP_INVALID */
	        for (int i = 0; i < 1024; i++)
		        RealTable4[i] = InvalidOp;
	        for (int i = 0; i < 1024; i++)
		        RealTable19[i] = InvalidOp;
	        for (int i = 0; i < 16; i++)
		        RealTable30[i] = InvalidOp;
	        for (int i = 0; i < 1024; i++)
		        RealTable31[i] = InvalidOp;
	        for (int i = 0; i < 4; i++)
		        RealTable58[i] = InvalidOp;
	        for (int i = 0; i < 32; i++)
		        RealTable59[i] = InvalidOp;
	        for (int i = 0; i < 1024; i++)
		        RealTable63[i] = InvalidOp;

	        for (int i = 0; i < Table19.Count(); i++)
	        {
		        int op = Table19[i].OpCode;
		        RealTable19[op] = Table19[i];
	        }

            for (int i = 0; i < Table30.Count(); i++)
	        {
		        int op = Table30[i].OpCode;
		        RealTable30[op] = Table30[i];
	        }

            for (int i = 0; i < Table31.Count(); i++)
	        {
		        int op = Table31[i].OpCode;
		        RealTable31[op] = Table31[i];
	        }

	        /* The Table31_2 contains OpCodes that may or not have the OV bit */
	        for (int i = 0; i < 1; i++)
	        {
		        int fill = i << 9;
                for (int j = 0; j < Table31_2.Count(); j++)
		        {
			        int op = fill + Table31_2[j].OpCode;
			        RealTable31[op] = Table31_2[j];
		        }
	        }

            for (int i = 0; i < Table58.Count(); i++)
	        {
		        int op = Table58[i].OpCode;
		        RealTable58[op] = Table58[i];
	        }

            for (int i = 0; i < Table59.Count(); i++)
	        {
		        int op = Table59[i].OpCode;
		        RealTable59[op] = Table59[i];
	        }

	        for (int i = 0; i < Table63.Count(); i++)
	        {
		        int op = Table63[i].OpCode;
		        RealTable63[op] = Table63[i];
	        }

	        for (int i = 0; i < 32; i++)
	        {
		        int fill = i << 5;
                for (int j = 0; j < Table63_2.Count(); j++)
		        {
			        int op = fill + Table63_2[j].OpCode;
			        RealTable63[op] = Table63_2[j];
		        }
	        }
        }

        public OpCodeInfo GetInfo(uint Instruction)
        {
	        uint bIndex = Instruction >> 26;
            if ((RealTableBase[bIndex].Flags & OP_SUBTABLE) == 0)
                return RealTableBase[bIndex];

            int subId = (RealTableBase[bIndex].Flags >> 24) & 7;
	        OpCodeInfo[] SubTable = SubTables[subId];

            if (bIndex == 31 || bIndex == 19 || bIndex == 63 || bIndex == 4)
		        return SubTable[(Instruction >> 1) & 0x3FF];
            else if (bIndex == 58)
		        return SubTable[Instruction & 0x3];
            else if (bIndex == 30)
		        return SubTable[Instruction & 0xF];
            else if (bIndex == 59)
		        return SubTable[(Instruction >> 1) & 0x1F];

	        throw new Exception("XenonParser: How the f*** did you get here??");
        }

        public class Instruction
        {
            public Instruction(uint instr)
            {
                OpCode = instr;
            }

            public ushort RS()
            {
                return (ushort)((OpCode >> 21) & 0x1F);
            }

            public ushort RD()
            {
                return (ushort)((OpCode >> 21) & 0x1F);
            }

            public ushort RA()
            {
                return (ushort)((OpCode >> 16) & 0x1F);
            }

            public ushort RB()
            {
                return (ushort)((OpCode >> 11) & 0x1F);
            }

            public ushort DS()
            {
                return (ushort)((OpCode & 0xFFFC) >> 2);
            }

            public uint LI()
            {
                return (uint)((OpCode & 0x03FFFFFC) >> 2);
            }

            public ushort BD()
            {
                return (ushort)((OpCode & 0xFFFC) >> 2);
            }

            public short SIMM()
            {
                return (short)(OpCode & 0xFFFF);
            }

            public ushort UIMM()
            {
                return (ushort)(OpCode & 0xFFFF);
            }

            public uint CRFD()
            {
                return (uint)((OpCode >> 23) & 7);
            }

            public bool CmpLong()
            {
                return (OpCode & 0x200000) != 0;
            }

            public uint BO()
            {
                return (uint)((OpCode >> 21) & 0x1F);
            }
            
            public uint BI()
            {
                return (uint)((OpCode >> 16) & 0x1F);
            }

            public bool LK()
            {
                return (OpCode & 1) == 1;
            }

            public bool RC()
            {
                return LK();
            }

            public bool OE()
            {
                return ((OpCode >> 10) & 1) == 1;
            }

            public uint SH()
            {
                return ((OpCode >> 11) & 0x1F);
            }
            
            public uint MB()
            {
                return ((OpCode >> 6) & 0x1F);
            }
            
            public uint ME()
            {
                return ((OpCode >> 1) & 0x1F);
            }

            public uint SPR()
            {
                return ((OpCode >> 11) & 0x3FF);
            }

            public uint TO()
            {
                return ((OpCode >> 21) & 0x1F);
            }

            public bool AA()
            {
                return (OpCode & 2) == 2;
            }

            public uint OpCode;
        }

        static string RegName(int rn)
        {
            if (rn == 0)
                throw new Exception("Shouldn't be here");
            if (rn == 1)
                return "sp";
            else
                return "r" + rn;
        }

        static List<CStatement> Add_C(uint pc, uint instruction)
        {
            Instruction i = new Instruction(instruction);

            /* op1 + op2 */
            CStatement stat = new CStatement(CStatement.Kinds.Addition, RegName(i.RA()), RegName(i.RB()));

            /* dest = expr */
            CStatement ass = new CStatement(CStatement.Kinds.Assignment, RegName(i.RD()), stat);

            List<CStatement> stats = new List<CStatement>();
            stats.Add(ass);

            if (i.OE())
                MessageBox.Show("This instruction sets the O bit. I don't know how to translate it! Ignoring.");

            if (i.RC())
                stats.Add(new CStatement(CStatement.Kinds.Assignment, "cr0", RegName(i.RD())));

            return stats;
        }

        static List<CStatement> Addi_C(uint pc, uint instruction)
        {
            Instruction i = new Instruction(instruction);

            CStatement stat = new CStatement(CStatement.Kinds.Assignment, RegName(i.RD()));

            if (i.RA() == 0)
                stat.Op2 = new CStatement.COperand((ulong)(long)i.SIMM());
            else if (i.SIMM() == 0)
                stat.Op2 = new CStatement.COperand(RegName(i.RA()));
            else
            {
                CStatement add = new CStatement(CStatement.Kinds.Addition, RegName(i.RA()), (ulong)(long)i.SIMM());

                long val = (long)add.Op2.Value;
                if (val < 0)
                {
                    add.Kind = CStatement.Kinds.Subtraction;
                    add.Op2.Value = (ulong)-val;
                }

                stat.Op2 = new CStatement.COperand(add);
            }

            List<CStatement> stats = new List<CStatement>();
            stats.Add(stat);
            return stats;
        }

        static List<CStatement> Addis_C(uint pc, uint instruction)
        {
            Instruction i = new Instruction(instruction);

            CStatement stat = new CStatement(CStatement.Kinds.Assignment, RegName(i.RD()));

            if (i.RA() == 0)
                stat.Op2 = new CStatement.COperand((ulong)(long)i.SIMM() << 16);
            else if (i.SIMM() == 0)
                stat.Op2 = new CStatement.COperand(RegName(i.RA()));
            else
            {
                CStatement add = new CStatement(CStatement.Kinds.Addition, RegName(i.RA()), (ulong)(long)i.SIMM() << 16);

                long val = (long)add.Op2.Value;
                if (val < 0)
                {
                    add.Kind = CStatement.Kinds.Subtraction;
                    add.Op2.Value = (ulong)-val;
                }

                stat.Op2 = new CStatement.COperand(add);
            }

            List<CStatement> stats = new List<CStatement>();
            stats.Add(stat);
            return stats;
        }

        static List<CStatement> And_C(uint pc, uint instruction)
        {
            Instruction i = new Instruction(instruction);

            /* op1 & op2 */
            CStatement stat = new CStatement(CStatement.Kinds.And, RegName(i.RS()), RegName(i.RB()));

            /* dest = expr */
            CStatement ass = new CStatement(CStatement.Kinds.Assignment, RegName(i.RA()), stat);

            List<CStatement> stats = new List<CStatement>();
            stats.Add(ass);

            if (i.RC())
                stats.Add(new CStatement(CStatement.Kinds.Assignment, "cr0", RegName(i.RA())));

            return stats;
        }

        static List<CStatement> Andc_C(uint pc, uint instruction)
        {
            Instruction i = new Instruction(instruction);

            CStatement not = new CStatement(CStatement.Kinds.BinaryNot, RegName(i.RB()));

            /* op1 & op2 */
            CStatement stat = new CStatement(CStatement.Kinds.And, RegName(i.RS()), not);

            /* dest = expr */
            CStatement ass = new CStatement(CStatement.Kinds.Assignment, RegName(i.RA()), stat);

            List<CStatement> stats = new List<CStatement>();
            stats.Add(ass);

            if (i.RC())
                stats.Add(new CStatement(CStatement.Kinds.Assignment, "cr0", RegName(i.RA())));

            return stats;
        }

        static List<CStatement> Bcctr_C(uint pc, uint instruction)
        {
            Instruction i = new Instruction(instruction);
            List<CStatement> stats = new List<CStatement>();

            if ((i.BO() & 4) == 0)
            {
                CStatement ctrSub = new CStatement(CStatement.Kinds.Subtraction, "CTR", 1);
                CStatement ctrAss = new CStatement(CStatement.Kinds.Assignment, "CTR", ctrSub);
                stats.Add(ctrAss);
            }

            CStatement final = ConditionCheck(i);

            CStatement branch = new CStatement();
            if (i.LK())
            {
                branch.Kind = CStatement.Kinds.Assignment;
                branch.Op1 = new CStatement.COperand("r3");

                CStatement realBranch = new CStatement(CStatement.Kinds.Call);
                realBranch.CallFuncName = "ctr";
                realBranch.BranchDestinationRegister = "ctr";

                branch.Op2 = new CStatement.COperand(realBranch);
            }
            else
            {
                branch.Kind = CStatement.Kinds.Goto;
                branch.BranchDestination = "ctr";
                branch.BranchDestinationRegister = "ctr";
            }

            if (final == null)
                final = branch;
            else
            {
                final.InnerBlock = new List<CStatement>();
                final.InnerBlock.Add(branch);
            }

            stats.Add(final);
            return stats;
        }

        static List<CStatement> Bclr_C(uint pc, uint instruction)
        {
            Instruction i = new Instruction(instruction);
            List<CStatement> stats = new List<CStatement>();

            if ((i.BO() & 4) == 0)
            {
                CStatement ctrSub = new CStatement(CStatement.Kinds.Subtraction, "CTR", 1);
                CStatement ctrAss = new CStatement(CStatement.Kinds.Assignment, "CTR", ctrSub);
                stats.Add(ctrAss);
            }

            CStatement final = ConditionCheck(i);

            CStatement branch = new CStatement();
            if (i.LK())
            {
                branch.Kind = CStatement.Kinds.Assignment;
                branch.Op1 = new CStatement.COperand("r3");

                CStatement realBranch = new CStatement(CStatement.Kinds.Call);
                realBranch.CallFuncName = "lr";
                realBranch.BranchDestinationRegister = "lr";

                branch.Op2 = new CStatement.COperand(realBranch);
            }
            else
                branch.Kind = CStatement.Kinds.Return;

            if (final == null)
                final = branch;
            else
            {
                final.InnerBlock = new List<CStatement>();
                final.InnerBlock.Add(branch);
            }

            stats.Add(final);
            return stats;
        }

        static List<CStatement> B_C(uint pc, uint instruction)
        {
            Instruction i = new Instruction(instruction);
            List<CStatement> stats = new List<CStatement>();

            uint destination = (uint) SignExtend24(i.LI() << 2);
            if ((instruction & 2) != 2)
                destination += pc;

            Function f = decompiler.Functions.Find(delegate(Function fn) { return fn.Address == destination; });
            String destName;
            if (f != null)
                destName = f.Name;
            else
                destName = "L" + destination.ToString("X8");

            CStatement branch = new CStatement();
            if (i.LK())
            {
                if (f != null && decompiler.IgnoredCalls.Contains(f))
                    return new List<CStatement>();

                if (f != null && f.Returns.Name == "void" && f.Returns.Kind == CType.TypeKind.ValueType)
                {
                    branch.Kind = CStatement.Kinds.Call;
                    branch.CallFuncName = destName;
                    branch.CalledFunction = f;
                }
                else
                {
                    branch.Kind = CStatement.Kinds.Assignment;
                    branch.Op1 = new CStatement.COperand("r3");

                    CStatement realBranch = new CStatement(CStatement.Kinds.Call);
                    realBranch.CallFuncName = destName;
                    realBranch.CalledFunction = f;

                    branch.Op2 = new CStatement.COperand(realBranch);
                }
            }
            else
            {
                if (f != null && decompiler.CallIsRet.Contains(f))
                    branch.Kind = CStatement.Kinds.Return;
                else
                {
                    branch.Kind = CStatement.Kinds.Goto;
                    branch.BranchDestination = destName;
                    branch.BranchDestinationAddr = destination;
                }
            }

            stats.Add(branch);
            return stats;
        }

        static List<CStatement> Bc_C(uint pc, uint instruction)
        {
            Instruction i = new Instruction(instruction);
            List<CStatement> stats = new List<CStatement>();

            if ((i.BO() & 4) == 0)
            {
                CStatement ctrSub = new CStatement(CStatement.Kinds.Subtraction, "CTR", 1);
                CStatement ctrAss = new CStatement(CStatement.Kinds.Assignment, "CTR", ctrSub);
                stats.Add(ctrAss);
            }

            CStatement final = ConditionCheck(i);

            uint destination = (uint)(int)(short)(i.BD() << 2);
            if ((instruction & 2) != 2)
                destination += pc;

            Function f = decompiler.Functions.Find(delegate(Function fn) { return fn.Address == destination; });
            String destName;
            if (f != null)
                destName = f.Name;
            else
                destName = "L" + destination.ToString("X8");

            CStatement branch = new CStatement();
            if (i.LK())
            {
                if (f != null && decompiler.IgnoredCalls.Contains(f))
                    return new List<CStatement>();

                if (f != null && f.Returns.Name == "void" && f.Returns.Kind == CType.TypeKind.ValueType)
                {
                    branch.Kind = CStatement.Kinds.Call;
                    branch.CallFuncName = destName;
                    branch.CalledFunction = f;
                }
                else
                {
                    branch.Kind = CStatement.Kinds.Assignment;
                    branch.Op1 = new CStatement.COperand("r3");

                    CStatement realBranch = new CStatement(CStatement.Kinds.Call);
                    realBranch.CallFuncName = destName;
                    realBranch.CalledFunction = f;

                    branch.Op2 = new CStatement.COperand(realBranch);
                }
            }
            else
            {
                if (f != null && decompiler.CallIsRet.Contains(f))
                    branch.Kind = CStatement.Kinds.Return;
                else
                {
                    branch.Kind = CStatement.Kinds.Goto;
                    branch.BranchDestination = destName;
                    branch.BranchDestinationAddr = destination;
                }
            }

            if (final == null)
                final = branch;
            else
            {
                final.InnerBlock = new List<CStatement>();
                final.InnerBlock.Add(branch);
            }

            stats.Add(final);
            return stats;
        }

        private static CStatement ConditionCheck(Instruction i)
        {
            CStatement CtrCondition = null;
            CStatement CrCondition = null;

            if ((i.BO() & 4) == 0)
            {
                CtrCondition = new CStatement();
            }

            if ((i.BO() & 0x10) == 0)
            {
                CrCondition = new CStatement();
                CrCondition.Kind = CStatement.Kinds.Comparison;

                uint cr = i.BI() / 4;
                uint fld = i.BI() % 4;

                CrCondition.Op1 = new CStatement.COperand("cr" + cr);
                CrCondition.Op2 = new CStatement.COperand(0);

                bool bo = ((i.BO() >> 3) & 1) == 1;
                switch (fld)
                {
                    case 0:
                        if (bo)
                            CrCondition.ConditionSign = CStatement.Conditions.LessThan;
                        else
                            CrCondition.ConditionSign = CStatement.Conditions.GreaterEqualThan;
                        break;
                    case 1:
                        if (bo)
                            CrCondition.ConditionSign = CStatement.Conditions.GreaterThan;
                        else
                            CrCondition.ConditionSign = CStatement.Conditions.LessEqualThan;
                        break;
                    case 2:
                        if (bo)
                            CrCondition.ConditionSign = CStatement.Conditions.Equal;
                        else
                            CrCondition.ConditionSign = CStatement.Conditions.NotEqual;
                        break;
                    case 3:
                        if (bo)
                            CrCondition.ConditionSign = CStatement.Conditions.Overflow;
                        else
                            CrCondition.ConditionSign = CStatement.Conditions.NotOverflow;
                        break;
                }
            }

            CStatement final = null;

            if (CrCondition != null)
            {
                if (CtrCondition != null)
                {
                    CStatement composite = new CStatement(CStatement.Kinds.CompositeCondition, CrCondition, CtrCondition);
                    composite.ConditionSign = CStatement.Conditions.And;

                    final = new CStatement();
                    final.Kind = CStatement.Kinds.Conditional;
                    final.Condition = composite;
                }
                else
                {
                    final = new CStatement();
                    final.Kind = CStatement.Kinds.Conditional;
                    final.Condition = CrCondition;
                }
            }
            else if (CtrCondition != null)
            {
                final = new CStatement();
                final.Kind = CStatement.Kinds.Conditional;
                final.Condition = CtrCondition;
            }

            return final;
        }

        static List<CStatement> Cmp_C(uint pc, uint instruction)
        {
            Instruction i = new Instruction(instruction);

            CStatement stat = new CStatement(CStatement.Kinds.Subtraction, RegName(i.RA()), RegName(i.RB()));
            CStatement ass = new CStatement(CStatement.Kinds.Assignment, "cr" + i.CRFD(), stat);
            if (!i.CmpLong())
                ass.OperandSizes = stat.OperandSizes = CStatement.Sizes.Int;

            List<CStatement> stats = new List<CStatement>();
            stats.Add(ass);
            return stats;
        }

        static List<CStatement> Cmpl_C(uint pc, uint instruction)
        {
            Instruction i = new Instruction(instruction);

            CStatement stat = new CStatement(CStatement.Kinds.Subtraction, RegName(i.RA()), RegName(i.RB()));
            CStatement ass = new CStatement(CStatement.Kinds.Assignment, "cr" + i.CRFD(), stat);
            if (!i.CmpLong())
                ass.OperandSizes = stat.OperandSizes = CStatement.Sizes.Int;

            List<CStatement> stats = new List<CStatement>();
            stats.Add(ass);
            return stats;
        }

        static List<CStatement> Cmpli_C(uint pc, uint instruction)
        {
            Instruction i = new Instruction(instruction);

            CStatement ass;
            if (i.UIMM() != 0)
            {
                CStatement stat = new CStatement(CStatement.Kinds.Subtraction, RegName(i.RA()), (ulong)i.UIMM());
                if (!i.CmpLong())
                    stat.OperandSizes = CStatement.Sizes.Int;
                ass = new CStatement(CStatement.Kinds.Assignment, "cr" + i.CRFD(), stat);
            }
            else
                ass = new CStatement(CStatement.Kinds.Assignment, "cr" + i.CRFD(), RegName(i.RA()));
         
            if (!i.CmpLong())
                ass.OperandSizes = CStatement.Sizes.Int;

            List<CStatement> stats = new List<CStatement>();
            stats.Add(ass);
            return stats;
        }

        static List<CStatement> Cmpi_C(uint pc, uint instruction)
        {
            Instruction i = new Instruction(instruction);

            CStatement ass;
            if (i.UIMM() != 0)
            {
                CStatement stat = new CStatement(CStatement.Kinds.Subtraction, RegName(i.RA()), (ulong)i.UIMM());
                if (!i.CmpLong())
                    stat.OperandSizes = CStatement.Sizes.Int;
                ass = new CStatement(CStatement.Kinds.Assignment, "cr" + i.CRFD(), stat);
            }
            else
                ass = new CStatement(CStatement.Kinds.Assignment, "cr" + i.CRFD(), RegName(i.RA()));
            
            if (!i.CmpLong())
                ass.OperandSizes = CStatement.Sizes.Int;

            List<CStatement> stats = new List<CStatement>();
            stats.Add(ass);
            return stats;
        }

        static List<CStatement> Lbz_C(uint pc, uint instruction)
        {
            Instruction i = new Instruction(instruction);

            CStatement stat = new CStatement(CStatement.Kinds.Assignment, RegName(i.RD()));
            stat.OperandSizes = CStatement.Sizes.Byte;

            int ds = (int)i.SIMM();
            if (i.RA() != 0)
                stat.Op2 = new CStatement.COperand(RegName(i.RA()), ds);
            else
            {
                stat.Op2 = new CStatement.COperand();
                stat.Op2.Kind = CStatement.OperandKinds.AddressPointer;
                stat.Op2.Offset = ds;
            }

            List<CStatement> stats = new List<CStatement>();
            stats.Add(stat);
            return stats;
        }

        static List<CStatement> Lbzx_C(uint pc, uint instruction)
        {
            Instruction i = new Instruction(instruction);

            CStatement stat = new CStatement(CStatement.Kinds.Assignment, RegName(i.RD()));
            stat.OperandSizes = CStatement.Sizes.Byte;

            if (i.RA() != 0)
            {
                if (i.RB() != 0)
                    stat.Op2 = new CStatement.COperand(RegName(i.RA()), RegName(i.RB()));
                else
                    stat.Op2 = new CStatement.COperand(RegName(i.RA()), 0);
            }
            else
                stat.Op2 = new CStatement.COperand(RegName(i.RB()), 0);

            List<CStatement> stats = new List<CStatement>();
            stats.Add(stat);
            return stats;
        }

        static List<CStatement> Ld_C(uint pc, uint instruction)
        {
            Instruction i = new Instruction(instruction);

            CStatement stat = new CStatement(CStatement.Kinds.Assignment, RegName(i.RD()));

            int ds = (int)i.DS() << 2;
            if (i.RA() != 0)
                stat.Op2 = new CStatement.COperand(RegName(i.RA()), ds);
            else
            {
                stat.Op2 = new CStatement.COperand();
                stat.Op2.Kind = CStatement.OperandKinds.AddressPointer;
                stat.Op2.Offset = ds;
            }

            List<CStatement> stats = new List<CStatement>();
            stats.Add(stat);
            return stats;
        }

        static List<CStatement> Lhz_C(uint pc, uint instruction)
        {
            Instruction i = new Instruction(instruction);

            CStatement stat = new CStatement(CStatement.Kinds.Assignment, RegName(i.RD()));
            stat.OperandSizes = CStatement.Sizes.Short;

            int ds = (int)i.SIMM();
            if (i.RA() != 0)
                stat.Op2 = new CStatement.COperand(RegName(i.RA()), ds);
            else
            {
                stat.Op2 = new CStatement.COperand();
                stat.Op2.Kind = CStatement.OperandKinds.AddressPointer;
                stat.Op2.Offset = ds;
            }

            List<CStatement> stats = new List<CStatement>();
            stats.Add(stat);
            return stats;
        }

        static List<CStatement> Lwz_C(uint pc, uint instruction)
        {
            Instruction i = new Instruction(instruction);

            CStatement stat = new CStatement(CStatement.Kinds.Assignment, RegName(i.RD()));
            stat.OperandSizes = CStatement.Sizes.Int;

            int ds = (int)i.SIMM();
            if (i.RA() != 0)
                stat.Op2 = new CStatement.COperand(RegName(i.RA()), ds);
            else
            {
                stat.Op2 = new CStatement.COperand();
                stat.Op2.Kind = CStatement.OperandKinds.AddressPointer;
                stat.Op2.Offset = ds;
            }

            List<CStatement> stats = new List<CStatement>();
            stats.Add(stat);
            return stats;
        }

        static List<CStatement> Mfspr_C(uint pc, uint instruction)
        {
            Instruction i = new Instruction(instruction);

            CStatement stat = new CStatement(CStatement.Kinds.Assignment, RegName(i.RD()));
            stat.OperandSizes = CStatement.Sizes.Long;

            stat.Op2 = new CStatement.COperand();
            stat.Op2.Kind = CStatement.OperandKinds.Variable;
            switch (i.SPR())
            {
                case 32:
                    stat.Op2.Name = "xer";
                    break;
                case 256:
                    stat.Op2.Name = "lr";
                    break;
                case 288:
                    stat.Op2.Name = "ctr";
                    break;
                default:
                    stat.Op2.Name = "spr" + i.SPR();
                    break;
            }

            List<CStatement> stats = new List<CStatement>();
            stats.Add(stat);
            return stats;
        }
        
        static List<CStatement> Mtspr_C(uint pc, uint instruction)
        {
            Instruction i = new Instruction(instruction);

            CStatement stat = new CStatement(CStatement.Kinds.Assignment, null, RegName(i.RS()));
            stat.OperandSizes = CStatement.Sizes.Long;

            stat.Op1 = new CStatement.COperand();
            stat.Op1.Kind = CStatement.OperandKinds.Variable;
            switch (i.SPR())
            {
                case 32:
                    stat.Op1.Name = "xer";
                    break;
                case 256:
                    stat.Op1.Name = "lr";
                    break;
                case 288:
                    stat.Op1.Name = "ctr";
                    break;
                default:
                    stat.Op1.Name = "spr" + i.SPR();
                    break;
            }

            List<CStatement> stats = new List<CStatement>();
            stats.Add(stat);
            return stats;
        }
        
        static List<CStatement> Or_C(uint pc, uint instruction)
        {
            Instruction i = new Instruction(instruction);
            CStatement stat;

            if (i.RS() == 0 && i.RB() != 0)
                stat = new CStatement(CStatement.Kinds.Assignment, RegName(i.RA()), RegName(i.RB()));
            else if (i.RB() == 0 && i.RS() != 0)
                stat = new CStatement(CStatement.Kinds.Assignment, RegName(i.RA()), RegName(i.RS()));
            else if (i.RS() == i.RB())
            {
                if (i.RS() != 0)
                    stat = new CStatement(CStatement.Kinds.Assignment, RegName(i.RA()), RegName(i.RS()));
                else
                    stat = new CStatement(CStatement.Kinds.Assignment, RegName(i.RA()), 0);
            }
            else
            {
                CStatement or = new CStatement(CStatement.Kinds.Or, RegName(i.RS()), RegName(i.RB()));
                or.OperandSizes = CStatement.Sizes.Long;

                stat = new CStatement(CStatement.Kinds.Assignment, RegName(i.RA()), or);
            }

            stat.OperandSizes = CStatement.Sizes.Long;

            List<CStatement> stats = new List<CStatement>();
            stats.Add(stat);

            if (i.RC())
                stats.Add(new CStatement(CStatement.Kinds.Assignment, "cr0", RegName(i.RA())));

            return stats;
        }

        static List<CStatement> Ori_C(uint pc, uint instruction)
        {
            Instruction i = new Instruction(instruction);

            /* NOP */
            if (i.RA() == i.RS() && i.RS() == 0 && i.UIMM() == 0)
                return new List<CStatement>();

            CStatement stat;

            if (i.RS() == 0)
                stat = new CStatement(CStatement.Kinds.Assignment, RegName(i.RA()), i.UIMM());
            else if (i.UIMM() == 0)
                stat = new CStatement(CStatement.Kinds.Assignment, RegName(i.RA()), RegName(i.RS()));
            else
            {
                CStatement or = new CStatement(CStatement.Kinds.Or, RegName(i.RS()), i.UIMM());
                or.OperandSizes = CStatement.Sizes.Long;

                stat = new CStatement(CStatement.Kinds.Assignment, RegName(i.RA()), or);
            }

            stat.OperandSizes = CStatement.Sizes.Long;

            List<CStatement> stats = new List<CStatement>();
            stats.Add(stat);
            return stats;
        }

        static List<CStatement> Rlwinm_C(uint pc, uint instruction)
        {
            /**
             * Very complex instruction
             * RA = RotateLeft(RS, SH) & ((0xFFFFFFFF >> MB) ^ (0xFFFFFFFF >> ME));
             * 
             * Can be simplified:
             *  if (MB == 0 && ME + SH == 31)
             *      ShiftLeft (BigEndian) = ShiftRight (LittleEndian)
             *  if (ME == 31 && SH + MB == 32)
             *      ShiftRight (BigEndian) = ShiftLeft
             *  if (SH == 0 && ME == 31)
             *      ClearLeft = ClearRight
             *  if (SH == 0 && MB == 0)
             *      ClearRight = ClearLeft
             *  if (MB == 0 && ME == 31)
             *      Rotate
             *      
             *  extlwi rA,rS,n,b equivalent to rlwinm rA,rS,b,0,n – 1
             *  extrwi rA,rS,n,b equivalent to rlwinm rA,rS,b + n,32 – n,31
             *  clrlslwi rA,rS,b,n equivalent to rlwinm rA,rS,n,b – n,31 – n
             */

            List<CStatement> stats = new List<CStatement>();
            Instruction i = new Instruction(instruction);

            if (i.MB() == 0)
            {
                if (i.ME() == 31)
                {
                    /*
                     * Rotate left - Rotate right...
                     * ((x >> n) | (x << (32 - n)))
                     */

                    uint n = i.SH();

                    CStatement lsh = new CStatement(CStatement.Kinds.LeftShift, RegName(i.RS()), 32 - n);
                    CStatement rsh = new CStatement(CStatement.Kinds.RightShift, RegName(i.RS()), n);

                    CStatement or = new CStatement(CStatement.Kinds.Or, rsh, lsh);
                    CStatement ass = new CStatement(CStatement.Kinds.Assignment, RegName(i.RA()), or);
                    stats.Add(ass);
                }
                else if (i.ME() + i.SH() == 31)
                {
                    /* ShiftLeft = ShiftRight */
                    uint n = i.SH();

                    CStatement rsh = new CStatement(CStatement.Kinds.RightShift, RegName(i.RS()), n);
                    CStatement ass = new CStatement(CStatement.Kinds.Assignment, RegName(i.RA()), rsh);
                    stats.Add(ass);
                }
                else if (i.SH() == 0)
                {
                    /* ClearRight = ClearLeft */
                    uint n = 31 - i.ME();
                    uint mask = 0xFFFFFFFF << (int)n;

                    CStatement and = new CStatement(CStatement.Kinds.And, RegName(i.RS()), mask);
                    CStatement ass = new CStatement(CStatement.Kinds.Assignment, RegName(i.RA()), and);
                    stats.Add(ass);
                }
            }
            else if (i.ME() == 31 && i.SH() + i.MB() == 32)
            {
                /* ShiftRight = ShiftLeft */
                uint n = i.MB();

                CStatement lsh = new CStatement(CStatement.Kinds.LeftShift, RegName(i.RS()), n);
                CStatement ass = new CStatement(CStatement.Kinds.Assignment, RegName(i.RA()), lsh);
                stats.Add(ass);
            }
            else if (i.SH() == 0)
            {
                if (i.ME() == 31)
                {
                    /* ClearLeft = ClearRight */
                    uint n = i.MB();
                    uint mask = 0xFFFFFFFF ^ (0xFFFFFFFF << (int)n);

                    CStatement and = new CStatement(CStatement.Kinds.And, RegName(i.RS()), mask);
                    CStatement ass = new CStatement(CStatement.Kinds.Assignment, RegName(i.RA()), and);
                    stats.Add(ass);
                }
                else
                {
                    /* And without rotate */
                    uint m1 = 0xFFFFFFFF >> (int)(i.ME() + 1);
                    uint m2 = 0xFFFFFFFF >> (int)i.MB();
                    uint mask = m1 ^ m2;

                    CStatement and = new CStatement(CStatement.Kinds.And, RegName(i.RS()), mask);
                    CStatement ass = new CStatement(CStatement.Kinds.Assignment, RegName(i.RA()), and);
                    stats.Add(ass);
                }
            }
            else
            {
                /* Rotate, then and */
                uint n = i.SH();
                uint m1 = 0xFFFFFFFF << (int)i.MB();
                uint m2 = 0xFFFFFFFF << (int)i.ME();
                uint mask = m1 ^ m2;

                CStatement lsh = new CStatement(CStatement.Kinds.LeftShift, RegName(i.RS()), 32 - n);
                CStatement rsh = new CStatement(CStatement.Kinds.RightShift, RegName(i.RS()), n);
                CStatement or = new CStatement(CStatement.Kinds.Or, rsh, lsh);
                CStatement and = new CStatement(CStatement.Kinds.And, or, mask);

                CStatement ass = new CStatement(CStatement.Kinds.Assignment, RegName(i.RA()), and);
                stats.Add(ass);
            }

            if (i.RC())
                stats.Add(new CStatement(CStatement.Kinds.Assignment, "cr0", RegName(i.RA())));

            return stats;
        }

        static List<CStatement> Rlwimi_C(uint pc, uint instruction)
        {
            List<CStatement> stats = new List<CStatement>();
            Instruction i = new Instruction(instruction);

            uint m1 = 0xFFFFFFFF << (int)i.ME();
            uint m2 = 0xFFFFFFFF << (int)i.MB();
            uint mask = m1 ^ m2;

            CStatement lsh = new CStatement(CStatement.Kinds.LeftShift, RegName(i.RS()), 32 - i.SH());
            CStatement rsh = new CStatement(CStatement.Kinds.RightShift, RegName(i.RS()), i.SH());
            CStatement orRot = new CStatement(CStatement.Kinds.Or, rsh, lsh);
            CStatement notMask = new CStatement(CStatement.Kinds.BinaryNot, mask);
            CStatement and1 = new CStatement(CStatement.Kinds.And, orRot, mask);
            CStatement and2 = new CStatement(CStatement.Kinds.And, RegName(i.RA()), notMask);
            CStatement or = new CStatement(CStatement.Kinds.Or, and1, and2);
            CStatement ass = new CStatement(CStatement.Kinds.Assignment, RegName(i.RA()), or);
            stats.Add(ass);

            /* RA = (ROTL(RS, i.SH) & mask) | (RA & ~mask); */

            if (i.RC())
                stats.Add(new CStatement(CStatement.Kinds.Assignment, "cr0", RegName(i.RA())));

            return stats;
        }

        static List<CStatement> Srawi_C(uint pc, uint instruction)
        {
            Instruction i = new Instruction(instruction);

            CStatement shift = new CStatement(CStatement.Kinds.RightShift, RegName(i.RS()), i.SH());
            shift.OperandSizes = CStatement.Sizes.Int;

            CStatement stat = new CStatement(CStatement.Kinds.Assignment, RegName(i.RA()), shift);
            stat.OperandSizes = CStatement.Sizes.Int;

            List<CStatement> stats = new List<CStatement>();
            stats.Add(stat);

            if (i.RC())
                stats.Add(new CStatement(CStatement.Kinds.Assignment, "cr0", RegName(i.RA())));

            return stats;
        }

        static List<CStatement> Stb_C(uint pc, uint instruction)
        {
            Instruction i = new Instruction(instruction);

            CStatement stat = new CStatement(CStatement.Kinds.Assignment);

            if (i.RS() == 0)
                stat.Op2 = new CStatement.COperand(0);
            else
                stat.Op2 = new CStatement.COperand(RegName(i.RS()));

            int ds = (int)i.SIMM();
            if (i.RA() != 0)
                stat.Op1 = new CStatement.COperand(RegName(i.RA()), ds);
            else
            {
                stat.Op1 = new CStatement.COperand();
                stat.Op1.Kind = CStatement.OperandKinds.AddressPointer;
                stat.Op1.Offset = ds;
            }

            List<CStatement> stats = new List<CStatement>();
            stats.Add(stat);
            return stats;
        }

        static List<CStatement> Std_C(uint pc, uint instruction)
        {
            Instruction i = new Instruction(instruction);

            CStatement stat = new CStatement(CStatement.Kinds.Assignment);

            if (i.RS() == 0)
                stat.Op2 = new CStatement.COperand(0);
            else
                stat.Op2 = new CStatement.COperand(RegName(i.RS()));

            int ds = (int)(short)(ushort)(i.DS() << 2);
            if (i.RA() != 0)
                stat.Op1 = new CStatement.COperand(RegName(i.RA()), ds);
            else
            {
                stat.Op1 = new CStatement.COperand();
                stat.Op1.Kind = CStatement.OperandKinds.AddressPointer;
                stat.Op1.Offset = ds;
            }

            List<CStatement> stats = new List<CStatement>();
            stats.Add(stat);
            return stats;
        }

        static List<CStatement> Sth_C(uint pc, uint instruction)
        {
            Instruction i = new Instruction(instruction);

            CStatement stat = new CStatement(CStatement.Kinds.Assignment);
            stat.OperandSizes = CStatement.Sizes.Short;

            if (i.RS() == 0)
                stat.Op2 = new CStatement.COperand(0);
            else
                stat.Op2 = new CStatement.COperand(RegName(i.RS()));

            int ds = (int)i.SIMM();
            if (i.RA() != 0)
                stat.Op1 = new CStatement.COperand(RegName(i.RA()), ds);
            else
            {
                stat.Op1 = new CStatement.COperand();
                stat.Op1.Kind = CStatement.OperandKinds.AddressPointer;
                stat.Op1.Offset = ds;
            }

            List<CStatement> stats = new List<CStatement>();
            stats.Add(stat);
            return stats;
        }

        static List<CStatement> Stw_C(uint pc, uint instruction)
        {
            Instruction i = new Instruction(instruction);

            CStatement stat = new CStatement(CStatement.Kinds.Assignment);
            stat.OperandSizes = CStatement.Sizes.Int;

            if (i.RS() == 0)
                stat.Op2 = new CStatement.COperand(0);
            else
                stat.Op2 = new CStatement.COperand(RegName(i.RS()));

            int ds = (int)i.SIMM();
            if (i.RA() != 0)
                stat.Op1 = new CStatement.COperand(RegName(i.RA()), ds);
            else
            {
                stat.Op1 = new CStatement.COperand();
                stat.Op1.Kind = CStatement.OperandKinds.AddressPointer;
                stat.Op1.Offset = ds;
            }

            List<CStatement> stats = new List<CStatement>();
            stats.Add(stat);
            return stats;
        }

        static List<CStatement> Stwu_C(uint pc, uint instruction)
        {
            Instruction i = new Instruction(instruction);

            List<CStatement> stats = Stw_C(pc, instruction);
            if (i.SIMM() == 0)
                return stats;

            CStatement add = new CStatement(CStatement.Kinds.Addition, RegName(i.RA()), (ulong)(long)i.SIMM());
            CStatement stat = new CStatement(CStatement.Kinds.Assignment, RegName(i.RA()), add);
            stat.OperandSizes = CStatement.Sizes.Int;
           
            stats.Add(stat);
            return stats;
        }

        static List<CStatement> Subf_C(uint pc, uint instruction)
        {
            Instruction i = new Instruction(instruction);

            CStatement stat = new CStatement(CStatement.Kinds.Subtraction, RegName(i.RB()), RegName(i.RA()));
            CStatement ass = new CStatement(CStatement.Kinds.Assignment, RegName(i.RD()), stat);

            List<CStatement> stats = new List<CStatement>();
            stats.Add(ass);

            if (i.OE())
                MessageBox.Show("This instruction sets the O bit. I don't know how to translate it! Ignoring.");

            if (i.RC())
                stats.Add(new CStatement(CStatement.Kinds.Assignment, "cr0", RegName(i.RD())));

            return stats;
        }

        static List<CStatement> Twi_C(uint pc, uint instruction)
        {
            Instruction i = new Instruction(instruction);

            CStatement stat = new CStatement(CStatement.Kinds.Throw);

            if (i.TO() != 31)
                throw new Exception("Can't handle complex twi");

            List<CStatement> stats = new List<CStatement>();
            stats.Add(stat);

            return stats;
        }

        static int SignExtend24(uint val)
        {
            int v = (int)(val << 8);
            v >>= 8;
            return v;
        }
    }
}
