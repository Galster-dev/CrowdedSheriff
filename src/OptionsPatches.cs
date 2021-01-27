using HarmonyLib;
using System;
using System.Linq;
using Discord;
using Il2CppSystem.IO;
using Hazel;
using UnhollowerBaseLib;
using UnityEngine;


namespace CrowdedSheriff
{
    static class OptionsPatches
    {
        public static byte sheriffCount = 1;
        public static bool doKillSheriffsTarget = false;
        public static float sheriffKillCd = 30.0f;

        const StringNames sheriffCountTitle = (StringNames) 313; // idk funny number
        const StringNames killTargetTitle = (StringNames) 314;
        const StringNames sheriffKillCdTitle = (StringNames) 315;

        [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString),
            new Type[] {typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>)})]
        static class TranslationController_GetString
        {
            public static bool Prefix(StringNames HKOIECMDOKL, ref string __result)
            {
                switch (HKOIECMDOKL)
                {
                    case sheriffCountTitle:
                        __result = "Sheriff count";
                        break;
                    case killTargetTitle:
                        __result = "Sheriff's target dies";
                        break;
                    case sheriffKillCdTitle:
                        __result = "Sheriff's kill cd";
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
                if (!AmongUsClient.Instance || !AmongUsClient.Instance.AmHost) return;
                switch (option.Title)
                {
                    case sheriffCountTitle:
                        sheriffCount = (byte) option.GetInt();
                        break;
                    case killTargetTitle:
                        doKillSheriffsTarget = option.GetBool();
                        break;
                    case sheriffKillCdTitle:
                        sheriffKillCd = option.GetFloat();
                        break;
                }

                if (PlayerControl.GameOptions.isDefaults)
                {
                    PlayerControl.GameOptions.isDefaults = false;
                    UnityEngine.Object.FindObjectOfType<GameOptionsMenu>().Method_16(); //RefreshChildren
                }

                var local = PlayerControl.LocalPlayer;
                if (local != null)
                {
                    local.RpcSyncSettings(PlayerControl.GameOptions);
                }
            }

            static float GetLowestConfigY(GameOptionsMenu __instance)
            {
                return __instance.GetComponentsInChildren<OptionBehaviour>()
                    .Min(option => option.transform.localPosition.y);
            }

            static void Postfix(ref GameOptionsMenu __instance)
            {
                var lowestY = GetLowestConfigY(__instance);

                var countOption = UnityEngine.Object.Instantiate(__instance.GetComponentsInChildren<NumberOption>()[1],
                    __instance.transform);
                countOption.transform.localPosition = new Vector3(countOption.transform.localPosition.x, lowestY - 0.5f,
                    countOption.transform.localPosition.z);
                countOption.Title = sheriffCountTitle;
                countOption.Value = sheriffCount;
                var str = "";
                TranslationController_GetString.Prefix(countOption.Title, ref str);
                countOption.TitleText.Text = str;
                countOption.OnValueChanged = new Action<OptionBehaviour>(OnValueChanged);
                countOption.gameObject.AddComponent<OptionBehaviour>();

                var toggleOption = UnityEngine.Object.Instantiate(__instance.GetComponentsInChildren<ToggleOption>()[1],
                    __instance.transform);
                toggleOption.transform.localPosition = new Vector3(toggleOption.transform.localPosition.x,
                    lowestY - 1.0f, toggleOption.transform.localPosition.z);
                toggleOption.Title = killTargetTitle;
                toggleOption.CheckMark.enabled = doKillSheriffsTarget;
                var str2 = "";
                TranslationController_GetString.Prefix(toggleOption.Title, ref str2);
                toggleOption.TitleText.Text = str2;
                toggleOption.OnValueChanged = new Action<OptionBehaviour>(OnValueChanged);
                toggleOption.gameObject.AddComponent<OptionBehaviour>();

                var countOption2 = UnityEngine.Object.Instantiate(
                    GameObject.FindObjectsOfType<NumberOption>().ToList()
                        .First(x => x.TitleText.Text == "Kill Cooldown"),
                    __instance.transform);
                countOption2.transform.localPosition = new Vector3(countOption2.transform.localPosition.x,
                    lowestY - 1.5f, countOption2.transform.localPosition.z);
                countOption2.Title = sheriffKillCdTitle;
                countOption2.Value = sheriffKillCd;
                var str3 = "";
                TranslationController_GetString.Prefix(countOption2.Title, ref str3);
                countOption2.TitleText.Text = str3;
                countOption2.OnValueChanged = new Action<OptionBehaviour>(OnValueChanged);
                countOption2.gameObject.AddComponent<OptionBehaviour>();
                countOption2.Increment = 2.5f;
                countOption2.ValidRange.min = 10.0f;
                countOption2.ValidRange.max = 60.0f;

                __instance.GetComponentInParent<Scroller>().YBounds.max += 1.5f;
            }
        }

        [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.OnEnable))]
        static class GameSettingsMenu_OnEnable
        {
            static void Prefix(ref GameSettingMenu __instance)
            {
                __instance.HideForOnline = new Il2CppReferenceArray<Transform>(0);
            }
        }

        [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.OnEnable))]
        static class NumberOption_OnEnable
        {
            static bool Prefix(ref NumberOption __instance)
            {
                if (__instance.Title == sheriffCountTitle)
                {
                    string smh = "";
                    TranslationController_GetString.Prefix(__instance.Title, ref smh);
                    __instance.TitleText.Text = smh;
                    __instance.OnValueChanged = new Action<OptionBehaviour>(GameOptionsMenu_Start.OnValueChanged);
                    __instance.Value = sheriffCount;
                    __instance.enabled = true;

                    return false;
                }

                if (__instance.Title == sheriffKillCdTitle)
                {
                    string smh = "";
                    TranslationController_GetString.Prefix(__instance.Title, ref smh);
                    __instance.TitleText.Text = smh;
                    __instance.OnValueChanged = new Action<OptionBehaviour>(GameOptionsMenu_Start.OnValueChanged);
                    __instance.Value = sheriffKillCd;
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
                if (__instance.Title == killTargetTitle)
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

        [HarmonyPatch(typeof(GameOptionsData), nameof(GameOptionsData.Method_24))]
        static class GameOptionsData_ToHudString
        {
            static void Postfix(ref string __result)
            {
                var builder = new Il2CppSystem.Text.StringBuilder(__result);
                builder.AppendLine();
                builder.AppendLine($"Sheriff count: {sheriffCount}");
                builder.AppendLine($"Sheriff's target dies: {(doKillSheriffsTarget ? "On" : "Off")}");

                builder.Append("Sheriff's kill cooldown: ");
                builder.Append(sheriffKillCd);
                builder.Append("s");
                builder.AppendLine();

                __result = builder.ToString();

                DestroyableSingleton<HudManager>.Instance.GameSettings.scale = 0.6f;
            }
        }

        [HarmonyPatch(typeof(GameOptionsData), nameof(GameOptionsData.Method_65), typeof(BinaryReader))]
        static class GameOptionsData_Deserialize
        {
            static void Postfix(BinaryReader ALMCIJKELCP)
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

                try
                {
                    sheriffKillCd = System.BitConverter.ToSingle(ALMCIJKELCP.ReadBytes(4).ToArray(), 0);
                }
                catch
                {
                    sheriffKillCd = 30.0f;
                }
            }
        }

        [HarmonyPatch(typeof(GameOptionsData), nameof(GameOptionsData.Method_7), typeof(MessageReader))]
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

                try
                {
                    sheriffKillCd = System.BitConverter.ToSingle(ALMCIJKELCP.ReadBytes(4).ToArray(), 0);
                }
                catch
                {
                    sheriffKillCd = 30.0f;
                }
            }
        }

        [HarmonyPatch(typeof(GameOptionsData), nameof(GameOptionsData.Method_53),
            new Type[] {typeof(BinaryWriter), typeof(byte)})]
        static class GameOptionsData_Serialize
        {
            static void Postfix(BinaryWriter AGLJMGAODDG)
            {
                AGLJMGAODDG.Write(sheriffCount);
                AGLJMGAODDG.Write(doKillSheriffsTarget);
                AGLJMGAODDG.Write(sheriffKillCd);
            }
        }
    }
}