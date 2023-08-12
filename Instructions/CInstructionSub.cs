using System;
using System.Collections.Generic;

namespace GorkhonScriptEditor.Instructions
{
    class CInstructionSub : CommunityToolkit.Mvvm.ComponentModel.ObservableObject, IInstruction
    {

        public CInstructionSub(List<Object> args, List<byte> bin)
        {
            OPCode = 0x20;
            Args = args;
            DisplayString = ("SUB [" + Args[0].ToString() + "] - [" + Args[1].ToString() + "]");
            DisplayString += ", POP " + Args[2].ToString();
            DisplayString += ", PUSH result";
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
