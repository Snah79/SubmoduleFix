using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System.Linq;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace PersistentEmpires.Views.Views
{
    public class PEPlayInstrumentView : MissionView
    {
        public bool IsPLaying = false;
        private InstrumentsBehavior _instrumentsBehavior;

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            this._instrumentsBehavior = base.Mission.GetMissionBehavior<InstrumentsBehavior>();

        }
#if CLIENT
        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            
            var canPlay = _instrumentsBehavior.CanPlay();
            var defendClick = HotKeyManager.GetCategory("CombatHotKeyCategory").GetGameKey(CombatHotKeyCategory.Defend);
                        
            if (MissionScreen.SceneLayer.Input.IsGameKeyReleased(defendClick.Id) && canPlay)
            {
                if(!IsPLaying)
                {
                    _instrumentsBehavior.RequestStartPlaying();
                    IsPLaying = true;
                }
                else
                {
                    _instrumentsBehavior.RequestStopPlaying();
                    IsPLaying = false;
                }
            }
            else if (IsPLaying && GameNetwork.MyPeer?.ControlledAgent != null &&
                    ((_instrumentsBehavior.AgentsPlayingSound.ContainsKey(GameNetwork.MyPeer.ControlledAgent)
                    && GameNetwork.MyPeer.ControlledAgent.GetCurrentAction(0).GetName() == "act_none")
                    || !canPlay))
            {
                _instrumentsBehavior.RequestStopPlaying();
                IsPLaying = false;
            }
        }
#endif
    }
}