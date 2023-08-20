using FargowiltasSouls.Content.Buffs.Masomode;
using FargowiltasSouls.Core.NPCMatching;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.PirateInvasion
{
    public class Parrot : NoclipFliers
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.Parrot);

        public override void AI(NPC npc)
        {
            base.AI(npc);

            CanNoclip = true;
        }

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(npc, target, hurtInfo);

            target.AddBuff(ModContent.BuffType<SqueakyToyBuff>(), 120);
            target.AddBuff(ModContent.BuffType<MidasBuff>(), 600);
            //target.AddBuff(ModContent.BuffType<LivingWasteland>(), 600);
        }
    }
}
