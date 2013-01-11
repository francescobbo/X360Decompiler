using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X360Decompiler
{
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
}
