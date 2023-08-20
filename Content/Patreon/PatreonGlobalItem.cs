using Terraria;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Patreon
{
    public class PatreonGlobalItem : GlobalItem
    {
        public override bool CanUseItem(Item item, Player player)
        {
            if (item.damage > 0 && player.GetModPlayer<PatreonPlayer>().CompOrb
                && item.DamageType != DamageClass.Magic && item.DamageType != DamageClass.Summon
                && item.pick == 0 && item.hammer == 0 && item.axe == 0)
            {
                if (!player.CheckMana(10, true, false))
                    return false;

                player.manaRegenDelay = 300;
                player.GetModPlayer<PatreonPlayer>().CompOrbDrainCooldown = item.useTime + item.reuseDelay + 30;
            }

            return true;
        }
    }
}