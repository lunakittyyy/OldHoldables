using BepInEx;
using BepInEx.Configuration;
using System.IO;
using UnityEngine;
using UnityEngine.XR;

namespace OldHoldables
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        
        static ConfigEntry<bool> useLeftStick;
        static ConfigEntry<bool> disableDropping;

        void OnEnable()
        {
            HarmonyPatches.ApplyHarmonyPatches();

            ConfigFile customFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "OldHoldables.cfg"), true);
            useLeftStick = customFile.Bind("Input", "UseLeftStick", false, "Use the Left stick to drop holdables instead of the right.");
            disableDropping = customFile.Bind("Input", "DisableDropping", false, "Turn off manual dropping altogether. Not recommended!");
        }

        void OnDisable()
        {
            HarmonyPatches.RemoveHarmonyPatches();
        }

        private static XRNode rNode = XRNode.RightHand;
        private static XRNode lNode = XRNode.LeftHand;
        public static bool RightStickClick = false;
        public static bool LeftStickClick = false;
        private float DropTime;

        void LateUpdate()
        {
            InputDevice rightController = InputDevices.GetDeviceAtXRNode(rNode);
            rightController.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out RightStickClick);

            InputDevice leftController = InputDevices.GetDeviceAtXRNode(lNode);
            leftController.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out LeftStickClick);

            if (!disableDropping.Value) 
            {
                if (LeftStickClick && useLeftStick.Value == true && (DropTime + 3) < Time.time)
                {
                    DropManually();
                }
                else if (RightStickClick && useLeftStick.Value != true && (DropTime + 3) < Time.time)
                {
                    DropManually();
                }
            }
        }

        void DropManually()
        {
            HarmonyPatches.SetGoingToChange = true;
            EquipmentInteractor.instance.ReleaseLeftHand();
            EquipmentInteractor.instance.ReleaseRightHand();
            HarmonyPatches.SetGoingToChange = false;
            // ratelimit manual redocking.
            // needed to prevent spamming snowballs, also prolly just a good idea to ratelimit networked objects in general
            // especially when we're making things behave differently than usual
            DropTime = Time.time;
        }
    }
}
