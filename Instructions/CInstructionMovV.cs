using System;
using System.Collections.Generic;

namespace GorkhonScriptEditor.Instructions
{
    class CInstructionMovV : CommunityToolkit.Mvvm.ComponentModel.ObservableObject, IInstruction
    {
        public CInstructionMovV(List<Object> args, List<byte> bin)
        {
            OPCode = 0x16;
            Args = args;
            DisplayString = ("MOV<coordinates> (" + Args[1].ToString() + "; " + Args[2].ToString() + "; " + Args[3].ToString() + ") -> [" + Args[0].ToString() + "]");
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
