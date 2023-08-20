//using System;
//using Microsoft.Xna.Framework;
//using Terraria;
//using Terraria.DataStructures;
//using Terraria.ID;
//using Terraria.Localization;
//using Terraria.ModLoader;

//namespace FargowiltasSouls.Content.Items.Weapons.Misc
//{
//    public class VoidBow : SoulsItem
//    {
//        public override void SetStaticDefaults()
//        {
//            DisplayName.SetDefault("Void Bow");
//            Tooltip.SetDefault(
//                "Converts all arrows to void arrows \n40% chance to not consume ammo\n'A glimpse to the other side'");
//            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "虚空弓");
//            Tooltip.AddTranslation((int)GameCulture.CultureName.Chinese, "'另一侧的一瞥' \n所有箭支转化为虚空箭 \n40%概率不消耗弹药");
//        }

//        public override void SetDefaults()
//        {
//            Item.damage = 175;
//            Item.DamageType = DamageClass.Ranged;
//            Item.width = 40;
//            Item.height = 20;
//            Item.useTime = 6;
//            Item.useAnimation = 20;
//            Item.useStyle = ItemUseStyleID.Shoot;
//            Item.noMelee = true;
//            Item.knockBack = 3;
//            Item.value = 1000;
//            Item.rare = ItemRarityID.Purple;
//            Item.UseSound = SoundID.Item5;
//            Item.autoReuse = true;
//            Item.shoot = ModContent.ProjectileType<VoidArrow>();
//            Item.shootSpeed = 12f;
//            Item.useAmmo = AmmoID.Arrow;
//            Item.crit = 25;
//        }

//        public override bool CanConsumeAmmo(Player p)
//        {
//            return Main.rand.Next(5) <= 2;
//        }

//        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
//        {
//            float spread = 45f * 0.0174f;
//            double startAngle = Math.Atan2(speedX, speedY) - spread / 2;
//            double deltaAngle = spread / 8f;
//            double offsetAngle;
//            int i;
//            for (i = 0; i < 1; i++)
//            {
//                offsetAngle = startAngle + deltaAngle * (i + i * i) / 2f + 32f * i;
//                Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<VoidArrow>(),
//                    damage, knockback, player.whoAmI);
//            }

//            return false;
//        }

//        public override Vector2? HoldoutOffset()
//        {
//            return Vector2.Zero;
//        }
//    }
//}