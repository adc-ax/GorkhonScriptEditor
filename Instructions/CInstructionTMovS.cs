using System;
using System.Collections.Generic;

namespace GorkhonScriptEditor.Instructions
{
    class CInstructionTMovS : CommunityToolkit.Mvvm.ComponentModel.ObservableObject, IInstruction
    {
        public CInstructionTMovS(List<Object> args, List<byte> bin)
        {
            OPCode = 0xB;
            Args = args;
            StringConstant = "";
            DisplayString = ("TMOV<string> [" + Args[0].ToString() + "] <- @" + Convert.ToInt32(Args[1]).ToString("X5"));
            binaryRepresentation = bin;
        }

        public CInstructionTMovS(List<Object> args, List<byte> bin, string strConst)
        {
            OPCode = 0xB;
            Args = args;
            StringConstant = strConst;
            DisplayString = "TMOV<string> taskVar #" + Args[0].ToString() + " <- @" + Convert.ToInt32(Args[1]).ToString("X5") + " \"" + strConst + "\"";
            binaryRepresentation = bin;
        }

        public string StringConstant { get; set; }

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
