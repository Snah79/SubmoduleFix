using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class ClientSideConfig : GameNetworkMessage
    {
        public bool AgentLabelConfigEnabled = true;
        public int MapTimeLimit = 50;

        public ClientSideConfig()
        {
        }
        public ClientSideConfig(bool agentLabelConfigEnabled, int mapTimeLimit)
        {
            AgentLabelConfigEnabled = agentLabelConfigEnabled;
            MapTimeLimit = mapTimeLimit;
        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.All;
        }

        protected override string OnGetLogFormat()
        {
            return "ClientSideConfig";
        }

        protected override bool OnRead()
        {
            bool result = true;

            AgentLabelConfigEnabled = GameNetworkMessage.ReadBoolFromPacket(ref result);
            var tmp = GameNetworkMessage.ReadStringFromPacket(ref result);
            MapTimeLimit = int.Parse(tmp);

            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteBoolToPacket(AgentLabelConfigEnabled);
            GameNetworkMessage.WriteStringToPacket(MapTimeLimit.ToString());
        }
    }
}
