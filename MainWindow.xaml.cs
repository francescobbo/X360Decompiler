using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using PdbParser;

namespace X360Decompiler
{
    public class FunctionBlock
    {
        public uint StartAddress;
        public uint InstrCount;
        public bool Analyzed = false;
        public List<FunctionBlock> Successors = new List<FunctionBlock>();
        public List<FunctionBlock> Predecessors = new List<FunctionBlock>();
        public List<CStatement> Statements = new List<CStatement>();
        public int Id;
        public BitArray Dominators;

        public List<String> Defines;
        public List<String> Uses;

        public List<String> Input;
        public List<String> Output;

        public Dictionary<String, CStatement> SavedVars = new Dictionary<string,CStatement>();
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        XPeParser pe;
        PdbParser.PdbParser pdb;
        XenonInstructions instrs = new XenonInstructions();

        public class Function
        {
            public Function()
            {
                Name = "";
                Address = 0;
                Size = 0;
            }

            public String Name;
            public uint Address;
            public uint Size;
            public List<FunctionBlock> Blocks = null;
            public List<Loop> Loops = null;
        }

        List<Function> Functions = new List<Function>();

        public class ListViewFunction
        {
            public ListViewFunction(string name, string addr)
            {
                FuncName = name;
                FuncAddr = addr;
            }

            public string FuncName { get; set; }
            public string FuncAddr { get; set; }
        }

        public class ListViewInstr
        {
            public ListViewInstr(string address, string mnemonic, string parameters)
            {
                Address = address;
                Mnemonic = mnemonic;
                Parameters = parameters;
            }

            public string Address { get; set; }
            public string Mnemonic { get; set; }
            public string Parameters { get; set; }
        }

        ObservableCollection<ListViewFunction> _FuncCollection = new ObservableCollection<ListViewFunction>();
        public ObservableCollection<ListViewFunction> FuncCollection { get { return _FuncCollection; } }

        ObservableCollection<ListViewInstr> _InstrCollection = new ObservableCollection<ListViewInstr>();
        public ObservableCollection<ListViewInstr> InstrCollection { get { return _InstrCollection; } }

        public MainWindow()
        {
            InitializeComponent();
            instrs.SetupTables();
        }

        public int FunctionAddressComparison(Function f1, Function f2)
        {
            return ((int) f1.Address - (int) f2.Address);
        }

        private void OpenExe(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Filter = "Executable files (*.exe)|*.exe";

            bool? res = ofd.ShowDialog();
            if (res.HasValue && res == true)
            {
                pe = new XPeParser(ofd.FileName);
                PdbMenu.IsEnabled = true;
            }
        }

        private void OpenPdb(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Filter = "Program Database (*.pdb)|*.pdb";

            bool? res = ofd.ShowDialog();
            if (res.HasValue && res == true)
            {
                pdb = new PdbParser.PdbParser(ofd.FileName);
                List<Symbol> syms = pdb.DumpAllPublics();

                foreach (Symbol s in syms)
                {
                    UInt32 addr = s.Rva + (uint) pe.optHdr.ImageBase;
                    String addrStr = "0x" + addr.ToString("X8");
                    _FuncCollection.Add(new ListViewFunction(s.Name, addrStr));

                    Function f = new Function();
                    f.Name = s.Name;
                    f.Address = addr;
                    f.Size = 0xFFFFFFFF;

                    Functions.Add(f);
                }

                Functions.Sort(FunctionAddressComparison);
            }
        }

        class FunctionsNameSorter : IComparer
        {
            public int Compare(object a, object b)
            {
                ListViewFunction f1 = (ListViewFunction)a, f2 = (ListViewFunction)b;
                return f1.FuncName.CompareTo(f2.FuncName);
            }
        }

        class FunctionsAddrSorter : IComparer
        {
            public int Compare(object a, object b)
            {
                ListViewFunction f1 = (ListViewFunction)a, f2 = (ListViewFunction)b;
                return f1.FuncAddr.CompareTo(f2.FuncAddr);
            }
        }

        IComparer nameSorter = new FunctionsNameSorter();
        IComparer addrSorter = new FunctionsAddrSorter();

