using FargowiltasSouls.Content.Projectiles.Masomode;
using Terraria.ID;
using Terraria.ModLoader;
using FargowiltasSouls.Core.NPCMatching;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.Ocean
{
    public class Crab : Shooters
    {
        public Crab() : base(10, ModContent.ProjectileType<BubbleHostile>(), 6, 1, DustID.Water, 100, 0) { }

        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.Crab);
    }
}
