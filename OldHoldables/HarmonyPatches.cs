using HarmonyLib;
using System;
using System.Reflection;


namespace OldHoldables
{
    public class HarmonyPatches
    {
        private static Harmony instance;

        public static bool IsPatched { get; private set; }
        public const string InstanceId = PluginInfo.GUID;
        public bool HoldablesAreSticky = false;

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
            static void Postfix(ref bool __result)
            {
                if (__result == true)
                {
                    __result = false;
                }
            }
        }

        [HarmonyPatch(typeof(EquipmentInteractor), "GetIsHolding")]
        class RopeBombsYouWantItItsYoursMyFriendAsLongAsYouHaveEnoughRupies
        {
            static void Postfix(ref bool __result)
            {
                if (__result == true)
                {
                    __result = false;
                }
            }

        }

    }
}
