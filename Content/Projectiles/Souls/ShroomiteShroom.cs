using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Souls
{
    public class ShroomiteShroom : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_131";

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Shroom");
        }

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.Mushroom);
            AIType = ProjectileID.Mushroom;
            Projectile.GetGlobalProjectile<FargoSoulsGlobalProjectile>().CanSplit = false;

            Projectile.DamageType = DamageClass.Ranged;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 20;
            Projectile.GetGlobalProjectile<FargoSoulsGlobalProjectile>().noInteractionWithNPCImmunityFrames = true;
        }

        public override void AI()
        {
            //dies thrice as fast
            Projectile.alpha += 8;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255 - Projectile.alpha, 255 - Projectile.alpha, 255 - Projectile.alpha, 0);
        }
    }
}