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

        public static bool SetGoingToChange { get; set; }
        public static bool HoldQueue { get; set; }
        public static bool HoverLeft { get; set; }

        [HarmonyPatch(typeof(TransferrableObject), "IsHeld")]
        class HoldingPatch
        {
            static bool Prefix(TransferrableObject __instance, ref bool __result)
            {
                if (__instance.TryGetComponent(out Slingshot _)) return true;
                else
                {
                    if (!SetGoingToChange) __result = false;
                    return SetGoingToChange;
                }
            }
        }

        [HarmonyPatch(typeof(EquipmentInteractor), "FireHandInteractions")]
        class HoverFixPatch
        {
            public static void Prefix(EquipmentInteractor __instance, GameObject interactingHand, bool isLeftHand)
            {
                if (__instance.leftHandHeldEquipment != null && __instance.rightHandHeldEquipment != null)
                {
                    foreach (InteractionPoint item in isLeftHand ? __instance.overlapInteractionPointsLeft : __instance.overlapInteractionPointsRight)
                    {
                        if (item != null && item.parentTransferrableObject != null)
                        {
                            bool num = item.parentTransferrableObject.GetComponent<Slingshot>() == null;
                            bool flag = item.parentTransferrableObject == __instance.leftHandHeldEquipment || item.parentTransferrableObject == __instance.rightHandHeldEquipment;

                            float leftValue = ControllerInputPoller.GripFloat(XRNode.LeftHand);
                            float tempValue = ControllerInputPoller.TriggerFloat(XRNode.LeftHand);
                            leftValue = Mathf.Max(leftValue, tempValue);
                            float rightValue = ControllerInputPoller.GripFloat(XRNode.RightHand);
                            tempValue = ControllerInputPoller.TriggerFloat(XRNode.RightHand);
                            rightValue = Mathf.Max(rightValue, tempValue);
                            bool canGrab = (leftValue > __instance.grabThreshold - __instance.grabHysteresis) || (rightValue > __instance.grabThreshold - __instance.grabHysteresis);
                            if (!num && canGrab)
                            {
                                HoldQueue = true;
                                HoverLeft = flag ? !interactingHand == EquipmentInteractor.instance.leftHand : interactingHand == EquipmentInteractor.instance.leftHand;
                                item.parentTransferrableObject.OnHover(item, interactingHand);
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(TransferrableObject), "OnHover")]
        class HoverPatch
        {
            public static void Prefix(TransferrableObject __instance, GameObject hoveringHand)
            {
                if (__instance.IsMyItem() && __instance.TryGetComponent(out Slingshot _) && !__instance.InHand())
                {
                    SetGoingToChange = true;

                    if (HoverLeft) EquipmentInteractor.instance.ReleaseLeftHand();
                    else EquipmentInteractor.instance.ReleaseRightHand();

                    float leftValue = ControllerInputPoller.GripFloat(XRNode.LeftHand);
                    float tempValue = ControllerInputPoller.TriggerFloat(XRNode.LeftHand);
                    leftValue = Mathf.Max(leftValue, tempValue);
                    float rightValue = ControllerInputPoller.GripFloat(XRNode.RightHand);
                    tempValue = ControllerInputPoller.TriggerFloat(XRNode.RightHand);
                    rightValue = Mathf.Max(rightValue, tempValue);
                    EquipmentInteractor interactor = EquipmentInteractor.instance;
                    bool canGrab = (leftValue > interactor.grabThreshold - interactor.grabHysteresis) || (rightValue > interactor.grabThreshold - interactor.grabHysteresis);
                    if (canGrab && HoldQueue)
                    {
                        HoldQueue = false;
                        __instance.OnGrab(__instance.gripInteractor, HoverLeft ? EquipmentInteractor.instance.leftHand : EquipmentInteractor.instance.rightHand);
                    }
                }
                else if (__instance.IsMyItem() && __instance.TryGetComponent(out Slingshot _) && __instance.InHand())
                {
                    bool pickUpLeft = hoveringHand == EquipmentInteractor.instance.leftHand;
                    if (pickUpLeft && __instance.InRightHand())
                    {
                        SetGoingToChange = true;
                        EquipmentInteractor.instance.ReleaseLeftHand();
                    }
                    else if (!pickUpLeft && __instance.InLeftHand())
                    {
                        SetGoingToChange = true;
                        EquipmentInteractor.instance.ReleaseRightHand();
                    }
                }
            }

            public static void Postfix(TransferrableObject __instance)
            {
                if (__instance.IsMyItem() && __instance.TryGetComponent(out Slingshot _) && !__instance.InHand())
                {
                    SetGoingToChange = false;
                }
            }
        }

        [HarmonyPatch(typeof(EquipmentInteractor), "ReleaseLeftHand")]
        class ReleaseLeftPatch
        {
            public static void Prefix(EquipmentInteractor __instance)
            {
                if (__instance.rightHandHeldEquipment != null)
                {
                    TransferrableObject transferrableObject = __instance.rightHandHeldEquipment.GetComponent<TransferrableObject>();
                    if (transferrableObject != null && transferrableObject.InLeftHand() && transferrableObject.TryGetComponent(out Slingshot _)) HoldQueue = false;
                }
                if (__instance.leftHandHeldEquipment != null)
                {
                    TransferrableObject transferrableObject = __instance.leftHandHeldEquipment.GetComponent<TransferrableObject>();
                    if (transferrableObject != null && transferrableObject.InLeftHand() && transferrableObject.TryGetComponent(out Slingshot _)) HoldQueue = false;
                }
            }
        }

        [HarmonyPatch(typeof(EquipmentInteractor), "ReleaseRightHand")]
        class ReleaseRightPatch
        {
            public static void Prefix(EquipmentInteractor __instance)
            {
                if (__instance.rightHandHeldEquipment != null)
                {
                    TransferrableObject transferrableObject = __instance.rightHandHeldEquipment.GetComponent<TransferrableObject>();
                    if (transferrableObject != null && transferrableObject.InRightHand() && transferrableObject.TryGetComponent(out Slingshot _)) HoldQueue = false;
                }
                if (__instance.leftHandHeldEquipment != null)
                {
                    TransferrableObject transferrableObject = __instance.leftHandHeldEquipment.GetComponent<TransferrableObject>();
                    if (transferrableObject != null && transferrableObject.InRightHand() && transferrableObject.TryGetComponent(out Slingshot _)) HoldQueue = false;
                }
            }
        }

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