        private void SortClick(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader hdr = (GridViewColumnHeader)sender;
            ListCollectionView view = (ListCollectionView)CollectionViewSource.GetDefaultView(FunctionsView.ItemsSource);

            if ((String) hdr.Content == "Function")
                view.CustomSort = nameSorter;
            else
                view.CustomSort = addrSorter;

            FunctionsView.Items.Refresh();
        }

        private void DisassembleFunction(object sender, RoutedEventArgs e)
        {
            ListViewFunction item = (ListViewFunction)FunctionsView.SelectedItem;
            uint addr = UInt32.Parse(item.FuncAddr.Substring(2), System.Globalization.NumberStyles.AllowHexSpecifier);
            
            int funcIndex = Functions.FindIndex(delegate(Function f) { return f.Address == addr; });
            if (funcIndex < 0)
                return;

            if (Functions[funcIndex].Size == 0xFFFFFFFF || Functions[funcIndex].Blocks == null)
            {
                Functions[funcIndex].Size = FindFunctionSize(funcIndex);
                FindDominators(Functions[funcIndex].Blocks);
            }

            _InstrCollection.Clear();
            foreach (FunctionBlock block in Functions[funcIndex].Blocks)
            {
                uint offset = pe.Rva2Offset(block.StartAddress - (uint)pe.optHdr.ImageBase);

                for (uint i = 0; i < block.InstrCount; i++)
                {
                    uint instruction = pe.ReadInstruction(offset + i * 4);

                    XenonInstructions.OpCodeInfo info = instrs.GetInfo(instruction);
                    ListViewInstr instr = new ListViewInstr("0x" + (block.StartAddress + i * 4).ToString("X8"), info.Name, "");
                    _InstrCollection.Add(instr);
                }
            }
        }
        
        public uint FindFunctionSize(int funcIndex)
        {
            /* Functions can't cross each other, so the maximum size is on the next function boundary */
            uint addr = Functions[funcIndex].Address - (uint)pe.optHdr.ImageBase;
            uint diff;
            if (funcIndex != Functions.Count - 1)
                diff = Functions[funcIndex + 1].Address - Functions[funcIndex].Address;
            else
                diff = pe.Rva2SectionEnd(addr) - addr;

            diff = (uint) (diff & (~3));

            /* Check that all instructions are valid, and stop at the first invalid instruction (if any) */
            uint i = 0;
            uint offset = pe.Rva2Offset(addr);
            for (; i < diff / 4; i++)
            {
                uint instruction = pe.ReadInstruction(offset + i * 4);
                XenonInstructions.OpCodeInfo info = instrs.GetInfo(instruction);
                if (info.Id == XenonInstructions.Mnemonics.PPC_OP_INVALID)
                    break;
            }
            diff = i * 4;

            /* Build a call tree and find the maximum address */
            List<FunctionBlock> fb = BuildCallTree(Functions[funcIndex].Address, diff);
            Functions[funcIndex].Blocks = fb;

            FunctionBlock lastBlock = fb[fb.Count - 1];
            uint maxAddr = lastBlock.StartAddress + lastBlock.InstrCount * 4;

            return maxAddr - Functions[funcIndex].Address;
        }

        List<FunctionBlock> BuildCallTree(uint address, uint maxLen)
        {
            List<FunctionBlock> ret = new List<FunctionBlock>();

            FunctionBlock f1 = new FunctionBlock();
            f1.StartAddress = address;
            f1.InstrCount = maxLen / 4;
            ret.Add(f1);

            while (true)
            {
                int toAnalyze = -1;
                for (int i = 0; i < ret.Count; i++)
                {
                    if (!ret[i].Analyzed)
                    {
                        toAnalyze = i;
                        break;
                    }
                }

                if (toAnalyze == -1)
                    break;

                AnalyzeBlock(ret, toAnalyze, address, address + maxLen);
            }

            AddLazyCalls(ret);

            return ret;
        }

