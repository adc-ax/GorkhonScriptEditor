using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using GorkhonScriptEditor.Instructions;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Reflection.Emit;
using System.Text;
using System.Windows.Documents;
using System.Windows.Forms;

namespace GorkhonScriptEditor
{
    [NotifyPropertyChangedRecipients]
    public partial class CScript : ObservableRecipient
    {

        public byte[] binaryData;

        //#TODO: Review and refactor all of this

        private UInt32 numGlobalVars;
        private String stringBlock;
        private List<byte> stringBlockBytes;
        private UInt32 numInstructions;
        private UInt32 numInstructionsOffset = 0;
        private Int32 stringBlockLength;
        private CControlFlowGraph flowGraph;
        private Int32 offset;
        private int stringBlockOffset;

        [ObservableProperty]
        public ObservableCollection<CFunction> listFunctions;

        public List<short> listGlobalVarTypes;

        [ObservableProperty]
        public List<CGlobalVar> listGlobalVars;
        private List<int> listLabelIndices;

        private List<Int32> listSubroutineIndices;
        private List<String> listLines;

        //#TODO: Implement data constant extraction
        [ObservableProperty]
        public List<CData> dataConstants;

        //#TODO: Fully integrate CString
        public Dictionary<Int32, String> listStringConstants;
        public Dictionary<Int32, bool> dictStringFormats;

        [ObservableProperty]
        public ObservableCollection<CString> stringConstants;

        public List<IInstruction> listInstructions;

        [ObservableProperty]
        public Int32 sectionAA;
        [ObservableProperty]
        public Int32 sectionAB;

        private int SectionAAoffset;

        [ObservableProperty]
        public List<CLabel> labels;

        [ObservableProperty]
        public List<CSubroutine> subroutines;

        [ObservableProperty]
        public List<CTask> sectionBTaskList;

        [ObservableProperty]
        public List<CEvent> taskCEvents;

        [ObservableProperty]
        public ObservableCollection<CLine> lines;

        public List<string> stringRepresentation;

        public int numFunctions;

        //#TODO: refactor function adding / binary reconstruction

        public int numFunctionsOffset = 0;

        public string ScriptName = "";

        public Int32 numStringConstants;

        public Int32 InstructionBlockOffset;
        //#TODO: throughout this file remove all the relaycommand/observableproperty stuff to move it into the viewmodel eventually for better separation
        
        public void RecreateScript() 
        {
            UpdateBinaryRepresentation();
            UpdateFromBinary(this.binaryData);
        }

        private string GetStringConstantFromBlock(int startingOffset)
        {
            string result = "";
            for (int i = startingOffset; i < stringBlockBytes.Count - 1;)
            {
                if (stringBlockBytes.ElementAt(i) != 0)
                {
                    if (stringBlockBytes.ElementAt(i + 1) != 0)
                    {
                        int off = 0;
                        while (stringBlockBytes.ElementAt(i + 1 + off) != 0)
                        {
                            off++;
                        }
                        result = System.Text.Encoding.UTF8.GetString((new ArraySegment<byte>(binaryData, stringBlockOffset + i, off + 1)).ToArray());
                        //listStringConstants.Add(i, result);
                        i += off + 1;
                        return result;
                    }
                    else
                    {
                        int off = 0;
                        while (stringBlockBytes.ElementAt(i + off) != 0)
                        {
                            off += 2;
                        }
                        result = System.Text.Encoding.Unicode.GetString((new ArraySegment<byte>(binaryData, stringBlockOffset + i, off)).ToArray());
                        //listStringConstants.Add(i, result);
                        i += off;
                        return result;
                    }
                }
                else
                {
                    i++;
                }

            }
            return result;
        }

        public CScript(byte[] binData)
        {
            UpdateFromBinary(binData);
        }

        private void CreateFlowGraph()
        {
            flowGraph = new(listInstructions);
        }

        public void AddFunction(string name, int args) 
        {
                ListFunctions.Add(new(name, args, ListFunctions.Count));
        }

        public void AddString(string text, bool isUTF8) 
        {
            int offset = StringConstants.Last().StartOffset + StringConstants.Last().BinaryRepresentation.Count + (StringConstants.Last().IsUTF8 ? 2 : 1);
            var bytes = Encoding.Unicode.GetBytes(text);
            if (!isUTF8) 
            { 
                bytes = Encoding.Unicode.GetBytes(text); 
            }
            else
            {
                bytes = Encoding.UTF8.GetBytes(text);
            }
            List<byte> listBytes = new();
            listBytes.AddRange(bytes);
            CString stringConstant = new(!isUTF8, text, offset, listBytes);
            StringConstants.Add(stringConstant);
        }

        public IInstruction AddInstruction(byte OPCode) 
        {
            IInstruction lastIns = listInstructions[^1];
            int offset = 0;
            byte[] arrayy = {0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00};
            
            IInstruction newIns = createInstruction(OPCode, (int)lastIns.ID+1, lastIns.ByteOffset + (uint)lastIns.binaryRepresentation.Count(),ref offset, ref arrayy);
            listInstructions.Add(newIns);
            return newIns;
        }

