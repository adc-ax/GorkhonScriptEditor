using GorkhonScriptEditor.ViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;


namespace GorkhonScriptEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    [ValueConversion(typeof(CFunction), typeof(String))]
    public class FunctionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return "test conversion";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CFunction test = new("testback", 2, 2);
            return test;
        }
    }

    public partial class MainWindow : Window
    {


        public MainWindow()
        {
            InitializeComponent();
            PreviewMouseWheel += Window_PreviewMouseWheel;
        }

        private void Window_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            //if (Keyboard.Modifiers == ModifierKeys.Control)
            //{
            //    if (e.Delta > 0)
            //    {
            //        if (LinesScrollViewer.IsMouseOver)
            //            LinesScrollViewer.FontSize++;
            //    }

            //    else if (e.Delta < 0)
            //    {
            //        if (LinesScrollViewer.IsMouseOver)
            //            LinesScrollViewer.FontSize--;
            //    }
            //}
            //return;

        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Manual_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Early-stage work in progress, importing more complex scripts can result in a crash, editing is somewhat limited for now. Suggested course of action: redirect subroutine of interest to a new one appended to the end of the file","Help");
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Gorkhon Script Editor 0.1.1\nhttps://github.com/adc-ax/GorkhonScriptEditor\nNo affiliation with Ice-Pick Lodge \nAdditional research: RoSoDude, DigiDragon7, \nTilalgis, rathologic \nBuild date: 08/20/2023", "About");
        }

        private void OpenParsed_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Under construction");
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count>0 && e.AddedItems[0] != null) { LinesScrollViewer.ScrollIntoView(e.AddedItems[0]); }
            
        }

        private void SubButton_Click(object sender, RoutedEventArgs e)
        {
           // LinesScrollViewer.ScrollIntoView(LinesScrollViewer.Items[15]);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Remove later
        }
    }
}
