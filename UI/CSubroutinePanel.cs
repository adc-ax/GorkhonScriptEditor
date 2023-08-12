using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GorkhonScriptEditor
{
    public class CSubroutinePanel
    {
        public Int32 locStart;
        public Int32 locEnd;
        public String subName;

        //Visual representation: 

        public StackPanel subroutineStackPanel;

        public TextBlock subroutineHeadingBlock;

        public string additionalText = "";

        public List<CLine> lineList = new();

        public CSubroutinePanel(Int32 start, string extraText = "", Int32 end = 0)
        {
            subroutineHeadingBlock = new();
            subroutineStackPanel = new();

            locStart = start;
            locEnd = end;

            this.additionalText = extraText;

            subName = locStart.ToString("X4");

            subroutineHeadingBlock.Text = "\n                                  SUB 0x" + subName + extraText;

            subroutineStackPanel.Children.Add(subroutineHeadingBlock);

            StyleElements();
        }

        private void StyleElements()
        {
            subroutineHeadingBlock.Foreground = new SolidColorBrush(Color.FromRgb(235, 180, 52));
            subroutineHeadingBlock.FontWeight = FontWeights.Bold;
        }

    }
}
