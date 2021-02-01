using System;
using System.IO;
using System.Linq;
using Hazel;
using LobbyOptionsAPI;

namespace CrowdedSheriff.Patches
{
    public class CustomGameOptionsData : LobbyOptions
    {
        private byte settingsVersion = 1;
        public static CustomGameOptionsData customGameOptions;

        public CustomGameOptionsData() : base(SheriffPlugin.Id, SheriffPlugin.rpcSettingsId)
        {
            sheriffCount = AddOption(0, "Sheriff count", 0, 9);
            doKillSheriffsTarget = AddOption(false, "Sheriff's target dies");
            sheriffKillCd = AddOption(30.0f, "Sheriff's kill cd", 10.0f, 60.0f, 2.5f, "s");
        }

        public CustomNumberOption sheriffCount;
        public CustomToggleOption doKillSheriffsTarget;
        public CustomNumberOption sheriffKillCd;

        public override void SetRecommendations()
        {
            sheriffCount.value = 0;
            doKillSheriffsTarget.value = false;
            sheriffKillCd.value = 30.0f;
        }

        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(this.settingsVersion);
            writer.Write((byte)sheriffCount.value);
            writer.Write(doKillSheriffsTarget.value);
            writer.Write(sheriffKillCd.value);
        }

        public override void Deserialize(BinaryReader reader)
        {
            try
            {
                SetRecommendations();
                byte b = reader.ReadByte();
                sheriffCount.value = reader.ReadByte();
                doKillSheriffsTarget.value = reader.ReadBoolean();
                sheriffKillCd.value = reader.ReadSingle();
            }
            catch
            {
            }
        }

        public override void Deserialize(MessageReader reader)
        {
            try
            {
                SetRecommendations();
                byte b = reader.ReadByte();
                sheriffCount.value = reader.ReadByte();
                doKillSheriffsTarget.value = reader.ReadBoolean();
                sheriffKillCd.value = reader.ReadSingle();
            }
            catch
            {
            }
        }

        public override string ToHudString()
        {
            settings.Length = 0;

            try
            {
                settings.AppendLine();
                settings.AppendLine($"Sheriff count: {sheriffCount.value}");
                if (sheriffCount.value != 0)
                {
                    settings.AppendLine($"Sheriff's target dies: {(doKillSheriffsTarget.value ? "On" : "Off")}");

                    settings.Append("Sheriff's kill cooldown: ");
                    settings.Append(sheriffKillCd.value);
                    settings.Append("s");
                    settings.AppendLine();
                }
            }
            catch
            {
            }

            return settings.ToString();
        }
    }
}