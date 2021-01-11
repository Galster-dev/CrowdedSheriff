using HarmonyLib;
using UnityEngine;
using VersionShower = BOCOFLHKCOJ;
using PingTracker = ELDIDNABIPI;

namespace CrowdedSheriff
{
    namespace GenericPatches
    {
        [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
        static class VersionShower_Start
        {
            static void Postfix(ref VersionShower __instance)
            {
                __instance.text.Text = $"Among Us {__instance.text.Text}\n" +
                                       $"[FFA500FF]CrowdedSheriff v{SheriffPlugin.version}\n" +
                                       $"by Galster (sleepyut#0710)";
            }
        }

        [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
        static class PingTracker_Update
        {
            static void Postfix(ref PingTracker __instance)
            {
                __instance.text.Text += "\n[FFA500FF]CrowdedSheriff";
            }
        }
    }
}
