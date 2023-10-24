using BepInEx;
using BepInEx.Configuration;
using GorillaNetworking;
using HarmonyLib;
using System.IO;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;
using Utilla;
using System;

namespace OldHoldables
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")]
    public class Plugin : BaseUnityPlugin
    {
        static ConfigEntry<bool> disableDropping;
        bool IsSteamVR;
        bool initialized = false;

        void Awake() => Utilla.Events.GameInitialized += GameInitialized;

        void GameInitialized(object sender, EventArgs e)
        {
            IsSteamVR = Traverse.Create(PlayFabAuthenticator.instance).Field("platform").GetValue().ToString().ToLower() == "steam";
            initialized = true;
        }

        void OnEnable()
        {
            HarmonyPatches.ApplyHarmonyPatches();

            ConfigFile customFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "OldHoldables.cfg"), true);
            disableDropping = customFile.Bind("Input", "DisableDropping", false, "Turn off manual dropping altogether. Not recommended, but may be needed for Index controllers");
        }

        void OnDisable() => HarmonyPatches.RemoveHarmonyPatches();

        public static bool RightStickClick = false;
        private float DropTime;

        void LateUpdate()
        {
            if (!initialized) return;
            
            if (IsSteamVR)
                RightStickClick = SteamVR_Actions.gorillaTag_RightJoystickClick.GetState(SteamVR_Input_Sources.RightHand);
            else
                ControllerInputPoller.instance.rightControllerDevice.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out RightStickClick);

            if (!disableDropping.Value) 
                if (RightStickClick && (DropTime + 3) < Time.time) { DropManually(); }
        }
        void DropManually()
        {
            HarmonyPatches.SetGoingToChange = true;
            EquipmentInteractor.instance.ReleaseLeftHand();
            EquipmentInteractor.instance.ReleaseRightHand();
            HarmonyPatches.SetGoingToChange = false;
            DropTime = Time.time;
        }
    }
}
