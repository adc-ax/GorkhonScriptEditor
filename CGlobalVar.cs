namespace GorkhonScriptEditor
{
    public class CGlobalVar
    {
        public byte Type { get; set; }
        public string Name { get; set; }

        public string TypeName { get; set; }

        public string ID { get; set; }

        public CGlobalVar(byte type, string name = "")
        {
            Type = type;
            Name = name;
            TypeName = "unknown";
            switch (type)
            {

                case 0: TypeName = "unknown"; break;
                case 1: TypeName = "bool"; break;
                case 2: TypeName = "int"; break;
                case 3: TypeName = "float"; break;
                case 4: TypeName = "string"; break;
                case 5: TypeName = "pointer"; break;
                case 6: TypeName = "coordinates"; break;
                default: break;
            }
        }
    }
}
