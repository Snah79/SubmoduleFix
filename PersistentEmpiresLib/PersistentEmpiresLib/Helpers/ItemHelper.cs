using NetworkMessages.FromServer;
using System;
using System.Linq;
using System.Reflection;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.Helpers
{
    public static class ItemHelper
    {
        public static int GetMaximumAmmo(ItemObject item)
        {
            int ammo = 0;
            if (item != null && item.Weapons != null)
            {
                foreach (WeaponComponentData weaponComponentData in item.Weapons)
                {
                    bool isConsumable = weaponComponentData.IsConsumable;
                    if (isConsumable || weaponComponentData.IsRangedWeapon || weaponComponentData.WeaponFlags.HasAnyFlag(WeaponFlags.HasHitPoints))
                    {
                        ammo = weaponComponentData.MaxDataValue;
                    }
                }
            }
            return ammo;
        }

        public static bool IsWeaponComparableWithUsage(ItemObject item, string comparedUsageId)
        {
            for (int i = 0; i < item.Weapons.Count; i++)
            {
                if (item.Weapons[i].WeaponDescriptionId == comparedUsageId
                    || (comparedUsageId == "OneHandedBastardSword" && item.Weapons[i].WeaponDescriptionId == "OneHandedSword")
                    || (comparedUsageId == "OneHandedSword" && item.Weapons[i].WeaponDescriptionId == "OneHandedBastardSword"))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsWeaponComparableWithUsage(ItemObject item, string comparedUsageId, out int comparableUsageIndex)
        {
            comparableUsageIndex = -1;
            for (int i = 0; i < item.Weapons.Count; i++)
            {
                if (item.Weapons[i].WeaponDescriptionId == comparedUsageId
                    || (comparedUsageId == "OneHandedBastardSword" && item.Weapons[i].WeaponDescriptionId == "OneHandedSword")
                    || (comparedUsageId == "OneHandedSword" && item.Weapons[i].WeaponDescriptionId == "OneHandedBastardSword"))
                {
                    comparableUsageIndex = i;
                    return true;
                }
            }
            return false;
        }

        public static bool CheckComparability(ItemObject item, ItemObject comparedItem)
        {
            if (item == null || comparedItem == null)
                return false;

            if (item.PrimaryWeapon != null && comparedItem.PrimaryWeapon != null &&
                ((item.PrimaryWeapon.IsMeleeWeapon && comparedItem.PrimaryWeapon.IsMeleeWeapon)
                 || (item.PrimaryWeapon.IsRangedWeapon && item.PrimaryWeapon.IsConsumable && comparedItem.PrimaryWeapon.IsRangedWeapon && comparedItem.PrimaryWeapon.IsConsumable)
                 || (!item.PrimaryWeapon.IsRangedWeapon && item.PrimaryWeapon.IsConsumable && !comparedItem.PrimaryWeapon.IsRangedWeapon && comparedItem.PrimaryWeapon.IsConsumable)
                 || (item.PrimaryWeapon.IsShield && comparedItem.PrimaryWeapon.IsShield)))
            {
                WeaponComponentData primaryWeapon = item.PrimaryWeapon;
                return ItemHelper.IsWeaponComparableWithUsage(comparedItem, primaryWeapon.WeaponDescriptionId);
            }

            return item.Type == comparedItem.Type;
        }

        public static bool CheckComparability(ItemObject item, ItemObject comparedItem, int usageIndex)
        {
            if (item == null || comparedItem == null)
                return false;

            if (item.PrimaryWeapon != null &&
                ((item.PrimaryWeapon.IsMeleeWeapon && comparedItem.PrimaryWeapon.IsMeleeWeapon)
                 || (item.PrimaryWeapon.IsRangedWeapon && item.PrimaryWeapon.IsConsumable && comparedItem.PrimaryWeapon.IsRangedWeapon && comparedItem.PrimaryWeapon.IsConsumable)
                 || (!item.PrimaryWeapon.IsRangedWeapon && item.PrimaryWeapon.IsConsumable && !comparedItem.PrimaryWeapon.IsRangedWeapon && comparedItem.PrimaryWeapon.IsConsumable)
                 || (item.PrimaryWeapon.IsShield && comparedItem.PrimaryWeapon.IsShield)))
            {
                WeaponComponentData weaponComponentData = item.Weapons[usageIndex];
                return ItemHelper.IsWeaponComparableWithUsage(comparedItem, weaponComponentData.WeaponDescriptionId);
            }

            return item.Type == comparedItem.Type;
        }

        private static TextObject GetDamageDescription(int damage, DamageTypes damageType)
        {
            TextObject textObject = new TextObject("{=vvCwVo7i}{DAMAGE} {DAMAGE_TYPE}", null);
            textObject.SetTextVariable("DAMAGE", damage);
            textObject.SetTextVariable("DAMAGE_TYPE", GameTexts.FindText("str_damage_types", damageType.ToString()));
            return textObject;
        }

        public static TextObject GetSwingDamageText(WeaponComponentData weapon, ItemModifier itemModifier)
        {
            int modifiedSwingDamage = weapon.GetModifiedSwingDamage(itemModifier);
            DamageTypes swingDamageType = weapon.SwingDamageType;
            return ItemHelper.GetDamageDescription(modifiedSwingDamage, swingDamageType);
        }

        public static TextObject GetMissileDamageText(WeaponComponentData weapon, ItemModifier itemModifier)
        {
            int modifiedMissileDamage = weapon.GetModifiedMissileDamage(itemModifier);
            DamageTypes damageType = (weapon.WeaponClass == WeaponClass.ThrowingAxe) ? weapon.SwingDamageType : weapon.ThrustDamageType;
            return ItemHelper.GetDamageDescription(modifiedMissileDamage, damageType);
        }

        public static TextObject GetThrustDamageText(WeaponComponentData weapon, ItemModifier itemModifier)
        {
            int modifiedThrustDamage = weapon.GetModifiedThrustDamage(itemModifier);
            DamageTypes thrustDamageType = weapon.ThrustDamageType;
            return ItemHelper.GetDamageDescription(modifiedThrustDamage, thrustDamageType);
        }

        public static TextObject NumberOfItems(int number, ItemObject item)
        {
            TextObject textObject = new TextObject("{=siWNDxgo}{.%}{?NUMBER_OF_ITEM > 1}{NUMBER_OF_ITEM} {PLURAL(ITEM)}{?}one {ITEM}{\\?}{.%}", null);
            textObject.SetTextVariable("ITEM", item.Name);
            textObject.SetTextVariable("NUMBER_OF_ITEM", number);
            return textObject;
        }

        // -----------------------------
        // FIX: SpawnWeaponAux overload-safe invoke
        // -----------------------------
        private static MethodInfo _spawnWeaponAuxMI;

        private static MethodInfo GetSpawnWeaponAuxMethod()
        {
            if (_spawnWeaponAuxMI != null)
                return _spawnWeaponAuxMI;

            var methods = typeof(Mission)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.Name == "SpawnWeaponAux")
                .ToArray();

            foreach (var m in methods)
            {
                var ps = m.GetParameters();
                if (ps.Length < 5) continue;

                bool p0ok = ps[0].ParameterType == typeof(GameEntity) || ps[0].ParameterType == typeof(WeakGameEntity);

                bool p1ok =
                    ps[1].ParameterType == typeof(MissionWeapon) ||
                    (ps[1].ParameterType.IsByRef && ps[1].ParameterType.GetElementType() == typeof(MissionWeapon));

                bool p2ok = ps[2].ParameterType == typeof(Mission.WeaponSpawnFlags);
                bool p3ok = ps[3].ParameterType == typeof(Vec3);
                bool p4ok = ps[4].ParameterType == typeof(Vec3);

                if (p0ok && p1ok && p2ok && p3ok && p4ok)
                {
                    _spawnWeaponAuxMI = m;
                    return _spawnWeaponAuxMI;
                }
            }

            var sigs = string.Join(" | ", methods.Select(x =>
            {
                var ps = x.GetParameters();
                return $"{x.Name}({string.Join(", ", ps.Select(p => p.ParameterType.Name))})";
            }));

            throw new MissingMethodException($"Could not find compatible Mission.SpawnWeaponAux overload. Found: {sigs}");
        }

        private static object GetDefaultValue(Type t)
        {
            return t.IsValueType ? Activator.CreateInstance(t) : null;
        }

        private static object ConvertWeakToExpectedEntity(object entity, Type expectedType)
        {
            if (entity == null) return null;

            if (expectedType.IsInstanceOfType(entity))
                return entity;

            // expected GameEntity but got WeakGameEntity
            if (expectedType == typeof(GameEntity) && entity is WeakGameEntity)
            {
                // Try WeakGameEntity.GetEntity() (name may vary by version; reflection makes it safer)
                var mi = typeof(WeakGameEntity).GetMethod("GetEntity", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (mi != null)
                {
                    var ge = mi.Invoke(entity, null);
                    if (ge != null && expectedType.IsInstanceOfType(ge))
                        return ge;
                }
            }

            throw new InvalidCastException($"Cannot convert entity argument of type {entity.GetType().FullName} to expected {expectedType.FullName}");
        }

        private static void InvokeSpawnWeaponAux(object spawnedEntity, MissionWeapon weapon, Mission.WeaponSpawnFlags flags, bool hasLifeTime)
        {
            var mi = GetSpawnWeaponAuxMethod();
            var ps = mi.GetParameters();

            Vec3 zero = Vec3.Zero;

            object entityArg = ConvertWeakToExpectedEntity(spawnedEntity, ps[0].ParameterType);

            object[] args;

            if (ps.Length == 5)
            {
                args = new object[] { entityArg, weapon, flags, zero, zero };
            }
            else if (ps.Length == 6)
            {
                args = new object[] { entityArg, weapon, flags, zero, zero, hasLifeTime };
            }
            else
            {
                args = new object[ps.Length];
                args[0] = entityArg;
                args[1] = weapon;
                args[2] = flags;
                args[3] = zero;
                args[4] = zero;

                // If param[5] exists and is bool, map hasLifeTime
                if (ps.Length >= 6)
                    args[5] = (ps[5].ParameterType == typeof(bool)) ? (object)hasLifeTime : GetDefaultValue(ps[5].ParameterType);

                for (int i = 6; i < ps.Length; i++)
                    args[i] = GetDefaultValue(ps[i].ParameterType);
            }

            // handle ref MissionWeapon overloads
            if (ps[1].ParameterType.IsByRef)
                args[1] = weapon;

            mi.Invoke(Mission.Current, args);
        }

        // -----------------------------
        // Your existing method + fixed invoke
        // -----------------------------
        public static WeakGameEntity SpawnWeaponWithNewEntityAux(Scene scene, MissionWeapon weapon, Mission.WeaponSpawnFlags spawnFlags, MatrixFrame frame, int forcedSpawnIndex, MissionObject attachedMissionObject, bool hasLifeTime)
        {
            var tmp = GameEntityExtensions.Instantiate(scene, weapon, spawnFlags.HasAnyFlag(Mission.WeaponSpawnFlags.WithHolster), true);
            var gameEntity = tmp.WeakEntity;

            gameEntity.CreateAndAddScriptComponent(typeof(SpawnedItemEntity).Name, true);
            SpawnedItemEntity firstScriptOfType = gameEntity.GetFirstScriptOfType<SpawnedItemEntity>();

            if (forcedSpawnIndex >= 0)
            {
                firstScriptOfType.Id = new MissionObjectId(forcedSpawnIndex, true);
            }

            if (attachedMissionObject != null)
            {
                attachedMissionObject.GameEntity.AddChild(gameEntity, false);
            }

            if (attachedMissionObject != null)
            {
                MatrixFrame matrixFrame = attachedMissionObject.GameEntity.GetGlobalFrame();
                matrixFrame = matrixFrame.TransformToParent(frame);
                gameEntity.SetGlobalFrame(matrixFrame);
            }
            else
            {
                gameEntity.SetGlobalFrame(frame);
            }

            if (GameNetwork.IsServerOrRecorder)
            {
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new SpawnWeaponWithNewEntity(
                    weapon, spawnFlags, firstScriptOfType.Id.Id, frame,
                    attachedMissionObject == null ? MissionObjectId.Invalid : attachedMissionObject.Id,
                    true, hasLifeTime, false
                ));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord, null);

                for (int i = 0; i < weapon.GetAttachedWeaponsCount(); i++)
                {
                    GameNetwork.BeginBroadcastModuleEvent();
                    GameNetwork.WriteMessage(new AttachWeaponToSpawnedWeapon(
                        weapon.GetAttachedWeapon(i), firstScriptOfType.Id, weapon.GetAttachedWeaponFrame(i)
                    ));
                    GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord, null);
                }
            }

            // ✅ FIXED: version-safe reflection invoke (no parameter count mismatch)
            InvokeSpawnWeaponAux(gameEntity, weapon, spawnFlags, hasLifeTime);

            return gameEntity;
        }
    }
}
