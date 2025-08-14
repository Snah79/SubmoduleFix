using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.Helpers;
using System;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.ObjectSystem;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class BroadcastChangeCustomColors : GameNetworkMessage
    {
        public string PlayerUserId { get; private set; }

        public int PrimaryColor { get; set; }

        public int SecondaryColor { get; set; }

        public BroadcastChangeCustomColors(string userid, int primaryColor, int secondaryColor)
        {
            PlayerUserId = userid;
            PrimaryColor = primaryColor;
            SecondaryColor = secondaryColor;
        }

        public BroadcastChangeCustomColors()
        {
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.General;
        }

        protected override string OnGetLogFormat()
        {
            return "Faction added.";
        }

        protected override bool OnRead()
        {
            bool bufferReadValid = true;
            PlayerUserId = ReadStringFromPacket(ref bufferReadValid);
            PrimaryColor = ReadIntFromPacket(new CompressionInfo.Integer(-1, int.MaxValue, true), ref bufferReadValid);
            SecondaryColor = ReadIntFromPacket(new CompressionInfo.Integer(-1, int.MaxValue, true), ref bufferReadValid);
            return bufferReadValid;
        }

        protected override void OnWrite()
        {
            WriteStringToPacket(PlayerUserId);
            WriteIntToPacket(PrimaryColor, new CompressionInfo.Integer(-1, int.MaxValue, true));
            WriteIntToPacket(SecondaryColor, new CompressionInfo.Integer(-1, int.MaxValue, true));
        }
    }
}