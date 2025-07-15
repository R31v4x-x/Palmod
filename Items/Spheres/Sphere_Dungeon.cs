using Terraria;
using Terraria.ID;

namespace VervPalMod.Items.Spheres {
    public class Sphere_Dungeon : Sphere_Base_Item {
        public override int DustKind => DustID.FireworksRGB;
        public override void SetDefaults() {
            base.SetDefaults();
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(0, 1, 50, 0);
            Item.shootSpeed = 12.5f;
        }
        public override float CaptureStrengthModifier(Player player) {
            return player.ZoneDungeon ? 0.2f : 0;
        }
        public override void AddRecipes() => CreateRecipe()
            .AddIngredient<Sphere_Normal>()
            .AddIngredient(ItemID.Bone, 25)
            .AddIngredient(ItemID.WaterCandle, 5)
            .AddTile(TileID.Anvils)
            .Register();
    }
}