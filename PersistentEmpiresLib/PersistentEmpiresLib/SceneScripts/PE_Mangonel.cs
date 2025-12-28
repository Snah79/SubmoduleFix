using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.SceneScripts.Extensions;
using PersistentEmpiresLib.SceneScripts.Interfaces;
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
    public sealed class PE_MangonelAI : RangedSiegeWeaponAi
    {
        // Token: 0x06001084 RID: 4228 RVA: 0x00035E83 File Offset: 0x00034083
        public PE_MangonelAI(PE_Mangonel mangonel) : base(mangonel)
        {
        }
    }
    public class PE_Mangonel : Mangonel, ISpawnable, IMoveable
    {
        protected override float MaximumBallisticError => 1.5f;
        protected override float ShootingSpeed => ProjectileSpeed;
        public bool IsMovingForward { get; set; }
        public bool IsMovingBackward { get; set; }
        public bool IsTurningRight { get; set; }
        public bool IsTurningLeft { get; set; }
        public bool IsMovingUp { get; set; }
        public bool IsMovingDown { get; set; }
        public bool DestroyedByStoneOnly = false;
        public float HitPoint;
        public float MaxHitPoint = 200f;
        public string ParticleEffectOnDestroy = "psys_siege_sturgia_wall_destruction";
        public string SoundEffectOnDestroy = "event:/mission/siege/generic/stone_destroy";
        private StandingPoint moverStandingPoint;
        public string MoverStandingPointTag = "mover";
        private float _timeElapsedAfterLoading;
        private ActionIndexCache _loadAmmoEndAnimationActionIndex;
        private ActionIndexCache _loadAmmoBeginAnimationActionIndex;

        public override TextObject GetDescriptionText(WeakGameEntity gameEntity)
        {
            if (!gameEntity.HasTag(this.AmmoPickUpTag))
            {
                return new TextObject("{=NbpcDXtJ}Mangonel", null);
            }
            return new TextObject("{=pzfbPbWW}Boulder", null);
        }

        // Token: 0x06002CD5 RID: 11477 RVA: 0x000B0E1C File Offset: 0x000AF01C
        public override TextObject GetActionTextForStandingPoint(UsableMissionObject usableGameObject)
        {
            TextObject textObject;
            if (usableGameObject.GameEntity.HasTag("reload"))
            {
                textObject = new TextObject((base.PilotStandingPoint == usableGameObject) ? "{=fEQAPJ2e}{KEY} Use" : "{=Na81xuXn}{KEY} Rearm", null);
            }
            else if (usableGameObject.GameEntity.HasTag("rotate"))
            {
                textObject = new TextObject("{=5wx4BF5h}{KEY} Rotate", null);
            }
            else if (usableGameObject.GameEntity.HasTag(this.AmmoPickUpTag))
            {
                textObject = new TextObject("{=bNYm3K6b}{KEY} Pick Up", null);
            }
            else if (usableGameObject.GameEntity.HasTag("ammoload"))
            {
                textObject = new TextObject("{=ibC4xPoo}{KEY} Load Ammo", null);
            }
            else
            {
                textObject = new TextObject("{=fEQAPJ2e}{KEY} Use", null);
            }
            textObject.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
            return textObject;
        }
        public override UsableMachineAIBase CreateAIBehaviorObject()
        {
            return new PE_MangonelAI(this);
        }

        protected override void OnInit()
        {
            AmmoPickUpTag = null;

            base.OnInit();
            this.InitiateMoveSynch();
            this.HitPoint = MaxHitPoint;
            LoadAmmoStandingPoint.InitRequiredWeaponClasses(new WeaponClass[] { OriginalMissileItem.PrimaryWeapon.WeaponClass });
            LoadAmmoStandingPoint.InitRequiredWeapon(null);
            LoadAmmoStandingPoint.InitGivenWeapon(null);

            EnemyRangeToStopUsing = 7f;
            moverStandingPoint = GameEntity.GetFirstChildEntityWithTag(MoverStandingPointTag).GetFirstScriptOfType<StandingPoint>();

            _loadAmmoEndAnimationActionIndex = ActionIndexCache.Create(LoadAmmoEndActionName);
            _loadAmmoBeginAnimationActionIndex = ActionIndexCache.Create(this.LoadAmmoBeginActionName);
            //if (base.AmmoPickUpPoints != null)
            //{
            //    foreach (StandingPoint ammoPickUpPoint in base.AmmoPickUpPoints)
            //    {
            //        ammoPickUpPoint.LockUserFrames = true;
            //    }
            //}

            //UpdateProjectilePosition();
        }

        public Agent GetPilotAgent()
        {
            StandingPoint pilotStandingPoint = this.moverStandingPoint;
            if (pilotStandingPoint == null)
            {
                return null;
            }
            return pilotStandingPoint.UserAgent;
        }

        protected void MoveControl()
        {
            if (GameNetwork.IsServer)
            {
                if (this.GetPilotAgent() != null)
                {
                    if (this.GetPilotAgent().Position.Distance(base.GameEntity.GlobalPosition) > 5f)
                    {
                        this.GetPilotAgent().StopUsingGameObjectMT(false);
                    }
                }
            }

            if (GameNetwork.IsClient)
            {
                if (Agent.Main != null && this.GetPilotAgent() == Agent.Main)
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
                        ActionIndexCache ac = ActionIndexCache.act_none;
                        this.GetPilotAgent().SetActionChannel(0, ac, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, 0, false, -0.2f, 0, true);
                    }
                }
            }
            if (this.GetPilotAgent() == null)
            {
                if (this.IsMovingBackward) this.StopMovingBackward();
                if (this.IsMovingDown) this.StopMovingDown();
                if (this.IsMovingForward) this.StopMovingForward();
                if (this.IsMovingUp) this.StopMovingUp();
                if (this.IsTurningLeft) this.StopTurningLeft();
                if (this.IsTurningRight) this.StopTurningRight();
            }
        }

        protected override bool CanRotate()
        {
            if (base.State != 0 && base.State != WeaponState.LoadingAmmo)
            {
                return base.State == WeaponState.WaitingBeforeIdle;
            }

            return true;
        }

        public override TickRequirement GetTickRequirement()
        {
            if (base.GameEntity.IsVisibleIncludeParents())
            {
                return base.GetTickRequirement() | TickRequirement.Tick | TickRequirement.TickParallel;
            }

            return base.GetTickRequirement();
        }

        protected override void OnTick(float dt)
        {
            base.OnTick(dt);
            MoveControl();

            if (GameNetwork.IsServer)
            {
                MatrixFrame frame = this.MoveObjectTick(dt);
                base.SetFrameSynched(ref frame);
            }
            if (!base.GameEntity.IsVisibleIncludeParents())
            {
                return;
            }

#if SERVER
            if (!GameNetwork.IsClientOrReplay)
            {
                foreach (StandingPointWithWeaponRequirement standingPointWithWeaponRequirement in this.AmmoPickUpPoints)
                {
                    if (standingPointWithWeaponRequirement.HasUser)
                    {
                        Agent userAgent = standingPointWithWeaponRequirement.UserAgent;
                        ActionIndexCache currentAction = userAgent.GetCurrentAction(1);
                        if (!(currentAction == ActionIndexCache.act_pickup_boulder_begin))
                        {
                            if (currentAction == ActionIndexCache.act_pickup_boulder_end)
                            {
                                MissionWeapon missionWeapon = new MissionWeapon(this.OriginalMissileItem, null, null, 1);
                                userAgent.EquipWeaponToExtraSlotAndWield(ref missionWeapon);
                                userAgent.StopUsingGameObject(true);
                                this.ConsumeAmmo();
                                if (userAgent.IsAIControlled)
                                {
                                    return;
                                }
                            }
                            else if (!userAgent.SetActionChannel(1, ActionIndexCache.act_pickup_boulder_begin, false, 0UL, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true) && userAgent.Controller != AgentControllerType.AI)
                            {
                                userAgent.StopUsingGameObject(true);
                            }
                        }
                    }
                }
            }
#endif
            switch (base.State)
            {
                case RangedSiegeWeapon.WeaponState.LoadingAmmo:
                    if (!GameNetwork.IsClientOrReplay)
                    {
                        if (this.LoadAmmoStandingPoint.HasUser)
                        {
                            Agent userAgent2 = this.LoadAmmoStandingPoint.UserAgent;
                            if (userAgent2.GetCurrentAction(1) == this._loadAmmoEndAnimationActionIndex)
                            {
                                EquipmentIndex wieldedItemIndex = userAgent2.GetPrimaryWieldedItemIndex();// GetWieldedItemIndex(Agent.HandIndex.MainHand);
                                Debug.Print(wieldedItemIndex.ToString());
                                if (wieldedItemIndex != EquipmentIndex.None && userAgent2.Equipment[wieldedItemIndex].CurrentUsageItem.WeaponClass == this.OriginalMissileItem.PrimaryWeapon.WeaponClass)
                                {
                                    base.ChangeProjectileEntityServer(userAgent2, userAgent2.Equipment[wieldedItemIndex].Item.StringId);
                                    userAgent2.RemoveEquippedWeapon(wieldedItemIndex);
                                    this._timeElapsedAfterLoading = 0f;
                                    base.Projectile.SetVisibleSynched(true, false);
                                    base.State = RangedSiegeWeapon.WeaponState.WaitingBeforeIdle;
                                    return;
                                }
                                userAgent2.StopUsingGameObject(true);
                                if (!userAgent2.IsPlayerControlled)
                                {
                                    base.SendAgentToAmmoPickup(userAgent2);
                                    return;
                                }
                            }
                            else if (userAgent2.GetCurrentAction(1) != this._loadAmmoBeginAnimationActionIndex && !userAgent2.SetActionChannel(1, this._loadAmmoBeginAnimationActionIndex, false, 0UL, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true))
                            {
                                for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.NumAllWeaponSlots; equipmentIndex++)
                                {
                                    if (!userAgent2.Equipment[equipmentIndex].IsEmpty && userAgent2.Equipment[equipmentIndex].CurrentUsageItem.WeaponClass == this.OriginalMissileItem.PrimaryWeapon.WeaponClass)
                                    {
                                        userAgent2.RemoveEquippedWeapon(equipmentIndex);
                                    }
                                }
                                userAgent2.StopUsingGameObject(true);
                                if (!userAgent2.IsPlayerControlled)
                                {
                                    base.SendAgentToAmmoPickup(userAgent2);
                                    return;
                                }
                            }
                        }
                        else if (this.LoadAmmoStandingPoint.HasAIMovingTo)
                        {
                            Agent movingAgent = this.LoadAmmoStandingPoint.MovingAgent;
                            EquipmentIndex wieldedItemIndex2 = movingAgent.GetPrimaryWieldedItemIndex();// WieldedItemIndex(Agent.HandIndex.MainHand);
                            if (wieldedItemIndex2 == EquipmentIndex.None || movingAgent.Equipment[wieldedItemIndex2].CurrentUsageItem.WeaponClass != this.OriginalMissileItem.PrimaryWeapon.WeaponClass)
                            {
                                movingAgent.StopUsingGameObject(true);
                                base.SendAgentToAmmoPickup(movingAgent);
                            }
                        }
                    }
                    break;
                case RangedSiegeWeapon.WeaponState.WaitingBeforeIdle:
                    this._timeElapsedAfterLoading += dt;
                    if (this._timeElapsedAfterLoading > 1f)
                    {
                        base.State = RangedSiegeWeapon.WeaponState.Idle;
                        return;
                    }
                    break;
                case RangedSiegeWeapon.WeaponState.Reloading:
                case RangedSiegeWeapon.WeaponState.ReloadingPaused:
                    break;
                default:
                    return;
            }
        }

        protected override bool OnHit(Agent attackerAgent, int damage, Vec3 impactPosition, Vec3 impactDirection, in MissionWeapon weapon, int affectorWeaponSlotOrMissileIndex, ScriptComponentBehavior attackerScriptComponentBehavior, out bool reportDamage, out float finalDamage)
        {
            reportDamage = true;
            MissionWeapon missionWeapon = weapon;
            WeaponComponentData currentUsageItem = missionWeapon.CurrentUsageItem;

            if (this.DestroyedByStoneOnly)
            {
                if (currentUsageItem == null || (currentUsageItem.WeaponClass != WeaponClass.Stone && currentUsageItem.WeaponClass != WeaponClass.Boulder) || !currentUsageItem.WeaponFlags.HasAnyFlag(WeaponFlags.NotUsableWithOneHand))
                {
                    damage = 0;
                }
            }
            if (impactDirection == null) impactDirection = Vec3.Zero;


            SetHitPoint(this.HitPoint - damage, impactDirection);
            if (GameNetwork.IsServer)
            {
                LoggerHelper.LogAnAction(attackerAgent.MissionPeer.GetNetworkPeer(), LogAction.PlayerHitToDestructable, null, new object[] { this.GetType().Name });
            }

            finalDamage = damage;

            return false;
        }

        protected override void SetActivationLoadAmmoPoint(bool activate)
        {
            //LoadAmmoStandingPoint.SetIsDeactivatedSynched(!activate);
        }

        //protected override void OnRangedSiegeWeaponStateChange()
        //{
        //    base.OnRangedSiegeWeaponStateChange();
        //    RangedSiegeWeapon.WeaponState state = base.State;
        //    if (state != RangedSiegeWeapon.WeaponState.Idle)
        //    {
        //        if (state != RangedSiegeWeapon.WeaponState.Shooting)
        //        {
        //            if (state == RangedSiegeWeapon.WeaponState.WaitingBeforeIdle)
        //            {
        //                this.UpdateProjectilePosition();
        //                return;
        //            }
        //        }
        //        else
        //        {
        //            if (!GameNetwork.IsClientOrReplay)
        //            {
        //                base.Projectile.SetVisibleSynched(false, false);
        //                return;
        //            }
        //            base.Projectile.GameEntity.SetVisibilityExcludeParents(false);
        //            return;
        //        }
        //    }
        //    else
        //    {
        //        if (!GameNetwork.IsClientOrReplay)
        //        {
        //            base.Projectile.SetVisibleSynched(true, false);
        //            return;
        //        }
        //        base.Projectile.GameEntity.SetVisibilityExcludeParents(true);
        //    }
        //}

        protected override void GetSoundEventIndices()
        {
            MoveSoundIndex = SoundEvent.GetEventIdFromString("event:/mission/siege/mangonel/move");
            ReloadSoundIndex = SoundEvent.GetEventIdFromString("event:/mission/siege/mangonel/reload");
            FireSoundIndex = SoundEvent.GetEventIdFromString("event:/mission/siege/mangonel/fire");
        }

        //public override TargetFlags GetTargetFlags()
        //{
        //    TargetFlags targetFlags = TargetFlags.None;
        //    targetFlags |= TargetFlags.IsFlammable;
        //    targetFlags |= TargetFlags.IsSiegeEngine;
        //    targetFlags |= TargetFlags.IsAttacker;
        //    if (base.IsDestroyed || this.IsDeactivated)
        //    {
        //        targetFlags |= TargetFlags.NotAThreat;
        //    }
        //    if (this.Side == BattleSideEnum.Attacker && DebugSiegeBehavior.DebugDefendState == DebugSiegeBehavior.DebugStateDefender.DebugDefendersToMangonels)
        //    {
        //        targetFlags |= TargetFlags.DebugThreat;
        //    }
        //    if (this.Side == BattleSideEnum.Defender && DebugSiegeBehavior.DebugAttackState == DebugSiegeBehavior.DebugStateAttacker.DebugAttackersToMangonels)
        //    {
        //        targetFlags |= TargetFlags.DebugThreat;
        //    }
        //    return targetFlags;
        //}

        //public override float ProcessTargetValue(float baseValue, TargetFlags flags)
        //{
        //    if (flags.HasAnyFlag(TargetFlags.NotAThreat))
        //    {
        //        return -1000f;
        //    }
        //    if (flags.HasAnyFlag(TargetFlags.IsSiegeEngine))
        //    {
        //        baseValue *= 1.5f;
        //    }
        //    if (flags.HasAnyFlag(TargetFlags.IsStructure))
        //    {
        //        baseValue *= 2.5f;
        //    }
        //    if (flags.HasAnyFlag(TargetFlags.IsSmall))
        //    {
        //        baseValue *= 0.5f;
        //    }
        //    if (flags.HasAnyFlag(TargetFlags.IsMoving))
        //    {
        //        baseValue *= 0.8f;
        //    }
        //    if (flags.HasAnyFlag(TargetFlags.DebugThreat))
        //    {
        //        baseValue *= 10000f;
        //    }
        //    return baseValue;
        //}

        public void SetHitPoint(float hitPoint, Vec3 impactDirection)
        {
            this.HitPoint = hitPoint;

            MatrixFrame globalFrame = base.GameEntity.GetGlobalFrame();
            if (this.HitPoint > this.MaxHitPoint) this.HitPoint = this.MaxHitPoint;
            if (this.HitPoint < 0) this.HitPoint = 0;

            if (this.HitPoint == 0)
            {
                for (int i = 0; i < base.StandingPoints.Count; i++)
                {
                    if (this.StandingPoints[i].HasUser)
                    {
                        this.StandingPoints[i].UserAgent.StopUsingGameObjectMT(false);
                    }
                }
                if (this.ParticleEffectOnDestroy != "")
                {
                    Mission.Current.Scene.CreateBurstParticle(ParticleSystemManager.GetRuntimeIdByName(this.ParticleEffectOnDestroy), globalFrame);
                }
                if (this.SoundEffectOnDestroy != "")
                {
                    Mission.Current.MakeSound(SoundEvent.GetEventIdFromString(this.SoundEffectOnDestroy), globalFrame.origin, false, true, -1, -1);
                }

                base.GameEntity.Remove(0);
            }
        }

        public void SetSpawnedFromSpawner()
        {
            this._spawnedFromSpawner = true;
        }

        public void OnSpawnedByPrefab(PE_PrefabSpawner spawner)
        {
            this._spawnedFromSpawner = true;
        }

        public UsableMachine GetAttachedObject()
        {
            return this;
        }

        //public void SetFrameAfterTick(MatrixFrame frame)
        //{
        //    this._setFrameAfterTick = frame;
        //    this._frameSetFlag = true;
        //}

        public float GetAdvanceSpeed()
        {
            return 1f;
        }

        public float GetRotationSpeed()
        {
            return 0.3f;
        }

        public float GetElevationSpeed()
        {
            return 1f;
        }

        public bool GetCanAdvance()
        {
            return true;
        }

        public bool GetCanRotate()
        {
            return true;
        }

        public bool GetCanElevate()
        {
            return false;
        }

        public bool GetAlwaysAlignToTerritory()
        {
            return true;
        }
    }
}