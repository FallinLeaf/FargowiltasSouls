using System;
using System.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace FargowiltasSouls.Projectiles.Challengers
{

    public class BaronMine : ModProjectile
    {
        public bool home = true;
        public bool BeenOutside = false;
        public override string Texture => "Terraria/Images/Projectile_135";
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Banished Baron's Rocket");
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 3;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.scale = 1f;
            Projectile.light = 1;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.velocity.X != oldVelocity.X && Math.Abs(oldVelocity.X) > 1f)
            {
                Projectile.velocity.X = oldVelocity.X * 0f;
            }
            if (Projectile.velocity.Y != oldVelocity.Y && Math.Abs(oldVelocity.Y) > 1f)
            {
                Projectile.velocity.Y = oldVelocity.Y * 0f;
            }
            return false;
        }
        public override void AI()
        {
            if (Projectile.localAI[0] == 0)
            {
                Projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            }
            Projectile.rotation += MathHelper.ToRadians(Projectile.velocity.Length());

            if (++Projectile.localAI[0] > 120f)
            {
                Projectile.Kill();
            }
            if (!Projectile.tileCollide)
            {
                Tile tile = Framing.GetTileSafely(Projectile.Center);
                if (!(tile.HasUnactuatedTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType]))
                    Projectile.tileCollide = true;
            }
            if (Projectile.ai[0] == 1) //floating
            {
                if (Collision.WetCollision(Projectile.position, Projectile.width, Projectile.height))
                    Projectile.velocity.Y -= 0.45f;
                else
                {
                    if (Projectile.velocity.Y < 0)
                        Projectile.velocity.Y /= 3;
                    Projectile.velocity.Y += 1f;
                }
                Projectile.velocity.X *= 0.97f;
            }
            if (Projectile.ai[0] == 2) //slowing down, flying
            {
                Projectile.velocity *= 0.97f;
            }
            //}
        }
        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 5; i++)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 0, default(Color), 1.5f);
                Main.dust[d].noGravity = true;
            }
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
            float speedmod = Projectile.ai[0] == 1 ? 1 : 1.5f;
            for (int i = 0; i < 8; i++)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 pos = new Vector2(0, 6*speedmod).RotatedBy(Projectile.rotation + (i * MathHelper.TwoPi / 8));
                    Vector2 vel = pos;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + pos, vel, ModContent.ProjectileType<BaronScrap>(), Projectile.damage, Projectile.knockBack, Main.myPlayer, 0, 0);
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture2D13 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            int num156 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type]; //ypos of lower right corner of sprite to draw
            int y3 = num156 * Projectile.frame; //ypos of upper left corner of sprite to draw
            Rectangle rectangle = new Rectangle(0, y3, texture2D13.Width, num156);
            Vector2 origin2 = rectangle.Size() / 2f;

            Color color26 = lightColor;
            color26 = Projectile.GetAlpha(color26);

            SpriteEffects effects = Projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Projectile.type]; i++)
            {
                Color color27 = color26 * 0.75f;
                color27 *= (float)(ProjectileID.Sets.TrailCacheLength[Projectile.type] - i) / ProjectileID.Sets.TrailCacheLength[Projectile.type];
                Vector2 value4 = Projectile.oldPos[i];
                float num165 = Projectile.oldRot[i];
                Main.EntitySpriteDraw(texture2D13, value4 + Projectile.Size / 2f - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), color27, num165, origin2, Projectile.scale, effects, 0);
            }

            Main.EntitySpriteDraw(texture2D13, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), Projectile.GetAlpha(lightColor), Projectile.rotation, origin2, Projectile.scale, effects, 0);


            return false;
        }
    }
}

