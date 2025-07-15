using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace VervPalMod {
    public static class VervPalMod_Helpers {
        public static VervPalModPlayer PalPlayer(this Player player) => player.GetModPlayer<VervPalModPlayer>();
        public static string GetModLocalization(string key) => Language.GetTextValue(ModContent.GetInstance<VervPalMod>().GetLocalizationKey(key));
    }
}
