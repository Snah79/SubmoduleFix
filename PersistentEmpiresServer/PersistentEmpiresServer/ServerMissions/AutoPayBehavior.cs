#if SERVER
using System;
using TaleWorlds.MountAndBlade;
using System.Timers;
using System.Linq;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresLib;
using TaleWorlds.Library;

namespace PersistentEmpiresServer.ServerMissions
{
    public class AutoPayBehavior : MissionNetwork
    {
        private static bool AutoPayEnabled = false;
        private static int AutoPayTimeInSeconds = 1800;
        private static int AutoPayGold = 100;
        private static System.Timers.Timer AutoPayTimer = null;
        private float _timer = 0f;

        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();

            AutoPayEnabled = ConfigManager.GetBoolConfig("AutoPayEnabled", false);
            AutoPayTimeInSeconds = ConfigManager.GetIntConfig("AutoPayTimeMinutes", 30) * 60;
            AutoPayGold = ConfigManager.GetIntConfig("AutoPayGold", 100);
        }

        public override void OnMissionTick(float dt)
        {
            if (AutoPayEnabled)
            {
                _timer += dt;
                if (_timer >= AutoPayTimeInSeconds)
                {
                    _timer = 0f;
                    // Safe logic here, runs every 5 seconds
                }
            }
        }

        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            var activePlayers = GameNetwork.NetworkPeers.ToList().Where(x => x.ControlledAgent?.IsPlayerControlled == true);

            foreach (NetworkCommunicator peer in activePlayers)
            {
                SendSendGoldToPeer(peer);
            }
        }

        private static void SendSendGoldToPeer(NetworkCommunicator networkPeer)
        {
            var representative = networkPeer.GetComponent<PersistentEmpireRepresentative>();
            var message = $"Autopay message: Amount of {AutoPayGold} have been added to your purse. Next payment in {AutoPayTimeInSeconds} minutes.";
            
            representative.GoldGain(AutoPayGold);
            InformationComponent.Instance.SendMessage(message, Colors.Yellow.ToUnsignedInteger(), networkPeer);
        }
    }
}
#endif