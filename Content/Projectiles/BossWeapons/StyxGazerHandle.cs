﻿

using FargowiltasSouls.Content.Bosses.AbomBoss;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.BossWeapons
{
    public class StyxGazerHandle : Deathrays.BaseDeathray
    {
        public override string Texture => "FargowiltasSouls/Content/Projectiles/Deathrays/AbomDeathray";
        public StyxGazerHandle() : base(120) { }
        public int counter;

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            // DisplayName.SetDefault("Styx Gazer");
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.extraUpdates = 1;
            Projectile.FargoSouls().CanSplit = false;
            Projectile.FargoSouls().TimeFreezeImmune = true;
            Projectile.FargoSouls().DeletionImmuneRank = 2;
        }

        public override void AI()
        {
            Projectile.maxPenetrate = 1;

            Vector2? vector78 = null;
            if (Projectile.velocity.HasNaNs() || Projectile.velocity == Vector2.Zero)
            {
                Projectile.velocity = -Vector2.UnitY;
            }
            int byIdentity = FargoSoulsUtil.GetProjectileByIdentity(Projectile.owner, (int)Projectile.ai[1], ModContent.ProjectileType<StyxGazer>(), ModContent.ProjectileType<StyxGazerArmor>());
            if (byIdentity != -1)
            {
                Projectile.Center = Main.projectile[byIdentity].Center;
                Projectile.position += Main.projectile[byIdentity].velocity * 75;
                Projectile.velocity = Main.projectile[byIdentity].velocity.RotatedBy(Projectile.ai[0]);
            }
            else if (Projectile.owner == Main.myPlayer && Projectile.localAI[0] > 5)
            {
                Projectile.Kill();
                return;
            }
            if (Projectile.velocity.HasNaNs() || Projectile.velocity == Vector2.Zero)
            {
                Projectile.velocity = -Vector2.UnitY;
            }
            //if (Projectile.localAI[0] == 0f)
            //{
            //   SoundEngine.PlaySound(SoundID.Zombie104 with { Volume = 0.6f, Pitch = 0 }, Projectile.Center);
            //}
            float num801 = 1f;
            Projectile.localAI[0] += 1f;
            if (Projectile.localAI[0] >= maxTime)
            {
                Projectile.Kill();
                return;
            }
            Projectile.scale = (float)Math.Sin(Projectile.localAI[0] * 3.14159274f / maxTime) * num801 * 6f;
            if (Projectile.scale > num801)
            {
                Projectile.scale = num801;
            }
            float num804 = Projectile.velocity.ToRotation();
            /*if (Main.npc[ai1].velocity != Vector2.Zero)
                num804 += Projectile.ai[0];*/
            Projectile.rotation = num804 - 1.57079637f;
            Projectile.velocity = num804.ToRotationVector2();
            float num805 = 3f;
            float num806 = Projectile.width;
            Vector2 samplingPoint = Projectile.Center;
            if (vector78.HasValue)
            {
                samplingPoint = vector78.Value;
            }
            float[] array3 = new float[(int)num805];
            //Collision.LaserScan(samplingPoint, Projectile.velocity, num806 * Projectile.scale, 3000f, array3);
            for (int i = 0; i < array3.Length; i++)
                array3[i] = 100f;
            float num807 = 0f;
            int num3;
            for (int num808 = 0; num808 < array3.Length; num808 = num3 + 1)
            {
                num807 += array3[num808];
                num3 = num808;
            }
            num807 /= num805;
            float amount = 0.5f;
            Projectile.localAI[1] = MathHelper.Lerp(Projectile.localAI[1], num807, amount);

            if (Main.rand.NextBool())
            {
                int d = Dust.NewDust(Projectile.position + Projectile.velocity * Main.rand.NextFloat(100), Projectile.width, Projectile.height, DustID.GemTopaz, 0f, 0f, 0, default, 1.5f);
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity *= 4f;
            }
        }

        public override bool? CanDamage()
        {
            Projectile.maxPenetrate = 1;
            return true;
        }

        public override bool? CanHitNPC(NPC target)
        {
            if (Projectile.localNPCImmunity[target.whoAmI] >= 15)
                return false;
            return null;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.localNPCImmunity[target.whoAmI]++;

            if (Projectile.owner == Main.myPlayer)
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center + Main.rand.NextVector2Circular(100, 100), Vector2.Zero, ModContent.ProjectileType<AbomBlast>(), 0, 0f, Projectile.owner);

            target.AddBuff(BuffID.ShadowFlame, 300);
            target.AddBuff(ModContent.BuffType<Buffs.Masomode.MutantNibbleBuff>(), 300);
            //target.immune[Projectile.owner] = Main.player[Projectile.owner].ownedProjectileCounts[ModContent.ProjectileType<BossWeapons.StyxGazer>()] > 0 ? 1 : 3;
        }

        public float WidthFunction(float _) => Projectile.width * Projectile.scale * 2;

        public static Color ColorFunction(float _) => new(253, 254, 32, 100);

        public override bool PreDraw(ref Color lightColor)
        {
            // This should never happen, but just in case.
            if (Projectile.velocity == Vector2.Zero)
                return false;

            ManagedShader shader = ShaderManager.GetShader("FargowiltasSouls.WillBigDeathray");

            // Get the laser end position.
            Vector2 laserEnd = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitY) * Projectile.localAI[1];

            // Create 8 points that span across the draw distance from the projectile center.

            // This allows the drawing to be pushed back, which is needed due to the shader fading in at the start to avoid
            // sharp lines.
            Vector2 initialDrawPoint = Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.UnitY) * Projectile.localAI[1];
            Vector2[] baseDrawPoints = new Vector2[8];
            for (int i = 0; i < baseDrawPoints.Length; i++)
                baseDrawPoints[i] = Vector2.Lerp(initialDrawPoint, laserEnd, i / (float)(baseDrawPoints.Length - 1f));

            // Set shader parameters. This one takes a fademap and a color.

            // The laser should fade to this in the middle.
            Color brightColor = Color.Black;
            shader.TrySetParameter("mainColor", brightColor);
            // GameShaders.Misc["FargoswiltasSouls:MutantDeathray"].UseImage1(); cannot be used due to only accepting vanilla paths.
            Texture2D fademap = ModContent.Request<Texture2D>("FargowiltasSouls/Assets/ExtraTextures/Trails/WillStreak").Value;
            FargoSoulsUtil.SetTexture1(fademap);
            for (int j = 0; j < 2; j++)
            {
                PrimitiveSettings primSettings = new(WidthFunction, ColorFunction, Shader: shader);
                PrimitiveRenderer.RenderTrail(baseDrawPoints, primSettings, 30);

                for (int i = 0; i < baseDrawPoints.Length / 2; i++)
                {
                    Vector2 temp = baseDrawPoints[i];
                    int swap = baseDrawPoints.Length - 1 - i;
                    baseDrawPoints[i] = baseDrawPoints[swap];
                    baseDrawPoints[swap] = temp;
                }
                PrimitiveRenderer.RenderTrail(baseDrawPoints, primSettings, 30);
            }
            return false;
        }
    }
}