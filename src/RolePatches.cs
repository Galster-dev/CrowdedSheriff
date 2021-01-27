using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using Hazel;


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

        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.SelectInfected))]
        static class ShipStatus_SelectInfected
        {
            static void Postfix()
            {
                impostors.Clear();
                sheriffs.Clear();
                var fake = new List<GameData.PlayerInfo>();
                foreach (var p in GameData.Instance.AllPlayers)
                {
                    fake.Add(p);
                }

                impostors = fake.Where(p => p.IsImpostor).Select(p => p.PlayerId).ToList();
                sheriffs = fake.Where(p => !p.Disconnected && !p.IsDead && !p.IsImpostor)
                    .Select(p => p.PlayerId)
                    .OrderBy(p => Guid.NewGuid()) // shuffle
                    .Take(OptionsPatches.sheriffCount)
                    .ToList();

                var writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId,
                    (byte) CustomRpc.SelectSheriffs, Hazel.SendOption.Reliable);
                writer.WriteBytesAndSize(sheriffs.ToArray());
                writer.EndMessage();
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc),
            new Type[] {typeof(byte), typeof(MessageReader)})]
        static class PlayerControl_HandleRpc
        {
            static bool Prefix(byte HKHMBLJFLMC, MessageReader ALMCIJKELCP)
            {
                switch ((CustomRpc) HKHMBLJFLMC)
                {
                    case CustomRpc.SelectSheriffs:
                        sheriffs = ALMCIJKELCP.ReadBytesAndSize().ToList();
                        break;
                    case CustomRpc.SheriffKill:
                        var sheriff = GameData.Instance.GetPlayerById(ALMCIJKELCP.ReadByte())?.Object;
                        PlayerControl_FixedUpdate.SheriffKill(sheriff, ALMCIJKELCP.ReadByte());
                        break;

                    default:
                        return true;
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(IntroCutscene.CoBegin__d), nameof(IntroCutscene.CoBegin__d.MoveNext))]
        static class IntroCutScene_CoBegin
        {
            static void Postfix(ref IntroCutscene.CoBegin__d __instance)
            {
                if (IsSheriff(PlayerControl.LocalPlayer.PlayerId))
                {
                    __instance.__this.Title.Text = "Sheriff";
                    PlayerControl.LocalPlayer.nameText.Color = __instance.__this.Title.Color = Palette.Orange;
                    //localscene.Title.scale /= 1.5f; // "Sheriff" isn't that big as "The Sheriff"
                    __instance.__this.ImpostorText.Text = "Kill the [FF0000FF]Impostor";
                    __instance.__this.BackgroundBar.material.color = Palette.Orange;
                    PlayerControl_FixedUpdate.SetSheriffKillTimer(PlayerControl.LocalPlayer,
                        15f); //Useless ? mby for % fix
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetInfected))]
        static class PlayerControl_SetInfected
        {
            static void Postfix()
            {
                PlayerControl_FixedUpdate.SetSheriffKillTimer(PlayerControl.LocalPlayer,
                    15f); // FIXED: ability to kill on intro scene
            }
        }


        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
        static class PlayerControl_FixedUpdate
        {
            public static void SetSheriffKillTimer(PlayerControl __instance, float time)
            {
                __instance.killTimer = time;
                if (OptionsPatches.sheriffKillCd > 0f)
                {
                    DestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(__instance.killTimer,
                        OptionsPatches.sheriffKillCd);
                    return;
                }

                DestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(0f, OptionsPatches.sheriffKillCd);
            }

            public static void SheriffKill(PlayerControl sheriff, byte targetId)
            {
                if (sheriff is null) return;
                if (!IsSheriff(sheriff.PlayerId) && sheriff.PlayerId != targetId) return;
                var target = GameData.Instance.GetPlayerById(targetId)?.Object;
                if (target is null) return;
                sheriff.MurderPlayer(target);
            }

            public static void RpcSheriffKill(PlayerControl sheriff, byte targetId)
            {
                if (AmongUsClient.Instance.AmClient)
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
                float distance =
                    GameOptionsData.KillDistances[Mathf.Clamp(PlayerControl.GameOptions.KillDistance, 0, 2)];
                var truePos = local.GetTruePosition();

                foreach (var p in GameData.Instance.AllPlayers)
                {
                    if (!p.Disconnected && p.PlayerId != local.PlayerId && !p.IsDead)
                    {
                        var control = p.Object;
                        if (control != null)
                        {
                            var vector = control.GetTruePosition() - truePos;
                            float magnitude = vector.magnitude;
                            if (magnitude <= distance && !PhysicsHelpers.AnyNonTriggersBetween(truePos,
                                vector.normalized, magnitude, Constants.InfinitySymbol))
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
                if (__instance.AmOwner && IsSheriff(__instance.PlayerId) && __instance.CanMove)
                {
                    if (!__instance.Data.IsDead)
                    {
                        SetSheriffKillTimer(__instance, Mathf.Max(0f, __instance.killTimer - Time.fixedDeltaTime));
                        HudManager._instance.KillButton.SetTarget(FindClosestTarget(ref __instance));
                    }
                    if(!HudManager._instance.KillButton.gameObject.active)
                        HudManager._instance.KillButton.gameObject.SetActive(true);
                }
                if (__instance.AmOwner && HudManager._instance.KillButton.gameObject.active && __instance.Data.IsDead)
                    HudManager._instance.KillButton.gameObject.SetActive(false);
            }
        }

        [HarmonyPatch(typeof(ExileController), nameof(ExileController.Method_37))] //WrapUp
        static class ExileController_WrapUp
        {
            static void Postfix()
            {
                if (DestroyableSingleton<TutorialManager>.InstanceExists || !ShipStatus.Instance.IsGameOverDueToDeath())
                {
                    if (IsSheriff(PlayerControl.LocalPlayer.PlayerId) && !PlayerControl.LocalPlayer.Data.IsDead)
                        PlayerControl_FixedUpdate.SetSheriffKillTimer(PlayerControl.LocalPlayer,
                            PlayerControl.GameOptions.KillCooldown);
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

        [HarmonyPatch(typeof(KillButtonManager), nameof(KillButtonManager.PerformKill))]
        static class KillButtonManager_PerformKill
        {
            static bool Prefix(ref KillButtonManager __instance)
            {
                if (IsSheriff(PlayerControl.LocalPlayer.PlayerId) && __instance.isActiveAndEnabled &&
                    __instance.CurrentTarget && !__instance.isCoolingDown &&
                    !PlayerControl.LocalPlayer.Data.IsImpostor && PlayerControl.LocalPlayer.CanMove)
                {
                    if (__instance.CurrentTarget.Data.IsImpostor) // target is an impostor
                    {
                        PlayerControl_FixedUpdate.RpcSheriffKill(PlayerControl.LocalPlayer,
                            __instance.CurrentTarget.PlayerId);
                    }
                    else
                    {
                        if (OptionsPatches.doKillSheriffsTarget)
                        {
                            // TODO: uhm
                            PlayerControl_FixedUpdate.RpcSheriffKill(__instance.CurrentTarget,
                                __instance.CurrentTarget.PlayerId);
                        }

                        PlayerControl_FixedUpdate.RpcSheriffKill(PlayerControl.LocalPlayer,
                            PlayerControl.LocalPlayer.PlayerId);
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

            private static byte
                allowToKillMe = 255; // FIXED: desync when sheriff and impostor kill each other at the same time

            [HarmonyPriority(Priority.High)]
            static void Prefix(ref PlayerControl __instance, [HarmonyArgument(0)] ref PlayerControl target)
            {
                if (target.PlayerId == PlayerControl.LocalPlayer.PlayerId && __instance.PlayerId == allowToKillMe)
                {
                    if (__instance.Data.IsDead)
                    {
                        __instance.Data.IsDead = false; // make the killer alive so we can accept his kill
                    }
                    else
                    {
                        allowToKillMe = 255; // if he's not dead here's (probably) no desync
                    }
                }

                trueImpost = __instance.Data.IsImpostor;
                bool suicide = __instance.PlayerId == target.PlayerId;
                if (IsSheriff(__instance.PlayerId) || suicide)
                {
                    if (!suicide) // sheriff killed an impostor
                    {
                        allowToKillMe = target.PlayerId;
                        Task.Run(async () =>
                        {
                            await Task.Delay(AmongUsClient.Instance.Ping * 4); // ping * 4
                            allowToKillMe = 255;
                        });
                    }

                    __instance.Data.IsImpostor = true;
                }
            }

            [HarmonyPriority(Priority.Low)]
            static void Postfix(ref PlayerControl __instance)
            {
                /*if (allowToKillMe == __instance.PlayerId)
                {
                    __instance.Data.IsDead = true; // make him dead back
                }*/
                __instance.Data.IsImpostor = trueImpost;
                if (IsSheriff(__instance.PlayerId))
                {
                    PlayerControl_FixedUpdate.SetSheriffKillTimer(__instance, OptionsPatches.sheriffKillCd);
                }
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Method_7), typeof(GameData.PlayerInfo))]
        static class MeetingHud_CreateButton
        {
            static void Postfix(ref GameData.PlayerInfo PPIKPNJEAKJ, ref PlayerVoteArea __result)
            {
                if (
                    PPIKPNJEAKJ.PlayerId == PlayerControl.LocalPlayer.PlayerId &&
                    IsSheriff(PPIKPNJEAKJ.PlayerId)
                )
                {
                    __result.NameText.Color = Palette.Orange;
                }
            }
        }
    }
}