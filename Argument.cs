using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X360Decompiler
{
    public class Argument
    {
        public String Name;
        public CType Type = new CType(CType.TypeKind.Unknown);

        public Argument(String name)
        {
            Name = name;
        }
    }
}
