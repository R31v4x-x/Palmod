using AltLibrary;
using AltLibrary.Common.AltBiomes;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VervPalMod.Items.Spheres {
    public class Sphere_Evil : Sphere_Base_Item {
        static bool? enabled = null;
        public static bool Enabled => enabled ??= ModLoader.HasMod(nameof(AltLibrary));
        public override int DustKind => DustID.FireworkFountain_Pink;
        public override void SetDefaults() {
            base.SetDefaults();
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.buyPrice(0, 1, 50, 0);
            Item.shootSpeed = 12.5f;
        }
        public override float CaptureStrengthModifier(Player player)
        {
            return IsInBiome(player) ? 0.2f : 0;
        }
        public override void Unload() {
            enabled = null;
        }
        public override void AddRecipes() {
            Recipe r = CreateRecipe()
                .AddIngredient<Sphere_Normal>();
            if (Enabled) r.AddRecipeGroup("EvilBars", 10)
                .AddRecipeGroup("ShadowScales", 15);
            else {
                Recipe r1 = r.Clone();
                r.AddIngredient(ItemID.DemoniteBar, 10)
                .AddIngredient(ItemID.ShadowScale, 15);

                r1.AddIngredient(ItemID.CrimtaneBar, 10)
                .AddIngredient(ItemID.TissueSample, 15)
                .AddTile(TileID.Anvils)
                .Register();
            }

            r.AddTile(TileID.Anvils)
                .Register();
        }
        public bool IsInBiome(Player player) {
            [JITWhenModsEnabled(nameof(AltLibrary))]
            bool ALBiome() {
                foreach (AltBiome biome in AltLibrary.AltLibrary.AllBiomes) {
                    if (biome.BiomeType == BiomeType.Evil && (biome.Biome?.IsInBiome(player) ?? false)) return true;
                }
                return false;
            }
            return Enabled ? ALBiome() : (player.ZoneCorrupt || player.ZoneCrimson);
        }
    }
}