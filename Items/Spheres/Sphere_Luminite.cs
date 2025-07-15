using Terraria;
using Terraria.ID;

namespace VervPalMod.Items.Spheres {
    public class Sphere_Luminite : Sphere_Base_Item {
        public override int DustKind => DustID.FireworkFountain_Green;
        public override float BaseStrength => 0.5f;
        public override int CaughtAmount => 5;
        public override void SetDefaults() {
            base.SetDefaults();
            Item.rare = ItemRarityID.Red;
            Item.value = Item.buyPrice(0, 25, 0, 0);
            Item.shootSpeed = 15f;
        }
        public override void AddRecipes() => CreateRecipe()
            .AddIngredient<Sphere_Universal>()
            .AddIngredient(ItemID.LunarBar, 10)
            .AddTile(TileID.LunarCraftingStation)
            .Register();
    }
}