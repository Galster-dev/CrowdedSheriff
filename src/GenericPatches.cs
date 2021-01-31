using HarmonyLib;
using UnityEngine;

using Palette = LOCPGOACAJF;
using VersionShower = BOCOFLHKCOJ;
using PingTracker = ELDIDNABIPI;
using AspectPosition = CKFHGGLODEF;
using PlayerControl = FFGALNAPKCD;
using HudManager = PPAEIPHJPDH<PIEFJFEOGOL>;

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
            private static Vector3 lastDist = Vector3.zero;
            static void Postfix(ref PingTracker __instance)
            {
                var aspect = __instance.text.gameObject.GetComponent<AspectPosition>();
                if (aspect.DistanceFromEdge != lastDist)
                {
                    aspect.DistanceFromEdge += new Vector3(0.6f, 0);
                    aspect.AdjustPosition();
                    
                    lastDist = aspect.DistanceFromEdge;
                }
                __instance.text.Text += $"\n[FFA500FF]CrowdedSheriff v{SheriffPlugin.version}\n" +
                                        $"by Galster (sleepyut#0710)";
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
        static class PlayerControl_SetKillTimer
        {
            static bool Prefix(ref PlayerControl __instance, [HarmonyArgument(0)] float time)
            {
                __instance.killTimer = time;
                if (__instance == PlayerControl.LocalPlayer)
                {
                    HudManager.IAINKLDJAGC.KillButton.SetCoolDown(
                        PlayerControl.GameOptions.IGHCIKIDAMO > 0 ? time : 0,
                        PlayerControl.GameOptions.IGHCIKIDAMO
                    );
                }

                return false;
            }
        }
    }
}
