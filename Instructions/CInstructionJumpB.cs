using System;
using System.Collections;
using System.Collections.Generic;

namespace GorkhonScriptEditor.Instructions
{
    class CInstructionJumpB : CommunityToolkit.Mvvm.ComponentModel.ObservableObject, IInstruction
    {
        public CInstructionJumpB(List<Object> args, List<byte> bin)
        {
            OPCode = 0x0F;
            Args = args;
            DisplayString = "";
            DisplayString += ("JUMPB 0x" + ((int)Args[1]).ToString("X2") + " if stack[" + Args[0].ToString() + "] == ");
            if ((bool)Args[2]) { DisplayString += "True"; } else { DisplayString += "False"; }
            DisplayString +=  ", pop " + Args[3].ToString() + " vars from stack";
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
