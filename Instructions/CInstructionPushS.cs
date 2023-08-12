using System;
using System.Collections.Generic;

namespace GorkhonScriptEditor.Instructions
{
    class CInstructionPushS : CommunityToolkit.Mvvm.ComponentModel.ObservableObject, IInstruction
    {
        public CInstructionPushS(List<Object> args, List<byte> bin)
        {
            OPCode = 0x14;
            Args = args;
            DisplayString = ("PUSH<string> @" + Convert.ToInt32(Args[0]).ToString("X5"));
            binaryRepresentation = bin;
            StringConstant = "";
        }

        public CInstructionPushS(List<Object> args, List<byte> bin, string strConst)
        {
            OPCode = 0x14;
            Args = args;
            StringConstant = strConst;
            DisplayString = ("PUSH<string> @" + Convert.ToInt32(Args[0]).ToString("X5") + " \"" + strConst + "\"");
            binaryRepresentation = bin;
        }

        public override string ToString()
        {
            return DisplayString;
        }

        public string StringConstant { get; set; }

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
