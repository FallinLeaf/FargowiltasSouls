using FargowiltasSouls.Content.Items.Accessories.Masomode;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader;

namespace FargowiltasSouls.Core.ItemDropRules.Conditions
{
    public class TimsConcoctionDropCondition : IItemDropRuleCondition
    {
        public bool CanDrop(DropAttemptInfo info) => !info.IsInSimulation && info.npc.lastInteraction != -1 && Main.player[info.npc.lastInteraction].GetModPlayer<FargoSoulsPlayer>().TimsConcoction;
        public bool CanShowItemDropInUI() => true;

        public string GetConditionDescription() => $"[i:{ModContent.ItemType<TimsConcoction>()}]Tim's Concoction drop rate";
    }
}
