using FargowiltasSouls.Common.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Items.Accessories.Enchantments
{
    public abstract class BaseEnchant : SoulsItem
    {
        protected abstract Color nameColor { get; }
        public string wizardEffect()
        {
            string text = Language.GetTextValue($"Mods.FargowiltasSouls.WizardEffect.{Name.Replace("Enchantment", "").Replace("Enchant", "")}");
            if (text.Contains("Mods.FargowiltasSouls.WizardEffect") || text.Length <= 1) //if there's no localization entry or it's empty
            {
                return "No upgrade";
            }
            else return text;
        }

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();

            ItemID.Sets.ItemNoGravity[Type] = true;
            Terraria.GameContent.Creative.CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SafeModifyTooltips(List<TooltipLine> tooltips)
        {
            base.SafeModifyTooltips(tooltips);

            if (tooltips.TryFindTooltipLine("ItemName", out TooltipLine itemNameLine))
                itemNameLine.OverrideColor = nameColor;
        }

        public override void SetDefaults()
        {
            base.SetDefaults();

            Item.width = 20;
            Item.height = 20;
            Item.accessory = true;
        }

        public sealed override void UpdateEquip(Player player)
        {
            //todo, change this to sealed UpdateAccessory and refactor every single enchantment file to accommodate
            player.GetModPlayer<FargoSoulsPlayer>().EquippedEnchants.Add(this);
        }
    }
}
