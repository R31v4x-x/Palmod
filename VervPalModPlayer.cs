using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using VervPalMod.Items.Spheres;

namespace VervPalMod
{
    public class VervPalModPlayer : ModPlayer
    {
        public bool SphereUI = false;
        public bool NoKillMode = false;
        public int TeleportCooldown = 300;

        public NPC Target;
        public int SphereUIIndex = 0;
        public List<Tuple<int, int>> TamedIDs; // ID, Amount
        public int keepSelected = -1;

        public byte MinionBuffer = 0;

        public int CurrentTamedNPCs = 0;
        public int MaxTamedNPCs => Player.maxMinions * 2;

        public bool[] TamedNPCsCheck = new bool[255];
        public bool[] TamedNPCsCheckNoCount = new bool[255];

        public override void OnEnterWorld()
        {
            TamedIDs = new List<Tuple<int, int>>();
            TamedNPCsCheck = new bool[Main.npc.Length];
            for (int i = 0; i < TamedNPCsCheck.Length; i++) TamedNPCsCheck[i] = false;
            for (int i = 0; i < TamedNPCsCheckNoCount.Length; i++) TamedNPCsCheckNoCount[i] = false;
        }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (Player.whoAmI == Main.myPlayer)
            {
                if (ModKeybindsLoader.Teleport.JustPressed && Player.HeldItem.ModItem is Sphere_Base_Item)
                {
                    if (TeleportCooldown >= 300)
                    {
                        SoundEngine.PlaySound(SoundID.DD2_LightningBugZap, Player.Center);
                        TeleportCooldown = 0;
                        CombatText.NewText(Player.Hitbox, Color.LightBlue, Language.GetTextValue(ModContent.GetInstance<VervPalMod>().GetLocalizationKey("Common.TeleportedCaptures")));

                        foreach (NPC npc in Main.npc)
                        {
                            if (npc.active && npc.GetGlobalNPC<VervPalModGlobalNPC>().Tamed && npc.aiStyle != NPCAIStyleID.Spell)
                            {
                                npc.position = Player.Center + Collision.TileCollision(Player.Center, Vector2.UnitY.RotatedByRandom(MathHelper.PiOver2) * -64f, npc.width, npc.height);
                            }
                        }
                    }
                    else
                    {
                        SoundEngine.PlaySound(SoundID.MenuTick, Player.Center);
                        CombatText.NewText(Player.Hitbox, Color.LightGray, Language.GetTextValue(ModContent.GetInstance<VervPalMod>().GetLocalizationKey("Common.TeleportOnCooldown")), false, true);
                    }
                }

                if (ModKeybindsLoader.NoKill.JustPressed && Player.HeldItem.ModItem is Sphere_Base_Item)
                {
                    NoKillMode = !NoKillMode;
                    if (NoKillMode) CombatText.NewText(Player.Hitbox, Color.LightBlue, Language.GetTextValue(ModContent.GetInstance<VervPalMod>().GetLocalizationKey("Common.CaptureFormation")));
                    else CombatText.NewText(Player.Hitbox, Color.LightPink, Language.GetTextValue(ModContent.GetInstance<VervPalMod>().GetLocalizationKey("Common.CombatFormation")));
                    SoundEngine.PlaySound(SoundID.MenuTick, Player.Center);
                }

                if (ModKeybindsLoader.Release.JustPressed)
                {
                    SoundEngine.PlaySound(SoundID.DD2_LightningBugZap, Player.Center);
                    TeleportCooldown = 0;

                    bool anyReleased = false;
                    foreach (NPC npc in Main.npc)
                    {
                        if (npc.active && npc.GetGlobalNPC<VervPalModGlobalNPC>().Tamed)
                        {
                            anyReleased = true;
                            npc.active = false;

                            for (int i = 0; i < npc.width; i += 2)
                            {
                                Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Electric);
                                dust.noGravity = true;
                            }
                        }
                    }

                    if (anyReleased) CombatText.NewText(Player.Hitbox, Color.LightBlue, Language.GetTextValue(ModContent.GetInstance<VervPalMod>().GetLocalizationKey("Common.CapturesReleased")));
                    else CombatText.NewText(Player.Hitbox, Color.LightGray, Language.GetTextValue(ModContent.GetInstance<VervPalMod>().GetLocalizationKey("Common.NothingToRelease")), false, true);
                }
            }
        }

        public override void PostUpdate()
        {
        }

        public override void ResetEffects()
        {
            if (Player.active)
            {
                TeleportCooldown++;
                CurrentTamedNPCs = 0;

                if (MinionBuffer < 60) MinionBuffer++;
                if (Player.numMinions > 0) MinionBuffer = 0;

                if (keepSelected != -1)
                {
                    Player.selectedItem = keepSelected;
                    keepSelected = -1;
                }

                if (!(Player.HeldItem.ModItem is Sphere_Base_Item)) SphereUI = false;

                if (Player.active)
                {
                    NPC closest = null;
                    float shortestDist = 900f;
                    for (int i = 0; i < TamedNPCsCheck.Length; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (npc.active)
                        {
                            VervPalModGlobalNPC globalNPC = npc.GetGlobalNPC<VervPalModGlobalNPC>();
                            if (!globalNPC.Tamed && TamedNPCsCheck[i])
                            {
                                globalNPC.Tamed = true;
                                npc.friendly = true;
                                npc.netUpdate = true;
                            }

                            float dist = npc.Center.Distance(Player.Center);
                            if (npc.aiStyle != NPCAIStyleID.Spell)
                            {
                                if (globalNPC.Tamed && (npc.realLife == -1 || npc.realLife == npc.whoAmI))
                                {
                                    if (!TamedNPCsCheckNoCount[npc.whoAmI]) CurrentTamedNPCs++;
                                    TamedNPCsCheck[i] = true;
                                }

                                if (!npc.friendly && npc.life > 0 && dist < shortestDist && !npc.dontTakeDamage && !npc.CountsAsACritter && (!NoKillMode || VervPalModGlobalNPC.CanBeTamed(npc)))
                                {
                                    shortestDist = dist;
                                    closest = npc;
                                }
                            }
                        }
                        else
                        {
                            TamedNPCsCheck[i] = false;
                            TamedNPCsCheckNoCount[i] = false;
                        }
                    }
                    Target = closest;

                    foreach (Projectile projectile in Main.projectile)
                    {
                        if (projectile.ModProjectile is Sphere_Base_Proj && projectile.ai[0] > -1 && projectile.owner == Player.whoAmI && projectile.active)
                        {
                            CurrentTamedNPCs++;
                        }
                    }
                }
            }
        }
    }
}