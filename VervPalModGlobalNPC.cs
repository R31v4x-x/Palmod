using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VervPalMod.Items.Spheres;

namespace VervPalMod
{
    public class VervPalModGlobalNPC : GlobalNPC
    {
        private int AttackCooldown = 0;
        private int TimeAlive = 0;
        public float[] Ai = new float[4] { 0, 0, 0, 0 };
        public bool Tamed = false;

        public static List<int> PossibleTypes = new List<int>()
        {
            NPCAIStyleID.Fighter,
            NPCAIStyleID.Bat,
            NPCAIStyleID.GiantTortoise,
            NPCAIStyleID.Flying,
            NPCAIStyleID.DemonEye,
            NPCAIStyleID.Slime,
            NPCAIStyleID.Spider,
            NPCAIStyleID.Worm,
            NPCAIStyleID.FlyingFish,
            NPCAIStyleID.Caster,
            NPCAIStyleID.DungeonSpirit,
            NPCAIStyleID.HoveringFighter,
            NPCAIStyleID.Piranha,
            NPCAIStyleID.Vulture,
            NPCAIStyleID.SkeletronHead
        };
        

        public static List<int> BlacklistedTypes = new List<int>()
        {
            NPCID.Medusa,
            NPCID.DesertDjinn,
            NPCID.EaterofWorldsBody,
            NPCID.EaterofWorldsHead,
            NPCID.EaterofWorldsTail
        };
        

        public static List<int> ExceptionTypes = new List<int>()
        {
            NPCID.DungeonGuardian,
            NPCID.Grubby
        };

        public static bool CanBeTamed(NPC npc) => ((PossibleTypes.Contains(npc.aiStyle) && !npc.boss && !npc.CountsAsACritter) || ExceptionTypes.Contains(npc.type)) && !BlacklistedTypes.Contains(npc.type);

        public override bool InstancePerEntity => true;


        public override void ModifyShop(NPCShop shop)
        {
            if (shop.NpcType == NPCID.Merchant)
            {
                shop.Add<Sphere_Normal>();
            }
        }

        public override void OnSpawn(NPC npc, IEntitySource source)
        {
            VervPalModPlayer modPlayer = Main.LocalPlayer.GetModPlayer<VervPalModPlayer>();
            if (source is EntitySource_Parent parent)
            {
                if (parent.Entity is NPC npcParent && (npcParent.GetGlobalNPC<VervPalModGlobalNPC>().Tamed || modPlayer.TamedNPCsCheck[npcParent.whoAmI]))
                {
                    modPlayer.TamedNPCsCheck[npc.whoAmI] = true;
                    modPlayer.TamedNPCsCheckNoCount[npc.whoAmI] = true;
                    Tamed = true;
                    npc.friendly = true;
                }
            }
        }

        public override bool CheckActive(NPC npc)
        {
            if (Tamed && npc.aiStyle != NPCAIStyleID.Spell) return false;
            return base.CheckActive(npc);
        }

        public override bool CanBeHitByNPC(NPC npc, NPC attacker)
        {
            VervPalModPlayer modPlayer = Main.LocalPlayer.GetModPlayer<VervPalModPlayer>();
            if (Tamed && npc.realLife != npc.whoAmI && npc.realLife != -1) return false;
            if (modPlayer.TamedNPCsCheck[npc.whoAmI] && modPlayer.TamedNPCsCheck[attacker.whoAmI]) return false;
            return base.CanBeHitByNPC(npc, attacker);
        }

        public override bool? CanBeHitByItem(NPC npc, Player player, Item item)
        {
            if (player.GetModPlayer<VervPalModPlayer>().TamedNPCsCheck[npc.whoAmI]) return false;
            return base.CanBeHitByItem(npc, player, item);
        }

        public override bool? CanBeHitByProjectile(NPC npc, Projectile projectile)
        {
            if (Main.LocalPlayer.GetModPlayer<VervPalModPlayer>().TamedNPCsCheck[npc.whoAmI] && projectile.friendly) return false;
            return base.CanBeHitByProjectile(npc, projectile);
        }

