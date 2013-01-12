using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X360Decompiler
{
    public class Function
    {
        public Function(State decState, String name, uint addr)
        {
            Name = name;
            Address = addr;
            Size = 0xFFFFFFFF;

            decompiler = decState;
        }

        private State decompiler;

        public String Name;
        public uint Address;
        public int ArgCount = -1;
        public List<Argument> Arguments = new List<Argument>();
        public uint Size;
        public List<FunctionBlock> Blocks = null;
        public List<Loop> Loops = null;

        public MainWindow.ListViewFunction ListViewEntry;

        public uint FindSize()
        {
            /* Functions can't cross each other, so the maximum size is on the next function boundary */
            uint addr = Address - (uint)decompiler.Pe.GetImageBase();
            uint diff;

            int index = decompiler.Functions.IndexOf(this);
            if (index != decompiler.Functions.Count - 1)
                diff = decompiler.Functions[index + 1].Address - Address;
            else
                diff = decompiler.Pe.Rva2SectionEnd(addr) - addr;

            diff = (uint)(diff & (~3));

            /* Check that all instructions are valid, and stop at the first invalid instruction (if any) */
            uint i = 0;
            uint offset = decompiler.Pe.Rva2Offset(addr);
            for (; i < diff / 4; i++)
            {
                uint instruction = decompiler.Pe.ReadInstruction(offset + i * 4);
                XenonInstructions.OpCodeInfo info = decompiler.Instructions.GetInfo(instruction);
                if (info.Id == XenonInstructions.Mnemonics.PPC_OP_INVALID)
                    break;
            }
            diff = i * 4;

            /* Build a call tree and find the maximum address */
            BuildCallTree(Address, diff);

            FunctionBlock lastBlock = Blocks[Blocks.Count - 1];
            uint maxAddr = lastBlock.StartAddress + lastBlock.InstrCount * 4;

            return maxAddr - Address;
        }

        void BuildCallTree(uint address, uint maxLen)
        {
            Blocks = new List<FunctionBlock>();

            FunctionBlock f1 = new FunctionBlock();
            f1.StartAddress = address;
            f1.InstrCount = maxLen / 4;
            Blocks.Add(f1);

            while (true)
            {
                int toAnalyze = -1;
                for (int i = 0; i < Blocks.Count; i++)
                {
                    if (!Blocks[i].Analyzed)
                    {
                        toAnalyze = i;
                        break;
                    }
                }

                if (toAnalyze == -1)
                    break;

                AnalyzeBlock(Blocks[toAnalyze], address, address + maxLen);
            }

            AddLazyCalls();
        }

        void AnalyzeBlock(FunctionBlock block, uint minFunctionAddr, uint maxFunctionAddr)
        {
            block.Analyzed = true;

            uint rva = block.StartAddress - (uint)decompiler.Pe.GetImageBase();
            uint offset = decompiler.Pe.Rva2Offset(rva);

            bool cont = true;
            for (uint i = 0; i < block.InstrCount && cont; i++)
            {
                uint instruction = decompiler.Pe.ReadInstruction(offset + i * 4);
                XenonInstructions.OpCodeInfo info = decompiler.Instructions.GetInfo(instruction);
                XenonInstructions.Instruction instr = new XenonInstructions.Instruction(instruction);

                /* A branch signals end of block. However a branch with link is just a call, and it doesn't end a block */
                uint pc = block.StartAddress + i * 4;

                if (instr.LK())
                    continue;

                switch (info.Id)
                {
                    case XenonInstructions.Mnemonics.PPC_OP_B:
                        {
                            uint npc = (uint)SignExtend26(instr.LI() << 2);
                            if (!instr.AA())
                                npc += pc;

                            AddBlock(block, npc, minFunctionAddr, maxFunctionAddr);
                            block.InstrCount = i + 1;
                            cont = false;

                            break;
                        }
                    case XenonInstructions.Mnemonics.PPC_OP_BC:
                        {
                            uint npc = (uint)(int)(short) (instr.BD() << 2);
                            if (!instr.AA())
                                npc += pc;

                            AddBlock(block, npc, minFunctionAddr, maxFunctionAddr);
                            AddBlock(block, pc + 4, minFunctionAddr, maxFunctionAddr);
                            block.InstrCount = i + 1;
                            cont = false;

                            break;
                        }
                    case XenonInstructions.Mnemonics.PPC_OP_BCCTR:
                    case XenonInstructions.Mnemonics.PPC_OP_BCLR:
                        {
                            if (instr.BO() != 20)
                                AddBlock(block, pc + 4, minFunctionAddr, maxFunctionAddr);

                            block.InstrCount = i + 1;
                            cont = false;
                            break;
                        }
                }
            }

            CleanupBlockList();
        }

        void AddBlock(FunctionBlock callerBlock, uint npc, uint minFunctionAddr, uint maxFunctionAddr)
        {
            if (npc >= minFunctionAddr && npc < maxFunctionAddr)
            {
                /* This is an internal branch. Need to add this block if it's not already present */
                foreach (FunctionBlock b in Blocks)
                {
                    if (b.StartAddress == npc)
                    {
                        callerBlock.Successors.Add(b);
                        b.Predecessors.Add(callerBlock);
                        return;
                    }
                }

                /* Not present. Create a new block and append to list. */
                FunctionBlock fb = new FunctionBlock();
                fb.StartAddress = npc;
                fb.Analyzed = false;
                if (npc < callerBlock.StartAddress)
                    fb.InstrCount = (callerBlock.StartAddress - npc) / 4;
                else
                    fb.InstrCount = (maxFunctionAddr - npc) / 4;

                Blocks.Add(fb);
                callerBlock.Successors.Add(fb);
                fb.Predecessors.Add(callerBlock);
            }
        }

        void CleanupBlockList()
        {
            Blocks.Sort(FunctionBlockSorter);
            
            for (int i = 0; i < Blocks.Count - 1; i++)
            {
                if (Blocks[i].StartAddress + Blocks[i].InstrCount * 4 >= Blocks[i + 1].StartAddress)
                    Blocks[i].InstrCount = (Blocks[i + 1].StartAddress - Blocks[i].StartAddress) / 4;
            }
        }

        void AddLazyCalls()
        {
            for (int i = 0; i < Blocks.Count - 1; i++)
            {
                uint lastInstructionAddr = Blocks[i].StartAddress + (Blocks[i].InstrCount - 1) * 4;
                uint rva = lastInstructionAddr - (uint)decompiler.Pe.GetImageBase();
                uint offset = decompiler.Pe.Rva2Offset(rva);

                uint instruction = decompiler.Pe.ReadInstruction(offset);
                XenonInstructions.OpCodeInfo info = decompiler.Instructions.GetInfo(instruction);
                switch (info.Id)
                {
                    case XenonInstructions.Mnemonics.PPC_OP_B:
                    case XenonInstructions.Mnemonics.PPC_OP_BC:
                    case XenonInstructions.Mnemonics.PPC_OP_BCCTR:
                    case XenonInstructions.Mnemonics.PPC_OP_BCLR:
                        break;
                    default:
                        Blocks[i].Successors.Add(Blocks[i + 1]);
                        Blocks[i + 1].Predecessors.Add(Blocks[i]);
                        break;
                }
            }
        }

        public static int FunctionBlockSorter(FunctionBlock f1, FunctionBlock f2)
        {
            return (int)f1.StartAddress - (int)f2.StartAddress;
        }

        static int SignExtend26(uint val)
        {
            int v = (int)(val << 6);
            v >>= 6;
            return v;
        }
    }
}
