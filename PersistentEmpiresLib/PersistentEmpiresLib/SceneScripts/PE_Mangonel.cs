using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.SceneScripts.Extensions;
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
    public sealed class PE_MangonelAI : RangedSiegeWeaponAi
    {
        // Token: 0x06001084 RID: 4228 RVA: 0x00035E83 File Offset: 0x00034083
        public PE_MangonelAI(PE_Mangonel mangonel) : base(mangonel)
        {
        }
    }
    public class PE_Mangonel : RangedSiegeWeapon, ISpawnable, IMoveable
    {
        private const string BodyTag = "body";
        private const string RopeTag = "rope";
        private const string RotateTag = "rotate";
        private const string LeftTag = "left";
        private const string VerticalAdjusterTag = "vertical_adjuster";
        //private static readonly ActionIndexCache act_usage_mangonel_idle = ActionIndexCache.Create("act_usage_mangonel_idle");
        //private static readonly ActionIndexCache act_usage_mangonel_load_ammo_begin = ActionIndexCache.Create("act_usage_mangonel_load_ammo_begin");
        //private static readonly ActionIndexCache act_usage_mangonel_load_ammo_end = ActionIndexCache.Create("act_usage_mangonel_load_ammo_end");
        //private static readonly ActionIndexCache act_pickup_boulder_begin = ActionIndexCache.Create("act_pickup_boulder_begin");
        //private static readonly ActionIndexCache act_pickup_boulder_end = ActionIndexCache.Create("act_pickup_boulder_end");
        //private static readonly ActionIndexCache act_usage_mangonel_reload = ActionIndexCache.Create("act_usage_mangonel_reload");
        //private static readonly ActionIndexCache act_usage_mangonel_reload_2 = ActionIndexCache.Create("act_usage_mangonel_reload_2");
        //private static readonly ActionIndexCache act_usage_mangonel_reload_2_idle = ActionIndexCache.Create("act_usage_mangonel_reload_2_idle");
        //private static readonly ActionIndexCache act_usage_mangonel_rotate_left = ActionIndexCache.Create("act_usage_mangonel_rotate_left");
        //private static readonly ActionIndexCache act_usage_mangonel_rotate_right = ActionIndexCache.Create("act_usage_mangonel_rotate_right");
        //private static readonly ActionIndexCache act_usage_mangonel_shoot = ActionIndexCache.Create("act_usage_mangonel_shoot");
        //private static readonly ActionIndexCache act_usage_mangonel_big_idle = ActionIndexCache.Create("act_usage_mangonel_big_idle");
        //private static readonly ActionIndexCache act_usage_mangonel_big_shoot = ActionIndexCache.Create("act_usage_mangonel_big_shoot");
        //private static readonly ActionIndexCache act_usage_mangonel_big_reload = ActionIndexCache.Create("act_usage_mangonel_big_reload");
        //private static readonly ActionIndexCache act_usage_mangonel_big_load_ammo_begin = ActionIndexCache.Create("act_usage_mangonel_big_load_ammo_begin");
        //private static readonly ActionIndexCache act_usage_mangonel_big_load_ammo_end = ActionIndexCache.Create("act_usage_mangonel_big_load_ammo_end");
        //private static readonly ActionIndexCache act_strike_bent_over = ActionIndexCache.Create("act_strike_bent_over");
        private string _missileBoneName = "end_throwarm";
        private List<StandingPoint> _rotateStandingPoints;
        private SynchedMissionObject _body;
        private SynchedMissionObject _rope;
        private GameEntity _verticalAdjuster;
        private MatrixFrame _verticalAdjusterStartingLocalFrame;
        private Skeleton _verticalAdjusterSkeleton;
        private Skeleton _bodySkeleton;
        private float _timeElapsedAfterLoading;
        private MatrixFrame[] _standingPointLocalIKFrames;
        private StandingPoint _reloadWithoutPilot;
        private StandingPoint moverStandingPoint;
        public string MoverStandingPointTag = "mover";
        public string MangonelBodySkeleton = "mangonel_skeleton";
        public string MangonelBodyFire = "mangonel_fire";
        public string MangonelBodyReload = "mangonel_set_up";
        public string MangonelRopeFire = "mangonel_holder_fire";
        public string MangonelRopeReload = "mangonel_holder_set_up";
        public string MangonelAimAnimation = "mangonel_a_anglearm_state";
        public string ProjectileBoneName = "end_throwarm";
        public string IdleActionName;
        public string ShootActionName;
        public string Reload1ActionName;
        public string Reload2ActionName;
        public string RotateLeftActionName;
        public string RotateRightActionName;
        public string LoadAmmoBeginActionName;
        public string LoadAmmoEndActionName;
        public string Reload2IdleActionName;
        public float ProjectileSpeed = 40f;
        private ActionIndexCache _idleAnimationActionIndex;
        private ActionIndexCache _shootAnimationActionIndex;
        private ActionIndexCache _reload1AnimationActionIndex;
        private ActionIndexCache _reload2AnimationActionIndex;
        private ActionIndexCache _rotateLeftAnimationActionIndex;
        private ActionIndexCache _rotateRightAnimationActionIndex;
        private ActionIndexCache _loadAmmoBeginAnimationActionIndex;
        private ActionIndexCache _loadAmmoEndAnimationActionIndex;
        private ActionIndexCache _reload2IdleActionIndex;
        private sbyte _missileBoneIndex;
        private bool _frameSetFlag;
        private MatrixFrame _setFrameAfterTick;

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
        private WeakGameEntity _weakGameEntity;
        protected override float MaximumBallisticError => 1.5f;

        protected override float ShootingSpeed => ProjectileSpeed;

        protected override float HorizontalAimSensitivity
        {
            get
            {
                if (DefaultSide == BattleSideEnum.Defender)
                {
                    return 0.25f;
                }

                float num = 0.05f;
                foreach (StandingPoint rotateStandingPoint in _rotateStandingPoints)
                {
                    if (rotateStandingPoint.HasUser && !rotateStandingPoint.UserAgent.IsInBeingStruckAction)
                    {
                        num += 0.1f;
                    }
                }

                return num;
            }
        }

        protected override float VerticalAimSensitivity => 0.1f;

        protected override Vec3 ShootingDirection
        {
            get
            {
                Mat3 rotation = _body.GameEntity.GetGlobalFrame().rotation;
                rotation.RotateAboutSide(0f - CurrentReleaseAngle);
                Vec3 v = new Vec3(0f, -1f);
                return rotation.TransformToParent(in v);
            }
        }

        protected override void UpdateAmmoMesh()
        { 
            // Wer not using any ammo stashes.
        }

        protected override bool HasAmmo
        {
            get
            {
                if (!base.HasAmmo && base.CurrentlyUsedAmmoPickUpPoint == null && !LoadAmmoStandingPoint.HasUser)
                {
                    return LoadAmmoStandingPoint.HasAIMovingTo;
                }

                return true;
            }
            set
            {
                base.HasAmmo = value;
            }
        }

        protected override void RegisterAnimationParameters()
        {
            SkeletonOwnerObjects = new SynchedMissionObject[2];
            Skeletons = new Skeleton[2];
            SkeletonNames = new string[1];
            FireAnimations = new string[2];
            FireAnimationIndices = new int[2];
            SetUpAnimations = new string[2];
            SetUpAnimationIndices = new int[2];
            SkeletonOwnerObjects[0] = _body;
            Skeletons[0] = _body.GameEntity.Skeleton;
            SkeletonNames[0] = MangonelBodySkeleton;
            FireAnimations[0] = MangonelBodyFire;
            FireAnimationIndices[0] = MBAnimation.GetAnimationIndexWithName(MangonelBodyFire);
            SetUpAnimations[0] = MangonelBodyReload;
            SetUpAnimationIndices[0] = MBAnimation.GetAnimationIndexWithName(MangonelBodyReload);
            SkeletonOwnerObjects[1] = _rope;
            Skeletons[1] = _rope.GameEntity.Skeleton;
            FireAnimations[1] = MangonelRopeFire;
            FireAnimationIndices[1] = MBAnimation.GetAnimationIndexWithName(MangonelRopeFire);
            SetUpAnimations[1] = MangonelRopeReload;
            SetUpAnimationIndices[1] = MBAnimation.GetAnimationIndexWithName(MangonelRopeReload);
            _missileBoneName = ProjectileBoneName;
            _idleAnimationActionIndex = ActionIndexCache.Create(IdleActionName);
            _shootAnimationActionIndex = ActionIndexCache.Create(ShootActionName);
            _reload1AnimationActionIndex = ActionIndexCache.Create(Reload1ActionName);
            _reload2AnimationActionIndex = ActionIndexCache.Create(Reload2ActionName);
            _rotateLeftAnimationActionIndex = ActionIndexCache.Create(RotateLeftActionName);
            _rotateRightAnimationActionIndex = ActionIndexCache.Create(RotateRightActionName);
            _loadAmmoBeginAnimationActionIndex = ActionIndexCache.Create(LoadAmmoBeginActionName);
            _loadAmmoEndAnimationActionIndex = ActionIndexCache.Create(LoadAmmoEndActionName);
            _reload2IdleActionIndex = ActionIndexCache.Create(Reload2IdleActionName);
        }

        public override UsableMachineAIBase CreateAIBehaviorObject()
        {
            return new PE_MangonelAI(this);
        }

        public override void AfterMissionStart()
        {
            /*if (this.AmmoPickUpStandingPoints != null)
			{
				foreach (StandingPointWithWeaponRequirement standingPointWithWeaponRequirement in this.AmmoPickUpStandingPoints)
				{
					standingPointWithWeaponRequirement.LockUserFrames = true;
				}
			}*/
            UpdateProjectilePosition();
        }

        public override SiegeEngineType GetSiegeEngineType()
        {
            if (DefaultSide != BattleSideEnum.Attacker)
            {
                return DefaultSiegeEngineTypes.Catapult;
            }

            return DefaultSiegeEngineTypes.Onager;
        }

        public void PreInit()
        {
        }

        protected override void OnInit()
        {
            AmmoPickUpTag = null;

            var list = GameEntity.CollectScriptComponentsWithTagIncludingChildrenRecursive<SynchedMissionObject>("rope");
            
            if (list.Count > 0)
            {
                _rope = list[0];
            }

            list = GameEntity.CollectScriptComponentsWithTagIncludingChildrenRecursive<SynchedMissionObject>("body");
            _body = list.Count > 0 ? list[0] : this;
            _bodySkeleton = _body.GameEntity.Skeleton;
            RotationObject = _body;

            var tlist2 = new List<WeakGameEntity>();// base.GameEntity.CollectChildrenEntitiesWithTag("vertical_adjuster");
            GameEntity.GetChildrenRecursive(ref tlist2);

            var list2 = tlist2.Where(x => x.Tags.Contains("vertical_adjuster")).ToList();

            _verticalAdjuster = TaleWorlds.Engine.GameEntity.CreateFromWeakEntity(list2[0]);
            _verticalAdjusterSkeleton = _verticalAdjuster.Skeleton;
            if (_verticalAdjusterSkeleton != null)
            {
                _verticalAdjusterSkeleton.SetAnimationAtChannel(MangonelAimAnimation, 0);
            }

            _verticalAdjusterStartingLocalFrame = _verticalAdjuster.GetFrame();
            _verticalAdjusterStartingLocalFrame = _body.GameEntity.GetBoneEntitialFrameWithIndex(0).TransformToLocal(in _verticalAdjusterStartingLocalFrame);
            base.OnInit();
            _weakGameEntity = GameEntity;
            this.InitiateMoveSynch();
            HitPoint = MaxHitPoint;
            LoadAmmoStandingPoint.InitRequiredWeaponClasses(new WeaponClass[] { OriginalMissileItem.PrimaryWeapon.WeaponClass });
            LoadAmmoStandingPoint.InitRequiredWeapon(OriginalMissileItem);
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

                var frame = _body.GameEntity.GetGlobalFrame();

                _standingPointLocalIKFrames = new MatrixFrame[base.StandingPoints.Count];

                for (int i = 0; i < base.StandingPoints.Count; i++)
                {
                    //_standingPointLocalIKFrames[i] = StandingPoints[i].GameEntity.GetGlobalFrame().TransformToLocal(frame);
                    _standingPointLocalIKFrames[i] = base.StandingPoints[i].GameEntity.GetGlobalFrame().TransformToLocalNonOrthogonal(in frame);
                    StandingPoints[i].AddComponent(new ClearHandInverseKinematicsOnStopUsageComponent());
                }
            }
            //_missileBoneIndex = Skeleton.GetBoneIndexFromName(SkeletonOwnerObjects[0].GameEntity.Skeleton.GetName(), _missileBoneName);
            _missileBoneIndex = Skeleton.GetBoneIndexFromName(Skeletons[0].GetName(), _missileBoneName);
            ApplyAimChange();

            foreach (var reloadStandingPoint in ReloadStandingPoints)
            {
                if (reloadStandingPoint != PilotStandingPoint)
                {
                    _reloadWithoutPilot = reloadStandingPoint;
                }
            }

            if (!GameNetwork.IsClientOrReplay)
            {
                //--SetActivationLoadAmmoPoint(activate: false);
            }

            EnemyRangeToStopUsing = 7f;
            moverStandingPoint = GameEntity.GetFirstChildEntityWithTag(MoverStandingPointTag).GetFirstScriptOfType<StandingPoint>();            
            SetScriptComponentToTick(GetTickRequirement());
            //--UpdateProjectilePosition();
        }

        protected override void OnEditorInit()
        {
        }

        public Agent GetPilotAgent()
        {
            StandingPoint pilotStandingPoint = moverStandingPoint;
            if (pilotStandingPoint == null)
            {
                return null;
            }

            return pilotStandingPoint.UserAgent;
        }

        // New 
        public override void OnPilotAssignedDuringSpawn()
        {
            PilotAgent.SetActionChannel(1, in _idleAnimationActionIndex, ignorePriority: false, (AnimFlags)0uL);
            
            var globalFrame = base.PilotStandingPoint.GameEntity.GetGlobalFrame();

            PilotAgent.TeleportToPosition(globalFrame.origin);
            PilotAgent.DisableScriptedMovement();
            
            var pilotAgent = base.PilotAgent;
            var direction = globalFrame.rotation.f.AsVec2.Normalized();
            
            pilotAgent.SetMovementDirection(in direction);
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
            if (GameEntity.IsVisibleIncludeParents())
            {
                return base.GetTickRequirement() | TickRequirement.Tick | TickRequirement.TickParallel;
            }

            return base.GetTickRequirement();
        }

        protected void MoveControl()
        {
            if (GameNetwork.IsServer)
            {
                if (this.GetPilotAgent() != null)
                {
                    if (_weakGameEntity.TryGetEntity(out var tmpGameEntity))
                    {
                        if (this.GetPilotAgent().Position.Distance(tmpGameEntity.GlobalPosition) > 5f)
                        {
                            this.GetPilotAgent().StopUsingGameObjectMT(false);
                        }
                    }
                }
            }

            if (GameNetwork.IsClient)
            {
                if (Agent.Main != null && this.GetPilotAgent() == Agent.Main)
                {
                    if (Mission.Current.InputManager.IsKeyPressed(InputKey.S))
                    {
                        this.RequestMovingForward();
                    }
                    else if (Mission.Current.InputManager.IsKeyReleased(InputKey.S))
                    {
                        this.RequestStopMovingForward();
                    }
                    if (Mission.Current.InputManager.IsKeyPressed(InputKey.W))
                    {
                        this.RequestMovingBackward();
                    }
                    else if (Mission.Current.InputManager.IsKeyReleased(InputKey.W))
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

        protected override void OnTick(float dt)
        {
            base.OnTick(dt);
            MoveControl();

#if SERVER
            var frame = this.MoveObjectTick(dt);
                
            SetFrameSynched(ref frame);
#endif

            if (!GameEntity.IsVisibleIncludeParents())
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
                                var missionWeapon = new MissionWeapon(this.OriginalMissileItem, null, null, 1);
                                
                                userAgent.EquipWeaponToExtraSlotAndWield(ref missionWeapon);
                                userAgent.StopUsingGameObject(true);
                                ConsumeAmmo();
                                
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
            switch (State)
            {                
                case WeaponState.LoadingAmmo:
                    if (GameNetwork.IsClientOrReplay)
                    {
                        break;
                    }

                    if (LoadAmmoStandingPoint.HasUser)
                    {
                        var userAgent2 = LoadAmmoStandingPoint.UserAgent;

                        if (userAgent2.GetCurrentAction(1) == _loadAmmoEndAnimationActionIndex)
                        {
                            var primaryWieldedItemIndex = userAgent2.GetPrimaryWieldedItemIndex();
                            
                            if (primaryWieldedItemIndex != EquipmentIndex.None && userAgent2.Equipment[primaryWieldedItemIndex].CurrentUsageItem.WeaponClass == OriginalMissileItem.PrimaryWeapon.WeaponClass)
                            {
                                ChangeProjectileEntityServer(userAgent2, userAgent2.Equipment[primaryWieldedItemIndex].Item.StringId);
                                userAgent2.RemoveEquippedWeapon(primaryWieldedItemIndex);
                                _timeElapsedAfterLoading = 0f;
                                Projectile.SetVisibleSynched(value: true);
                                State = WeaponState.WaitingBeforeIdle;
                            }
                            else
                            {
                                userAgent2.StopUsingGameObject(isSuccessful: true, Agent.StopUsingGameObjectFlags.None);
                            }
                        }
                        else
                        {
                            if (!(userAgent2.GetCurrentAction(1) != _loadAmmoBeginAnimationActionIndex) || userAgent2.SetActionChannel(1, in _loadAmmoBeginAnimationActionIndex, ignorePriority: false, (AnimFlags)0uL))
                            {
                                break;
                            }

                            for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.NumAllWeaponSlots; equipmentIndex++)
                            {
                                if (!userAgent2.Equipment[equipmentIndex].IsEmpty && userAgent2.Equipment[equipmentIndex].CurrentUsageItem.WeaponClass == OriginalMissileItem.PrimaryWeapon.WeaponClass)
                                {
                                    userAgent2.RemoveEquippedWeapon(equipmentIndex);
                                }
                            }

                            userAgent2.StopUsingGameObject(isSuccessful: true, Agent.StopUsingGameObjectFlags.None);
                        }
                    }
                    else if (LoadAmmoStandingPoint.HasAIMovingTo)
                    {
                        Agent movingAgent = LoadAmmoStandingPoint.MovingAgent;
                        EquipmentIndex primaryWieldedItemIndex2 = movingAgent.GetPrimaryWieldedItemIndex();
                        if (primaryWieldedItemIndex2 == EquipmentIndex.None || movingAgent.Equipment[primaryWieldedItemIndex2].CurrentUsageItem.WeaponClass != OriginalMissileItem.PrimaryWeapon.WeaponClass)
                        {
                            movingAgent.StopUsingGameObject(isSuccessful: true, Agent.StopUsingGameObjectFlags.None);
                            SendAgentToAmmoPickup(movingAgent);
                        }
                    }

                    break;
                case WeaponState.WaitingBeforeIdle:
                    _timeElapsedAfterLoading += dt;
                    if (_timeElapsedAfterLoading > 1f)
                    {
                        State = WeaponState.Idle;
                    }

                    break;
                case WeaponState.Reloading:
                case WeaponState.ReloadingPaused:
                    break;
            }
        }

        protected override void ApplyCurrentDirectionToEntity()
        {
            /*MatrixFrame rotationObjectInitialFrame = this.RotationObject.GameEntity.GetFrame();
			rotationObjectInitialFrame.rotation.RotateAboutUp(this.currentDirection);
			this.RotationObject.GameEntity.SetFrame(ref rotationObjectInitialFrame);*/
        }

        protected override void OnTickParallel(float dt)
        {
            base.OnTickParallel(dt);
            if (_weakGameEntity.TryGetEntity(out var tmpGameEntity))
            {
                if (!tmpGameEntity.IsVisibleIncludeParents())
                {
                    return;
                }
            }
            else
            {
                return;
            }

            if (State == WeaponState.WaitingBeforeProjectileLeaving)
            {
                UpdateProjectilePosition();
            }

            if (_verticalAdjusterSkeleton != null)
            {
                float parameter = MBMath.ClampFloat((CurrentReleaseAngle - BottomReleaseAngleRestriction) / (TopReleaseAngleRestriction - BottomReleaseAngleRestriction), 0f, 1f);
                _verticalAdjusterSkeleton.SetAnimationParameterAtChannel(0, parameter);
            }

            var frame = Skeletons[0].GetBoneEntitialFrameWithIndex(0).TransformToParent(in _verticalAdjusterStartingLocalFrame);
            var boundEntityGlobalFrame = _body.GameEntity.GetGlobalFrame();

            _verticalAdjuster.SetFrame(ref frame);

            for (int i = 0; i < base.StandingPoints.Count; i++)
            {
                if (!StandingPoints[i].HasUser)
                {
                    continue;
                }

                if (StandingPoints[i].UserAgent.IsInBeingStruckAction || AmmoPickUpPoints.IndexOf(StandingPoints[i]) >= 0)
                {
                    StandingPoints[i].UserAgent.ClearHandInverseKinematics();
                    continue;
                }

                var currentAction = base.StandingPoints[i].UserAgent.GetCurrentAction(1);
                var currentActionProgress = base.StandingPoints[i].UserAgent.GetCurrentActionProgress(1);

                if (currentAction != _reload2IdleActionIndex && (currentAction != _reload2AnimationActionIndex || currentActionProgress > 0.1f) && (currentAction != _shootAnimationActionIndex || currentActionProgress < 0.15f))
                {
                    StandingPoints[i].UserAgent.SetHandInverseKinematicsFrameForMissionObjectUsage(in _standingPointLocalIKFrames[i], in boundEntityGlobalFrame);
                }
                else
                {
                    StandingPoints[i].UserAgent.ClearHandInverseKinematics();
                }
            }

            if (!GameNetwork.IsClientOrReplay)
            {
                for (int j = 0; j < _rotateStandingPoints.Count; j++)
                {
                    var standingPoint = _rotateStandingPoints[j];

                    if (standingPoint.HasUser)
                    {
                        Agent userAgent = standingPoint.UserAgent;
                        ActionIndexCache actionIndexCache = ((j == 0) ? _rotateLeftAnimationActionIndex : _rotateRightAnimationActionIndex);
                        if (!userAgent.SetActionChannel(1, in actionIndexCache, ignorePriority: false, (AnimFlags)0uL) && standingPoint.UserAgent.Controller != AgentControllerType.AI)
                        {
                            standingPoint.UserAgent.StopUsingGameObjectMT();
                        }
                    }
                }

                if (PilotAgent != null)
                {
                    var currentAction2 = PilotAgent.GetCurrentAction(1);
                    
                    if (State == WeaponState.WaitingBeforeProjectileLeaving)
                    {
                        if (PilotAgent.IsInBeingStruckAction)
                        {
                            if (currentAction2 != ActionIndexCache.act_none && currentAction2 != ActionIndexCache.act_strike_bent_over)
                            {
                                PilotAgent.SetActionChannel(1, in ActionIndexCache.act_strike_bent_over, ignorePriority: false, (AnimFlags)0uL);
                            }
                        }
                        else if (!PilotAgent.SetActionChannel(1, in _shootAnimationActionIndex, ignorePriority: false, (AnimFlags)0uL) && base.PilotAgent.Controller != AgentControllerType.AI)
                        {
                            PilotAgent.StopUsingGameObjectMT();
                        }
                    }
                    else if (!PilotAgent.SetActionChannel(1, in _idleAnimationActionIndex, ignorePriority: false, (AnimFlags)0uL) && currentAction2 != _reload1AnimationActionIndex && currentAction2 != _shootAnimationActionIndex && base.PilotAgent.Controller != AgentControllerType.AI)
                    {
                        PilotAgent.StopUsingGameObjectMT();
                    }
                }

                if (_reloadWithoutPilot.HasUser)
                {
                    var userAgent2 = _reloadWithoutPilot.UserAgent;

                    if (!userAgent2.SetActionChannel(1, in _reload2IdleActionIndex, ignorePriority: false, (AnimFlags)0uL) && userAgent2.GetCurrentAction(1) != _reload2AnimationActionIndex && userAgent2.Controller != AgentControllerType.AI)
                    {
                        userAgent2.StopUsingGameObjectMT();
                    }
                }
            }

            if (State != WeaponState.Reloading)
            {
                return;
            }

            foreach (StandingPoint reloadStandingPoint in ReloadStandingPoints)
            {
                if (!reloadStandingPoint.HasUser)
                {
                    continue;
                }

                ActionIndexCache currentAction3 = reloadStandingPoint.UserAgent.GetCurrentAction(1);
                if (currentAction3 == _reload1AnimationActionIndex || currentAction3 == _reload2AnimationActionIndex)
                {
                    reloadStandingPoint.UserAgent.SetCurrentActionProgress(1, _bodySkeleton.GetAnimationParameterAtChannel(0));
                }
                else if (!GameNetwork.IsClientOrReplay)
                {
                    ActionIndexCache actionIndexCache2 = ((reloadStandingPoint == base.PilotStandingPoint) ? _reload1AnimationActionIndex : _reload2AnimationActionIndex);
                    if (!reloadStandingPoint.UserAgent.SetActionChannel(1, in actionIndexCache2, ignorePriority: false, (AnimFlags)0uL, 0f, 1f, -0.2f, 0.4f, _bodySkeleton.GetAnimationParameterAtChannel(0)) && reloadStandingPoint.UserAgent.Controller != AgentControllerType.AI)
                    {
                        reloadStandingPoint.UserAgent.StopUsingGameObjectMT();
                    }
                }
            }
        }

        protected override void SetActivationLoadAmmoPoint(bool activate)
        {
            LoadAmmoStandingPoint.SetIsDeactivatedSynched(!activate);
        }

        protected override void UpdateProjectilePosition()
        {
            MatrixFrame frame = Skeletons[0].GetBoneEntitialFrameWithIndex(_missileBoneIndex);
            Projectile.GameEntity.SetFrame(ref frame);
        }

        protected override void OnRangedSiegeWeaponStateChange()
        {
            base.OnRangedSiegeWeaponStateChange();
            
            switch (State)
            {
                case WeaponState.WaitingBeforeIdle:
                    UpdateProjectilePosition();

                    break;
                case WeaponState.Shooting:
                    if (!GameNetwork.IsClientOrReplay)
                    {
                        Projectile.SetVisibleSynched(value: false);
                    }
                    else
                    {
                        Projectile.GameEntity.SetVisibilityExcludeParents(visible: false);
                    }

                    break;
                case WeaponState.Idle:
                    if (!GameNetwork.IsClientOrReplay)
                    {
                        Projectile.SetVisibleSynched(value: true);
                    }
                    else
                    {
                        Projectile.GameEntity.SetVisibilityExcludeParents(visible: true);
                    }

                    break;
            }
        }

        protected override void GetSoundEventIndices()
        {
            MoveSoundIndex = SoundEvent.GetEventIdFromString("event:/mission/siege/mangonel/move");
            ReloadSoundIndex = SoundEvent.GetEventIdFromString("event:/mission/siege/mangonel/reload");
            FireSoundIndex = SoundEvent.GetEventIdFromString("event:/mission/siege/mangonel/fire");
        }

        protected override void ApplyAimChange()
        {
            base.ApplyAimChange();

            ShootingDirection.Normalize();
        }

        public override TextObject GetDescriptionText(WeakGameEntity gameEntity)
        {
            if (!gameEntity.HasTag(AmmoPickUpTag))
            {
                return new TextObject("{=NbpcDXtJ}Mangonel");
            }

            return new TextObject("{=pzfbPbWW}Boulder");
        }

        public override TextObject GetActionTextForStandingPoint(UsableMissionObject usableGameObject)
        {
            TextObject textObject = (usableGameObject.GameEntity.HasTag("reload") ? new TextObject((base.PilotStandingPoint == usableGameObject) ? "{=fEQAPJ2e}{KEY} Use" : "{=Na81xuXn}{KEY} Rearm") : (usableGameObject.GameEntity.HasTag("rotate") ? new TextObject("{=5wx4BF5h}{KEY} Rotate") : (usableGameObject.GameEntity.HasTag(AmmoPickUpTag) ? new TextObject("{=bNYm3K6b}{KEY} Pick Up") : ((!usableGameObject.GameEntity.HasTag("ammoload")) ? new TextObject("{=fEQAPJ2e}{KEY} Use") : new TextObject("{=ibC4xPoo}{KEY} Load Ammo")))));
            textObject.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
            return textObject;
            /*
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
            */
        }

        public override TargetFlags GetTargetFlags()
        {
            var targetFlags = TargetFlags.None;

            targetFlags |= TargetFlags.IsFlammable;
            targetFlags |= TargetFlags.IsSiegeEngine;
            targetFlags |= TargetFlags.IsAttacker;
            //if (Side == BattleSideEnum.Attacker)
            //{
            //    targetFlags |= TargetFlags.IsAttacker;
            //}

            if (IsDestroyed || IsDeactivated)
            {
                targetFlags |= TargetFlags.NotAThreat;
            }

            if (Side == BattleSideEnum.Attacker && DebugSiegeBehavior.DebugDefendState == DebugSiegeBehavior.DebugStateDefender.DebugDefendersToMangonels)
            {
                targetFlags |= TargetFlags.DebugThreat;
            }

            if (Side == BattleSideEnum.Defender && DebugSiegeBehavior.DebugAttackState == DebugSiegeBehavior.DebugStateAttacker.DebugAttackersToMangonels)
            {
                targetFlags |= TargetFlags.DebugThreat;
            }

            return targetFlags;
        }

        public override float GetTargetValue(List<Vec3> weaponPos)
        {
            return 40f * GetUserMultiplierOfWeapon() * GetDistanceMultiplierOfWeapon(weaponPos[0]) * GetHitPointMultiplierOfWeapon();
        }

        public override float ProcessTargetValue(float baseValue, TargetFlags flags)
        {
            if (flags.HasAnyFlag(TargetFlags.NotAThreat))
            {
                return -1000f;
            }

            if (flags.HasAnyFlag(TargetFlags.IsSiegeEngine))
            {
                baseValue *= 10000f;
            }

            if (flags.HasAnyFlag(TargetFlags.IsStructure))
            {
                baseValue *= 2.5f;
            }

            if (flags.HasAnyFlag(TargetFlags.IsSmall))
            {
                baseValue *= 8f;
            }

            if (flags.HasAnyFlag(TargetFlags.IsMoving))
            {
                baseValue *= 8f;
            }

            if (flags.HasAnyFlag(TargetFlags.DebugThreat))
            {
                baseValue *= 10000f;
            }

            if (flags.HasAnyFlag(TargetFlags.IsSiegeTower))
            {
                baseValue *= 8f;
            }

            return baseValue;
        }

        protected override float GetDetachmentWeightAux(BattleSideEnum side)
        {
            return GetDetachmentWeightAuxForExternalAmmoWeapons(side);
        }

        public void SetSpawnedFromSpawner()
        {
            _spawnedFromSpawner = true;
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
            _setFrameAfterTick = frame;
            _frameSetFlag = true;
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
            HitPoint = hitPoint;
            
            if (HitPoint > MaxHitPoint) HitPoint = MaxHitPoint;
            
            if (HitPoint < 0) HitPoint = 0;

            if (!_weakGameEntity.TryGetEntity(out var tmpGameEntity))
            {
                return;
            }

            var globalFrame = tmpGameEntity.GetGlobalFrame();

            if (this.HitPoint == 0)
            {
                for (int i = 0; i < base.StandingPoints.Count; i++)
                {
                    if (StandingPoints[i].HasUser)
                    {
                        StandingPoints[i].UserAgent.StopUsingGameObjectMT(false);
                    }
                }
#if CLIENT
                if (ParticleEffectOnDestroy != "")
                {
                    Mission.Current.Scene.CreateBurstParticle(ParticleSystemManager.GetRuntimeIdByName(this.ParticleEffectOnDestroy), globalFrame);
                }
                if (SoundEffectOnDestroy != "")
                {
                    Mission.Current.MakeSound(SoundEvent.GetEventIdFromString(this.SoundEffectOnDestroy), globalFrame.origin, false, true, -1, -1);
                }
#endif
                tmpGameEntity.Remove(0);
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

        internal void Remove(int code)
        {
            if (_weakGameEntity.TryGetEntity(out var tmpGameEntity))
            {
                tmpGameEntity.Remove(code);
            }
        }
    }
}