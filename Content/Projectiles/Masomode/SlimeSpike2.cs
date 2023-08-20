using Terraria;

namespace FargowiltasSouls.Content.Projectiles.Masomode
{
    public class SlimeSpike2 : SlimeSpike
    {
        public override string Texture => "Terraria/Images/Projectile_605";

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.scale = 1.5f;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            base.AI();
            if (!Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height))
                Lighting.AddLight(Projectile.Center, 0f, 0f, 0.8f);
        }
    }
}