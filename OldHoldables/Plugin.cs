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
        // since we're changing every storedZone we can find let's create an array so we can save all of them in one place
        Enum[] oldZone = new Enum[10];
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
            // wait a few seconds for the game to give you your cosmetics first. just to be safe
            if (init == true)
            {
                Debug.Log("looks like i'm being called by utilla init, let's wait a little longer for cosmetics to load first since ongameinitialized isnt enough");
                await Task.Delay(5000);
                init = false;
            }

            // this is the position we are in the array we defined earlier
            // we'll want to start from the top each time since we want to either save or rewrite all of the stored zones at once
            byte oldZonePosition = 0;
            Debug.Log("reset oldZonePosition to 0");
            // doing enabled inside foreach here checks the behavior status of the transferrableobjects themselves
            // so we have to define this first in the context of the plugin and not of the transferrableobjects (i think)
            bool pluginEnabled = enabled;
            Debug.Log("going through transferrable objects. plugin status appears to be: " + pluginEnabled);
            foreach (TransferrableObject transferrableObject in UnityEngine.Object.FindObjectsOfType<TransferrableObject>())
            {
                if (pluginEnabled == true && transferrableObject.IsMyItem() && transferrableObject.storedZone != BodyDockPositions.DropPositions.Chest)
                {
                    // save the storedzone we're on and increment the position of the array for the next transferrableobject
                    oldZone[oldZonePosition] = transferrableObject.storedZone;
                    Debug.Log("tried to save a zone. it is at: " + oldZone[oldZonePosition]);
                    oldZonePosition += 1;
                    
                    // set the storedzone we're on to none so the cosmetic sticks
                    transferrableObject.storedZone = BodyDockPositions.DropPositions.None;
                }
                if (pluginEnabled == false && transferrableObject.IsMyItem() && transferrableObject.storedZone != BodyDockPositions.DropPositions.Chest && (BodyDockPositions.DropPositions)oldZone[0] != BodyDockPositions.DropPositions.None)
                {
                    // use the array we wrote into when the plugin was enabled to put the cosmetic back to how it was :)
                    transferrableObject.storedZone = (BodyDockPositions.DropPositions)oldZone[oldZonePosition];
                    Debug.Log("tried to set zone to: " + oldZone[oldZonePosition]);
                    oldZonePosition += 1;
                }
            }
        }
    }
}
