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
        PdbParser.PdbParser pdb;
        State state;

        public class ListViewFunction
        {
            public ListViewFunction(string name, uint addr, Function f)
            {
                FuncName = name;
                FuncAddr = "0x" + addr.ToString("X8");
                Details = f;
            }

            public string FuncName { get; set; }
            public string FuncAddr { get; set; }

            public Function Details;
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
                state = new State(ofd.FileName);
                PdbMenu.IsEnabled = true;
                SaveMenu.IsEnabled = true;
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
                    UInt32 addr = s.Rva + (uint) state.Pe.GetImageBase();
                    XPeParser.SectionHeader section = state.Pe.GetSectionByAddress(s.Rva);

                    /* Code or Mem_Executable */
                    if ((section.Characteristics & 0x20000020) != 0)
                    {
                        Function f = new Function(state, s.Name, addr);
                        f.Name = s.Name;
                        f.Address = addr;

                        ListViewFunction lvf = new ListViewFunction(s.Name, addr, f);
                        _FuncCollection.Add(lvf);

                        f.ListViewEntry = lvf;
                        state.Functions.Add(f);
                    }
                }

                state.Functions.Sort(FunctionAddressComparison);
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
            Function f = item.Details;

            if (f.Size == 0xFFFFFFFF || f.Blocks == null)
            {
                f.FindSize();
                FindDominators(f.Blocks);
            }

            _InstrCollection.Clear();
            foreach (FunctionBlock block in f.Blocks)
            {
                uint offset = state.Pe.Rva2Offset(block.StartAddress - (uint)state.Pe.optHdr.ImageBase);

                for (uint i = 0; i < block.InstrCount; i++)
                {
                    uint instruction = state.Pe.ReadInstruction(offset + i * 4);

                    XenonInstructions.OpCodeInfo info = state.Instructions.GetInfo(instruction);
                    ListViewInstr instr = new ListViewInstr("0x" + (block.StartAddress + i * 4).ToString("X8"), info.Name, "");
                    _InstrCollection.Add(instr);
                }
            }
        }

        private void JumpIsRet(object sender, RoutedEventArgs e)
        {
            ListViewFunction item = (ListViewFunction)FunctionsView.SelectedItem;
            Function f = item.Details;

            state.CallIsRet.Add(f);
        }

        private void IgnoreCalls(object sender, RoutedEventArgs e)
        {
            ListViewFunction item = (ListViewFunction)FunctionsView.SelectedItem;
            Function f = item.Details;

            state.IgnoredCalls.Add(f);
        }

        private void DecompileFunction(object sender, RoutedEventArgs e)
        {
            ListViewFunction item = (ListViewFunction)FunctionsView.SelectedItem;
            Function f = item.Details;

            if (f.Size == 0xFFFFFFFF || f.Blocks == null)
            {
                f.FindSize();
                FindDominators(f.Blocks);
            }

            f.Loops = FindLoops(f.Blocks);
            DecompileEasy(f);
            ComputeUseDefs(f.Blocks);
//            PropagateExpressions(f.Blocks);
//            NiceConditions(f.Blocks);
//            LoopPhase2(f.Blocks, f.Loops);
//            IfElse(f.Blocks);

            List<CStatement> statements = new List<CStatement>();
            foreach (FunctionBlock b in f.Blocks)
            {
                CStatement label = new CStatement(CStatement.Kinds.Label);
                label.LabelName = "L" + b.StartAddress.ToString("X8");

                statements.Add(label);
                statements.AddRange(b.Statements);
            }

            string csource = "void " + f.Name + "()\n{\n";
            foreach (CStatement stat in statements)
            {
                if (stat.Kind != CStatement.Kinds.Label)
                    csource += "\t";

                csource += stat.ToString(1);
                if (stat.Kind != CStatement.Kinds.Conditional && stat.Kind != CStatement.Kinds.Label)
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

            loop.Blocks.Sort(Function.FunctionBlockSorter);
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

        private void PropagateExpressions(List<FunctionBlock> blocks)
        {
            foreach (FunctionBlock b in blocks)
            {
                Dictionary<String, CStatement.COperand> var2expr = new Dictionary<String, CStatement.COperand>();
                foreach (CStatement stat in b.Statements)
                {
                    stat.Replace(var2expr, false);

                    if (stat.Kind == CStatement.Kinds.Assignment && stat.Op1.Kind == CStatement.OperandKinds.Variable)
                    {
                        foreach (KeyValuePair<String, CStatement.COperand> kvp in var2expr)
                        {
                            if (kvp.Value.GetUses().Contains(stat.Op1.Name))
                            {
                                /*
                                 * r10 = 3 + r11
                                 * r11 = 4 + r12
                                 * r5 = r10 => 3 + r11 (INCORRECT!!)
                                 * Remove r10 from the dictionary since a definition to r11 is made after it
                                 */

                                var2expr.Remove(kvp.Key);
                                break;
                            }
                        }

                        if (!stat.Op2.GetUses().Contains(stat.Op1.Name))
                            var2expr[stat.Op1.Name] = stat.Op2;
                    }
                    else
                        stat.Replace(var2expr, true);
                }
            }
        }
        
        private void NiceConditions(List<FunctionBlock> blocks)
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
                uint offset = state.Pe.Rva2Offset(lastInstruction - (uint)state.Pe.optHdr.ImageBase);
                uint instruction = state.Pe.ReadInstruction(offset);
                XenonInstructions.OpCodeInfo info = state.Instructions.GetInfo(instruction);

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
                    blocks.Sort(Function.FunctionBlockSorter);
                }

                /* This will have removed some statements, reexpand the blocks */
                loop.LoopBlock.Statements[0].ExpandBlocks();
            }
        }

        private void StructureBreakContinue(CStatement container, FunctionBlock continueBlock, FunctionBlock breakBlock)
        {
            foreach (CStatement stmt in container.InnerBlock)
            {
                switch (stmt.Kind)
                {
                    case CStatement.Kinds.Goto:
                        if (continueBlock != null && stmt.BranchDestinationAddr == continueBlock.StartAddress)
                            stmt.Kind = CStatement.Kinds.Continue;
                        else if (stmt.BranchDestinationAddr == breakBlock.StartAddress)
                            stmt.Kind = CStatement.Kinds.Break;
                        break;
                    case CStatement.Kinds.Conditional:
                        /* Recursion to subscopes */
                        StructureBreakContinue(stmt, continueBlock, breakBlock);;
                        break;
                    default:
                        /* Do not recurse to do/while, while and for, since they have their own continue and break blocks! */
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
                    return false;
            }
            return true;
        }

        private void DecompileEasy(Function f)
        {
            foreach (FunctionBlock block in f.Blocks)
            {
                uint offset = state.Pe.Rva2Offset(block.StartAddress - (uint)state.Pe.optHdr.ImageBase);

                for (uint i = 0; i < block.InstrCount; i++)
                {
                    uint instruction = state.Pe.ReadInstruction(offset + i * 4);
                    XenonInstructions.OpCodeInfo info = state.Instructions.GetInfo(instruction);

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

/*            bool changed;
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
            }*/
        }

        String projFn = null;

        private void OpenProject(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Filter = "x360Dec projects (*.xdp)|*.xdp";
            bool? res = ofd.ShowDialog();

            if (res == null || res == false)
                return;

            _FuncCollection.Clear();
            projFn = ofd.FileName;
            state = State.FromFile(projFn);

            foreach (Function f in state.Functions)
            {
                ListViewFunction lvf = new ListViewFunction(f.Name, f.Address, f);
                _FuncCollection.Add(lvf);
                f.ListViewEntry = lvf;
            }

            SaveMenu.IsEnabled = true;
            PdbMenu.IsEnabled = true;

            ListCollectionView view = (ListCollectionView)CollectionViewSource.GetDefaultView(FunctionsView.ItemsSource);
            view.CustomSort = nameSorter;
        }

        private void SaveProject(object sender, RoutedEventArgs e)
        {
            if (projFn == null)
            {
                String exeFile = state.Pe.FileName;
                String fn = exeFile.Split('\\').Last().Split('.').First();

                Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
                sfd.Filter = "x360Dec projects (*.xdp)|*.xdp";
                sfd.FileName = fn;
                bool? res = sfd.ShowDialog();

                if (res == null || res == false)
                    return;

                projFn = sfd.FileName;
            }

            state.SaveToFile(projFn);
        }

        private void OpenFuncProperties(object sender, RoutedEventArgs e)
        {
            ListViewFunction item = (ListViewFunction)FunctionsView.SelectedItem;
            Function f = item.Details;

            FunctionProperties fp = new FunctionProperties(f);
            fp.ShowDialog();
        }

        String searchBuffer = "";
        private void FunctionsView_KeyDown(object sender, KeyEventArgs e)
        {
            char c = (char)KeyInterop.VirtualKeyFromKey(e.Key);
            if (c == '\b')
            {
                if (searchBuffer.Length != 0)
                    searchBuffer = searchBuffer.Substring(0, searchBuffer.Length - 1);
            }
            else if (Char.IsLetterOrDigit(c))
                searchBuffer += c;

            SearchBufferView.Text = searchBuffer;
            FunctionViewSearchNext();
        }

        private void FunctionViewSearchNext()
        {
            int curIndex = FunctionsView.SelectedIndex;
            if (curIndex == -1)
                curIndex = 0;

            for (int i = curIndex; i < FunctionsView.Items.Count; i++)
            {
                ListViewFunction f = FunctionsView.Items[i] as ListViewFunction;
                if (f.FuncName.StartsWith(searchBuffer, true, null))
                {
                    FunctionsViewScrollTo(i);
                    return;
                }
            }

            for (int i = 0; i < curIndex; i++)
            {
                ListViewFunction f = FunctionsView.Items[i] as ListViewFunction;
                if (f.FuncName.StartsWith(searchBuffer, true, null))
                {
                    FunctionsViewScrollTo(i);
                    return;
                }
            }
        }

        private void FunctionsViewScrollTo(int i)
        {
            FunctionsView.SelectedItem = FunctionsView.Items.GetItemAt(i);
            FunctionsView.ScrollIntoView(FunctionsView.SelectedItem);
            ListViewItem item = FunctionsView.ItemContainerGenerator.ContainerFromItem(FunctionsView.SelectedItem) as ListViewItem;
            item.Focus();
        }
    }
}
