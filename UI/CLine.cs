using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging.Messages;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace GorkhonScriptEditor
{
    public partial class CLine : ObservableValidator
    {
        public IInstruction instructionRef;

        public String LineNumber { get; set; }
        public String OpCode { get; set; }

        public const string pattern1 = "[0-9a-fA-F ]+";

        //private string operands;

        IReadOnlyCollection<ValidationResult> errors;

        /*public string Operands 
        {
            get => this.operands;
            set
            {
                TrySetProperty(ref this.operands, value,out errors);
            }
        }*/

        
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [RegularExpression(pattern1)]
        public string operands;
        public String InstructionName { get; set; }
        public String OperandsText { get; set; }

        partial void OnOperandsChanged(string value)
        {
            UpdateInstructionFromEditor();
        }

        public System.Windows.Media.Brush Foreground { get; set; }

        public System.Windows.Media.Brush OpCodeBrush { get; set; }

        public String InstructionComment { get; set; }

        public CLine(IInstruction instruction)
        {
            instructionRef = instruction;
            LineNumber = instructionRef.ID.ToString("X5") + "[" + instructionRef.ByteOffset.ToString("X6") + "]";

            OpCode = "        " + instructionRef.OPCode.ToString("X2") + "00 | ";
            string operandString = BitConverter.ToString(instructionRef.binaryRepresentation.ToArray()).Replace('-', ' ');
            Operands = operandString;

            InstructionComment = instructionRef.DisplayString;


            InstructionName = InstructionComment.Split(" ")[0];
            InstructionComment = InstructionComment.Substring(InstructionComment.Split(" ")[0].Length + 1);

            Foreground = Brushes.OliveDrab;
            OpCodeBrush = Brushes.Purple;

            if (instruction.OPCode == 0x4F)
            {
                OpCodeBrush = Brushes.Green;
            }
        }

        private void UpdateStrings()
        {

        }

        public CLine(Int32 start, string extraText = "", Int32 end = 0)
        {

            string subName = start.ToString("X4");
            InstructionComment = "\n                                  SUB 0x" + subName + extraText;
            Foreground = Brushes.Orange;
        }

        public CLine(Int32 loc)
        {
            InstructionComment = "LABEL 0x" + loc.ToString("X2");
            Foreground = Brushes.Green;
        }

        public void UpdateInstructionFromEditor()
        {
            List<byte> updatedBytes = new();
            string trimmedops = Operands.Replace(" ", string.Empty);

            for (int i = 0; i < trimmedops.Length - 1; i += 2)
            {
                string temporary = trimmedops[i].ToString() + trimmedops[i + 1].ToString();
                int temporaryInt = Convert.ToInt32(temporary, 16);
                updatedBytes.Add((byte)temporaryInt);

            }
            instructionRef.binaryRepresentation = updatedBytes;
        }

    }
}
