using FargowiltasSouls.Content.Bosses.DeviBoss;
using FargowiltasSouls.Content.Items.Materials;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Items.BossBags
{
    public class DeviBag : BossBag
    {
        protected override bool IsPreHMBag => true;

        public override void ModifyItemLoot(ItemLoot itemLoot)
        {
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<DeviatingEnergy>(), 1, 15, 30));
            itemLoot.Add(ItemDropRule.CoinsBasedOnNPCValue(ModContent.NPCType<DeviBoss>()));
        }
    }
}