        private IInstruction createInstruction(byte OPCode, Int32 insID, UInt32 byteOffset, ref Int32 offset, ref byte[] binaryData)
        {
            List<object> args = new();
            List<byte> binaryRepresentation = new();
            binaryRepresentation.Add(OPCode);
            binaryRepresentation.Add((byte)0);
            int binarySize;
            IInstruction newIns;
            
            switch (OPCode)
            {
                // 0x00 Mov
                case 0x00:
                    {
                        binarySize = 8;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        int sourceRegister = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(sourceRegister);
                        offset += 4;
                        int targetRegister = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(targetRegister);
                        offset += 4;
                        newIns = new CInstructionMov(args, binaryRepresentation);
                        break;
                    }
                // 0x01 MovB
                case 0x01:
                    {
                        binarySize = 5;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        int targetRegister = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(targetRegister);
                        offset += 4;
                        var boolConst = System.BitConverter.ToBoolean(binaryData.AsSpan<byte>(offset, 1));
                        offset += 1;
                        args.Add(boolConst);
                        newIns = new CInstructionMovB(args, binaryRepresentation);
                        break;
                    }
                // 0x02 MovI
                case 0x02:
                    {
                        binarySize = 8;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 targetRegister = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(targetRegister);
                        offset += 4;
                        Int32 intConstant = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(intConstant);
                        offset += 4;
                        newIns = new CInstructionMovI(args, binaryRepresentation);
                        break;
                    }
                // 0x03 MovF
                case 0x03:
                    {
                        binarySize = 8;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 targetRegister = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(targetRegister);
                        offset += 4;
                        float floatConstant = System.BitConverter.ToSingle(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(floatConstant);
                        offset += 4;
                        newIns = new CInstructionMovF(args, binaryRepresentation);
                        break;
                    }
                // 0x04 MovS
                case 0x04:
                    {
                        binarySize = 8;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 targetRegister = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(targetRegister);
                        offset += 4;
                        Int32 stringOffset = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(stringOffset);
                        offset += 4;
                        newIns = new CInstructionMovS(args, binaryRepresentation, GetStringConstantByOffset(stringOffset));
                        break;
                    }
                // 0x05 MovV
                case 0x05:
                    {
                        binarySize = 16;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 targetRegister = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(targetRegister / 2);
                        offset += 4;

                        float x = System.BitConverter.ToSingle(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(x);
                        offset += 4;

                        float y = System.BitConverter.ToSingle(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(y);
                        offset += 4;

                        float z = System.BitConverter.ToSingle(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(z);
                        offset += 4;

                        newIns = new CInstructionMovV(args, binaryRepresentation);
                        break;
                    }
                // 0x06 MovT
                case 0x06:
                    {
                        binarySize = 8;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 sourceRegister = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(sourceRegister);
                        offset += 4;
                        Int32 targetRegister = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(targetRegister);
                        offset += 4;
                        newIns = new CInstructionMovT(args, binaryRepresentation);
                        break;
                    }
                // 0x07 TMov - STUB
                case 0x07:
                    {
                        binarySize = 8;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 sourceRegister = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(sourceRegister);
                        offset += 4;
                        Int32 targetRegister = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(targetRegister);
                        offset += 4;
                        newIns = new CInstructionTMov(args, binaryRepresentation);
                        break;
                    }
                // 0x08 TMovB
                case 0x08:
                    {
                        binarySize = 5;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 targetRegister = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(targetRegister);
                        offset += 4;
                        Boolean boolConst = System.BitConverter.ToBoolean(binaryData.AsSpan<byte>(offset, 1));
                        offset += 1;
                        args.Add(boolConst);
                        newIns = new CInstructionTMovB(args, binaryRepresentation);
                        break;
                    }
                // 0x09 TMovI 
                case 0x09:
                    {
                        binarySize = 8;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 targetRegister = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(targetRegister);
                        offset += 4;
                        Int32 intConstant = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(intConstant);
                        offset += 4;
                        newIns = new CInstructionTMovI(args, binaryRepresentation);
                        break;
                    }
                // 0x0A TMovF
                case 0x0A:
                    {
                        binarySize = 8;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 targetRegister = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(targetRegister);
                        offset += 4;
                        float floatConstant = System.BitConverter.ToSingle(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(floatConstant);
                        offset += 4;
                        newIns = new CInstructionTMovF(args, binaryRepresentation);
                        break;
                    }
                // 0x0B TMovS 
                case 0x0B:
                    {
                        binarySize = 8;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 targetRegister = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(targetRegister);
                        offset += 4;
                        Int32 stringOffset = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(stringOffset / 2);
                        offset += 4;
                        newIns = new CInstructionTMovS(args, binaryRepresentation);
                        //listStringConstants.Add((stringOffset,"test"));
                        break;
                    }
                // 0x0C TMovV - STUB
                case 0x0C:
                    {
                        binarySize = 16;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 targetRegister = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(targetRegister / 2);
                        offset += 4;

                        float x = System.BitConverter.ToSingle(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(x);
                        offset += 4;

                        float y = System.BitConverter.ToSingle(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(y);
                        offset += 4;

                        float z = System.BitConverter.ToSingle(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(z);
                        offset += 4;

                        newIns = new CInstructionTMovV(args, binaryRepresentation);
                        break;
                    }
                // 0x0D TMovT - STUB
                case 0x0D:
                    {
                        binarySize = 8;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 sourceRegister = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(sourceRegister);
                        offset += 4;
                        Int32 targetRegister = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(targetRegister);
                        offset += 4;
                        newIns = new CInstructionTMovT(args, binaryRepresentation);
                        break;
                    }
                // 0x0E Jump
                case 0x0E:
                    {
                        binarySize = 4;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 targetInstruction = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(targetInstruction);
                        offset += 4;
                        newIns = new CInstructionJump(args, binaryRepresentation);
                        if (!listLabelIndices.Contains(targetInstruction))
                        {
                            Labels.Add(new CLabel(targetInstruction));
                            listLabelIndices.Add(targetInstruction);
                        }
                        Labels.Find(i => i.location == targetInstruction).Calls.Add(insID);
                        //Labels.ElementAt(targetInstruction).Calls.Add(insID);
                        break;
                    }
                // 0x0F JumpB
                case 0x0F:
                    {
                        binarySize = 11;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 conditionPointer = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(conditionPointer);
                        offset += 4;
                        Int32 targetInstruction = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(targetInstruction);
                        offset += 4;
                        //Don't forget to read one more byte here
                        Boolean boolConst = System.BitConverter.ToBoolean(binaryData.AsSpan<byte>(offset, 1));
                        offset += 1;
                        args.Add(boolConst);
                        //But for now just skip over it
                        Int16 comparison = System.BitConverter.ToInt16(binaryData.AsSpan<byte>(offset, 2));
                        offset += 2;
                        args.Add(comparison);
                        newIns = new CInstructionJumpB(args, binaryRepresentation);
                        if (!listLabelIndices.Contains(targetInstruction))
                        {
                            Labels.Add(new CLabel(targetInstruction));
                            listLabelIndices.Add(targetInstruction);
                        }
                        Labels.Find(i => i.location == targetInstruction).Calls.Add(insID);
                        //Labels.ElementAt(targetInstruction).Calls.Add(insID);
                        break;
                    }
                // 0x10 Push
                case 0x10:
                    {
                        binarySize = 4;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 srcRegister = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(srcRegister);
                        offset += 4;
                        newIns = new CInstructionPush(args, binaryRepresentation);
                        break;
                    }
                // 0x11 PushB
                case 0x11:
                    {
                        binarySize = 1;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Boolean boolConst = System.BitConverter.ToBoolean(binaryData.AsSpan<byte>(offset, 1));
                        offset += 1;
                        args.Add(boolConst);
                        newIns = new CInstructionPushB(args, binaryRepresentation);
                        newIns.ID = (uint)insID;
                        CData cData = new CData(CData.Type.Boolean, boolConst, newIns);
                        DataConstants.Add(cData);
                        break;
                    }
                // 0x12 PushI
                case 0x12:
                    {
                        binarySize = 4;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 intConstant = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(intConstant);
                        offset += 4;
                        newIns = new CInstructionPushI(args, binaryRepresentation);
                        newIns.ID = (uint)insID;
                        CData cData = new CData(CData.Type.Integer, intConstant, newIns);
                        DataConstants.Add(cData);
                        break;
                    }
                // 0x13 PushF
                case 0x13:
                    {
                        binarySize = 4;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        float floatConstant = System.BitConverter.ToSingle(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(floatConstant);
                        offset += 4;
                        newIns = new CInstructionPushF(args, binaryRepresentation);
                        newIns.ID = (uint)insID;
                        CData cData = new CData(CData.Type.Float, floatConstant, newIns);
                        DataConstants.Add(cData);
                        break;
                    }
                // 0x14 PushS
                case 0x14:
                    {
                        binarySize = 4;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 dataOffset = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(dataOffset);
                        offset += 4;
                        string strConst;
                        strConst = GetStringConstantByOffset(dataOffset);
                        newIns = new CInstructionPushS(args, binaryRepresentation, strConst);

                        break;
                    }
                // 0x15 PushT
                case 0x15:
                    {
                        binarySize = 4;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 dataOffset = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(dataOffset);
                        offset += 4;
                        newIns = new CInstructionPushT(args, binaryRepresentation);
                        break;
                    }
                // 0x16 PushVec
                case 0x16:
                    {
                        binarySize = 12;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        float x = System.BitConverter.ToSingle(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(x);
                        offset += 4;

                        float y = System.BitConverter.ToSingle(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(y);
                        offset += 4;

                        float z = System.BitConverter.ToSingle(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(z);
                        offset += 4;

                        newIns = new CInstructionPushVec(args, binaryRepresentation);
                        newIns.ID = (uint)insID;
                        List<float> coordsConstant = new List<float>();
                        coordsConstant.Add(x);
                        coordsConstant.Add(y);
                        coordsConstant.Add(z);

                        CData cData = new CData(CData.Type.Coordinates, coordsConstant, newIns);
                        DataConstants.Add(cData);

                        break;
                    }
                // 0x17 PushV
                case 0x17:
                    {
                        Int32 vectorSize = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        binarySize = 4 + vectorSize;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        args.Add(vectorSize);
                        offset += 4;
                        byte[] bytes = new ArraySegment<byte>(binaryData, offset, vectorSize).ToArray();
                        args.Add(bytes);
                        offset += vectorSize;
                        newIns = new CInstructionPushV(args, binaryRepresentation);
                        break;
                    }
                // 0x18 PushE
                case 0x18:
                    {
                        binarySize = 9;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 lhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(lhs);
                        offset += 4;
                        byte carry = binaryData[offset];
                        offset += 1;
                        args.Add(carry);
                        Int32 rhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(rhs);
                        offset += 4;

                        newIns = new CInstructionPushE(args, binaryRepresentation);
                        break;
                    }
                // 0x19 PushGE
                case 0x19:
                    {
                        /*binarySize = 9;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 lhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(lhs);
                        offset += 4;
                        byte carry = binaryData[offset];
                        offset += 1;
                        args.Add(carry);
                        Int32 rhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(rhs);
                        offset += 1;

                        newIns = new CInstructionPushGE(args, binaryRepresentation);
                        */
                        binarySize = 4;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 lhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(lhs);
                        offset += 4;
                        newIns = new CInstructionPushGE(args, binaryRepresentation);
                        break;
                    }
                // 0x1A Pop
                case 0x1A:
                    {
                        binarySize = 4;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 popAmount = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(popAmount);
                        offset += 4;
                        newIns = new CInstructionPop(args, binaryRepresentation);
                        break;
                    }
                // 0x1B PopE
                case 0x1B:
                    {
                        binarySize = 9;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 lhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(lhs);
                        offset += 4;
                        byte carry = binaryData[offset];
                        offset += 1;
                        args.Add(carry);
                        Int32 rhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(rhs);
                        offset += 4;

                        newIns = new CInstructionPopE(args, binaryRepresentation);
                        break;
                    }
                // 0x1С PopGE - STUB
                case 0x1C:
                    {
                        binarySize = 5;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 lhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(lhs);
                        offset += 4;
                        byte carry = binaryData[offset];
                        offset += 1;
                        args.Add(carry);

                        newIns = new CInstructionPopGE(args, binaryRepresentation);
                        break;
                    }
                // 0x1D SetNull
                case 0x1D:
                    {
                        binarySize = 4;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 argument = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(argument);
                        offset += 4;
                        newIns = new CInstructionSetNull(args, binaryRepresentation);
                        break;
                    }
                // 0x1E SetNullT
                case 0x1E:
                    {
                        binarySize = 4;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 argument = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(argument);
                        offset += 4;
                        newIns = new CInstructionSetNullT(args, binaryRepresentation);
                        break;
                    }
                // 0x1F Add
                case 0x1f:
                    {
                        binarySize = 9;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 lhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(lhs);
                        offset += 4;
                        Int32 rhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(rhs);
                        offset += 4;
                        byte carry = binaryData[offset];
                        offset += 1;
                        args.Add(carry);
                        newIns = new CInstructionAdd(args, binaryRepresentation);
                        break;
                    }
                // 0x20 Sub
                case 0x20:
                    {
                        binarySize = 9;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 lhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(lhs);
                        offset += 4;
                        Int32 rhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(rhs);
                        offset += 4;
                        byte carry = binaryData[offset];
                        offset += 1;
                        args.Add(carry);
                        newIns = new CInstructionSub(args, binaryRepresentation);
                        break;
                    }
                // 0x21 Mult
                case 0x21:
                    {
                        binarySize = 9;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 lhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(lhs);
                        offset += 4;
                        Int32 rhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(rhs);
                        offset += 4;
                        byte carry = binaryData[offset];
                        offset += 1;
                        args.Add(carry);
                        newIns = new CInstructionMult(args, binaryRepresentation);
                        break;
                    }
                // 0x22 Div
                case 0x22:
                    {
                        binarySize = 9;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 lhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(lhs);
                        offset += 4;
                        Int32 rhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(rhs);
                        offset += 4;
                        byte carry = binaryData[offset];
                        offset += 1;
                        args.Add(carry);
                        newIns = new CInstructionDiv(args, binaryRepresentation);
                        break;
                    }
                // 0x23 Mod
                case 0x23:
                    {
                        binarySize = 9;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 lhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(lhs);
                        offset += 4;
                        Int32 rhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(rhs);
                        offset += 4;
                        byte carry = binaryData[offset];
                        offset += 1;
                        args.Add(carry);
                        newIns = new CInstructionMod(args, binaryRepresentation);
                        break;
                    }
                // 0x24 And
                case 0x24:
                    {
                        binarySize = 9;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 lhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(lhs);
                        offset += 4;
                        Int32 rhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(rhs);
                        offset += 4;
                        byte carry = binaryData[offset];
                        offset += 1;
                        args.Add(carry);
                        newIns = new CInstructionAnd(args, binaryRepresentation);
                        break;
                    }
                // 0x25 Or
                case 0x25:
                    {
                        binarySize = 9;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 lhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(lhs);
                        offset += 4;
                        Int32 rhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(rhs);
                        offset += 4;
                        byte carry = binaryData[offset];
                        offset += 1;
                        args.Add(carry);
                        newIns = new CInstructionOr(args, binaryRepresentation);
                        break;
                    }
                // 0x26 Xor 
                case 0x26:
                    {
                        binarySize = 9;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 lhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(lhs);
                        offset += 4;
                        Int32 rhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(rhs);
                        offset += 4;
                        byte carry = binaryData[offset];
                        offset += 1;
                        args.Add(carry);
                        newIns = new CInstructionXor(args, binaryRepresentation);
                        break;
                    }
                // 0x27 Eq
                case 0x27:
                    {
                        binarySize = 9;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 lhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(lhs);
                        offset += 4;
                        Int32 rhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(rhs);
                        offset += 4;
                        byte carry = binaryData[offset];
                        offset += 1;
                        args.Add(carry);
                        newIns = new CInstructionEq(args, binaryRepresentation);
                        break;
                    }
                // 0x28 Neq
                case 0x28:
                    {
                        binarySize = 9;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 lhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(lhs);
                        offset += 4;
                        Int32 rhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(rhs);
                        offset += 4;
                        byte carry = binaryData[offset];
                        offset += 1;
                        args.Add(carry);
                        newIns = new CInstructionNeq(args, binaryRepresentation);
                        break;
                    }
                // 0x29 LT
                case 0x29:
                    {
                        binarySize = 9;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 rhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(rhs);
                        offset += 4;
                        Int32 lhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(lhs);
                        offset += 4;
                        byte condition = binaryData[offset];
                        offset += 1;
                        args.Add(condition);
                        newIns = new CInstructionLT(args, binaryRepresentation);
                        break;
                    }
                // 0x2A GT
                case 0x2A:
                    {
                        binarySize = 9;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 rhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(rhs);
                        offset += 4;
                        Int32 lhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(lhs);
                        offset += 4;
                        byte condition = binaryData[offset];
                        offset += 1;
                        args.Add(condition);
                        newIns = new CInstructionGT(args, binaryRepresentation);
                        break;
                    }
                // 0x2B LE
                case 0x2B:
                    {
                        binarySize = 9;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 rhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(rhs);
                        offset += 4;
                        Int32 lhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(lhs);
                        offset += 4;
                        byte condition = binaryData[offset];
                        offset += 1;
                        args.Add(condition);
                        newIns = new CInstructionLE(args, binaryRepresentation);
                        break;
                    }
                // 0x2C GE
                case 0x2C:
                    {
                        binarySize = 9;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 rhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(rhs);
                        offset += 4;
                        Int32 lhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(lhs);
                        offset += 4;
                        byte condition = binaryData[offset];
                        offset += 1;
                        args.Add(condition);
                        newIns = new CInstructionGE(args, binaryRepresentation);
                        break;
                    }
                // 0x2D NullEq
                case 0x2D:
                    {
                        binarySize = 5;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 targetRegister = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(targetRegister);
                        offset += 4;
                        byte option = binaryData[offset];
                        args.Add(option);
                        offset += 1;
                        newIns = new CInstructionNullEq(args, binaryRepresentation);
                        break;
                    }
                // 0x2E NullNeq
                case 0x2E:
                    {
                        binarySize = 5;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 targetRegister = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(targetRegister);
                        offset += 4;
                        byte option = binaryData[offset];
                        args.Add(option);
                        offset += 1;
                        newIns = new CInstructionNullNeq(args, binaryRepresentation);
                        break;
                    }
                // 0x2F NEG - WRONG
                case 0x2F:
                    {
                        binarySize = 5;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 targetRegister = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(targetRegister);
                        offset += 4;
                        byte option = binaryData[offset];
                        args.Add(option);
                        offset += 1;
                        newIns = new CInstructionNeg(args, binaryRepresentation);
                        break;
                    }
                // 0x30 Not
                case 0x30:
                    {
                        binarySize = 5;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 targetRegister = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(targetRegister);
                        offset += 4;
                        byte option = binaryData[offset];
                        args.Add(option);
                        offset += 1;
                        newIns = new CInstructionNot(args, binaryRepresentation);
                        break;
                    }
                // 0x31 Add2
                case 0x31:
                    {
                        binarySize = 13;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 num1 = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(num1);
                        offset += 4;
                        Int32 num2 = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(num2);
                        offset += 4;
                        Int32 targetRegister = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(targetRegister);
                        offset += 4;
                        byte option = binaryData[offset];
                        args.Add(option);
                        offset += 1;
                        newIns = new CInstructionAdd2(args, binaryRepresentation);
                        break;
                    }
                // 0x32 Sub2
                case 0x32:
                    {
                        binarySize = 13;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 num1 = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(num1);
                        offset += 4;
                        Int32 num2 = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(num2);
                        offset += 4;
                        Int32 targetRegister = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(targetRegister);
                        offset += 4;
                        byte option = binaryData[offset];
                        args.Add(option);
                        offset += 1;
                        newIns = new CInstructionSub2(args, binaryRepresentation);
                        break;
                    }

                // 0x33 Mult2
                case 0x33:
                    {
                        binarySize = 13;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 num1 = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(num1);
                        offset += 4;
                        Int32 num2 = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(num2);
                        offset += 4;
                        Int32 targetRegister = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(targetRegister);
                        offset += 4;
                        byte option = binaryData[offset];
                        args.Add(option);
                        offset += 1;
                        newIns = new CInstructionMult2(args, binaryRepresentation);
                        break;
                    }
                // 0x34 Div2
                case 0x34:
                    {
                        binarySize = 13;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 num1 = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(num1);
                        offset += 4;
                        Int32 num2 = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(num2);
                        offset += 4;
                        Int32 targetRegister = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(targetRegister);
                        offset += 4;
                        byte option = binaryData[offset];
                        args.Add(option);
                        offset += 1;
                        newIns = new CInstructionDiv2(args, binaryRepresentation);
                        break;
                    }
                // 0x35 Mod2
                case 0x35:
                    {
                        binarySize = 13;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 num1 = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(num1);
                        offset += 4;
                        Int32 num2 = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(num2);
                        offset += 4;
                        Int32 targetRegister = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(targetRegister);
                        offset += 4;
                        byte option = binaryData[offset];
                        args.Add(option);
                        offset += 1;
                        newIns = new CInstructionMod2(args, binaryRepresentation);
                        break;
                    }
                // 0x36 And2
                case 0x36:
                    {
                        binarySize = 13;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 num1 = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(num1);
                        offset += 4;
                        Int32 num2 = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(num2);
                        offset += 4;
                        Int32 targetRegister = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(targetRegister);
                        offset += 4;
                        byte option = binaryData[offset];
                        args.Add(option);
                        offset += 1;
                        newIns = new CInstructionAnd2(args, binaryRepresentation);
                        break;
                    }
                // 0x37 Or2
                case 0x37:
                    {
                        binarySize = 13;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 num1 = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(num1);
                        offset += 4;
                        Int32 num2 = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(num2);
                        offset += 4;
                        Int32 targetRegister = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(targetRegister);
                        offset += 4;
                        byte option = binaryData[offset];
                        args.Add(option);
                        offset += 1;
                        newIns = new CInstructionOr2(args, binaryRepresentation);
                        break;
                    }
                // 0x38 Xor2
                case 0x38:
                    {
                        binarySize = 13;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 num1 = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(num1);
                        offset += 4;
                        Int32 num2 = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(num2);
                        offset += 4;
                        Int32 targetRegister = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(targetRegister);
                        offset += 4;
                        byte option = binaryData[offset];
                        args.Add(option);
                        offset += 1;
                        newIns = new CInstructionXor2(args, binaryRepresentation);
                        break;
                    }
                // 0x39 Eq2
                case 0x39:
                    {
                        binarySize = 13;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 num1 = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(num1);
                        offset += 4;
                        Int32 num2 = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(num2);
                        offset += 4;
                        Int32 targetRegister = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(targetRegister);
                        offset += 4;
                        byte option = binaryData[offset];
                        args.Add(option);
                        offset += 1;
                        newIns = new CInstructionEq2(args, binaryRepresentation);
                        break;
                    }
                // 0x3A Neq2 
                case 0x3A:
                    {
                        binarySize = 13;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 num1 = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(num1);
                        offset += 4;
                        Int32 num2 = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(num2);
                        offset += 4;
                        Int32 targetRegister = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(targetRegister);
                        offset += 4;
                        byte option = binaryData[offset];
                        args.Add(option);
                        offset += 1;
                        newIns = new CInstructionNeq2(args, binaryRepresentation);
                        break;
                    }
                // 0x3B LT2
                case 0x3B:
                    {
                        binarySize = 13;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 rhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(rhs);
                        offset += 4;
                        Int32 lhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(lhs);
                        offset += 4;
                        Int32 targetRegister = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(targetRegister);
                        offset += 4;
                        byte condition = binaryData[offset];
                        offset += 1;
                        args.Add(condition);
                        newIns = new CInstructionGT2(args, binaryRepresentation);
                        break;
                    }
                // 0x3C GT2
                case 0x3C:
                    {
                        binarySize = 13;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 rhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(rhs);
                        offset += 4;
                        Int32 lhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(lhs);
                        offset += 4;
                        Int32 targetRegister = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(targetRegister);
                        offset += 4;
                        byte condition = binaryData[offset];
                        offset += 1;
                        args.Add(condition);
                        newIns = new CInstructionGT2(args, binaryRepresentation);
                        break;
                    }
                // 0x3D LE2
                case 0x3D:
                    {
                        binarySize = 13;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 rhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(rhs);
                        offset += 4;
                        Int32 lhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(lhs);
                        offset += 4;
                        Int32 targetRegister = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(targetRegister);
                        offset += 4;
                        byte condition = binaryData[offset];
                        offset += 1;
                        args.Add(condition);
                        newIns = new CInstructionLE2(args, binaryRepresentation);
                        break;
                    }
                // 0x3E GE2
                case 0x3E:
                    {
                        binarySize = 13;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 rhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(rhs);
                        offset += 4;
                        Int32 lhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(lhs);
                        offset += 4;
                        Int32 targetRegister = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(targetRegister);
                        offset += 4;
                        byte condition = binaryData[offset];
                        offset += 1;
                        args.Add(condition);
                        newIns = new CInstructionGE2(args, binaryRepresentation);
                        break;
                    }
                // 0x3F NullEq2
                case 0x3F:
                    {
                        binarySize = 9;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 lhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(lhs);
                        offset += 4;
                        Int32 rhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(rhs);
                        offset += 4;
                        byte carry = binaryData[offset];
                        offset += 1;
                        args.Add(carry);
                        newIns = new CInstructionNullEq2(args, binaryRepresentation);
                        break;
                    }
                // 0x40 NullNeq2
                case 0x40:
                    {
                        binarySize = 9;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 lhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(lhs);
                        offset += 4;
                        Int32 rhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(rhs);
                        offset += 4;
                        byte carry = binaryData[offset];
                        offset += 1;
                        args.Add(carry);
                        newIns = new CInstructionNullNeq2(args, binaryRepresentation);
                        break;
                    }
                // 0x41 Neg2
                case 0x41:
                    {
                        binarySize = 9;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 lhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(lhs);
                        offset += 4;
                        Int32 rhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(rhs);
                        offset += 4;
                        byte carry = binaryData[offset];
                        offset += 1;
                        args.Add(carry);
                        newIns = new CInstructionNeg2(args, binaryRepresentation);
                        break;
                    }
                // 0x42 Not2
                case 0x42:
                    {
                        binarySize = 9;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 lhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(lhs);
                        offset += 4;
                        Int32 rhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(rhs);
                        offset += 4;
                        byte carry = binaryData[offset];
                        offset += 1;
                        args.Add(carry);
                        newIns = new CInstructionNot2(args, binaryRepresentation);
                        break;
                    }
                // 0x43 Sqrt
                case 0x43:
                    {
                        binarySize = 5;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 srcRegister = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(srcRegister);
                        offset += 4;
                        Boolean param = System.BitConverter.ToBoolean(binaryData.AsSpan<byte>(offset, 1));
                        offset += 1;
                        args.Add(param);
                        newIns = new CInstructionSqrt(args, binaryRepresentation);
                        break;
                    }
                // 0x44 Sqrt2
                case 0x44:
                    {
                        binarySize = 9;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 lhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(lhs);
                        offset += 4;
                        Int32 rhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(rhs);
                        offset += 4;
                        byte carry = binaryData[offset];
                        offset += 1;
                        args.Add(carry);
                        newIns = new CInstructionSqrt2(args, binaryRepresentation);
                        break;
                    }
                // 0x45 Sin
                case 0x45:
                    {
                        binarySize = 5;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 srcRegister = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(srcRegister);
                        offset += 4;
                        Boolean param = System.BitConverter.ToBoolean(binaryData.AsSpan<byte>(offset, 1));
                        offset += 1;
                        args.Add(param);
                        newIns = new CInstructionSin(args, binaryRepresentation);
                        break;
                    }
                // 0x46 Sin2
                case 0x46:
                    {
                        binarySize = 9;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 lhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(lhs);
                        offset += 4;
                        Int32 rhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(rhs);
                        offset += 4;
                        byte carry = binaryData[offset];
                        offset += 1;
                        args.Add(carry);
                        newIns = new CInstructionSin2(args, binaryRepresentation);
                        break;
                    }
                // 0x47 Cos
                case 0x47:
                    {
                        binarySize = 5;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 srcRegister = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(srcRegister);
                        offset += 4;
                        Boolean param = System.BitConverter.ToBoolean(binaryData.AsSpan<byte>(offset, 1));
                        offset += 1;
                        args.Add(param);
                        newIns = new CInstructionCos(args, binaryRepresentation);
                        break;
                    }
                // 0x48 Cos2
                case 0x48:
                    {
                        binarySize = 9;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 lhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(lhs);
                        offset += 4;
                        Int32 rhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(rhs);
                        offset += 4;
                        byte carry = binaryData[offset];
                        offset += 1;
                        args.Add(carry);
                        newIns = new CInstructionCos2(args, binaryRepresentation);
                        break;
                    }
                // 0x49 ASin
                case 0x49:
                    {
                        binarySize = 5;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 srcRegister = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(srcRegister);
                        offset += 4;
                        Boolean param = System.BitConverter.ToBoolean(binaryData.AsSpan<byte>(offset, 1));
                        offset += 1;
                        args.Add(param);
                        newIns = new CInstructionASin(args, binaryRepresentation);
                        break;
                    }
                // 0x4A ASin2
                case 0x4A:
                    {
                        binarySize = 9;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 lhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(lhs);
                        offset += 4;
                        Int32 rhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(rhs);
                        offset += 4;
                        byte carry = binaryData[offset];
                        offset += 1;
                        args.Add(carry);
                        newIns = new CInstructionASin2(args, binaryRepresentation);
                        break;
                    }
                // 0x4B Pow
                case 0x4b:
                    {
                        binarySize = 9;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 lhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(lhs);
                        offset += 4;
                        Int32 rhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(rhs);
                        offset += 4;
                        byte carry = binaryData[offset];
                        offset += 1;
                        args.Add(carry);
                        newIns = new CInstructionPow(args, binaryRepresentation);
                        break;
                    }
                // 0x4C Pow2
                case 0x4C:
                    {
                        binarySize = 13;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 num1 = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(num1);
                        offset += 4;
                        Int32 num2 = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(num2);
                        offset += 4;
                        Int32 targetRegister = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(targetRegister);
                        offset += 4;
                        byte option = binaryData[offset];
                        args.Add(option);
                        offset += 1;
                        newIns = new CInstructionPow2(args, binaryRepresentation);
                        break;
                    }
                // 0x4D CString
                case 0x4D:
                    {
                        binarySize = 5;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 srcRegister = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(srcRegister);
                        offset += 4;
                        Boolean param = System.BitConverter.ToBoolean(binaryData.AsSpan<byte>(offset, 1));
                        offset += 1;
                        args.Add(param);
                        newIns = new CInstructionCString(args, binaryRepresentation);
                        break;
                    }
                // 0x4E CString2
                case 0x4E:
                    {
                        binarySize = 9;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 lhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(lhs);
                        offset += 4;
                        Int32 rhs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(rhs);
                        offset += 4;
                        byte carry = binaryData[offset];
                        offset += 1;
                        args.Add(carry);
                        newIns = new CInstructionCString2(args, binaryRepresentation);
                        break;
                    }
                // 0x4F Call
                case 0x4F:
                    {
                        binarySize = 4;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 address = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(address);
                        offset += 4;
                        newIns = new CInstructionCall(args, binaryRepresentation);
                        if (!listSubroutineIndices.Contains(address))
                        {
                            Subroutines.Add(new CSubroutine(address));
                            listSubroutineIndices.Add(address);
                        }
                        Subroutines.Find(i => i.locStart == address).Calls.Add(insID);
                        //Subroutines.ElementAt(address).Calls.Add(insID);
                        break;
                    }
                // 0x50 Return
                case 0x50:
                    {
                        binarySize = 4;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 popAmount = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(popAmount);
                        offset += 4;
                        newIns = new CInstructionReturn(args, binaryRepresentation);
                        break;
                    }
                // 0x51 TaskCall
                case 0x51:
                    {
                        binarySize = 4;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 task = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(task);
                        offset += 4;
                        newIns = new CInstructionTaskCall(args, binaryRepresentation);
                        break;
                    }
                // 0x52 TaskReturn
                case 0x52:
                    {
                        newIns = new CInstructionTaskReturn(args, binaryRepresentation);
                        break;
                    }
                // 0x53 Func
                case 0x53:
                    {
                        binarySize = 0;
                        int startingOffset = offset;
                        Int32 funcID = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(funcID);
                        offset += 4;
                        binarySize += 4;
                        ListFunctions.ElementAt(funcID).Calls.Add(insID);
                        for (int i = 0; i < ListFunctions.ElementAt(funcID).NumArgs; i++)
                        {
                            Int32 arg = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                            args.Add(arg);
                            offset += 4;
                            offset += 1;
                            binarySize += 5;
                        }
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[startingOffset + i]);
                        }
                        args.Add(ListFunctions.ElementAt(funcID));
                        newIns = new CInstructionFunc(args, binaryRepresentation);
                        break;
                    }
                // 0x54 ObjFunc
                case 0x54:
                    {
                        binarySize = 8;
                        Int32 startingOffset = offset;
                        Int32 objectNumber = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(objectNumber);
                        offset += 4;
                        Int32 funcNameOffset = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(listStringConstants[funcNameOffset]);
                        offset += 4;
                        Int32 repeats = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        offset += 4;
                        binarySize += 4;
                        for (int i = 0; i < repeats; i++)
                        {
                            offset += 5;
                            binarySize += 5;
                        }
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[startingOffset + i]);
                        }
                        newIns = new CInstructionObjFunc(args, binaryRepresentation);
                        break;
                    }
                // 0x55 TObjFunc
                case 0x55:
                    {
                        binarySize = 8;
                        Int32 startingOffset = offset;
                        Int32 objectNumber = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(objectNumber);
                        offset += 4;
                        Int32 funcNameOffset = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(listStringConstants[funcNameOffset]);
                        offset += 4;
                        Int32 repeats = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        offset += 4;
                        binarySize += 4;
                        for (int i = 0; i < repeats; i++)
                        {
                            offset += 5;
                            binarySize += 5;
                        }
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[startingOffset + i]);
                        }
                        newIns = new CInstructionTObjFunc(args, binaryRepresentation);
                        break;
                    }
                // 0x56 EventEnable
                case 0x56:
                    {
                        binarySize = 4;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 targetEvent = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(targetEvent);
                        offset += 4;
                        newIns = new CInstructionEventEnable(args, binaryRepresentation);
                        break;
                    }
                // 0x57 EventDisable
                case 0x57:
                    {
                        binarySize = 4;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        Int32 targetEvent = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(targetEvent);
                        offset += 4;
                        newIns = new CInstructionEventDisable(args, binaryRepresentation);
                        break;
                    }
                // 0x58 FuncExist
                case 0x58:
                    {
                        //58 00 04 00 00 00 03 00 00 00 02 00 00 00 01
                        binarySize = 13;
                        int startingOffset = offset;
                        Int32 objectPointer = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(objectPointer);
                        offset += 4;

                        
                        Int32 funcID = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(funcID);
                        offset += 4;
                        

                        Int32 numArgs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(numArgs);
                        offset += 4;

                        byte popNum = binaryData[offset];
                        args.Add(popNum);
                        offset += 1;

                        /*for (int i = 0; i < binarySize; i++)
                        {
                            args.Add(binaryData[offset]);
                            offset += 1;
                            binarySize += 1;
                        }*/
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[startingOffset + i]);
                        }
                        newIns = new CInstructionFuncExist(args, binaryRepresentation);
                        break;
                    }
                // 0x59 FuncExist2
                case 0x59:
                    {
                        //59 00 05 00 00 00 04 00 00 00 03 00 00 00 02 00 00 00 01
                        binarySize = 17;
                        int startingOffset = offset;
                        Int32 objectPointer = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(objectPointer);
                        offset += 4;


                        Int32 funcID = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(funcID);
                        offset += 4;


                        Int32 numArgs = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(numArgs);
                        offset += 4;

                        Int32 destination = System.BitConverter.ToInt32(binaryData.AsSpan<byte>(offset, 4));
                        args.Add(destination);
                        offset += 4;

                        byte popNum = binaryData[offset];
                        args.Add(popNum);
                        offset += 1;

                        /*for (int i = 0; i < binarySize; i++)
                        {
                            args.Add(binaryData[offset]);
                            offset += 1;
                            binarySize += 1;
                        }*/
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[startingOffset + i]);
                        }
                        newIns = new CInstructionFuncExist2(args, binaryRepresentation);
                        break;
                    }
                default:
                    {
                        binarySize = 8;
                        for (int i = 0; i < binarySize; i++)
                        {
                            binaryRepresentation.Add(binaryData[offset + i]);
                        }
                        int one = 13;
                        int two = 37;
                        args.Add(one);
                        args.Add(two);
                        newIns = new CInstructionMov(args, binaryRepresentation);
                        break;
                    }
            }

            newIns.ByteOffset = byteOffset;
            newIns.ID = (uint)insID;

            return newIns;
        }

        private void CreateStringRepresentation()
        {
            foreach (KeyValuePair<Int32, string> constStr in listStringConstants)
            {
                stringRepresentation.Add(constStr.ToString() + "\n");
            }
            Int32 funID = 0;
            foreach (CFunction fun in ListFunctions)
            {
                stringRepresentation.Add(funID.ToString() + ": " + fun.Name + ", " + fun.NumArgs + " arguments.\n");
                funID++;
            }
            //#TODO REMOVE
            stringRepresentation.Add("//MAIN START\n");

            //Process instructions
            int ID = 0;

            foreach (IInstruction ins in listInstructions)
            {
                string lineString = "";
                lineString += "0x" + ID.ToString("X4") + ": ";
                lineString += ins.OPCode.ToString("X2") + "00 |";
                string argString = BitConverter.ToString(ins.binaryRepresentation.ToArray()).Replace('-', ' ');
                lineString += argString + " ";
                lineString += " ;" + ins.ToString();
                if (ins.OPCode == 0x14)
                {
                    string stringConstant;
                    if (!listStringConstants.TryGetValue(Convert.ToInt32(ins.Args[0]), out stringConstant))
                    {
                        stringConstant = GetStringConstantFromBlock(Convert.ToInt32(ins.Args[0]));
                    }
                    lineString += " \"" + stringConstant + "\"";

                }
                else
                {
                    if (ins.OPCode == 0x04)
                    {
                        string stringConstant;

                        if (!listStringConstants.TryGetValue(Convert.ToInt32(ins.Args[1]), out stringConstant))
                        {
                            stringConstant = GetStringConstantFromBlock(Convert.ToInt32(ins.Args[1]));
                        }
                        lineString += " \"" + stringConstant + "\"";

                    }
                }
                lineString += "\n";
                ID++;
                listLines.Add(lineString);
            }

            //Insert subs and labels into the list of lines
            ID = 0;

            foreach (CSubroutine sub in Subroutines)
            {
                int index = 0;
                foreach (CBasicBlock subStart in flowGraph.subStarts)
                {

                    if (subStart.start == sub.locStart)
                    {
                        sub.locEnd = flowGraph.subEnds[index];
                        continue;
                    }
                    index++;
                }
            }
            foreach (string s in listLines)
            {
                //Process subs
                foreach (CSubroutine sub in Subroutines)
                {
                    if (sub.locStart == ID)
                    {
                        string lineString = "";
                        lineString += "//SUBROUTINE " + sub.SubName + " START\n";
                        stringRepresentation.Add(lineString);
                    }
                    if (sub.locEnd == ID)
                    {
                        string lineString = "";
                        lineString += "//SUBROUTINE " + sub.SubName + " END\n";
                        stringRepresentation.Add(lineString);
                    }

                }
                //Process labels
                foreach (CLabel lab in Labels)
                {
                    if (lab.location == ID)
                    {
                        string lineString = "";
                        lineString += "//LABEL 0x" + lab.location.ToString("X2") + "\n";
                        stringRepresentation.Add(lineString);
                    }
                }
                stringRepresentation.Add(s);
                ID++;
            }

            stringRepresentation.Add("//MAIN END");
        }

        private string GetStringConstantByOffset(Int32 offset)
        {
            int offsetFromStart = -1;
            int baseOffset = -1;
            foreach (var strConst in listStringConstants)
            {
                if (offset >= strConst.Key)
                {
                    baseOffset = strConst.Key;
                }
                else
                {
                    offsetFromStart = offset - baseOffset;
                    if (dictStringFormats[baseOffset])
                    {
                        offsetFromStart /= 2;
                    }
                    return listStringConstants[baseOffset].Substring(offsetFromStart);
                }
            }
            if (baseOffset == -1)
                return "string parsing failed";
            else
            {
                offsetFromStart = offset - baseOffset;
                if (dictStringFormats[baseOffset])
                {
                    offsetFromStart /= 2;
                }
                return listStringConstants[baseOffset].Substring(offsetFromStart);
            }
        }

        public bool UpdateBinaryRepresentation()
        {
            // update header

            int offsetFromStart = 0;
            List<byte> updatedBytes = new();

            // update strings
            //Counting up to string block
            for (; offsetFromStart < stringBlockOffset-4; offsetFromStart++)
            {
                updatedBytes.Add(binaryData[offsetFromStart]);
            }

            int stringBlockSize = StringConstants.Last().StartOffset + StringConstants.Last().BinaryRepresentation.Count + (StringConstants.Last().IsUTF8 ? 2  : 1);
            byte[] sizeStringBlockBytes = BitConverter.GetBytes(stringBlockSize);


            //Add the 4-byte string block size first
            for (int i = 0; i < 4; i++)
            {
                binaryData[numInstructionsOffset + i] = sizeStringBlockBytes[i];
                updatedBytes.Add(sizeStringBlockBytes[i]);
                offsetFromStart++;
            }
            //Add every string constant afterwards
            foreach (var str in StringConstants) 
            {
                updatedBytes.AddRange(str.BinaryRepresentation);
                updatedBytes.Add(0x0);
                if (str.IsUTF8) 
                {
                    updatedBytes.Add(0x0);
                }
                //offsetFromStart += str.BinaryRepresentation.Count;
            }



            //Counting up to functions

            /* for (; offsetFromStart < numFunctionsOffset; offsetFromStart++)
             {
                 updatedBytes.Add(binaryData[offsetFromStart]);
             }*/


            offsetFromStart = numFunctionsOffset - 1;
            // update functions

            int funcNum = ListFunctions.Count;
            byte[] sizebytes = BitConverter.GetBytes(funcNum);

            for (int i = 0; i < 4; i++)
            {
                binaryData[numInstructionsOffset + i] = sizebytes[i];
                updatedBytes.Add(sizebytes[i]);
                offsetFromStart++;
            }

            foreach (var func in ListFunctions)
            {
                updatedBytes.AddRange(func.binaryRepresentation);
                offsetFromStart += func.binaryRepresentation.Count;
            }

            // update tasks + events

            for (int i = SectionAAoffset; i < numInstructionsOffset; i++)
            {
                updatedBytes.Add(binaryData[i]);

            }

            // update instructions

            int insnum = listInstructions.Count;
            sizebytes = BitConverter.GetBytes(insnum);

            for (int i = 0; i < 4; i++) 
            {
                binaryData[numInstructionsOffset + i] = sizebytes[i];
                updatedBytes.Add(sizebytes[i]);
                offsetFromStart++; 
            }
            

            /*for (; offsetFromStart < InstructionBlockOffset; offsetFromStart++)
            {
                updatedBytes.Add(binaryData[offsetFromStart]);
            }*/

            foreach (var ins in listInstructions)
            {
                updatedBytes.AddRange(ins.binaryRepresentation);
            }

            binaryData = updatedBytes.ToArray();
            UpdateFromBinary(binaryData);
            return true;
        }

        public bool UpdateFromBinary(byte[] binData)
        {
            binaryData = binData;

            //#TODO: Refactor into proper list names + abstract things into separate types

            ListFunctions = new();
            listInstructions = new();
            listLabelIndices = new();
            listSubroutineIndices = new();
            Labels = new();
            Subroutines = new();
            listLines = new();
            stringRepresentation = new();
            listStringConstants = new();
            stringBlockBytes = new();
            dictStringFormats = new();
            listGlobalVarTypes = new();
            ListGlobalVars = new();
            DataConstants = new();

            StringConstants = new();

            InstructionBlockOffset = 0;

            int instructionsRead = 0;
            string errorMessage = "";

            //Parsing script binary starts here

            if (binData.Length < 16) {
                // Sanity check before we try to extract numGlobalVars
                errorMessage = "File is too short to be a valid script: " + binData.Length + " bytes";
                MessageBox.Show(errorMessage, "Critical error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw new ArgumentException(errorMessage);
            }

            //Global variables block
            offset = 0;
            numGlobalVars = System.BitConverter.ToUInt32(binData.AsSpan<byte>(offset, 4));
            if (numGlobalVars < 0 | numGlobalVars > binData.Length) {
                // Avoid possible infinite loop
                errorMessage = "Invalid number of global variables: " + numGlobalVars;
                MessageBox.Show(errorMessage, "Critical error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw new ArgumentException(errorMessage);
            }
            offset += 4;

            for (int i = 0; i < numGlobalVars; i++)
            {
                byte varType = binData[offset];
                offset++;
                Boolean hasName = System.BitConverter.ToBoolean(binData.AsSpan<byte>(offset, 1));
                offset++;
                string varName = "N/A";
                if (hasName)
                {
                    try
                    {
                        byte varNameLength = binData[offset];
                        offset++;
                        varName = System.Text.Encoding.UTF8.GetString(new ArraySegment<byte>(binData, offset, varNameLength).ToArray());
                        offset += varNameLength;
                    }
                    catch (ArgumentException) {
                        errorMessage = "Failed to parse global variable #" + i + " of " + numGlobalVars;
                        MessageBox.Show(errorMessage, "Critical error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    }
                }
                CGlobalVar newVar = new(varType, varName);
                newVar.ID = i.ToString();
                listGlobalVars.Add(newVar);
                //listGlobalVarTypes.Add(item);

            }

            //String block
            try {
                Span<byte> stringBlockLengthBin = binData.AsSpan<byte>(offset, 4);
                stringBlockLength = System.BitConverter.ToInt32(stringBlockLengthBin);
                offset += 4;

                stringBlockOffset = offset;
                stringBlockBytes = (new ArraySegment<byte>(binData, offset, stringBlockLength)).ToArray().ToList();
            } catch (ArgumentException)
            {
                errorMessage = "Failed to parse string block of length " + stringBlockLength;
                MessageBox.Show(errorMessage, "Critical error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw new ArgumentException(errorMessage);
                // This error is unrecoverable; stop the loading process
            }

            //Detecting separate strings

            for (int i = 0; i < stringBlockBytes.Count - 1;)
            {
                if (stringBlockBytes.ElementAt(i) != 0)
                {
                    if (stringBlockBytes.ElementAt(i + 1) != 0)
                    {
                        Int32 off = 0;
                        while (stringBlockBytes.ElementAt(i + 1 + off) != 0)
                        {
                            off++;
                        }
                        //UTF-8 string detected, adding to the list
                        //legacy
                        listStringConstants.Add(i, System.Text.Encoding.UTF8.GetString((new ArraySegment<byte>(binData, offset + i, off + 1)).ToArray()));
                        dictStringFormats.Add(i, false);

                        //Updated
                        string text = System.Text.Encoding.UTF8.GetString((new ArraySegment<byte>(binData, offset + i, off + 1)).ToArray());

                        List<byte> bytes = (new ArraySegment<byte>(binData, offset + i, off + 1)).ToList<byte>();
                        CString stringConstant = new(false, text, i, bytes);
                        StringConstants.Add(stringConstant);
                        //StringConstants.Add(new CString(false, System.Text.Encoding.UTF8.GetString((new ArraySegment<byte>(binData, offset + i, off + 1)).ToArray()),i, (new ArraySegment<byte>(binData, offset + i, off + 1)).ToList());
                        i += off + 1;
                    }
                    else
                    {
                        Int32 off = 0;
                        while (stringBlockBytes.ElementAt(i + off) != 0)
                        {
                            off += 2;
                        }
                        //UTF-16 string detected, adding to the list

                        //legacy
                        listStringConstants.Add(i, System.Text.Encoding.Unicode.GetString((new ArraySegment<byte>(binData, offset + i, off)).ToArray()));
                        dictStringFormats.Add(i, true);

                        //updated

                        string text = System.Text.Encoding.Unicode.GetString((new ArraySegment<byte>(binData, offset + i, off)).ToArray());
                        List<byte> bytes = (new ArraySegment<byte>(binData, offset + i, off)).ToList<byte>();
                        CString stringConstant = new(true, text, i, bytes);
                        StringConstants.Add(stringConstant);

                        i += off;
                    }
                }
                else
                {
                    i++;
                }

            }

            numStringConstants = listStringConstants.Count;

            stringBlock = System.Text.Encoding.UTF8.GetString((new ArraySegment<byte>(binData, offset, stringBlockLength)).ToArray());

            offset += stringBlockLength;

            //Collect function info
            numFunctions = System.BitConverter.ToInt32(binData.AsSpan<byte>(offset, 4));
            numFunctionsOffset = offset;
            offset += 4;
            for (int i = 0; i < numFunctions; i++)
            {
                byte length = binaryData[offset];
                offset += 1;
                String name = System.Text.Encoding.UTF8.GetString((new ArraySegment<byte>(binData, offset, length)).ToArray());
                offset += length;
                Int32 numArgs = System.BitConverter.ToInt32(binData.AsSpan<byte>(offset, 4));
                offset += 4;
                ListFunctions.Add(new(name, numArgs, i));
            }
            //Updated Event parsing, based on Tilalgis's documentation:
            //Section A
            //Collect two integers
            

            SectionAA = System.BitConverter.ToInt32(binData.AsSpan<byte>(offset, 4));
            SectionAAoffset = offset;
            offset += 4;
            SectionAB = System.BitConverter.ToInt32(binData.AsSpan<byte>(offset, 4));
            offset += 4;

            if (!listSubroutineIndices.Contains(SectionAB))
            {
                Subroutines.Add(new CSubroutine(SectionAB));
                listSubroutineIndices.Add(SectionAB);
            }
            //Subroutines.Find(i => i.locStart == address).Calls.Add(insID);

            //Section B
            Int32 sectionBTaskNum = System.BitConverter.ToInt32(binData.AsSpan<byte>(offset, 4));
            offset += 4;
            SectionBTaskList = new();
            for (int i = 0; i < sectionBTaskNum; i++)
            {
                Int32 taskVarNum = System.BitConverter.ToInt32(binData.AsSpan<byte>(offset, 4));
                offset += 4;
                List<byte> varTypes = new();
                for (int j = 0; j < taskVarNum; j++)
                {
                    byte varType = binaryData[offset];
                    varTypes.Add(varType);
                    offset++;
                }
                Int32 taskVarFromCode = System.BitConverter.ToInt32(binData.AsSpan<byte>(offset, 4));
                offset += 4;
                Int32 taskEventNum = System.BitConverter.ToInt32(binData.AsSpan<byte>(offset, 4));
                offset += 4;
                List<CEvent> taskEvents = new();
                for (int j = 0; j < taskEventNum; j++)
                {
                    Int32 eventName = System.BitConverter.ToInt32(binData.AsSpan<byte>(offset, 4));
                    offset += 4;
                    Int32 eventStartInstruction = System.BitConverter.ToInt32(binData.AsSpan<byte>(offset, 4));
                    offset += 4;
                    Int32 eventVarNum = System.BitConverter.ToInt32(binData.AsSpan<byte>(offset, 4));
                    offset += 4;
                    List<byte> eventVarTypes = new();
                    for (int k = 0; k < eventVarNum; k++)
                    {
                        byte varType = binaryData[offset];
                        eventVarTypes.Add(varType);
                        offset++;
                    }
                    CEvent ev = new(eventName, eventStartInstruction, eventVarTypes);
                    if (!listSubroutineIndices.Contains(eventStartInstruction))
                    {
                        Subroutines.Add(new CSubroutine(eventStartInstruction));
                        listSubroutineIndices.Add(eventStartInstruction);
                    }
                    taskEvents.Add(ev);
                }
                CTask cTask = new(taskVarNum, varTypes, taskVarFromCode, taskEvents, i);
                SectionBTaskList.Add(cTask);
            }
            //Section C
            Int32 sectionCEventNum = System.BitConverter.ToInt32(binData.AsSpan<byte>(offset, 4));
            offset += 4;
            TaskCEvents = new();

            for (int j = 0; j < sectionCEventNum; j++)
            {
                Int32 eventName = System.BitConverter.ToInt32(binData.AsSpan<byte>(offset, 4));
                offset += 4;
                Int32 eventStartInstruction = System.BitConverter.ToInt32(binData.AsSpan<byte>(offset, 4));
                offset += 4;
                Int32 eventVarNum = System.BitConverter.ToInt32(binData.AsSpan<byte>(offset, 4));
                offset += 4;
                List<byte> eventVarTypes = new();
                for (int k = 0; k < eventVarNum; k++)
                {
                    byte varType = binaryData[offset];
                    eventVarTypes.Add(varType);
                    offset++;
                }
                CEvent ev = new(eventName, eventStartInstruction, eventVarTypes);
                TaskCEvents.Add(ev);
                if (!listSubroutineIndices.Contains(eventStartInstruction))
                {
                    Subroutines.Add(new CSubroutine(eventStartInstruction));
                    listSubroutineIndices.Add(eventStartInstruction);
                }
            }
            List<byte> nullListBytes = new();
            CTask taskC = new(0, nullListBytes, 0, TaskCEvents,-1);
            //Number of instructions: 
            numInstructions = System.BitConverter.ToUInt32(binData.AsSpan<byte>(offset, 4));
            numInstructionsOffset = (uint)offset;
            offset += 4;

            InstructionBlockOffset = offset;

            //Create instruction list
            for (int i = 0; (i < numInstructions) && (offset < binData.Count()); i++)
            {
                try {
                    byte opcode = binData[offset];
                    offset += 2;
                    listInstructions.Add(createInstruction(opcode, i, (UInt32)offset,ref this.offset,ref this.binaryData));
                } catch (IndexOutOfRangeException) {
                    MessageBox.Show("Failed to parse instruction #" + instructionsRead + " of " + numInstructions, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // DO NOT abort: it's useful to see which instructions succeeded loading, if any
                }
                instructionsRead++;
            }
            if (instructionsRead != numInstructions) {
                MessageBox.Show("Wrong instruction count: read " + instructionsRead + " instructions, but expected " + numInstructions, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            //Flow graph (not sure if still necessary in the current iteration)

            CreateFlowGraph();

            //Main assembly block

            int index = 0;
            int indexFinal = 0;

            Lines = new();

            foreach (IInstruction ins in this.listInstructions)
            {

                CLine lineOfASM = new CLine(ins);

                //Subrotine tags
                this.Subroutines.Sort((a, b) => a.locStart.CompareTo(b.locStart));

                foreach (CSubroutine sub in this.Subroutines)
                {
                    if (sub.locStart == index)
                    {
                        int indexCurrent = indexFinal;
                        indexFinal++;
                        CLine subStart = new(sub.locStart, " START\n", sub.locEnd);
                        Lines.Add(subStart);
                    }
                    else if (sub.locEnd == index)
                    {
                        indexFinal++;
                    }
                }
                //Labels
                this.Labels.Sort((a, b) => a.location.CompareTo(b.location));
                foreach (CLabel lab in this.Labels)
                {
                    if (lab.location == index)
                    {
                        CLine labelPos = new(index);
                        Lines.Add(labelPos);
                        int indexCurrent = indexFinal;
                        indexFinal++;
                    }
                }
                Lines.Add(lineOfASM);
                index++;
                indexFinal++;
            }

            //CreateStringRepresentation();
            //DataConstants.Sort((a, b) => a.dataType.CompareTo(b.dataType));
            DataConstants = DataConstants.OrderBy(x => x.dataType).ThenBy(x => x.LocationString).ToList<CData>();
            return true;
        }

        public string Decompile() 
        {
            string ret = "";
            /*foreach (IInstruction ins in listInstructions) 
            {
                
            }*/
            return ret;
        }
        public override string ToString()
        {
            string singleString = "";
            foreach (string s in stringRepresentation)
            {
                singleString += s;
            }
            return singleString;
        }
    }
}