        public override void AI(NPC npc)
        {
            if (Tamed)
            {
                AttackCooldown++;
                TimeAlive++;
                if (AttackCooldown >= 30) Attack(npc);

                Player player = Main.LocalPlayer;
                NPC target = player.GetModPlayer<VervPalModPlayer>().Target;
                Vector2 targetPosition = player.Center;
                if (target != null) targetPosition = target.Center;

                foreach (NPC npc2 in Main.npc)
                {
                    if (npc2.realLife == npc.whoAmI && !npc2.friendly && npc2.active)
                    {
                        VervPalModGlobalNPC globalNPC = npc2.GetGlobalNPC<VervPalModGlobalNPC>();
                        npc2.friendly = true;
                        globalNPC.Tamed = true;
                        npc2.netUpdate = true;
                    }
                }

                switch (npc.aiStyle)
                {
                    case NPCAIStyleID.Fighter:
                        if (target != null)
                        {
                            if (Collision.CanHit(npc.Center, 1, 1, target.Center, 1, 1))
                            {
                                targetPosition = target.Center;
                            }
                            else
                            {
                                targetPosition = player.Center;
                            }
                        }

                        Ai[0]--;
                        Ai[1]++;
                        if (npc.frameCounter != Ai[2] || Main.npcFrameCount[npc.type] < 5)
                        {
                            Ai[2] = (int)npc.frameCounter;
                            Ai[1] = 0;
                        }

                        float targetLocation = targetPosition.X + (float)Math.Sin(TimeAlive * 0.1f) * 32f;
                        Vector2 forcedVelocity = Vector2.Zero;
                        float length = targetLocation - npc.Center.X;
                        if (Math.Abs(length) > 20f) length = 20f * Math.Sign(length);
                        forcedVelocity.X = length * 0.1f;
                        if (Ai[0] <= 0 && Ai[1] < 10) npc.velocity.X = forcedVelocity.X;
                        npc.direction = Math.Sign(forcedVelocity.X);

                        if (targetPosition.Y < (npc.Center.Y - npc.height) && npc.collideY && npc.velocity.Y >= 0 && Main.npcFrameCount[npc.type] < 5)
                        { // jump
                            npc.velocity.Y = -8f;
                            npc.direction = Math.Sign(npc.velocity.X);
                        }
                        break;
                    case NPCAIStyleID.CritterWorm:
                        Ai[0]--;
                        Ai[1]++;
                        if (npc.frameCounter != Ai[2] || Main.npcFrameCount[npc.type] < 5)
                        {
                            Ai[2] = (int)npc.frameCounter;
                            Ai[1] = 0;
                        }

                        npc.friendly = true;
                        npc.catchItem = 0;

                        targetLocation = targetPosition.X + (float)Math.Sin(TimeAlive * 0.1f) * 32f;
                        forcedVelocity = Vector2.Zero;
                        length = targetLocation - npc.Center.X;
                        if (Math.Abs(length) > 20f) length = 20f * Math.Sign(length);
                        forcedVelocity.X = length * 0.02f;
                        if (Ai[0] <= 0 && Ai[1] < 10) npc.velocity.X = forcedVelocity.X;
                        npc.direction = Math.Sign(forcedVelocity.X);
                        break;
                    case NPCAIStyleID.SkeletronHead:
                        Vector2 directionSkeletron = targetPosition - npc.Center;
                        if (directionSkeletron.Length() > 8f) npc.velocity = Vector2.Normalize(directionSkeletron) * 8f;
                        else npc.velocity *= 0f;
                        if (npc.rotation == Ai[0]) npc.rotation -= 0.3f;
                        Ai[0] = npc.rotation;
                        break;

                    case NPCAIStyleID.GiantTortoise:
                        if (target != null)
                        {
                            if (Collision.CanHit(npc.Center, 1, 1, target.Center, 1, 1))
                            {
                                targetPosition = target.Center;
                            }
                            else
                            {
                                targetPosition = player.Center;
                            }
                        }


                        if (npc.collideY && npc.rotation == Ai[3])
                        {
                            targetLocation = targetPosition.X + (float)Math.Sin(TimeAlive * 0.1f) * 32f;
                            forcedVelocity = Vector2.Zero;
                            length = targetLocation - npc.Center.X;
                            if (Math.Abs(length) > Math.Abs(npc.velocity.X)) length = Math.Abs(npc.velocity.X) * Math.Sign(length);
                            forcedVelocity.X = length;
                            npc.velocity.X = forcedVelocity.X;
                            npc.direction = Math.Sign(forcedVelocity.X);
                            Ai[1] = forcedVelocity.X;
                            Ai[2] = npc.velocity.Y;
                        }
                        else if (Ai[0] <= 2f && npc.rotation != Ai[3])
                        {
                            float minVel = npc.velocity.Length();
                            if (minVel < 10f) minVel = 10f;
                            forcedVelocity = Vector2.Normalize(targetPosition - npc.Center) * minVel;
                            npc.velocity = forcedVelocity;
                            Ai[0] ++;
                            Ai[1] = forcedVelocity.X;
                            Ai[2] = forcedVelocity.Y;
                        }
                        else if (Ai[0] <= 30f || npc.rotation != Ai[3])
                        {
                            npc.velocity = new Vector2(Ai[1], Ai[2]);
                            if (Ai[0] >= 20f) npc.velocity *= 0.75f;
                            if (Ai[0] >= 40f) npc.velocity *= 0.75f;

                            // remove the following for shelly into space bug
                            Ai[0]++;
                        }

                        if (((npc.collideY || npc.collideX) && npc.rotation == Ai[3]) || npc.wet)
                        {
                            Ai[0] = 0f;
                        }

                        Ai[3] = npc.rotation;
                        break;
                    case NPCAIStyleID.Worm:
                        if (npc.realLife == -1 || npc.realLife == npc.whoAmI)
                        {
                            if (Ai[2] == 0)
                            {
                                Ai[2] = 1f;
                            }

                            if (Collision.CanHit(npc.Center, 1, 1, npc.Center + Vector2.Normalize( new Vector2(Ai[1], Ai[2])) * 18f, 1, 1))
                            {
                                Ai[2] += 0.1f;
                                if (Ai[0] > 1)
                                {
                                    Vector2 newTargetPosition = targetPosition;
                                    newTargetPosition.Y += (newTargetPosition.Y - npc.Center.Y) * 0.1f;
                                    Vector2 direction = new Vector2(Ai[1], Ai[2]) * 0.5f + Vector2.Normalize(newTargetPosition - npc.Center) * 8f;
                                    Ai[1] = direction.X;
                                    Ai[2] = direction.Y;
                                    Ai[0] = 0;
                                }
                            }
                            else
                            {
                                Vector2 direction = new Vector2(Ai[1], Ai[2]) + Vector2.Normalize(targetPosition - npc.Center) * 0.075f;
                                if (direction.Length() > 6f) direction = Vector2.Normalize(direction) * 6f;
                                Ai[1] = direction.X;
                                Ai[2] = direction.Y;
                                Ai[0] += 0.01f;
                            }

                            npc.velocity = new Vector2(Ai[1], Ai[2]);
                            npc.direction = Math.Sign(Ai[1]);
                        }
                        break;
                    case NPCAIStyleID.Slime:
                        if ((Ai[0] == 1 && npc.velocity.Y < 0) || npc.wet)
                        {
                            Ai[0] = 0;
                            if (targetPosition.X < npc.Center.X && npc.velocity.X > 0 || targetPosition.X > npc.Center.X && npc.velocity.X < 0)
                            {
                                npc.velocity.X *= -1;
                            }
                            Ai[1] = npc.velocity.X;
                        }

                        if (Ai[0] == 0 || npc.wet) npc.velocity.X = Ai[1];

                        if (npc.collideY || npc.wet) Ai[0] = 1;
                        break;
                    case NPCAIStyleID.Bat:

                        if (target != null)
                        {
                            if (Collision.CanHit(npc.Center, 1, 1, target.Center, 1, 1))
                            {
                                targetPosition = target.Center;
                            }
                            else
                            {
                                targetPosition = player.Center;
                            }
                        }

                        targetLocation = targetPosition.X + (float)Math.Sin(TimeAlive * 0.05f) * 72f;
                        forcedVelocity = Vector2.Zero;
                        length = targetLocation - npc.Center.X;
                        if (Math.Abs(length) > 20f) length = 20f * Math.Sign(length);
                        forcedVelocity.X = length * 0.2f;

                        forcedVelocity.Y = (targetPosition.Y - npc.Center.Y) * 0.05f;
                        if (forcedVelocity.Y > 5f) forcedVelocity.Y = 5f;
                        if (forcedVelocity.Y < -5f) forcedVelocity.Y = -5f;

                        npc.velocity = forcedVelocity;
                        npc.direction = Math.Sign(forcedVelocity.X);
                        break;
                    case NPCAIStyleID.Vulture:

                        if (target != null)
                        {
                            if (Collision.CanHit(npc.Center, 1, 1, target.Center, 1, 1))
                            {
                                targetPosition = target.Center;
                            }
                            else
                            {
                                targetPosition = player.Center;
                            }
                        }

                        targetLocation = targetPosition.X + (float)Math.Sin(TimeAlive * 0.05f) * 72f;
                        forcedVelocity = Vector2.Zero;
                        length = targetLocation - npc.Center.X;
                        if (Math.Abs(length) > 20f) length = 20f * Math.Sign(length);
                        forcedVelocity.X = length * 0.2f;

                        forcedVelocity.Y = (targetPosition.Y - npc.Center.Y) * 0.05f;
                        if (forcedVelocity.Y > 5f) forcedVelocity.Y = 5f;
                        if (forcedVelocity.Y < -5f) forcedVelocity.Y = -5f;

                        npc.velocity = forcedVelocity;
                        npc.direction = Math.Sign(forcedVelocity.X);
                        break;
                    case NPCAIStyleID.Piranha:
                        if (target != null)
                        {
                            if (Collision.CanHit(npc.Center, 1, 1, target.Center, 1, 1))
                            {
                                targetPosition = target.Center;
                            }
                            else
                            {
                                targetPosition = player.Center;
                            }
                        }

                        if (npc.wet)
                        {
                            if (npc.collideX) Ai[1] *= -0.75f;
                            if (npc.collideY) Ai[2] *= -0.75f;

                            Vector2 newVelocityFish = new Vector2(Ai[1], Ai[2]) + Vector2.Normalize(targetPosition - npc.Center) * 0.2f;
                            npc.velocity = newVelocityFish;
                            if (npc.velocity.Length() > 8f) npc.velocity = Vector2.Normalize(npc.velocity) * 8f;
                            npc.direction = Math.Sign(npc.velocity.X);

                            Ai[1] = npc.velocity.X;
                            Ai[2] = npc.velocity.Y;
                        }
                        else Ai[2] = 4f + Main.rand.NextFloat(4f);
                        break;
                    case NPCAIStyleID.DungeonSpirit:

                        if (target != null)
                        {
                            if (Collision.CanHit(npc.Center, 1, 1, target.Center, 1, 1))
                            {
                                targetPosition = target.Center;
                            }
                            else
                            {
                                targetPosition = player.Center;
                            }
                        }

                        targetLocation = targetPosition.X + (float)Math.Sin(TimeAlive * 0.05f) * 72f;
                        forcedVelocity = Vector2.Zero;
                        length = targetLocation - npc.Center.X;
                        if (Math.Abs(length) > 20f) length = 20f * Math.Sign(length);
                        forcedVelocity.X = length * 0.2f;

                        forcedVelocity.Y = (targetPosition.Y - npc.Center.Y) * 0.05f;
                        if (forcedVelocity.Y > 5f) forcedVelocity.Y = 5f;
                        if (forcedVelocity.Y < -5f) forcedVelocity.Y = -5f;

                        npc.velocity = forcedVelocity;
                        npc.direction = Math.Sign(forcedVelocity.X);
                        break;
                    case NPCAIStyleID.FlyingFish:

                        if (target != null)
                        {
                            if (Collision.CanHit(npc.Center, 1, 1, target.Center, 1, 1) || npc.noTileCollide)
                            {
                                targetPosition = target.Center;
                            }
                            else
                            {
                                targetPosition = player.Center;
                            }
                        }

                        targetLocation = targetPosition.X + (float)Math.Sin(TimeAlive * 0.05f) * 72f;
                        forcedVelocity = Vector2.Zero;
                        length = targetLocation - npc.Center.X;
                        if (Math.Abs(length) > 20f) length = 20f * Math.Sign(length);
                        forcedVelocity.X = length * 0.2f;

                        forcedVelocity.Y = (targetPosition.Y - npc.Center.Y) * 0.05f;
                        if (forcedVelocity.Y > 5f) forcedVelocity.Y = 5f;
                        if (forcedVelocity.Y < -5f) forcedVelocity.Y = -5f;

                        npc.velocity = forcedVelocity;
                        npc.direction = Math.Sign(forcedVelocity.X);
                        break;
                    case NPCAIStyleID.HoveringFighter:
                        if (target != null)
                        {
                            if (Collision.CanHit(npc.Center, 1, 1, target.Center, 1, 1) || npc.noTileCollide)
                            {
                                targetPosition = target.Center;
                            }
                            else
                            {
                                targetPosition = player.Center;
                            }
                        }

                        targetPosition.X += (float)Math.Sin(TimeAlive * 0.025f) * 32f;
                        forcedVelocity = Vector2.Zero;
                        length = targetPosition.X - npc.Center.X;
                        if (Math.Abs(length) > 20f) length = 20f * Math.Sign(length);
                        forcedVelocity.X = length * 0.1f;

                        targetPosition.Y += (float)Math.Cos(TimeAlive * 0.015f) * 16f;
                        forcedVelocity.Y = (targetPosition.Y - npc.Center.Y) * 0.02f;
                        if (forcedVelocity.Y > 3f) forcedVelocity.Y = 3f;
                        if (forcedVelocity.Y < -3f) forcedVelocity.Y = -3f;

                        npc.velocity = forcedVelocity;
                        npc.direction = Math.Sign(forcedVelocity.X);
                        break;
                    case NPCAIStyleID.Flying:
                        Ai[0]++;
                        if (Ai[0] > 450) Ai[0] = 0;

                        if (target != null)
                        {
                            if (Collision.CanHit(npc.Center, 1, 1, target.Center, 1, 1) || npc.noTileCollide)
                            {
                                targetPosition = target.Center;
                            }
                            else
                            {
                                targetPosition = player.Center;
                                Ai[0] = 200;
                            }
                        }
                        else
                        {
                            Ai[0] = 200;
                        }

                        if (npc.collideX) Ai[1] *= -0.75f;
                        if (npc.collideY) Ai[2] *= -0.75f;

                        Vector2 newVelocity = new Vector2(Ai[1], Ai[2]) + Vector2.Normalize(targetPosition - npc.Center) * (Ai[0] < 300 ? 0.1f : 0.005f);
                        npc.velocity = newVelocity;
                        if (npc.velocity.Length() > (Ai[0] < 300 ? 6f : 1f)) npc.velocity = Vector2.Normalize(npc.velocity) * (Ai[0] < 300 ? 6f : 1f);
                        npc.direction = Math.Sign(npc.velocity.X);
                        npc.rotation = npc.velocity.ToRotation() - MathHelper.PiOver2;
                        Ai[1] = npc.velocity.X;
                        Ai[2] = npc.velocity.Y;
                        break;
                    case NPCAIStyleID.Spider:
                        if (target != null)
                        {
                            if (Collision.CanHit(npc.Center, 1, 1, target.Center, 1, 1) || npc.noTileCollide)
                            {
                                targetPosition = target.Center;
                            }
                            else
                            {
                                targetPosition = player.Center;
                            }
                        }

                        if (npc.collideX) Ai[1] *= -0.75f;
                        if (npc.collideY) Ai[2] *= -0.75f;

                        newVelocity = new Vector2(Ai[1], Ai[2]) + Vector2.Normalize(targetPosition - npc.Center) * 0.15f;
                        npc.velocity = newVelocity;
                        if (npc.velocity.Length() > 4f) npc.velocity = Vector2.Normalize(npc.velocity) * 4f;
                        npc.direction = Math.Sign(npc.velocity.X);
                        npc.rotation = npc.velocity.ToRotation();
                        Ai[1] = npc.velocity.X;
                        Ai[2] = npc.velocity.Y;
                        break;
                    case NPCAIStyleID.DemonEye:

                        if (target != null)
                        {
                            if (Collision.CanHit(npc.Center, 1, 1, target.Center, 1, 1))
                            {
                                targetPosition = target.Center;
                            }
                            else
                            {
                                targetPosition = player.Center;
                            }
                        }

                        Ai[0]++;
                        if (Ai[0] > 90) Ai[0] = 0;
                        if (Ai[0] < 60)
                        {
                            Vector2 direction = new Vector2(Ai[1], Ai[2]) + Vector2.Normalize(targetPosition - npc.Center) * 0.2f;
                            // Funny bug line
                            //Vector2 direction = Vector2.Normalize(targetPosition - npc.Center) * 0.2f;
                            if (direction.Length() > 6f) direction = Vector2.Normalize(direction) * 6f;
                            Ai[1] = direction.X;
                            Ai[2] = direction.Y;
                        }
                        else
                        {
                            Ai[1] *= 0.95f;
                            Ai[2] *= 0.95f;
                        }

                        npc.velocity = new Vector2(Ai[1], Ai[2]);
                        npc.direction = Math.Sign(Ai[1]);
                        break;
                    case NPCAIStyleID.Spell:
                        if (Ai[0] == 0)
                        {
                            if (target != null)
                            {
                                Vector2 velocity = Vector2.Normalize(target.Center - npc.Center) * npc.velocity.Length();
                                Ai[1] = velocity.X;
                                Ai[2] = velocity.Y;
                            }
                            else
                            {
                                npc.active = false;
                            }
                            Ai[0] = 1;
                            npc.netUpdate = true;
                        }

                        npc.velocity = new Vector2(Ai[1], Ai[2]);
                        break;
                    case NPCAIStyleID.Caster:
                        // Nothing to do!
                        break;
                    default:
                        break;
                }
            }
        }

