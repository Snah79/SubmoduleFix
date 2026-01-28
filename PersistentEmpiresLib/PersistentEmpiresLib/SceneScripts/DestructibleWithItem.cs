using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.NetworkMessages.Server;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace PersistentEmpiresLib.SceneScripts
{
    public struct DropItem
    {
        public DropItem(string DropItemId, int DropChance, int DropAmount, float DropBelowHit)
        {
            this.DropItemId = DropItemId;
            this.DropChance = DropChance;
            this.DropAmount = DropAmount;
            this.DropBelowHit = DropBelowHit;
        }
        public string DropItemId { get; set; }
        public int DropChance { get; set; }
        public int DropAmount { get; set; }
        public float DropBelowHit { get; set; }
    }
    public class PE_DestructibleWithItem : PE_DestructableComponent
    {
        private List<DropItem> DropItems = new List<DropItem>();
        public string ItemDrops;
        public int RespawnAsSeconds = 5;
        public bool ApplyPhysicsOnDestruction = false;
        public string PhysicMaterial = "wood";
        public string ParticleEffectOnDestroy = "";
        public string SoundEffectOnDestroy = "";
        public float SoundAndParticleEffectHeightOffset;
        public float SoundAndParticleEffectForwardOffset;
        public string RequiredSkillId = "Gathering";
        public int RequiredSkillLevel = 10;
        public string RequiredItemId = "pe_buildhammer";
        public bool RandomizedRespawn = false;
        public int RandomRespawnOffset = 0;

        private MatrixFrame initialFrame;
        internal bool destructed = false;
        private long destructedAt = 0;

        private WeakGameEntity _weakEntity;

        protected override void OnInit()
        {
            base.OnInit();

            _weakEntity = GameEntity;

            if (_weakEntity.TryGetEntity(out var tmpGameEntity))
            {
                initialFrame = tmpGameEntity.GetGlobalFrame();
            }
#if SERVER
            _hitPoint = MaxHitPoint;

            string[] dropItemList = ItemDrops.Split('|');

            foreach (string dropItemAsString in dropItemList)
            {
                string[] args = dropItemAsString.Split(',');
                string DropItemId = args[0];
                int DropChance = int.Parse(args[1]);
                int DropAmount = int.Parse(args[2]);
                float DropBelowHit = float.Parse(args[3]);
                DropItems.Add(new DropItem(DropItemId, DropChance, DropAmount, DropBelowHit));
            }

            if (RandomizedRespawn)
            {
                RespawnAsSeconds += MBRandom.RandomInt(this.RandomRespawnOffset);
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
            if (destructed && (destructedAt + RespawnAsSeconds < DateTimeOffset.UtcNow.ToUnixTimeSeconds()))
            {
                ResetObject();
            }
        }

        private void SpawnItem(Agent agent, ItemObject item)
        {
            if (_weakEntity.TryGetEntity(out var tmpGameEntity))
            {
                var spawnWeapon = new MissionWeapon(item, null, null);
                var frame = tmpGameEntity.GetGlobalFrame();

                frame.origin = agent.Position;
                frame.origin.z += 1;

                var entity = ItemHelper.SpawnWeaponWithNewEntityAux(Scene, spawnWeapon, Mission.WeaponSpawnFlags.WithPhysics | Mission.WeaponSpawnFlags.WithHolster, frame, -1, null, true);
            }
        }
#endif

        public void ResetObject()
        {
            if (destructed)
            {
#if SERVER
                var myTrace = new System.Diagnostics.StackTrace(0, true);
                try
                {
                    if (_weakEntity.TryGetEntity(out var tmpGameEntity))
                    {
                        tmpGameEntity.SetVisibilityExcludeParents(true);
                        tmpGameEntity.SetGlobalFrame(initialFrame);
                        HitPoint = MaxHitPoint;
                        destructed = false;
                    }

                    GameNetwork.BeginBroadcastModuleEvent();
                    GameNetwork.WriteMessage(new ResetDestructableItem(this));
                    GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord, null);
                }
                catch (Exception ex)
                {
                    var tmp = $"Exception was thrown in PE_DestructibleWithItem ResetObject.";
                    ex.HelpLink = tmp;
                    SaveSystemBehavior.RglExceptionThrown(myTrace, ex);
                }
#endif
#if CLIENT
                if (_weakEntity.TryGetEntity(out var tmpGameEntity))
                {
                    tmpGameEntity.SetVisibilityExcludeParents(true);
                    tmpGameEntity.SetGlobalFrame(initialFrame);
                }
#endif
            }
        }

#if SERVER
        public override void SetHitPoint(float hitPoint, Vec3 impactDirection, ScriptComponentBehavior attackerScriptComponentBehavior)
        {
            var myTrace = new System.Diagnostics.StackTrace(0, true);
            try
            {
                HitPoint = hitPoint;

                if (HitPoint <= 0 && !destructed)
                {
                    if (_weakEntity.TryGetEntity(out var tmpGameEntity))
                    {
                        var globalFrame = tmpGameEntity.GetGlobalFrame();

                        tmpGameEntity.SetVisibilityExcludeParents(false);

                        destructedAt = DateTimeOffset.Now.ToUnixTimeSeconds();
                        destructed = true;
                    }

                    // update clients only when they need to be destroyed
                    GameNetwork.BeginBroadcastModuleEvent();
                    GameNetwork.WriteMessage(new SyncObjectHitpointsPE(this, impactDirection, HitPoint));
                    GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord, null);
                }
            }
            catch (Exception ex)
            {
                var tmp = $"Exception was thrown in PE_DestructibleWithItem         public override void SetHitPoint(float hitPoint, Vec3 impactDirection, ScriptComponentBehavior attackerScriptComponentBehavior)\r\n.";
                ex.HelpLink = tmp;
                SaveSystemBehavior.RglExceptionThrown(myTrace, ex);
            }
        }

        protected override bool OnHit(Agent attackerAgent, int damage, Vec3 impactPosition, Vec3 impactDirection, in MissionWeapon weapon, int affectorWeaponSlotOrMissileIndex, ScriptComponentBehavior attackerScriptComponentBehavior, out bool reportDamage, out float finalDamage)
        {
            var myTrace = new System.Diagnostics.StackTrace(0, true);
            
            reportDamage = false;
            finalDamage = 0;

            try
            {
                var missionWeapon = weapon;
                var currentUsageItem = missionWeapon.CurrentUsageItem;

                if (weapon.Item == null || weapon.Item.StringId != this.RequiredItemId || this.destructed)
                {
                    reportDamage = false;
                    finalDamage = 0;
                    damage = 0;
                    return false;
                }

                var requiredSkillObject = MBObjectManager.Instance.GetObject<SkillObject>(this.RequiredSkillId);

                if (attackerAgent.Character.GetSkillValue(requiredSkillObject) < this.RequiredSkillLevel)
                {
                    reportDamage = false;
                    finalDamage = 0;
                    damage = 0;
                    return false;
                }

                if (attackerAgent == null)
                {
                    reportDamage = false;
                    finalDamage = 0;
                    damage = 0;
                    return false;
                }

                foreach (DropItem dropItem in this.DropItems)
                {
                    if (dropItem.DropChance >= MBRandom.RandomInt(100))
                    {
                        var item = MBObjectManager.Instance.GetObject<ItemObject>(dropItem.DropItemId);
                        var persistentEmpireRepresentative = attackerAgent.MissionPeer.GetNetworkPeer().GetComponent<PersistentEmpireRepresentative>();
                        var inventory = persistentEmpireRepresentative.GetInventory();

                        InformationComponent.Instance.SendMessage("You gathered " + dropItem.DropAmount + "*" + item.Name.ToString(), Colors.Green.ToUnsignedInteger(), attackerAgent.MissionPeer.GetNetworkPeer());

                        if (inventory.HasEnoughRoomFor(item, dropItem.DropAmount) == false)
                        {
                            InformationComponent.Instance.SendMessage(GameTexts.FindText("PE_Not_Enough_Space_Drop", null).ToString(), Colors.Red.ToUnsignedInteger(), attackerAgent.MissionPeer.GetNetworkPeer());
                        }
                        for (int i = 0; i < dropItem.DropAmount; i++)
                        {
                            if (persistentEmpireRepresentative != null)
                            {
                                if (inventory.HasEnoughRoomFor(item, 1))
                                {
                                    inventory.AddCountedItemSynced(item, 1, ItemHelper.GetMaximumAmmo(item));
                                }
                                else
                                {
                                    SpawnItem(attackerAgent, item);
                                }
                            }
                        }
                    }
                }

                damage = 10;
                finalDamage = damage;

                SetHitPoint(HitPoint - damage, impactDirection, attackerScriptComponentBehavior);
            }
            catch (Exception ex)
            {
                var tmp = $"Exception was thrown in PE_DestructibleWithItem  OnHit";
                ex.HelpLink = tmp;
                SaveSystemBehavior.RglExceptionThrown(myTrace, ex);
            }
            return false;
        }
#endif
#if CLIENT
        public override void SetHitPoint(float hitPoint, Vec3 impactDirection, ScriptComponentBehavior attackerScriptComponentBehavior)
        {
            if (hitPoint <= 0)
            {
                if (_weakEntity.TryGetEntity(out var tmpGameEntity))
                {
                    var globalFrame = tmpGameEntity.GetGlobalFrame();
                    if (ParticleEffectOnDestroy != "")
                    {
                        Mission.Current.Scene.CreateBurstParticle(ParticleSystemManager.GetRuntimeIdByName(ParticleEffectOnDestroy), globalFrame);
                    }
                    if (SoundEffectOnDestroy != "")
                    {
                        Mission.Current.MakeSound(SoundEvent.GetEventIdFromString(SoundEffectOnDestroy), globalFrame.origin, false, true, -1, -1);
                    }

                    tmpGameEntity.SetVisibilityExcludeParents(false);

                    destructedAt = DateTimeOffset.Now.ToUnixTimeSeconds();
                    destructed = true;
                }
            }
        }
#endif
    }
}