using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X360Decompiler
{
    [Serializable()]
    public class CType
    {
        public static State decompiler;

        public enum TypeKind
        {
            Unknown, UnknownPointer,
            ValueType,
            Pointer
        };

        public enum PrimitiveTypes
        {
            Void, Char, Short, Int, Long,
            UnsignedChar, UnsignedShort, UnsignedInt, UnsignedLong,
            Float, Double, Bool
        }

        public TypeKind Kind { get; set; }
        public string Name { get; set; }
        
        private PrimitiveTypes Prim { get; set; }
        private Structure Struct { get; set; }
        private bool IsPrimitive { get; set; }

        public CType(TypeKind k)
        {
            Kind = k;
        }

        static Dictionary<string, PrimitiveTypes> string2pt = new Dictionary<string, PrimitiveTypes>()
        {
            { "void", PrimitiveTypes.Void },
            { "char", PrimitiveTypes.Char },
            { "short", PrimitiveTypes.Short },
            { "int", PrimitiveTypes.Int },
            { "long", PrimitiveTypes.Long },
            { "unsigned char", PrimitiveTypes.UnsignedChar },
            { "unsigned short", PrimitiveTypes.UnsignedShort },
            { "unsigned int", PrimitiveTypes.UnsignedInt },
            { "unsigned long", PrimitiveTypes.UnsignedLong },
            { "float", PrimitiveTypes.Float },
            { "double", PrimitiveTypes.Double },
            { "bool", PrimitiveTypes.Bool },
        };

        public CType(TypeKind k, string typeName)
        {
            Kind = k;
            if (k == TypeKind.Unknown)
                return;

            if (!string2pt.ContainsKey(typeName))
            {
                IsPrimitive = false;
                
                // TODO: Search in structures array and throw if not found!
            }
            else
            {
                IsPrimitive = true;
                Prim = string2pt[typeName];
            }
         
            Name = typeName;
        }

        public override string ToString()
        {
            if (Kind == TypeKind.Unknown)
                return "_unknown_";
            else if (Kind == TypeKind.UnknownPointer)
                return "_unknown_ *";

            String ret = Name;

            if (Kind == TypeKind.Pointer)
                ret += " *";

            return ret;
        }
    }
}
