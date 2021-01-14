using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;

namespace CrowdedSheriff
{
    [BepInPlugin(id, "Sheriff role mod", version)]
    public class SheriffPlugin : BasePlugin
    {
        public const string id = "ru.galster.sheriffmod";
        public const string version = "0.3.10";
        public static ManualLogSource Logger; 
        public Harmony Harmony { get; } = new Harmony(id); 

        public override void Load()
        {
            Harmony.PatchAll();
            Logger = Log;
        }
    }
}
