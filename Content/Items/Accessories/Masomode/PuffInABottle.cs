using Terraria;
using Terraria.ID;

namespace FargowiltasSouls.Content.Items.Accessories.Masomode
{
    public class PuffInABottle : SoulsItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Puff in a Bottle");
            // Tooltip.SetDefault(@"Allows the holder to double jump");

            Terraria.GameContent.Creative.CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.CloudinaBottle);
            Item.value = (int)(Item.value * 0.75);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.hasJumpOption_Cloud = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Bottle)
                .AddIngredient(ItemID.Cloud)
                .Register();
        }
    }
}