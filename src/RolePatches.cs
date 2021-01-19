using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
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
using Palette = LOCPGOACAJF;
using KillButtonManager = MLPJGKEACMM;
using PhysicsHelper = LBKBHDOOGHL;
using Constants = CAMOCHFAHMA;
using EndGameManager = ABNGEPFHMHP;
using KeyboardJoystick = ADEHDODPMHJ;
using GameData = EGLJNOMOGNP;
using PlayerInfo = EGLJNOMOGNP.DCJMABDDJCF;
using MeetingHud = OOCJALPKPEP;
using PlayerVoteArea = HDJGDMFCHDN;

namespace CrowdedSheriff
{
    static class RolePatches
    {
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

                var writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.SelectSheriffs, Hazel.SendOption.Reliable);
                writer.WriteBytesAndSize(sheriffs.ToArray());
                writer.EndMessage();
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc), new Type[] { typeof(byte), typeof(MessageReader)})]
        static class PlayerControl_HandleRpc
        {
            static bool Prefix(byte HKHMBLJFLMC, MessageReader ALMCIJKELCP)
            {
                switch ((CustomRpc)HKHMBLJFLMC)
                {
                    case CustomRpc.SelectSheriffs:
                        sheriffs = ALMCIJKELCP.ReadBytesAndSize().ToList();
                        break;
                    case CustomRpc.SheriffKill:
                        var sheriff = GameData.Instance.GetPlayerById(ALMCIJKELCP.ReadByte())?.LAOEJKHLKAI;
                        
                        PlayerControl_FixedUpdate.SheriffKill(sheriff, ALMCIJKELCP.ReadByte());
                        break;
                    
                    default:
                        return true;
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(IntroCutScene.CKACLKCOJFO), nameof(IntroCutScene.CKACLKCOJFO.MoveNext))]
        static class IntroCutScene_CoBegin
        {
            static void Postfix(ref IntroCutScene.CKACLKCOJFO __instance)
            {
                if (IsSheriff(PlayerControl.LocalPlayer.PlayerId))
                {
                    IntroCutScene localscene = __instance.field_Public_PENEIDJGGAF_0;
                    localscene.Title.Text = "Sheriff";
                    PlayerControl.LocalPlayer.nameText.Color = localscene.Title.Color = Palette.HPMGFCCJLIF;
                    //localscene.Title.scale /= 1.5f; // "Sheriff" isn't that big as "The Sheriff"
                    localscene.ImpostorText.Text = "Kill the [FF0000FF]Impostor";
                    localscene.BackgroundBar.material.color = Palette.HPMGFCCJLIF;
                    PlayerControl.LocalPlayer.SetKillTimer(8f);
                    // FIXME: you can kill on 
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.OJPCECLPCCF))]
        static class PlayerControl_SetInfected
        {
            static void Postfix()
            {
                PlayerControl.LocalPlayer.SetKillTimer(10f); // FIXED: ability to kill on intro scene
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
        static class PlayerControl_FixedUpdate
        {
            public static void SheriffKill(PlayerControl sheriff, byte targetId)
            {
                if (sheriff is null) return;
                if (!IsSheriff(sheriff.PlayerId) && sheriff.PlayerId != targetId) return;
                var target  = GameData.Instance.GetPlayerById(targetId)?.LAOEJKHLKAI;
                if (target is null) return;
                sheriff.MurderPlayer(target);
            }

            public static void RpcSheriffKill(PlayerControl sheriff, byte targetId)
            {
                if (AmongUsClient.Instance.LJMBKFFAMIP)
                {
                    SheriffKill(sheriff, targetId);
                }

                var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                    (byte) CustomRpc.SheriffKill, SendOption.Reliable, -1);
                writer.Write(PlayerControl.LocalPlayer.PlayerId);
                writer.Write(targetId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }
            
            static PlayerControl FindClosestTarget(ref PlayerControl local)
            {
                PlayerControl result = null;
                if (!ShipStatus.Instance) return null;
                float distance = GameOptionsData.JMLGACIOLIK[Mathf.Clamp(PlayerControl.GameOptions.DLIBONBKPKL, 0, 2)];
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
                            if (magnitude <= distance && !PhysicsHelper.IIPMKCELMED(truePos, vector.normalized, magnitude, Constants.BBHMKOHHIKI))
                            {
                                result = control;
                                distance = magnitude;
                            }
                        }
                    }
                }

                return result;
            }
            static void Postfix(ref PlayerControl __instance)
            {
                if(__instance.LGDCIDJJHMC && IsSheriff(__instance.PlayerId) && __instance.GEBLLBHGHLD)
                // AmOwner && isSheriff && canMove
                {
                    if (!__instance.JLGGIOLCDFC.DLPCKPBIJOE)
                    {
                        __instance.SetKillTimer(Mathf.Max(0f, __instance.killTimer - Time.fixedDeltaTime));
                        HudManager.IAINKLDJAGC.KillButton.SetTarget(FindClosestTarget(ref __instance));
                    }
                    HudManager.IAINKLDJAGC.KillButton.gameObject.SetActive(!__instance.JLGGIOLCDFC.DLPCKPBIJOE);
                }
            }
        }

        [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.Start))]
        static class EndGameManager_Start
        {
            static void Prefix()
            {
                sheriffs.Clear();
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

        [HarmonyPatch(typeof(KillButtonManager), nameof(KillButtonManager.PerformKill))]
        static class KillButtonManager_PerformKill
        {
            static bool Prefix(ref KillButtonManager __instance)
            {
                if (IsSheriff(PlayerControl.LocalPlayer.PlayerId) && __instance.isActiveAndEnabled && 
                    __instance.CurrentTarget && !__instance.isCoolingDown &&
                    !PlayerControl.LocalPlayer.JLGGIOLCDFC.DLPCKPBIJOE && PlayerControl.LocalPlayer.GEBLLBHGHLD)
                {
                    if (__instance.CurrentTarget.JLGGIOLCDFC.DAPKNDBLKIA) // target is an impostor
                    {
                        PlayerControl_FixedUpdate.RpcSheriffKill(PlayerControl.LocalPlayer, __instance.CurrentTarget.PlayerId);
                    }
                    else
                    {
                        if (OptionsPatches.doKillSheriffsTarget)
                        {
                            // TODO: uhm
                            PlayerControl_FixedUpdate.RpcSheriffKill(__instance.CurrentTarget, __instance.CurrentTarget.PlayerId);
                        }
                        PlayerControl_FixedUpdate.RpcSheriffKill(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer.PlayerId);
                    }
                    __instance.SetTarget(null);
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
        static class PlayerControl_MurderPlayer
        {
            private static bool trueImpost = false;
            private static byte allowToKillMe = 255; // FIXED: desync when sheriff and impostor kill each other at the same time

            static void Prefix(ref PlayerControl __instance, [HarmonyArgument(0)] ref PlayerControl target)
            {
                if (target.PlayerId == PlayerControl.LocalPlayer.PlayerId && __instance.PlayerId == allowToKillMe)
                {
                    if (__instance.JLGGIOLCDFC.DLPCKPBIJOE)
                    {
                        __instance.JLGGIOLCDFC.DLPCKPBIJOE = false; // make the killer alive so we can accept his kill
                    }
                    else
                    {
                        allowToKillMe = 255; // if he's not dead here's (probably) no desync
                    }
                }
                trueImpost = __instance.JLGGIOLCDFC.DAPKNDBLKIA;
                bool suicide = __instance.PlayerId == target.PlayerId;
                if (IsSheriff(__instance.PlayerId) || suicide)
                {
                    if (!suicide) // sheriff killed an impostor
                    {
                        allowToKillMe = target.PlayerId;
                        Task.Run(async () =>
                        {
                            await Task.Delay(AmongUsClient.Instance.DGAKPKLMIEI * 4); // ping * 4
                            allowToKillMe = 255;
                        });
                    }
                    __instance.JLGGIOLCDFC.DAPKNDBLKIA = true;
                }
            }

            static void Postfix(ref PlayerControl __instance)
            {
                /*if (allowToKillMe == __instance.PlayerId)
                {
                    __instance.JLGGIOLCDFC.DLPCKPBIJOE = true; // make him dead back
                }*/
                __instance.JLGGIOLCDFC.DAPKNDBLKIA = trueImpost;
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.GHDDIAMOCLF), typeof(PlayerInfo))]
        static class MeetingHud_CreateButton
        {
            static void Postfix(ref PlayerInfo PPIKPNJEAKJ, ref PlayerVoteArea __result)
            {
                if(
                    PPIKPNJEAKJ.JKOMCOJCAID == PlayerControl.LocalPlayer.PlayerId &&
                    IsSheriff(PPIKPNJEAKJ.JKOMCOJCAID)
                )
                {
                    __result.NameText.Color = Palette.HPMGFCCJLIF;
                }
            }
        }
    }
}
