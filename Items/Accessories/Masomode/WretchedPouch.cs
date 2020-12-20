using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace FargowiltasSouls.Items.Accessories.Masomode
{
    [AutoloadEquip(EquipType.Waist)]
    public class WretchedPouch : SoulsItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Wretched Pouch");
            Tooltip.SetDefault(
@"Grants immunity to Shadowflame
You erupt into Shadowflame tentacles when injured
'The accursed incendiary powder of a defeated foe'");
            DisplayName.AddTranslation(GameCulture.Chinese, "诅咒袋子");
            Tooltip.AddTranslation(GameCulture.Chinese, @"'被打败的敌人的诅咒燃烧炸药'
免疫暗影烈焰
受伤时爆发暗影烈焰触须");
        }

        public override void SetDefaults()
        {
            item.width = 20;
            item.height = 20;
            item.accessory = true;
            item.rare = ItemRarityID.Pink;
            item.value = Item.sellPrice(0, 4);
            item.GetGlobalItem<EternityItem>().Eternity = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.buffImmune[BuffID.ShadowFlame] = true;
            player.buffImmune[mod.BuffType("Shadowflame")] = true;
            player.GetModPlayer<FargoPlayer>().WretchedPouch = true;
        }
    }
}