        void AnalyzeBlock(List<FunctionBlock> blocks, int curBlock, uint minFunctionAddr, uint maxFunctionAddr)
        {
            blocks[curBlock].Analyzed = true;

            uint rva = blocks[curBlock].StartAddress - (uint)pe.optHdr.ImageBase;
            uint offset = pe.Rva2Offset(rva);

            bool cont = true;
            for (uint i = 0; i < blocks[curBlock].InstrCount && cont; i++)
            {
                uint instruction = pe.ReadInstruction(offset + i * 4);
                XenonInstructions.OpCodeInfo info = instrs.GetInfo(instruction);

                /* A branch signals end of block. However a branch with link is just a call, and it doesn't end a block */
                uint npc;
                uint pc = blocks[curBlock].StartAddress + i * 4;

                switch (info.Id)
                {
                    case XenonInstructions.Mnemonics.PPC_OP_B:
                        {
                            if ((instruction & 1) == 1)     // LK
                                continue;

                            if ((instruction & 2) == 2)     // AA
                                npc = (uint)SignExtend26(instruction & 0x3FFFFFC);
                            else
                                npc = pc + (uint)SignExtend26(instruction & 0x3FFFFFC);

                            AddBlock(blocks, curBlock, npc, minFunctionAddr, maxFunctionAddr);
                            blocks[curBlock].InstrCount = i + 1;
                            cont = false;

                            break;
                        }
                    case XenonInstructions.Mnemonics.PPC_OP_BC:
                        {
                            if ((instruction & 1) == 1)    // LK
                                continue;

                            short bd = (short)(instruction & 0xFFFC);

                            if ((instruction & 2) == 2)     // AA
                                npc = (uint)(int)bd;
                            else
                                npc = (uint)(pc + bd);

                            uint npc2 = pc + 4;
                            AddBlock(blocks, curBlock, npc, minFunctionAddr, maxFunctionAddr);
                            AddBlock(blocks, curBlock, npc2, minFunctionAddr, maxFunctionAddr);
                            blocks[curBlock].InstrCount = i + 1;
                            cont = false;

                            break;
                        }
                    case XenonInstructions.Mnemonics.PPC_OP_BCCTR:
                        {
                            if ((instruction & 1) == 1)    // LK
                                continue;

                            if (((instruction >> 21) & 0x1F) != 20)
                            {
                                uint npc2 = pc + 4;
                                AddBlock(blocks, curBlock, npc2, minFunctionAddr, maxFunctionAddr);
                            }

                            blocks[curBlock].InstrCount = i + 1;
                            cont = false;
                            break;
                        }
                    case XenonInstructions.Mnemonics.PPC_OP_BCLR:
                        {
                            if ((instruction & 1) == 1)    // LK
                                continue;

                            if (((instruction >> 21) & 0x1F) != 20)
                            {
                                uint npc2 = pc + 4;
                                AddBlock(blocks, curBlock, npc2, minFunctionAddr, maxFunctionAddr);
                            }

                            blocks[curBlock].InstrCount = i + 1;
                            cont = false;
                            break;
                        }
                }
            }

            CleanupBlockList(blocks);
        }

        void AddBlock(List<FunctionBlock> blocks, int curBlock, uint npc, uint minFunctionAddr, uint maxFunctionAddr)
        {
            if (npc >= minFunctionAddr && npc < maxFunctionAddr)
            {
                /* This is an internal branch. Need to add this block if it's not already present */
                bool found = false;
                for (int j = 0; j < blocks.Count; j++)
                {
                    if (blocks[j].StartAddress == npc)
                    {
                        found = true;
                        blocks[curBlock].Successors.Add(blocks[j]);
                        blocks[j].Predecessors.Add(blocks[curBlock]);
                        break;
                    }
                }

                if (!found)
                {
                    FunctionBlock fb = new FunctionBlock();
                    fb.StartAddress = npc;
                    fb.Analyzed = false;
                    if (npc < blocks[curBlock].StartAddress)
                        fb.InstrCount = (blocks[curBlock].StartAddress - npc) / 4;
                    else
                        fb.InstrCount = (maxFunctionAddr - npc) / 4;

                    blocks.Add(fb);
                    blocks[curBlock].Successors.Add(fb);
                    fb.Predecessors.Add(blocks[curBlock]);
                }
            }
        }

