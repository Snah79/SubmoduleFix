using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.SceneScripts
{
    public class PE_MoneyBag : UsableMissionObject
    {
        private int _amount;
        private int _usedChannelIndex;
        private ActionIndexCache _progressActionIndex;
        private ActionIndexCache _successActionIndex;
        private static readonly ActionIndexCache act_pickup_down_begin = ActionIndexCache.Create("act_pickup_down_begin");
        private static readonly ActionIndexCache act_pickup_down_end = ActionIndexCache.Create("act_pickup_down_end");
        private static readonly ActionIndexCache act_pickup_down_begin_left_stance = ActionIndexCache.Create("act_pickup_down_begin_left_stance");
        private static readonly ActionIndexCache act_pickup_down_end_left_stance = ActionIndexCache.Create("act_pickup_down_end_left_stance");
        private static readonly ActionIndexCache act_pickup_down_left_begin = ActionIndexCache.Create("act_pickup_down_left_begin");
        private static readonly ActionIndexCache act_pickup_down_left_end = ActionIndexCache.Create("act_pickup_down_left_end");
        private static readonly ActionIndexCache act_pickup_down_left_begin_left_stance = ActionIndexCache.Create("act_pickup_down_left_begin_left_stance");
        private static readonly ActionIndexCache act_pickup_down_left_end_left_stance = ActionIndexCache.Create("act_pickup_down_left_end_left_stance");
        private static readonly ActionIndexCache act_pickup_middle_begin = ActionIndexCache.Create("act_pickup_middle_begin");
        private static readonly ActionIndexCache act_pickup_middle_end = ActionIndexCache.Create("act_pickup_middle_end");
        private static readonly ActionIndexCache act_pickup_middle_begin_left_stance = ActionIndexCache.Create("act_pickup_middle_begin_left_stance");
        private static readonly ActionIndexCache act_pickup_middle_end_left_stance = ActionIndexCache.Create("act_pickup_middle_end_left_stance");
        private static readonly ActionIndexCache act_pickup_middle_left_begin = ActionIndexCache.Create("act_pickup_middle_left_begin");
        private static readonly ActionIndexCache act_pickup_middle_left_end = ActionIndexCache.Create("act_pickup_middle_left_end");
        private static readonly ActionIndexCache act_pickup_middle_left_begin_left_stance = ActionIndexCache.Create("act_pickup_middle_left_begin_left_stance");
        private static readonly ActionIndexCache act_pickup_middle_left_end_left_stance = ActionIndexCache.Create("act_pickup_middle_left_end_left_stance");
        private static readonly ActionIndexCache act_pickup_up_begin = ActionIndexCache.Create("act_pickup_up_begin");
        private static readonly ActionIndexCache act_pickup_up_end = ActionIndexCache.Create("act_pickup_up_end");
        private static readonly ActionIndexCache act_pickup_up_begin_left_stance = ActionIndexCache.Create("act_pickup_up_begin_left_stance");
        private static readonly ActionIndexCache act_pickup_up_end_left_stance = ActionIndexCache.Create("act_pickup_up_end_left_stance");
        private static readonly ActionIndexCache act_pickup_up_left_begin = ActionIndexCache.Create("act_pickup_up_left_begin");
        private static readonly ActionIndexCache act_pickup_up_left_end = ActionIndexCache.Create("act_pickup_up_left_end");
        private static readonly ActionIndexCache act_pickup_up_left_begin_left_stance = ActionIndexCache.Create("act_pickup_up_left_begin_left_stance");
        private static readonly ActionIndexCache act_pickup_up_left_end_left_stance = ActionIndexCache.Create("act_pickup_up_left_end_left_stance");
        private static readonly ActionIndexCache act_pickup_from_right_down_horseback_begin = ActionIndexCache.Create("act_pickup_from_right_down_horseback_begin");
        private static readonly ActionIndexCache act_pickup_from_right_down_horseback_end = ActionIndexCache.Create("act_pickup_from_right_down_horseback_end");
        private static readonly ActionIndexCache act_pickup_from_right_down_horseback_left_begin = ActionIndexCache.Create("act_pickup_from_right_down_horseback_left_begin");
        private static readonly ActionIndexCache act_pickup_from_right_down_horseback_left_end = ActionIndexCache.Create("act_pickup_from_right_down_horseback_left_end");
        private static readonly ActionIndexCache act_pickup_from_right_middle_horseback_begin = ActionIndexCache.Create("act_pickup_from_right_middle_horseback_begin");
        private static readonly ActionIndexCache act_pickup_from_right_middle_horseback_end = ActionIndexCache.Create("act_pickup_from_right_middle_horseback_end");
        private static readonly ActionIndexCache act_pickup_from_right_middle_horseback_left_begin = ActionIndexCache.Create("act_pickup_from_right_middle_horseback_left_begin");
        private static readonly ActionIndexCache act_pickup_from_right_middle_horseback_left_end = ActionIndexCache.Create("act_pickup_from_right_middle_horseback_left_end");
        private static readonly ActionIndexCache act_pickup_from_right_up_horseback_begin = ActionIndexCache.Create("act_pickup_from_right_up_horseback_begin");
        private static readonly ActionIndexCache act_pickup_from_right_up_horseback_end = ActionIndexCache.Create("act_pickup_from_right_up_horseback_end");
        private static readonly ActionIndexCache act_pickup_from_right_up_horseback_left_begin = ActionIndexCache.Create("act_pickup_from_right_up_horseback_left_begin");
        private static readonly ActionIndexCache act_pickup_from_right_up_horseback_left_end = ActionIndexCache.Create("act_pickup_from_right_up_horseback_left_end");
        private static readonly ActionIndexCache act_pickup_from_left_down_horseback_begin = ActionIndexCache.Create("act_pickup_from_left_down_horseback_begin");
        private static readonly ActionIndexCache act_pickup_from_left_down_horseback_end = ActionIndexCache.Create("act_pickup_from_left_down_horseback_end");
        private static readonly ActionIndexCache act_pickup_from_left_down_horseback_left_begin = ActionIndexCache.Create("act_pickup_from_left_down_horseback_left_begin");
        private static readonly ActionIndexCache act_pickup_from_left_down_horseback_left_end = ActionIndexCache.Create("act_pickup_from_left_down_horseback_left_end");
        private static readonly ActionIndexCache act_pickup_from_left_middle_horseback_begin = ActionIndexCache.Create("act_pickup_from_left_middle_horseback_begin");
        private static readonly ActionIndexCache act_pickup_from_left_middle_horseback_end = ActionIndexCache.Create("act_pickup_from_left_middle_horseback_end");
        private static readonly ActionIndexCache act_pickup_from_left_middle_horseback_left_begin = ActionIndexCache.Create("act_pickup_from_left_middle_horseback_left_begin");
        private static readonly ActionIndexCache act_pickup_from_left_middle_horseback_left_end = ActionIndexCache.Create("act_pickup_from_left_middle_horseback_left_end");
        private static readonly ActionIndexCache act_pickup_from_left_up_horseback_begin = ActionIndexCache.Create("act_pickup_from_left_up_horseback_begin");
        private static readonly ActionIndexCache act_pickup_from_left_up_horseback_end = ActionIndexCache.Create("act_pickup_from_left_up_horseback_end");
        private static readonly ActionIndexCache act_pickup_from_left_up_horseback_left_begin = ActionIndexCache.Create("act_pickup_from_left_up_horseback_left_begin");
        private static readonly ActionIndexCache act_pickup_from_left_up_horseback_left_end = ActionIndexCache.Create("act_pickup_from_left_up_horseback_left_end");
        public override bool LockUserFrames { get => false; }
        public override bool LockUserPositions { get => false; }
        private WeakGameEntity _weakGameEntity;

        protected override void OnInit()
        {
            base.OnInit();

            _weakGameEntity = GameEntity;

            ActionMessage = new TextObject("Money Bag");
            TextObject descriptionMessage = new TextObject("Press {KEY} To Loot");
            descriptionMessage.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
            DescriptionMessage = descriptionMessage;
        }
        public override TextObject GetDescriptionText(WeakGameEntity gameEntity)
        {
            return new TextObject("Money Bag");
        }

        public override ScriptComponentBehavior.TickRequirement GetTickRequirement()
        {
            //if (GameNetwork.IsServer && base.HasUser)
            //{
            //    return base.GetTickRequirement() | ScriptComponentBehavior.TickRequirement.Tick | ScriptComponentBehavior.TickRequirement.TickParallel2;
            //}
#if SERVER
            if (HasUser)
            {
                return base.GetTickRequirement() | ScriptComponentBehavior.TickRequirement.Tick;
            }
#endif
            return base.GetTickRequirement();
        }

        protected override void OnTick(float dt)
        {
            base.OnTick(dt);
            DoTick(dt);
        }

        protected void DoTick(float dt)
        {
#if SERVER
            if (HasUser)
            {
                ActionIndexCache currentAction = UserAgent.GetCurrentAction(_usedChannelIndex);
                if (currentAction == _successActionIndex)
                {
                    UserAgent.StopUsingGameObjectMT(UserAgent.CanUseObject(this));
                    GetTickRequirement();
                }
                else if (currentAction != this._progressActionIndex)
                {
                    UserAgent.StopUsingGameObjectMT(false);
                    GetTickRequirement();
                }
            }
#endif
        }

        public void SetAmount(int amount)
        {
            _amount = amount;
        }

        public int GetAmount()
        {
            return _amount;
        }

        public override void OnUse(Agent userAgent, sbyte agentBoneIndex)
        {
            if (HasUser)
            {
                return;
            }

            base.OnUse(userAgent, agentBoneIndex);

            // userAgent.StopUsingGameObjectMT(true, true, false);
            if (GameNetwork.IsServer)
            {
                Debug.Print("[USING LOG] AGENT USE " + this.GetType().Name);

                if (_weakGameEntity.TryGetEntity(out var tmpGameEntity))
                {
                    var globalFrame = tmpGameEntity.GetGlobalFrame();
                    var num = globalFrame.origin.z;

                    var eyeGlobalHeight = userAgent.GetEyeGlobalHeight();
                    var isLeftStance = userAgent.GetIsLeftStance();

                    if (userAgent.HasMount)
                    {
                        var frame = userAgent.Frame;
                        var flag = Vec2.DotProduct(frame.rotation.f.AsVec2.LeftVec(), (tmpGameEntity.GetGlobalFrame().origin - frame.origin).AsVec2) > 0f;

                        _usedChannelIndex = 1;

                        if (num < eyeGlobalHeight * 0.7f + userAgent.Position.z)
                        {
                            _progressActionIndex = (flag ? PE_MoneyBag.act_pickup_from_left_down_horseback_begin : PE_MoneyBag.act_pickup_from_right_down_horseback_begin);
                            _successActionIndex = (flag ? PE_MoneyBag.act_pickup_from_left_down_horseback_end : PE_MoneyBag.act_pickup_from_right_down_horseback_end);
                        }
                        else if (num < eyeGlobalHeight * 1.1f + userAgent.Position.z)
                        {
                            _progressActionIndex = (flag ? PE_MoneyBag.act_pickup_from_left_middle_horseback_begin : PE_MoneyBag.act_pickup_from_right_middle_horseback_begin);
                            _successActionIndex = (flag ? PE_MoneyBag.act_pickup_from_left_middle_horseback_end : PE_MoneyBag.act_pickup_from_right_middle_horseback_end);
                        }
                        else
                        {
                            _progressActionIndex = (flag ? PE_MoneyBag.act_pickup_from_left_up_horseback_begin : PE_MoneyBag.act_pickup_from_right_up_horseback_begin);
                            _successActionIndex = (flag ? PE_MoneyBag.act_pickup_from_left_up_horseback_end : PE_MoneyBag.act_pickup_from_right_up_horseback_end);
                        }
                    }
                    else if (num < eyeGlobalHeight * 0.4f + userAgent.Position.z)
                    {
                        _usedChannelIndex = 0;
                        _progressActionIndex = (isLeftStance ? PE_MoneyBag.act_pickup_down_begin_left_stance : PE_MoneyBag.act_pickup_down_begin);
                        _successActionIndex = (isLeftStance ? PE_MoneyBag.act_pickup_down_end_left_stance : PE_MoneyBag.act_pickup_down_end);

                    }
                    else if (num < eyeGlobalHeight * 1.1f + userAgent.Position.z)
                    {
                        _usedChannelIndex = 1;
                        _progressActionIndex = (isLeftStance ? PE_MoneyBag.act_pickup_middle_begin_left_stance : PE_MoneyBag.act_pickup_middle_begin);
                        _successActionIndex = (isLeftStance ? PE_MoneyBag.act_pickup_middle_end_left_stance : PE_MoneyBag.act_pickup_middle_end);
                    }
                    else
                    {
                        _usedChannelIndex = 1;
                        _progressActionIndex = (isLeftStance ? PE_MoneyBag.act_pickup_up_begin_left_stance : PE_MoneyBag.act_pickup_up_begin);
                        _successActionIndex = (isLeftStance ? PE_MoneyBag.act_pickup_up_end_left_stance : PE_MoneyBag.act_pickup_up_end);
                    }
                    userAgent.SetActionChannel(_usedChannelIndex, _progressActionIndex, false, 0UL, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
                }
            }
        }

        public override void OnUseStopped(Agent userAgent, bool isSuccessful, int preferenceIndex)
        {
            base.OnUseStopped(userAgent, isSuccessful, preferenceIndex);
            Debug.Print("[USING LOG] AGENT USE STOPPED " + GetType().Name);

            if (isSuccessful)
            {
                if (GameNetwork.IsServer)
                {
                    PersistentEmpireRepresentative representative = userAgent.MissionPeer.GetNetworkPeer().GetComponent<PersistentEmpireRepresentative>();
                    representative.GoldGain(_amount);
                    LoggerHelper.LogAnAction(userAgent.MissionPeer.GetNetworkPeer(), LogAction.PlayerPickedUpGold, null, new object[] { _amount });
                    // Mission.Current.MakeSound(SoundEvent.GetEventIdFromString("event:/ui/notification/coins_positive"), userAgent.Frame.origin, false, true, -1, -1);
                }
                Remove(80);
            }
        }

        internal void Remove(int reason)
        {
            var myTrace = new System.Diagnostics.StackTrace(0, true);

            try
            {
                if (_weakGameEntity.TryGetEntity(out var tmpGameEntity))
                {
                    Mission.Current.Scene.RemoveEntity(tmpGameEntity, reason);
                    //tmpGameEntity.Remove(80);
                }
            }
            catch (Exception ex)
            {
                SaveSystemBehavior.RglExceptionThrown(myTrace, ex);
            }
        }
    }
}