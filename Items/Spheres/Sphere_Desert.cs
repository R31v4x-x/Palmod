using Terraria;
using Terraria.ID;

namespace VervPalMod.Items.Spheres {
    public class Sphere_Desert : Sphere_Base_Item {
        public override int DustKind => DustID.FireworkFountain_Yellow;
        public override void SetDefaults() {
            base.SetDefaults();
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.buyPrice(0, 1, 50, 0);
            Item.shootSpeed = 12.5f;
        }
        public override float CaptureStrengthModifier(Player player) {
            return player.ZoneDesert ? 0.2f : 0;
        }
        public override void AddRecipes() => CreateRecipe()
            .AddIngredient<Sphere_Normal>()
            .AddIngredient(ItemID.AntlionMandible, 5)
            .AddIngredient(ItemID.FossilOre, 15)
            .AddIngredient(ItemID.Amber, 8)
            .AddTile(TileID.Anvils)
            .Register();
    }
}