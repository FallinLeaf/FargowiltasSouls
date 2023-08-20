using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace FargowiltasSouls.Content.Buffs.Souls
{
    public class CrimsonRegenBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Crimson Regen");
            // Description.SetDefault("You are regenning your last wound");
            Main.buffNoSave[Type] = true;
            //Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            //player.buffTime[buffIndex] = 2;
            //player.GetModPlayer<FargoSoulsPlayer>().CrimsonRegen = true;
            player.lifeRegenTime++;
            player.lifeRegen += player.GetModPlayer<FargoSoulsPlayer>().CrimsonRegenAmount;

            for (int i = 0; i < 10; i++)
            {
                int num6 = Dust.NewDust(player.position, player.width, player.height, DustID.Blood, 0f, 0f, 175, default, 1.75f);
                Main.dust[num6].noGravity = true;
                Main.dust[num6].velocity *= 0.75f;
                int num7 = Main.rand.Next(-40, 41);
                int num8 = Main.rand.Next(-40, 41);
                Dust dust = Main.dust[num6];
                dust.position.X += num7;
                Dust dust2 = Main.dust[num6];
                dust2.position.Y += num8;
                Main.dust[num6].velocity.X = (float)-(float)num7 * 0.075f;
                Main.dust[num6].velocity.Y = (float)-(float)num8 * 0.075f;
            }

        }
    }
}