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
            Loaded,
            Broken
        }

        [ObservableProperty]
        public int gotoTarget = 0;

        public ProgramState state = ProgramState.Opened;

        public CMainWindowViewModel() { }

        [ObservableProperty]
        public CLine lineOfInterest;

        [ObservableProperty]
        public bool interfaceEnabled = false;

        public List<int> lineNav = new List<int>();

        //#TODO: implement proper forward/back navigation system
        //Make sure the list does not get expanded by pressing the forward/back arrows, this is paramount

        int navIndex = 0;

        [ObservableProperty]
        public List<string> navList = new();

        [ObservableProperty]
        public bool canGoForward = false;

        [ObservableProperty]
        public bool canGoBack = false;

        private bool addLine = true;

        [RelayCommand]
        public void GotoLine(string tar)
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
                    if (addLine) 
                    {
                        NavList.Add(tar);
                        if (NavList.Count > 1) { CanGoBack = true; }
                        
                        navIndex = navList.Count - 1;
                        CanGoForward = false;
                    }
                    addLine = true;                        
                }
                catch 
                {
                    //Handle "not a valid line number" exceptions here in a future release
                }
            }
        }

        [RelayCommand]
        public void GoForward() 
        {
            if (CanGoForward && navIndex < NavList.Count-1) 
            {
                navIndex++;
                addLine = false;
                GotoLine(NavList[navIndex]);
                if (navIndex == NavList.Count-1) 
                {
                    CanGoForward = false;
                }
                addLine = true;
                CanGoBack = true;
            }
            
        }

        [RelayCommand]
        public void GoBack() 
        {
            if (CanGoBack && navIndex > 0) 
            {
                navIndex--;
                addLine = false;
                GotoLine(NavList[navIndex]);
                if (navIndex == 0) 
                {
                    CanGoBack = false;
                }
                CanGoForward = true;
                addLine = true;
            }
            
        }

        [RelayCommand]
        public void AddFunction() 
        {

            try
            {
                Int32 args = Convert.ToInt32(NewFunctionArguments, 10);
                MainScript.AddFunction(NewFunctionName, args);
                MainScript.UpdateBinaryRepresentation();
            }
            catch 
            { 
            
            }

                
        }
        
        [RelayCommand]
        public void AddString() 
        {
            if (NewStringText != "") 
            {
                MainScript.AddString(NewStringText,IsUTF8);
                MainScript.UpdateBinaryRepresentation();
            }
        }

        [ObservableProperty]
        private string newFunctionName = "Function name";

        [ObservableProperty]
        private string newFunctionArguments = "Number of arguments";

        [ObservableProperty]
        private string newStringText = "";

        [ObservableProperty]
        private bool isUTF8 = false;

        [RelayCommand]
        public void AddInstruction() 
        {
            MainScript.AddInstruction(0x00);
            MainScript.UpdateBinaryRepresentation();
        }

        [RelayCommand]
        public void RecreateScript()
        {
            foreach (var line in ListLines)
            {
                line.UpdateInstructionFromEditor();
            }
            MainScript.RecreateScript();
        }

        [RelayCommand]
        public void LoadBinary()
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

                try
                {
                    MainScript = new CScript(binaryData);

                    //Perhaps find a more elegant solution
                    MainScript.ScriptName = openFileDialog.FileName.Split('\\')[^1];

                    state = ProgramState.Loaded;
                    InterfaceEnabled = true;

                    //Temporarily disabling this
                    //LineOfInterest = MainScript.Lines[0];

                    //Save a backup file here
                    System.IO.File.WriteAllBytes(openFileDialog.FileName + ".gse_bak", binaryData);

                } catch (ArgumentException)
                {
                    // If the script failed to load, do not enable the interface
                    WindowTitle = "Gorkhon Script Editor: failed to load " + openFileDialog.FileName;
                    // TODO: somehow display exception message in the main window

                    state = ProgramState.Broken;
                    InterfaceEnabled = false;
                    // Workaround - ideally we would close the open script instead, once
                    // "close current script" functionality is added. Need to ensure that
                    // a load failure doesn't cause unintended behavior if the user resumes
                    // interacting with the previously successfully loaded script.
                }

            }
        }

        [RelayCommand]
        public void ExportScript()
        {

            if (state == ProgramState.Loaded)
            {
                RecreateScript();
                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();

                //saveFileDialog.DefaultExt = ".muscript";
                saveFileDialog.DefaultExt = ".bin";
                saveFileDialog.FileName = MainScript.ScriptName;
                saveFileDialog.AddExtension = true;
                saveFileDialog.Filter = "Pathologic script file|*.bin|Parsed script (txt)|*.txt";

                Nullable<bool> result = saveFileDialog.ShowDialog();

                if (result == true)
                {
                    String fileName = saveFileDialog.FileName;

                    if (saveFileDialog.FilterIndex == 2)
                    {
                        String[] test = { "Text exporting temporarily disabled", "until parsing into text is reworked" };
                        System.IO.File.WriteAllLines(fileName, test);
                        //MessageBox.Show("Text exporting temporarily disabled until parsing into text is reworked");
                        return;
                    }
                    if (saveFileDialog.FilterIndex == 1)
                    {
                        foreach (var line in ListLines)
                        {
                            line.UpdateInstructionFromEditor();
                        }
                        MainScript.UpdateBinaryRepresentation();
                        WindowTitle = "Gorkhon Script Editor: " + saveFileDialog.FileName;
                        MainScript.ScriptName = saveFileDialog.FileName.Split('\\')[^1];
                        System.IO.File.WriteAllBytes(fileName, MainScript.binaryData);
                        return;
                    }

                }
            }
        }

    }
}
