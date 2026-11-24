using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class ClientSideConfig : GameNetworkMessage
    {
        public bool AgentLabelConfigEnabled = true;
        public int MapTimeLimit = 100000;

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
            MapTimeLimit = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 100000, true), ref result);

            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteBoolToPacket(this.AgentLabelConfigEnabled);
            GameNetworkMessage.WriteIntToPacket(this.MapTimeLimit, new CompressionInfo.Integer(0, 100000, true));
        }
    }
}
