using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.SceneScripts.Extensions;
using PersistentEmpiresLib.SceneScripts.Interfaces;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
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
    public class PE_Mangonel : RangedSiegeWeapon, ISpawnable, IMoveable
    {
        // Token: 0x170007FE RID: 2046
        // (get) Token: 0x06002CBD RID: 11453 RVA: 0x000AFC6E File Offset: 0x000ADE6E
        protected override float MaximumBallisticError
        {
            get
            {
                return 1.5f;
            }
        }

        // Token: 0x170007FF RID: 2047
        // (get) Token: 0x06002CBE RID: 11454 RVA: 0x000AFC75 File Offset: 0x000ADE75
        protected override float ShootingSpeed
        {
            get
            {
                return this.ProjectileSpeed;
            }
        }

        // Token: 0x06002CBF RID: 11455 RVA: 0x000AFC80 File Offset: 0x000ADE80
        protected override void RegisterAnimationParameters()
        {
            this.SkeletonOwnerObjects = new SynchedMissionObject[2];
            this.Skeletons = new Skeleton[2];
            this.SkeletonNames = new string[1];
            this.FireAnimations = new string[2];
            this.FireAnimationIndices = new int[2];
            this.SetUpAnimations = new string[2];
            this.SetUpAnimationIndices = new int[2];
            this.SkeletonOwnerObjects[0] = this._body;
            this.Skeletons[0] = this._body.GameEntity.Skeleton;
            this.SkeletonNames[0] = this.MangonelBodySkeleton;
            this.FireAnimations[0] = this.MangonelBodyFire;
            this.FireAnimationIndices[0] = MBAnimation.GetAnimationIndexWithName(this.MangonelBodyFire);
            this.SetUpAnimations[0] = this.MangonelBodyReload;
            this.SetUpAnimationIndices[0] = MBAnimation.GetAnimationIndexWithName(this.MangonelBodyReload);
            this.SkeletonOwnerObjects[1] = this._rope;
            this.Skeletons[1] = this._rope.GameEntity.Skeleton;
            this.FireAnimations[1] = this.MangonelRopeFire;
            this.FireAnimationIndices[1] = MBAnimation.GetAnimationIndexWithName(this.MangonelRopeFire);
            this.SetUpAnimations[1] = this.MangonelRopeReload;
            this.SetUpAnimationIndices[1] = MBAnimation.GetAnimationIndexWithName(this.MangonelRopeReload);
            this._missileBoneName = this.ProjectileBoneName;
            this._idleAnimationActionIndex = ActionIndexCache.Create(this.IdleActionName);
            this._shootAnimationActionIndex = ActionIndexCache.Create(this.ShootActionName);
            this._reload1AnimationActionIndex = ActionIndexCache.Create(this.Reload1ActionName);
            this._reload2AnimationActionIndex = ActionIndexCache.Create(this.Reload2ActionName);
            this._rotateLeftAnimationActionIndex = ActionIndexCache.Create(this.RotateLeftActionName);
            this._rotateRightAnimationActionIndex = ActionIndexCache.Create(this.RotateRightActionName);
            this._loadAmmoBeginAnimationActionIndex = ActionIndexCache.Create(this.LoadAmmoBeginActionName);
            this._loadAmmoEndAnimationActionIndex = ActionIndexCache.Create(this.LoadAmmoEndActionName);
            this._reload2IdleActionIndex = ActionIndexCache.Create(this.Reload2IdleActionName);
            //this.SkeletonOwnerObjects = new SynchedMissionObject[2];
            //this.Skeletons = new Skeleton[2];
            //this.SkeletonNames = new string[1];
            //this.FireAnimations = new string[2];
            //this.FireAnimationIndices = new int[2];
            //this.SetUpAnimations = new string[2];
            //this.SetUpAnimationIndices = new int[2];
            //this.SkeletonOwnerObjects[0] = this._body;
            //this.Skeletons[0] = this._body.GameEntity.Skeleton;
            //this.SkeletonNames[0] = this.MangonelBodySkeleton;
            //this.FireAnimations[0] = this.MangonelBodyFire;
            //this.FireAnimationIndices[0] = MBAnimation.GetAnimationIndexWithName(this.MangonelBodyFire);
            //this.SetUpAnimations[0] = this.MangonelBodyReload;
            //this.SetUpAnimationIndices[0] = MBAnimation.GetAnimationIndexWithName(this.MangonelBodyReload);
            //this.SkeletonOwnerObjects[1] = this._rope;
            //this.Skeletons[1] = this._rope.GameEntity.Skeleton;
            //this.FireAnimations[1] = this.MangonelRopeFire;
            //this.FireAnimationIndices[1] = MBAnimation.GetAnimationIndexWithName(this.MangonelRopeFire);
            //this.SetUpAnimations[1] = this.MangonelRopeReload;
            //this.SetUpAnimationIndices[1] = MBAnimation.GetAnimationIndexWithName(this.MangonelRopeReload);
            //this._missileBoneName = this.ProjectileBoneName;
            //this._idleAnimationActionIndex = ActionIndexCache.Create(this.IdleActionName);
            //this._shootAnimationActionIndex = ActionIndexCache.Create(this.ShootActionName);
            //this._reload1AnimationActionIndex = ActionIndexCache.Create(this.Reload1ActionName);
            //this._reload2AnimationActionIndex = ActionIndexCache.Create(this.Reload2ActionName);
            //this._rotateLeftAnimationActionIndex = ActionIndexCache.Create(this.RotateLeftActionName);
            //this._rotateRightAnimationActionIndex = ActionIndexCache.Create(this.RotateRightActionName);
            //this._loadAmmoBeginAnimationActionIndex = ActionIndexCache.Create(this.LoadAmmoBeginActionName);
            //this._loadAmmoEndAnimationActionIndex = ActionIndexCache.Create(this.LoadAmmoEndActionName);
            //this._reload2IdleActionIndex = ActionIndexCache.Create(this.Reload2IdleActionName);
        }

        // Token: 0x06002CC0 RID: 11456 RVA: 0x000AFE28 File Offset: 0x000AE028
        public override UsableMachineAIBase CreateAIBehaviorObject()
        {
            return new PE_MangonelAI(this);
        }

        // Token: 0x06002CC2 RID: 11458 RVA: 0x000AFE90 File Offset: 0x000AE090
        public override SiegeEngineType GetSiegeEngineType()
        {
            if (DefaultSide != BattleSideEnum.Attacker)
            {
                return DefaultSiegeEngineTypes.Catapult;
            }
            return DefaultSiegeEngineTypes.Onager;
        }

        // Token: 0x06002CC3 RID: 11459 RVA: 0x000AFEA8 File Offset: 0x000AE0A8
        private bool initCompleted = false;
        protected override void OnInit()
        {
            initCompleted = false;            
            var synchedMissionObjectList = GameEntity.CollectScriptComponentsWithTagIncludingChildrenRecursive<SynchedMissionObject>("rope");
            //AmmoPickUpTag = null;

            if (synchedMissionObjectList.Count > 0)
            {
                _rope = synchedMissionObjectList[0];
            }
            synchedMissionObjectList = GameEntity.CollectScriptComponentsWithTagIncludingChildrenRecursive<SynchedMissionObject>("body");
            _body = synchedMissionObjectList.Count > 0 ? synchedMissionObjectList[0] : this;
            RotationObject = _body;
            
            var list2 = GameEntity.CollectChildrenEntitiesWithTag("vertical_adjuster");
            _verticalAdjuster = list2[0];
            
            if (_verticalAdjuster.Skeleton != null)
            {
                _verticalAdjusterSkeleton = _verticalAdjuster.Skeleton;

                if ((NativeObject)this._verticalAdjusterSkeleton != (NativeObject)null)
                    this._verticalAdjusterSkeleton.SetAnimationAtChannel(this.MangonelAimAnimation, 0, 1f, -1f, 0.0f);
            }
            _verticalAdjusterStartingLocalFrame = _verticalAdjuster.GetFrame();
            _verticalAdjusterStartingLocalFrame = _body.GameEntity.GetBoneEntitialFrameWithIndex(0).TransformToLocal(_verticalAdjusterStartingLocalFrame);
            //RegisterAnimationParameters();
            this.InitiateMoveSynch();
            base.OnInit();
            TimeGapBetweenShootActionAndProjectileLeaving = 0.23f;
            TimeGapBetweenShootingEndAndReloadingStart = 0f;
            this._rotateStandingPoints = new List<StandingPoint>();
            if (this.StandingPoints != null)
            {
                foreach (StandingPoint standingPoint in (List<StandingPoint>)this.StandingPoints)
                {
                    if (standingPoint.GameEntity.HasTag("rotate"))
                    {
                        if (standingPoint.GameEntity.HasTag("left") && this._rotateStandingPoints.Count > 0)
                            this._rotateStandingPoints.Insert(0, standingPoint);
                        else
                            this._rotateStandingPoints.Add(standingPoint);
                    }
                }
                WeakGameEntity gameEntity = this._body.GameEntity;
                MatrixFrame frame = gameEntity.GetGlobalFrame();
                this._standingPointLocalIKFrames = new MatrixFrame[this.StandingPoints.Count];
                for (int index1 = 0; index1 < this.StandingPoints.Count; ++index1)
                {
                    MatrixFrame[] pointLocalIkFrames = this._standingPointLocalIKFrames;
                    int index2 = index1;
                    gameEntity = this.StandingPoints[index1].GameEntity;
                    MatrixFrame localNonOrthogonal = gameEntity.GetGlobalFrame().TransformToLocalNonOrthogonal(in frame);
                    pointLocalIkFrames[index2] = localNonOrthogonal;
                    this.StandingPoints[index1].AddComponent((UsableMissionObjectComponent)new ClearHandInverseKinematicsOnStopUsageComponent());
                }
            }
            _missileBoneIndex = Skeleton.GetBoneIndexFromName(SkeletonOwnerObjects[0].GameEntity.Skeleton.GetName(), _missileBoneName);
            ApplyAimChange();

            foreach (StandingPoint reloadStandingPoint in this.ReloadStandingPoints)
            {
                if (reloadStandingPoint != this.PilotStandingPoint)
                    this._reloadWithoutPilot = reloadStandingPoint;
            }
            if (!GameNetwork.IsClientOrReplay)
                this.SetActivationLoadAmmoPoint(false);
            this.EnemyRangeToStopUsing = 7f;
            this.SetScriptComponentToTick(this.GetTickRequirement());
            if (this.AmmoPickUpPoints != null)
            {
                foreach (UsableMissionObject ammoPickUpPoint in this.AmmoPickUpPoints)
                    ammoPickUpPoint.LockUserFrames = true;
            }
            this.UpdateProjectilePosition();            
            //HitPoint = MaxHitPoint;
            /*
            var list = GameEntity.CollectScriptComponentsWithTagIncludingChildrenRecursive<SynchedMissionObject>("rope");
            
            AmmoPickUpTag = null;
            
            if (list.Count > 0)
            {
                _rope = list[0];
            }
            
            list = GameEntity.CollectScriptComponentsWithTagIncludingChildrenRecursive<SynchedMissionObject>("body");
            _body = list.Count > 0 ? list[0] : this;
            RotationObject = _body;
            
            var list2 = GameEntity.CollectChildrenEntitiesWithTag("vertical_adjuster");

            _verticalAdjuster = list2[0];
            
            if (_verticalAdjuster.Skeleton != null)
            {
                _verticalAdjuster.Skeleton.SetAnimationAtChannel(MangonelAimAnimation, 0, 1f, -1f, 0f);
            }
            _verticalAdjusterStartingLocalFrame = _verticalAdjuster.GetFrame();
            _verticalAdjusterStartingLocalFrame = _body.GameEntity.GetBoneEntitialFrameWithIndex(0).TransformToLocal(_verticalAdjusterStartingLocalFrame);
            base.OnInit();
            this.InitiateMoveSynch();
            HitPoint = MaxHitPoint;
            LoadAmmoStandingPoint.InitRequiredWeaponClasses(new WeaponClass[] { OriginalMissileItem.PrimaryWeapon.WeaponClass });
            LoadAmmoStandingPoint.InitRequiredWeapon(null);
            LoadAmmoStandingPoint.InitGivenWeapon(null);
            TimeGapBetweenShootActionAndProjectileLeaving = 0.23f;
            TimeGapBetweenShootingEndAndReloadingStart = 0f;
            _rotateStandingPoints = new List<StandingPoint>();
            
            if (StandingPoints != null)
            {
                foreach (StandingPoint standingPoint in base.StandingPoints)
                {
                    if (standingPoint.GameEntity.HasTag("rotate"))
                    {
                        if (standingPoint.GameEntity.HasTag("left") && _rotateStandingPoints.Count > 0)
                        {
                            _rotateStandingPoints.Insert(0, standingPoint);
                        }
                        else
                        {
                            _rotateStandingPoints.Add(standingPoint);
                        }
                    }
                }
                var globalFrame = _body.GameEntity.GetGlobalFrame();

                _standingPointLocalIKFrames = new MatrixFrame[StandingPoints.Count];
                
                for (int i = 0; i < StandingPoints.Count; i++)
                {
                    _standingPointLocalIKFrames[i] = StandingPoints[i].GameEntity.GetGlobalFrame().TransformToLocal(globalFrame);
                    StandingPoints[i].AddComponent(new ClearHandInverseKinematicsOnStopUsageComponent());
                }
            }

            _missileBoneIndex = Skeleton.GetBoneIndexFromName(SkeletonOwnerObjects[0].GameEntity.Skeleton.GetName(), _missileBoneName);
            ApplyAimChange();

            foreach (var standingPoint2 in ReloadStandingPoints)
            {
                if (standingPoint2 != PilotStandingPoint)
                {
                    _reloadWithoutPilot = standingPoint2;
                }
            }
            if (!GameNetwork.IsClientOrReplay)
            {
                // this.SetActivationLoadAmmoPoint(false);
            }

            EnemyRangeToStopUsing = 7f;
            moverStandingPoint = GameEntity.GetFirstChildEntityWithTag(MoverStandingPointTag).GetFirstScriptOfType<StandingPoint>();
            SetScriptComponentToTick(GetTickRequirement());
            */
            initCompleted = true;
        }
        // Token: 0x06002CC1 RID: 11457 RVA: 0x000AFE30 File Offset: 0x000AE030
        //     public override void AfterMissionStart()
        //     {
        //         /*if (this.AmmoPickUpStandingPoints != null)
        //{
        //	foreach (StandingPointWithWeaponRequirement standingPointWithWeaponRequirement in this.AmmoPickUpStandingPoints)
        //	{
        //		standingPointWithWeaponRequirement.LockUserFrames = true;
        //	}
        //}*/
        //         this.UpdateProjectilePosition();
        //     }



        protected override void UpdateAmmoMesh()
        {
            /*GameEntity gameEntity = this.AmmoPickUpStandingPoints[0].GameEntity;
			int num = 20 - this.AmmoCount;
			while (gameEntity.Parent != null)
			{
				for (int i = 0; i < gameEntity.MultiMeshComponentCount; i++)
				{
					MetaMesh metaMesh = gameEntity.GetMetaMesh(i);
					for (int j = 0; j < metaMesh.MeshCount; j++)
					{
						metaMesh.GetMeshAtIndex(j).SetVectorArgument(0f, (float)num, 0f, 0f);
					}
				}
				gameEntity = gameEntity.Parent;
			}*/
        }

        public void PreInit()
        {
            
        }
        

        // Token: 0x06002CC4 RID: 11460 RVA: 0x000B0180 File Offset: 0x000AE380
        protected override void OnEditorInit()
        {
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

        // Token: 0x06002CC5 RID: 11461 RVA: 0x000B0182 File Offset: 0x000AE382
        protected override bool CanRotate()
        {
            return base.State == RangedSiegeWeapon.WeaponState.Idle || base.State == RangedSiegeWeapon.WeaponState.LoadingAmmo || base.State == RangedSiegeWeapon.WeaponState.WaitingBeforeIdle;
        }

        // Token: 0x06002CC6 RID: 11462 RVA: 0x000B01A0 File Offset: 0x000AE3A0
        public override ScriptComponentBehavior.TickRequirement GetTickRequirement()
        {
            if (base.GameEntity.IsVisibleIncludeParents())
            {
                return base.GetTickRequirement() | ScriptComponentBehavior.TickRequirement.Tick | ScriptComponentBehavior.TickRequirement.TickParallel;
            }
            return base.GetTickRequirement();
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

        protected override void OnFixedTick(float fixedDt)
        {
            if (!initCompleted)
            {
                return;
            }
            base.OnFixedTick(fixedDt);
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

        protected override void OnParallelFixedTick(float fixedDt)
        {
            if (!initCompleted)
            {
                return;
            }
            base.OnParallelFixedTick(fixedDt);
        }

        // Token: 0x06002CC7 RID: 11463 RVA: 0x000B01C0 File Offset: 0x000AE3C0
        protected override void OnTick(float dt)
        {
            if(!initCompleted)
            {
                return;
            }

            base.OnTick(dt);
            this.MoveControl();

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
                        if (!(currentAction == PE_Mangonel.act_pickup_boulder_begin))
                        {
                            if (currentAction == PE_Mangonel.act_pickup_boulder_end)
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
                            else if (!userAgent.SetActionChannel(1, PE_Mangonel.act_pickup_boulder_begin, false, 0UL, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true) && userAgent.Controller != AgentControllerType.AI)
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

        protected override void ApplyCurrentDirectionToEntity()
        {
            /*MatrixFrame rotationObjectInitialFrame = this.RotationObject.GameEntity.GetFrame();
			rotationObjectInitialFrame.rotation.RotateAboutUp(this.currentDirection);
			this.RotationObject.GameEntity.SetFrame(ref rotationObjectInitialFrame);*/
        }

        // Token: 0x06002CC8 RID: 11464 RVA: 0x000B05F4 File Offset: 0x000AE7F4
        protected override void OnTickParallel(float dt)
        {
            if (!initCompleted)
            {
                return;
            }

            base.OnTickParallel(dt);
            if (!GameEntity.IsVisibleIncludeParents())
            {
                return;
            }
            if (State == RangedSiegeWeapon.WeaponState.WaitingBeforeProjectileLeaving)
            {
                UpdateProjectilePosition();
            }
            if (_verticalAdjuster.Skeleton != null)
            {
                float parameter = MBMath.ClampFloat((CurrentReleaseAngle - BottomReleaseAngleRestriction) / (TopReleaseAngleRestriction - BottomReleaseAngleRestriction), 0f, 1f);
                _verticalAdjuster.Skeleton.SetAnimationParameterAtChannel(0, parameter);
            }
            var matrixFrame = SkeletonOwnerObjects[0].GameEntity.GetBoneEntitialFrameWithIndex(0).TransformToParent(_verticalAdjusterStartingLocalFrame);
            _verticalAdjuster.SetFrame(ref matrixFrame);
            var globalFrame = _body.GameEntity.GetGlobalFrame();
            for (int i = 0; i < StandingPoints.Count; i++)
            {
                if (StandingPoints[i].HasUser)
                {
                    if (StandingPoints[i].UserAgent.IsInBeingStruckAction)
                    {
                        StandingPoints[i].UserAgent.ClearHandInverseKinematics();
                    }
                    else if (StandingPoints[i] != PilotStandingPoint)
                    {
                        if (StandingPoints[i].UserAgent.GetCurrentAction(1) != _reload2IdleActionIndex)
                        {
                            StandingPoints[i].UserAgent.SetHandInverseKinematicsFrameForMissionObjectUsage(_standingPointLocalIKFrames[i], globalFrame, 0f);
                        }
                        else
                        {
                            StandingPoints[i].UserAgent.ClearHandInverseKinematics();
                        }
                    }
                    else
                    {
                        StandingPoints[i].UserAgent.SetHandInverseKinematicsFrameForMissionObjectUsage(_standingPointLocalIKFrames[i], globalFrame, 0f);
                    }
                }
            }
            if (!GameNetwork.IsClientOrReplay)
            {
                for (int j = 0; j < _rotateStandingPoints.Count; j++)
                {
                    StandingPoint standingPoint = _rotateStandingPoints[j];
                    if (standingPoint.HasUser && !standingPoint.UserAgent.SetActionChannel(1, (j == 0) ? _rotateLeftAnimationActionIndex : _rotateRightAnimationActionIndex, false, 0UL, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true) && standingPoint.UserAgent.Controller != AgentControllerType.AI)
                    {
                        standingPoint.UserAgent.StopUsingGameObjectMT(true);
                    }
                }
                if (base.PilotAgent != null)
                {
                    ActionIndexCache currentAction = base.PilotAgent.GetCurrentAction(1);
                    if (base.State == RangedSiegeWeapon.WeaponState.WaitingBeforeProjectileLeaving)
                    {
                        if (base.PilotAgent.IsInBeingStruckAction)
                        {
                            if (currentAction != ActionIndexCache.act_none && currentAction != PE_Mangonel.act_strike_bent_over)
                            {
                                base.PilotAgent.SetActionChannel(1, PE_Mangonel.act_strike_bent_over, false, 0UL, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
                            }
                        }
                        else if (!base.PilotAgent.SetActionChannel(1, this._shootAnimationActionIndex, false, 0UL, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true) && base.PilotAgent.Controller != AgentControllerType.AI)
                        {
                            base.PilotAgent.StopUsingGameObjectMT(true);
                        }
                    }
                    else if (!base.PilotAgent.SetActionChannel(1, this._idleAnimationActionIndex, false, 0UL, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true) && currentAction != this._reload1AnimationActionIndex && currentAction != this._shootAnimationActionIndex && base.PilotAgent.Controller != AgentControllerType.AI)
                    {
                        base.PilotAgent.StopUsingGameObjectMT(true);
                    }
                }
                if (this._reloadWithoutPilot.HasUser)
                {
                    Agent userAgent = this._reloadWithoutPilot.UserAgent;
                    if (!userAgent.SetActionChannel(1, this._reload2IdleActionIndex, false, 0UL, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true) && userAgent.GetCurrentAction(1) != this._reload2AnimationActionIndex && userAgent.Controller != AgentControllerType.AI)
                    {
                        userAgent.StopUsingGameObjectMT(true);
                    }
                }
            }
            if (base.State == RangedSiegeWeapon.WeaponState.Reloading)
            {
                foreach (StandingPoint standingPoint2 in this.ReloadStandingPoints)
                {
                    if (standingPoint2.HasUser)
                    {
                        ActionIndexCache currentAction2 = standingPoint2.UserAgent.GetCurrentAction(1);
                        if (currentAction2 == this._reload1AnimationActionIndex || currentAction2 == this._reload2AnimationActionIndex)
                        {
                            standingPoint2.UserAgent.SetCurrentActionProgress(1, this._body.GameEntity.Skeleton.GetAnimationParameterAtChannel(0));
                        }
                        else if (!GameNetwork.IsClientOrReplay)
                        {
                            ActionIndexCache actionIndexCache = (standingPoint2 == base.PilotStandingPoint) ? this._reload1AnimationActionIndex : this._reload2AnimationActionIndex;
                            if (!standingPoint2.UserAgent.SetActionChannel(1, actionIndexCache, false, 0UL, 0f, 1f, -0.2f, 0.4f, this._body.GameEntity.Skeleton.GetAnimationParameterAtChannel(0), false, -0.2f, 0, true) && standingPoint2.UserAgent.Controller != AgentControllerType.AI)
                            {
                                standingPoint2.UserAgent.StopUsingGameObjectMT(true);
                            }
                        }
                    }
                }
            }
        }

        // Token: 0x06002CC9 RID: 11465 RVA: 0x000B0B8C File Offset: 0x000AED8C
        protected override void SetActivationLoadAmmoPoint(bool activate)
        {
            this.LoadAmmoStandingPoint.SetIsDeactivatedSynched(!activate);
        }

        // Token: 0x06002CCA RID: 11466 RVA: 0x000B0BA0 File Offset: 0x000AEDA0
        protected override void UpdateProjectilePosition()
        {
            MatrixFrame boneEntitialFrameWithIndex = this.SkeletonOwnerObjects[0].GameEntity.GetBoneEntitialFrameWithIndex(this._missileBoneIndex);
            base.Projectile.GameEntity.SetFrame(ref boneEntitialFrameWithIndex);
        }

        // Token: 0x06002CCB RID: 11467 RVA: 0x000B0BD8 File Offset: 0x000AEDD8
        protected override void OnRangedSiegeWeaponStateChange()
        {
            base.OnRangedSiegeWeaponStateChange();
            RangedSiegeWeapon.WeaponState state = base.State;
            if (state != RangedSiegeWeapon.WeaponState.Idle)
            {
                if (state != RangedSiegeWeapon.WeaponState.Shooting)
                {
                    if (state == RangedSiegeWeapon.WeaponState.WaitingBeforeIdle)
                    {
                        this.UpdateProjectilePosition();
                        return;
                    }
                }
                else
                {
                    if (!GameNetwork.IsClientOrReplay)
                    {
                        base.Projectile.SetVisibleSynched(false, false);
                        return;
                    }
                    base.Projectile.GameEntity.SetVisibilityExcludeParents(false);
                    return;
                }
            }
            else
            {
                if (!GameNetwork.IsClientOrReplay)
                {
                    base.Projectile.SetVisibleSynched(true, false);
                    return;
                }
                base.Projectile.GameEntity.SetVisibilityExcludeParents(true);
            }
        }

        // Token: 0x06002CCC RID: 11468 RVA: 0x000B0C51 File Offset: 0x000AEE51
        protected override void GetSoundEventIndices()
        {
            this.MoveSoundIndex = SoundEvent.GetEventIdFromString("event:/mission/siege/mangonel/move");
            this.ReloadSoundIndex = SoundEvent.GetEventIdFromString("event:/mission/siege/mangonel/reload");
        }

        // Token: 0x17000800 RID: 2048
        // (get) Token: 0x06002CCD RID: 11469 RVA: 0x000B0C74 File Offset: 0x000AEE74
        protected override float HorizontalAimSensitivity
        {
            get
            {
                if (DefaultSide == BattleSideEnum.Defender)
                {
                    return 0.25f;
                }

                return 0.05f + (from rotateStandingPoint in this._rotateStandingPoints
                                where rotateStandingPoint.HasUser && !rotateStandingPoint.UserAgent.IsInBeingStruckAction
                                select rotateStandingPoint).Sum((StandingPoint rotateStandingPoint) => 0.1f);
            }
        }

        // Token: 0x17000801 RID: 2049
        // (get) Token: 0x06002CCE RID: 11470 RVA: 0x000B0CE3 File Offset: 0x000AEEE3
        protected override float VerticalAimSensitivity
        {
            get
            {
                return 0.1f;
            }
        }

        // Token: 0x17000802 RID: 2050
        // (get) Token: 0x06002CCF RID: 11471 RVA: 0x000B0CEC File Offset: 0x000AEEEC
        protected override Vec3 ShootingDirection
        {
            get
            {
                Mat3 rotation = this._body.GameEntity.GetGlobalFrame().rotation;
                rotation.RotateAboutSide(-CurrentReleaseAngle);
                return rotation.TransformToParent(new Vec3(0f, -1f, 0f, -1f));
            }
        }

        // Token: 0x17000803 RID: 2051
        // (get) Token: 0x06002CD0 RID: 11472 RVA: 0x000B0D40 File Offset: 0x000AEF40
        /*protected override Vec3 VisualizationShootingDirection
        {
            get
            {
                Mat3 rotation = this._body.GameEntity.GetGlobalFrame().rotation;
                rotation.RotateAboutSide(-this.VisualizeReleaseTrajectoryAngle);
                return rotation.TransformToParent(new Vec3(0f, -1f, 0f, -1f));
            }
        }*/

        // Token: 0x17000804 RID: 2052
        // (get) Token: 0x06002CD1 RID: 11473 RVA: 0x000B0D91 File Offset: 0x000AEF91
        // (set) Token: 0x06002CD2 RID: 11474 RVA: 0x000B0DBD File Offset: 0x000AEFBD
        protected override bool HasAmmo
        {
            get
            {
                return base.HasAmmo || base.CurrentlyUsedAmmoPickUpPoint != null || this.LoadAmmoStandingPoint.HasUser || this.LoadAmmoStandingPoint.HasAIMovingTo;
            }
            set
            {
                base.HasAmmo = value;
            }
        }

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

        // Token: 0x06002CD3 RID: 11475 RVA: 0x000B0DC8 File Offset: 0x000AEFC8
        protected override void ApplyAimChange()
        {
            base.ApplyAimChange();
            this.ShootingDirection.Normalize();
        }

        // Token: 0x06002CD4 RID: 11476 RVA: 0x000B0DEA File Offset: 0x000AEFEA
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

        // Token: 0x06002CD6 RID: 11478 RVA: 0x000B0EE4 File Offset: 0x000AF0E4
        public override TargetFlags GetTargetFlags()
        {
            TargetFlags targetFlags = TargetFlags.None;
            targetFlags |= TargetFlags.IsFlammable;
            targetFlags |= TargetFlags.IsSiegeEngine;
            targetFlags |= TargetFlags.IsAttacker;
            if (base.IsDestroyed || this.IsDeactivated)
            {
                targetFlags |= TargetFlags.NotAThreat;
            }
            if (this.Side == BattleSideEnum.Attacker && DebugSiegeBehavior.DebugDefendState == DebugSiegeBehavior.DebugStateDefender.DebugDefendersToMangonels)
            {
                targetFlags |= TargetFlags.DebugThreat;
            }
            if (this.Side == BattleSideEnum.Defender && DebugSiegeBehavior.DebugAttackState == DebugSiegeBehavior.DebugStateAttacker.DebugAttackersToMangonels)
            {
                targetFlags |= TargetFlags.DebugThreat;
            }
            return targetFlags;
        }

        // Token: 0x06002CD7 RID: 11479 RVA: 0x000B0F47 File Offset: 0x000AF147
        public override float GetTargetValue(List<Vec3> weaponPos)
        {
            return 40f * base.GetUserMultiplierOfWeapon() * this.GetDistanceMultiplierOfWeapon(weaponPos[0]) * base.GetHitPointMultiplierOfWeapon();
        }

        // Token: 0x06002CD8 RID: 11480 RVA: 0x000B0F6C File Offset: 0x000AF16C
        public override float ProcessTargetValue(float baseValue, TargetFlags flags)
        {
            if (flags.HasAnyFlag(TargetFlags.NotAThreat))
            {
                return -1000f;
            }
            if (flags.HasAnyFlag(TargetFlags.IsSiegeEngine))
            {
                baseValue *= 1.5f;
            }
            if (flags.HasAnyFlag(TargetFlags.IsStructure))
            {
                baseValue *= 2.5f;
            }
            if (flags.HasAnyFlag(TargetFlags.IsSmall))
            {
                baseValue *= 0.5f;
            }
            if (flags.HasAnyFlag(TargetFlags.IsMoving))
            {
                baseValue *= 0.8f;
            }
            if (flags.HasAnyFlag(TargetFlags.DebugThreat))
            {
                baseValue *= 10000f;
            }
            return baseValue;
        }

        // Token: 0x06002CD9 RID: 11481 RVA: 0x000B0FE9 File Offset: 0x000AF1E9
        protected override float GetDetachmentWeightAux(BattleSideEnum side)
        {
            return base.GetDetachmentWeightAuxForExternalAmmoWeapons(side);
        }

        // Token: 0x06002CDA RID: 11482 RVA: 0x000B0FF2 File Offset: 0x000AF1F2
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

        public void SetFrameAfterTick(MatrixFrame frame)
        {
            this._setFrameAfterTick = frame;
            this._frameSetFlag = true;
        }

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


            this.SetHitPoint(this.HitPoint - damage, impactDirection);
            if (GameNetwork.IsServer)
            {
                LoggerHelper.LogAnAction(attackerAgent.MissionPeer.GetNetworkPeer(), LogAction.PlayerHitToDestructable, null, new object[] { this.GetType().Name });
            }
            
            finalDamage = damage;

            return false;
        }

        // Token: 0x040011A6 RID: 4518
        private const string BodyTag = "body";

        // Token: 0x040011A7 RID: 4519
        private const string RopeTag = "rope";

        // Token: 0x040011A8 RID: 4520
        private const string RotateTag = "rotate";

        // Token: 0x040011A9 RID: 4521
        private const string LeftTag = "left";

        // Token: 0x040011AA RID: 4522
        private const string VerticalAdjusterTag = "vertical_adjuster";

        // Token: 0x040011AB RID: 4523
        private static readonly ActionIndexCache act_usage_mangonel_idle = ActionIndexCache.Create("act_usage_mangonel_idle");

        // Token: 0x040011AC RID: 4524
        private static readonly ActionIndexCache act_usage_mangonel_load_ammo_begin = ActionIndexCache.Create("act_usage_mangonel_load_ammo_begin");

        // Token: 0x040011AD RID: 4525
        private static readonly ActionIndexCache act_usage_mangonel_load_ammo_end = ActionIndexCache.Create("act_usage_mangonel_load_ammo_end");

        // Token: 0x040011AE RID: 4526
        private static readonly ActionIndexCache act_pickup_boulder_begin = ActionIndexCache.Create("act_pickup_boulder_begin");

        // Token: 0x040011AF RID: 4527
        private static readonly ActionIndexCache act_pickup_boulder_end = ActionIndexCache.Create("act_pickup_boulder_end");

        // Token: 0x040011B0 RID: 4528
        private static readonly ActionIndexCache act_usage_mangonel_reload = ActionIndexCache.Create("act_usage_mangonel_reload");

        // Token: 0x040011B1 RID: 4529
        private static readonly ActionIndexCache act_usage_mangonel_reload_2 = ActionIndexCache.Create("act_usage_mangonel_reload_2");

        // Token: 0x040011B2 RID: 4530
        private static readonly ActionIndexCache act_usage_mangonel_reload_2_idle = ActionIndexCache.Create("act_usage_mangonel_reload_2_idle");

        // Token: 0x040011B3 RID: 4531
        private static readonly ActionIndexCache act_usage_mangonel_rotate_left = ActionIndexCache.Create("act_usage_mangonel_rotate_left");

        // Token: 0x040011B4 RID: 4532
        private static readonly ActionIndexCache act_usage_mangonel_rotate_right = ActionIndexCache.Create("act_usage_mangonel_rotate_right");

        // Token: 0x040011B5 RID: 4533
        private static readonly ActionIndexCache act_usage_mangonel_shoot = ActionIndexCache.Create("act_usage_mangonel_shoot");

        // Token: 0x040011B6 RID: 4534
        private static readonly ActionIndexCache act_usage_mangonel_big_idle = ActionIndexCache.Create("act_usage_mangonel_big_idle");

        // Token: 0x040011B7 RID: 4535
        private static readonly ActionIndexCache act_usage_mangonel_big_shoot = ActionIndexCache.Create("act_usage_mangonel_big_shoot");

        // Token: 0x040011B8 RID: 4536
        private static readonly ActionIndexCache act_usage_mangonel_big_reload = ActionIndexCache.Create("act_usage_mangonel_big_reload");

        // Token: 0x040011B9 RID: 4537
        private static readonly ActionIndexCache act_usage_mangonel_big_load_ammo_begin = ActionIndexCache.Create("act_usage_mangonel_big_load_ammo_begin");

        // Token: 0x040011BA RID: 4538
        private static readonly ActionIndexCache act_usage_mangonel_big_load_ammo_end = ActionIndexCache.Create("act_usage_mangonel_big_load_ammo_end");

        // Token: 0x040011BB RID: 4539
        private static readonly ActionIndexCache act_strike_bent_over = ActionIndexCache.Create("act_strike_bent_over");

        // Token: 0x040011BC RID: 4540
        private string _missileBoneName = "end_throwarm";

        // Token: 0x040011BD RID: 4541
        private List<StandingPoint> _rotateStandingPoints;

        // Token: 0x040011BE RID: 4542
        private SynchedMissionObject _body;

        // Token: 0x040011BF RID: 4543
        private SynchedMissionObject _rope;

        // Token: 0x040011C0 RID: 4544
        private WeakGameEntity _verticalAdjuster;
        
        private Skeleton _verticalAdjusterSkeleton;

        // Token: 0x040011C1 RID: 4545
        private MatrixFrame _verticalAdjusterStartingLocalFrame;

        // Token: 0x040011C2 RID: 4546
        private float _timeElapsedAfterLoading;

        // Token: 0x040011C3 RID: 4547
        private MatrixFrame[] _standingPointLocalIKFrames;

        // Token: 0x040011C4 RID: 4548
        private StandingPoint _reloadWithoutPilot;
        private StandingPoint moverStandingPoint;
        public string MoverStandingPointTag = "mover";
        // Token: 0x040011C5 RID: 4549
        public string MangonelBodySkeleton = "mangonel_skeleton";

        // Token: 0x040011C6 RID: 4550
        public string MangonelBodyFire = "mangonel_fire";

        // Token: 0x040011C7 RID: 4551
        public string MangonelBodyReload = "mangonel_set_up";

        // Token: 0x040011C8 RID: 4552
        public string MangonelRopeFire = "mangonel_holder_fire";

        // Token: 0x040011C9 RID: 4553
        public string MangonelRopeReload = "mangonel_holder_set_up";

        // Token: 0x040011CA RID: 4554
        public string MangonelAimAnimation = "mangonel_a_anglearm_state";

        // Token: 0x040011CB RID: 4555
        public string ProjectileBoneName = "end_throwarm";

        // Token: 0x040011CC RID: 4556
        public string IdleActionName;

        // Token: 0x040011CD RID: 4557
        public string ShootActionName;

        // Token: 0x040011CE RID: 4558
        public string Reload1ActionName;

        // Token: 0x040011CF RID: 4559
        public string Reload2ActionName;

        // Token: 0x040011D0 RID: 4560
        public string RotateLeftActionName;

        // Token: 0x040011D1 RID: 4561
        public string RotateRightActionName;

        // Token: 0x040011D2 RID: 4562
        public string LoadAmmoBeginActionName;

        // Token: 0x040011D3 RID: 4563
        public string LoadAmmoEndActionName;

        // Token: 0x040011D4 RID: 4564
        public string Reload2IdleActionName;

        // Token: 0x040011D5 RID: 4565
        public float ProjectileSpeed = 40f;

        // Token: 0x040011D6 RID: 4566
        private ActionIndexCache _idleAnimationActionIndex;

        // Token: 0x040011D7 RID: 4567
        private ActionIndexCache _shootAnimationActionIndex;

        // Token: 0x040011D8 RID: 4568
        private ActionIndexCache _reload1AnimationActionIndex;

        // Token: 0x040011D9 RID: 4569
        private ActionIndexCache _reload2AnimationActionIndex;

        // Token: 0x040011DA RID: 4570
        private ActionIndexCache _rotateLeftAnimationActionIndex;

        // Token: 0x040011DB RID: 4571
        private ActionIndexCache _rotateRightAnimationActionIndex;

        // Token: 0x040011DC RID: 4572
        private ActionIndexCache _loadAmmoBeginAnimationActionIndex;

        // Token: 0x040011DD RID: 4573
        private ActionIndexCache _loadAmmoEndAnimationActionIndex;

        // Token: 0x040011DE RID: 4574
        private ActionIndexCache _reload2IdleActionIndex;

        // Token: 0x040011DF RID: 4575
        private sbyte _missileBoneIndex;
        private bool _frameSetFlag;
        private MatrixFrame _setFrameAfterTick;
    }
}
