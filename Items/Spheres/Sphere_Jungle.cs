using AltLibrary;
using AltLibrary.Common.AltBiomes;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VervPalMod.Items.Spheres {
    public class Sphere_Jungle : Sphere_Base_Item {
        static bool? enabled = null;
        public static bool Enabled => enabled ??= ModLoader.HasMod(nameof(AltLibrary));
        public override int DustKind => DustID.FireworkFountain_Green;
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
                .AddIngredient<Sphere_Normal>()
                .AddIngredient(ItemID.Stinger, 8)
                .AddIngredient(ItemID.Vine, 3);
            if (Enabled) r.AddRecipeGroup("JungleSpores", 15);
            else r.AddIngredient(ItemID.JungleSpores, 15);

            r.AddTile(TileID.Anvils)
                .Register();
        }
        public bool IsInBiome(Player player) {
            [JITWhenModsEnabled(nameof(AltLibrary))]
            bool ALBiome() {
                foreach (AltBiome biome in AltLibrary.AltLibrary.AllBiomes) {
                    if (biome.BiomeType == BiomeType.Jungle && (biome.Biome?.IsInBiome(player) ?? false)) return true;
                }
                return false;
            }
            return (Enabled ? ALBiome() : player.ZoneJungle) || player.ZoneGlowshroom;
        }
    }
}