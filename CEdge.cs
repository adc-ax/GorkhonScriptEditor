namespace GorkhonScriptEditor
{
    public class CEdge
    {

        public bool isCall = false;

        public CBasicBlock A, B;
        public CEdge(CBasicBlock blockA, CBasicBlock blockB)
        {
            A = blockA;
            B = blockB;
        }

    }
}
