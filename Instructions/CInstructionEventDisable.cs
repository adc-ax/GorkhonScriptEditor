using System;
using System.Collections.Generic;

namespace GorkhonScriptEditor.Instructions
{
    class CInstructionEventDisable : CommunityToolkit.Mvvm.ComponentModel.ObservableObject, IInstruction
    {
        public CInstructionEventDisable(List<Object> args, List<byte> bin)
        {
            OPCode = 0x57;
            Args = args;
            string InGameName = "";
            switch (Args[0])
            {
                case 0x00: InGameName = "PlayerUse"; break; //FIXED
                case 0x01: InGameName = "See"; break; //FIXED
                case 0x02: InGameName = "StopSee"; break; //FIXED
                case 0x03: InGameName = "Hear"; break; //FIXED
                case 0x04: InGameName = "StopHear"; break; //FIXED
                case 0x05: InGameName = "ActorLoad"; break; //FIXED
                case 0x06: InGameName = "ActorUnload"; break; //FIXED
                case 0x07: InGameName = "Timer"; break; //FIXED
                case 0x08: InGameName = "RegionChange"; break; //FIXED
                case 0x09: InGameName = "GameTime"; break; //FIXED
                case 0x0A: InGameName = "Collision"; break; //FIXED
                case 0x0B: InGameName = "DialogReply"; break; //FIXED
                case 0x0C: InGameName = "MusicChange"; break; //FIXED
                case 0x0D: InGameName = "PlayerStartShooting"; break; //FIXED
                case 0x0E: InGameName = "PlayerStopShooting"; break; //FIXED
                case 0x0F: InGameName = "PlayerHolsterWeapon"; break; //FIXED
                case 0x10: InGameName = "PropChanged"; break; //FIXED
                case 0x11: InGameName = "Attacked"; break; //FIXED
                case 0x12: InGameName = "Action"; break; //FIXED 
                case 0x13: InGameName = "PlayerLand"; break; //FIXED
                case 0x14: InGameName = "Attacked"; break; //FIXED
                case 0x15: InGameName = "0x15 - maybe Action"; break; //updated
                case 0x16: InGameName = "Hit"; break; //FIXED
                case 0x17: InGameName = "Intersection"; break; //FIXED
                case 0x18: InGameName = "KeyDown"; break; //FIXED
                case 0x19: InGameName = "KeyUp"; break; //FIXED
                case 0x1A: InGameName = "Trigger"; break; //FIXED
                case 0x1B: InGameName = "Intersection"; break; //FIXED
                case 0x1C: InGameName = "ActorStuck"; break; //FIXED
                case 0x1D: InGameName = "ActorUnstuck??"; break; //FIXED
                case 0x1E: InGameName = "PlayerDamage"; break; //FIXED
                case 0x1F: InGameName = "ReputationChange"; break; //FIXED
                case 0x20: InGameName = "Dispose"; break;//FIXED
                case 0x21: InGameName = "InventoryAddItem"; break; //FIXED
                case 0x22: InGameName = "InventoryRemoveItem"; break; //FIXED
                case 0x23: InGameName = "InventorySelChange"; break; //FIXED
                case 0x24: InGameName = "PlayerStartAltShooting"; break; //FIXED
                case 0x25: InGameName = "PlayerStopAltShooting"; break; //FIXED
                case 0x26: InGameName = "ClearPath"; break; //FIXED
                case 0x27: InGameName = "FallDamage"; break; //FIXED
                case 0x28: InGameName = "Steal"; break; //FIXED
                case 0x29: InGameName = "Death"; break; //FIXED
                case 0x2A: InGameName = "Message"; break; //FIXED
                case 0x2B: InGameName = "Hit2"; break; //FIXED
                case 0x2C: InGameName = "PlayerStartSneaking"; break; //FIXED
                case 0x2D: InGameName = "PlayerStopSneaking"; break; //FIXED
                case 0x2E: InGameName = "0x2E - maybe WndMessage"; break; //up
                case 0x2F: InGameName = "0x2F - maybe PlayerEnemy"; break; //up
                case 0x30: InGameName = "0x30 - maybe LSHAnimationEnd"; break; //up
                default: InGameName = "0x" + Convert.ToInt32(Args[0]).ToString("X5") + ": unknown, bug adc_ax"; break;
            }
            DisplayString = ("EventDisable " + InGameName);
            binaryRepresentation = bin;
        }

        public override string ToString()
        {
            return DisplayString;
        }

        public string DisplayString { get; set; }
        public UInt16 OPCode { get; set; }
        public List<Object> Args { get; set; }

        public List<byte> binaryRepresentation { get; set; }
        uint IInstruction.ByteOffset { get; set; }
        uint IInstruction.ID { get; set; }

        bool IInstruction.ValidateOperands(byte[] operands, bool updateBinary)
        {
            throw new NotImplementedException();
        }

        public void UpdateFromBinary(byte[] operands)
        {
            throw new NotImplementedException();
        }
    }
}
