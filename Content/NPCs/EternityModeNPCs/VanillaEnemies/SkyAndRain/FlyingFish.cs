using Terraria;
using Terraria.ID;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.SkyAndRain
{
    public class FlyingFish : Shooters
    {
        public FlyingFish() : base(70, ProjectileID.WaterStream, 10, 1, DustID.Water, 250) { }

        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.FlyingFish);

        public override void OnFirstTick(NPC npc)
        {
            base.OnFirstTick(npc);

            if (Main.rand.NextBool(4))
                EModeGlobalNPC.Horde(npc, Main.rand.Next(1, 5));
        }
    }
}
