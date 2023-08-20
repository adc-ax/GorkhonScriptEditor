using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GorkhonScriptEditor
{
    public partial class CEvent : ObservableObject
    {

        public Int32 Name;
        public String InGameName = "";
        public Int32 StartInstruction;
        public Int32 VarNum;
        public List<byte> varTypes;

        public CEvent(Int32 name, Int32 start, List<byte> vartypes)
        {
            Name = name;
            StartInstruction = start;
            varTypes = vartypes;
            VarNum = vartypes.Count();
            switch (name) 
            {
                /*case 0x00: InGameName = "PlayerUse"; break;
                case 0x01: InGameName = "See"; break;
                case 0x02: InGameName = "StopSee"; break;
                case 0x03: InGameName = "Hear"; break;
                case 0x04: InGameName = "StopHear"; break;
                case 0x05: InGameName = "ActorLoad"; break;
                case 0x06: InGameName = "ActorUnload"; break;
                case 0x07: InGameName = "Timer"; break;
                case 0x08: InGameName = "RegionChange"; break;
                case 0x09: InGameName = "GameTime"; break;
                case 0x0A: InGameName = "Collision"; break;
                case 0x0B: InGameName = "DialogReply"; break;
                case 0x0C: InGameName = "MusicChange"; break;
                case 0x0D: InGameName = "PlayerStartShooting"; break;
                case 0x0E: InGameName = "PlayerStopShooting"; break;
                case 0x0F: InGameName = "PlayerHolsterWeapon"; break;
                case 0x10: InGameName = "PropChanged"; break;
                case 0x11: InGameName = "Attacked"; break;
                case 0x12: InGameName = "Action"; break;
                case 0x13: InGameName = "PlayerLand"; break;
                case 0x14: InGameName = "PlayerStartWalking"; break;
                case 0x15: InGameName = "PlayerStopWalking"; break;
                case 0x16: InGameName = "Hit"; break;
                case 0x17: InGameName = "Unknown, please file a bug"; break;
                case 0x18: InGameName = "KeyDown"; break;
                case 0x19: InGameName = "KeyUp"; break;
                case 0x1A: InGameName = "Unknown, please file a bug"; break;
                case 0x1B: InGameName = "Intersection"; break;
                case 0x1C: InGameName = "ActorStuck"; break;
                case 0x1D: InGameName = "Unknown, please file a bug"; break;
                case 0x1E: InGameName = "PlayerDamage"; break;
                case 0x1F: InGameName = "ReputationChange"; break;
                case 0x20: InGameName = "Dispose"; break;
                case 0x21: InGameName = "InventoryAddItem"; break;
                case 0x22: InGameName = "InventoryRemoveItem"; break;
                case 0x23: InGameName = "InventorySelChange"; break;
                case 0x24: InGameName = "PlayerStartAltShooting"; break;
                case 0x25: InGameName = "PlayerStopAltShooting"; break;
                case 0x26: InGameName = "ClearPath"; break;
                case 0x27: InGameName = "FallDamage"; break;
                case 0x28: InGameName = "Steal"; break;
                case 0x29: InGameName = "Death"; break;
                case 0x2A: InGameName = "Message"; break;
                case 0x2B: InGameName = "Unknown, please file a bug"; break;
                case 0x2C: InGameName = "PlayerStartSneaking"; break;
                case 0x2D: InGameName = "Unknown, please file a bug"; break;
                case 0x2E: InGameName = "WndMessage"; break;
                case 0x2F: InGameName = "Unknown, please file a bug"; break;
                default: InGameName = "Unknown, bug adc_ax"; break;*/
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
                default:   InGameName = "0x"+name.ToString("X5")+": Unknown, bug adc_ax"; break;
            }

        }

        public override string ToString()
        {
            string ret = "";

            ret+="Event " + /*Name.ToString("X5")*/ InGameName + ": " + "CALL 0x" + StartInstruction.ToString("X5") + ", " + VarNum.ToString() + " vars: ";
            foreach (var type in varTypes)
            {
                string vartype = "";
                switch (type)
                {
                    case 0: vartype = "var"; break;
                    case 1: vartype = "bool"; break;
                    case 2: vartype = "int"; break;
                    case 3: vartype = "float"; break;
                    case 4: vartype = "string"; break;
                    case 5: vartype = "pointer"; break;
                    case 6: vartype = "coordinates"; break;
                    default: break;
                }
                ret += vartype + "; ";
            }
            return ret;  
        }

    }
}