        void AddLazyCalls(List<FunctionBlock> blocks)
        {
            for (int i = 0; i < blocks.Count - 1; i++)
            {
                uint lastInstructionAddr = blocks[i].StartAddress + (blocks[i].InstrCount - 1) * 4;
                uint rva = lastInstructionAddr - (uint)pe.optHdr.ImageBase;
                uint offset = pe.Rva2Offset(rva);

                uint instruction = pe.ReadInstruction(offset);
                XenonInstructions.OpCodeInfo info = instrs.GetInfo(instruction);
                switch (info.Id)
                {
                    case XenonInstructions.Mnemonics.PPC_OP_B:
                    case XenonInstructions.Mnemonics.PPC_OP_BC:
                    case XenonInstructions.Mnemonics.PPC_OP_BCCTR:
                    case XenonInstructions.Mnemonics.PPC_OP_BCLR:
                        break;
                    default:
                        blocks[i].Successors.Add(blocks[i + 1]);
                        blocks[i + 1].Predecessors.Add(blocks[i]);
                        break;
                }
            }
        }

        int FunctionBlockSorter(FunctionBlock f1, FunctionBlock f2)
        {
            return (int) f1.StartAddress - (int) f2.StartAddress; 
        }

        void CleanupBlockList(List<FunctionBlock> blocks)
        {
            blocks.Sort(FunctionBlockSorter);
            for (int i = 0; i < blocks.Count - 1; i++)
            {
                if (blocks[i].StartAddress + blocks[i].InstrCount * 4 >= blocks[i + 1].StartAddress)
                    blocks[i].InstrCount = (blocks[i + 1].StartAddress - blocks[i].StartAddress) / 4;
            }
        }

        int SignExtend26(uint val)
        {
            int v = (int)(val << 6);
            v >>= 6;
            return v;
        }

        private void DecompileFunction(object sender, RoutedEventArgs e)
        {
            ListViewFunction item = (ListViewFunction)FunctionsView.SelectedItem;
            uint addr = UInt32.Parse(item.FuncAddr.Substring(2), System.Globalization.NumberStyles.AllowHexSpecifier);

            int funcIndex = Functions.FindIndex(delegate(Function f) { return f.Address == addr; });
            if (funcIndex < 0)
                return;

            if (Functions[funcIndex].Size == 0xFFFFFFFF || Functions[funcIndex].Blocks == null)
            {
                MessageBox.Show("Disassemble first before decompiling.");
                return;
            }

            Functions[funcIndex].Loops = FindLoops(Functions[funcIndex].Blocks);
            DecompileEasy(funcIndex);
            ComputeUseDefs(Functions[funcIndex].Blocks);
            PropagateExpressions(Functions[funcIndex].Blocks);
            LoopPhase2(Functions[funcIndex].Blocks, Functions[funcIndex].Loops);
            IfElse(Functions[funcIndex].Blocks);

            List<CStatement> statements = new List<CStatement>();
            foreach (FunctionBlock b in Functions[funcIndex].Blocks)
                statements.AddRange(b.Statements);

            string csource = "void " + Functions[funcIndex].Name + "()\n{\n";
            foreach (CStatement stat in statements)
            {
                csource += "\t";
                csource += stat.ToString(1);
                if (stat.Kind != CStatement.Kinds.Conditional)
                    csource += ";\n";
                else
                    csource += "\n";
            }
            csource += "}";

            CSource src = new CSource(csource);
            src.Show();
        }

        private void FindDominators(List<FunctionBlock> blocks)
        {
            int nBlocks = blocks.Count;
            int i = 0;
 
            foreach (FunctionBlock b in blocks)
            {
                b.Id = i++;
                b.Dominators = new BitArray(nBlocks);
                b.Dominators.SetAll(true);
            }

            blocks[0].Dominators.SetAll(false);
            blocks[0].Dominators.Set(blocks[0].Id, true);
 
            BitArray tmp = new BitArray(nBlocks);

            bool changed;
            do
            {
                changed = false;
 
                foreach (FunctionBlock b in blocks)
                {
                    foreach (FunctionBlock pred in b.Predecessors)
                    {
                        tmp.SetAll(false);
                        tmp.Or(b.Dominators);
                        b.Dominators.And(pred.Dominators);
                        b.Dominators.Set(b.Id, true);
                        if (!BitArrayEqual(b.Dominators, tmp))
                            changed = true;
                    }
                }
            } while (changed);
        }

