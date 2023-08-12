using System;
using System.Collections.Generic;

namespace GorkhonScriptEditor.Instructions
{
    class CInstructionPopE : CommunityToolkit.Mvvm.ComponentModel.ObservableObject, IInstruction
    {

        public CInstructionPopE(List<Object> args, List<byte> bin)
        {
            OPCode = 0x1B;
            Args = args;
            DisplayString = ("POP " + Args[1].ToString() + " items if stack[" + Args[0].ToString() + "] ");
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
