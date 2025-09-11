using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.NetworkMessages.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public struct Instrument
    {
        public ItemObject Item;
        public ActionIndexCache Animation;
        public int SoundIndex;

        public Instrument(string itemId, string animation, string musicId)
        {
            Item = MBObjectManager.Instance.GetObject<ItemObject>(itemId);
            Animation = ActionIndexCache.Create(animation);
            SoundIndex = SoundEvent.GetEventIdFromString(musicId);
        }
    }

    public class InstrumentsBehavior : MissionNetwork
    {
        public class PlayingAction
        {
            public Agent PlayerAgent;
            public Instrument Instrument;
            public long PlayingStartedAt;

            public PlayingAction(Agent player, Instrument instrument)
            {
                PlayerAgent = player;
                Instrument = instrument;
                PlayingStartedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }
        }

        public List<Instrument> Instruments = new List<Instrument>();
#if SERVER
        public Dictionary<Agent, PlayingAction> AgentsPlaying = new Dictionary<Agent, PlayingAction>();
#endif
#if CLIENT
        public Dictionary<Agent, SoundEvent> AgentsPlayingSound = new Dictionary<Agent, SoundEvent>();
#endif
        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();

            AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);

            foreach (ModuleInfo module in ModuleHelper.GetModules())
            {
                LoadInstruments(module.Id);
            }
        }

        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();

            AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Remove);
            Instruments.Clear();
#if CLIENT
            foreach (var agent in AgentsPlayingSound.Keys)
            {
                if (AgentsPlayingSound[agent].IsPlaying())
                {
                    AgentsPlayingSound[agent].Stop();
                }
            }
            AgentsPlayingSound.Clear();
#endif
        }

        private void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
        {
            GameNetwork.NetworkMessageHandlerRegisterer networkMessageHandlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(mode);
#if CLIENT
            networkMessageHandlerRegisterer.Register<AgentPlayingInstrument>(this.HandleAgentPlayingInstrumentFromServer);
#endif
#if SERVER
            networkMessageHandlerRegisterer.Register<RequestStartPlaying>(this.HandleRequestStartPlayingFromClient);
            networkMessageHandlerRegisterer.Register<RequestStopPlaying>(this.HandleRequestStopPlayingFromClient);
#endif
        }

        private void LoadInstruments(string moduleId)
        {
            var FoodPath = ModuleHelper.GetXmlPath(moduleId, "Instruments");

            if (File.Exists(FoodPath) == false) return;

            var xmlDocument = new XmlDocument();

            xmlDocument.Load(FoodPath);

            foreach (XmlNode node in xmlDocument.SelectNodes("/Instruments/Instrument"))
            {
                var ItemId = node["ItemId"].InnerText;
                var animation = node["Animation"].InnerText;
                var musicId = node["MusicId"].InnerText;
                var instrument = new Instrument(ItemId, animation, musicId);

                Instruments.Add(instrument);
            }
        }

#if SERVER        
        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
        {
            if (affectedAgent == null) return;

            if (AgentsPlaying.ContainsKey(affectedAgent))
            {
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new AgentPlayingInstrument(affectedAgent, 0, false));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);

                AgentsPlaying.Remove(affectedAgent);
            }
        }

        private bool HandleRequestStopPlayingFromClient(NetworkCommunicator peer, RequestStopPlaying message)
        {
            if (peer.ControlledAgent == null) return false;

            if (AgentsPlaying.ContainsKey(peer.ControlledAgent))
            {
                // peer.ControlledAgent.SetActionChannel(0, ActionIndexCache.act_none, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new AgentPlayingInstrument(peer.ControlledAgent, 0, false));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
                // peer.ControlledAgent.ClearTargetFrame();
                AgentsPlaying.Remove(peer.ControlledAgent);
            }
            return true;
        }

        private bool HandleRequestStartPlayingFromClient(NetworkCommunicator peer, RequestStartPlaying message)
        {
            if (peer.ControlledAgent == null) return false;

            PersistentEmpireRepresentative persistentEmpireRepresentative = peer.GetComponent<PersistentEmpireRepresentative>();

            if (persistentEmpireRepresentative == null) return false;

            EquipmentIndex index = peer.ControlledAgent.GetWieldedItemIndex(Agent.HandIndex.OffHand);

            if (index == EquipmentIndex.None) return false;

            MissionWeapon equipmentElement = peer.ControlledAgent.Equipment[index];

            var instrumentWithIndex = this.Instruments.Select((instr, instrIndex) => new { Instrument = instr, Index = instrIndex }).FirstOrDefault(f => f.Instrument.Item.Id == equipmentElement.Item.Id);

            if (instrumentWithIndex.Instrument.Item == null) return false;

            PlayingAction playingAction = new PlayingAction(peer.ControlledAgent, instrumentWithIndex.Instrument);
            AgentsPlaying[peer.ControlledAgent] = playingAction;
            // peer.ControlledAgent.SetActionChannel(0, instrumentWithIndex.Instrument.Animation, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);

            GameNetwork.BeginBroadcastModuleEvent();
            GameNetwork.WriteMessage(new AgentPlayingInstrument(peer.ControlledAgent, instrumentWithIndex.Index, true));
            GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
            // peer.ControlledAgent.SetTargetPosition(peer.ControlledAgent.Position.AsVec2);

            return true;
        }
