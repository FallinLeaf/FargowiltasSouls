using FargowiltasSouls.Core.Systems;
using Terraria;
using Terraria.GameContent.ItemDropRules;

namespace FargowiltasSouls.Core.ItemDropRules.Conditions
{
    public class PatreonDemonKingDropCondition : IItemDropRuleCondition
    {
        protected readonly string Description;

        private static float Chance
        {
            get
            {
                float chance = 3f;
                for (int i = 0; i < WorldSavingSystem.DownedBoss.Length; i++)
                    if (WorldSavingSystem.DownedBoss[i])
                        chance += 0.5f;
                return chance;
            }
        }

        public PatreonDemonKingDropCondition(string description) => Description = description;

        public bool CanDrop(DropAttemptInfo info) => !info.IsInSimulation && SoulConfig.Instance.PatreonFishron &&
            info.npc.lastInteraction != -1 && Main.rand.NextFloat(100) < Chance;

        public bool CanShowItemDropInUI() => true;

        public string GetConditionDescription() => Description;
    }
}
