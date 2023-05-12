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
                __result = false;
                return false;
            }
        }

        [HarmonyPatch(typeof(EquipmentInteractor), "GetIsHolding")]
        class RopeHoldingPatch
        {
            static bool Prefix(ref bool __result)
            {
                __result = false;
                return false;
            }
        }
        /*
        [HarmonyPatch(typeof(CosmeticsController), "ApplyCosmeticItemToSet")]
        class UnpatchBeforeChangingHoldables
        {
            MethodInfo OriginalHolding = typeof(TransferrableObject).GetMethod("IsHeld");
            MethodInfo OriginalRopeHolding = typeof(EquipmentInteractor).GetMethod("GetIsHolding");
            void Prefix()
            {
                instance.Unpatch(OriginalHolding, HarmonyPatchType.Prefix);
                instance.Unpatch(OriginalRopeHolding, HarmonyPatchType.Prefix);
            }

            void Postfix()
            {
                if (IsPatched)
                {
                    instance.PatchAll(typeof(HoldingPatch));
                    instance.PatchAll(typeof(RopeHoldingPatch));
                }
            }
        }
        */
    }
}
