using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresLib.SceneScripts.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
    public class PE_MoveableGroundedMachine : PE_MoveableMachine
    {
        public string Name = "Mangonel";
        public bool isPlayerUsing = false;
        public string Animation = "";
        public int StrayDurationSeconds = 7200;

        public string RidingSkillId = "";
        public int RidingSkillRequired = 0;

        public string RepairingSkillId = "";
        public int RepairingSkillRequired = 0;

        public string RepairItemRecipies = "pe_hardwood*2,pe_wooden_stick*1";
        public int RepairDamage = 20;
        public string RepairItem = "pe_buildhammer";
        public string ParticleEffectOnDestroy = "";
        public string SoundEffectOnDestroy = "";
        public string ParticleEffectOnRepair = "";
        public string SoundEffectOnRepair = "";
        public bool DestroyedByStoneOnly = false;

        private long WillBeDeletedAt = 0;
        private SkillObject RidingSkill;
        private SkillObject RepairSkill;

        private List<RepairReceipt> receipt = new List<RepairReceipt>();
        private WeakGameEntity _weakGameEntity;

        public override ScriptComponentBehavior.TickRequirement GetTickRequirement() => !this.GameEntity.IsVisibleIncludeParents() ? base.GetTickRequirement() : ScriptComponentBehavior.TickRequirement.Tick | ScriptComponentBehavior.TickRequirement.TickParallel;

        private void ParseRepairReceipts()
        {
            string[] repairReceipt = this.RepairItemRecipies.Split(',');
            foreach (string receipt in repairReceipt)
            {
                string[] inflictedReceipt = receipt.Split('*');
                string receiptId = inflictedReceipt[0];
                int count = int.Parse(inflictedReceipt[1]);
                this.receipt.Add(new RepairReceipt(receiptId, count));
            }
        }
        public void PreInit()
        {
            if (this.RidingSkillId != "")
            {
                this.RidingSkill = MBObjectManager.Instance.GetObject<SkillObject>(this.RidingSkillId);
            }
            if (this.RepairingSkillId != "")
            {
                this.RepairSkill = MBObjectManager.Instance.GetObject<SkillObject>(this.RepairingSkillId);
            }
            this.ParseRepairReceipts();
            this.ResetStrayDuration();
            this.HitPoint = this.MaxHitPoint;
            this.AlwaysAlignToTerritory = true;
        }

        private bool initCompleted = false;
        protected override void OnInit()
        {
            initCompleted = false;

            base.OnInit();

            _weakGameEntity = GameEntity;

            if (RidingSkillId != "")
            {
                RidingSkill = MBObjectManager.Instance.GetObject<SkillObject>(RidingSkillId);
            }
            if (RepairingSkillId != "")
            {
                RepairSkill = MBObjectManager.Instance.GetObject<SkillObject>(RepairingSkillId);
            }
            ParseRepairReceipts();
            ResetStrayDuration();
            HitPoint = MaxHitPoint;
            AlwaysAlignToTerritory = true;
            initCompleted = true;
        }

        public bool IsAgentFullyUsing(Agent usingAgent)
        {
            return this.PilotAgent == usingAgent;
        }
        public override TextObject GetActionTextForStandingPoint(UsableMissionObject usableGameObject)
        {
            TextObject forStandingPoint = new TextObject(this.IsAgentFullyUsing(GameNetwork.MyPeer.ControlledAgent) ? "{=QGdaakYW}{KEY} Stop Using" : "{=bl2aRW8f}{KEY} Use machine");
            forStandingPoint.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
            return forStandingPoint;
        }

        public override TextObject GetDescriptionText(WeakGameEntity gameEntity)
        {
            return new TextObject("{=}" + this.Name);
        }

        public override bool IsStray()
        {
            return false;
            /*if (this.PilotAgent != null) return false;
            return this.WillBeDeletedAt < DateTimeOffset.UtcNow.ToUnixTimeSeconds();*/
        }

        public override void ResetStrayDuration()
        {
            this.WillBeDeletedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + this.StrayDurationSeconds;
        }

        public override void SetHitPoint(float hitPoint, Vec3 impactDirection)
        {
            HitPoint = hitPoint;
            var globalFrame = GameEntity.GetGlobalFrame();

            if (HitPoint > MaxHitPoint) HitPoint = MaxHitPoint;
            if (HitPoint < 0) HitPoint = 0;

            if (HitPoint == 0)
            {
                if (PilotAgent != null)
                {
                    PilotAgent.StopUsingGameObjectMT(false);
                }
#if CLIENT
                if (ParticleEffectOnDestroy != "")
                {
                    Mission.Current.Scene.CreateBurstParticle(ParticleSystemManager.GetRuntimeIdByName(ParticleEffectOnDestroy), globalFrame);
                }
                if (SoundEffectOnDestroy != "")
                {
                    Mission.Current.MakeSound(SoundEvent.GetEventIdFromString(SoundEffectOnDestroy), globalFrame.origin, false, true, -1, -1);
                }
#endif
                Remove(0);
            }
            if (HitPoint == MaxHitPoint)
            {
#if CLIENT
                if (ParticleEffectOnRepair != "")
                {
                    Mission.Current.Scene.CreateBurstParticle(ParticleSystemManager.GetRuntimeIdByName(ParticleEffectOnRepair), globalFrame);
                }
                if (SoundEffectOnRepair != "")
                {
                    Mission.Current.MakeSound(SoundEvent.GetEventIdFromString(SoundEffectOnRepair), globalFrame.origin, false, true, -1, -1);
                }
#endif
            }
        }
        protected override void OnTick(float dt)
        {
            if (!_weakGameEntity.TryGetEntity(out var tmpGameEntity))
            {
                return;
            }

            if (!initCompleted)
            {
                return;
            }

            base.OnTick(dt);
            if (GameNetwork.IsServer)
            {
                if (PilotAgent != null)
                {
                    if (RidingSkill != null)
                    {
                        int skillValue = PilotAgent.Character.GetSkillValue(RidingSkill);
                        if (skillValue < RidingSkillRequired)
                        {
                            PilotAgent.StopUsingGameObjectMT(false);
                            return;
                        }
                    }
                    ResetStrayDuration();

                    if (PilotAgent.Position.Distance(tmpGameEntity.GlobalPosition) > 5f)
                    {
                        PilotAgent.StopUsingGameObjectMT(false);
                    }
                }
            }

            if (GameNetwork.IsClient)
            {
                if (Agent.Main != null && PilotAgent == Agent.Main)
                {
                    if (Mission.Current.InputManager.IsKeyPressed(InputKey.W))
                    {
                        this.RequestMovingForward();
                    }
                    else if (Mission.Current.InputManager.IsKeyReleased(InputKey.W))
                    {
                        this.RequestStopMovingForward();
                    }
                    if (Mission.Current.InputManager.IsKeyPressed(InputKey.S))
                    {
                        this.RequestMovingBackward();
                    }
                    else if (Mission.Current.InputManager.IsKeyReleased(InputKey.S))
                    {
                        this.RequestStopMovingBackward();
                    }
                    if (Mission.Current.InputManager.IsKeyPressed(InputKey.A))
                    {
                        this.RequestTurningLeft();
                    }
                    else if (Mission.Current.InputManager.IsKeyReleased(InputKey.A))
                    {
                        this.RequestStopTurningLeft();
                    }
                    if (Mission.Current.InputManager.IsKeyPressed(InputKey.D))
                    {
                        this.RequestTurningRight();
                    }
                    else if (Mission.Current.InputManager.IsKeyReleased(InputKey.D))
                    {
                        this.RequestStopTurningRight();
                    }
                    if (Mission.Current.InputManager.IsKeyPressed(InputKey.Space))
                    {
                        this.RequestMovingUp();
                    }
                    else if (Mission.Current.InputManager.IsKeyReleased(InputKey.Space))
                    {
                        this.RequestStopMovingUp();
                    }
                    if (Mission.Current.InputManager.IsKeyPressed(InputKey.LeftShift))
                    {
                        this.RequestMovingDown();
                    }
                    else if (Mission.Current.InputManager.IsKeyReleased(InputKey.LeftShift))
                    {
                        this.RequestStopMovingDown();
                    }
                    if (Mission.Current.InputManager.IsKeyPressed(InputKey.F))
                    {
                        GameNetwork.MyPeer.ControlledAgent.HandleStopUsingAction();
                        isPlayerUsing = false;
                        ActionIndexCache ac = ActionIndexCache.act_none;
                        PilotAgent.SetActionChannel(0, ac, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, 0, false, -0.2f, 0, true);
                    }
                }
            }
            if (PilotAgent == null)
            {
                if (base.IsMovingBackward) this.StopMovingBackward();
                if (base.IsMovingDown) this.StopMovingDown();
                if (base.IsMovingForward) this.StopMovingForward();
                if (base.IsMovingUp) this.StopMovingUp();
                if (base.IsTurningLeft) this.StopTurningLeft();
                if (base.IsTurningRight) this.StopTurningRight();
            }
        }

        protected override void OnFixedTick(float fixedDt)
        {
            if (!initCompleted)
            {
                return;
            }
            base.OnFixedTick(fixedDt);
        }

        protected override void OnParallelFixedTick(float fixedDt)
        {
            if (!initCompleted)
            {
                return;
            }
            base.OnParallelFixedTick(fixedDt);
        }

        protected override void OnTickOccasionally(float currentFrameDeltaTime)
        {
            if (!initCompleted)
            {
                return;
            }
            base.OnTickOccasionally(currentFrameDeltaTime);
        }

        protected override void OnTickParallel2(float dt)
        {
            if (!initCompleted)
            {
                return;
            }
            base.OnTickParallel2(dt);
        }

        protected override void OnTickParallel3(float dt)
        {
            if (!initCompleted)
            {
                return;
            }
            base.OnTickParallel3(dt);
        }

        protected override void OnTickParallel(float dt)
        {
            if (!initCompleted)
            {
                return;
            }

            base.OnTickParallel(dt);

            if (_weakGameEntity.TryGetEntity(out var tmpGameEntity))
            {
                if (!tmpGameEntity.IsVisibleIncludeParents())
                {
                    return;
                }
            }
        }

        protected override bool OnHit(Agent attackerAgent, int damage, Vec3 impactPosition, Vec3 impactDirection, in MissionWeapon weapon, int affectorWeaponSlotOrMissileIndex, ScriptComponentBehavior attackerScriptComponentBehavior, out bool reportDamage, out float finalDamage)
        {
            reportDamage = true;
            MissionWeapon missionWeapon = weapon;
            WeaponComponentData currentUsageItem = missionWeapon.CurrentUsageItem;
            if (
                attackerAgent != null &&
                this.RepairSkill != null &&
                attackerAgent.Character.GetSkillValue(this.RepairSkill) >= this.RepairingSkillRequired &&
                missionWeapon.Item != null &&
                missionWeapon.Item.StringId == this.RepairItem &&
                attackerAgent.IsHuman &&
                attackerAgent.IsPlayerControlled &&
                this.HitPoint != this.MaxHitPoint
                )
            {
                reportDamage = false;
                finalDamage = 0;
                NetworkCommunicator player = attackerAgent.MissionPeer.GetNetworkPeer();
                PersistentEmpireRepresentative persistentEmpireRepresentative = player.GetComponent<PersistentEmpireRepresentative>();
                if (persistentEmpireRepresentative == null) return false;
                bool playerHasAllItems = this.receipt.All((r) => persistentEmpireRepresentative.GetInventory().IsInventoryIncludes(r.RepairItem, r.NeededCount));
                if (!playerHasAllItems)
                {
                    //TODO: Inform player
                    InformationComponent.Instance.SendMessage(GameTexts.FindText("PE_Required_Items", null).ToString(), 0x02ab89d9, player);
                    foreach (RepairReceipt r in this.receipt)
                    {
                        InformationComponent.Instance.SendMessage(r.NeededCount + " * " + r.RepairItem.Name.ToString(), 0x02ab89d9, player);
                    }
                    return false;
                }
                foreach (RepairReceipt r in this.receipt)
                {
                    persistentEmpireRepresentative.GetInventory().RemoveCountedItem(r.RepairItem, r.NeededCount);
                }
                InformationComponent.Instance.SendMessage((this.HitPoint + this.RepairDamage).ToString() + "/" + this.MaxHitPoint + ", repaired", 0x02ab89d9, player);
                this.SetHitPoint(this.HitPoint + this.RepairDamage, impactDirection);
            }
            else
            {
                if (this.DestroyedByStoneOnly)
                {
                    if (currentUsageItem == null || (currentUsageItem.WeaponClass != WeaponClass.Stone && currentUsageItem.WeaponClass != WeaponClass.Boulder) || !currentUsageItem.WeaponFlags.HasAnyFlag(WeaponFlags.NotUsableWithOneHand))
                    {
                        damage = 0;
                    }
                }
                if (impactDirection == null) impactDirection = Vec3.Zero;
                this.SetHitPoint(this.HitPoint - damage, impactDirection);
            }
            finalDamage = damage;

            return false;
        }

        internal void Remove(int reason)
        {
            var myTrace = new StackTrace(0, true);

            try
            {
                if (_weakGameEntity.TryGetEntity(out var tmpGameEntity))
                {
                    Mission.Current.Scene.RemoveEntity(tmpGameEntity, reason);
                    //tmpGameEntity.Remove(80);
                }
            }
            catch (Exception ex)
            {
                SaveSystemBehavior.RglExceptionThrown(myTrace, ex);
            }
        }
    }
}