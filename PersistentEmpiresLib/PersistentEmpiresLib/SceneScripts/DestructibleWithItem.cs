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
        public bool ApplyPhysicsOnDestruction = true;
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
        private bool destructed = false;
        private long destructedAt = 0;

        private WeakGameEntity _weakEntity;

        protected override void OnInit()
        {
            base.OnInit();
            
            _weakEntity = base.GameEntity;

            if (_weakEntity.TryGetEntity(out var tmpGameEntity))
            {
                initialFrame = tmpGameEntity.GetGlobalFrame();
            }
            _hitPoint = MaxHitPoint;
            
            string[] dropItemList = ItemDrops.Split('|');
            
            foreach (string dropItemAsString in dropItemList)
            {
                string[] args = dropItemAsString.Split(',');
                string DropItemId = args[0];
                int DropChance = int.Parse(args[1]);
                int DropAmount = int.Parse(args[2]);
                float DropBelowHit = float.Parse(args[3]);
                this.DropItems.Add(new DropItem(DropItemId, DropChance, DropAmount, DropBelowHit));
            }
            this.ApplyPhysicsOnDestruction = false;
            if (RandomizedRespawn)
            {
                this.RespawnAsSeconds += MBRandom.RandomInt(this.RandomRespawnOffset);
            }
        }

        public override ScriptComponentBehavior.TickRequirement GetTickRequirement()
        {
            return base.GetTickRequirement() | ScriptComponentBehavior.TickRequirement.TickOccasionally;
        }

        protected override void OnTickOccasionally(float currentFrameDeltaTime)
        {
            base.OnTickOccasionally(currentFrameDeltaTime);
            if (this.destructed && this.destructedAt + this.RespawnAsSeconds < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            {
                this.ResetObject();
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

        public void ResetObject()
        {
#if SERVER
            if (_weakEntity.TryGetEntity(out var tmpGameEntity))
            {
                if (ApplyPhysicsOnDestruction)
                {
                    tmpGameEntity.RemoveBodyFlags(BodyFlags.Moveable, true);
                    tmpGameEntity.RemoveBodyFlags(BodyFlags.Dynamic, true);
                    tmpGameEntity.SetBodyFlagsRecursive(BodyFlags.BodyOwnerNone);
                }
                else
                {
                    tmpGameEntity.SetVisibilityExcludeParents(true);
                }
                //tmpGameEntity.RemovePhysics();
                tmpGameEntity.SetGlobalFrame(initialFrame);
                //tmpGameEntity.AddPhysics(tmpGameEntity.Mass, tmpGameEntity.CenterOfMass, tmpGameEntity.GetBodyShape(), Vec3.Zero, Vec3.Zero, PhysicsMaterial.GetFromName(PhysicMaterial), true, 0);
                HitPoint = MaxHitPoint;
                destructed = false;
            }

            GameNetwork.BeginBroadcastModuleEvent();
            GameNetwork.WriteMessage(new ResetDestructableItem(this));
            GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord, null);
#endif
#if CLIENT
            if (_weakEntity.TryGetEntity(out var tmpGameEntity))
            {
                if (ApplyPhysicsOnDestruction)
                {
                    tmpGameEntity.RemoveBodyFlags(BodyFlags.Moveable, true);
                    tmpGameEntity.RemoveBodyFlags(BodyFlags.Dynamic, true);
                    tmpGameEntity.SetBodyFlagsRecursive(BodyFlags.BodyOwnerNone);
                }
                else
                {
                    tmpGameEntity.SetVisibilityExcludeParents(true);
                }
                //tmpGameEntity.RemovePhysics();
                tmpGameEntity.SetGlobalFrame(initialFrame);
                //tmpGameEntity.AddPhysics(tmpGameEntity.Mass, tmpGameEntity.CenterOfMass, tmpGameEntity.GetBodyShape(), Vec3.Zero, Vec3.Zero, PhysicsMaterial.GetFromName(PhysicMaterial), true, 0);
                HitPoint = MaxHitPoint;
                destructed = false;
            }
#endif
        }

        public override void SetHitPoint(float hitPoint, Vec3 impactDirection, ScriptComponentBehavior attackerScriptComponentBehavior)
        {
#if SERVER
            HitPoint = hitPoint;

            if (HitPoint <= 0)
            {
                if (_weakEntity.TryGetEntity(out var tmpGameEntity))
                {
                    var globalFrame = tmpGameEntity.GetGlobalFrame();
                    /*
                    if (ParticleEffectOnDestroy != "")
                    {
                        Mission.Current.Scene.CreateBurstParticle(ParticleSystemManager.GetRuntimeIdByName(ParticleEffectOnDestroy), globalFrame);
                    }
                    if (SoundEffectOnDestroy != "")
                    {
                        Mission.Current.MakeSound(SoundEvent.GetEventIdFromString(SoundEffectOnDestroy), globalFrame.origin, false, true, -1, -1);
                    }
                    */
                    if (ApplyPhysicsOnDestruction)
                    {
                        tmpGameEntity.AddPhysics(tmpGameEntity.Mass, tmpGameEntity.CenterOfMass, tmpGameEntity.GetBodyShape(), impactDirection * 3, Vec3.Zero, PhysicsMaterial.GetFromName(PhysicMaterial), false, 0);
                    }
                    else
                    {
                        tmpGameEntity.SetVisibilityExcludeParents(false);
                    }

                    destructedAt = DateTimeOffset.Now.ToUnixTimeSeconds();
                    destructed = true;
                }
            }
            GameNetwork.BeginBroadcastModuleEvent();
            GameNetwork.WriteMessage(new SyncObjectHitpointsPE(this, impactDirection, this.HitPoint));
            GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord, null);
#endif
#if CLIENT
            this.HitPoint = hitPoint;

            if (HitPoint <= 0)
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

                        if (ApplyPhysicsOnDestruction)
                        {
                            tmpGameEntity.AddPhysics(tmpGameEntity.Mass, tmpGameEntity.CenterOfMass, tmpGameEntity.GetBodyShape(), impactDirection * 3, Vec3.Zero, PhysicsMaterial.GetFromName(PhysicMaterial), false, 0);
                        }
                        else
                        {
                            tmpGameEntity.SetVisibilityExcludeParents(false);
                        }

                        destructedAt = DateTimeOffset.Now.ToUnixTimeSeconds();
                        destructed = true;
                    }
                }
#endif
        }

        protected override bool OnHit(Agent attackerAgent, int damage, Vec3 impactPosition, Vec3 impactDirection, in MissionWeapon weapon, int affectorWeaponSlotOrMissileIndex, ScriptComponentBehavior attackerScriptComponentBehavior, out bool reportDamage, out float finalDamage)
        {
            reportDamage = true;
            MissionWeapon missionWeapon = weapon;
            WeaponComponentData currentUsageItem = missionWeapon.CurrentUsageItem;
            if (weapon.Item == null || weapon.Item.StringId != this.RequiredItemId || this.destructed)
            {
                reportDamage = false;
                finalDamage = 0;
                damage = 0;
                return false;
            }
            SkillObject requiredSkillObject = MBObjectManager.Instance.GetObject<SkillObject>(this.RequiredSkillId);
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
                    ItemObject item = MBObjectManager.Instance.GetObject<ItemObject>(dropItem.DropItemId);
                    PersistentEmpireRepresentative persistentEmpireRepresentative = attackerAgent.MissionPeer.GetNetworkPeer().GetComponent<PersistentEmpireRepresentative>();
                    Inventory inventory = persistentEmpireRepresentative.GetInventory();
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

            return false;
        }
    }
}