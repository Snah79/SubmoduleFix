//#if SERVER
//using PERoleplay.MissionBehaviors;
//using TaleWorlds.MountAndBlade;
//using PERoleplaySave;
//#endif
//#if CLIENT
//using TaleWorlds.Core;
//using TaleWorlds.InputSystem;
//using TaleWorlds.Localization;
//#endif
//using System;
//using PersistentEmpiresLib.SceneScripts;
//using TaleWorlds.Engine;
//using System.Collections.Generic;
//using System.Linq;

//namespace PERoleplay.SceneScripts
//{
//    public class PE_Lock_Gate : PE_Gate
//    {
//        public int LockId;
//        public bool IsLocked = true;
//        public string Description;

//        public PE_Lock_Gate() : base()
//        {
//        }

//        protected override void OnInit()
//        {
//            base.OnInit();
//#if SERVER
//            try
//            {
//                //PERoleplaySubModule.DebugPrint($"Rp_Lock_PE_Gate OnInit", PERoleplaySubModule._Configuration.DebugLockBehavior);
//                LockBehavior.RegisterLock(this);
//            }
//            catch (Exception ex)
//            {
//                DbHandler.LogException(ex);
//            }
//#endif
//#if CLIENT
//            try
//            {
//                if (string.IsNullOrEmpty(Description))
//                    base.ActionMessage = new TextObject($"Door ({LockId})");
//                else
//                    base.ActionMessage = new TextObject($"{Description}");

//                TextObject descriptionMessage = new TextObject("Press {KEY} to lock/unlock door");
//                descriptionMessage.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
//                base.DescriptionMessage = descriptionMessage;
//            }
//            catch (Exception ex)
//            {

//            }
//#endif
//        }

//#if CLIENT
//        protected override void OnEditorInit()
//        {
//            if (LockId == 0)
//            {
//                LockId = GenerateId();
//            }
//        }

//        private int GenerateId()
//        {
//            List<GameEntity> reference1 = new List<GameEntity>();
//            Scene.GetAllEntitiesWithScriptComponent<PE_Lock_Gate>(ref reference1);
//            //List<GameEntity> reference2 = new List<GameEntity>();
//            //Scene.GetAllEntitiesWithScriptComponent<Rp_Lock_PE_LiftingDoor>(ref reference2);
//            //List<GameEntity> reference3 = new List<GameEntity>();
//            //Scene.GetAllEntitiesWithScriptComponent<Rp_Lock_PE_Gate>(ref reference3);

//            List<int> reference = new List<int>();
//            reference.AddRange(reference1.Select(r => r.GetFirstScriptOfType<PE_Lock_Gate>().LockId));
//            //reference.AddRange(reference2.Select(r => r.GetFirstScriptOfType<Rp_Lock_PE_LiftingDoor>().LockId));
//            //reference.AddRange(reference3.Select(r => r.GetFirstScriptOfType<Rp_Lock_PE_Gate>().LockId));
//            var ids = reference.OrderByDescending(r => r);
//            var idMax = ids.FirstOrDefault();

//            return ++idMax;
//        }
//#endif

//#if SERVER
//        public override bool IsDisabledForAgent(Agent agent)
//        {
//            if (IsLocked)
//            {
//                if (string.IsNullOrEmpty(ItemToOpen) || agent.WieldedWeapon.Item?.StringId == ItemToOpen || agent.WieldedWeapon.Item?.StringId == "RP_admin_keys")
//                {
//                    return false;
//                }

//                LockBehavior.InformClientAccessDenied(this, agent);
//            }

//            return IsLocked;
//        }

//        public override void OnUse(Agent agent)
//        {
//            if (string.IsNullOrEmpty(ItemToOpen) || agent.WieldedWeapon.Item?.StringId == ItemToOpen || agent.WieldedWeapon.Item?.StringId == "RP_admin_keys")
//            {
//                if (IsLocked)
//                {
//                    LockBehavior.TryToUnlock(this, agent);                    
//                    agent.StopUsingGameObjectMT(true);

//                    FindLock();
//                    if (_lock != null)
//                    {
//                        _lock.SetVisibleSynched(IsLocked, false);
//                    }

//                    return;
//                }
//                else
//                {
//                    // dont lock open doors
//                    if (this.isOpen)
//                    {
//                        base.OnUse(agent);
//                    }
//                    else
//                    {
//                        LockBehavior.TryToLock(this, agent);
//                        agent.StopUsingGameObjectMT(true);

//                        FindLock();
//                        if (_lock != null)
//                        {
//                            _lock.SetVisibleSynched(IsLocked, false);
//                        }

//                        return;
//                    }
//                }
//            }
//            else
//            {
//                base.OnUse(agent);
//            }
//        }
//#endif
//    }
//}