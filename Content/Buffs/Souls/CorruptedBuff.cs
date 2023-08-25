using FargowiltasSouls.Core.Globals;
using Terraria;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Buffs.Souls
{
    public class CorruptedBuff : ModBuff
    {

        public override string Texture => "FargowiltasSouls/Content/Buffs/PlaceholderBuff";

        public override void Update(NPC npc, ref int buffIndex)
        {
            bool check = false;
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (player.active)
                {
                    if (player.GetModPlayer<FargoSoulsPlayer>().EbonwoodEnchantItem != null)
                    {
                        check = true;
                    }
                }
            }
            if (check)
            {
                npc.buffTime[buffIndex] = 60;
            }
            npc.GetGlobalNPC<FargoSoulsGlobalNPC>().Corrupted = true;
        }
    }
}