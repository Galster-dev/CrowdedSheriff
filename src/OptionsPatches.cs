using HarmonyLib;
using System;
using Il2CppSystem.IO;
using Hazel;
using UnhollowerBaseLib;
using UnityEngine;

using GameOptionsMenu = PHCKLDDNJNP;
using GameOptionsData = KMOGFLPJLLK;
using GameSettingsMenu = JCLABFFHPEO;
using NumberOption = PCGDGFIAJJI;
using ToggleOption = BCLDBBKFJPK;
using Scroller = CIAACBCIDFI;
using OptionBehaviour = LLKOLCLGCBD;
using AmongUsClient = FMLLKEACGIO;
using TranslationController = GIGNEFLFPDE;
using PlayerControl = FFGALNAPKCD;

namespace CrowdedSheriff
{
    static class OptionsPatches
    {
        public static byte sheriffCount = 1;
        public static bool doKillSheriffsTarget = false;
        const StringNames sheriffCountTitle = (StringNames)313; // idk funny number
        const StringNames killTargetTitle   = (StringNames)314;

        [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new Type[] { typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>) })]
        static class TranslationController_GetString
        {
            public static bool Prefix(StringNames HKOIECMDOKL, ref string __result)
            {
                switch(HKOIECMDOKL)
                {
                    case sheriffCountTitle:
                        __result = "# Sheriff count";
                        break;
                    case killTargetTitle:
                        __result = "Sheriff's target dies";
                        break;
                    default:
                        return true;
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
        static class GameOptionsMenu_Start
        {
            public static void OnValueChanged(OptionBehaviour option)
            {
                if (!AmongUsClient.Instance || !AmongUsClient.Instance.BEIEANEKAFC) return;
                switch(option.Title)
                {
                    case sheriffCountTitle:
                        sheriffCount = (byte)option.GetInt();
                        break;
                    case killTargetTitle:
                        doKillSheriffsTarget = option.GetBool();
                        break;
                }
                if (PlayerControl.GameOptions.JFALOOKBBAD)
                {
                    PlayerControl.GameOptions.JFALOOKBBAD = false;
                    UnityEngine.Object.FindObjectOfType<GameOptionsMenu>().POPOILOGEAL();
                }
                var local = PlayerControl.LocalPlayer;
                if (local != null)
                {
                    local.RpcSyncSettings(PlayerControl.GameOptions);
                }
            }
            static void Postfix(ref GameOptionsMenu __instance)
            {
                var countOption = UnityEngine.Object.Instantiate(__instance.GetComponentsInChildren<NumberOption>()[1], __instance.transform);
                countOption.transform.localPosition = new Vector3(countOption.transform.localPosition.x, -8.35f, countOption.transform.localPosition.z);
                countOption.Title = sheriffCountTitle;
                countOption.Value = sheriffCount;
                var str = "";
                TranslationController_GetString.Prefix(countOption.Title, ref str);
                countOption.TitleText.Text = str;
                countOption.OnValueChanged = new Action<OptionBehaviour>(OnValueChanged);
                countOption.gameObject.AddComponent<OptionBehaviour>();

                var toggleOption = UnityEngine.Object.Instantiate(__instance.GetComponentsInChildren<ToggleOption>()[1], __instance.transform);
                toggleOption.transform.localPosition = new Vector3(toggleOption.transform.localPosition.x, -8.85f, toggleOption.transform.localPosition.z);
                toggleOption.Title = killTargetTitle;
                toggleOption.CheckMark.enabled = doKillSheriffsTarget;
                var str2 = "";
                TranslationController_GetString.Prefix(toggleOption.Title, ref str2);
                toggleOption.TitleText.Text = str2;
                toggleOption.OnValueChanged = new Action<OptionBehaviour>(OnValueChanged);
                toggleOption.gameObject.AddComponent<OptionBehaviour>();
                __instance.GetComponentInParent<Scroller>().YBounds.max+=0.3f;
            }
        }

        [HarmonyPatch(typeof(GameSettingsMenu), nameof(GameSettingsMenu.OnEnable))]
        static class GameSettingsMenu_OnEnable
        {
            static void Prefix(ref GameSettingsMenu __instance)
            {
                __instance.HideForOnline = new Il2CppReferenceArray<Transform>(0);
            }
        }

        [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.OnEnable))]
        static class NumberOption_OnEnable
        {
            static bool Prefix(ref NumberOption __instance)
            {
                if(__instance.Title == sheriffCountTitle)
                {
                    string smh = "";
                    TranslationController_GetString.Prefix(__instance.Title, ref smh);
                    __instance.TitleText.Text = smh;
                    __instance.OnValueChanged = new Action<OptionBehaviour>(GameOptionsMenu_Start.OnValueChanged);
                    __instance.Value = sheriffCount;
                    __instance.enabled = true;

                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(ToggleOption), nameof(ToggleOption.OnEnable))]
        static class ToggleOption_OnEnable
        {
            static bool Prefix(ref ToggleOption __instance)
            {
                if(__instance.Title == killTargetTitle)
                {
                    string str = "";
                    TranslationController_GetString.Prefix(__instance.Title, ref str);
                    __instance.TitleText.Text = str;
                    __instance.CheckMark.enabled = doKillSheriffsTarget;
                    __instance.OnValueChanged = new Action<OptionBehaviour>(GameOptionsMenu_Start.OnValueChanged);
                    __instance.enabled = true;

                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(GameOptionsData), nameof(GameOptionsData.CKKJMLEDCJB))]
        static class GameOptionsData_ToHudString
        {
            static void Postfix(ref string __result)
            {
                var builder = new System.Text.StringBuilder(__result);
                builder.AppendLine();
                builder.AppendLine($"Sheriff count: {sheriffCount}");
                builder.AppendLine($"Sheriff's target dies: {(doKillSheriffsTarget ? "On" : "Off")}");
                __result = builder.ToString();
            }
        }

        [HarmonyPatch(typeof(GameOptionsData), nameof(GameOptionsData.FEJLLGGCJCC), typeof(BinaryReader))]
        static class GameOptionsData_Deserialize
        {
            static void Postfix(BinaryReader ALMCIJKELCP)
            {
                try
                {
                    sheriffCount = ALMCIJKELCP.ReadByte();
                }
                catch {
                    sheriffCount = 0;
                }
                try
                {
                    doKillSheriffsTarget = ALMCIJKELCP.ReadBoolean();
                }
                catch
                {
                    doKillSheriffsTarget = false;
                }
            }
        }
        [HarmonyPatch(typeof(GameOptionsData), nameof(GameOptionsData.FEJLLGGCJCC), typeof(MessageReader))]
        static class GameOptionsData_DeserializeM
        {

            static void Postfix(MessageReader ALMCIJKELCP)
            {
                try
                {
                    sheriffCount = ALMCIJKELCP.ReadByte();
                }
                catch
                {
                    sheriffCount = 0;
                }
                try
                {
                    doKillSheriffsTarget = ALMCIJKELCP.ReadBoolean();
                }
                catch
                {
                    doKillSheriffsTarget = false;
                }
            }
        }

        [HarmonyPatch(typeof(GameOptionsData), nameof(GameOptionsData.FGBDCOOJCKD), new Type[]{ typeof(BinaryWriter), typeof(byte) })]
        static class GameOptionsData_Serialize
        {
            static void Postfix(BinaryWriter AGLJMGAODDG)
            {
                AGLJMGAODDG.Write(sheriffCount);
                AGLJMGAODDG.Write(doKillSheriffsTarget);
            }
        }
    }
}