        public class Loop
        {
            public FunctionBlock Header = null;
            public FunctionBlock LoopBlock = null;
            public List<FunctionBlock> Blocks = new List<FunctionBlock>();

            public enum LoopKind
            {
                DoWhile, While, For
            }

            public LoopKind Kind = LoopKind.DoWhile;
        }

        private Loop NaturalLoopForEdge(FunctionBlock header, FunctionBlock tail)
        {
            Stack<FunctionBlock> workList = new Stack<FunctionBlock>();
            Loop loop;
 
            loop = new Loop();
            loop.Header = header;
            loop.Blocks.Add(header);
 
            if (header != tail)
            {
                loop.Blocks.Add(tail);
                workList.Push(tail);
            }

            while (workList.Count != 0)
            {
                FunctionBlock b = workList.Pop();
                foreach (FunctionBlock pred in b.Predecessors)
                {
                    if (loop.Blocks.IndexOf(pred) < 0)
                    {
                        loop.Blocks.Add(pred);
                        workList.Push(pred);
                    }
                }
            }

            loop.Blocks.Sort(FunctionBlockSorter);
            return loop;
        }
 
        private List<Loop> FindLoops(List<FunctionBlock> blocks)
        {
            List<Loop> loopSet = new List<Loop>();
 
            foreach (FunctionBlock b in blocks)
            {
                foreach (FunctionBlock succ in b.Successors)
                {
                    /* If a successor dominates a predecessor, it's a loop */
                    if (b.Dominators[succ.Id])
                        loopSet.Add(NaturalLoopForEdge(succ, b));
                }
            }

            return loopSet;
        }

        private void JoinCmpBc(List<FunctionBlock> blocks)
        {
            foreach (FunctionBlock b in blocks)
            {
                for (int i = 0; i < b.Statements.Count; i++)
                {
                    if (b.Statements[i].Kind != CStatement.Kinds.Conditional)
                        continue;

                    CStatement If = b.Statements[i];

                    CStatement cond = If.Condition;
                    String cr = cond.Op1.Name;

                    /* Take the last cr# = a - b or cr# = a */
                    CStatement cmp = null;
                    int j;
                    int lastj = -1;
                    for (j = 0; j < i; j++)
                    {
                        if (b.Statements[j].Kind == CStatement.Kinds.Assignment && b.Statements[j].Op1.Name == cr &&
                            (b.Statements[j].Op2.Kind == CStatement.OperandKinds.Expression && b.Statements[j].Op2.Expr.Kind == CStatement.Kinds.Subtraction ||
                             b.Statements[j].Op2.Kind == CStatement.OperandKinds.Variable))
                        {
                            cmp = b.Statements[j];
                            lastj = j;
                        }
                    }

                    if (cmp == null)
                        continue;

                    j = lastj;

                    if (b.Statements[j].Op2.Kind == CStatement.OperandKinds.Expression)
                    {
                        CStatement.COperand op1 = b.Statements[j].Op2.Expr.Op1;
                        CStatement.COperand op2 = b.Statements[j].Op2.Expr.Op2;

                        String r1 = op1.Kind == CStatement.OperandKinds.Variable ? op1.Name : null;
                        String r2 = op2.Kind == CStatement.OperandKinds.Variable ? op2.Name : null;

                        /* Check that nor a nor b are written to until the if */
                        bool canRemove = true;
                        for (int k = j + 1; k < i; k++)
                        {
                            if (b.Statements[k].Kind == CStatement.Kinds.Assignment && b.Statements[k].Op1.Kind == CStatement.OperandKinds.Variable)
                            {
                                if (r1 != null && b.Statements[k].Op1.Name == r1)
                                {
                                    canRemove = false;
                                    break;
                                }

                                if (r2 != null && b.Statements[k].Op1.Name == r2)
                                {
                                    canRemove = false;
                                    break;
                                }
                            }
                        }

                        if (!canRemove)
                            continue;

                        If.Condition.Op1 = new CStatement.COperand(b.Statements[j].Op2.Expr);
                        b.Statements.RemoveAt(j);
                    }
                    else
                    {
                        /* cr# = a */
                        String var = b.Statements[j].Op2.Name;
                        
                        bool canRemove = true;
                        for (int k = j + 1; k < i; k++)
                        {
                            if (b.Statements[k].Kind == CStatement.Kinds.Assignment && b.Statements[k].Op1.Kind == CStatement.OperandKinds.Variable)
                            {
                                if (b.Statements[k].Op1.Name == var)
                                {
                                    canRemove = false;
                                    break;
                                }
                            }
                        }

                        if (!canRemove)
                            continue;

                        If.Condition.Op1 = new CStatement.COperand(var);
                        b.Statements.RemoveAt(j);
                    }

                    /* Simplify "a - 0 op 0" => "a op 0", and "a - b op 0" => "a op b" */
                    if (If.Condition.Op1.Kind == CStatement.OperandKinds.Expression)
                    {
                        if (If.Condition.Op1.Expr.Kind == CStatement.Kinds.Subtraction)
                        {
                            if (If.Condition.Op1.Expr.Op2.Kind == CStatement.OperandKinds.Constant &&
                                If.Condition.Op1.Expr.Op2.Value == 0)
                            {
                                If.Condition.Op1 = If.Condition.Op1.Expr.Op1;
                            }
                            else
                            {
                                If.Condition.Op2 = If.Condition.Op1.Expr.Op2;
                                If.Condition.Op1 = If.Condition.Op1.Expr.Op1;
                            }
                        }
                    }
                }
            }
        }

