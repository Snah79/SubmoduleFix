using NetworkMessages.FromClient;
using PersistentEmpiresLib.SceneScripts;
using System;
using System.Linq;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresHarmony.Patches
{
    public class PatchTemporary
    {
        public static bool PrefixHandleClientEventRequestUseObject(NetworkCommunicator networkPeer, GameNetworkMessage baseMessage)
        {
            RequestUseObject requestUseObject = (RequestUseObject)baseMessage;
            MissionPeer component = networkPeer.GetComponent<MissionPeer>();
            //5739
            //{2892305087360}

            var tmp = new MissionObjectId(110, true);
            var t = Mission.Current.GetActiveEntitiesWithScriptComponentOfType<PE_PrefabSpawner>().FirstOrDefault();
            if (t != null)
            {
                var s = t.GetFirstScriptOfType<PE_PrefabSpawner>();
                var a = s.SpawnedPrefabs.FirstOrDefault();
            }
            var bb = Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(tmp);

            foreach (Mission.DynamicallyCreatedEntity createdEntity in Mission.Current.AddedEntitiesInfo)
            { 
                var aaa = Mission.Current.MissionObjects.FirstOrDefault((MissionObject mo) => mo.Id == createdEntity.ObjectId);
            }
            if (Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(requestUseObject.UsableMissionObjectId) is UsableMissionObject usableMissionObject && component.ControlledAgent != null && component.ControlledAgent.IsActive())
            //if (Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(tmp) is UsableMissionObject usableMissionObject && component.ControlledAgent != null && component.ControlledAgent.IsActive())
            {
                Vec3 position = component.ControlledAgent.Position;
                Vec3 globalPosition = usableMissionObject.InteractionEntity.GlobalPosition;
                float num;
                if (usableMissionObject is StandingPoint)
                {
                    num = usableMissionObject.GetUserFrameForAgent(component.ControlledAgent).Origin.AsVec2.Distance(component.ControlledAgent.Position.AsVec2);
                }
                else
                {
                    usableMissionObject.InteractionEntity.GetPhysicsMinMax(includeChildren: true, out var bbmin, out var bbmax, returnLocal: false);
                    float a = globalPosition.Distance(bbmin);
                    float b = globalPosition.Distance(bbmax);
                    float num2 = TaleWorlds.Library.MathF.Max(a, b);
                    num = globalPosition.Distance(new Vec3(position.x, position.y, position.z + component.ControlledAgent.GetEyeGlobalHeight()));
                    num -= num2;
                    num = TaleWorlds.Library.MathF.Max(num, 0f);
                }

                if (component.ControlledAgent.CurrentlyUsedGameObject != usableMissionObject && component.ControlledAgent.CanReachAndUseObject(usableMissionObject, num * num * 0.9f * 0.9f) && component.ControlledAgent.ObjectHasVacantPosition(usableMissionObject))
                {
                        component.ControlledAgent.UseGameObject(usableMissionObject, requestUseObject.UsedObjectPreferenceIndex);
                }
            }
            return false;
        }
        public static Exception FinalizerHandleClientEventRequestUseObject(Exception __exception)
        {
            if (__exception != null)
            {
                Debug.Print("ERROR ON USE ITEM");
            }

            return __exception;
        }
    }
}
