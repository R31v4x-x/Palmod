using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;
using VervPalMod.Items.Spheres;

namespace VervPalMod.UIs
{
    public class UIStateSphere : ModUIState
    {
        public static Texture2D TextureArrow;
        public static Texture2D TextureStop;
        public static Texture2D TextureTamed;

        public override int InsertionIndex(List<GameInterfaceLayer> layers)
            => layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));

        public override void OnInitialize()
        {
            TextureArrow ??= ModContent.Request<Texture2D>("VervPalMod/UIs/Textures/Arrow", AssetRequestMode.ImmediateLoad).Value;
            TextureStop ??= ModContent.Request<Texture2D>("VervPalMod/UIs/Textures/Stop", AssetRequestMode.ImmediateLoad).Value;
            TextureTamed ??= ModContent.Request<Texture2D>("VervPalMod/UIs/Textures/Tamed", AssetRequestMode.ImmediateLoad).Value;

            Width.Set(0f, 0f);
            Height.Set(0f, 0f);
            Left.Set(Main.screenWidth / 2, 0f);
            Top.Set(Main.screenHeight / 2, 0f);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Player player = Main.LocalPlayer;
            VervPalModPlayer modPlayer = player.GetModPlayer<VervPalModPlayer>();

            if (player.HeldItem.ModItem is Sphere_Base_Item sphere)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

                foreach (NPC npc in Main.npc)
                {
                    if (npc.active)
                    {
                        if (modPlayer.TamedNPCsCheck[npc.whoAmI])
                        {
                            if (npc.realLife == -1 || npc.realLife == npc.whoAmI)
                            {
                                Vector2 position = new Vector2(npc.Center.X, npc.position.Y + npc.gfxOffY - 32) - Main.screenPosition;
                                Color color = Lighting.GetColor((int)(npc.Center.X / 16f), (int)(npc.Center.Y / 16f));
                                spriteBatch.Draw(TextureTamed, position, null, color * 0.85f, 0f, TextureStop.Size() * 0.5f, 1f, SpriteEffects.None, 0f);
                            }
                        }
                        else if (!npc.friendly && npc.life > 0 && !npc.dontTakeDamage && VervPalModGlobalNPC.CanBeTamed(npc))
                        {
                            Vector2 position = new Vector2(npc.Center.X, npc.position.Y + npc.gfxOffY - 32) - Main.screenPosition;
                            Color color = Lighting.GetColor((int)(npc.Center.X / 16f), (int)(npc.Center.Y / 16f));

                            float threshold = sphere.GetCaptureThreshold(player);
                            if (npc.realLife == -1 || npc.realLife == npc.whoAmI)
                            {
                                if (npc.life > npc.lifeMax * threshold && (npc.life + npc.defense * 0.5) > (threshold * 100 * (Main.hardMode ? 3 : 1))) spriteBatch.Draw(TextureStop, position, null, color * 0.75f, 0f, TextureStop.Size() * 0.5f, 1f, SpriteEffects.None, 0f);
                                else spriteBatch.Draw(TextureArrow, position, null, color * 0.8f, 0f, TextureArrow.Size() * 0.5f, 1f, SpriteEffects.None, 0f);
                            }
                        }
                    }

                }

                if (modPlayer.SphereUI && !player.dead)
                {
                    Vector2 position = (player.position + new Vector2(player.width * 0.5f, player.gravDir > 0 ? player.height + player.gfxOffY : 10 + player.gfxOffY)).Floor();
                    position = Vector2.Transform(position - Main.screenPosition, Main.GameViewMatrix.EffectMatrix * Main.GameViewMatrix.ZoomMatrix);
                    Vector2 position2 = position;

                    string text = Language.GetTextValue(ModContent.GetInstance<VervPalMod>().GetLocalizationKey("Common.Slots")) + " : " + modPlayer.CurrentTamedNPCs + " /" + modPlayer.MaxTamedNPCs;
                    position2.X -= FontAssets.MouseText.Value.MeasureString(text).X * 0.5f;
                    ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, text, position2, Color.White, 0f, Vector2.Zero, new Vector2(1, 1f));

                    position2 = position;
                    position2.Y += 20;
                    if (modPlayer.TamedIDs.Count > 0) text = Lang.GetNPCNameValue(modPlayer.TamedIDs[modPlayer.SphereUIIndex].Item1);
                    else text = Language.GetTextValue(ModContent.GetInstance<VervPalMod>().GetLocalizationKey("Common.NoCaptures"));

                    position2.X -= FontAssets.MouseText.Value.MeasureString(text).X * 0.5f;
                    ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, text, position2, Color.White, 0f, Vector2.Zero, new Vector2(1, 1f));
                }

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
            }
        }
    }
}