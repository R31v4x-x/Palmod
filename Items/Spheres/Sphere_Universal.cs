using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VervPalMod.Items.Spheres {
    public class Sphere_Universal : Sphere_Base_Item {
        static bool? enabled = null;
        public static bool Enabled => enabled ??= ModLoader.HasMod(nameof(AltLibrary));
        public override float BaseStrength => 0.35f;
        public override int CaughtAmount => 3;
        public override void SetDefaults() {
            base.SetDefaults();
            Item.rare = ItemRarityID.LightPurple;
            Item.value = Item.buyPrice(0, 5, 0, 0);
            Item.shootSpeed = 15f;
        }
        public override void AddRecipes() {
            Recipe r = CreateRecipe()
                .AddIngredient<Sphere_Jungle>()
                .AddIngredient<Sphere_Underground>()
                .AddIngredient<Sphere_Desert>()
                .AddIngredient<Sphere_Hell>()
                .AddIngredient<Sphere_Evil>()
                .AddIngredient<Sphere_Holy>()
                .AddIngredient<Sphere_Sky>()
                .AddIngredient<Sphere_Dungeon>()
                .AddIngredient(ItemID.SoulofMight, 5)
                .AddIngredient(ItemID.SoulofSight, 5)
                .AddIngredient(ItemID.SoulofFright, 5);
            if (Enabled) r.AddRecipeGroup("HallowBars", 20);
            else r.AddIngredient(ItemID.HallowedBar, 20);

            r.AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}