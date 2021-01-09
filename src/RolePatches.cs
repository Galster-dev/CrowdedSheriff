using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using Hazel;

using ShipStatus = HLBNNHFCNAJ;
using PlayerControl = FFGALNAPKCD;
using IntroCutScene = PENEIDJGGAF;
using HatManager = PPAEIPHJPDH<OFCPCFDHIEF>;
using HudManager = PPAEIPHJPDH<PIEFJFEOGOL>;
using AmongUsClient = FMLLKEACGIO;
using GameOptionsData = KMOGFLPJLLK;
using Pallete = LOCPGOACAJF;
using PhysicsHelper = LBKBHDOOGHL;
using Constants = CAMOCHFAHMA;
using DeathReason = DBLJKMDLJIF;
using KeyboardJoystick = ADEHDODPMHJ;
using GameData = EGLJNOMOGNP;
using PlayerInfo = EGLJNOMOGNP.DCJMABDDJCF;

namespace CrowdedSheriff
{
    static class RolePatches
    {
        const byte rpcId = 160;
        static List<byte> sheriffs = new List<byte>();
        static List<byte> impostors = new List<byte>();

        public static bool IsSheriff(byte playerId)
        {
            return sheriffs.Contains(playerId);
        }

        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.NJOJCFDGEAE))]
        static class ShipStatus_SelectInfected
        {
            static void Postfix()
            {
                impostors.Clear();
                sheriffs.Clear();
                var fake = new List<PlayerInfo>();
                foreach(var p in GameData.Instance.AllPlayers)
                {
                    fake.Add(p);
                }
                impostors = fake.Where(p => p.DAPKNDBLKIA).Select(p => p.JKOMCOJCAID).ToList();
                sheriffs = fake.Where(p => !p.OMHGJKAKOHO && !p.DLPCKPBIJOE && !p.DAPKNDBLKIA)
                               .Select(p => p.JKOMCOJCAID)
                               .OrderBy(p => Guid.NewGuid()) // shuffle
                               .Take(OptionsPatches.sheriffCount)
                               .ToList();

                var writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, rpcId, Hazel.SendOption.Reliable);
                writer.WriteBytesAndSize(sheriffs.ToArray());
                writer.EndMessage();
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc), new Type[] { typeof(byte), typeof(MessageReader)})]
        static class PlayerControl_HandleRpc
        {
            static bool Prefix(byte HKHMBLJFLMC, MessageReader ALMCIJKELCP)
            {
                if(HKHMBLJFLMC == rpcId)
                {
                    sheriffs = ALMCIJKELCP.ReadBytesAndSize().ToList();

                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(IntroCutScene), nameof(IntroCutScene.MKDJLGEGEGC))]
        static class IntroCutScene_BeginCrewmate
        {
            static bool Prefix(ref IntroCutScene __instance)
            {
                if (!IsSheriff(PlayerControl.LocalPlayer.PlayerId))
                {
                    return true;
                }

                __instance.Title.Text = "The Sheriff";
                __instance.Title.Color = new Color(0xFF, 0xA5, 0x00, 0xFF);
                __instance.Title.scale /= 2;
                __instance.BackgroundBar.material.SetColor("_Color", new Color(0xFF, 0xA5, 0x00));

                var poolablePlayer = UnityEngine.Object.Instantiate(__instance.PlayerPrefab, __instance.transform);
                var data = PlayerControl.LocalPlayer.JLGGIOLCDFC;

                poolablePlayer.SetFlipX(false);
                poolablePlayer.transform.localPosition = new Vector3(0, __instance.BaseY - 0.25f, -8);
                var vec = new Vector3(1.8f, 1.8f, 1.8f);
                poolablePlayer.transform.localScale = vec;
                PlayerControl.SetPlayerMaterialColors(data.EHAHBDFODKC, poolablePlayer.Body);
                HatManager.IAINKLDJAGC.HCAEGGFGECL(poolablePlayer.SkinSlot, data.HPAMBHFDLEH);
                poolablePlayer.HatSlot.SetHat(data.AFEJLMBMKCJ, 0);
                PlayerControl.SetPetImage(data.AJIBCNMKNPM, data.EHAHBDFODKC, poolablePlayer.PetSlot);
                poolablePlayer.NameText.gameObject.SetActive(true);
                poolablePlayer.NameText.Text = data.EIGEKHDAKOH;
                poolablePlayer.NameText.Color = Pallete.HPMGFCCJLIF;
                PlayerControl.LocalPlayer.nameText.Color = Pallete.HPMGFCCJLIF;
                HudManager.IAINKLDJAGC.KillButton.gameObject.SetActive(true);

                return false;
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
        static class PlayerControl_FixedUpdate
        {
            static PlayerControl FindClosestTarget(ref PlayerControl local)
            {
                PlayerControl result = null;
                if (!ShipStatus.Instance) return null;
                float disatnce = GameOptionsData.JMLGACIOLIK[Mathf.Clamp(PlayerControl.GameOptions.DLIBONBKPKL, 0, 2)];
                var truePos = local.GetTruePosition();

                foreach(var p in GameData.Instance.AllPlayers)
                {
                    if(!p.OMHGJKAKOHO && p.JKOMCOJCAID != local.PlayerId && !p.DLPCKPBIJOE)
                    {
                        var control = p.LAOEJKHLKAI;
                        if(control != null)
                        {
                            var vector = control.GetTruePosition() - truePos;
                            float magnitude = vector.magnitude;
                            if (magnitude <= disatnce && !PhysicsHelper.IIPMKCELMED(truePos, vector.normalized, magnitude, Constants.BBHMKOHHIKI))
                            {
                                result = control;
                                disatnce = magnitude;
                            }
                        }
                    }
                }

                return result;
            }
            static void Postfix(ref PlayerControl __instance)
            {
                if(__instance.LGDCIDJJHMC && IsSheriff(__instance.PlayerId) && __instance.GEBLLBHGHLD && !__instance.JLGGIOLCDFC.DLPCKPBIJOE)
                // AmOwner && isSheriff && canMove && !isDead
                {
                    __instance.SetKillTimer(Mathf.Max(0f, __instance.killTimer - Time.fixedDeltaTime));
                    HudManager.IAINKLDJAGC.KillButton.SetTarget(FindClosestTarget(ref __instance));
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Die), new Type[] { typeof(DeathReason) })]
        static class PlayerControl_Die
        {
            static void Postfix(ref PlayerControl __instance)
            {
                if(__instance.LGDCIDJJHMC)
                {
                    HudManager.IAINKLDJAGC.KillButton.gameObject.SetActive(false);
                    HudManager.IAINKLDJAGC.ReportButton.gameObject.SetActive(false); // why not
                }
            }
        }

        [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.DFEHKDBDDIH))]
        static class KeyboardJoystick_HandleHud
        {
            static bool started = false;
            static void Postfix()
            {
                if(PlayerControl.LocalPlayer.JLGGIOLCDFC != null 
                    && IsSheriff(PlayerControl.LocalPlayer.PlayerId)
                    && BepInEx.IL2CPP.UnityEngine.Input.GetKeyInt(BepInEx.IL2CPP.UnityEngine.KeyCode.Q)
                  )
                {
                    if(!started)
                    {
                        started = true;
                        HudManager.IAINKLDJAGC.KillButton.PerformKill();
                    } else
                    {
                        started = false;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer), typeof(PlayerControl))]
        static class PlayerControl_MurderPlayer
        {
            static bool trueImpost;
            static void Prefix(ref PlayerControl __instance, ref PlayerControl CAKODNGLPDF)
            {
                trueImpost = __instance.JLGGIOLCDFC.DAPKNDBLKIA;
                if (!trueImpost && IsSheriff(__instance.PlayerId) && !CAKODNGLPDF.JLGGIOLCDFC.DAPKNDBLKIA)
                {
                    // TODO: make sheriff's target die too (customizable)
                    CAKODNGLPDF = __instance;
                }
                __instance.JLGGIOLCDFC.DAPKNDBLKIA = true;
            }
            static void Postfix(ref PlayerControl __instance)
            {
                if (!trueImpost)
                {
                    __instance.JLGGIOLCDFC.DAPKNDBLKIA = false;
                }
            }
        }
    }
}
