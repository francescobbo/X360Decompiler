using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X360Decompiler
{
    public class CStatement
    {
        public enum Kinds
        {
            Addition,
            And,
            Assignment,
            BinaryNot,
            Break,
            Call,
            Comparison,
            CompositeCondition,
            Continue,
            Conditional,
            DoWhile,
            Goto,
            LeftShift,
            Or,
            Return,
            RightShift,
            Subtraction,
            Throw,
            While
        }

        public enum Sizes
        {
            Byte, Short, Int, Long
        }

        public enum OperandKinds
        {
            Constant, Variable,
            AddressPointer, BasePointer, BaseOffsetPointer, BaseBasePointer,
            Expression
        }

        public enum Conditions
        {
            LessThan, GreaterThan,
            LessEqualThan, GreaterEqualThan,
            Equal, NotEqual,
            Overflow, NotOverflow,
            And, Or,
        }

        public Kinds Kind;
        public Sizes OperandSizes;
        public COperand Op1, Op2;
        public Conditions ConditionSign;
        
        public CStatement Condition;
        public List<CStatement> InnerBlock;
        public List<FunctionBlock> SubBlocks;
        public FunctionBlock BreakBlock;
        public FunctionBlock ContinueBlock;

        public String CallFuncName, BranchDestination;
        public uint BranchDestinationAddr;
        public string BranchDestinationRegister;

        public CStatement Op1Cond, Op2Cond;

        public CStatement()
        {
        }

        public CStatement(Kinds k)
        {
            this.Kind = k;
            this.Op1 = null;
            this.Op2 = null;
        }

        public CStatement(Kinds k, COperand op1, COperand op2)
        {
            this.Kind = k;
            this.Op1 = op1;
            this.Op2 = op2;
        }

        public CStatement(Kinds k, String var1)
        {
            this.Kind = k;
            this.Op1 = new COperand(var1);
            this.Op2 = null;
        }

        public CStatement(Kinds k, ulong var1)
        {
            this.Kind = k;
            this.Op1 = new COperand(var1);
            this.Op2 = null;
        }

        public CStatement(Kinds k, String var1, String var2)
        {
            this.Kind = k;
            this.Op1 = new COperand(var1);
            this.Op2 = new COperand(var2);
        }

        public CStatement(Kinds k, String var1, ulong var2)
        {
            this.Kind = k;
            this.Op1 = new COperand(var1);
            this.Op2 = new COperand(var2);
        }

        public CStatement(Kinds k, String var1, CStatement expr2)
        {
            this.Kind = k;
            this.Op1 = new COperand(var1);
            this.Op2 = new COperand(expr2);
        }

        public CStatement(Kinds k, CStatement op1, CStatement op2)
        {
            this.Kind = k;
            this.Op1 = new COperand(op1);
            this.Op2 = new COperand(op2);
        }

        public CStatement(Kinds k, CStatement op1, ulong op2)
        {
            this.Kind = k;
            this.Op1 = new COperand(op1);
            this.Op2 = new COperand(op2);
        }

        public override string ToString()
        {
            return ToString(1);
        }

        public List<String> Defines, Uses;
        public List<String> LiveOut, Dead;

        public void FindUsesAndDefines()
        {
            Defines = new List<String>();
            Uses = new List<String>();
                    
            if (Kind == CStatement.Kinds.Assignment && Op1.Kind == CStatement.OperandKinds.Variable)
                Defines.Add(Op1.Name);
            else
            {
                if (Op1 != null)
                {
                    List<String> uses = Op1.GetUses();
                    if (uses != null)
                        Uses.AddRange(uses);
                }
            }

            if (Op2 != null)
            {
                List<String> uses = Op2.GetUses();
                if (uses != null)
                    Uses.AddRange(uses);
            }
            
            if (Condition != null)
            {
                if (Condition.Op1 != null)
                {
                    List<String> uses = Condition.Op1.GetUses();
                    if (uses != null)
                        Uses.AddRange(uses);
                }

                if (Condition.Op2 != null)
                {
                    List<String> uses = Condition.Op2.GetUses();
                    if (uses != null)
                        Uses.AddRange(uses);
                }
            }

            if (BranchDestinationRegister != null)
                Uses.Add(BranchDestinationRegister);
        }

        public String ToString(int indentation)
        {
            switch (Kind)
            {
                case Kinds.Addition:
                    return AdditionToString();
                case Kinds.And:
                    return AndToString();
                case Kinds.Assignment:
                    return AssignmentToString();
                case Kinds.BinaryNot:
                    return BinaryNotToString();
                case Kinds.Break:
                    return "break";
                case Kinds.Call:
                    return CallToString();
                case Kinds.Comparison:
                    return ComparisonToString();
                case Kinds.CompositeCondition:
                    return CompositeToString();
                case Kinds.Conditional:
                    return ConditionalToString(indentation);
                case Kinds.Continue:
                    return "continue";
                case Kinds.DoWhile:
                    return DoWhileToString(indentation);
                case Kinds.Goto:
                    return GotoToString();
                case Kinds.LeftShift:
                    return LeftShiftToString();
                case Kinds.RightShift:
                    return RightShiftToString();
                case Kinds.Or:
                    return OrToString();
                case Kinds.Return:
                    return ReturnToString();
                case Kinds.Subtraction:
                    return SubtractionToString();
                case Kinds.Throw:
                    return ThrowToString();
                case Kinds.While:
                    return WhileToString(indentation);
            }

            return null;
        }

        private String AdditionToString()
        {
            return Op1.ToString() + " + " + Op2.ToString();
        }

        private String AndToString()
        {
            return Op1.ToString() + " & " + Op2.ToString();
        }

        private String AssignmentToString()
        {
            return Op1.ToString() + " = " + Op2.ToString();
        }

        private String BinaryNotToString()
        {
            return "~" + Op1.ToString();
        }

        private String CallToString()
        {
            return CallFuncName + "()";
        }

        private String ComparisonToString()
        {
            return Op1.ToString() + " " + ComparisonOperatorToString() + " " + Op2.ToString();
        }

        private String CompositeToString()
        {
            return Op1Cond.ToString() + " " + ComparisonOperatorToString() + " " + Op2Cond.ToString();
        }

        private String ConditionalToString(int indentation)
        {
            String ret = "if (" + Condition.ToString() + ")\n";
            
            for (int i = 0; i < indentation; i++)
                ret += "\t";
            ret += "{\n";

            if (InnerBlock != null)
            {
                foreach (CStatement stat in InnerBlock)
                {
                    for (int i = 0; i < indentation + 1; i++)
                        ret += "\t";
                    if (stat.Kind != Kinds.Conditional && stat.Kind != Kinds.While)
                        ret += stat.ToString(indentation + 1) + ";\n";
                    else
                        ret += stat.ToString(indentation + 1) + "\n";
                }
            }

            for (int i = 0; i < indentation; i++)
                ret += "\t";
            ret += "}";
            return ret;
        }

        private String DoWhileToString(int indentation)
        {
            String ret = "do\n";

            for (int i = 0; i < indentation; i++)
                ret += "\t";
            ret += "{\n";

            foreach (CStatement stat in InnerBlock)
            {
                for (int i = 0; i < indentation + 1; i++)
                    ret += "\t";
                if (stat.Kind != Kinds.Conditional && stat.Kind != Kinds.While)
                    ret += stat.ToString(indentation + 1) + ";\n";
                else
                    ret += stat.ToString(indentation + 1) + "\n";
            }

            for (int i = 0; i < indentation; i++)
                ret += "\t";
            ret += "} while (" + Condition.ToString() + ")";
            return ret;
        }

        private String GotoToString()
        {
            return "goto " + BranchDestination;
        }

        private String LeftShiftToString()
        {
            return Op1.ToString() + " << " + Op2.ToString();
        }

        private String OrToString()
        {
            return Op1.ToString() + " | " + Op2.ToString();
        }

        private String ReturnToString()
        {
            return "return";
        }

        private String RightShiftToString()
        {
            return Op1.ToString() + " >> " + Op2.ToString();
        }

        private String SubtractionToString()
        {
            return Op1.ToString() + " - " + Op2.ToString();
        }

        private String ThrowToString()
        {
            return "throw";
        }

        private String WhileToString(int indentation)
        {
            String ret = "while (" + Condition.ToString() + ")\n";

            for (int i = 0; i < indentation; i++)
                ret += "\t";
            ret += "{\n";

            foreach (CStatement stat in InnerBlock)
            {
                for (int i = 0; i < indentation + 1; i++)
                    ret += "\t";
                if (stat.Kind != Kinds.Conditional && stat.Kind != Kinds.While)
                    ret += stat.ToString(indentation + 1) + ";\n";
                else
                    ret += stat.ToString(indentation + 1) + "\n";
            }

            for (int i = 0; i < indentation; i++)
                ret += "\t";
            ret += "}";
            return ret;
        }
        
        private String SizeToString()
        {
            switch (OperandSizes)
            {
                case Sizes.Byte:
                    return "char";
                case Sizes.Short:
                    return "short";
                case Sizes.Int:
                    return "int";
                case Sizes.Long:
                    return "long";
            }

            return null;
        }

        private String ComparisonOperatorToString()
        {
            switch (ConditionSign)
            {
                case Conditions.Equal:
                    return "==";
                case Conditions.GreaterEqualThan:
                    return ">=";
                case Conditions.GreaterThan:
                    return ">";
                case Conditions.LessEqualThan:
                    return "<=";
                case Conditions.LessThan:
                    return "<";
                case Conditions.NotEqual:
                    return "!=";
                case Conditions.NotOverflow:
                    return "!Overflow";
                case Conditions.Overflow:
                    return "Overflow";
                case Conditions.And:
                    return "&&";
                case Conditions.Or:
                    return "||";
            }

            return null;
        }

        public void ExpandBlocks()
        {
            InnerBlock = new List<CStatement>();

            foreach (FunctionBlock b in SubBlocks)
            {
                foreach (CStatement stat in b.Statements)
                    InnerBlock.Add(stat);
            }
        }

        public class COperand
        {
            public COperand()
            {
            }

            public COperand(CStatement expression)
            {
                Kind = OperandKinds.Expression;
                Expr = expression;
            }

            public COperand(String varName)
            {
                Kind = OperandKinds.Variable;
                Name = varName;
            }

            public COperand(ulong value)
            {
                Kind = OperandKinds.Constant;
                Value = value;
            }

            public COperand(String baseVar, long offset)
            {
                Kind = OperandKinds.BaseOffsetPointer;
                Base = baseVar;
                Offset = offset;
            }

            public COperand(String baseVar, String baseVar2)
            {
                Kind = OperandKinds.BaseBasePointer;
                Base = baseVar;
                Base2 = baseVar2;
            }

            public OperandKinds Kind;
            public Sizes OperandSize = Sizes.Long;

            public ulong Value;
            public string Name, Base, Base2;
            public long Offset;
            public CStatement Expr;

            public List<String> GetUses()
            {
                if (Kind == OperandKinds.Constant)
                    return null;

                List<String> ret = new List<string>();

                if (Kind == OperandKinds.Variable)
                    ret.Add(Name);
                else if (Kind == OperandKinds.BaseOffsetPointer)
                    ret.Add(Base);
                else if (Kind == OperandKinds.BaseBasePointer)
                {
                    ret.Add(Base);
                    ret.Add(Base2);
                }
                else if (Kind == OperandKinds.Expression)
                {
                    if (Expr.Op1 != null)
                    {
                        List<String> uses = Expr.Op1.GetUses();
                        if (uses != null)
                            ret.AddRange(uses);
                    }
                    if (Expr.Op2 != null)
                    {
                        List<String> uses = Expr.Op2.GetUses();
                        if (uses != null)
                            ret.AddRange(uses);
                    }
                }

                return ret;
            }

            public override String ToString()
            {
                switch (Kind)
                {
                    case OperandKinds.Constant:
                        return "0x" + Value.ToString("X");
                    case OperandKinds.Variable:
                        return Name;
                    case OperandKinds.AddressPointer:
                        return "*(" + SizeToString() + " *)(0x" + Offset.ToString("X") + ")";
                    case OperandKinds.BaseOffsetPointer:
                        if (Offset == 0)
                            return "*(" + SizeToString() + " *)(" + Base + ")";
                        else
                            return "*(" + SizeToString() + " *)(&" + Base + "[" + Offset + "])";
                    case OperandKinds.BaseBasePointer:
                        return "*(" + SizeToString() + " *)(" + Base + " + " + Base2 + ")";
                    case OperandKinds.Expression:
                        return "(" + Expr.ToString() + ")";
                }

                return null;
            }

            private String SizeToString()
            {
                switch (OperandSize)
                {
                    case Sizes.Byte:
                        return "char";
                    case Sizes.Short:
                        return "short";
                    case Sizes.Int:
                        return "int";
                    case Sizes.Long:
                        return "long";
                }

                return null;
            }
        }
    }
}
