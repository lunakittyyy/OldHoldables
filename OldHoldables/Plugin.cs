using BepInEx;
using UnityEngine;
using UnityEngine.XR;

namespace OldHoldables
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        void OnEnable()
        {
            HarmonyPatches.ApplyHarmonyPatches();
        }

        void OnDisable()
        {
            HarmonyPatches.RemoveHarmonyPatches();
        }

        private static XRNode rNode = XRNode.RightHand;
        public static bool RightStickClick = false;
        private float DropTime;

        void LateUpdate()
        {
            InputDevice rightController = InputDevices.GetDeviceAtXRNode(rNode);
            rightController.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out RightStickClick);
            if (RightStickClick && (DropTime + 3) < Time.time)
            {
                HarmonyPatches.SetGoingToChange = true;
                EquipmentInteractor.instance.ReleaseLeftHand();
                EquipmentInteractor.instance.ReleaseRightHand();
                HarmonyPatches.SetGoingToChange = false;
                // ratelimit manual redocking. don't know if this is needed
                // but obviously holdables are networked so lets restrict this to only happen every three seconds or more
                DropTime = Time.time;
            }
        }
    }
}
