using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X360Decompiler
{
    public class XPeParser
    {
        public String FileName;

        const int IMAGE_DOS_SIGNATURE = 0x5A4D;
        const int IMAGE_NT_SIGNATURE = 0x00004550;
        const int PE_MAGIC_32 = 0x10B;
        const int PE_MAGIC_64 = 0x20B;

        struct ImageFileHeader
        {
            public ImageFileHeader(BinaryReader br)
            {
                Machine = br.ReadUInt16();
                NumberOfSections = br.ReadUInt16();
                TimeDateStamp = br.ReadUInt32();
                PointerToSymbolTable = br.ReadUInt32();
                NumberOfSymbols = br.ReadUInt32();
                SizeOfOptionalHeader = br.ReadUInt16();
                Characteristics = br.ReadUInt16();
            }

            public ushort Machine;
            public ushort NumberOfSections;
            public uint TimeDateStamp;
            public uint PointerToSymbolTable;
            public uint NumberOfSymbols;
            public ushort SizeOfOptionalHeader;
            public ushort Characteristics;
        }

        public struct OptionalHeader
        {
            public OptionalHeader(BinaryReader br)
            {
                Magic = br.ReadUInt16();
                MajorLinkerVersion = br.ReadByte();
                MinorLinkerVersion = br.ReadByte();
                SizeOfCode = br.ReadUInt32();
                SizeOfInitializedData = br.ReadUInt32();
                SizeOfUninitializedData = br.ReadUInt32();
                AddressOfEntryPoint = br.ReadUInt32();
                BaseOfCode = br.ReadUInt32();

                if (Magic == PE_MAGIC_32)
                {
                    BaseOfData = br.ReadUInt32();
                    ImageBase = br.ReadUInt32();
                }
                else
                {
                    ImageBase = br.ReadUInt64();
                    BaseOfData = 0;
                }

                SectionAlignment = br.ReadUInt32();
                FileAlignment = br.ReadUInt32();

                MajorOperatingSystemVersion = br.ReadUInt16();
                MinorOperatingSystemVersion = br.ReadUInt16();
                MajorImageVersion = br.ReadUInt16();
                MinorImageVersion = br.ReadUInt16();
                MajorSubsystemVersion = br.ReadUInt16();
                MinorSubsystemVersion = br.ReadUInt16();
                Win32VersionValue = br.ReadUInt32();
                SizeOfImage = br.ReadUInt32();
                SizeOfHeaders = br.ReadUInt32();
                CheckSum = br.ReadUInt32();
                Subsystem = br.ReadUInt16();
                DllCharacteristics = br.ReadUInt16();

                if (Magic == PE_MAGIC_32)
                {
                    SizeOfStackReserve = br.ReadUInt32();
                    SizeOfStackCommit = br.ReadUInt32();
                    SizeOfHeapReserve = br.ReadUInt32();
                    SizeOfHeapCommit = br.ReadUInt32();
                }
                else
                {
                    SizeOfStackReserve = br.ReadUInt64();
                    SizeOfStackCommit = br.ReadUInt64();
                    SizeOfHeapReserve = br.ReadUInt64();
                    SizeOfHeapCommit = br.ReadUInt64();
                }

                LoaderFlags = br.ReadUInt32();
                NumberOfRvaAndSizes = br.ReadUInt32();

                RvaSizes = new RvaSize[NumberOfRvaAndSizes];
                for (int i = 0; i < NumberOfRvaAndSizes; i++)
                {
                    RvaSizes[i].Rva = br.ReadUInt32();
                    RvaSizes[i].Size = br.ReadUInt32();
                }
            }

            public ushort Magic;
            public byte MajorLinkerVersion;
            public byte MinorLinkerVersion;
            public uint SizeOfCode;
            public uint SizeOfInitializedData;
            public uint SizeOfUninitializedData;
            public uint AddressOfEntryPoint;
            public uint BaseOfCode;
            public uint BaseOfData;
            public ulong ImageBase;
            public uint SectionAlignment;
            public uint FileAlignment;
            public ushort MajorOperatingSystemVersion;
            public ushort MinorOperatingSystemVersion;
            public ushort MajorImageVersion;
            public ushort MinorImageVersion;
            public ushort MajorSubsystemVersion;
            public ushort MinorSubsystemVersion;
            public uint Win32VersionValue;
            public uint SizeOfImage;
            public uint SizeOfHeaders;
            public uint CheckSum;
            public ushort Subsystem;
            public ushort DllCharacteristics;
            public ulong SizeOfStackReserve;
            public ulong SizeOfStackCommit;
            public ulong SizeOfHeapReserve;
            public ulong SizeOfHeapCommit;
            public uint LoaderFlags;
            public uint NumberOfRvaAndSizes;

            public struct RvaSize
            {
                public uint Rva;
                public uint Size;
            };

            RvaSize[] RvaSizes;
        }

        public class SectionHeader
        {
            public SectionHeader()
            {
            }

            public SectionHeader(BinaryReader br)
            {
                Read(br);
            }

            public void Read(BinaryReader br)
            {
                Name = br.ReadChars(8);
                VirtualSize = br.ReadUInt32();
                VirtualAddress = br.ReadUInt32();
                SizeOfRawData = br.ReadUInt32();
                PointerToRawData = br.ReadUInt32();
                PointerToRelocations = br.ReadUInt32();
                PointerToLinenumbers = br.ReadUInt32();
                NumberOfRelocations = br.ReadUInt16();
                NumberOfLinenumbers = br.ReadUInt16();
                Characteristics = br.ReadUInt32();
            }

            public char[] Name = null;
            public uint VirtualSize = 0;
            public uint VirtualAddress = 0;
            public uint SizeOfRawData = 0;
            public uint PointerToRawData = 0;
            public uint PointerToRelocations = 0;
            public uint PointerToLinenumbers = 0;
            public ushort NumberOfRelocations = 0;
            public ushort NumberOfLinenumbers = 0;
            public uint Characteristics = 0;
        }

        ImageFileHeader imHdr;
        public OptionalHeader optHdr;
        SectionHeader[] sectHdrs;
        FileStream f;

        public XPeParser(String path)
        {
            f = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            BinaryReader br = new BinaryReader(f);

            /* Check the main signature */
            ushort dosSig = br.ReadUInt16();
            if (dosSig != IMAGE_DOS_SIGNATURE)
        		throw new Exception("Invalid DOS Signature for the executable!");

	        /* Seek to the PE header */
	        br.BaseStream.Seek(0x3C, SeekOrigin.Begin);
	        uint peOffset = br.ReadUInt32();

        	/* Check the PE signature */
	        br.BaseStream.Seek(peOffset, SeekOrigin.Begin);

            uint peSign = br.ReadUInt32();
        	if (peSign != IMAGE_NT_SIGNATURE)
		        throw new Exception("Invalid PE Signature for the executable!");

            /* Read the rest of PE header, then check some fields */
            imHdr = new ImageFileHeader(br);

            /* Specific machine value to indicate Xbox 360 */
            if (imHdr.Machine != 0x1F2)
                throw new Exception("The PE is valid but it's not an Xbox 360 executable.");

	        /*
	         * The strange thing is that imHdr.Characteristics is set to 0x102
	         * (usually), indicating a 32bit architecture, while the Xenon is 64-bit.
	         * Microsoft likes to mess with their own specifications...
	         */
            optHdr = new OptionalHeader(br);

            sectHdrs = new SectionHeader[imHdr.NumberOfSections];
            for (int i = 0; i < imHdr.NumberOfSections; i++)
                sectHdrs[i] = new SectionHeader(br);

            FileName = path;
        }

        public uint Rva2Offset(uint rva)
        {
            for (int i = 0; i < imHdr.NumberOfSections; i++)
            {
                if (sectHdrs[i].VirtualAddress <= rva && sectHdrs[i].VirtualAddress + sectHdrs[i].VirtualSize > rva)
                {
                    return sectHdrs[i].PointerToRawData + (rva - sectHdrs[i].VirtualAddress);
                }
            }

            return 0;
        }

        public uint Rva2SectionEnd(uint rva)
        {
            for (int i = 0; i < imHdr.NumberOfSections; i++)
            {
                if (sectHdrs[i].VirtualAddress <= rva && sectHdrs[i].VirtualAddress + sectHdrs[i].VirtualSize > rva)
                    return sectHdrs[i].VirtualAddress + sectHdrs[i].VirtualSize;
            }

            return 0;
        }

        public uint ReadInstruction(uint offset)
        {
            f.Seek(offset, SeekOrigin.Begin);
            BinaryReader br = new BinaryReader(f);
            uint res = br.ReadUInt32();
            
            byte[] temp = BitConverter.GetBytes(res);
            Array.Reverse(temp);
            return BitConverter.ToUInt32(temp, 0);
        }

        public ulong GetImageBase()
        {
            return optHdr.ImageBase;
        }

        public SectionHeader GetSection(int section)
        {
            if (section < sectHdrs.Count())
                return sectHdrs[section];
            return null;
        }

        public SectionHeader GetSectionByAddress(uint address)
        {
            foreach (SectionHeader s in sectHdrs)
            {
                if (address >= s.VirtualAddress && address < s.VirtualAddress + s.VirtualSize)
                    return s;
            }

            return null;
        }
    }
}
