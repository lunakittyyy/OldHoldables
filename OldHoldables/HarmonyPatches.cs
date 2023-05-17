using GorillaNetworking;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;


namespace OldHoldables
{
    public class HarmonyPatches
    {
        private static Harmony instance;

        public static bool IsPatched { get; private set; }
        public static bool SetGoingToChange { get; set; }

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

        [HarmonyPatch(typeof(TransferrableObject), "IsHeld")]
        class HoldingPatch
        {
            static bool Prefix(ref bool __result)
            {
                if (!SetGoingToChange)
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(EquipmentInteractor), "GetIsHolding")]
        class RopeHoldingPatch
        {
            static bool Prefix(ref bool __result)
            {
                if (!SetGoingToChange)
                {
                    __result = false;
                    return false;
                }
                return true;
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
