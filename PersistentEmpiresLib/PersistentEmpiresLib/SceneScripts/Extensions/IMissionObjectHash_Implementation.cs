using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.SceneScripts.Interfaces;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace PersistentEmpiresLib.SceneScripts.Extensions
{
    public static class IMissionObjectHash_Implementation
    {
        public static string GetMissionObjectHash(this IMissionObjectHash missionObjectHash)
        {
            if (!missionObjectHash.GetMissionObject().GameEntity.TryGetEntity(out var tmpGameEntity))
            {
                return "";
            }

            MatrixFrame frame = tmpGameEntity.GetGlobalFrame();
            float x = frame.origin.X;
            float y = frame.origin.Y;
            float z = frame.origin.Z;

            string toHashed = x + "," + y + "," + z;
            return CryptoHelper.GetHashString(toHashed);
        }
    }
}
