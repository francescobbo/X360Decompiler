using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X360Decompiler
{
    [Serializable()]
    public class Variable
    {
        public String Name { get; set; }
        public CType Type { get; set; }

        public Variable(String name, CType type)
        {
            Name = name;
            Type = type;
        }
    }
}
