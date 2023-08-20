using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GorkhonScriptEditor
{
    public partial class CData : ObservableRecipient
    {
        public enum Type { Boolean = 1,Integer,Float,String,Coordinates,Pointer }

        [ObservableProperty]
        public IInstruction instructionRef;

        public Type dataType = Type.Boolean;


        [ObservableProperty]
        object value = null;

        [ObservableProperty]
        public String locationString;

        public CData(Type type, object val, IInstruction iRef)
        {
            dataType = type; Value = val;
            this.InstructionRef = iRef;
            LocationString = InstructionRef.ID.ToString("X5");
        }

        public override string ToString()
        {
            string stringRep = "";
            stringRep += "0x" + InstructionRef.ID.ToString("X5") +": ";
            stringRep += dataType.ToString().ToLowerInvariant() + " ";
            if (dataType == Type.Coordinates)
            {
                stringRep += string.Join(", ", ((List<float>)Value).ToArray());
            }
            else
            {
                stringRep += Value.ToString();
            }
            
            return stringRep;
        }

    }
}