        int LoopSortDesc(Loop l1, Loop l2)
        {
            return (int) l2.Blocks[0].StartAddress - (int) l1.Blocks[0].StartAddress;
        }

        private void LoopPhase2(List<FunctionBlock> blocks, List<Loop> loops)
        {
            /* Take loops in reverse order (inner loops are processed before outer loops */
            loops.Sort(LoopSortDesc);

            foreach (Loop loop in loops)
            {
                FunctionBlock lastBlock = loop.Blocks[loop.Blocks.Count - 1];

                CStatement doWhile = new CStatement();
                doWhile.Kind = CStatement.Kinds.DoWhile;

                doWhile.Condition = FindLastCondition(lastBlock.Statements);
                doWhile.SubBlocks = loop.Blocks;
                doWhile.BreakBlock = blocks[lastBlock.Id + 1];
                doWhile.ContinueBlock = lastBlock;

                doWhile.ExpandBlocks();

                if (doWhile.ContinueBlock.Statements.Count != 1)
                    doWhile.ContinueBlock = null;
                else
                {
                    CStatement stat = doWhile.ContinueBlock.Statements[0];
                    if (stat.Kind != CStatement.Kinds.Conditional)
                        doWhile.ContinueBlock = null;
                    else
                    {
                        if (stat.InnerBlock[0].BranchDestinationAddr != loop.Blocks[0].StartAddress)
                            doWhile.ContinueBlock = null;
                    }
                }

                /* Remove all blocks in the loop from the main block list. It is replaced by a single while block */
                int firstIndex = blocks.IndexOf(loop.Blocks[0]);
                FunctionBlock firstBlock = blocks[firstIndex];
                foreach (FunctionBlock b in loop.Blocks)
                    blocks.Remove(b);

                FunctionBlock fake = new FunctionBlock();
                fake.StartAddress = firstBlock.StartAddress;
                fake.InstrCount = 0;
                fake.Statements.Add(doWhile);

                loop.LoopBlock = fake;
                blocks.Insert(firstIndex, fake);

                StructureBreakContinue(doWhile, doWhile.ContinueBlock, doWhile.BreakBlock);
            }

            foreach (Loop loop in loops)
            {
                FunctionBlock precBlock = blocks.Find(delegate(FunctionBlock b) { return b.Id == loop.Blocks[0].Id - 1; });

                uint precedingBlockStart = precBlock.StartAddress;
                uint lastInstruction = precedingBlockStart + (precBlock.InstrCount - 1) * 4;
                uint offset = pe.Rva2Offset(lastInstruction - (uint)pe.optHdr.ImageBase);
                uint instruction = pe.ReadInstruction(offset);
                XenonInstructions.OpCodeInfo info = instrs.GetInfo(instruction);

                if (info.Id == XenonInstructions.Mnemonics.PPC_OP_B)
                {
                    uint bDest = (instruction & 0xFFFFFF) + lastInstruction;
                    if (bDest == loop.Header.StartAddress)
                    {
                        loop.LoopBlock.Statements[0].Kind = CStatement.Kinds.While;
                        precBlock.InstrCount--;
                        precBlock.Statements.RemoveAt(precBlock.Statements.Count - 1);
                    }
                }
            }

            foreach (Loop loop in loops)
            {
                int cnt = loop.LoopBlock.Statements[0].SubBlocks.Count;
                FunctionBlock last = loop.LoopBlock.Statements[0].SubBlocks[cnt - 1];
                loop.LoopBlock.Statements[0].SubBlocks.RemoveAt(cnt - 1);

                last.Statements.RemoveAt(0);
                if (last.Statements.Count != 0)
                {
                    blocks.Add(last);
                    blocks.Sort(FunctionBlockSorter);
                }

                /* This will have removed some statements, reexpand the blocks */
                loop.LoopBlock.Statements[0].ExpandBlocks();
            }
        }

