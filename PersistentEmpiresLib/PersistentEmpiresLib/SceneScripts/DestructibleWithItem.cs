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
    public class PE_DestructibleWithItem : UsableMissionObject
    {

        public override ScriptComponentBehavior.TickRequirement GetTickRequirement()
        {
            if (base.GameEntity.IsVisibleIncludeParents() && GameNetwork.IsServer)
            {
                return base.GetTickRequirement() | ScriptComponentBehavior.TickRequirement.Tick;
            }
            return base.GetTickRequirement();
        }

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

        public float MaxHitPoint = 100f;
        protected float _hitPoint;

        public float HitPoint
        {
            get => this._hitPoint;
            set
            {
                if (!this._hitPoint.Equals(value))
                {
                    this._hitPoint = MathF.Max(value, 0f);
                }
            }
        }

        protected override void OnInit()
        {
            base.OnInit();
            this.initialFrame = base.GameEntity.GetGlobalFrame();
            this._hitPoint = this.MaxHitPoint;

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

        protected override void OnTick(float dt)
        {
            base.OnTick(dt);
            if (this.destructed && this.destructedAt + this.RespawnAsSeconds < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            {
                this.ResetObject();
            }
        }

        //public void TriggerOnHit(Agent attackerAgent, int inflictedDamage, Vec3 impactPosition, Vec3 impactDirection, in MissionWeapon weapon, ScriptComponentBehavior attackerScriptComponentBehavior)
        //{
        //    bool flag;
        //    float flag2;
        //    this.OnHit(attackerAgent, inflictedDamage, impactPosition, impactDirection, weapon, attackerScriptComponentBehavior, out flag, out flag2);
        //}

        private void SpawnItem(Agent agent, ItemObject item)
        {
            MissionWeapon spawnWeapon = new MissionWeapon(item, null, null);
            MatrixFrame frame = base.GameEntity.GetGlobalFrame();
            frame.origin = agent.Position;
            frame.origin.z += 1;

            var entity = ItemHelper.SpawnWeaponWithNewEntityAux(this.Scene, spawnWeapon, Mission.WeaponSpawnFlags.WithPhysics | Mission.WeaponSpawnFlags.WithHolster, frame, -1, null, true);
        }

        public void ResetObject()
        {
            if (this.ApplyPhysicsOnDestruction)
            {
                base.GameEntity.RemoveBodyFlags(BodyFlags.Moveable, true);
                base.GameEntity.RemoveBodyFlags(BodyFlags.Dynamic, true);
                base.GameEntity.SetBodyFlagsRecursive(BodyFlags.BodyOwnerNone);
            }
            else
            {
                GameEntity.SetVisibilityExcludeParents(true);
            }
            base.GameEntity.RemovePhysics();
            base.GameEntity.SetGlobalFrame(initialFrame);
            base.GameEntity.AddPhysics(base.GameEntity.Mass, base.GameEntity.CenterOfMass, base.GameEntity.GetBodyShape(), Vec3.Zero, Vec3.Zero, PhysicsMaterial.GetFromName(this.PhysicMaterial), true, 0);
            this.HitPoint = this.MaxHitPoint;
            this.destructed = false;

            if (GameNetwork.IsServer)
            {
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new ResetDestructableItem(this));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord, null);
            }
        }

        
        public void SetHitPoint(float hitPoint, Vec3 impactDirection, ScriptComponentBehavior attackerScriptComponentBehavior)
        {
            this.HitPoint = hitPoint;

            if (this.HitPoint <= 0)
            {
                MatrixFrame globalFrame = base.GameEntity.GetGlobalFrame();
                if (this.ParticleEffectOnDestroy != "")
                {
                    Mission.Current.Scene.CreateBurstParticle(ParticleSystemManager.GetRuntimeIdByName(this.ParticleEffectOnDestroy), globalFrame);
                }
                if (this.SoundEffectOnDestroy != "")
                {
                    Mission.Current.MakeSound(SoundEvent.GetEventIdFromString(this.SoundEffectOnDestroy), globalFrame.origin, false, true, -1, -1);
                }
                if (this.ApplyPhysicsOnDestruction)
                {
                    base.GameEntity.AddPhysics(base.GameEntity.Mass, base.GameEntity.CenterOfMass, base.GameEntity.GetBodyShape(), impactDirection * 3, Vec3.Zero, PhysicsMaterial.GetFromName(this.PhysicMaterial), false, 0);
                }
                else
                {
                    GameEntity.SetVisibilityExcludeParents(false);
                }
                destructedAt = DateTimeOffset.Now.ToUnixTimeSeconds();
                destructed = true;
            }
            if (GameNetwork.IsServer)
            {
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new SyncObjectHitpointsPE(this, impactDirection, this.HitPoint));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord, null);
            }
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

        public override TextObject GetDescriptionText(WeakGameEntity gameEntity)
        {
            return new TextObject("");
        }
    }
}