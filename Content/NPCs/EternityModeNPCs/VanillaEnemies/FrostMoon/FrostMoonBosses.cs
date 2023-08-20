using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.FrostMoon
{
    public class FrostMoonBosses : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchTypeRange(
            NPCID.Everscream,
            NPCID.SantaNK1,
            NPCID.IceQueen
        );

        public override bool PreKill(NPC npc)
        {
            if (Main.snowMoon && NPC.waveNumber < 15)
            {
                for (int i = 0; i < 10; i++)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        Item.NewItem(npc.GetSource_Loot(), npc.Hitbox, ItemID.Heart);
                }
                return false;
            }

            return base.PreKill(npc);
        }
    }
}