        public void Attack(NPC npc)
        {
            foreach (NPC npcTarget in Main.npc)
            {
                if (npcTarget.active && !npcTarget.friendly && npcTarget.life > 0 && !npcTarget.dontTakeDamage && npcTarget.whoAmI != npc.whoAmI)
                {
                    if (npcTarget.Hitbox.Intersects(npc.Hitbox))
                    {
                        VervPalModPlayer modPlayer = Main.LocalPlayer.GetModPlayer<VervPalModPlayer>();
                        bool canCrit = Main.rand.Next(100) < Main.LocalPlayer.GetTotalCritChance(DamageClass.Summon) && !modPlayer.NoKillMode;
                        float damagemult = Main.masterMode ? 1.5f : Main.expertMode ? 1.75f : 2f;
                        int damage = (int)Main.LocalPlayer.GetTotalDamage<SummonDamageClass>().ApplyTo(npc.damage * damagemult);
                        if (modPlayer.MinionBuffer < 60) damage = (int)(damage * 0.01f);
                        NPC newTarget = npcTarget;

                        if (newTarget.realLife != -1 && newTarget.realLife != newTarget.whoAmI) newTarget = Main.npc[npcTarget.realLife];
                        if (newTarget.life - damage < 0 && modPlayer.NoKillMode && CanBeTamed(newTarget))
                        {
                            damage = (int)(newTarget.life - 1 + newTarget.defense * 0.5f);
                            if (newTarget.life < newTarget.lifeMax * 0.15f) damage = 0;
                        }  

                        if (damage > 5)
                        {
                            Main.LocalPlayer.ApplyDamageToNPC(npcTarget, damage, 5f, npc.direction, canCrit);
                            AttackCooldown = 0;
                        }
                    }
                }
            }
        }
    }
}