using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using TaleWorlds.InputSystem;
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

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            
            var canPlay = _instrumentsBehavior.CanPlay();
            var defendClick = HotKeyManager.GetCategory("CombatHotKeyCategory").GetGameKey("Defend");
                        
            if (MissionScreen.SceneLayer.Input.IsGameKeyReleased(defendClick.Id) && canPlay)
            {
                if(!IsPLaying)
                {
                    _instrumentsBehavior.RequestStartPlaying();
                    IsPLaying = true;
                }
                else
                {
                    _instrumentsBehavior.RequestStopEat();
                    IsPLaying = false;
                }
            }
            
            if(IsPLaying && !canPlay)
            {
                _instrumentsBehavior.RequestStopEat();
                IsPLaying = false;
            }
        }
    }
}