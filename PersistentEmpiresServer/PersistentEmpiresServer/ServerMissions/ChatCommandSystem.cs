using NetworkMessages.FromClient;
using PersistentEmpiresHarmony.Patches;
using PersistentEmpiresLib;
using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresServer.ChatCommands.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresServer.ServerMissions
{
    public class ChatCommandSystem : MissionNetwork
    {
        internal Dictionary<string, Command> commands;
        public static ChatCommandSystem Instance;
        internal bool DisableGlobalChat;
        internal PatreonRegistryBehavior patreonRegistry;
        internal Dictionary<NetworkCommunicator, bool> Muted;
        public string CommandPrefix;
        internal string DefaultMessageColor = "#FFFDFDFD";
        private GameNetworkMessage.ClientMessageHandlerDelegate<PlayerMessageAll> _messageAll;
        private GameNetworkMessage.ClientMessageHandlerDelegate<PlayerMessageTeam> _messageTeam;

        public ChatCommandSystem()
        {
            Instance = this;
        }

        //bool _runMeOnce = true;
        //public override void OnMissionTick(float dt)
        //{
        //    if (_runMeOnce)
        //    {
        //        _runMeOnce = false;
        //        //var logger = Mission.Current.GetMissionBehavior<MultiplayerGameLogger>();
        //        var chatBoxInstance = TaleWorlds.Core.Game.Current.GetGameHandler<ChatBox>();
        //        if (chatBoxInstance != null)
        //        {
        //            /*
        //            ChatBox

        //            networkMessageHandlerRegisterer.Register<NetworkMessages.FromClient.PlayerMessageAll>(HandleClientEventPlayerMessageAll);
        //    networkMessageHandlerRegisterer.Register<NetworkMessages.FromClient.PlayerMessageTeam>(HandleClientEventPlayerMessageTeam);
        //            */
        //            var handlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Remove);
        //            var method = typeof(ChatBox).GetMethod("HandleClientEventPlayerMessageAll", BindingFlags.NonPublic | BindingFlags.Instance);
        //            _messageAll = (GameNetworkMessage.ClientMessageHandlerDelegate<PlayerMessageAll>)Delegate.CreateDelegate(typeof(GameNetworkMessage.ClientMessageHandlerDelegate<PlayerMessageAll>), chatBoxInstance, method);
        //            var method2 = typeof(ChatBox).GetMethod("HandleClientEventPlayerMessageTeam", BindingFlags.NonPublic | BindingFlags.Instance);
        //            _messageTeam = (GameNetworkMessage.ClientMessageHandlerDelegate<PlayerMessageTeam>)Delegate.CreateDelegate(typeof(GameNetworkMessage.ClientMessageHandlerDelegate<PlayerMessageTeam>), chatBoxInstance, method2);

        //            handlerRegisterer.Register<PlayerMessageAll>(_messageAll);
        //            handlerRegisterer.Register<PlayerMessageTeam>(_messageTeam);

        //            handlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
        //            handlerRegisterer.Register<PlayerMessageAll>(PatchGlobalChat_OnClientEventPlayerMessageAll);
        //            handlerRegisterer.Register<PlayerMessageTeam>(PatchGlobalChat_OnClientEventPlayerMessageTeam);
        //        }
        //    }
        //}

        public override void AfterStart()
        {
            var chatBoxInstance = TaleWorlds.Core.Game.Current.GetGameHandler<ChatBox>();
            if (chatBoxInstance != null)
            {
                var handlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Remove);
                var method = typeof(ChatBox).GetMethod("HandleClientEventPlayerMessageAll", BindingFlags.NonPublic | BindingFlags.Instance);
                _messageAll = (GameNetworkMessage.ClientMessageHandlerDelegate<PlayerMessageAll>)Delegate.CreateDelegate(typeof(GameNetworkMessage.ClientMessageHandlerDelegate<PlayerMessageAll>), chatBoxInstance, method);
                var method2 = typeof(ChatBox).GetMethod("HandleClientEventPlayerMessageTeam", BindingFlags.NonPublic | BindingFlags.Instance);
                _messageTeam = (GameNetworkMessage.ClientMessageHandlerDelegate<PlayerMessageTeam>)Delegate.CreateDelegate(typeof(GameNetworkMessage.ClientMessageHandlerDelegate<PlayerMessageTeam>), chatBoxInstance, method2);

                handlerRegisterer.Register<PlayerMessageAll>(_messageAll);
                handlerRegisterer.Register<PlayerMessageTeam>(_messageTeam);

                handlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
                handlerRegisterer.Register<PlayerMessageAll>(PatchGlobalChat_OnClientEventPlayerMessageAll);
                handlerRegisterer.Register<PlayerMessageTeam>(PatchGlobalChat_OnClientEventPlayerMessageTeam);
            }
        }

        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            commands = new Dictionary<string, Command>();
            Muted = new Dictionary<NetworkCommunicator, bool>();
            //PatchGlobalChat.OnClientEventPlayerMessageAll += PatchGlobalChat_OnClientEventPlayerMessageAll;
            //PatchGlobalChat.OnClientEventPlayerMessageTeam += PatchGlobalChat_OnClientEventPlayerMessageTeam;
            LocalChatComponent localChat = base.Mission.GetMissionBehavior<LocalChatComponent>();
            localChat.OnPrefixHandleLocalChatFromClient += this.OnPrefixHandleLocalChatFromClient;
            patreonRegistry = base.Mission.GetMissionBehavior<PatreonRegistryBehavior>();
            CommandPrefix = ConfigManager.GetStrConfig("MessagePrefix", "!");
            DefaultMessageColor = ConfigManager.GetStrConfig("DefaultMessageColor", "#FFFDFDFD");

            Initialize();
        }

        private bool OnPrefixHandleLocalChatFromClient(NetworkCommunicator Sender, string Message, bool shout)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = Sender.GetComponent<PersistentEmpireRepresentative>();
            if (Message.StartsWith(CommandPrefix))
            {
                string[] argsWithCommand = Message.Split(' ');
                string command = argsWithCommand[0];
                string[] args = argsWithCommand.Skip(1).ToArray();
                this.Execute(Sender, command, args);
                return false;
            }
            return true;
        }

        public bool PatchGlobalChat_OnClientEventPlayerMessageAll(NetworkCommunicator networkPeer, PlayerMessageAll message)
        {
            var myTrace = new System.Diagnostics.StackTrace(0, true);
            try
            {
                var persistentEmpireRepresentative = networkPeer.GetComponent<PersistentEmpireRepresentative>();

                if (persistentEmpireRepresentative != null && persistentEmpireRepresentative.IsAdmin)
                {
                    InformationComponent.Instance.BroadcastMessage("(Admin) " + networkPeer.GetComponent<MissionPeer>().DisplayedName + ": " + message.Message, Color.ConvertStringToColor("#FDD835FF").ToUnsignedInteger());
                    return true;
                }
                else if (DisableGlobalChat)
                {
                    return true;
                }

                if (message.Message.StartsWith(CommandPrefix))
                {
                    string[] argsWithCommand = message.Message.Split(' ');
                    string command = argsWithCommand[0];
                    string[] args = argsWithCommand.Skip(1).ToArray();
                    this.Execute(networkPeer, command, args);

                    return true;
                }
                //if (persistentEmpireRepresentative != null || persistentEmpireRepresentative.IsAdmin || this.patreonRegistry.IsPlayerPatreon(networkPeer)) return true;

                if (this.Muted.ContainsKey(networkPeer))
                {
                    InformationComponent.Instance.SendMessage("You are muted.", Colors.Red.ToUnsignedInteger(), networkPeer);
                    return true;
                }

                // Let TW logic handle send message to all players
                return false;
            }
            catch (Exception ex)
            {
                var tmp = $"Exception was thrown in PatchGlobalChat_OnClientEventPlayerMessageAll. Player {networkPeer.UserName}. Message {message.Message}";
                InformationComponent.Instance.SendMessage(tmp, new Color(1f, 0f, 0f).ToUnsignedInteger(), networkPeer);
                ex.HelpLink = tmp;
                SaveSystemBehavior.RglExceptionThrown(myTrace, ex);
            }
            /*
            if(_messageAll != null)
            {
                _messageAll(networkPeer, message);
            }
            */
            return true;
        }

        public bool PatchGlobalChat_OnClientEventPlayerMessageTeam(NetworkCommunicator networkPeer, PlayerMessageTeam message)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = networkPeer.GetComponent<PersistentEmpireRepresentative>();
            Faction f = persistentEmpireRepresentative.GetFaction();

            if (f != null)
            {
                if(f.lordId == networkPeer.VirtualPlayer.ToPlayerId() || f.marshalls.Contains(networkPeer.VirtualPlayer.ToPlayerId()))
                {
                    foreach (NetworkCommunicator n in f.members)
                    {
                        if (n.IsConnectionActive && n.IsNetworkActive)
                        {
                            InformationComponent.Instance.SendMessage(f.name + " [" + networkPeer.UserName + "]: " + message.Message, Colors.Red.ToUnsignedInteger(), n);
                            InformationComponent.Instance.SendQuickInformationToPlayer("[" + f.name + "] " + message.Message, n, Colors.Red.ToUnsignedInteger());
                        }
                    }

                    LoggerHelper.LogAnActionNoDiscord(networkPeer, LogAction.PlayerMessageTeam, null, new object[] { f, message.Message });
                    
                    return true;
                }
                else if (!DisableGlobalChat)
                {
                    foreach (NetworkCommunicator n in f.members)
                    {
                        if (n.IsConnectionActive && n.IsNetworkActive)
                        {
                            InformationComponent.Instance.SendMessage(f.name + " [" + networkPeer.UserName + "]: " + message.Message, Colors.Red.ToUnsignedInteger(), networkPeer);
                        }
                    }
                    
                    LoggerHelper.LogAnActionNoDiscord(networkPeer, LogAction.PlayerMessageTeam, null, new object[] { f, message.Message });
                    
                    return true;
                }
            }
            /*
            if (_messageTeam != null)
            {
                _messageTeam(networkPeer, message);
            }
            */
            return true;
        }

        public bool Execute(NetworkCommunicator networkPeer, string command, string[] args)
        {
            Command executableCommand;
            bool exists = commands.TryGetValue(command, out executableCommand);
            if (!exists)
            {
                InformationComponent.Instance.SendMessage("This command is not exists", Colors.Red.ToUnsignedInteger(), networkPeer);
                return false;
            }
            if (!executableCommand.CanUse(networkPeer))
            {
                InformationComponent.Instance.SendMessage("You are not authorized to run this command", Colors.Red.ToUnsignedInteger(), networkPeer);
                return false;
            }
            return executableCommand.Execute(networkPeer, args);
        }

        private void Initialize()
        {
            this.commands = new Dictionary<string, Command>();
            foreach (Type mytype in System.Reflection.Assembly.GetExecutingAssembly().GetTypes()
                 .Where(mytype => mytype.GetInterfaces().Contains(typeof(Command))))
            {
                Command command = (Command)Activator.CreateInstance(mytype);
                if (!commands.ContainsKey(command.Command()) && command.IsEnabled())
                {
                    Debug.Print("** Chat Command " + command.Command() + " have been initiated !", 0, Debug.DebugColor.Green);
                    commands.Add(command.Command(), command);
                }
            }
        }
    }
}