#endif
#if CLIENT
        private static int _counter = 0;
        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);

            if (++_counter < 10)
                return;
            // Reset counter
            _counter = 0;

            foreach (Agent key in AgentsPlayingSound.Keys.ToList())
            {
                if (key != null && key.IsActive())
                {
                    AgentsPlayingSound[key].SetPosition(key.Position);
                }
            }
        }

        public bool CanPlay()
        {
            Agent myAgent = GameNetwork.MyPeer.ControlledAgent;
            if (myAgent == null) return false;

            EquipmentIndex wieldedIndex = myAgent.GetWieldedItemIndex(Agent.HandIndex.OffHand);
            if (wieldedIndex == EquipmentIndex.None) return false;

            MissionWeapon equipment = myAgent.Equipment[wieldedIndex];
            if (equipment.IsEmpty) return false;

            Instrument instrument = this.Instruments.FirstOrDefault(f => f.Item != null && f.Item.StringId == equipment.Item.StringId);
            if (instrument.Item == null) return false;

            if (myAgent.HasMount) return false;

            return true;
        }

        public void RequestStartPlaying()
        {
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestStartPlaying());
            GameNetwork.EndModuleEventAsClient();
        }
        public void RequestStopPlaying()
        {
            var myAgent = GameNetwork.MyPeer.ControlledAgent;

            if (myAgent == null) return;

            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestStopPlaying());
            GameNetwork.EndModuleEventAsClient();
        }

        private void HandleAgentPlayingInstrumentFromServer(AgentPlayingInstrument message)
        {
            if (message.PlayerAgent == null || message.PlayerAgent.IsActive() == false) return;

            if (message.IsPlaying)
            {
                StopAgentPlaying(message.PlayerAgent);
                if (Instruments.Count > message.PlayingInstrumentIndex)
                {
                    PlayAgentSound(message.PlayerAgent, this.Instruments[message.PlayingInstrumentIndex]);
                }
            }
            else
            {
                StopAgentPlaying(message.PlayerAgent);
            }
        }

        private void StopAgentPlaying(Agent agent)
        {
            if (AgentsPlayingSound.ContainsKey(agent) == false) return;

            if (AgentsPlayingSound[agent].IsValid && this.AgentsPlayingSound[agent].IsPlaying())
            {
                AgentsPlayingSound[agent].Stop();
                AnimationSystemData animationSystemData = agent.Monster.FillAnimationSystemData(MBGlobals.GetActionSet("as_human_warrior"), agent.Character.GetStepSize(), false);
                agent.SetActionSet(ref animationSystemData);
                agent.SetActionChannel(0, ActionIndexCache.act_none, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
            }
            
            AgentsPlayingSound.Remove(agent);
        }

        private void PlayAgentSound(Agent agent, Instrument instrument)
        {
            var eventRef = SoundEvent.CreateEvent(instrument.SoundIndex, base.Mission.Scene);//get a reference to sound and update parameters later.
            
            eventRef.SetPosition(agent.Position);
            eventRef.Play();
            AgentsPlayingSound[agent] = eventRef;
            
            var animationSystemData = agent.Monster.FillAnimationSystemData(MBGlobals.GetActionSet("as_human_musician"), agent.Character.GetStepSize(), false);
            
            agent.SetActionSet(ref animationSystemData);
            agent.SetActionChannel(0, instrument.Animation, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
        }
#endif
    }
}