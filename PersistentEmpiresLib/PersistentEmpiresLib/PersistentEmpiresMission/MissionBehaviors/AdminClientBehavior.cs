using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.NetworkMessages.Server;
using PersistentEmpiresLib.SceneScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class AdminClientBehavior : MissionNetwork
    {
        public delegate void AdminPanelClick();
        public event AdminPanelClick OnAdminPanelClick;
        public static List<AdminTp> AdminTps = new List<AdminTp>();
        public static bool IsVisible = true;
#if SERVER
        public static bool CanUseSuicide = true;
        public static bool CanUseChangeColor = true;
#endif

        public void HandleAdminPanelClick()
        {
            if (this.OnAdminPanelClick != null)
            {
                this.OnAdminPanelClick();
            }
        }

        public void HandleUnbanPlayerClick()
        {
            InformationManager.ShowTextInquiry(
             new TextInquiryData(
             GameTexts.FindText("AdminClientBehaviorInqCaption", null).ToString()
             , GameTexts.FindText("AdminClientBehaviorInqText", null).ToString()
             , true
             , true
             , GameTexts.FindText("PE_InquiryData_Select", null).ToString()
             , GameTexts.FindText("PE_InquiryData_Cancel", null).ToString()
             , OnIdWritten
             , (Action)null
             , false
             , IsNameApplicable
             , ""
             , ""));
        }
        
        private void OnIdWritten(string name)
        {
            var message = new RequestUnBan(GameNetwork.MyPeer.VirtualPlayer?.ToPlayerId(), name);
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(message);
            GameNetwork.EndModuleEventAsClient();
        }

        private Tuple<bool, string> IsNameApplicable(string inputText)
        {
            var reg = new Regex(@"^[0-9.]+$");
            var result = reg.Match(inputText);

            if (!result.Success)
                return new Tuple<bool, string>(false, GameTexts.FindText("AdminClientBehaviorError1", null).ToString());

            if (string.IsNullOrWhiteSpace(inputText))
                return new Tuple<bool, string>(false, GameTexts.FindText("AdminClientBehaviorError2", null).ToString());

            if (inputText.Length > 30)
                return new Tuple<bool, string>(false, GameTexts.FindText("AdminClientBehaviorError3", null).ToString());

            return new Tuple<bool, string>(true, inputText);
        }

        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            IsVisible = true;
            AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
#if SERVER
            CanUseSuicide = ConfigManager.GetBoolConfig("CanUseSuicide", true);
            CanUseChangeColor = ConfigManager.GetBoolConfig("CanUseChangeColor", true);
#endif
        }
        public override void OnRemoveBehavior()
        {
            AdminTps.Clear();
            IsVisible = true;
            base.OnRemoveBehavior();
            AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Remove);
        }

        public void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
        {
            GameNetwork.NetworkMessageHandlerRegisterer networkMessageHandlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(mode);
#if CLIENT
            networkMessageHandlerRegisterer.Register<AuthorizeAsAdmin>(HandleAuthorizeAsAdminFromServer);
            networkMessageHandlerRegisterer.Register<ToggleVisibilityForAgent>(HandleToggleVisibilityForAgent);
            networkMessageHandlerRegisterer.Register<BroadcastChangeCustomColors>(HandleBroadcastChangeCustomColors);
#endif
        }

        private void HandleAuthorizeAsAdminFromServer(AuthorizeAsAdmin message)
        {
            GameNetwork.MyPeer.GetComponent<PersistentEmpireRepresentative>().IsAdmin = true;
        }

        private void HandleToggleVisibilityForAgent(ToggleVisibilityForAgent message)
        {
            message.Agent.AgentVisuals.SetVisible(message.Visible);
            message.Agent.AgentVisuals.LazyUpdateAgentRendererData();
        }

        private void HandleBroadcastChangeCustomColors(BroadcastChangeCustomColors message)
        {
            var peer = GameNetwork.NetworkPeers.Where(x => x.VirtualPlayer.Id.ToString() == message.PlayerUserId).FirstOrDefault();

            if(peer != null && peer.ControlledAgent != null)
            {
                peer.ControlledAgent.SetClothingColor1(BannerManager.GetColor(message.PrimaryColor));
                peer.ControlledAgent.SetClothingColor2(BannerManager.GetColor(message.SecondaryColor));
                AgentHelpers.ResetAgentMesh(peer.ControlledAgent);
            }
        }

        internal static void Register(AdminTp adminTp)
        {
            AdminTps.Add(adminTp);
        }

        public void ToggleInvisible()
        {
            IsVisible = !IsVisible;
            var message = new RequestToggleInvisibility(IsVisible);
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(message);
            GameNetwork.EndModuleEventAsClient();
        }
    }
}