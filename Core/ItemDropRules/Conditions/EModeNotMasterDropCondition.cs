using FargowiltasSouls.Core.Systems;
using Terraria;
using Terraria.GameContent.ItemDropRules;

namespace FargowiltasSouls.Core.ItemDropRules.Conditions
{
    public class EModeNotMasterDropCondition : IItemDropRuleCondition
    {
        public bool CanDrop(DropAttemptInfo info) => !info.IsInSimulation && !info.IsMasterMode && WorldSavingSystem.EternityMode;

        public bool CanShowItemDropInUI() => !Main.masterMode && WorldSavingSystem.EternityMode;

        public string GetConditionDescription() => "This is a Master/Eternity Mode drop rate";
    }
}
