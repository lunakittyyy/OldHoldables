using System;
using System.IO;
using UnityEngine;
using UnityEngine.XR;
using MelonLoader;

namespace OldHoldables
{
    public class OldHoldablesMod : MelonMod
    {
        private static XRNode rNode = XRNode.RightHand;
        public static bool RightStickClick = false;
        private float DropTime;

        void LateUpdate()
        {
            InputDevice rightController = InputDevices.GetDeviceAtXRNode(rNode);
            rightController.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out RightStickClick);

            if (RightStickClick && (DropTime + 3) <= Time.time) 
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
}
