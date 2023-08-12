using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GorkhonScriptEditor.ViewModel
{
    public sealed partial class CMainWindowViewModel : CommunityToolkit.Mvvm.ComponentModel.ObservableRecipient
    {
        [ObservableProperty]
        CScript mainScript;

        [ObservableProperty]
        string windowTitle = "Gorkhon Script Editor: no script loaded";

        [ObservableProperty]
        ObservableCollection<CLine> listLines = new();

        public enum ProgramState
        {
            Opened,
            Loaded
        }

        [ObservableProperty]
        public int gotoTarget = 0;

        public ProgramState state = ProgramState.Opened;

        public CMainWindowViewModel() { }

        [ObservableProperty]
        public CLine lineOfInterest;

        [RelayCommand]
        private void GotoLine(string tar)
        {
            if (state == ProgramState.Loaded)
            {
                try
                {
                    Int32 targetID = Convert.ToInt32(tar, 16);

                    var line = MainScript.Lines[0];
                    //HAXX: reset focus to line 0 so that OnSelectionChange fires - fixes inability to scroll back to a previously selected line;
                    LineOfInterest = line;
                    foreach (var ln in MainScript.Lines)
                    {
                        if (ln.instructionRef != null && ln.instructionRef.ID == targetID)
                        {
                            line = ln;
                            break;
                        }
                    }
                    LineOfInterest = line;
                }
                catch 
                {
                    //Handle "not a valid line number" exceptions here in a future release
                }
            }
        }

        [RelayCommand]
        private void LoadBinary()
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension  
            openFileDialog.DefaultExt = ".bin";
            openFileDialog.Filter = "Pathologic script binaries|*.bin";


            Nullable<bool> result = openFileDialog.ShowDialog();

            if (result == true)
            {
                byte[] binaryData = System.IO.File.ReadAllBytes(openFileDialog.FileName);

                String byteDisplay = System.BitConverter.ToString(binaryData).Replace('-', ' ');

                WindowTitle = "Gorkhon Script Editor: " + openFileDialog.FileName;

                MainScript = new CScript(binaryData);

                state = ProgramState.Loaded;
                LineOfInterest = MainScript.Lines[0];

                //Save a backup file here
                System.IO.File.WriteAllBytes(openFileDialog.FileName + ".gse_bak", binaryData);
            }
        }

        [RelayCommand]
        private void ExportScript()
        {

            if (state == ProgramState.Loaded)
            {
                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();

                //saveFileDialog.DefaultExt = ".muscript";
                saveFileDialog.DefaultExt = ".txt";
                saveFileDialog.AddExtension = true;
                saveFileDialog.Filter = "Parsed script (txt)|*.txt|Pathologic script file|*.bin";

                Nullable<bool> result = saveFileDialog.ShowDialog();

                if (result == true)
                {
                    String fileName = saveFileDialog.FileName;

                    if (saveFileDialog.FilterIndex == 1)
                    {
                        //System.IO.File.WriteAllLines(fileName, scriptCurrent.stringRepresentation);
                        //MessageBox.Show("Text exporting temporarily disabled until parsing into text is reworked");
                        return;
                    }
                    if (saveFileDialog.FilterIndex == 2)
                    {
                        foreach (var line in ListLines)
                        {
                            line.UpdateInstructionFromEditor();
                        }
                        MainScript.UpdateBinaryRepresentation();
                        System.IO.File.WriteAllBytes(fileName, MainScript.binaryData);
                        return;
                    }

                }
            }
        }

    }
}
