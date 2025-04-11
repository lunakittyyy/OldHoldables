using System;
using System.Collections.Generic;
using GorillaNetworking;
using GorillaTagScripts;
using HarmonyLib;
using System.Reflection;

// TODO: Limit classes, although not totally necessary atm.
namespace OldHoldables
{
    public class HarmonyPatches
    {
        private static Harmony instance;

        public static bool IsPatched { get; private set; }

        public const string InstanceId = PluginInfo.GUID;

        internal static void ApplyHarmonyPatches()
        {
            if (!IsPatched)
            {
                if (instance == null)
                {
                    instance = new Harmony(InstanceId);
                }

                instance.PatchAll(Assembly.GetExecutingAssembly());
                IsPatched = true;
            }
        }

        internal static void RemoveHarmonyPatches()
        {
            if (instance != null && IsPatched)
            {
                instance.UnpatchSelf();
                IsPatched = false;
            }
        }

        public static bool SetGoingToChange;

        public static bool HoldQueue;
        public static bool HoverLeft;

        [HarmonyPatch(typeof(TransferrableObject), "IsHeld")]
        class HoldingPatch
        {
            static bool Prefix(TransferrableObject __instance, ref bool __result)
            {
                if (__instance.TryGetComponent(out Slingshot _) ||
                    __instance.TryGetComponent(out ThrowableBug _) ||
                    __instance.TryGetComponent(out ThrowableSetDressing _) ||
                    __instance.TryGetComponent(out DecorativeItem _)) return true;
                if (!SetGoingToChange) __result = false;
                return SetGoingToChange;
            }
        }

        [HarmonyPatch(typeof(TransferrableObject), "OnEnable")]
        class CosSpawnPatch
        {
            static void Postfix(TransferrableObject __instance)
            {
                if (!__instance.TryGetComponent(out Slingshot _) ||
                    !__instance.TryGetComponent(out ThrowableBug _) ||
                    !__instance.TryGetComponent(out ThrowableSetDressing _) ||
                    !__instance.TryGetComponent(out DecorativeItem _))
                {

                    if (__instance.IsMyItem())
                    {
                        if (__instance.currentState == TransferrableObject.PositionState.OnLeftArm)
                        {
                            __instance.currentState = TransferrableObject.PositionState.InRightHand;
                        }
                        if (__instance.currentState == TransferrableObject.PositionState.OnRightArm)
                        {
                            __instance.currentState = TransferrableObject.PositionState.InLeftHand;
                        }
                    }
                }
            }
        }


        /* TODO: Fix old auto-dropping code
         * It's very broken now and I don't have the desire to fix it right now
         * Slingshots should still be blacklisted but it
         * probably won't automatically drop anything else
         */

        [HarmonyPatch(typeof(EquipmentInteractor), "GetIsHolding")]
        class RopeHoldingPatch
        {
            static bool Prefix(ref bool __result)
            {
                if (!SetGoingToChange) __result = false;
                return SetGoingToChange;
            }
        }

        [HarmonyPatch(typeof(CosmeticsController), "ApplyCosmeticItemToSet", new Type[] { typeof(CosmeticsController.CosmeticSet), typeof(CosmeticsController.CosmeticItem), typeof(bool), typeof(bool), typeof(List<CosmeticsController.CosmeticSlots>) })]
        class ReleaseBeforeChangingHoldables
        {
            static void Prefix()
            {
                SetGoingToChange = true;
                EquipmentInteractor.instance.ReleaseLeftHand();
                EquipmentInteractor.instance.ReleaseRightHand();
            }

            static void Postfix()
            {
                SetGoingToChange = false;
            }
        }
    }
}