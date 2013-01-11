using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X360Decompiler
{
    public class State
    {
        public List<Function> Functions = new List<Function>();
        public XenonInstructions Instructions = new XenonInstructions();
        public XPeParser Pe;

        public State(String exeName)
        {
            Pe = new XPeParser(exeName);
            Instructions.SetupTables();
        }
    }
}