        void PropagateExpressions(List<FunctionBlock> blocks)
        {
            foreach (FunctionBlock b in blocks)
            {
                foreach (CStatement i in b.Statements)
                {
                    /* On assignment, propagation on Op1 must be done after saving */
                    if (i.Kind != CStatement.Kinds.Assignment)
                        i.Op1 = ReplaceUses(b, i.Op1, i);
                    i.Op2 = ReplaceUses(b, i.Op2, i);

                    if (i.Kind == CStatement.Kinds.Assignment && i.Op1.Kind == CStatement.OperandKinds.Variable)
                    {
                        b.SavedVars[i.Op1.Name] = i;
                        continue;
                    }
                    
                    /* Now propagate on Op1 */
                    if (i.Kind == CStatement.Kinds.Assignment)
                        i.Op1 = ReplaceUses(b, i.Op1, i);
                }
            }
        }

        CStatement.COperand ReplaceUses(FunctionBlock b, CStatement.COperand n, CStatement stat)
        {
            if (n == null)
                return null;

            if (n.Kind == CStatement.OperandKinds.Expression)
            {
                n.Expr.Op1 = ReplaceUses(b, n.Expr.Op1, stat);
                n.Expr.Op2 = ReplaceUses(b, n.Expr.Op2, stat);
            }
            
            if (n.Kind == CStatement.OperandKinds.Variable && b.SavedVars.ContainsKey(n.Name))
            {
                String reg = n.Name;
                if (!stat.LiveOut.Contains(reg))
                {
                    b.SavedVars.Remove(reg);
                    return n;
                }
                n = b.SavedVars[reg].Op2;
            }

            return n;
        }

        private void StructureBreakContinue(CStatement container, FunctionBlock continueBlock, FunctionBlock breakBlock)
        {
            foreach (CStatement stmt in container.InnerBlock)
            {
                switch (stmt.Kind)
                {
                    case CStatement.Kinds.Goto:
                        if (continueBlock && stmt.BranchDestinationAddr == continueBlock.StartAddress)
                            stmt.Kind = CStatement.Kinds.Continue;
                        else if (stmt.BranchDestinationAddr == breakBlock.StartAddress)
                            stmt.Kind = CStatement.Kinds.Break;
                        break;
                    case CStatement.Kinds.Conditional:
                        /* Recursion to subscopes */
                        StructureBreakContinue(stmt, continueBlock, breakBlock);;
                        break;
                    default:
                        /* Do not recurse to do/while, while and for, sinche they have their own continue and break blocks! */
                        break;
                }
            }
        }

        private CStatement FindLastCondition(List<CStatement> stats)
        {
            CStatement If = stats[stats.Count - 1];
            if (If.Kind != CStatement.Kinds.Conditional)
                throw new Exception("Whatever...");

            return If.Condition;
        }

        private void IfElse(List<FunctionBlock> blocks)
        {
            foreach (FunctionBlock b in blocks)
            {
                if (b.Statements[b.Statements.Count - 1].Kind == CStatement.Kinds.Conditional)
                {
                    if (b.Successors.Count != 2)
                        continue;

                    FunctionBlock tb = b.Successors[0];
                    FunctionBlock fb = b.Successors[1];

                    if (tb.Successors.Count != 1 || fb.Successors.Count != 1)
                        continue;

                    if (tb.Successors[0] != fb.Successors[0])
                        continue;
                }
            }
        }

