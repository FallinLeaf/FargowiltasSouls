using System.IO;
using Terraria.ModLoader.IO;
using FargowiltasSouls.Content.Projectiles.Masomode;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using FargowiltasSouls.Content.Items.Accessories.Masomode;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.Snow
{
    public class IceGolem : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.IceGolem);

        public int Counter;

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            base.SendExtraAI(npc, bitWriter, binaryWriter);

            binaryWriter.Write7BitEncodedInt(Counter);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            base.ReceiveExtraAI(npc, bitReader, binaryReader);

            Counter = binaryReader.Read7BitEncodedInt();
        }

        public override void AI(NPC npc)
        {
            base.AI(npc);

            if (npc.velocity.Y < 0) //higher jump
                npc.position.Y += npc.velocity.Y;

            if (++Counter % 120 == 0)
            {
                if (npc.HasPlayerTarget && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, new Vector2(6f, 0f).RotatedByRandom(2 * Math.PI),
                        ModContent.ProjectileType<FrostfireballHostile>(), FargoSoulsUtil.ScaledProjectileDamage(npc.damage, 0.8f), 0f, Main.myPlayer, npc.target, 30f);
                }
            }

            if (Counter == 600 - 60)
            {
                FargoSoulsUtil.DustRing(npc.Center, 64, DustID.IceTorch, 16f, scale: 3f);
            }

            if (Counter > 600)
            {
                Counter = 0;

                if (npc.HasPlayerTarget && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    const int max = 16;
                    for (int i = 0; i < max; i++)
                    {
                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, 6f * npc.DirectionTo(Main.player[npc.target].Center).RotatedBy(MathHelper.TwoPi / max * i),
                            ModContent.ProjectileType<FrostfireballHostile>(), FargoSoulsUtil.ScaledProjectileDamage(npc.damage), 0f, Main.myPlayer, npc.target, 180f);
                    }
                }

                NetSync(npc);
            }
        }
    }
}
