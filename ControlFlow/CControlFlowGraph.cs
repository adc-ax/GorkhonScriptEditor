using GorkhonScriptEditor.Instructions;
using System;
using System.Collections.Generic;
using System.Linq;


namespace GorkhonScriptEditor
{
    public class CControlFlowGraph
    {
        //This entire class is cursed and hasn't been reviewed in months, I am not sure any of this is actually still meaningfully useful
        public List<(IInstruction, int)> leaders;

        public List<CBasicBlock> basicBlocks;
        public CControlFlowGraph() { leaders = new(); basicBlocks = new(); }

        public List<IInstruction> nodes;

        public List<CEdge> edges;

        public List<CBasicBlock> subStarts = new();
        public List<int> subEnds = new();

        public CControlFlowGraph(List<IInstruction> listInstructions)
        {
            int ID = 0;
            leaders = new();
            basicBlocks = new();
            edges = new();
            nodes = new();
            IInstruction prev = null;
            //Find leaders
            foreach (IInstruction instruction in listInstructions)
            {
                if (ID == 0)
                    leaders.Add((instruction, ID));
                else
                {
                    if ((prev is CInstructionJump) || (prev is CInstructionJumpB) || (prev is CInstructionCall))
                        leaders.Add((instruction, ID));
                    if ((instruction is CInstructionJump) || (instruction is CInstructionCall))
                        leaders.Add((listInstructions[Convert.ToInt32(instruction.Args[0])], Convert.ToInt32(instruction.Args[0])));
                    if (instruction is CInstructionJumpB)
                        leaders.Add((listInstructions[Convert.ToInt32(instruction.Args[1])], Convert.ToInt32(instruction.Args[1])));

                }
                ID++;
                prev = instruction;
            }

            leaders.Sort((a, b) => a.Item2.CompareTo(b.Item2));

            leaders = leaders.Distinct().ToList();

            //Create basic blocks

            for (int i = 0; i < leaders.Count; i++)
            {
                int indexStart, indexEnd;
                indexStart = leaders[i].Item2;
                if (i + 1 == leaders.Count)
                {
                    indexEnd = listInstructions.Count;
                }
                else
                    indexEnd = leaders[i + 1].Item2;
                CBasicBlock newBlock = new(indexStart, indexEnd - 1);
                for (int j = indexStart; j < indexEnd; j++)
                {
                    newBlock.instructionList.Add(listInstructions[j]);
                }
                basicBlocks.Add(newBlock);
            }

            CBasicBlock entry = new(-1, -1);

            CEdge firstEdge = new(entry, basicBlocks[0]);

            edges.Add(firstEdge);

            for (int i = 0; i < basicBlocks.Count; i++)
            {
                for (int j = 0; j < basicBlocks.Count; j++)
                {
                    if ((basicBlocks[i].instructionList.Last() is CInstructionJump) || (basicBlocks[i].instructionList.Last() is CInstructionCall))
                    {
                        if ((Convert.ToInt32(basicBlocks[i].instructionList.Last().Args[0])) == basicBlocks[j].start)
                        {
                            CEdge newEdge = new(basicBlocks[i], basicBlocks[j]);
                            if (basicBlocks[i].instructionList.Last() is CInstructionCall)
                            {
                                newEdge.isCall = true;
                            }
                            edges.Add(newEdge);
                        }
                    }
                    else
                    if (basicBlocks[i].instructionList.Last() is CInstructionJumpB)
                    {
                        if ((Convert.ToInt32(basicBlocks[i].instructionList.Last().Args[1])) == basicBlocks[j].start)
                        {
                            CEdge newEdge = new(basicBlocks[i], basicBlocks[j]);
                            edges.Add(newEdge);
                        }
                    }
                    else
                    if (basicBlocks[i].end + 1 == basicBlocks[j].start)
                    {
                        CEdge newEdge = new(basicBlocks[i], basicBlocks[j]);
                        edges.Add(newEdge);
                    }

                }
            }

            CBasicBlock currentNode = entry;

            subStarts = new();
            subEnds = new();

            foreach (CEdge edge in edges)
            {
                if (edge.isCall == true)
                {
                    subStarts.Add(edge.B);
                }

            }

            subStarts.Sort((a, b) => a.start.CompareTo(b.start));
            subStarts = subStarts.Distinct().ToList();

            for (int i = 0; i < subStarts.Count; i++)
            {
                int start = subStarts[i].start;
                int end;
                if (i < subStarts.Count - 1)
                {
                    end = subStarts[i + 1].start;
                }
                else
                    end = listInstructions.Count;

                int funcEnd = start;
                for (int j = start; j < end; j++)
                {
                    if (listInstructions[j] is CInstructionReturn)
                    {
                        funcEnd = j + 1;
                    }
                }
                subEnds.Add(funcEnd);
            }
        }
    }
}
