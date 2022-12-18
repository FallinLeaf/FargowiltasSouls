﻿using FargowiltasSouls.EternityMode.Net;
using FargowiltasSouls.EternityMode.Net.Strategies;
using FargowiltasSouls.EternityMode.NPCMatching;
using FargowiltasSouls.Projectiles.Souls;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.EternityMode.Content.Enemy.Cavern
{
    public class RockGolem : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.RockGolem);

        public int JumpTimer;
        public bool Jumped;

        public override Dictionary<Ref<object>, CompoundStrategy> GetNetInfo() =>
            new Dictionary<Ref<object>, CompoundStrategy> {
                { new Ref<object>(JumpTimer), IntStrategies.CompoundStrategy },
            };

        public override void SetDefaults(NPC npc)
        {
            base.SetDefaults(npc);

            JumpTimer = 300 + Main.rand.Next(60);
        }

        public override bool SafePreAI(NPC npc)
        {
            bool result = base.SafePreAI(npc);

            const float gravity = 0.4f;

            if (JumpTimer > 360) //initiate jump
            {
                JumpTimer = 0;
                Jumped = true;

                int t = npc.HasPlayerTarget ? npc.target : npc.FindClosestPlayer();
                if (t != -1 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    const float time = 90;
                    Vector2 distance;
                    if (Main.player[t].active && !Main.player[t].dead && !Main.player[t].ghost)
                        distance = Main.player[t].Center - npc.Bottom;
                    else
                        distance = new Vector2(npc.Center.X < Main.player[t].Center.X ? -300 : 300, -100);
                    distance.X = distance.X / time;
                    distance.Y = distance.Y / time - 0.5f * gravity * time;
                    npc.ai[1] = time;
                    npc.ai[2] = distance.X;
                    npc.ai[3] = distance.Y;
                    npc.netUpdate = true;
                }

                return false;
            }

            if (JumpTimer == 330)
            {
                JumpTimer++; //avoid edge case

                if (Main.netMode != NetmodeID.MultiplayerClient)
                    Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, ModContent.ProjectileType<IronParry>(), 0, 0f, Main.myPlayer);
            }

            if (npc.ai[1] > 0f) //while jumping
            {
                npc.ai[1]--;
                npc.noTileCollide = true;
                npc.velocity.X = npc.ai[2];
                npc.velocity.Y = npc.ai[3];
                npc.ai[3] += gravity;

                int num22 = 2;
                for (int index1 = 0; index1 < num22; ++index1)
                {
                    Vector2 vector2_2 = ((float)(Main.rand.NextDouble() * 3.14159274101257) - 1.570796f).ToRotationVector2() * Main.rand.Next(3, 8);
                    int index2 = Dust.NewDust(npc.position, npc.width, npc.height, DustID.Stone, vector2_2.X * 2f, vector2_2.Y * 2f, 100, new Color(), 1.4f);
                    Main.dust[index2].noGravity = true;
                    Main.dust[index2].noLight = true;
                    Main.dust[index2].velocity /= 4f;
                    Main.dust[index2].velocity -= npc.velocity;
                }

                JumpTimer = 0;
                JumpTimer++;
                return false;
            }
            else
            {
                if (npc.noTileCollide)
                {
                    JumpTimer = 0;
                    npc.noTileCollide = Collision.SolidCollision(npc.position, npc.width, npc.height);
                    return false;
                }
            }

            if (npc.HasValidTarget && (Collision.CanHitLine(npc.Center, 0, 0, Main.player[npc.target].Center, 0, 0)
                || npc.life < npc.lifeMax / 2))
            {
                JumpTimer++;
            }

            if (npc.velocity.Y == 0f && Jumped)
            {
                Jumped = false;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, ProjectileID.DD2OgreStomp, FargoSoulsUtil.ScaledProjectileDamage(npc.damage), 0f, Main.myPlayer);
            }

            return result;
        }
    }
}
