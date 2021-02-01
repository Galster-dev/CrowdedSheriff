using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using CrowdedSheriff.Patches;
using HarmonyLib;

namespace CrowdedSheriff
{
    [BepInPlugin(Id, "Sheriff role mod", version)]
    public class SheriffPlugin : BasePlugin
    {
        public const string Id = "ru.galster.sheriffmod";
        public const string version = "0.4.2";
        public static byte rpcSettingsId = 80;
        public static ManualLogSource Logger; 
        public Harmony Harmony { get; } = new Harmony(Id); 

        public override void Load()
        {
            CustomGameOptionsData.customGameOptions = new CustomGameOptionsData();
            Harmony.PatchAll();
            Logger = Log;
        }
    }
}
