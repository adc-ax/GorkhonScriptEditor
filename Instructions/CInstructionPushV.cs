using System;
using System.Collections.Generic;

namespace GorkhonScriptEditor.Instructions
{
    class CInstructionPushV : CommunityToolkit.Mvvm.ComponentModel.ObservableObject, IInstruction
    {
        public CInstructionPushV(List<Object> args, List<byte> bin)
        {
            OPCode = 0x17;
            Args = args;
            //DisplayString = ("PUSH<var> " + Args[0].ToString() + " var(s) of types (1 = bool,2 = int,3 = float,4 = string,5 - pointer,6-coordinates)");
            DisplayString = ("PUSH<var> " + Args[0].ToString() + " var(s): ");
            for (int i = 0; i < Convert.ToInt32(Args[0]); i++)
            {
                string type = "";
                byte sel = (Args[1] as byte[])[i];
                switch (sel)
                {
                    case 0: type = "var"; break;
                    case 1: type = "bool"; break;
                    case 2: type = "int"; break;
                    case 3: type = "float"; break;
                    case 4: type = "string"; break;
                    case 5: type = "pointer"; break;
                    case 6: type = "coordinates"; break;
                    default: break;
                }
                DisplayString += type + "; ";

            }
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
