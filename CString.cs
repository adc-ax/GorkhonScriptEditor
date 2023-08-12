using System.Collections.Generic;

namespace GorkhonScriptEditor
{
    public class CString
    {
        public bool IsUTF8;
        public string StringRepresentation;
        public int StartOffset;
        public List<byte> BinaryRepresentation;
        //System.Text.Encoding.UTF8.GetString((new ArraySegment<byte>(binData, offset + i, off + 1)).ToArray())

        public CString(bool utf8, string stringRepresentation, int offset, List<byte> binaryRepresentation)
        {
            IsUTF8 = utf8;
            StringRepresentation = stringRepresentation;
            BinaryRepresentation = binaryRepresentation;
            StartOffset = offset;
        }

        public override string ToString()
        {
            return StartOffset.ToString("X5") + ": " + StringRepresentation + (IsUTF8 ? " ; UTF-16" : " ; UTF-8");
        }
    }
}
