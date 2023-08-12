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
        enum Type { Boolean = 0,Integer,Float,String,Coordinates,Pointer }

        Type dataType = Type.Boolean;
        [ObservableProperty]
        object value = null;

        CData(int type, object val) 
        { 
            dataType = (Type)type; Value = val; 
        }

    }
}
