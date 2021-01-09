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
        static readonly StringNames sheriffCountTitle = (StringNames)313; // idk funny number

        [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new Type[] { typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>) })]
        static class TranslationController_GetString
        {
            public static bool Prefix(StringNames HKOIECMDOKL, ref string __result)
            {
                if (HKOIECMDOKL == sheriffCountTitle)
                {
                    __result = "# Sheriff count";
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
        static class GameOptionsMenu_Start
        {
            public static void OnCountChanged(OptionBehaviour option)
            {
                if (!AmongUsClient.Instance || !AmongUsClient.Instance.BEIEANEKAFC) return;
                sheriffCount = (byte)option.GetInt();
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
                // FIXME: `Value` forces to be 1
                countOption.Value = 1f;
                var str = "";
                TranslationController_GetString.Prefix(countOption.Title, ref str);
                countOption.TitleText.Text = str;
                countOption.OnValueChanged = new Action<OptionBehaviour>(OnCountChanged);
                countOption.gameObject.AddComponent<OptionBehaviour>();

                //__instance.GetComponentInParent<Scroller>().YBounds = new FloatRange(0, 9);
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
                    __instance.TitleText.Text = smh; // smort
                    //__instance.ValueText.Text = string.Format(__instance.FormatString, __instance.Value);
                    __instance.OnValueChanged = new Action<OptionBehaviour>(GameOptionsMenu_Start.OnCountChanged);
                    __instance.Value = 1f;
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
                __result += "\nSheriff count: " + sheriffCount;
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
                catch { }
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
                catch { }
            }
        }

        [HarmonyPatch(typeof(GameOptionsData), nameof(GameOptionsData.FGBDCOOJCKD), new Type[]{ typeof(BinaryWriter), typeof(byte) })]
        static class GameOptionsData_Serialize
        {
            static void Postfix(BinaryWriter AGLJMGAODDG)
            {
                AGLJMGAODDG.Write(sheriffCount);
            }
        }
    }
}
