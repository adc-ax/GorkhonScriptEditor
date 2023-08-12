using System;
using System.Collections.Generic;

namespace GorkhonScriptEditor.Instructions
{
    class CInstructionTaskReturn : CommunityToolkit.Mvvm.ComponentModel.ObservableObject, IInstruction
    {
        public CInstructionTaskReturn(List<Object> args, List<byte> bin)
        {
            OPCode = 0x51;
            Args = args;
            DisplayString = ("TASKRET POP task vars");
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
