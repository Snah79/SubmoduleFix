using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.SceneScripts
{
    public class PE_StandingPointWithWeaponRequirement : StandingPointWithWeaponRequirement
    {
        public override bool IsDisabledForAgent(Agent agent)
        {
            EquipmentIndex wieldedItemIndex = agent.GetPrimaryWieldedItemIndex();
            if (wieldedItemIndex == EquipmentIndex.None) return false;

            if (agent.Equipment[wieldedItemIndex].Item.PrimaryWeapon.WeaponClass == WeaponClass.Boulder)
            {
                return false;
            }
            return true;
        }
    }
}
