using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X360Decompiler
{
    [Serializable()]
    public class Structure
    {
        public String Name { get; set; }
        public List<Variable> Fields { get; set; }
    }
}
