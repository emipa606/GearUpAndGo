using UnityEngine;
using Verse;

namespace GearUpAndGo
{
    internal class Settings : ModSettings
    {
        public string betterPawnControlBattlePolicy = "";

        public static Settings Get()
        {
            return LoadedModManager.GetMod<Mod>().GetSettings<Settings>();
        }

        public void DoWindowContents(Rect wrect)
        {
            var options = new Listing_Standard();
            options.Begin(wrect);

            options.Label("TD.SettingBetterPawnControlPolicy".Translate());
            betterPawnControlBattlePolicy = options.TextEntry(betterPawnControlBattlePolicy);
            options.Label("TD.SettingBetterPawnControlRemembered".Translate());
            options.Gap();

            options.End();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref betterPawnControlBattlePolicy, "betterPawnControlBattlePolicy", "");
        }
    }
}