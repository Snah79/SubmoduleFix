using RestSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using static TaleWorlds.MountAndBlade.Agent;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class DrowningBehavior : MissionLogic
    {
        public static DrowningBehavior _instance;
        public bool IsSetProperly = false;
        public float UpperLimit;
        public float LowerLimit;

        public long LastCheckedAt = 0;
        public long Duration = 10;
        private object _lock = new object();
        private List<Agent> _agentsToRemove = new List<Agent>();

        public DrowningBehavior()
        {
            _instance = this;
        }

        public override void AfterStart()
        {
            GameEntity upperLimit = base.Mission.Scene.FindEntityWithTag("drowning_upper_limit");
            GameEntity lowerLimit = base.Mission.Scene.FindEntityWithTag("drowning_lower_limit");

            if (upperLimit == null || lowerLimit == null)
            {
                this.IsSetProperly = false;
                return;
            }
            this.IsSetProperly = true;
            this.UpperLimit = upperLimit.GetGlobalFrame().origin.Z;
            this.LowerLimit = lowerLimit.GetGlobalFrame().origin.Z;
            this.LastCheckedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent, in MissionWeapon affectorWeapon, in Blow blow, in AttackCollisionData attackCollisionData)
        {
            if (this.IsSetProperly && affectedAgent.IsActive() && !affectedAgent.IsHuman && affectedAgent.Position.Z < UpperLimit && affectedAgent.Position.Z > LowerLimit)
            {
                if (affectedAgent.Health < blow.InflictedDamage)
                {
                    // Restore health which was already removed for agent and mark him for fading out.
                    affectedAgent.Health += blow.InflictedDamage;
                }
                lock (_lock)
                {
                    _agentsToRemove.Add(affectedAgent);
                }
            }
        }

        public override void OnMissionTick(float dt)
        {
            if(_agentsToRemove.Any())
            {
                lock (_lock)
                {
                    var agentCount = _agentsToRemove.Count();

                    for (int i = 0; i < agentCount; i++)
                    {
                        RemoveNoneHumanAgent(_agentsToRemove[i]);
                    }

                    _agentsToRemove.Clear();
                }
                
                // Don't check drowning on this tick
                return;
            }

            if (LastCheckedAt + this.Duration < DateTimeOffset.UtcNow.ToUnixTimeSeconds() && this.IsSetProperly)
            {
                var delAgentList = new List<Agent>();

                LastCheckedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                foreach (Agent agent in base.Mission.Agents)
                {
                    if (IsInWater(agent))
                    //if (agent.IsActive() && !agent.IsHuman)
                    {
                        delAgentList.Add(agent);                        
                    }
                }

                delAgentList.ForEach(agent =>
                {
                    if(agent.IsHuman)
                    {
                        //Mission.Current.KillAgentCheat(agent);
                        Blow blow = new Blow(agent.Index);
                        //blow.DamageType = TaleWorlds.Core.DamageTypes.Pierce;
                        blow.DamageType = DamageTypes.Blunt;
                        blow.BoneIndex = agent.Monster.HeadLookDirectionBoneIndex;
                        blow.GlobalPosition = agent.Position;
                        blow.GlobalPosition.z = blow.GlobalPosition.z + agent.GetEyeGlobalHeight();
                        blow.BaseMagnitude = 40;
                        blow.WeaponRecord.FillAsMeleeBlow(null, null, -1, -1);
                        blow.InflictedDamage = 40;
                        blow.SwingDirection = agent.LookDirection;

                        MatrixFrame frame = agent.Frame;
                        blow.SwingDirection = frame.rotation.TransformToParent(new Vec3(-1f, 0f, 0f, -1f));
                        blow.SwingDirection.Normalize();

                        blow.Direction = blow.SwingDirection;
                        blow.DamageCalculated = true;
                        blow.IsFallDamage = true;
                        sbyte mainHandItemBoneIndex = agent.Monster.MainHandItemBoneIndex;
                        var attackCollisionDataForDebugPurpose = AttackCollisionData.GetAttackCollisionDataForDebugPurpose(false, false, false, true, false, false, false, false, false, false, false, false, CombatCollisionResult.StrikeAgent, -1, 0, 2, blow.BoneIndex, BoneBodyPartType.Head, mainHandItemBoneIndex, Agent.UsageDirection.AttackLeft, -1, CombatHitResultFlags.NormalHit, 0.5f, 1f, 0f, 0f, 0f, 0f, 0f, 0f, Vec3.Up, blow.Direction, blow.GlobalPosition, Vec3.Zero, Vec3.Zero, agent.Velocity, Vec3.Up);
                        agent.RegisterBlow(blow, in attackCollisionDataForDebugPurpose);
                    }
                    else
                    {
                        RemoveNoneHumanAgent(agent);
                    }
                });
                delAgentList.Clear();
            }
        }

        private void RemoveNoneHumanAgent(Agent agent)
        {
            if (agent.RiderAgent != null)
            {
                var blow = new Blow(agent.Index);
                blow.DamageType = DamageTypes.Blunt;
                blow.BoneIndex = -1;
                blow.GlobalPosition = agent.Position;
                blow.BaseMagnitude = 1f;
                blow.InflictedDamage = 400;
                blow.SwingDirection = agent.LookDirection;
                blow.Direction = agent.LookDirection;
                blow.WeaponRecord.FillAsMeleeBlow(null, null, -1, -1);
                agent.Die(blow);
            }
            else
            {
                agent.FadeOut(false, false);
            }
        }

        public bool IsInWater(Agent agent)
        {
            if(!IsSetProperly)
            {
                return false;
            }

            if (agent.IsActive() && agent.Position.Z < UpperLimit && agent.Position.Z > LowerLimit)
            {
                return true;
            }

            return false;
        }
    }
}