using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;

namespace GorkhonScriptEditor
{
    public partial class CTask : ObservableObject
    {
        Int32 numVariables;
        List<byte> variableTypes;

        [ObservableProperty]
        List<CEvent> listEvents;
        Int32 numVariablesFromCode;
        //private List<CEvent> taskCEvents;

        public int ID = 1337;

        public CTask(Int32 numVars, List<byte> varTypes, Int32 numVarsFromCode, List<CEvent> listEvs, int id)
        {
            numVariables = numVars;
            variableTypes = varTypes;
            listEvents = listEvs;
            numVariablesFromCode = numVarsFromCode;
            ID = id;
        }

        public override string ToString()
        {
            string ret = "";
            ret += "Task " + ID.ToString() + ": " + numVariables.ToString() + " vars: ";

            
            foreach (var type in variableTypes)
            {
                string vartype = "";
                switch (type)
                {
                    case 0x00: vartype = "var"; break;
                    case 0x01: vartype = "bool"; break;
                    case 0x02: vartype = "int"; break;
                    case 0x03: vartype = "float"; break;
                    case 0x04: vartype = "string"; break;
                    case 0x05: vartype = "pointer"; break;
                    case 0x06: vartype = "coordinates"; break;
                    default: break;
                }
                ret += vartype + "; ";
            }

            ret += "; " + numVariablesFromCode.ToString() + " instr vars, " + ListEvents.Count.ToString() + " events";
            return ret;
        }
    }
}
