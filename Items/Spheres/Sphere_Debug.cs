using Terraria;
using Terraria.ID;

namespace VervPalMod.Items.Spheres {
    public class Sphere_Debug : Sphere_Base_Item {
        public override int DustKind => DustID.FireworkFountain_Yellow;
        public override float BaseStrength => 1;
        public override int CaughtAmount => 10;
        public override void SetDefaults() {
            base.SetDefaults();
            Item.rare = ItemRarityID.Quest;
            Item.value = 0;
            Item.shootSpeed = 15f;
        }
    }
}