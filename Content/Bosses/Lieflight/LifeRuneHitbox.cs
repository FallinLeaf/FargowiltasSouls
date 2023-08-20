using System;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.Lieflight
{

    public class LifeRuneHitbox : ModProjectile
    {
        public override string Texture => "FargowiltasSouls/Assets/ExtraTextures/LifeChallengerParts/Rune1";

        private NPC lifelight;
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Life Rune");
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 2;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.aiStyle = 0;
            Projectile.hostile = false;
            AIType = 14;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.scale = 1.5f;
            Projectile.timeLeft = 600;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) //line collision, needed because of the speed they move at when creating the arena, to form a solid wall
        {
            float collisionPoint = 0f;
            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.oldPos[1], Projectile.width, ref collisionPoint))
            {
                return true;
            }
            return false;
        }

        public override void AI()
        {
            if (Main.npc[(int)Projectile.ai[0]] == null)
            {
                Main.NewText("he's dead");
                Projectile.Kill();
            }
            lifelight = Main.npc[(int)Projectile.ai[0]]; //get npc
            float RuneDistance = lifelight.localAI[0];
            float BodyRotation = lifelight.localAI[1];
            int RuneCount = (int)lifelight.localAI[2];
            int i = (int)Projectile.ai[1];

            float runeRot = (float)(BodyRotation + Math.PI * 2 / RuneCount * i);
            Vector2 runePos = lifelight.Center + runeRot.ToRotationVector2() * RuneDistance;
            Projectile.Center = runePos;

            if (i % 3 == 0) //cyan
            {
                Dust.NewDust(Projectile.Center, 0, 0, DustID.UltraBrightTorch);
            }
            else if (i % 3 == 1) //yellow
            {
                Dust.NewDust(Projectile.Center, 0, 0, DustID.YellowTorch);
            }
            else //pink
            {
                Dust.NewDust(Projectile.Center, 0, 0, DustID.PinkTorch);
            }

            //fix for weird bug (may not fix it)
            if (Projectile.timeLeft < 595)
            {
                Projectile.hostile = true;
            }
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (WorldSavingSystem.EternityMode)
                target.AddBuff(ModContent.BuffType<Buffs.Masomode.SmiteBuff>(), 600);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
    }
}
