﻿using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using FargowiltasSouls.Core.Toggler.Content;
using FargowiltasSouls.Content.Buffs.Souls;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Items.Accessories.Enchantments
{
	public class TurtleEnchant : BaseEnchant
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();

            // DisplayName.SetDefault("Turtle Enchantment");
            /* Tooltip.SetDefault(
@"100% of contact damage is reflected
When standing still and not attacking, you will enter your shell
While in your shell, you will gain 90% damage resistance 
Additionally you will destroy incoming projectiles and deal 10x more thorns damage
The shell lasts at least 1 second and up to 20 attacks blocked
Enemies will explode into needles on death if they are struck with your needles
'You suddenly have the urge to hide in a shell'"); */
            //             DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "乌龟魔石");
            //             Tooltip.AddTranslation((int)GameCulture.CultureName.Chinese,
            // @"反弹100%接触伤害
            // 站定不动时且不攻击时你会缩进壳里
            // 当你缩进壳里时增加90%伤害减免
            // 当你缩进壳里时你会摧毁来犯的敌对弹幕且反弹10倍近战伤害
            // 壳可以在消失前手动取消且能抵挡25次攻击
            // 敌人死亡时有几率爆裂出针刺
            // '你突然有一种想躲进壳里的冲动'");
        }

        public override Color nameColor => new(248, 156, 92);
        

        public override void SetDefaults()
        {
            base.SetDefaults();

            Item.rare = ItemRarityID.LightPurple;
            Item.value = 250000;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            AddEffects(player, Item);
        }
        public static void AddEffects(Player player, Item item)
        {
            player.AddEffect<CactusEffect>(item);
            player.AddEffect<TurtleEffect>(item);

            player.turtleThorns = true;
            player.thorns = 1f;
            FargoSoulsPlayer modPlayer = player.FargoSouls();

            if (player.HasEffect<TurtleEffect>() && !player.HasBuff(ModContent.BuffType<BrokenShellBuff>()) && modPlayer.IsStandingStill && !player.controlUseItem && player.whoAmI == Main.myPlayer && !modPlayer.noDodge)
            {
                modPlayer.TurtleCounter++;

                if (modPlayer.TurtleCounter > 20)
                {
                    player.AddBuff(ModContent.BuffType<ShellHideBuff>(), 2);
                }
            }
            else
            {
                modPlayer.TurtleCounter = 0;
            }

            if (modPlayer.TurtleShellHP < 20 && !player.HasBuff(ModContent.BuffType<BrokenShellBuff>()) && !modPlayer.ShellHide && modPlayer.ForceEffect<TurtleEnchant>())
            {
                modPlayer.turtleRecoverCD--;
                if (modPlayer.turtleRecoverCD <= 0)
                {
                    modPlayer.turtleRecoverCD = 240;

                    modPlayer.TurtleShellHP++;
                }
            }

            //Main.NewText($"shell HP: {TurtleShellHP}, counter: {TurtleCounter}, recovery: {turtleRecoverCD}");
        }

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ItemID.TurtleHelmet)
            .AddIngredient(ItemID.TurtleScaleMail)
            .AddIngredient(ItemID.TurtleLeggings)
            .AddIngredient(null, "CactusEnchant")
            .AddIngredient(ItemID.ChlorophytePartisan)
            .AddIngredient(ItemID.Yelets)

            //chloro saber
            //
            //jungle turtle
            //.AddIngredient(ItemID.Seaweed);
            //.AddIngredient(ItemID.LizardEgg);

            .AddTile(TileID.CrystalBall)
            .Register();
        }
    }

    public class TurtleEffect : AccessoryEffect
    {
        public override Header ToggleHeader => Header.GetHeader<LifeHeader>();
        public override int ToggleItemType => ModContent.ItemType<TurtleEnchant>();

    }
}
