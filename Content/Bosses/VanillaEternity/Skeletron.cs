using System.IO;
using Terraria.ModLoader.IO;
using FargowiltasSouls.Core.ItemDropRules.Conditions;
using FargowiltasSouls.Content.Projectiles.Masomode;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using FargowiltasSouls.Content.Items.Accessories.Masomode;
using FargowiltasSouls.Content.Buffs.Masomode;
using FargowiltasSouls.Core.Systems;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Common.Utilities;
using FargowiltasSouls.Content.Bosses.Champions.Shadow;
using FargowiltasSouls.Core.NPCMatching;
using FargowiltasSouls.Content.Bosses.DeviBoss;

namespace FargowiltasSouls.Content.Bosses.VanillaEternity
{
    public class SkeletronHead : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.SkeletronHead);

        public int ReticleTarget;
        public int BabyGuardianTimer;
        public int DGSpeedRampup;
        public int MasoArmsTimer;

        public bool InPhase2;

        public bool DroppedSummon;
        public bool SpawnedArms;
        public bool HasSaidEndure;


        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            base.SendExtraAI(npc, bitWriter, binaryWriter);

            binaryWriter.Write7BitEncodedInt(ReticleTarget);
            binaryWriter.Write7BitEncodedInt(BabyGuardianTimer);
            binaryWriter.Write7BitEncodedInt(DGSpeedRampup);
            bitWriter.WriteBit(InPhase2);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            base.ReceiveExtraAI(npc, bitReader, binaryReader);

            ReticleTarget = binaryReader.Read7BitEncodedInt();
            BabyGuardianTimer = binaryReader.Read7BitEncodedInt();
            DGSpeedRampup = binaryReader.Read7BitEncodedInt();
            InPhase2 = bitReader.ReadBit();
        }

        static int BabyGuardianTimerRefresh(NPC npc) => !WorldSavingSystem.MasochistModeReal && NPC.AnyNPCs(NPCID.SkeletronHand) && npc.life > npc.lifeMax * 0.25 ? 240 : 180;

        void GrowHands(NPC npc)
        {
            FargoSoulsUtil.NewNPCEasy(npc.GetSource_FromAI(), npc.Center, NPCID.SkeletronHand, npc.whoAmI, 1f, npc.whoAmI, 0f, 0f, npc.target);
            FargoSoulsUtil.NewNPCEasy(npc.GetSource_FromAI(), npc.Center, NPCID.SkeletronHand, npc.whoAmI, -1f, npc.whoAmI, 0f, 0f, npc.target);

            FargoSoulsUtil.PrintLocalization($"Mods.{Mod.Name}.Message.SkeletronRegrow", new Color(175, 75, 255));
        }

        public override bool SafePreAI(NPC npc)
        {
            bool result = base.SafePreAI(npc);

            EModeGlobalNPC.skeleBoss = npc.whoAmI;

            if (WorldSavingSystem.SwarmActive)
                return result;

            if (!SpawnedArms && npc.life < npc.lifeMax * .5)
            {
                SpawnedArms = true;
                GrowHands(npc);
            }

            if (npc.ai[1] == 0f)
            {
                if (npc.ai[2] == 800 - 90) //telegraph spin
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient && !WorldSavingSystem.MasochistModeReal)
                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, ModContent.ProjectileType<TargetingReticle>(), 0, 0f, Main.myPlayer, npc.whoAmI, npc.type);
                }
                if (npc.ai[2] < 800 - 5)
                {
                    ReticleTarget = npc.target;
                }
            }

            if (npc.ai[1] == 1f || npc.ai[1] == 2f) //spinning or DG mode
            {
                //only runs once per spin
                if (ReticleTarget > -1 && ReticleTarget < Main.maxPlayers)
                {
                    //ensure consistency
                    int threshold = BabyGuardianTimerRefresh(npc);
                    if (BabyGuardianTimer > threshold)
                        BabyGuardianTimer = threshold;

                    //force targeted player back to the one i telegraphed with reticle (otherwise, may target another player when spin starts)
                    npc.target = ReticleTarget;
                    ReticleTarget = -1;

                    npc.netUpdate = true;
                    NetSync(npc);

                    if (!npc.HasValidTarget)
                        npc.TargetClosest(false);

                    if (npc.ai[1] == 1)
                    {
                        CrossGuardianAttack(npc);
                    }
                }

                float ratio = (float)npc.life / npc.lifeMax;
                float cooldown = 20f;
                if (!WorldSavingSystem.MasochistModeReal)
                    cooldown += 100f * ratio;
                if (++npc.localAI[2] >= cooldown) //spray bones
                {
                    npc.localAI[2] = 0f;
                    if (cooldown > 0 && npc.HasPlayerTarget && Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 speed = Vector2.Normalize(Main.player[npc.target].Center - npc.Center) * 6f;
                        for (int i = 0; i < 8; i++)
                        {
                            Vector2 vel = speed.RotatedBy(Math.PI * 2 / 8 * i);
                            vel += npc.velocity * (1f - ratio);
                            vel.Y -= Math.Abs(vel.X) * 0.2f;
                            Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, vel, ModContent.ProjectileType<SkeletronBone>(), npc.defDamage / 9 * 2, 0f, Main.myPlayer);
                        }
                    }
                }

                if (npc.life < npc.lifeMax * .75 && npc.ai[1] == 1f && --BabyGuardianTimer < 0)
                {
                    BabyGuardianTimer = BabyGuardianTimerRefresh(npc);

                    SoundEngine.PlaySound(SoundID.ForceRoarPitched, npc.Center);

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        SprayHomingBabies(npc);

                        if (WorldSavingSystem.MasochistModeReal)
                            DungeonGuardianAttack(npc);
                    }
                }
            }
            else
            {
                if (SpawnedArms && WorldSavingSystem.MasochistModeReal && ++MasoArmsTimer == 120)
                {
                    GrowHands(npc);
                }

                if (npc.ai[2] == 0)
                {
                    //compensate for not changing targets when beginning spin
                    npc.TargetClosest(false);

                    //prevent skeletron from firing his stupid tick 1 no telegraph skull right after finishing spin
                    if (!WorldSavingSystem.MasochistModeReal)
                        npc.ai[2] = 1;
                }

                if (npc.life < npc.lifeMax * .75) //phase 2
                {
                    //vomit skeletons
                    if (npc.ai[2] <= 60 && npc.ai[2] % 15 == 0 && !NPC.AnyNPCs(NPCID.SkeletronHand))
                    {
                        int[] skeletons = {
                            NPCID.BoneThrowingSkeleton,
                            NPCID.BoneThrowingSkeleton2,
                            NPCID.BoneThrowingSkeleton3,
                            NPCID.BoneThrowingSkeleton4
                        };

                        if (Main.npc.Count(n => n.active && skeletons.Contains(n.type)) < 12)
                        {
                            float gravity = 0.4f; //shoot down
                            const float time = 60f;
                            Vector2 distance = Main.player[npc.target].Top - npc.Center + Main.rand.NextVector2Circular(80, 80);
                            distance.X /= time;
                            distance.Y = distance.Y / time - 0.5f * gravity * time;

                            FargoSoulsUtil.NewNPCEasy(
                                npc.GetSource_FromAI(),
                                npc.Center,
                                Main.rand.Next(skeletons),
                                velocity: distance);

                            SoundEngine.PlaySound(SoundID.NPCDeath13, npc.Center);
                        }
                    }

                    if (--BabyGuardianTimer < 0)
                    {
                        BabyGuardianTimer = BabyGuardianTimerRefresh(npc);
                        if (!WorldSavingSystem.MasochistModeReal)
                            BabyGuardianTimer += 60;

                        SoundEngine.PlaySound(SoundID.ForceRoarPitched, npc.Center);

                        for (int j = -1; j <= 1; j++) //to both sides
                        {
                            if (j == 0)
                                continue;

                            const int gap = 40;
                            const int max = 14;
                            float modifier = 1f - (float)npc.life / npc.lifeMax;
                            modifier *= 4f / 3f; //scaling maxes at 25% life
                            if (modifier > 1f || WorldSavingSystem.MasochistModeReal) //cap it, or force it to cap in emode
                                modifier = 1f;
                            int actualNumberToSpawn = (int)(max * modifier);
                            Vector2 baseVel = npc.DirectionTo(Main.player[npc.target].Center).RotatedBy(MathHelper.ToRadians(gap) * j);
                            for (int k = 0; k < actualNumberToSpawn; k++) //a fan of skulls
                            {
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    float velModifier = 1f + 9f * k / max;
                                    Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, velModifier * baseVel.RotatedBy(MathHelper.ToRadians(10) * j * k),
                                        ModContent.ProjectileType<SkeletronGuardian2>(), FargoSoulsUtil.ScaledProjectileDamage(npc.damage, 0.8f), 0f, Main.myPlayer);
                                }
                            }
                        }

                        if (Main.netMode != NetmodeID.MultiplayerClient) //one more shot straight behind skeletron
                        {
                            float velModifier = 10f;
                            Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, velModifier * npc.DirectionFrom(Main.player[npc.target].Center),
                                ModContent.ProjectileType<SkeletronGuardian2>(), FargoSoulsUtil.ScaledProjectileDamage(npc.damage, 0.8f), 0f, Main.myPlayer);
                        }
                    }
                }
            }

            if (npc.ai[1] == 2f)
            {
                npc.defense = 9999;
                npc.damage = npc.defDamage * 15;

                if (!Main.dayTime && !WorldSavingSystem.MasochistModeReal)
                {
                    if (++DGSpeedRampup < 120)
                    {
                        npc.position -= npc.velocity * (120 - DGSpeedRampup) / 120;
                    }
                }
            }

            EModeUtils.DropSummon(npc, "SuspiciousSkull", NPC.downedBoss3, ref DroppedSummon);

            return result;
        }

        void CrossGuardianAttack(NPC npc)
        {
            if (!WorldSavingSystem.MasochistModeReal)
            {
                for (int i = 0; i < Main.maxProjectiles; i++) //also clear leftover babies
                {
                    if (Main.projectile[i].active && Main.projectile[i].hostile && Main.projectile[i].type == ModContent.ProjectileType<SkeletronGuardian2>())
                        Main.projectile[i].Kill();
                }
            }

            if ((npc.life >= npc.lifeMax * .75 || WorldSavingSystem.MasochistModeReal) && Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int i = 0; i < 4; i++)
                {
                    for (int j = -2; j <= 2; j++)
                    {
                        Vector2 spawnPos = new(1200, 80 * j);
                        Vector2 vel = -8 * Vector2.UnitX;
                        spawnPos = Main.player[npc.target].Center + spawnPos.RotatedBy(Math.PI / 2 * (i + 0.5));
                        vel = vel.RotatedBy(Math.PI / 2 * (i + 0.5));
                        int p = Projectile.NewProjectile(npc.GetSource_FromThis(), spawnPos, vel, ModContent.ProjectileType<ShadowGuardian>(),
                            FargoSoulsUtil.ScaledProjectileDamage(npc.damage), 0f, Main.myPlayer);
                        if (p != Main.maxProjectiles)
                            Main.projectile[p].timeLeft = 1200 / 8 + 1;
                    }
                }
            }
        }

        void SprayHomingBabies(NPC npc)
        {
            const int max = 30;
            float modifier = 1f - (float)npc.life / npc.lifeMax;
            modifier *= 4f / 3f; //scaling maxes at 25% life
            if (modifier > 1f || WorldSavingSystem.MasochistModeReal) //cap it, or force it to cap in emode
                modifier = 1f;
            int actualNumberToSpawn = (int)(max * modifier);
            for (int i = 0; i < actualNumberToSpawn; i++)
            {
                float speed = Main.rand.NextFloat(3f, 9f);
                Vector2 velocity = speed * npc.DirectionFrom(Main.player[npc.target].Center).RotatedBy(Math.PI * (Main.rand.NextDouble() - 0.5));
                float ai1 = speed / (60f + Main.rand.NextFloat(actualNumberToSpawn * 2));
                Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, velocity, ModContent.ProjectileType<SkeletronGuardian>(), FargoSoulsUtil.ScaledProjectileDamage(npc.damage, 0.8f), 0f, Main.myPlayer, 0f, ai1);
            }
        }

        void DungeonGuardianAttack(NPC npc)
        {
            switch(Main.rand.Next(4))
            {
                case 0: //walls of guardians
                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = -2; j <= 2; j++)
                        {
                            Vector2 spawnPos = new(1200, 80 * j);
                            Vector2 vel = -8 * Vector2.UnitX;
                            spawnPos = Main.player[npc.target].Center + spawnPos.RotatedBy(Math.PI / 2 * i);
                            vel = vel.RotatedBy(Math.PI / 2 * i);
                            Projectile.NewProjectile(npc.GetSource_FromThis(), spawnPos, vel, ModContent.ProjectileType<ShadowGuardian>(),
                                FargoSoulsUtil.ScaledProjectileDamage(npc.damage), 0f, Main.myPlayer);
                        }
                    }
                    break;

                case 1: //ring of babies
                    {
                        const int max = 16;
                        Vector2 baseOffset = npc.DirectionTo(Main.player[npc.target].Center);
                        for (int i = 0; i < max; i++)
                        {
                            Projectile.NewProjectile(npc.GetSource_FromThis(), Main.player[npc.target].Center + 1000 * baseOffset.RotatedBy(2 * Math.PI / max * i),
                                -8f * baseOffset.RotatedBy(2 * Math.PI / max * i), ModContent.ProjectileType<DeviGuardian>(),
                                FargoSoulsUtil.ScaledProjectileDamage(npc.damage), 0f, Main.myPlayer);
                        }
                    }
                    break;

                case 2: //homing skulls
                    {
                        Vector2 speed = Main.player[npc.target].Center - npc.Center;
                        speed.X += Main.rand.Next(-20, 21);
                        speed.Y += Main.rand.Next(-20, 21);
                        speed.Normalize();
                        speed *= 3f;
                        for (int i = 0; i < 6; i++)
                        {
                            Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, speed.RotatedBy(Math.PI / 3 * i),
                                ProjectileID.Skull, FargoSoulsUtil.ScaledProjectileDamage(npc.damage), 0, Main.myPlayer, -1f, 0);
                        }
                    }
                    break;

                case 3:
                    CrossGuardianAttack(npc);
                    break;

                default:
                    SprayHomingBabies(npc);
                    break;
            }
        }

        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            if (!WorldSavingSystem.SwarmActive && Main.npc.Any(n => n.active && n.type == NPCID.SkeletronHand && n.ai[0] == npc.whoAmI))
                modifiers.FinalDamage /= 2;

            base.ModifyIncomingHit(npc, ref modifiers);
        }

        public override bool CheckDead(NPC npc)
        {
            if (npc.ai[1] != 2f && !WorldSavingSystem.SwarmActive)
            {
                SoundEngine.PlaySound(SoundID.Roar, npc.Center);

                npc.life = npc.lifeMax / 176;
                if (npc.life < 50)
                    npc.life = 50;

                npc.defense = 9999;
                npc.damage = npc.defDamage * 15;

                npc.ai[1] = 2f;
                npc.netUpdate = true;
                NetSync(npc);

                if (!HasSaidEndure)
                {
                    HasSaidEndure = true;
                    FargoSoulsUtil.PrintLocalization($"Mods.{Mod.Name}.Message.SkeletronGuardian", new Color(175, 75, 255));
                }
                return false;
            }

            return base.CheckDead(npc);
        }

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(npc, target, hurtInfo);

            target.AddBuff(ModContent.BuffType<DefenselessBuff>(), 300);
            target.AddBuff(ModContent.BuffType<LethargicBuff>(), 300);
        }

        public override void LoadSprites(NPC npc, bool recolor)
        {
            base.LoadSprites(npc, recolor);

            LoadNPCSprite(recolor, npc.type);
            LoadBossHeadSprite(recolor, 19);
            LoadGoreRange(recolor, 54, 57);

            LoadSpecial(recolor, ref TextureAssets.BoneArm, ref FargowiltasSouls.TextureBuffer.BoneArm, "Arm_Bone");
        }
    }

    public class SkeletronHand : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.SkeletronHand);

        public int AttackTimer;

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            base.SendExtraAI(npc, bitWriter, binaryWriter);

            binaryWriter.Write7BitEncodedInt(AttackTimer);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            base.ReceiveExtraAI(npc, bitReader, binaryReader);

            AttackTimer = binaryReader.Read7BitEncodedInt();
        }

        public override bool SafePreAI(NPC npc)
        {
            bool result = base.SafePreAI(npc);

            if (WorldSavingSystem.SwarmActive)
                return result;

            NPC head = FargoSoulsUtil.NPCExists(npc.ai[1], NPCID.SkeletronHead);
            if (head == null)
                return result;

            if (npc.timeLeft < 60) //never despawn normally
                npc.timeLeft = 60;

            //vanilla ai sometimes throws hand too far away and self despawns
            //if too far, tp back and reset ai
            if (npc.Distance(head.Center) > 1600)
            {
                npc.Center = head.Center;
                npc.ai[2] = 0;
                npc.ai[3] = 0;
                npc.localAI[0] = 0;
                npc.localAI[1] = 0;
                npc.localAI[2] = 0;
                npc.localAI[3] = 0;
                npc.netUpdate = true;
            }

            if (head.ai[1] == 1f || head.ai[1] == 2f) //spinning or DG mode
            {
                if (AttackTimer > 0 && head.life >= head.lifeMax * .75) //for a short period
                {
                    if (--AttackTimer < 65)
                    {
                        Vector2 centerPoint = head.Center - 10 * 16 * Vector2.UnitY;
                        if (!npc.HasValidTarget || npc.Distance(centerPoint) > 15 * 16)
                        {
                            AttackTimer++; //pause here, dont begin guardians attack until in range
                        }
                        else if (AttackTimer % 10 == 0 && Main.netMode != NetmodeID.MultiplayerClient) //periodic below 50%
                        {
                            Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, npc.DirectionTo(Main.player[npc.target].Center), ModContent.ProjectileType<SkeletronGuardian2>(), FargoSoulsUtil.ScaledProjectileDamage(npc.damage), 0f, Main.myPlayer);
                        }
                    }
                }
            }
            else
            {
                if (AttackTimer != 65 + 150)
                {
                    AttackTimer = 65 + 150;

                    if (Main.netMode != NetmodeID.MultiplayerClient && npc.HasPlayerTarget) //throw undead miner
                    {
                        float gravity = 0.4f; //shoot down
                        const float time = 60f;
                        Vector2 distance = Main.player[npc.target].Top - npc.Center + Main.rand.NextVector2Circular(80, 80);
                        distance.X /= time;
                        distance.Y = distance.Y / time - 0.5f * gravity * time;

                        FargoSoulsUtil.NewNPCEasy(
                            npc.GetSource_FromAI(),
                            npc.Center,
                            Main.rand.Next(new int[] {
                                NPCID.BoneThrowingSkeleton,
                                NPCID.BoneThrowingSkeleton2,
                                NPCID.BoneThrowingSkeleton3,
                                NPCID.BoneThrowingSkeleton4
                            }),
                            velocity: distance);
                    }
                }
            }

            return result;
        }

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(npc, target, hurtInfo);

            target.AddBuff(ModContent.BuffType<LethargicBuff>(), 300);
            target.AddBuff(BuffID.Dazed, 60);
        }

        public override void LoadSprites(NPC npc, bool recolor)
        {
            base.LoadSprites(npc, recolor);

            LoadNPCSprite(recolor, npc.type);
        }
    }
}
