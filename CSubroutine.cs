using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;

namespace GorkhonScriptEditor
{
    public partial class CSubroutine : ObservableObject
    {
        public Int32 locStart;
        public Int32 locEnd;

        [ObservableProperty]
        public String subName;

        [ObservableProperty]
        public ObservableCollection<int> calls;

        public CSubroutine(Int32 start, Int32 end = 0)
        {
            locStart = start;
            locEnd = end;
            SubName = locStart.ToString("X4");
            Calls = new();
        }

    }
}
