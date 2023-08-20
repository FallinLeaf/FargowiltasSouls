using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using FargowiltasSouls.Content.Buffs.Souls;


namespace FargowiltasSouls.Content.Items.Accessories.Enchantments
{
    public class CrimsonEnchant : BaseEnchant
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();

            // DisplayName.SetDefault("Crimson Enchantment");
            /* Tooltip.SetDefault(
@"After taking a hit, regen is greatly increased until half the hit is healed off
If you take another hit before it's healed, the heal ends early
This does not affect hits dealing less than 10 damage
'The blood of your enemy is your rebirth'"); */
        }

        protected override Color nameColor => new(200, 54, 75);
        public override string wizardEffect => Language.GetTextValue("Mods.FargowiltasSouls.WizardEffect.Crimson");

        public override void SetDefaults()
        {
            base.SetDefaults();

            Item.rare = ItemRarityID.Orange;
            Item.value = 50000;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CrimsonEffect(player);
        }

        public static void CrimsonEffect(Player player)
        {
            FargoSoulsPlayer modPlayer = player.GetModPlayer<FargoSoulsPlayer>();
            modPlayer.CrimsonEnchantActive = true;
        }

        public static void CrimsonHurt(Player player, FargoSoulsPlayer modPlayer, ref Player.HurtModifiers modifiers)
        {
            //if was already healing, stop the heal and do nothing
            if (player.HasBuff(ModContent.BuffType<CrimsonRegenBuff>()))
            {
                player.ClearBuff(ModContent.BuffType<CrimsonRegenBuff>());
            }
            else
            {
                modifiers.ModifyHurtInfo += (ref Player.HurtInfo hurtInfo) =>
                {
                    if (hurtInfo.Damage < 10) return;
                    player.AddBuff(ModContent.BuffType<CrimsonRegenBuff>(), 300);
                        
                    int totalToRegen = hurtInfo.Damage / 2;
                    modPlayer.CrimsonRegenAmount = (int)(totalToRegen / 5f * 2f);
                };
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ItemID.CrimsonHelmet)
            .AddIngredient(ItemID.CrimsonScalemail)
            .AddIngredient(ItemID.CrimsonGreaves)
            //blood axe tging
            .AddIngredient(ItemID.TheUndertaker)
            .AddIngredient(ItemID.TheMeatball)
            .AddIngredient(ItemID.CrimsonHeart)

            //blood rain bow

            .AddTile(TileID.DemonAltar)
            .Register();
        }
    }
}
