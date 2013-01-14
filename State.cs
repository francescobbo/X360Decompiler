using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace X360Decompiler
{
    public class State
    {
        public List<Function> Functions = new List<Function>();
        public List<Function> IgnoredCalls = new List<Function>();
        public List<Function> CallIsRet = new List<Function>();
        public List<Structure> Structures = new List<Structure>();

        public XenonInstructions Instructions;
        public XPeParser Pe;

        private State()
        {
            CType.decompiler = this;
            Instructions = new XenonInstructions(this);
            Instructions.SetupTables();
        }

        public State(String exeName)
        {
            CType.decompiler = this;
            Instructions = new XenonInstructions(this);
            Instructions.SetupTables();
            Pe = new XPeParser(exeName);
        }

        public static State FromFile(String xdp)
        {
            State state = new State();
            state.LoadFromFile(xdp);
            return state;
        }

        public void SaveToFile(String fn)
        {
            FileStream fs = new FileStream(fn, FileMode.Create, FileAccess.Write, FileShare.None);
            BinaryFormatter bin = new BinaryFormatter();
            BinaryWriter bw = new BinaryWriter(fs);

            bw.Write(Pe.FileName);

            bw.Write(Functions.Count);
            foreach (Function f in Functions)
            {
                bw.Write(f.Name);
                bw.Write(f.Address);
                bw.Write(f.Size);
                bw.Write(f.ArgCount);
                if (f.ArgCount != -1)
                    bin.Serialize(fs, f.Arguments);
                bin.Serialize(fs, f.Returns);
            }

            bw.Write(IgnoredCalls.Count);
            foreach (Function f in IgnoredCalls)
                bw.Write(f.Address);

            bw.Write(CallIsRet.Count);
            foreach (Function f in CallIsRet)
                bw.Write(f.Address);

            bin.Serialize(fs, Structures);

            fs.Close();
        }

        public void LoadFromFile(String fn)
        {
            FileStream fs = new FileStream(fn, FileMode.Open, FileAccess.Read, FileShare.Read);
            BinaryReader br = new BinaryReader(fs);
            BinaryFormatter bin = new BinaryFormatter();

            Pe = new XPeParser(br.ReadString());

            int count = br.ReadInt32();
            Functions = new List<Function>();
            for (int i = 0; i < count; i++)
            {
                Function f = new Function(this, br.ReadString(), br.ReadUInt32());
                f.Size = br.ReadUInt32();
                f.ArgCount = br.ReadInt32();
                if (f.ArgCount != -1)
                    f.Arguments = (List<Variable>) bin.Deserialize(fs);
                f.Returns = (CType)bin.Deserialize(fs);
                Functions.Add(f);
            }

            count = br.ReadInt32();
            IgnoredCalls = new List<Function>();
            for (int i = 0; i < count; i++)
            {
                uint addr = br.ReadUInt32();
                Function f = Functions.Find(delegate(Function fnc) { return fnc.Address == addr; });
                IgnoredCalls.Add(f);
            }

            count = br.ReadInt32();
            CallIsRet = new List<Function>();
            for (int i = 0; i < count; i++)
            {
                uint addr = br.ReadUInt32();
                Function f = Functions.Find(delegate(Function fnc) { return fnc.Address == addr; });
                CallIsRet.Add(f);
            }

            Structures = (List<Structure>)bin.Deserialize(fs);

            fs.Close();
        }
    }
}
