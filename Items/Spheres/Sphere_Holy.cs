using AltLibrary;
using AltLibrary.Common.AltBiomes;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VervPalMod.Items.Spheres {
    public class Sphere_Holy: Sphere_Base_Item {
        static bool? enabled = null;
        public static bool Enabled => enabled ??= ModLoader.HasMod(nameof(AltLibrary));
        public override int DustKind => DustID.FireworksRGB;
        public override void SetDefaults() {
            base.SetDefaults();
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.buyPrice(0, 1, 50, 0);
            Item.shootSpeed = 12.5f;
        }
        public override float CaptureStrengthModifier(Player player) {
            return IsInBiome(player) ? 0.2f : 0;
        }
        public override void AddRecipes() {
            Recipe r = CreateRecipe()
                .AddIngredient<Sphere_Normal>();
            if (Enabled) r.AddRecipeGroup("PixieDusts", 10)
                .AddRecipeGroup("UnicornHorns", 3)
                .AddRecipeGroup("CrystalShards", 20);
            else r.AddIngredient(ItemID.PixieDust, 10)
                .AddIngredient(ItemID.UnicornHorn, 3)
                .AddIngredient(ItemID.CrystalShard, 20);

            r.AddTile(TileID.Anvils)
                .Register();
        }
        public bool IsInBiome(Player player) {
            [JITWhenModsEnabled(nameof(AltLibrary))]
            bool ALBiome() {
                foreach (AltBiome biome in AltLibrary.AltLibrary.AllBiomes) {
                    if (biome.BiomeType == BiomeType.Hallow && (biome.Biome?.IsInBiome(player) ?? false)) return true;
                }
                return false;
            }
            return Enabled ? ALBiome() : player.ZoneHallow;
        }
    }
}