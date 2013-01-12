using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X360Decompiler
{
    public class CType
    {
        public enum TypeKind
        {
            Unknown,
            ValueType,
            Pointer
        };

        public TypeKind Kind;

        public CType(TypeKind k)
        {
            Kind = k;
        }
    }
}
