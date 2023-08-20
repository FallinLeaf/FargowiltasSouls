using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using FargowiltasSouls.Content.Buffs.Souls;
using FargowiltasSouls.Content.Buffs.Masomode;
using FargowiltasSouls.Core.Systems;
using FargowiltasSouls.Core.Globals;

namespace FargowiltasSouls.Content.Buffs
{
    internal class FargoGlobalBuff : GlobalBuff
    {
        public override void ModifyBuffText(int type, ref string buffName, ref string tip, ref int rare)
        {
            if (WorldSavingSystem.EternityMode)
            {
                if (type == BuffID.ShadowDodge)
                    tip += "\n" + Language.GetTextValue("Mods.FargowiltasSouls.EModeBalance.ShadowDodge");
                else if (type == BuffID.IceBarrier)
                    tip += "\n" + Language.GetTextValue("Mods.FargowiltasSouls.EModeBalance.IceBarrier");
            }
        }

        public static int[] DebuffsToLetDecreaseNormally => new int[] {
            BuffID.Frozen,
            BuffID.Stoned,
            BuffID.Cursed,
            ModContent.BuffType<FusedBuff>(),
            ModContent.BuffType<TimeFrozenBuff>(),
            ModContent.BuffType<StunnedBuff>()
        };

        public override void Update(int type, Player player, ref int buffIndex)
        {
            switch (type)
            {
                case BuffID.Slimed:
                    Main.buffNoTimeDisplay[type] = false;
                    if (WorldSavingSystem.EternityMode)
                        player.GetModPlayer<FargoSoulsPlayer>().Slimed = true;
                    break;

                case BuffID.BrainOfConfusionBuff:
                    if (WorldSavingSystem.EternityMode)
                        player.AddBuff(ModContent.BuffType<BrainOfConfusionBuff>(), player.buffTime[buffIndex] * 2);
                    break;

                case BuffID.OnFire:
                    if (WorldSavingSystem.EternityMode && Main.raining && player.position.Y < Main.worldSurface * 16
                        && Framing.GetTileSafely(player.Center).WallType == WallID.None && player.buffTime[buffIndex] > 2)
                        player.buffTime[buffIndex] -= 1;
                    break;

                case BuffID.Chilled:
                    if (WorldSavingSystem.EternityMode && player.buffTime[buffIndex] > 60 * 15)
                        player.buffTime[buffIndex] = 60 * 15;
                    break;

                case BuffID.Dazed:
                    if (player.whoAmI == Main.myPlayer && player.buffTime[buffIndex] % 60 == 55)
                        SoundEngine.PlaySound(new SoundStyle("FargowiltasSouls/Assets/Sounds/DizzyBird"));
                    break;


                case BuffID.ThornWhipPlayerBuff:
                    if (WorldSavingSystem.EternityMode)
                    {
                        player.GetAttackSpeed(DamageClass.Melee) -= 0.20f;
                        player.GetAttackSpeed(DamageClass.SummonMeleeSpeed) += 0.20f;
                    }
                    goto case BuffID.CoolWhipPlayerBuff;

                case BuffID.SwordWhipPlayerBuff:
                    if (WorldSavingSystem.EternityMode)
                    {
                        player.GetAttackSpeed(DamageClass.Melee) -= 0.35f;
                        player.GetAttackSpeed(DamageClass.SummonMeleeSpeed) += 0.35f;
                    }
                    goto case BuffID.CoolWhipPlayerBuff;

                case BuffID.ScytheWhipPlayerBuff:
                    if (WorldSavingSystem.EternityMode)
                    {
                        player.GetAttackSpeed(DamageClass.Melee) -= 0.50f;
                        player.GetAttackSpeed(DamageClass.SummonMeleeSpeed) += 0.50f;
                    }
                    goto case BuffID.CoolWhipPlayerBuff;

                case BuffID.CoolWhipPlayerBuff:
                    if (WorldSavingSystem.EternityMode)
                    {
                        if (player.GetModPlayer<EModePlayer>().HasWhipBuff)
                            player.buffTime[buffIndex] = Math.Min(player.buffTime[buffIndex], 1);
                        player.GetModPlayer<EModePlayer>().HasWhipBuff = true;
                    }
                    break;

                default:
                    break;
            }

            if (WorldSavingSystem.EternityMode && player.buffTime[buffIndex] > 5 && Main.debuff[type] && player.GetModPlayer<EModePlayer>().ShorterDebuffsTimer <= 0
                && !Main.buffNoTimeDisplay[type]
                && type != BuffID.Tipsy && (!BuffID.Sets.NurseCannotRemoveDebuff[type] || type == BuffID.ManaSickness || type == BuffID.PotionSickness)
                && !DebuffsToLetDecreaseNormally.Contains(type)
                && !(type == BuffID.Confused && FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.brainBoss, NPCID.BrainofCthulhu)))
            {
                player.buffTime[buffIndex] -= 1;
            }

            base.Update(type, player, ref buffIndex);
        }

        public override void Update(int type, NPC npc, ref int buffIndex)
        {
            switch (type)
            {
                case BuffID.BrokenArmor:
                    npc.GetGlobalNPC<FargoSoulsGlobalNPC>().BrokenArmor = true;
                    break;

                //                //case BuffID.Chilled: npc.GetGlobalNPC<NPCs.FargoSoulsGlobalNPC>().Chilled = true; break;

                case BuffID.Darkness:
                    npc.color = Color.Gray;

                    if (npc.buffTime[buffIndex] % 30 == 0)
                    {
                        for (int i = 0; i < Main.maxNPCs; i++)
                        {
                            NPC target = Main.npc[i];
                            if (target.active && !target.friendly && Vector2.Distance(npc.Center, target.Center) < 250)
                            {
                                Vector2 velocity = Vector2.Normalize(target.Center - npc.Center) * 5;
                                Projectile.NewProjectile(npc.GetSource_Buff(buffIndex), npc.Center, velocity, ProjectileID.ShadowFlame, 40 + FargoSoulsUtil.ScaledProjectileDamage(npc.damage), 0, Main.myPlayer);
                                if (Main.rand.NextBool(3))
                                    break;
                            }
                        }
                    }
                    break;

                case BuffID.Electrified:
                    npc.GetGlobalNPC<FargoSoulsGlobalNPC>().Electrified = true;
                    break;

                case BuffID.Suffocation:
                    npc.GetGlobalNPC<FargoSoulsGlobalNPC>().Suffocation = true;
                    break;

                case BuffID.OnFire:
                    if (WorldSavingSystem.EternityMode && Main.raining && npc.position.Y < Main.worldSurface * 16
                        && Framing.GetTileSafely(npc.Center).WallType == WallID.None && npc.buffTime[buffIndex] > 2)
                        npc.buffTime[buffIndex] -= 1;
                    break;

                case BuffID.BoneWhipNPCDebuff:
                case BuffID.MaceWhipNPCDebuff:
                case BuffID.RainbowWhipNPCDebuff:
                case BuffID.SwordWhipNPCDebuff:
                case BuffID.ThornWhipNPCDebuff:
                    if (WorldSavingSystem.EternityMode && npc.GetGlobalNPC<EModeGlobalNPC>().HasWhipDebuff)
                        npc.buffTime[buffIndex] = Math.Min(npc.buffTime[buffIndex], 1);
                    npc.GetGlobalNPC<EModeGlobalNPC>().HasWhipDebuff = true;
                    break;

                default:
                    break;
            }
        }
    }
}