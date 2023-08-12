using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GorkhonScriptEditor
{
    public class CLabelPanel
    {
        public Int32 location;
        public StackPanel labelStackPanel;

        public TextBlock labelHeadingBlock;

        public CLabelPanel(Int32 loc)
        {
            labelHeadingBlock = new();
            labelStackPanel = new();

            location = loc;

            labelHeadingBlock.Text = "\nLABEL 0x" + location.ToString("X5");

            labelStackPanel.Children.Add(labelHeadingBlock);

            StyleElements();
        }

        private void StyleElements()
        {
            labelHeadingBlock.Foreground = new SolidColorBrush(Color.FromRgb(0, 180, 52));
            labelHeadingBlock.FontWeight = FontWeights.Bold;
        }

    }
}
