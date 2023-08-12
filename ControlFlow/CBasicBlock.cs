using System.Collections.Generic;

namespace GorkhonScriptEditor
{
    public class CBasicBlock
    {
        public int start, end;
        public List<IInstruction> instructionList;
        public CBasicBlock(int s, int e) { start = s; end = e; instructionList = new(); }
    }
}
