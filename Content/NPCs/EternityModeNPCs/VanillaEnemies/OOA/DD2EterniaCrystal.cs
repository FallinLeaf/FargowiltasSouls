using FargowiltasSouls.Common.Utilities;
using Terraria;
using Terraria.GameContent.Events;
using Terraria.ID;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.OOA
{
    public class DD2EterniaCrystal : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.DD2EterniaCrystal);

        public int InvulTimer;

        public override void AI(NPC npc)
        {
            base.AI(npc);

            if (DD2Event.Ongoing && DD2Event.TimeLeftBetweenWaves > 600)
                DD2Event.TimeLeftBetweenWaves = 600;

            //cant use HasValidTarget for this because that returns true even if betsy is targeting the crystal (npc.target seems to become -1)
            if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.betsyBoss, NPCID.DD2Betsy))
            {
                int p = npc.FindClosestPlayer(out float distanceToPlayer);
                if (p != -1 && distanceToPlayer < 3000)
                {
                    InvulTimer = 30; //wait before becoming fully vulnerable
                    if (npc.life < npc.lifeMax && npc.life < 500)
                        npc.life++;
                }
            }

            if (InvulTimer > 0)
                InvulTimer--;
        }

        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            if (InvulTimer > 0)
                modifiers.Null();

            base.ModifyIncomingHit(npc, ref modifiers);
        }
    }
}
