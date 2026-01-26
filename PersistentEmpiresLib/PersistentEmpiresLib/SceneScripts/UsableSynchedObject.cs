using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.NetworkMessages.Server;
using System.Reflection;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.SceneScripts
{
    public abstract class PE_UsableSynchedObject : UsableMissionObject
    {
        public void AddBodyFlagsSynchedPE(BodyFlags flags, bool applyToChildren = true)
        {
            if (!GameEntity.TryGetEntity(out var tmpGameEntity))
            {
                return;
            }

            if ((tmpGameEntity.BodyFlag & flags) != flags)
            {
                if (GameNetwork.IsServerOrRecorder)
                {
                    GameNetwork.BeginBroadcastModuleEvent();
                    GameNetwork.WriteMessage(new AddMissionObjectBodyFlagPE(this, flags, applyToChildren));
                    GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord, null);
                }
                tmpGameEntity.AddBodyFlags(flags, applyToChildren);
                // this._initialSynchFlags |= SynchedMissionObject.SynchFlags.SynchBodyFlags;
                FieldInfo synchField = typeof(PE_InventoryEntity).BaseType.BaseType.GetField("_initialSynchFlags", BindingFlags.Instance | BindingFlags.NonPublic);
                SynchedMissionObject.SynchFlags synchFlags = (SynchedMissionObject.SynchFlags)synchField.GetValue(this);
                synchFlags |= SynchedMissionObject.SynchFlags.SynchBodyFlags;
                synchField.SetValue(this, synchFlags);
            }
        }
        public void AddPhysicsSynchedPE(Vec3 initialVelocity, Vec3 angularVelocity, string physicsMaterial)
        {
            if (!GameEntity.TryGetEntity(out var tmpGameEntity))
            {
                return;
            }

            tmpGameEntity.AddPhysics(tmpGameEntity.Mass, tmpGameEntity.CenterOfMass, tmpGameEntity.GetBodyShape(), initialVelocity, angularVelocity, PhysicsMaterial.GetFromName(physicsMaterial), false, 0);
            
            if (GameNetwork.IsServerOrRecorder)
            {
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new AddPhysicsToMissionObject(this, initialVelocity, angularVelocity, physicsMaterial));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord, null);
            }
        }
    }
}
