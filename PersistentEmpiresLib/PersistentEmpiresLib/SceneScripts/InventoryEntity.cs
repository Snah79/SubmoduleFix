using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresLib.SceneScripts.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.SceneScripts
{
    public class PE_InventoryEntity : PE_UsableSynchedObject, IRemoveable, ISpawnable
    {
        public static Random random;
        public string InventoryName = "Chest";
        public string InventoryId = "";
        public float Distance = 1f;
        public int CastleId = -1;
        public int Slot = 30; // Max value can be 256
        public int StackCount = 10; // Max value can be 256
        private int _usedChannelIndex;
        private ActionIndexCache _progressActionIndex;
        private ActionIndexCache _successActionIndex;
        private PlayerInventoryComponent playerInventoryComponent;

        private static readonly ActionIndexCache act_pickup_down_begin = ActionIndexCache.Create("act_pickup_down_begin");
        private static readonly ActionIndexCache act_pickup_down_end = ActionIndexCache.Create("act_pickup_down_end");
        private static readonly ActionIndexCache act_pickup_down_begin_left_stance = ActionIndexCache.Create("act_pickup_down_begin_left_stance");
        private static readonly ActionIndexCache act_pickup_down_end_left_stance = ActionIndexCache.Create("act_pickup_down_end_left_stance");
        private static readonly ActionIndexCache act_pickup_down_left_begin = ActionIndexCache.Create("act_pickup_down_left_begin");
        private static readonly ActionIndexCache act_pickup_down_left_end = ActionIndexCache.Create("act_pickup_down_left_end");
        private static readonly ActionIndexCache act_pickup_down_left_begin_left_stance = ActionIndexCache.Create("act_pickup_down_left_begin_left_stance");
        private static readonly ActionIndexCache act_pickup_down_left_end_left_stance = ActionIndexCache.Create("act_pickup_down_left_end_left_stance");
        private static readonly ActionIndexCache act_pickup_middle_begin = ActionIndexCache.Create("act_pickup_middle_begin");
        private static readonly ActionIndexCache act_pickup_middle_end = ActionIndexCache.Create("act_pickup_middle_end");
        private static readonly ActionIndexCache act_pickup_middle_begin_left_stance = ActionIndexCache.Create("act_pickup_middle_begin_left_stance");
        private static readonly ActionIndexCache act_pickup_middle_end_left_stance = ActionIndexCache.Create("act_pickup_middle_end_left_stance");
        private static readonly ActionIndexCache act_pickup_middle_left_begin = ActionIndexCache.Create("act_pickup_middle_left_begin");
        private static readonly ActionIndexCache act_pickup_middle_left_end = ActionIndexCache.Create("act_pickup_middle_left_end");
        private static readonly ActionIndexCache act_pickup_middle_left_begin_left_stance = ActionIndexCache.Create("act_pickup_middle_left_begin_left_stance");
        private static readonly ActionIndexCache act_pickup_middle_left_end_left_stance = ActionIndexCache.Create("act_pickup_middle_left_end_left_stance");
        private static readonly ActionIndexCache act_pickup_up_begin = ActionIndexCache.Create("act_pickup_up_begin");
        private static readonly ActionIndexCache act_pickup_up_end = ActionIndexCache.Create("act_pickup_up_end");
        private static readonly ActionIndexCache act_pickup_up_begin_left_stance = ActionIndexCache.Create("act_pickup_up_begin_left_stance");
        private static readonly ActionIndexCache act_pickup_up_end_left_stance = ActionIndexCache.Create("act_pickup_up_end_left_stance");
        private static readonly ActionIndexCache act_pickup_up_left_begin = ActionIndexCache.Create("act_pickup_up_left_begin");
        private static readonly ActionIndexCache act_pickup_up_left_end = ActionIndexCache.Create("act_pickup_up_left_end");
        private static readonly ActionIndexCache act_pickup_up_left_begin_left_stance = ActionIndexCache.Create("act_pickup_up_left_begin_left_stance");
        private static readonly ActionIndexCache act_pickup_up_left_end_left_stance = ActionIndexCache.Create("act_pickup_up_left_end_left_stance");
        private static readonly ActionIndexCache act_pickup_from_right_down_horseback_begin = ActionIndexCache.Create("act_pickup_from_right_down_horseback_begin");
        private static readonly ActionIndexCache act_pickup_from_right_down_horseback_end = ActionIndexCache.Create("act_pickup_from_right_down_horseback_end");
        private static readonly ActionIndexCache act_pickup_from_right_down_horseback_left_begin = ActionIndexCache.Create("act_pickup_from_right_down_horseback_left_begin");
        private static readonly ActionIndexCache act_pickup_from_right_down_horseback_left_end = ActionIndexCache.Create("act_pickup_from_right_down_horseback_left_end");
        private static readonly ActionIndexCache act_pickup_from_right_middle_horseback_begin = ActionIndexCache.Create("act_pickup_from_right_middle_horseback_begin");
        private static readonly ActionIndexCache act_pickup_from_right_middle_horseback_end = ActionIndexCache.Create("act_pickup_from_right_middle_horseback_end");
        private static readonly ActionIndexCache act_pickup_from_right_middle_horseback_left_begin = ActionIndexCache.Create("act_pickup_from_right_middle_horseback_left_begin");
        private static readonly ActionIndexCache act_pickup_from_right_middle_horseback_left_end = ActionIndexCache.Create("act_pickup_from_right_middle_horseback_left_end");
        private static readonly ActionIndexCache act_pickup_from_right_up_horseback_begin = ActionIndexCache.Create("act_pickup_from_right_up_horseback_begin");
        private static readonly ActionIndexCache act_pickup_from_right_up_horseback_end = ActionIndexCache.Create("act_pickup_from_right_up_horseback_end");
        private static readonly ActionIndexCache act_pickup_from_right_up_horseback_left_begin = ActionIndexCache.Create("act_pickup_from_right_up_horseback_left_begin");
        private static readonly ActionIndexCache act_pickup_from_right_up_horseback_left_end = ActionIndexCache.Create("act_pickup_from_right_up_horseback_left_end");
        private static readonly ActionIndexCache act_pickup_from_left_down_horseback_begin = ActionIndexCache.Create("act_pickup_from_left_down_horseback_begin");
        private static readonly ActionIndexCache act_pickup_from_left_down_horseback_end = ActionIndexCache.Create("act_pickup_from_left_down_horseback_end");
        private static readonly ActionIndexCache act_pickup_from_left_down_horseback_left_begin = ActionIndexCache.Create("act_pickup_from_left_down_horseback_left_begin");
        private static readonly ActionIndexCache act_pickup_from_left_down_horseback_left_end = ActionIndexCache.Create("act_pickup_from_left_down_horseback_left_end");
        private static readonly ActionIndexCache act_pickup_from_left_middle_horseback_begin = ActionIndexCache.Create("act_pickup_from_left_middle_horseback_begin");
        private static readonly ActionIndexCache act_pickup_from_left_middle_horseback_end = ActionIndexCache.Create("act_pickup_from_left_middle_horseback_end");
        private static readonly ActionIndexCache act_pickup_from_left_middle_horseback_left_begin = ActionIndexCache.Create("act_pickup_from_left_middle_horseback_left_begin");
        private static readonly ActionIndexCache act_pickup_from_left_middle_horseback_left_end = ActionIndexCache.Create("act_pickup_from_left_middle_horseback_left_end");
        private static readonly ActionIndexCache act_pickup_from_left_up_horseback_begin = ActionIndexCache.Create("act_pickup_from_left_up_horseback_begin");
        private static readonly ActionIndexCache act_pickup_from_left_up_horseback_end = ActionIndexCache.Create("act_pickup_from_left_up_horseback_end");
        private static readonly ActionIndexCache act_pickup_from_left_up_horseback_left_begin = ActionIndexCache.Create("act_pickup_from_left_up_horseback_left_begin");
        private static readonly ActionIndexCache act_pickup_from_left_up_horseback_left_end = ActionIndexCache.Create("act_pickup_from_left_up_horseback_left_end");
        public override bool LockUserFrames { get => false; }
        public override bool LockUserPositions { get => false; }
        private WeakGameEntity _weakGameEntity;

        private string GenerateId()
        {
            if (PE_InventoryEntity.random == null)
            {
                PE_InventoryEntity.random = new Random();
            }
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = PE_InventoryEntity.random;
            return new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());

        }

        public override ScriptComponentBehavior.TickRequirement GetTickRequirement()
        {
            //if (GameNetwork.IsServer && base.HasUser)
            //{
            //    return base.GetTickRequirement() | ScriptComponentBehavior.TickRequirement.Tick | ScriptComponentBehavior.TickRequirement.TickParallel2;
            //}
#if SERVER
            if (base.HasUser)
            {
                return base.GetTickRequirement() | ScriptComponentBehavior.TickRequirement.Tick;
            }
#endif
            return base.GetTickRequirement();
        }

        protected override void OnTick(float dt)
        {
            base.OnTick(dt);
            this.DoTick(dt);
        }

        protected void DoTick(float dt)
        {
#if SERVER
            if (GameNetwork.IsServer)
            {
                if (base.HasUser)
                {
                    ActionIndexCache currentAction = base.UserAgent.GetCurrentAction(this._usedChannelIndex);
                    if (currentAction == this._successActionIndex)
                    {
                        base.UserAgent.StopUsingGameObjectMT(true);
                        GetTickRequirement();
                    }
                    else if (currentAction != this._progressActionIndex)
                    {
                        base.UserAgent.StopUsingGameObjectMT(false);
                        GetTickRequirement();
                    }
                }
            }
#endif
        }

        protected override void OnEditorInit()
        {
            if (this.InventoryId == "")
            {
                this.InventoryId = this.GenerateId();
            }
        }
        protected override void OnInit()
        {
            base.OnInit();

            _weakGameEntity = GameEntity;

            ActionMessage = new TextObject(InventoryName);
            TextObject descriptionMessage = new TextObject("Press {KEY} To Open The Inventory");
            descriptionMessage.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
            DescriptionMessage = descriptionMessage;
            playerInventoryComponent = Mission.Current.GetMissionBehavior<PlayerInventoryComponent>();
        }

        protected override void OnRemoved(int removeReason)
        {
            base.OnRemoved(removeReason);
            this.OnEntityRemove();
        }

        protected bool ValidateValues()
        {
            List<GameEntity> reference = new List<GameEntity>();
            Scene.GetAllEntitiesWithScriptComponent<PE_InventoryEntity>(ref reference);
            List<PE_InventoryEntity> sameId = reference.Select(r => r.GetFirstScriptOfType<PE_InventoryEntity>()).Where(r => r.InventoryId == this.InventoryId && r != this).ToList();
            if (sameId.Count() > 0)
            {
                MBEditor.AddEntityWarning(GameEntity, this.InventoryId + " has a same id with another chest");
                return false;
            }
            return true;
        }
        protected override void OnSceneSave(string saveFolder)
        {
            this.ValidateValues();
        }
        protected override bool OnCheckForProblems()
        {
            return this.ValidateValues();
        }

        public override TextObject GetDescriptionText(WeakGameEntity gameEntity)
        {
            return new TextObject("Use Inventory");
        }
        public PE_CastleBanner GetCastleBanner()
        {
            // FactionsBehavior factionBehavior = Mission.Current.GetMissionBehavior<FactionsBehavior>();
            CastlesBehavior castleBehaviors = Mission.Current.GetMissionBehavior<CastlesBehavior>();
            if (castleBehaviors.castles.ContainsKey(this.CastleId))
            {
                return castleBehaviors.castles[this.CastleId];
            }
            return null;
        }
#if SERVER
        public void OpenInventory(Agent userAgent)
        {
            if (_weakGameEntity.TryGetEntity(out var tmpGameEntity))
            {
                Mission.Current.MakeSound(SoundEvent.GetEventIdFromString("event:/mission/movement/foley/door_open"), tmpGameEntity.GetGlobalFrame().origin, false, true, -1, -1);
                this.playerInventoryComponent.OpenInventoryForPeer(userAgent.MissionPeer.GetNetworkPeer(), this.InventoryId);
                // userAgent.StopUsingGameObjectMT(true);            
            }
        }        

        public override void OnUse(Agent userAgent, sbyte agentBoneIndex)
        {
            if (_weakGameEntity.TryGetEntity(out var tmpGameEntity))
            {
                var distance = tmpGameEntity.GetGlobalFrame().origin.Distance(userAgent.Position);

                if (distance >= Distance)
                {
                    userAgent.StopUsingGameObjectMT(false);
                    return;
                }

                base.OnUse(userAgent, agentBoneIndex);
                
                Debug.Print("[USING LOG] AGENT USE " + this.GetType().Name + " ID " + this.InventoryId + " PLAYER " + userAgent.MissionPeer.DisplayedName);
                
                if (this.GameEntity == null || this.InteractionEntity == null) return;

                var player = userAgent.MissionPeer.GetNetworkPeer();
                var canUserUse = true;

                if (this.CastleId > -1)
                {
                    canUserUse = false;
                    var f = this.GetCastleBanner().GetOwnerFaction();
                    
                    if (f.chestManagers.Contains(player.VirtualPlayer.ToPlayerId()) || f.marshalls.Contains(player.VirtualPlayer.ToPlayerId()) || f.lordId == player.VirtualPlayer.ToPlayerId()) canUserUse = true;
                    
                    var destructComponent = tmpGameEntity.GetFirstScriptOfType<PE_RepairableDestructableComponent>();
                    
                    if (destructComponent != null && destructComponent.IsBroken) canUserUse = true;
                    
                    if (!canUserUse)
                    {
                        InformationComponent.Instance.SendMessage("This chest is locked by " + f.name, 0x0606c2d9, player);
                        return;
                    }
                }

                if (GameNetwork.IsServer)
                {
                    var globalFrame = tmpGameEntity.GetGlobalFrame();
                    var num = globalFrame.origin.z;
                    var eyeGlobalHeight = userAgent.GetEyeGlobalHeight();
                    var isLeftStance = userAgent.GetIsLeftStance();
                    
                    if (num < eyeGlobalHeight * 0.4f + userAgent.Position.z)
                    {
                        _usedChannelIndex = 0;
                        _progressActionIndex = (isLeftStance ? PE_InventoryEntity.act_pickup_down_begin_left_stance : PE_InventoryEntity.act_pickup_down_begin);
                        _successActionIndex = (isLeftStance ? PE_InventoryEntity.act_pickup_down_end_left_stance : PE_InventoryEntity.act_pickup_down_end);
                    }
                    else if (num < eyeGlobalHeight * 1.1f + userAgent.Position.z)
                    {
                        _usedChannelIndex = 1;
                        _progressActionIndex = (isLeftStance ? PE_InventoryEntity.act_pickup_middle_begin_left_stance : PE_InventoryEntity.act_pickup_middle_begin);
                        _successActionIndex = (isLeftStance ? PE_InventoryEntity.act_pickup_middle_end_left_stance : PE_InventoryEntity.act_pickup_middle_end);
                    }
                    else
                    {
                        _usedChannelIndex = 1;
                        _progressActionIndex = (isLeftStance ? PE_InventoryEntity.act_pickup_up_begin_left_stance : PE_InventoryEntity.act_pickup_up_begin);
                        _successActionIndex = (isLeftStance ? PE_InventoryEntity.act_pickup_up_end_left_stance : PE_InventoryEntity.act_pickup_up_end);
                    }
                    userAgent.SetActionChannel(_usedChannelIndex, _progressActionIndex, false, 0UL, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
                }
            }
        }
#endif
        public override void OnUseStopped(Agent userAgent, bool isSuccessful, int preferenceIndex)
        {
            base.OnUseStopped(userAgent, isSuccessful, preferenceIndex);
            
            Debug.Print("[USING LOG] AGENT USE STOPPED " + this.GetType().Name);

            if (isSuccessful)
            {
#if SERVER
                OpenInventory(userAgent);
#endif
            }
        }

        public void OnEntityRemove()
        {
#if SERVER
            if (playerInventoryComponent.CustomInventories.ContainsKey(InventoryId) || playerInventoryComponent.LootableObjects.ContainsKey(InventoryId))
            {
                playerInventoryComponent.CleanUpInventory(playerInventoryComponent.CustomInventories[InventoryId]);
            }
#endif
        }

        public void OnSpawnedByPrefab(PE_PrefabSpawner spawner)
        {
            // if(this.InventoryId.Trim() == "")
            // {
            InventoryId = GenerateId();
            // }
            if (playerInventoryComponent.CustomInventories != null && !playerInventoryComponent.CustomInventories.ContainsKey(InventoryId))
            {
                Inventory inventory = new Inventory(Slot, StackCount, InventoryId, this);
                inventory.GeneratedViaSpawner = true;
                playerInventoryComponent.CustomInventories[InventoryId] = inventory;
            }
        }

        internal void Remove(int removeReason)
        {
            var myTrace = new System.Diagnostics.StackTrace(0, true);

            try
            {
                if (_weakGameEntity.TryGetEntity(out var tmpGameEntity))
                {
                    Mission.Current.Scene.RemoveEntity(tmpGameEntity, removeReason);
                    //tmpGameEntity.Remove(removeReason);
                }
            }
            catch (Exception ex)
            {
                SaveSystemBehavior.RglExceptionThrown(myTrace, ex);
            }
        }
    }
}