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
            entity = GameEntity.CreateFromWeakEntity(_weakEntity);

            return entity != null;
        }
    }
}