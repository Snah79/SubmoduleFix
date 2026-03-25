using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.NetworkMessages.Server;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace PersistentEmpiresLib.SceneScripts
{
    public class PE_ItemGathering : PE_UsableFromDistance
    {
        public string Name = "Berry";
        public int RespawnTime = 10;
        public int ItemCount = 10;
        public int AnimationDurationInSeconds = 5;
        public string Animation = "act_npc_farmer_bush_cutting_while_stand";
        public string NeededItem = "";
        public string RequiredSkillId = "Gathering";
        public int RequiredSkill = 15;
        public string DropsItem = "";
        public int DropCount = 1;
        public bool RotateWhenUsage = false;
        public string LookPointTag = "lookpoint";
        public bool RandomizedRespawn = false;
        public int RandomRespawnOffset = 0;
        public bool IsDestroyed = false;
        private long DestroyedAt = 0;
        private long UseStartedAt = 0;
        private long UseWillEndAt = 0;
        private int CurrentCount = 0;
        private ItemObject DropsItemObject;
        private WeakGameEntity _weakEntity;

        public override bool LockUserFrames
        {
            get
            {
                return false;
            }
        }
        public override bool LockUserPositions
        {
            get
            {
                return false;
            }
        }

        protected override void OnInit()
        {
            base.OnInit();

            _weakEntity = GameEntity;
#if CLIENT
            ActionMessage = new TextObject(Name);
            TextObject descriptionMessage = new TextObject("Press {KEY} To Gather");
            descriptionMessage.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
            DescriptionMessage = descriptionMessage;
#endif
            DropsItemObject = MBObjectManager.Instance.GetObject<ItemObject>(DropsItem);
#if SERVER
            CurrentCount = ItemCount;
            if (RandomizedRespawn)
            {
                RespawnTime += MBRandom.RandomInt(RandomRespawnOffset);
            }
            if (DropsItemObject == null)
            {
                Debug.Print(DropsItem + " CANNOT BE FOUND ON PE_ITEMGATHERING", 0, Debug.DebugColor.Red);
            }
#endif
        }

#if SERVER
        public override ScriptComponentBehavior.TickRequirement GetTickRequirement()
        {
            return base.GetTickRequirement() | ScriptComponentBehavior.TickRequirement.TickOccasionally;
        }

        protected override void OnTickOccasionally(float currentFrameDeltaTime)
        {
            base.OnTickOccasionally(currentFrameDeltaTime);
            this.DoTick(currentFrameDeltaTime);
        }

        protected void DoTick(float dt)
        {
            if (base.HasUser)
            {
                if (base.HasUser)
                {
                    if (this.UseWillEndAt < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                    {
                        base.UserAgent.StopUsingGameObjectMT(base.UserAgent.CanUseObject(this));
                        //GetTickRequirement();
                    }
                }
            }
            if (this.IsDestroyed && this.DestroyedAt + this.RespawnTime < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            {
                this.UpdateIsDestroyed(false);
                //GetTickRequirement();
            }
        }
#endif

        public void UpdateIsDestroyed(bool isDestroyed)
        {
#if SERVER
            if (!_weakEntity.TryGetEntity(out var tmpGameEntity))
            {
                return;
            }

            if (!isDestroyed)
            {
                CurrentCount = this.ItemCount;
                tmpGameEntity.SetVisibilityExcludeParents(true);
                IsDestroyed = false;
            }
            else
            {
                IsDestroyed = true;
                tmpGameEntity.SetVisibilityExcludeParents(false);
                DestroyedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }

            GameNetwork.BeginBroadcastModuleEvent();
            GameNetwork.WriteMessage(new UpdateItemGatheringDestroyed(this, isDestroyed));
            GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
#endif
#if CLIENT
            if (!_weakEntity.TryGetEntity(out var tmpGameEntity))
            {
                return;
            }

            if (!isDestroyed)
            {
                tmpGameEntity.SetVisibilityExcludeParents(true);
            }
            else
            {
                tmpGameEntity.SetVisibilityExcludeParents(false);
            }
#endif
        }

        public override void OnUseStopped(Agent userAgent, bool isSuccessful, int preferenceIndex)
        {
            base.OnUseStopped(userAgent, isSuccessful, preferenceIndex);
            Debug.Print("[USING LOG] AGENT USE " + this.GetType().Name);

            userAgent.SetActionChannel(0, ActionIndexCache.act_none, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, MBRandom.RandomFloatRanged(1f), false, -0.2f, 0, true);
#if SERVER
            if (isSuccessful)
            {
                CurrentCount--;
                
                var  peer = userAgent.MissionPeer.GetNetworkPeer();
                var  persistentEmpireRepresentative = peer.GetComponent<PersistentEmpireRepresentative>();
                var playerInventory = persistentEmpireRepresentative.GetInventory();

                playerInventory.AddCountedItemSynced(DropsItemObject, DropCount, ItemHelper.GetMaximumAmmo(DropsItemObject));
                LoggerHelper.LogAnActionNoDiscord(peer, LogAction.PlayerItemGathers, null, new object[] { DropsItemObject });
                if (CurrentCount == 0)
                {
                    UpdateIsDestroyed(true);
                }
            }
#endif
#if CLIENT
            if (userAgent.IsMine)
            {
                PEInformationManager.StopCounter();
            }
#endif
            userAgent.ClearTargetFrame();
        }

        public override void OnUse(Agent userAgent, sbyte agentBoneIndex)
        {
#if SERVER
            Debug.Print("[USING LOG] AGENT USE " + GetType().Name);

            if (HasUser)
            {
                userAgent.StopUsingGameObjectMT(false);
                
                return;
            }
            SkillObject requiredSkillObject = MBObjectManager.Instance.GetObject<SkillObject>(RequiredSkillId);
            if (userAgent.Character.GetSkillValue(requiredSkillObject) < RequiredSkill)
            {
                InformationComponent.Instance.SendMessage(GameTexts.FindText("PE_Not_Qualified", null).ToString(), new Color(1f, 0, 0).ToUnsignedInteger(), userAgent.MissionPeer.GetNetworkPeer());
                userAgent.StopUsingGameObjectMT(false);
                return;
            }
            EquipmentIndex wieldedItemIndex = userAgent.GetPrimaryWieldedItemIndex();//.GetWieldedItemIndex(Agent.HandIndex.MainHand);
                                                                                     // MissionWeapon wieldedItem = userAgent.Equipment[wieldedItemIndex];
            if (NeededItem == "" && wieldedItemIndex != EquipmentIndex.None)
            {
                InformationComponent.Instance.SendMessage(GameTexts.FindText("PE_Empty_Your_Hands", null).ToString(), new Color(1f, 0, 0).ToUnsignedInteger(), userAgent.MissionPeer.GetNetworkPeer());
                userAgent.StopUsingGameObjectMT(false);
                return;
            }
            else if (NeededItem != "")
            {
                if (wieldedItemIndex == EquipmentIndex.None)
                {
                    var neededItems = NeededItem.Split(';');
                    var tmps = neededItems.Select(x=> MBObjectManager.Instance.GetObject<ItemObject>(x));
                    InformationComponent.Instance.SendMessage("You need a " + string.Join(" or ", tmps.Select(x=> x.Name.ToString())) + " to do this.", new Color(1f, 0, 0).ToUnsignedInteger(), userAgent.MissionPeer.GetNetworkPeer());
                    userAgent.StopUsingGameObjectMT(false);
                    return;
                }
                else
                {
                    var wieldedItem = userAgent.Equipment[wieldedItemIndex];
                    var neededItems = NeededItem.Split(';');

                    if (!neededItems.Any(x=> x ==  wieldedItem.Item.StringId))
                    {
                        var tmps = neededItems.Select(x => MBObjectManager.Instance.GetObject<ItemObject>(x));
                        InformationComponent.Instance.SendMessage("You need a " + string.Join(" or ", tmps.Select(x => x.Name.ToString())) + " to do this.", new Color(1f, 0, 0).ToUnsignedInteger(), userAgent.MissionPeer.GetNetworkPeer());
                        userAgent.StopUsingGameObjectMT(false);
                        return;
                    }
                }
            }
            if (IsDestroyed)
            {
                InformationComponent.Instance.SendMessage("This object is disabled please try later.", new Color(1f, 0, 0).ToUnsignedInteger(), userAgent.MissionPeer.GetNetworkPeer());
                userAgent.StopUsingGameObjectMT(false);
                return;
            }

            var peer = userAgent.MissionPeer.GetNetworkPeer();
            var persistentEmpireRepresentative = peer.GetComponent<PersistentEmpireRepresentative>();
            var playerInventory = persistentEmpireRepresentative.GetInventory();
            
            if (!playerInventory.HasEnoughRoomFor(DropsItemObject, DropCount))
            {
                InformationComponent.Instance.SendMessage(GameTexts.FindText("PE_Not_Enough_Space", null).ToString(), new Color(1f, 0, 0).ToUnsignedInteger(), userAgent.MissionPeer.GetNetworkPeer());
                userAgent.StopUsingGameObjectMT(false);
                return;
            }
#endif
            var actionIndexCache = ActionIndexCache.Create(Animation);
            
            userAgent.SetActionChannel(0, actionIndexCache, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
            UseStartedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            UseWillEndAt = UseStartedAt + AnimationDurationInSeconds;

            if (RotateWhenUsage)
            {
                var entity = base.GameEntity.GetFirstChildEntityWithTag(LookPointTag);
                GameEntityWithWorldPosition gameEntityWithWorldPosition = new GameEntityWithWorldPosition(entity);
                WorldFrame userFrameLook = gameEntityWithWorldPosition.WorldFrame;
                WorldFrame userFrameForAgent = this.GetUserFrameForAgent(userAgent);
                userAgent.SetTargetPositionAndDirection(userFrameForAgent.Origin.AsVec2, -userFrameLook.Rotation.f);
            }
            else
            {
                userAgent.SetTargetPosition(userAgent.GetWorldFrame().Origin.AsVec2);
            }
#if CLIENT
            if (userAgent.IsMine)
            {
                PEInformationManager.StartCounter("Gathering " + DropsItemObject.Name.ToString() + "...", this.AnimationDurationInSeconds);
            }
#endif
            base.OnUse(userAgent, agentBoneIndex);
        }

        public override TextObject GetDescriptionText(WeakGameEntity gameEntity)
        {
            return new TextObject("Item Gathering");
        }
    }
}