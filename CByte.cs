namespace GorkhonScriptEditor
{
    public class CByte
    {
        public string ValueString { get; set; }
        public byte ValueByte { get; set; }

        public CByte(byte val)
        {
            ValueByte = val;
            ValueString = val.ToString("X2");
        }

    }
}
