using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace PersistentEmpires.Views.Views
{
    public class PEPlayInstrumentView : MissionView
    {
        public bool RequestedStartPlaying = false;
        private InstrumentsBehavior _instrumentsBehavior;

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            this._instrumentsBehavior = base.Mission.GetMissionBehavior<InstrumentsBehavior>();

        }

        private bool _firstTime = true;
        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            GameKey defendClick = HotKeyManager.GetCategory("CombatHotKeyCategory").GetGameKey("Defend");
            if (base.MissionScreen.SceneLayer.Input.IsGameKeyPressed(defendClick.Id) && !RequestedStartPlaying)
            {
                this.RequestedStartPlaying = this._instrumentsBehavior.RequestStartPlaying();
                _firstTime = true;
            }
            else if (base.MissionScreen.SceneLayer.Input.IsGameKeyReleased(defendClick.Id) && this.RequestedStartPlaying)
            {
                if(_firstTime)
                {
                    _firstTime = false;
                    return;
                }
                this._instrumentsBehavior.RequestStopEat();
                this.RequestedStartPlaying = false;
            }
        }
    }
}