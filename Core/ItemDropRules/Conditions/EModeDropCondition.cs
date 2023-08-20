using FargowiltasSouls.Content.Items;
using FargowiltasSouls.Core.Systems;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader;

namespace FargowiltasSouls.Core.ItemDropRules.Conditions
{
    public class EModeDropCondition : IItemDropRuleCondition
    {
        public bool CanDrop(DropAttemptInfo info) => !info.IsInSimulation && WorldSavingSystem.EternityMode;

        public bool CanShowItemDropInUI() => WorldSavingSystem.EternityMode;

        public string GetConditionDescription() => $"[i:{ModContent.ItemType<Masochist>()}]Eternity Mode drop rate";
    }
}
