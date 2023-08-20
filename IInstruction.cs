using System;
using System.Collections.Generic;

namespace GorkhonScriptEditor
{
    public interface IInstruction
    {
        string DisplayString { get; set; }
        UInt16 OPCode { get; set; }
        UInt32 ByteOffset { get; set; }

        public UInt32 ID { get; set; }

        List<Object> Args { get; set; }

        public List<byte> binaryRepresentation { get; set; }
        public string ToString();

        public bool ValidateOperands(byte[] operands, bool updateBinary);

        public void UpdateFromBinary(byte[] operands);

    }
}
