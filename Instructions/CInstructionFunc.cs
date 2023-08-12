using System;
using System.Collections.Generic;

namespace GorkhonScriptEditor.Instructions
{
    class CInstructionFunc : CommunityToolkit.Mvvm.ComponentModel.ObservableObject, IInstruction
    {
        public CInstructionFunc(List<Object> args, List<byte> bin)
        {
            OPCode = 0x53;
            Args = args;
            DisplayString = "FUNC #" + Args[0].ToString() + " " + (Args[^1] as CFunction).Name + "(";
            int i;
            for (i = 1; i < Args.Count - 2; i++)
            {
                DisplayString += "stack[" + Args[i].ToString() + "]" + ", ";
            }
            if (((CFunction)Args[Args.Count - 1]).NumArgs != 0)
                DisplayString += "stack[" + Args[i].ToString() + "]";
            DisplayString += ")";
            binaryRepresentation = bin;

        }

        public override string ToString()
        {
            return DisplayString;
        }

        public string DisplayString { get; set; }
        public UInt16 OPCode { get; set; }
        public List<Object> Args { get; set; }

        public List<byte> binaryRepresentation { get; set; }
        uint IInstruction.ByteOffset { get; set; }
        uint IInstruction.ID { get; set; }

        bool IInstruction.ValidateOperands(byte[] operands, bool updateBinary)
        {
            throw new NotImplementedException();
        }

        public void UpdateFromBinary(byte[] operands)
        {
            throw new NotImplementedException();
        }
    }
}
