using FargowiltasSouls.Content.Buffs.Masomode;
using FargowiltasSouls.Core.NPCMatching;
using System;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.LunarEvents.Vortex
{
    public class LunarTowerVortex : LunarTowers
    {
        public override int ShieldStrength
        {
            get => NPC.ShieldStrengthTowerVortex;
            set => NPC.ShieldStrengthTowerVortex = value;
        }

        public override NPCMatcher CreateMatcher() =>
            new NPCMatcher().MatchType(NPCID.LunarTowerVortex);

        public LunarTowerVortex() : base(ModContent.BuffType<JammedBuff>(), DustID.Vortex) { }
        public override void SetDefaults(NPC npc)
        {
            base.SetDefaults(npc);

            npc.lifeMax = (int)Math.Round(npc.lifeMax * 5f);
            npc.damage = (int)Math.Round(npc.damage * 0.9f);
        }
        public enum Attacks
        {
            Idle,
            VortexVortex,
            LightningBall,
            SkyLightning,
            VortexShield

        }
        public override List<int> RandomAttacks => new List<int>() //these are randomly chosen attacks in p1
        {
            (int)Attacks.LightningBall,
            (int)Attacks.SkyLightning,
            (int)Attacks.VortexShield
        };
        public override void ShieldsDownAI(NPC npc)
        {
            Player target = Main.player[npc.target];
            if (npc.HasPlayerTarget && target.active)
            {
                switch (Attack)
                {
                    case (int)Attacks.VortexVortex:
                        VortexVortex(npc, target);
                        break;
                    case (int)Attacks.LightningBall:
                        LightningBall(npc, target);
                        break;
                    case (int)Attacks.SkyLightning:
                        SkyLightning(npc, target);
                        break;
                    case (int)Attacks.VortexShield:
                        VortexShield(npc, target);
                        break;
                    case (int)Attacks.Idle:
                        Idle(npc, target);
                        break;
                }
            }
        }
        #region Attacks
        private int Vortex = -1;
        private void VortexVortex(NPC npc, Player player)
        {
            //no end
            void Attack()
            {
                    

                if (Main.netMode != NetmodeID.MultiplayerClient && AttackTimer % 20 == 1) //anti lag
                {
                    bool vortexAlive = false;
                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        if (Main.projectile[i].type == ModContent.ProjectileType<VortexVortex>() && Main.projectile[i].whoAmI == Vortex)
                        {
                            vortexAlive = true;
                            break;
                        }
                    }
                    if (!vortexAlive)
                    {
                        Vector2 pos = npc.Center - Vector2.UnitY * npc.height * 0.8f;
                        Vortex = Projectile.NewProjectile(npc.GetSource_FromThis(), pos, Vector2.Zero, ModContent.ProjectileType<VortexVortex>(), FargoSoulsUtil.ScaledProjectileDamage(npc.damage), 0f, Main.myPlayer, 0f, 1);
                    }
                        
                }
                if (Vortex > -1 && Vortex < Main.maxNPCs)
                {
                    if (Main.projectile[Vortex].active)
                    {
                        Main.projectile[Vortex].velocity = (npc.Center - Vector2.UnitY * npc.height * 0.8f) - Main.projectile[Vortex].Center;
                    }
                }
            }
            Attack();
        }
        private void LightningBall(NPC npc, Player player)
        {
            const int WindupDuration = 60 * 1;
            const int AttackDuration = 60 * 4;
            const int EndlagDuration = 60 * 2;
            void Windup()
            {

            }
            void Attack()
            {
                const int AttackDelay = 180;
                if ((AttackTimer - WindupDuration) % AttackDelay == 1)
                {
                    bool second = false;
                    if ((AttackTimer - WindupDuration) % (AttackDelay * 2) == AttackDelay + 1) //every other attack
                    {
                        second = true;
                    }
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 pos = npc.Center - Vector2.UnitY * npc.height * 0.8f;
                        Vector2 vel = second ? pos.DirectionTo(player.Center) * 6 : Vector2.Zero;
                        Projectile.NewProjectile(npc.GetSource_FromThis(), pos, vel.RotatedBy(Math.PI / 2), ProjectileID.CultistBossLightningOrb, FargoSoulsUtil.ScaledProjectileDamage(npc.damage), 3f, Main.myPlayer);
                        if (second)
                        {
                            Projectile.NewProjectile(npc.GetSource_FromThis(), pos, vel.RotatedBy(-Math.PI / 2), ProjectileID.CultistBossLightningOrb, FargoSoulsUtil.ScaledProjectileDamage(npc.damage), 3f, Main.myPlayer);
                        }
                    }
                }
            }
            void Endlag()
            {

            }

            if (AttackTimer <= WindupDuration)
            {
                Windup();
            }
            else if (AttackTimer <= WindupDuration + AttackDuration)
            {
                Attack();
            }
            else
            {
                Endlag();
            }
            if (AttackTimer > WindupDuration + AttackDuration + EndlagDuration)
            {
                EndAttack(npc);
            }
        }
        private void SkyLightning(NPC npc, Player player)
        {
            const int WindupDuration = 60 * 0;
            const int AttackDuration = 60 * 4;
            const int EndlagDuration = (int)(60 * 1.5f);
            void Windup()
            {

            }
            void Attack()
            {
                const int distance = 300;
                const int AttackDelay = 50;
                if ((AttackTimer - WindupDuration) % AttackDelay == 1)
                {
                    bool second = false;
                    if ((AttackTimer - WindupDuration) % (AttackDelay * 2) == AttackDelay + 1) //every other attack
                    {
                        second = true;
                    }
                    for (int i = 0; i < 16; i++)
                    {
                        int x = i;
                        if (x >= 8)
                        {
                            x = 7 - x; //split i into 1 to 8 and -1 to -8
                        }
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int offset = second ? distance / 2 : 0;
                            Vector2 pos = npc.Center + Vector2.UnitX * (distance * x + offset);
                            Vector2 vel = Vector2.UnitY * 16;
                            Projectile.NewProjectile(npc.GetSource_FromThis(), pos, vel, ModContent.ProjectileType<PillarSpawner>(), FargoSoulsUtil.ScaledProjectileDamage(npc.damage), 3f, Main.myPlayer, ai0: 2, ai2: npc.whoAmI);
                        }

                    }
                }
            }
            void Endlag()
            {

            }

            if (AttackTimer <= WindupDuration)
            {
                Windup();
            }
            else if (AttackTimer <= WindupDuration + AttackDuration)
            {
                Attack();
            }
            else
            {
                Endlag();
            }
            if (AttackTimer > WindupDuration + AttackDuration + EndlagDuration)
            {
                EndAttack(npc);
            }
        }
        private void VortexShield(NPC npc, Player player)
        {
            const int AttackDuration = 60 * 2;
            if (AttackTimer == 1) //triggers "shield going down" animation
            {
                npc.ai[3] = 1f;
                npc.netUpdate = true;
                NetSync(npc);
            }
            const int ReactionTime = 15;
            npc.reflectsProjectiles = AttackTimer >= ReactionTime;
            if (npc.reflectsProjectiles) //dust
            {
                for (int i = 0; i < 20; i++)
                {
                    Vector2 offset = new();
                    double angle = Main.rand.NextDouble() * 2d * Math.PI;
                    offset.X += (float)(Math.Sin(angle) * npc.height / 2);
                    offset.Y += (float)(Math.Cos(angle) * npc.height / 2);
                    Dust dust = Main.dust[Dust.NewDust(
                        npc.Center + offset - new Vector2(4, 4), 0, 0,
                        DustID.Vortex, 0, 0, 100, Color.White, 1f
                        )];
                    dust.noGravity = true;
                }
            }

            if (AttackTimer > AttackDuration)
            {
                npc.reflectsProjectiles = false;
                EndAttack(npc);
            }
        }
        const int IdleTime = 90;
        private void Idle(NPC npc, Player player)
        {
            if (AttackTimer > IdleTime)
            {
                RandomAttack(npc);
            }
        }
        #endregion
    }
}
