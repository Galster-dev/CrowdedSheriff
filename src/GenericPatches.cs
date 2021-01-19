using HarmonyLib;
using UnityEngine;
using Palette = LOCPGOACAJF;
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
                /*__instance.text.Text = $"Among Us {__instance.text.Text}\n" +
                                       $"[FFA500FF]CrowdedSheriff v{SheriffPlugin.version}\n" +
                                       $"by Galster (sleepyut#0710)";*/
                // FOR: crying reactor users
                __instance.text.Color = Palette.HPMGFCCJLIF; // orange
            }
        }

        [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
        [HarmonyPriority(Priority.First)]
        static class PingTracker_Update
        {
            private static bool firstRun = true;
            static void Postfix(ref PingTracker __instance)
            {
                if (firstRun)
                {
                    firstRun = false;
                    __instance.text.transform.position -= new Vector3(0.55f, 0, 0);
                }
                __instance.text.Text += $"\n[FFA500FF]CrowdedSheriff v{SheriffPlugin.version}\n" +
                                        $"by Galster (sleepyut#0710)";
            }
        }
    }
}
