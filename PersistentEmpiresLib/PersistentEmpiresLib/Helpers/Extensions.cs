using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System;
using TaleWorlds.Core;
using TaleWorlds.Engine;

namespace PersistentEmpiresLib.Helpers
{
    public static class Extensions
    {
        public static string ToPlayerId(this VirtualPlayer virtualPlayer)
        {
            return $"{virtualPlayer.Id.ToString()}_{virtualPlayer.UserName}";
        }

        public static string EncodeSpecialMariaDbChars(this string tmp)
        {
            return tmp.Replace(@"""", "'").Replace(@"''", @"'").Replace(@"'", @"\'").Replace(@"\\", @"\");
        }

        public static bool TryGetEntity(this WeakGameEntity _weakEntity, out GameEntity entity)
        {
            var myTrace = new System.Diagnostics.StackTrace(0, true);
            entity = null;
            try
            {
                if(!_weakEntity.IsValid)
                {
                    return false;
                }
                
                entity = GameEntity.CreateFromWeakEntity(_weakEntity);

                return entity != null;
            }
            catch (System.Exception ex)
            {
                var tmp = $"Exception was thrown in TryGetEntity.";
                ex.HelpLink = tmp;
                SaveSystemBehavior.RglExceptionThrown(myTrace, ex);

                entity = null;

                return false;
            }
        }
    }
}