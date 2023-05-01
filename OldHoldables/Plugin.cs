using BepInEx;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using Utilla;

namespace OldHoldables
{
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        bool init = false;
        Enum[] oldZone = new Enum[6];
        void Start()
        {
            Utilla.Events.GameInitialized += OnGameInitialized;
        }

        void OnEnable()
        {
            HoldableToggle();
            HarmonyPatches.ApplyHarmonyPatches();
        }

        void OnDisable()
        {
            HoldableToggle();
            HarmonyPatches.RemoveHarmonyPatches();
        }

        void OnGameInitialized(object sender, EventArgs e)
        {
            init = true;
            HoldableToggle();
        }
        
        
        async void HoldableToggle()
        {
            if (init == true)
            {
                Debug.Log("looks like i'm being called by utilla init, let's wait a little longer for cosmetics to load first since ongameinitialized isnt enough");
                await Task.Delay(5000);
                init = false;
            }

            byte oldZonePosition = 0;
            Debug.Log("reset oldZonePosition to 0");
            bool pluginEnabled = enabled;
            Debug.Log("going through transferrable objects. plugin status appears to be: " + pluginEnabled);
            foreach (TransferrableObject transferrableObject in UnityEngine.Object.FindObjectsOfType<TransferrableObject>())
            {
                if (pluginEnabled == true && transferrableObject.IsMyItem() && transferrableObject.storedZone != BodyDockPositions.DropPositions.Chest)
                {
                    oldZone[oldZonePosition] = transferrableObject.storedZone;
                    Debug.Log("tried to save zone. it is: " + oldZone[oldZonePosition]);
                    oldZonePosition += 1;
                    transferrableObject.storedZone = BodyDockPositions.DropPositions.None;
                }
                if (pluginEnabled == false && transferrableObject.IsMyItem() && transferrableObject.storedZone != BodyDockPositions.DropPositions.Chest && (BodyDockPositions.DropPositions)oldZone[0] != BodyDockPositions.DropPositions.None)
                {
                    transferrableObject.storedZone = (BodyDockPositions.DropPositions)oldZone[oldZonePosition];
                    Debug.Log("tried to set zone to: " + oldZone[oldZonePosition]);
                    oldZonePosition += 1;
                }
            }
        }
    }
}
