using Terraria;
using Terraria.ID;

namespace FargowiltasSouls.Content.Items.Accessories.Masomode
{
    public class TimsConcoction : SoulsItem
    {
        public override bool Eternity => true;

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Tim's Concoction");
            /* Tooltip.SetDefault(@"Certain enemies will drop potions when defeated
'Smells funny'"); */

            Terraria.GameContent.Creative.CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.accessory = true;
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(0, 4);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            if (player.GetToggleValue("MasoConcoction"))
                player.GetModPlayer<FargoSoulsPlayer>().TimsConcoction = true;
        }
    }
}