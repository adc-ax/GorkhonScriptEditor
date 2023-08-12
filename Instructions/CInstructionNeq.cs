using System;
using System.Collections.Generic;

namespace GorkhonScriptEditor.Instructions
{
    class CInstructionNeq : CommunityToolkit.Mvvm.ComponentModel.ObservableObject, IInstruction
    {

        public CInstructionNeq(List<Object> args, List<byte> bin)
        {
            OPCode = 0x28;
            Args = args;
            DisplayString = ("NEQ stack[" + Args[0].ToString() + "], stack[" + Args[1].ToString() + "] and push");
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