        private static bool BitArrayEqual(BitArray a, BitArray b)
        {
            for (int j = 0; j < a.Count; j++)
            {
                if (a[j] != b[j])
                    return false; ;
            }
            return true;
        }

        private void DecompileEasy(int funcIndex)
        {
            foreach (FunctionBlock block in Functions[funcIndex].Blocks)
            {
                uint offset = pe.Rva2Offset(block.StartAddress - (uint)pe.optHdr.ImageBase);

                for (uint i = 0; i < block.InstrCount; i++)
                {
                    uint instruction = pe.ReadInstruction(offset + i * 4);
                    XenonInstructions.OpCodeInfo info = instrs.GetInfo(instruction);

                    if (info.CEquivalent != null)
                    {
                        List<CStatement> cEquiv = info.CEquivalent(block.StartAddress + i * 4, instruction);
                        block.Statements.AddRange(cEquiv);
                    }
                    else
                    {
                        MessageBox.Show("Can't translate instruction " + info.Name);
                    }
                }
            }
        }

        void ComputeUseDefs(List<FunctionBlock> blocks)
        {
            foreach (FunctionBlock b in blocks)
            {
                foreach (CStatement stat in b.Statements)
                    stat.FindUsesAndDefines();
            }

            foreach (FunctionBlock b in blocks)
            {
                b.Uses = new List<String>();
                b.Defines = new List<String>();
  //              b.Input = new List<String>();
   //             b.Output = new List<String>();

                for (int i = b.Statements.Count - 1; i >= 0; i--)
                {
                    CStatement s = b.Statements[i];

                    s.LiveOut = s.Uses;
                    s.Dead = s.Defines;
                    foreach (String lo in s.LiveOut)
                        s.Dead.Remove(lo);

                    foreach (String lo in s.LiveOut)
                        b.Defines.Remove(lo);

                    foreach (String lo in s.Dead)
                        b.Uses.Remove(lo);

                    b.Uses.AddRange(s.LiveOut);
                    b.Defines.AddRange(s.Dead);

                    b.Uses = b.Uses.Distinct().ToList();
                    b.Defines = b.Defines.Distinct().ToList();
                }
            }

            bool changed;
            do
            {
                changed = false;
                foreach (FunctionBlock b in blocks)
                {
                    List<String> output = new List<String>(b.Defines);
                    foreach (FunctionBlock succ in b.Successors)
                    {
                        if (succ.Input != null)
                            output.AddRange(succ.Input);
                    }
                    output = output.Distinct().ToList();

                    List<String> input = new List<String>(output);
                    foreach (String d in b.Defines)
                        input.Remove(d);
                    input.AddRange(b.Uses);
                    input = input.Distinct().ToList();

                    bool inputChanged, outputChanged;
                    if (b.Input != null)
                        inputChanged = (input.Count == b.Input.Count) && input.Except(b.Input).Any();
                    else
                        inputChanged = true;

                    if (b.Output != null)
                        outputChanged = (input.Count == b.Output.Count) && input.Except(b.Output).Any();
                    else
                        outputChanged = true;

                    if (inputChanged || outputChanged)
                    {
                        changed = true;
                        b.Input = input;
                        b.Output = output;
                    }
                }
            } while (changed == true);

            foreach (FunctionBlock b in blocks)
            {
                List<String> live = new List<String>();
                foreach (FunctionBlock succ in b.Successors)
                    live.AddRange(succ.Input);
                live = live.Distinct().ToList();

                for (int i = b.Statements.Count - 1; i >= 0; i--)
                {
                    CStatement stat = b.Statements[i];
                    List<String> newLive = new List<String>(live);
                    foreach (String d in stat.Dead)
                        newLive.Remove(d);
                    newLive.AddRange(stat.LiveOut);
                    newLive = newLive.Distinct().ToList();
                    
                    stat.LiveOut = live;
                    live = newLive;
                }
            }
        }
    }
}
