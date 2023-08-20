using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Souls
{
    public class FossilBone : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_21";

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Fossil Bone");
        }

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.Bone);
            Projectile.aiStyle = -1;
            Projectile.timeLeft = 900;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            Projectile.rotation += 0.2f;

            Projectile.velocity *= .95f;

            if (Projectile.velocity.Length() < 0.1)
            {
                Projectile.velocity = Vector2.Zero;
            }

            if (Projectile.velocity == Vector2.Zero && player.Hitbox.Intersects(Projectile.Hitbox))
            {
                player.GetModPlayer<FargoSoulsPlayer>().HealPlayer(20);
                Projectile.Kill();
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.position += Projectile.velocity;
            Projectile.velocity = Vector2.Zero;
            return false;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.SandyBrown;
        }

        public override void Kill(int timeLeft)
        {
            const int max = 16;
            for (int i = 0; i < max; i++)
            {
                Vector2 vector6 = Vector2.UnitY * 5f;
                vector6 = vector6.RotatedBy((i - (max / 2 - 1)) * 6.28318548f / max) + Projectile.Center;
                Vector2 vector7 = vector6 - Projectile.Center;
                int d = Dust.NewDust(vector6 + vector7, 0, 0, DustID.Dirt, 0f, 0f, 0, default, 1.5f);
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity = vector7;
            }
        }
    }
}