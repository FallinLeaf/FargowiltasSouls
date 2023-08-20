using Terraria;
using Terraria.ID;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.Cavern
{
    public class GraniteGolem : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.GraniteGolem);

        public override void AI(NPC npc)
        {
            base.AI(npc);

            if (npc.ai[2] < 0f) //while shielding, reflect
                EModeGlobalNPC.CustomReflect(npc, DustID.Granite, 2);
        }

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(npc, target, hurtInfo);

            target.GetModPlayer<FargoSoulsPlayer>().AddBuffNoStack(BuffID.Stoned, 60);
        }
    }
}
