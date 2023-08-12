using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GorkhonScriptEditor
{
    public partial class CFunction : ObservableObject
    {
        [ObservableProperty]
        public ObservableCollection<int> calls;

        [ObservableProperty]
        public String name;

        public int ID;
        public int NumArgs;

        public CFunction(String name, int numArgs, int id)
        {
            Name = name;
            NumArgs = numArgs;
            Calls = new();
            ID = id;
        }

        public override string ToString()
        {
            string ret = "";
            ret += this.ID.ToString() + ": " + this.Name + ", " + this.NumArgs + " args, " + this.Calls.Count.ToString() + " calls.";
            return ret;
        }

    }
}
