using GorillaNetworking;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using UnityEngine.XR;

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
                if (__instance.TryGetComponent(out Slingshot _) || __instance.TryGetComponent(out ThrowableBug _) || __instance.TryGetComponent(out ThrowableSetDressing _)) return true;
                else
                {
                    if (!SetGoingToChange) __result = false;
                    return SetGoingToChange;
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

        [HarmonyPatch(typeof(CosmeticsController), "ApplyCosmeticItemToSet")]
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