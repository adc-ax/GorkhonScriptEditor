using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GorkhonScriptEditor
{
    public partial class CLabel : ObservableObject
    {
        public Int32 location;
        public StackPanel labelStackPanel;

        public TextBlock labelHeadingBlock;

        [ObservableProperty]
        public string name;

        [ObservableProperty]
        public ObservableCollection<int> calls;

        public CLabel(Int32 loc)
        {
            labelHeadingBlock = new();
            labelStackPanel = new();

            location = loc;

            Name = /*"LABEL 0x" +*/ location.ToString("X2");

            Calls = new();
        }



    }
}
