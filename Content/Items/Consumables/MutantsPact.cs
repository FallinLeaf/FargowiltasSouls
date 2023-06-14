using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;

namespace FargowiltasSouls.Content.Items.Consumables
{
    public class MutantsPact : SoulsItem
    {
        public override bool Eternity => true;

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Mutant's Pact");
            // Tooltip.SetDefault("Permanently increases the number of accessory slots");
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(4, 6));
            Terraria.GameContent.Creative.CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.rare = ItemRarityID.Purple;
            Item.maxStack = 99;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.consumable = true;
            Item.UseSound = SoundID.Item123;
            Item.value = Item.sellPrice(0, 15);
        }

        public override bool CanUseItem(Player player)
        {
            return !player.GetModPlayer<FargoSoulsPlayer>().MutantsPactSlot;
        }

        public override bool? UseItem(Player player)
        {
            if (player.itemAnimation > 0 && player.itemTime == 0)
            {
                player.GetModPlayer<FargoSoulsPlayer>().MutantsPactSlot = true;

                SoundEngine.PlaySound(SoundID.Roar, player.Center);
                if (!Main.dedServ)
                    SoundEngine.PlaySound(new SoundStyle("FargowiltasSouls/Sounds/Thunder"), player.Center);
            }
            return true;
        }

        public override Color? GetAlpha(Color lightColor) => Color.White;
    }
}