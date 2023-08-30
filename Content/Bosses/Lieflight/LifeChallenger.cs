using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.Bestiary;
using Terraria.Localization;
using FargowiltasSouls.Content.Projectiles;
using FargowiltasSouls.Content.Items.BossBags;
using FargowiltasSouls.Content.Items.Pets;
using FargowiltasSouls.Content.Items.Placables.Relics;
using FargowiltasSouls.Content.Items.Placables.Trophies;
using FargowiltasSouls.Content.Items.Summons;
using FargowiltasSouls.Content.Items.Weapons.Challengers;
using FargowiltasSouls.Content.Buffs.Masomode;
using FargowiltasSouls.Core.Systems;

namespace FargowiltasSouls.Content.Bosses.Lieflight
{

    [AutoloadBossHead]
    public class LifeChallenger : ModNPC
    {
        #region Variables
        

        const int DefaultHeight = 200;
        const int DefaultWidth = 200;

        public double Phase;

        private bool first = true;

        private bool flyfast;

        private bool Flying = true;

        private bool Charging = false;

        private bool AttackF1;

        private int Attacking = -1;

        public bool PhaseOne = true;

        public bool Variant = false;

        private int dustcounter;

        public int state;

        private int oldstate = 999;

        private int statecount = 10;

        private bool shoot = false;

        private readonly List<int> availablestates = new(0);

        public Vector2 LockVector1 = new(0, 0);

        private Vector2 LockVector2 = new(0, 0);

        private Vector2 LockVector3 = new(0, 0);

        private Vector2 AuraCenter = new(0, 0);

        private int index;

        private double rotspeed = 0;

        private double rot = 0;

        private bool HitPlayer = false;

        int index2;

        int firstblaster = 2;

        private bool UseTrueOriginAI;

        float BodyRotation = 0;

        public float RPS = 0.1f;

        private int P1state = -2;

        private int oldP1state;

        private readonly int P1statecount = 6;

        private bool Draw = false;

        private bool useDR;
        private bool phaseProtectionDR;
        private bool DoAura;

        int flyTimer = 9000;

        private readonly List<int> intervalist = new(0);

        int P2Threshold => Main.expertMode ? (int)(NPC.lifeMax * 0.75) : 0;
        //int P3Threshold => WorldSavingSystem.EternityMode ? NPC.lifeMax / (WorldSavingSystem.MasochistModeReal ? 2 : 3) : 0;
        int SansThreshold => WorldSavingSystem.MasochistModeReal && UseTrueOriginAI ? NPC.lifeMax / 10 : 0;

        private List<int> chunklist = new(0);
        private readonly List<float> chunkrotlist = new(0);

        //float ChunkTriangleInnerRotation = 0;
        //float ChunkTriangleOuterRotation = 0;

        public float RuneDistance = 100;

        public float ChunkDistance = 10;

        public const float ChunkDistanceMax = 80;

        public float SpritePhase = 1;

        public int DrawTime = 0;

        public float PyramidWobble = 5;


        //NPC.AI
        public ref float AI_Timer => ref NPC.ai[1];

        #endregion
        #region Standard
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Lieflight");
            Main.npcFrameCount[NPC.type] = 8;
            NPCID.Sets.TrailCacheLength[NPC.type] = 18;
            NPCID.Sets.TrailingMode[NPC.type] = 1;
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.NoMultiplayerSmoothingByType[NPC.type] = true;

            NPCID.Sets.BossBestiaryPriority.Add(NPC.type);
            NPCID.Sets.DebuffImmunitySets.Add(NPC.type, new NPCDebuffImmunityData
            {
                SpecificallyImmuneTo = new int[]
                {
                    BuffID.Confused,
                    BuffID.Chilled,
                    BuffID.Suffocation,
                    ModContent.BuffType<LethargicBuff>(),
                    ModContent.BuffType<ClippedWingsBuff>()
                }
            });
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheHallow,
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.DayTime,
                new FlavorTextBestiaryInfoElement($"Mods.FargowiltasSouls.Bestiary.{Name}")
            });
        }
        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.lifeMax = 44000;
            NPC.defense = 0;
            NPC.damage = 70;
            NPC.knockBackResist = 0f;
            NPC.width = 200;
            NPC.height = 200;
            NPC.boss = true;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.NPCDeath7;

            Music = ModLoader.TryGetMod("FargowiltasMusic", out Mod musicMod)
                ? MusicLoader.GetMusicSlot(musicMod, "Assets/Music/LieflightNoCum") : MusicID.OtherworldlyBoss1;
            SceneEffectPriority = SceneEffectPriority.BossLow;

            NPC.value = Item.buyPrice(0, 15);

            NPC.dontTakeDamage = true; //until it Appears in Opening
        }

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * balance);
        }

        public override void OnSpawn(IEntitySource source)
        {
            //only enable this in maso
            if (WorldSavingSystem.MasochistModeReal)// && Main.player.Any(p => p.active && p.name.ToLower().Contains("cum")))
                UseTrueOriginAI = true;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write7BitEncodedInt(state);
            writer.Write7BitEncodedInt(oldstate);
            writer.Write7BitEncodedInt(index);
            writer.Write7BitEncodedInt(index2);
            writer.Write7BitEncodedInt(P1state);
            writer.Write7BitEncodedInt(oldP1state);
            writer.Write(UseTrueOriginAI);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            state = reader.Read7BitEncodedInt();
            oldstate = reader.Read7BitEncodedInt();
            index = reader.Read7BitEncodedInt();
            index2 = reader.Read7BitEncodedInt();
            P1state = reader.Read7BitEncodedInt();
            oldP1state = reader.Read7BitEncodedInt();
            UseTrueOriginAI = reader.ReadBoolean();
        }
        #endregion
        #region AI
        public override void AI()
        {

            //Defaults
            Player Player = Main.player[NPC.target];
            Main.time = 27000; //noon
            Main.dayTime = true;
            NPC.defense = NPC.defDefense;
            NPC.chaseable = true;
            useDR = false;
            phaseProtectionDR = false;
            if (WorldSavingSystem.MasochistModeReal)
            {
                DoAura = true;
            }

            if (PhaseOne && NPC.life < P2Threshold)
                phaseProtectionDR = true;
            /*
            if (!PhaseThree && NPC.life < P3Threshold)
                phaseProtectionDR = true;
            */
            if (UseTrueOriginAI && NPC.life < SansThreshold)
                phaseProtectionDR = true;

            //permanent regen for sans phase
            //deliberately done this way so that you can still eventually muscle past with endgame gear (this is ok)
            if (UseTrueOriginAI && NPC.life < SansThreshold * 0.5) //lowered so that sans phase check goes through properly
            {
                int healPerSecond = NPC.lifeMax / 10;
                NPC.life += healPerSecond / 60;
                CombatText.NewText(NPC.Hitbox, CombatText.HealLife, healPerSecond);
            }

            //rotation
            BodyRotation += RPS * MathHelper.TwoPi / 60f; //first number is rotations/second
            //ChunkTriangleOuterRotation -= 0.2f * MathHelper.TwoPi / 60f; //first number is rotations/second
            //ChunkTriangleInnerRotation += 0.1f * MathHelper.TwoPi / 60f; //first number is rotations/second
            //ChunkTriangleInnerRotation %= MathHelper.TwoPi;
            //ChunkTriangleOuterRotation %= MathHelper.TwoPi;

            if (P1state != -2) //do not check during spawn anim
            {
                //Aura
                if (DoAura)
                {
                    if (dustcounter > 5 && (DoAura && state == 1 || P1state == 4)) //do dust instead of runes during rune expand attack
                    {
                        for (int l = 0; l < 180; l++)
                        {
                            double rad2 = 2.0 * l * (MathHelper.Pi / 180.0);
                            double dustdist2 = 1200.0;
                            int DustX2 = (int)AuraCenter.X - (int)(Math.Cos(rad2) * dustdist2);
                            int DustY2 = (int)AuraCenter.Y - (int)(Math.Sin(rad2) * dustdist2);
                            int DustType = Main.rand.NextFromList(DustID.YellowTorch, DustID.PinkTorch, DustID.UltraBrightTorch);
                            int i = Dust.NewDust(new Vector2(DustX2, DustY2), 1, 1, DustType, Scale: 1.5f);
                            Main.dust[i].noGravity = true;
                        }
                        dustcounter = 0;
                    }
                    dustcounter++;

                    float distance = AuraCenter.Distance(Main.LocalPlayer.Center);
                    float threshold = 1200f;
                    Player player = Main.LocalPlayer;
                    if (player.active && !player.dead && !player.ghost) //pull into arena
                    {
                        if (distance > threshold && distance < threshold * 4f)
                        {
                            if (distance > threshold * 2f)
                            {
                                player.controlLeft = false;
                                player.controlRight = false;
                                player.controlUp = false;
                                player.controlDown = false;
                                player.controlUseItem = false;
                                player.controlUseTile = false;
                                player.controlJump = false;
                                player.controlHook = false;
                                if (player.grapCount > 0)
                                    player.RemoveAllGrapplingHooks();
                                if (player.mount.Active)
                                    player.mount.Dismount(player);
                                player.velocity.X = 0f;
                                player.velocity.Y = -0.4f;
                                player.GetModPlayer<FargoSoulsPlayer>().NoUsingItems = 2;
                            }

                            Vector2 movement = AuraCenter - player.Center;
                            float difference = movement.Length() - threshold;
                            movement.Normalize();
                            movement *= difference < 17f ? difference : 17f;
                            player.position += movement;

                            for (int i = 0; i < 10; i++)
                            {
                                int DustType = Main.rand.NextFromList(DustID.YellowTorch, DustID.PinkTorch, DustID.UltraBrightTorch);
                                int d = Dust.NewDust(player.position, player.width, player.height, DustType, 0f, 0f, 0, default, 1.25f);
                                Main.dust[d].noGravity = true;
                                Main.dust[d].velocity *= 5f;
                            }
                        }
                    }
                }
                AuraCenter = NPC.Center;

                //Targeting
                if (!Player.active || Player.dead || Player.ghost || NPC.Distance(Player.Center) > 2400)
                {
                    NPC.TargetClosest(false);
                    Player = Main.player[NPC.target];
                    if (!Player.active || Player.dead || Player.ghost || NPC.Distance(Player.Center) > 2400)
                    {
                        if (NPC.timeLeft > 60)
                            NPC.timeLeft = 60;
                        NPC.velocity.Y -= 0.4f;
                        return;
                    }
                }
                NPC.timeLeft = 60;
            }

            if (PhaseOne) //p1 just skip the rest of the ai and do its own ai lolll
            {
                P1AI();
                return;
            }

            if (Phase < 4.0)
            {
                NPC.dontTakeDamage = true;
            }
            else if (Phase >= 4.0)
            {
                NPC.dontTakeDamage = false;
                Attacking = 1;
            }


            if (Phase == 0.0)
            {
                NPC.TargetClosest(true);
                Phase = 0.5;
            }
            if (Phase < 4) //Initial Attack
            {
                Phase = 4.0; //REWORK: REMOVED P2 OPENING ATTACK
                NPC.netUpdate = true;
                //AttackP2Start();
            }

            //Normal looping attack AI
            if (Phase >= 4.0)
            {
                if (Flying) //Flying AI
                {
                    FlyingState();
                }

                if (Charging) //Charging AI (orientation)
                {
                    NPC.rotation = NPC.velocity.ToRotation() + MathHelper.Pi / 2;
                    if (NPC.velocity == Vector2.Zero)
                    {
                        NPC.rotation = 0f;
                    }
                }
                if (!Charging && !Flying) //standard upright orientation
                {
                    NPC.rotation = MathHelper.Lerp(NPC.rotation, 0, 0.09f);
                }
                if (Attacking == 1) //Phases and random attack choosing
                {
                    if (Phase == 4.0)
                    {
                        AI_Timer = 0f;
                        Phase = 5.0;
                        StateReset();
                    }
                    if (state == oldstate) //ensure you never get the same attack twice (might happen when the possible state list is refilled)
                    {
                        RandomizeState();

                        bool resetFly = true;
                        /*
                        if (!PhaseThree && NPC.life < P3Threshold)
                        {
                            state = 100;
                            resetFly = false;
                        }
                        */
                        if (!PhaseOne && NPC.life < SansThreshold)
                        {
                            state = 101;
                            oldstate = 0;
                            resetFly = false;
                        }



                        if (resetFly)
                            flyTimer = 0;
                    }

                    if (FlightCheck())
                    {
                        AI_Timer -= 1; //negate increment below
                    }
                    else if (state != oldstate)
                    {
                        switch (state) //Attack Choices
                        {
                            case 0: //slurp n burp attack
                                AttackSlurpBurp();
                                break;
                            case 1: //rune expand attack
                                AttackRuneExpand();
                                break;
                            case 2: //charge attack
                                AttackCharge();
                                break;
                            case 3: //above tp and down charge -> antigrav cum attack
                                AttackPlunge();
                                break;
                            case 4: //homing pixie attack
                                AttackPixies();
                                break;
                            case 5: // attack where he cuts you off (fires shots at angles from you) then fires a random assortment of projectiles in your direction (including nukes)
                                AttackRoulette();
                                break;
                            case 6: //charged reaction crosshair shotgun
                                AttackReactionShotgun();
                                break;
                            case 7: //running minigun
                                AttackRunningMinigun();
                                break;
                            case 8: //p3 shotgun run
                                AttackShotgun();
                                break;
                            case 9: //p3 teleport on you -> shit nukes
                                AttackTeleportNukes();
                                break;
                            case 101: // Life is a cage, and death is the key.
                                {
                                    AttackFinal();
                                    break;
                                }
                            default:
                                StateReset();
                                break;
                        }
                    }
                }
            }
            AI_Timer += 1f;
        }
        public void P1AI()
        {
            ref float P1Attacking = ref NPC.ai[0];
            ref float P1AI_Timer = ref NPC.ai[2];

            if (P1Attacking == 0f)
            {
                if (AI_Timer == 30f || P1state == -2)
                {
                    NPC.TargetClosest(true);
                }
                if (AI_Timer >= 60f || P1state == -2)
                {
                    AI_Timer = 0f;
                    P1Attacking = 1f;
                }
            }
            if (P1Attacking == 1f)
            {
                if (P1state == oldP1state && P1state != -2) //ensure you never get the same attack twice
                {
                    flyTimer = 0;
                    RandomizeP1state();
                }

                if (FlightCheck()) //negate increment below
                {
                    AI_Timer -= 1f;
                    P1AI_Timer -= 1f;
                }
                else if (P1state != oldP1state)
                {
                    switch (P1state)
                    {
                        case -2:
                            Opening();
                            break;
                        case -1:
                            P1Transition();
                            break;
                        case 0:
                            P1ShotSpam();
                            break;
                        case 1:
                            P1Nuke();
                            break;
                        case 2:
                            P1Mines();
                            break;
                        case 3:
                            P1Pixies();
                            break;
                        case 4:
                            AttackRuneExpand();
                            break;
                        case 5:
                            AttackReactionShotgun();
                            break;
                        default:
                            RandomizeP1state();
                            flyTimer = 9000;
                            break;
                    }

                    if (!Flying)
                        NPC.rotation = MathHelper.Lerp(NPC.rotation, 0, 0.09f);
                }
            }
            P1PeriodicNuke();

            AI_Timer += 1f;
            P1AI_Timer += 1f;
        }
        #endregion
        #region States
        #region P1
        public void Opening()
        {
            ref float P1AI_Timer = ref NPC.ai[2];

            if (!NPC.HasValidTarget)
                NPC.TargetClosest(false);

            Player Player = Main.player[NPC.target];
            NPC.position.X = Player.Center.X - NPC.width / 2;
            NPC.position.Y = Player.Center.Y - 490 - NPC.height / 2;
            NPC.alpha = (int)(255 - P1AI_Timer * 17);
            RPS = 0.1f;

            if (!Main.dedServ && UseTrueOriginAI && ModLoader.TryGetMod("FargowiltasMusic", out Mod musicMod)
                && musicMod.Version >= Version.Parse("0.1.1.5"))
            {
                Music = MusicLoader.GetMusicSlot(musicMod, "Assets/Music/Lieflight");
            }
            if (AI_Timer == 120)
            {
                if (UseTrueOriginAI)
                {
                    string text = Language.GetTextValue($"Mods.{Mod.Name}.Message.FatherOfLies");
                    Color color = Color.Goldenrod;
                    FargoSoulsUtil.PrintText(text, color);
                    CombatText.NewText(Player.Hitbox, color, text, true);
                }

                if (!Main.dedServ)
                    Main.LocalPlayer.GetModPlayer<FargoSoulsPlayer>().Screenshake = 60;

                if (WorldSavingSystem.EternityMode && !WorldSavingSystem.DownedBoss[(int)WorldSavingSystem.Downed.LifeChallenger] && Main.netMode != NetmodeID.MultiplayerClient)
                    Item.NewItem(NPC.GetSource_Loot(), Main.player[NPC.target].Hitbox, ModContent.ItemType<FragilePixieLamp>());

                SoundEngine.PlaySound(SoundID.ScaryScream, NPC.Center);
                SoundEngine.PlaySound(SoundID.Item62, NPC.Center);

                for (int i = 0; i < 150; i++)
                {
                    Vector2 vel = new Vector2(1, 0).RotatedByRandom(MathHelper.Pi * 2) * Main.rand.Next(20);
                    int DustType = Main.rand.NextFromList(DustID.YellowTorch, DustID.PinkTorch, DustID.UltraBrightTorch);
                    Dust.NewDust(NPC.Center, 0, 0, DustType, vel.X, vel.Y, 100, new Color(), 1f);
                }
                Draw = true;
                NPC.dontTakeDamage = false;
            }

            if (AI_Timer == 180)
            {
                P1state = -3;
                oldP1state = P1state;
                P1stateReset();
            }
        }
        public void P1ShotSpam()
        {
            ref float ShotTimer = ref NPC.localAI[1];
            ref float P1AI_Timer = ref NPC.ai[2];

            if (WorldSavingSystem.MasochistModeReal)
                NPC.velocity *= 0.95f;
            else
                FlyingState(0.15f);

            Player Player = Main.player[NPC.target];
            if (AttackF1)
            {
                AttackF1 = false;
                NPC.netUpdate = true;
                ShotTimer = 0;
                //Rampup = 1;
            }

            //if (P1AI_Timer >= (60 - (11 * Rampup)))
            //{
            //    SoundEngine.PlaySound(SoundID.Item12, NPC.Center);

            //    if (Main.netMode != NetmodeID.MultiplayerClient)
            //    {
            //        Vector2 shootatPlayer2 = NPC.DirectionTo(Player.Center) * Rampup * 3.2f;
            //        for (int i = -1; i < 2; i++)
            //        {
            //            int type = Rampup == 5 ? ModContent.ProjectileType<LifeSplittingProjSmall>() : ModContent.ProjectileType<LifeProjSmall>();
            //            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, shootatPlayer2.RotatedBy(i * MathHelper.Pi / 5), type, FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer);
            //        }
            //    }
            //    if (Rampup < 5)
            //    {
            //        Rampup++;
            //    }
            //    P1AI_Timer = 0f;
            //}
            //P1AI_Timer++;

            if (P1AI_Timer > 60)
            {
                int threshold = WorldSavingSystem.MasochistModeReal ? 5 : 6;
                if (++ShotTimer % threshold == 0)
                {
                    SoundEngine.PlaySound(SoundID.Item12, NPC.Center);

                    float finalSpreadOffset = MathHelper.Pi / (WorldSavingSystem.MasochistModeReal ? 8 : 5);
                    float startOffset = (MathHelper.Pi - finalSpreadOffset) * 0.9f;
                    const int timeToFocus = 60;

                    float rampRatio = (float)Math.Min(1f, ShotTimer / timeToFocus);
                    float rotationToUse = finalSpreadOffset + startOffset * (float)Math.Cos(MathHelper.PiOver2 * rampRatio);

                    Vector2 vel = NPC.DirectionTo(Player.Center);
                    vel *= 3f + 12f * rampRatio;

                    int projType = ShotTimer > timeToFocus ? ModContent.ProjectileType<LifeSplittingProjSmall>() : ModContent.ProjectileType<LifeProjSmall>();

                    for (int i = -1; i <= 1; i++)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, vel.RotatedBy(rotationToUse * i), projType, FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer);
                    }
                }
            }

            if (AI_Timer >= 300f)
            {
                oldP1state = P1state;
                P1stateReset();
            }
        }
        public void P1Nuke()
        {
            ref float P1AI_Timer = ref NPC.ai[2];

            if (WorldSavingSystem.MasochistModeReal)
                NPC.velocity *= 0.95f;
            else
                FlyingState(0.5f);

            Player Player = Main.player[NPC.target];
            if (AttackF1)
            {
                AttackF1 = false;
                NPC.netUpdate = true;
            }
            if (AI_Timer == 70f)
            {
                SoundEngine.PlaySound(SoundID.Item91, NPC.Center);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    float ProjectileSpeed3 = 12f;
                    Vector2 shootatPlayer3 = NPC.DirectionTo(Player.Center) * ProjectileSpeed3;
                    float ai1 = WorldSavingSystem.EternityMode ? 1 : 0;
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, shootatPlayer3, ModContent.ProjectileType<LifeNuke>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage, 1.5f), 300f, Main.myPlayer, 32f, ai1);
                }
            }
            if (AI_Timer >= 145f)
            {
                oldP1state = P1state;
                P1stateReset();
            }
        }
        public void P1Pixies()
        {
            ref float P1AI_Timer = ref NPC.ai[2];
            ref float RandomRotation = ref NPC.localAI[1];

            if (WorldSavingSystem.MasochistModeReal)
                NPC.velocity *= 0.95f;
            else
                FlyingState(0.2f);

            if (AttackF1)
            {
                AttackF1 = false;
                NPC.netUpdate = true;
                RandomRotation = 0;
            }
            if (P1AI_Timer > 60f && (NPC.ai[2] % 5) == 0 && AI_Timer < 280)
            {
                SoundEngine.PlaySound(SoundID.Item25, NPC.Center);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    float knockBack10 = 3f;
                    Vector2 shootoffset4 = NPC.DirectionTo(Main.player[NPC.target].position).RotatedBy(RandomRotation) * -4f;
                    RandomRotation = (float)(Main.rand.Next(-20, 20) * (MathHelper.Pi / 180.0f)); //change random offset after so game has time to sync
                    float ai0 = 0;
                    if (!WorldSavingSystem.MasochistModeReal)
                    {
                        ai0 = -10;
                    }
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, shootoffset4, ModContent.ProjectileType<LifeHomingProj>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), knockBack10, Main.myPlayer, ai0, NPC.whoAmI);
                }
            }
            P1AI_Timer += 1f;
            int endtime = WorldSavingSystem.MasochistModeReal ? 160 : 200;
            if (AI_Timer >= endtime)
            {
                oldP1state = P1state;
                P1stateReset();
            }
            
        }
        public void P1Mines()
        {
            ref float P1AI_Timer = ref NPC.ai[2];

            if (WorldSavingSystem.MasochistModeReal)
                NPC.velocity *= 0.95f;
            else
                FlyingState(0.5f);

            Player Player = Main.player[NPC.target];

            if (AttackF1)
            {
                AttackF1 = false;
                NPC.netUpdate = true;

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    foreach (Projectile p in Main.projectile)
                    {
                        if (p.type == ModContent.ProjectileType<LifeBombExplosion>())
                        {
                            //make them fade
                            p.ai[0] = Math.Max(p.ai[0], 2400 - 30);
                            p.netUpdate = true;
                        }
                    }
                }
            }

            if (AI_Timer > 0 && AI_Timer % 70f == 0)
            {
                SoundEngine.PlaySound(SoundID.Item92, NPC.Center);
                int max = 14;// Main.expertMode ? 14 : 10;
                for (int i = 0; i < max; i++)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        float bigSpeed = Main.rand.NextFloat(25, 172); //172 goes to edge of arena
                        int maxDegreeRand = 40;// Main.expertMode ? 60 : 40;
                        double rotationrad = MathHelper.ToRadians(Main.rand.NextFloat(-maxDegreeRand, maxDegreeRand));
                        Vector2 shootrandom = (NPC.DirectionTo(Player.Center) * (bigSpeed / 6f)).RotatedBy(rotationrad);
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, shootrandom, ModContent.ProjectileType<LifeBomb>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer);
                    }
                }
                NPC.netUpdate = true;
            }
            if (AI_Timer >= 70f * 3.5f)
            {
                oldP1state = P1state;
                P1stateReset();
            }
        }
        public void P1PeriodicNuke()
        {
            ref float PeriodicNukeTimer = ref NPC.ai[3];

            Player Player = Main.player[NPC.target];
            if (PeriodicNukeTimer > 600f)
            {
                SoundEngine.PlaySound(SoundID.Item91, NPC.Center);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    float ProjectileSpeed = 8f;
                    float knockBack = 300f;
                    Vector2 shootatPlayer = NPC.DirectionTo(Player.Center) * ProjectileSpeed;
                    float ai1 = WorldSavingSystem.EternityMode ? 1 : 0;
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero - shootatPlayer, ModContent.ProjectileType<LifeNuke>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage, 1.5f), knockBack, Main.myPlayer, 32f, ai1);
                    PeriodicNukeTimer = 0f;
                }
                NPC.netUpdate = true;
            }
            PeriodicNukeTimer += 1f;
        }
        public void P1Transition()
        {
            ref float P1AI_Timer = ref NPC.ai[2];
            ref float SubAttack = ref NPC.localAI[1];
            

            Charging = false;
            Flying = false;
            useDR = true;
            DoAura = true;
            NPC.velocity *= 0.95f;

            void PhaseTransition()
            {
                if (RPS < 0.2f) //speed up rotation up
                {
                    RPS += 0.1f / 100;
                }
                else
                {
                    RPS = 0.2f;
                }

                //if (WorldSavingSystem.MasochistModeReal)
                //{
                //    int heal = (int)(NPC.lifeMax / 100f * Main.rand.NextFloat(1f, 1.5f));
                //    NPC.life += heal;
                //    if (NPC.life > NPC.lifeMax)
                //        NPC.life = NPC.lifeMax;
                //    CombatText.NewText(NPC.Hitbox, CombatText.HealLife, heal);
                //}
                if (AI_Timer < 60)
                {
                    if (AI_Timer % 5 == 0)
                        SoundEngine.PlaySound(SoundID.Tink, Main.LocalPlayer.Center);

                    if (PyramidWobble > 0)
                        PyramidWobble -= 0.2f;

                    int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Gold, Scale: 1.5f);
                    Main.dust[dust].velocity = (Main.dust[dust].position - NPC.Center) / 10;
                }
                if (AI_Timer == 60f)
                {
                    SoundEngine.PlaySound(SoundID.Item62, Main.LocalPlayer.Center);
                    for (int i = 0; i < 60; i++)
                    {
                        int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Gold, Scale: 1.5f);
                        Main.dust[dust].velocity = (Main.dust[dust].position - NPC.Center) / 5;
                    }
                }
                if (AI_Timer >= 60f)
                {
                    SpritePhase = 2;
                    if (ChunkDistance < ChunkDistanceMax)
                        ChunkDistance++;

                }

                if (AI_Timer == 180f)
                {
                    //mine explosion
                    const int MineAmount = 100;
                    for (int i = 0; i < MineAmount; i++)
                    {
                        float rotation = (i / 64f) * MathHelper.TwoPi;
                        float distance = Main.rand.Next(1200);
                        Vector2 pos = NPC.Center + (rotation.ToRotationVector2() * distance);
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<LifeTransitionBomb>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer, 0, pos.X, pos.Y);
                    }
                    SoundEngine.PlaySound(SoundID.Item82, Main.LocalPlayer.Center);

                    if (!Main.dedServ)
                        Main.LocalPlayer.GetModPlayer<FargoSoulsPlayer>().Screenshake = 60;

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        foreach (Projectile p in Main.projectile)
                        {
                            if (p.type == ModContent.ProjectileType<LifeBombExplosion>())
                            {
                                //make them fade
                                p.ai[0] = Math.Max(p.ai[0], 2400 - 30);
                                p.netUpdate = true;
                            }
                        }
                    }

                    
                }
                if (AI_Timer == 240f)
                {
                    LockVector1 = -NPC.DirectionTo(Main.player[NPC.target].Center);
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<BloomLine>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, 1, LockVector1.ToRotation());
                }
            }
            
            void LaserSpin()
            {
                ref float RandomDistance = ref NPC.ai[0];
                ref float LaserTimer = ref NPC.localAI[2];
                //ref float RotationDirection = ref NPC.localAI[0];

                Player Player = Main.player[NPC.target];
                NPC.velocity *= 0.9f;

                NPC.dontTakeDamage = true;
                if (AttackF1)
                {
                    AttackF1 = false;
                    
                    //SoundEngine.PlaySound(SoundID.Zombie104 with { Volume = 0.5f }, NPC.Center);
                    SoundEngine.PlaySound(SoundID.Zombie104 with { Volume = 0.5f}, NPC.Center);
                    NPC.velocity.X = 0;
                    NPC.velocity.Y = 0;
                    Flying = false;
                    
                    //RotationDirection = Main.rand.NextBool() ? 1 : -1;
                    NPC.netUpdate = true;
                    rotspeed = 0;
                    rot = 0;
                }

                //for a starting time, make it fade in, then make it spin faster and faster up to a max speed
                int fadeintime = 10;
                if (Main.netMode != NetmodeID.MultiplayerClient && LaserTimer < fadeintime)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, LockVector1,
                                    ModContent.ProjectileType<LifeChalDeathray>(), 0, 0f, Main.myPlayer, 0, NPC.whoAmI);
                }

                int endTime = 850;

                if (Main.netMode != NetmodeID.MultiplayerClient && LaserTimer >= fadeintime)
                {
                    if (rotspeed < 0.82f)
                    {
                        rotspeed += 2f / 60 / 4;
                    }
                    else
                    {
                        rotspeed = 0.82f;
                    }
                    rot += MathHelper.Pi / 180 * rotspeed;
                    Vector2 rotV = LockVector1.RotatedBy(rot);
                    int rayDamage = FargoSoulsUtil.ScaledProjectileDamage(NPC.damage, WorldSavingSystem.MasochistModeReal ? 4f : 1.5f);
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, rotV,
                        ModContent.ProjectileType<LifeChalDeathray>(), rayDamage, 0f, Main.myPlayer, 0, NPC.whoAmI);
                    //randomly make Scar obstacles at specific points, obstacles have Projectile.ai[1] = LaserTimer
                    /*
                    if (LaserTimer % 8 == 0 && Main.netMode != NetmodeID.MultiplayerClient && rotspeed > 0.82f)
                    {
                        if (intervalist.Count < 1)
                        {
                            intervalist.Clear();
                            for (int i = 0; i < 6; i++)
                            {
                                intervalist.Add(i);
                            }
                        }
                        index2 = Main.rand.Next(intervalist.Count);
                        NPC.netUpdate = true;
                        interval = intervalist[index2];
                        intervalist.RemoveAt(index2);


                        RandomDistance = Main.rand.Next(200);
                        int dist = 200 * interval + (int)RandomDistance;

                        Vector2 distV = NPC.Center - new Vector2(0f, dist).RotatedBy(rot);
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), distV, Vector2.Zero, ModContent.ProjectileType<LifeScar>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer, AI_Timer, endTime);

                        NPC.netUpdate = true;
                    }
                    */
                    
                }

                LaserTimer++;
                if (LaserTimer > endTime)
                {
                    foreach (Projectile p in Main.projectile)
                    {
                        if (p.type == ModContent.ProjectileType<LifeBombExplosion>())
                        {
                            //make them fade
                            p.ai[0] = Math.Max(p.ai[0], 2400 - 30);
                            p.netUpdate = true;
                        }
                    }

                    DoAura = WorldSavingSystem.MasochistModeReal;
                    PhaseOne = false;
                    NPC.netUpdate = true;
                    NPC.TargetClosest(true);
                    NPC.netUpdate = true;
                    AttackF1 = true;
                    AI_Timer = 0f;
                    NPC.ai[2] = 0f;
                    NPC.ai[3] = 0f;
                    NPC.ai[0] = 0f;
                    StateReset();
                }
            }
            
            
            

            PhaseTransition();
            //ExpandRunes();
            if (AI_Timer > 280)
            {
                LaserSpin();
            }
            
        }

        #endregion
        #region P2
        public void FlyingState(float speedModifier = 1f)
        {
            ref float speedVar = ref NPC.localAI[3];
            Flying = true;

            //basically, create a smooth transition when using different speedMod values
            float accel = 0.5f / 30f;
            if (speedVar < speedModifier)
            {
                speedVar += accel;
                if (speedVar > speedModifier)
                    speedVar = speedModifier;
            }
            if (speedVar > speedModifier)
            {
                speedVar -= accel;
                if (speedVar < speedModifier)
                    speedVar = speedModifier;
            }
            speedModifier = speedVar;

            Player Player = Main.player[NPC.target];
            //flight AI
            float flySpeed = 0f;
            float inertia = 10f;
            Vector2 AbovePlayer = new(Player.Center.X, Player.Center.Y - 300f);
            if (state == 8)
            {
                AbovePlayer.Y = Player.Center.Y - 700f;
            }
            bool Close = Math.Abs(AbovePlayer.Y - NPC.Center.Y) < 32f && Math.Abs(AbovePlayer.X - NPC.Center.X) < 160f;
            if (!Close && NPC.Distance(AbovePlayer) < 500f)
            {
                flySpeed = 9f;
                if (!flyfast)
                {
                    Vector2 flyabovePlayer3 = NPC.DirectionTo(AbovePlayer) * flySpeed;
                    NPC.velocity = (NPC.velocity * (inertia - 1f) + flyabovePlayer3) / inertia;
                }
            }
            if (NPC.velocity == Vector2.Zero)
            {
                NPC.velocity = NPC.DirectionTo(AbovePlayer) * 1f;
            }
            if (NPC.Distance(AbovePlayer) > 360f)
            {
                flySpeed = NPC.Distance(AbovePlayer) / 35f;
                flyfast = true;
                Vector2 flyabovePlayer2 = NPC.DirectionTo(AbovePlayer) * flySpeed;
                NPC.velocity = (NPC.velocity * (inertia - 1f) + flyabovePlayer2) / inertia;
            }
            if (flyfast && (NPC.Distance(AbovePlayer) < 100f || NPC.Distance(Player.Center) < 100f))
            {
                flyfast = false;
                Vector2 flyabovePlayer = NPC.DirectionTo(AbovePlayer) * flySpeed;
                NPC.velocity = flyabovePlayer;
            }

            //orientation
            if (NPC.velocity.ToRotation() > MathHelper.Pi)
            {
                NPC.rotation = 0f - MathHelper.Pi * NPC.velocity.X * speedModifier / 100;
            }
            else
            {
                NPC.rotation = 0f + MathHelper.Pi * NPC.velocity.X * speedModifier / 100;
            }

            NPC.position -= NPC.velocity * (1f - speedModifier);
        }
        public void AttackFinal()
        {
            ref float RandomFloat = ref NPC.ai[0];

            Player Player = Main.player[NPC.target];

            if (UseTrueOriginAI) //disable items
            {
                NPC.chaseable = false;

                if (Main.LocalPlayer.active && !Main.LocalPlayer.dead && !Main.LocalPlayer.ghost && NPC.Distance(Main.LocalPlayer.Center) < 3000)
                {
                    if (Main.LocalPlayer.grapCount > 0)
                        Main.LocalPlayer.RemoveAllGrapplingHooks();

                    //Main.LocalPlayer.controlUseItem = false;
                    //Main.LocalPlayer.controlUseTile = false;
                    //Main.LocalPlayer.GetModPlayer<FargoSoulsPlayer>().NoUsingItems = true;
                }
            }

            if (AttackF1)
            {
                AttackF1 = false;
                NPC.dontTakeDamage = true;
                NPC.netUpdate = true;
                Flying = true;
            }

            //for (int i = 0; i < Main.musicFade.Length; i++) //shut up music
            //    if (Main.musicFade[i] > 0f)
            //        Main.musicFade[i] -= 1f / 60;
            const int InitTime = 120;

            if (AI_Timer == 0 && Main.netMode != NetmodeID.MultiplayerClient) // cage size is 600x600, 300 from center, 25 projectiles per side, 24x24 each
            {
                Projectile.NewProjectile(NPC.GetSource_FromThis(), Player.Center, Vector2.Zero, ModContent.ProjectileType<LifeCageTelegraph>(), 0, 0f, Main.myPlayer, ai1: Player.whoAmI);
            }
            if (AI_Timer == InitTime)
            {
                SoundEngine.PlaySound(SoundID.DD2_DefenseTowerSpawn, Player.Center);
                for (int i = 0; i < 26; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), new Vector2(Player.Center.X - 300 + 600 * j, Player.Center.Y - 300 + 24 * i), Vector2.Zero, ModContent.ProjectileType<LifeCageProjectile>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer, j);
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), new Vector2(Player.Center.X - 300 + 24 * i, Player.Center.Y - 300 + 600 * j), Vector2.Zero, ModContent.ProjectileType<LifeCageProjectile>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer, 2 + j);
                        }
                    }
                }
                /*if (Main.netMode != NetmodeID.MultiplayerClient) //bars
                {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), Player.Center, Vector2.Zero, ModContent.ProjectileType<LifeCageBars>(), 0, 0, Main.myPlayer);
                }*/
                LockVector1 = Player.Center;
                NPC.netUpdate = true;
            }

            if (AI_Timer > InitTime) //make sure to teleport any player outside the cage inside
            {
                if (Main.LocalPlayer.active && (Math.Abs(Main.LocalPlayer.Center.X - LockVector1.X) > 320 || Math.Abs(Main.LocalPlayer.Center.Y - LockVector1.Y) > 320) && Main.LocalPlayer.active && (Math.Abs(Main.LocalPlayer.Center.X - LockVector1.X) < 1500 || Math.Abs(Main.LocalPlayer.Center.Y - LockVector1.Y) < 1500))
                {
                    Main.LocalPlayer.position = LockVector1;
                }
            }
            #region GridShots (removed)
            const int Attack1Start = InitTime + 40;
            const int Attack1End = Attack1Start;
            #endregion
            #region BulletHell
            //start of shooting attack: cum god fires a nuke or two straight up while he shoots slow shots straight down from him
            const int Attack2Time = 25;
            const int Attack2Start = Attack1End + 60;
            const int Attack2End = Attack2Start + 60 * 8;
            int time2 = (int)AI_Timer - Attack2Start;

            if (AI_Timer > Attack2Start && time2 % (Attack2Time * 3) + 1 == 1 && AI_Timer < Attack2End) //cum nuke up
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    for (int i = 0; i < 2; i++)
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(-4 + 8 * i, -2f), ModContent.ProjectileType<LifeNuke>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage, 1.5f), 3f, Main.myPlayer, 24f);
            }
            if (AI_Timer > Attack2Start && time2 % (Attack2Time * 2) + 1 == 1 && AI_Timer < Attack2End) //fire shots down
            {
                SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, NPC.Center);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(0, 2.5f), ModContent.ProjectileType<LifeProjLarge>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer);
            }
            #endregion

            #region Blaster1
            //GASTER BLASTER 1
            const int Attack3Time = 90;
            const int Attack3Start = Attack2End + 60;
            const int Attack3End = Attack3Start + Attack3Time * 8;
            int time5 = (int)AI_Timer - Attack3Start;
            if (AI_Timer >= Attack3Start && time5 % Attack3Time + 1 == 1 && AI_Timer < Attack3End) // get random angle
            {
                RandomFloat = Main.rand.Next(-90, 90);
                NPC.netUpdate = true;
            }
            if (AI_Timer >= Attack3Start && time5 % Attack3Time + 1 == Attack3Time && AI_Timer < Attack3End) // spawn blasters
            {
                Vector2 aim = new(0, 450);
                if (firstblaster < 1 || firstblaster > 1)
                    SoundEngine.PlaySound(SoundID.Item92, NPC.Center);
                for (int i = 0; i <= 12; i++)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient && (firstblaster < 1 || firstblaster > 1))
                    {
                        Vector2 vel = -Vector2.Normalize(aim).RotatedBy(i * MathHelper.Pi / 6 + MathHelper.ToRadians(NPC.ai[0]));
                        float ai0 = vel.ToRotation();
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), LockVector1 + aim.RotatedBy(i * MathHelper.Pi / 6 + MathHelper.ToRadians(NPC.ai[0])), Vector2.Zero, ModContent.ProjectileType<LifeBlaster>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer, ai0, firstblaster);
                    }
                }
                if (firstblaster > 0)
                    firstblaster -= 1;
                NPC.netUpdate = true;

            }
            #endregion
            #region Blaster2
            //GASTER BLASTER 2 FINAL BIG SPIN FINAL CUM GOD DONE DUN DEAL
            const int Attack4Time = 4;
            const int Attack4Start = Attack3End + 90;
            const int Attack4End = Attack4Start + 180 * 5; //2 seconds per rotation
            int time6 = (int)AI_Timer - Attack4Start;
            if (AI_Timer >= Attack4Start && time6 == 0) // reset NPC.ai[0]
            {
                RandomFloat = 0;
                NPC.netUpdate = true;
                LockVector2 = Player.Center;
            }

            if (AI_Timer > Attack4Start && time5 % Attack4Time == Attack4Time - 1 && AI_Timer < Attack4End) // spawn blasters. 1 every 4th frame, 2 seconds per rotation, 45 total
            {
                SoundEngine.PlaySound(SoundID.Item92, NPC.Center);
                Vector2 aim = (Vector2.Normalize(LockVector2 - LockVector1) * 550).RotatedBy(MathHelper.PiOver2);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 vel = -Vector2.Normalize(aim).RotatedBy(RandomFloat * MathHelper.Pi / 18);
                    float ai0 = vel.ToRotation();
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), LockVector1 + aim.RotatedBy(RandomFloat * MathHelper.Pi / 18), Vector2.Zero, ModContent.ProjectileType<LifeBlaster>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer, ai0);
                }
                NPC.netUpdate = true;
                RandomFloat += 1;
            }
            #endregion
            #region End
            int end = Attack4End + 120;
            if (AI_Timer >= end)
            {

                UseTrueOriginAI = false;
                NPC.dontTakeDamage = false;
                SoundEngine.PlaySound(SoundID.ScaryScream, NPC.Center);
            }
            if (AI_Timer >= end && AI_Timer % 10 == 0)
            {
                for (int i = 0; i < 50; i++)
                {
                    int DustType = Main.rand.NextFromList(DustID.YellowTorch, DustID.PinkTorch, DustID.UltraBrightTorch);
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustType);
                }

                NPC.position = NPC.position + new Vector2(Main.rand.Next(-60, 60), Main.rand.Next(-60, 60));
                NPC.netUpdate = true;
                SoundEngine.PlaySound(SoundID.Item84, NPC.Center);
            }
            if (AI_Timer == end + 90)
            {
                NPC.life = 0;
                NPC.checkDead();
                //there was dialogue here before
            }
            #endregion
        }
        public void AttackSlurpBurp()
        {
            ref float SlurpTimer = ref NPC.ai[2];
            ref float BurpTimer = ref NPC.ai[3];

            Player Player = Main.player[NPC.target];
            if (AttackF1)
            {
                //only do attack when in range
                //this had bugs and is currently disabled, may be changed in the future
                Vector2 targetPos = Player.Center;
                targetPos.Y -= 16 * 15;
                //if (NPC.Distance(targetPos) < 18 * 10 || WorldSavingSystem.MasochistModeReal)

                AttackF1 = false;
                NPC.netUpdate = true;
                /*
                if (true)
                {
                    AttackF1 = false;
                    NPC.netUpdate = true;
                }
                else
                {
                    FlyingState();
                    NPC.velocity = NPC.DirectionTo(targetPos) * NPC.velocity.Length();
                    AI_Timer = 0; //negate the usual increment
                    NPC.ai[2] = 0;
                    NPC.ai[3] = 0;
                    return;
                }
                */
            }

            if (NPC.Distance(Player.Center) > 2000)
            {
                FlyingState(1.5f);
            }

            NPC.velocity = Vector2.Zero;
            Flying = false;

            float knockBack = 3f;
            double rad = AI_Timer * 5.721237 * (MathHelper.Pi / 180.0);

            double dustdist = 1200;
            if (!WorldSavingSystem.MasochistModeReal)
            {
                float distanceToPlayer = NPC.Distance(Player.Center);
                distanceToPlayer += 240;
                dustdist = Math.Max(dustdist, distanceToPlayer); //take higher of these values
                dustdist = Math.Min(dustdist, 2400); //capped at this value
            }

            int DustX = (int)NPC.Center.X - (int)(Math.Cos(rad) * dustdist);
            int DustY = (int)NPC.Center.Y - (int)(Math.Sin(rad) * dustdist);
            Vector2 DustV = new(DustX, DustY);
            if (SlurpTimer >= 2f && AI_Timer <= 300f)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), DustV, Vector2.Zero, ModContent.ProjectileType<LifeSlurp>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), knockBack, Main.myPlayer, 0, NPC.whoAmI);
                }

                SlurpTimer = 0f;
            }
            SlurpTimer += 1f;
            if (AI_Timer < 300f)
            {
                if (BurpTimer > 15f)
                {
                    SoundEngine.PlaySound(SoundID.Item101, DustV);

                    if (WorldSavingSystem.MasochistModeReal && shoot != false) //extra projectiles in maso
                    {
                        SoundEngine.PlaySound(SoundID.Item12, NPC.Center);

                        float ProjectileSpeed = 10f;
                        float knockBack2 = 3f;
                        Vector2 shootatPlayer = NPC.DirectionTo(Player.Center) * ProjectileSpeed;
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, shootatPlayer, ModContent.ProjectileType<LifeWave>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), knockBack2, Main.myPlayer);

                        //for (int i = -2; i <= 2; i++)
                        //{
                        //    if (Main.netMode != NetmodeID.MultiplayerClient)
                        //        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, 0.9f * NPC.DirectionTo(Player.Center).RotatedBy(MathHelper.ToRadians(3) * i), ModContent.ProjectileType<LifeSplittingProjSmall>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, -60, 2f);
                        //}

                        shoot = false;
                    }
                    else
                    {
                        shoot = true;
                    }
                    BurpTimer = 0f;
                }
                BurpTimer += 1f;
            }

            if (AI_Timer > 300f && AI_Timer < 600f)
            {
                if (BurpTimer > 15f)
                {
                    SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, NPC.Center);
                    BurpTimer = 0f;
                }
                BurpTimer += 1f;
            }

            if (!WorldSavingSystem.MasochistModeReal && AI_Timer < 120)
            {
                SlurpTimer -= 0.5f;
                BurpTimer -= 0.5f;
            }

            if (AI_Timer >= 660f)
            {
                oldstate = state;
                Flying = true;
                StateReset();
            }
        }

        public void AttackShotgun()
        {
            ref float ShotCount = ref NPC.ai[3];
            ref float ShotTimer = ref NPC.ai[2];

            int StartTime = (WorldSavingSystem.MasochistModeReal ? 80 : WorldSavingSystem.EternityMode ? 95 : 105);
            int AttackTime = (WorldSavingSystem.MasochistModeReal ? 40 : WorldSavingSystem.EternityMode ? 50 : 55);
            Player Player = Main.player[NPC.target];

            if (AttackF1)
            {
                AttackF1 = false;
                NPC.netUpdate = true;
                Flying = true;
                SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
            }
            Flying = false;
            float flySpeed2 = 7f;
            float inertia2 = 7f;
            Vector2 flyonPlayer = NPC.DirectionTo(Player.Center) * flySpeed2;
            NPC.velocity = (NPC.velocity * (inertia2 - 1f) + flyonPlayer) / inertia2;

            //rotation
            if (NPC.velocity.ToRotation() > MathHelper.Pi)
            {
                NPC.rotation = 0f - MathHelper.Pi * NPC.velocity.X / 100;
            }
            else
            {
                NPC.rotation = 0f + MathHelper.Pi * NPC.velocity.X / 100;
            }
            if (ShotCount < 3f)
            {
                ShotCount = 3f;
            }
            if (ShotTimer >= StartTime)
            {
                SoundEngine.PlaySound(SoundID.Item12, NPC.Center);
                float ProjectileSpeed = 10f;
                float knockBack2 = 3f;
                Vector2 shootatPlayer = NPC.DirectionTo(Player.Center) * ProjectileSpeed;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int spread = 10;
                    for (int i = 0; i <= ShotCount; i++)
                    {
                        double rotationrad = MathHelper.ToRadians(0f - ShotCount * spread / 2 + i * spread);
                        Vector2 shootoffset = shootatPlayer.RotatedBy(rotationrad);
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, shootoffset, ModContent.ProjectileType<LifeWave>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), knockBack2, Main.myPlayer);
                    }
                }
                ShotCount += 1f;
                ShotTimer = StartTime - AttackTime;
            }

            /*
            //old p2 variant, unused rn
            {
                if (NPC.ai[3] < 3f)
                {
                    NPC.ai[3] = 3f;
                }
                float ProjectileSpeed = 30f;
                float knockBack2 = 3f;
                int spread = 18 - (int)(NPC.ai[3] - 3); //gets tighter after each shot
                if (NPC.ai[2] == 41f)
                {
                    LockVector1 = NPC.Center;
                    LockVector2 = (NPC.DirectionTo(Player.Center) * ProjectileSpeed).RotatedBy(MathHelper.Pi / 80 * (Main.rand.NextFloat() - 0.5f));
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int i = 0; (float)i <= NPC.ai[3]; i++)
                        {
                            double rotationrad = MathHelper.ToRadians(0f - NPC.ai[3] * spread / 2 + (float)(i * spread));
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), LockVector1, Vector2.Zero, ModContent.ProjectileType<BloomLine>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, 0, LockVector2.RotatedBy(rotationrad).ToRotation());
                        }
                    }
                    NPC.netUpdate = true;
                }
                if (NPC.ai[2] >= 80f)
                {
                    SoundEngine.PlaySound(SoundID.Item12, NPC.Center);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int i = 0; (float)i <= NPC.ai[3]; i++)
                        {
                            double rotationrad = MathHelper.ToRadians(0f - NPC.ai[3] * spread / 2 + (float)(i * spread));
                            Vector2 shootoffset = LockVector2.RotatedBy(rotationrad);
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), LockVector1, shootoffset, ModContent.ProjectileType<LifeWave>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), knockBack2, Main.myPlayer);
                        }
                    }
                    NPC.ai[3] += 1f;
                    NPC.ai[2] = 40f;
                }
            }
            */
            ShotTimer += 1f;
            if (ShotCount >= 12f)
            {
                Flying = true;
                oldstate = state;
                StateReset();
            }
        }
        public void AttackCharge()
        {
            ref float TeleportX = ref NPC.localAI[0];
            ref float TeleportY = ref NPC.localAI[1];
            ref float TeleportAngle = ref NPC.ai[3];
            ref float AttackCount = ref NPC.ai[2];

            Player Player = Main.player[NPC.target];
            int StartTime;
            if (Variant)
            {
                StartTime = (WorldSavingSystem.MasochistModeReal ? 80 : WorldSavingSystem.EternityMode ? 90 : 100);
            }
            else
            {
                StartTime = (WorldSavingSystem.MasochistModeReal ? 60 : WorldSavingSystem.EternityMode ? 70 : 80);
            }

            if (AttackF1)
            {
                LockVector3 = NPC.Center;
                AttackF1 = false;
                NPC.netUpdate = true;
                SoundEngine.PlaySound(SoundID.ScaryScream, NPC.Center);
            }
            Flying = false;
            Charging = true;
            HitPlayer = true;
            AuraCenter = LockVector3; //lock arena in place during charges
            if (Variant) //tp
            {
                if (AI_Timer == 0f)
                {
                    TeleportAngle = Main.rand.Next(360);
                    if (AttackCount >= 6f)
                    {
                        TeleportAngle = 90f;
                    }
                }
                double rad3 = TeleportAngle * (MathHelper.Pi / 180.0);
                double tpdist = 350.0;
                int TpX = (int)Player.Center.X - (int)(Math.Cos(rad3) * tpdist);
                int TpY = (int)Player.Center.Y - (int)(Math.Sin(rad3) * tpdist);
                Vector2 TpPos = new(TpX, TpY);

                TeleportX = TpPos.X; //exposing these so proj can access them
                TeleportY = TpPos.Y;

                if (AI_Timer == 1f && Main.netMode != NetmodeID.MultiplayerClient) //telegraph
                {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), new Vector2(TpPos.X + NPC.width / 2, TpPos.Y + NPC.height / 2), Vector2.Zero, ModContent.ProjectileType<LifeTpTelegraph>(), 0, 0f, Main.myPlayer, -70);
                }
                if (AI_Timer == StartTime - 5f) //tp
                {
                    NPC.Center = new Vector2(TpX, TpY);
                    NPC.velocity.X = 0f;
                    NPC.velocity.Y = 0f;
                    NPC.rotation = NPC.DirectionTo(Player.Center).ToRotation();
                    SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
                    NPC.netUpdate = true;
                }
            }
            if (AI_Timer == StartTime && Main.netMode != NetmodeID.MultiplayerClient && AttackCount < 6f)
            {
                SoundEngine.PlaySound(SoundID.ForceRoarPitched, NPC.Center);
                SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, NPC.Center);
                //circle of cum before charge
                float ProjectileSpeed = 10f;
                Vector2 shootatPlayer = NPC.DirectionTo(Player.Center) * ProjectileSpeed;
                int amount = 14;
                for (int i = 0; i < amount; i++)
                {
                    Vector2 shootoffset = shootatPlayer.RotatedBy(i * (MathHelper.Pi / (amount / 2)));
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, shootoffset, ModContent.ProjectileType<LifeProjLarge>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer);
                }
                //charge
                float chargeSpeed = 22f;
                Vector2 chargeatPlayer = NPC.DirectionTo(Player.Center) * chargeSpeed;
                NPC.velocity = chargeatPlayer;
                TeleportAngle = Main.rand.Next(360);
                NPC.netUpdate = true;
                AI_Timer = 0;
                AttackCount += 1f;
            }
            if (!Variant)
            {
                NPC.velocity = NPC.velocity * 0.99f;
            }
            if ((AttackCount >= 6f && AI_Timer >= StartTime + 15f && !Variant) || (AttackCount >= 6f && AI_Timer >= StartTime + 25f && Variant))
            {
                NPC.velocity.X = 0f;
                NPC.velocity.Y = 0f;
                HitPlayer = false;
                Flying = true;
                Charging = false;
                oldstate = state;
                StateReset();
            }
        }
        public void AttackPlunge()
        {
            ref float TeleportX = ref NPC.localAI[0];
            ref float TeleportY = ref NPC.localAI[1];
            ref float PlungeCount = ref NPC.ai[2];

            Player Player = Main.player[NPC.target];
            int StartTime = (WorldSavingSystem.MasochistModeReal ? 60 : WorldSavingSystem.EternityMode ? 70 : 80);

            if (AttackF1)
            {
                LockVector3 = NPC.Center;
                AttackF1 = false;
                NPC.netUpdate = true;
                Flying = true;
            }
            AuraCenter = LockVector3;

            Vector2 TpPos = new(Player.Center.X, Player.Center.Y - 400f);

            TeleportX = TpPos.X; //exposing so proj can access
            TeleportY = TpPos.Y;

            if (AI_Timer == 1)
            {
                LockVector2 = new Vector2(Player.Center.X, Player.Center.Y - 400f);
            }
            if (AI_Timer == 5 && Main.netMode != NetmodeID.MultiplayerClient)
            {

                Projectile.NewProjectile(NPC.GetSource_FromThis(), TpPos, Vector2.Zero, ModContent.ProjectileType<LifeTpTelegraph>(), 0, 0f, Main.myPlayer, -40);
                //below wall telegraph
                for (int i = 0; i < 60; i++)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), new Vector2(LockVector2.X - 1500, LockVector2.Y + 400 + 500 + 60 * i), Vector2.Zero, ModContent.ProjectileType<BloomLine>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, 0, 0);
                }
            }
            if (AI_Timer == StartTime - 15)
            {
                Flying = false;
                Charging = true;
                NPC.Center = TpPos;
                NPC.velocity.X = 0f;
                NPC.velocity.Y = 0f;
                NPC.rotation = MathHelper.Pi;
                SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
                NPC.netUpdate = true;
            }
            if (AI_Timer == StartTime)
            {
                SoundEngine.PlaySound(SoundID.ForceRoarPitched, NPC.Center);
                HitPlayer = true;
                float chargeSpeed2 = 55f;
                NPC.velocity.Y = chargeSpeed2;
                NPC.netUpdate = true;
                //below wall
                if (WorldSavingSystem.MasochistModeReal)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int i = 0; i < 120; i++)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), new Vector2(LockVector2.X - 1200, LockVector2.Y + 600 + 500 + 30 * i), new Vector2(60, 0), ModContent.ProjectileType<LifeProjLarge>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer);
                        }
                        for (int i = 0; i < 120; i++)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), new Vector2(LockVector2.X + 1200, LockVector2.Y + 600 + 500 + 30 * i), new Vector2(-60, 0), ModContent.ProjectileType<LifeProjLarge>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer);
                        }
                    }
                }
            }
            if (AI_Timer >= StartTime)
            {
                HitPlayer = true;
                NPC.velocity = NPC.velocity * 0.96f;
            }
            if (AI_Timer == StartTime + 30 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                float knockBack4 = 3f;
                Vector2 shootdown2 = new(0f, 10f);
                SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, NPC.Center);
                for (int k = 0; k <= 15; k++)
                {
                    double rotationrad3 = MathHelper.ToRadians(-90 + k * 12);
                    Vector2 shootoffset3 = shootdown2.RotatedBy(rotationrad3);
                    if (!WorldSavingSystem.MasochistModeReal)
                        shootoffset3.X *= 2f;
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, shootoffset3, ModContent.ProjectileType<LifeNeggravProj>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), knockBack4, Main.myPlayer);
                }
            }
            if (AI_Timer == StartTime + 45 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                float knockBack3 = 3f;
                Vector2 shootdown = new(0f, 10f);
                SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, NPC.Center);
                for (int j = 0; j <= 15; j++)
                {
                    double rotationrad2 = MathHelper.ToRadians(-90 + j * 10);
                    Vector2 shootoffset2 = shootdown.RotatedBy(rotationrad2);
                    if (!WorldSavingSystem.MasochistModeReal)
                        shootoffset2.X *= 2f;
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, shootoffset2, ModContent.ProjectileType<LifeNeggravProj>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), knockBack3, Main.myPlayer);
                }
            }
            if (AI_Timer == StartTime + 50 && PlungeCount < 1f)
            {
                AI_Timer = 0f;
                PlungeCount += 1f;
            }
            if (AI_Timer == StartTime + 180)
            { //teleport back up
                NPC.position.X = Player.Center.X - NPC.width / 2;
                NPC.position.Y = Player.Center.Y - (NPC.height / 2 + 450f);
                SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
                HitPlayer = false;
                Flying = true;
                Charging = false;
                NPC.netUpdate = true;
            }
            if (AI_Timer >= StartTime + 240)
            {
                HitPlayer = false;
                oldstate = state;
                StateReset();
            }
        }
        public void AttackPixies()
        {
            ref float ChargeTimer = ref NPC.ai[2];
            ref float RandomOffset = ref NPC.ai[3];

            Player Player = Main.player[NPC.target];
            int StartTime = (WorldSavingSystem.MasochistModeReal ? 60 : WorldSavingSystem.EternityMode ? 70 : 80);

            if (AttackF1)
            {
                Flying = true;
                AttackF1 = false;
                NPC.netUpdate = true;
                RandomOffset = 0;
                /*
                if (PhaseThree)
                {
                */
                SoundEngine.PlaySound(SoundID.ScaryScream, NPC.Center);
                LockVector3 = NPC.Center;
                //}
            }
            /*
            if (!PhaseThree) //unused, previously p2 version
            {
                if (NPC.ai[2] > 60f && (NPC.ai[2] % 5) == 0 && AI_Timer < 280)
                {
                    SoundEngine.PlaySound(SoundID.Item25, NPC.Center);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        float knockBack10 = 3f;
                        Vector2 shootoffset4 = new Vector2(0f, -5f).RotatedBy(NPC.ai[3]);
                        NPC.ai[3] = (float)(Main.rand.Next(-30, 30) * (MathHelper.Pi / 180.0)); //change random offset after so game has time to sync
                        float ai0 = 0;
                        if (!WorldSavingSystem.MasochistModeReal)
                        {
                            ai0 = -30;
                            shootoffset4 /= 2;
                        }
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, shootoffset4, ModContent.ProjectileType<LifeHomingProj>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), knockBack10, Main.myPlayer, ai0, NPC.whoAmI);
                    }
                }
                NPC.ai[2] += 1f;
                int endtime = WorldSavingSystem.MasochistModeReal ? 280 : 390;
                if (AI_Timer > endtime)
                {
                    oldstate = state;
                    StateReset();
                }
            }
            */
            if (true) //previously phase 3 exclusive
            {
                AuraCenter = LockVector3;
                Flying = false;
                Charging = true;
                if (AI_Timer == StartTime)
                {
                    LockVector1 = Player.Center;
                    NPC.netUpdate = true;
                }
                const int ChargeCD = 60;
                if (ChargeTimer == ChargeCD) //charge
                {
                    SoundEngine.PlaySound(SoundID.ForceRoarPitched, NPC.Center);
                    float chargeSpeed = 22f;
                    Vector2 chargeatPlayer = NPC.DirectionTo(Player.Center) * chargeSpeed;
                    NPC.velocity = chargeatPlayer;
                    NPC.netUpdate = true;
                }
                if (AI_Timer % 5 == 0 && ChargeTimer > ChargeCD && ChargeTimer < ChargeCD + 40) //fire pixies during charges
                {
                    SoundEngine.PlaySound(SoundID.Item25, NPC.Center);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        float knockBack10 = 3f;
                        Vector2 shootoffset4 = Vector2.Normalize(NPC.velocity).RotatedBy(RandomOffset) * 5f;
                        RandomOffset = (float)(Main.rand.Next(-30, 30) * (MathHelper.Pi / 180.0)); //change random offset after so game has time to sync
                        float ai0 = 0;
                        if (!WorldSavingSystem.MasochistModeReal)
                        {
                            ai0 = -30;
                            shootoffset4 /= 2;
                        }
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, shootoffset4, ModContent.ProjectileType<LifeHomingProj>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), knockBack10, Main.myPlayer, ai0, NPC.whoAmI);
                    }
                }
                if (ChargeTimer >= ChargeCD + 60)
                {
                    ChargeTimer = ChargeCD - 1;
                }
                ChargeTimer++;
                NPC.velocity *= 0.99f;
                if (AI_Timer >= ChargeCD * 5)
                {
                    foreach (Projectile p in Main.projectile)
                    {
                        if (p.type == ModContent.ProjectileType<LifeHomingProj>())
                        {
                            p.ai[2] = 1;
                        }
                    }
                    Flying = true;
                    Charging = false;
                    oldstate = state;
                    StateReset();
                }
            }


        }
        public void AttackRoulette()
        {
            ref float TeleportX = ref NPC.localAI[0];
            ref float TeleportY = ref NPC.localAI[1];
            ref float RandomAngle = ref NPC.ai[3];

            Player Player = Main.player[NPC.target];
            if (AttackF1)
            {
                AttackF1 = false;
                Flying = false;
                NPC.velocity = Vector2.Zero;
                RandomAngle = Main.rand.NextFloat(MathHelper.ToRadians(45)) * (Main.rand.NextBool() ? 1 : -1);
                if (Player.Center.X < NPC.Center.X)
                    RandomAngle += MathHelper.Pi;
                NPC.netUpdate = true;
            }

            Vector2 RouletteTpPos = Player.Center + 500 * RandomAngle.ToRotationVector2();
            TeleportX = RouletteTpPos.X; //exposing so proj can access
            TeleportY = RouletteTpPos.Y;

            if (AI_Timer == 1 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                Projectile.NewProjectile(NPC.GetSource_FromThis(), RouletteTpPos, Vector2.Zero, ModContent.ProjectileType<LifeTpTelegraph>(), 0, 0f, Main.myPlayer, -40);
            }

            if (AI_Timer == 40)
            {
                NPC.Center = RouletteTpPos;
                SoundEngine.PlaySound(SoundID.Item8, NPC.Center); //PLACEHOLDER
                LockVector1 = NPC.DirectionTo(Player.Center);
                TeleportY = 0;
                NPC.netUpdate = true;
            }

            if (AI_Timer > 40)
            {
                float angleDiff = MathHelper.WrapAngle(NPC.DirectionTo(Player.Center).ToRotation() - LockVector1.ToRotation());
                if (Math.Abs(angleDiff) > MathHelper.Pi / 3f)
                {
                    LockVector1 = NPC.DirectionTo(Player.Center);
                    NPC.netUpdate = true;
                }
            }

            if (AI_Timer < 420 + 120 && AI_Timer % 9 == 0 && AI_Timer > 60 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                const float speed = 20f;
                Vector2 offset1 = LockVector1.RotatedBy(MathHelper.Pi / 3f) * speed;
                Vector2 offset2 = LockVector1.RotatedBy(-MathHelper.Pi / 3f) * speed;
                /*
                //removed variant, wavy border
                //in p3, rotate offsets by +-5 degrees determined by sine curve, one loop is 4 seconds
                if (PhaseThree)
                {
                    float waveModifier = WorldSavingSystem.MasochistModeReal ? 6.5f : 8f;
                    offset1 = offset1.RotatedBy((MathHelper.Pi / waveModifier) * Math.Sin(MathHelper.ToRadians(1.5f * AI_Timer)));
                    offset2 = offset2.RotatedBy((MathHelper.Pi / waveModifier) * -Math.Sin(MathHelper.ToRadians(1.5f * AI_Timer)));
                }
                */

                //const int timeleft = 180;
                 TeleportY++;
                //int p =
                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, offset1, ModContent.ProjectileType<LifeProjSmall>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0, Main.myPlayer, 0, 3);
                //if (p != Main.maxProjectiles)
                //    Main.projectile[p].timeLeft = timeleft;
                //p = 
                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, offset2, ModContent.ProjectileType<LifeProjSmall>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0, Main.myPlayer, 0, 4);
                //if (p != Main.maxProjectiles)
                //    Main.projectile[p].timeLeft = timeleft;
            }


            //new homing swords:

            if (AI_Timer >= 70 && AI_Timer % 70 == 0 && AI_Timer <= 420)
            {
                SoundEngine.PlaySound(SoundID.Item71, NPC.Center);
                int randSide = Main.rand.NextBool(2) ? 1 : -1;
                float randRot = Main.rand.NextFloat(-MathHelper.Pi / 8, MathHelper.Pi / 8);
                Vector2 offset1 = (NPC.DirectionTo(Player.Center) * 8f).RotatedBy(MathHelper.PiOver2 * randSide + randRot);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, -offset1, ModContent.ProjectileType<JevilScar>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer, 0f, NPC.whoAmI);
            }
            if (AI_Timer > 480 + 100/*rework*/)
            {
                foreach (Projectile p in Main.projectile)
                {
                    if (p.type == ModContent.ProjectileType<JevilScar>())
                    {
                        p.ai[0] = 1200;
                    }
                }
                Flying = true;
                oldstate = state;
                StateReset();
            }
        }
        public void AttackReactionShotgun()
        {
            ref float RandomSide = ref NPC.localAI[1];
            ref float RandomWindup = ref NPC.localAI[2];
            ref float RandomAngle = ref NPC.localAI[0];

            Player Player = Main.player[NPC.target];
            if (AttackF1)
            {
                AttackF1 = false;
                HitPlayer = true;
                NPC.netUpdate = true;
            }

            if (NPC.Distance(Player.Center) > 1200)
            {
                FlyingState(1.5f);
            }
            else
            {
                Flying = false;
                NPC.velocity *= 0.9f;
            }
            if (AI_Timer == 1)
            {
                RandomWindup = Main.rand.Next(140, 220);
                SoundEngine.PlaySound(SoundID.Unlock, Player.Center);
                RandomSide = Main.rand.Next(2);
                NPC.netUpdate = true;
            }


            if (AI_Timer < RandomWindup)
            { //wait for blast
                Flying = false;
                //float inertia2 = 1f;
                //Vector2 flyonPlayer = NPC.DirectionTo(OnPlayer) * flySpeed2;
                // NPC.velocity = (NPC.velocity * (inertia2 - 1f) + flyonPlayer) / inertia2;
                if (AI_Timer == 1 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    // ARC
                    int timeLeft = ((int)RandomWindup - 30);
                    for (int i = -1; i < 2; i+= 2)
                    {
                        float rot = (NPC.Center - Player.Center).RotatedBy(i * MathHelper.Pi / 12).ToRotation();
                        //TODO: arc telegraph
                        
                        //int p = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<ArcTelegraph>(), 0, 0f, Main.myPlayer, rot);
                        //if (p != Main.maxProjectiles)
                            //Main.projectile[p].timeLeft = timeLeft;
                    }
                    // ARC

                    float ai0 = -(RandomWindup - 30);
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center - (NPC.Center - Player.Center).RotatedBy(-MathHelper.Pi / 12), Vector2.Zero, ModContent.ProjectileType<LifeCrosshair>(), 0, 0f, Main.myPlayer, ai0, 2);
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center - (NPC.Center - Player.Center).RotatedBy(MathHelper.Pi / 12), Vector2.Zero, ModContent.ProjectileType<LifeCrosshair>(), 0, 0f, Main.myPlayer, ai0, 2);

                    if (!PhaseOne)
                    {
                        // ARC
                        for (int i = -1; i < 2; i+= 2)
                        {
                            //TODO: arc telegraph
                            float rot = ((NPC.Center - Player.Center).RotatedBy(i * MathHelper.Pi / 4)).ToRotation();
                            //int p = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<ArcTelegraph>(), 0, 0f, Main.myPlayer, rot);
                            //if (p != Main.maxProjectiles)
                            //    Main.projectile[p].timeLeft = timeLeft;
                        }
                        // ARC

                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center - (NPC.Center - Player.Center).RotatedBy(MathHelper.Pi / 4), Vector2.Zero, ModContent.ProjectileType<LifeCrosshair>(), 0, 0f, Main.myPlayer, ai0, 2);
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center - (NPC.Center - Player.Center).RotatedBy(-MathHelper.Pi / 4), Vector2.Zero, ModContent.ProjectileType<LifeCrosshair>(), 0, 0f, Main.myPlayer, ai0, 2);
                    }
                }
            }
            if (AI_Timer == RandomWindup - 30)
            {
                SoundEngine.PlaySound(SoundID.Unlock, Player.Center);
                NPC.netUpdate = true;
            }
            if (AI_Timer == RandomWindup - 20 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                // ARC
                float rot1 = (NPC.Center - Player.Center).RotatedBy((-MathHelper.Pi / 12) + (RandomSide * MathHelper.Pi / 6)).ToRotation();
                //TODO: arc telegraph
                //int p1 = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<ArcTelegraph>(), 0, 0f, Main.myPlayer, rot1);
                //if (p1 != Main.maxProjectiles)
                   // Main.projectile[p1].timeLeft = 20;
                // ARC
                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center - (NPC.Center - Player.Center).RotatedBy(-MathHelper.Pi / 12 + RandomSide * MathHelper.Pi / 6), Vector2.Zero, ModContent.ProjectileType<LifeCrosshair>(), 0, 0f, Main.myPlayer, -20, 2);
                if (!PhaseOne)
                {
                    // ARC
                    for (int i = -1; i < 2; i+= 2)
                    {
                        float rot2 = (NPC.Center - Player.Center).RotatedBy(i * MathHelper.Pi / 4).ToRotation();
                        //TODO: arc telegraph
                        //int p2 = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<ArcTelegraph>(), 0, 0f, Main.myPlayer, rot2);
                        //if (p2 != Main.maxProjectiles)
                            //Main.projectile[p2].timeLeft = 20;
                    }
                    // ARC
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center - (NPC.Center - Player.Center).RotatedBy(MathHelper.Pi / 4), Vector2.Zero, ModContent.ProjectileType<LifeCrosshair>(), 0, 0f, Main.myPlayer, -20, 2);
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center - (NPC.Center - Player.Center).RotatedBy(-MathHelper.Pi / 4), Vector2.Zero, ModContent.ProjectileType<LifeCrosshair>(), 0, 0f, Main.myPlayer, -20, 2);
                }
            }
            else if (AI_Timer == RandomWindup)
            {
                float shootSpeed = WorldSavingSystem.MasochistModeReal || !PhaseOne ? 27f : 22f;
                LockVector2 = NPC.DirectionTo(Player.Center) * shootSpeed;
                NPC.netUpdate = true;
            }
            else if ((AI_Timer - RandomWindup) % 10 == 0 && AI_Timer > RandomWindup && Main.netMode != NetmodeID.MultiplayerClient && (AI_Timer < RandomWindup + 90 && PhaseOne || AI_Timer < RandomWindup + 270 && !PhaseOne)) //blast
            {
                SoundEngine.PlaySound(SoundID.Item12, Player.Center);
                float knockBack10 = 3f;
                for (int i = -3; i < 17; i++)
                {
                    int p = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, LockVector2.RotatedBy(i * -MathHelper.Pi / 48 + i * RandomSide * MathHelper.Pi / 24), ModContent.ProjectileType<LifeWave>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), knockBack10, Main.myPlayer);
                    if (p != Main.maxProjectiles)
                        Main.projectile[p].timeLeft = 120;

                    if (!PhaseOne)
                    {
                        p = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, LockVector2.RotatedBy(MathHelper.Pi / 4 + (i + 4) * MathHelper.Pi / 48 - (RandomSide * MathHelper.Pi / 2 + (i + 4) * RandomSide * MathHelper.Pi / 24)), ModContent.ProjectileType<LifeWave>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), knockBack10, Main.myPlayer);
                        if (p != Main.maxProjectiles)
                            Main.projectile[p].timeLeft = 120;
                    }
                }
            }

            //in p2, shoot volleys in closed area
            if (!PhaseOne && AI_Timer >= RandomWindup && AI_Timer < RandomWindup + 244)
            {
                if ((AI_Timer - RandomWindup) % 61 == 0) //choose spot
                {
                    RandomAngle = MathHelper.ToRadians(Main.rand.Next(-15, 15));
                    LockVector1 = Vector2.Normalize(LockVector2).RotatedBy(MathHelper.ToRadians(25 - 50 * RandomSide) - RandomAngle);
                    NPC.netUpdate = true;
                    if (Main.netMode != NetmodeID.MultiplayerClient) //telegraph
                    {
                        int x = WorldSavingSystem.MasochistModeReal ? 1 : 0; //1 shot below maso, 3 shots in maso
                        for (int i = -x; i <= x; i++)
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<BloomLine>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, 1, LockVector1.RotatedBy(MathHelper.Pi / 32 * i).ToRotation());
                        //Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + LockVector1 * 600f, Vector2.Zero, ModContent.ProjectileType<LifeCrosshair>(), 0, 0f, Main.myPlayer, -55);
                    }
                }
                if ((AI_Timer - RandomWindup) % 61 > 55 && (AI_Timer - RandomWindup) % 2 == 0) //fire
                {
                    SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, Player.Center);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int x = WorldSavingSystem.MasochistModeReal ? 1 : 0; //1 shot below maso, 3 shots in maso
                        for (int i = -x; i <= x; i++)
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, 30f * LockVector1.RotatedBy(MathHelper.Pi / 32 * i), ModContent.ProjectileType<LifeProjLarge>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer);
                    }
                }

            }


            if (AI_Timer == RandomWindup + 90 && PhaseOne || AI_Timer == RandomWindup + 300 && !PhaseOne)
            {
                //NPC.position.X = Player.position.X - (NPC.width / 2);
                //NPC.position.Y = Player.position.Y - 450f - (NPC.height / 2);
                //SoundEngine.PlaySound(SoundID.Item8, NPC.Center); //PLACEHOLDER
                HitPlayer = false;
                Flying = true;
                NPC.netUpdate = true;
            }


            int endtime = !PhaseOne ? WorldSavingSystem.MasochistModeReal ? 340 : 240 : 110;
            if (AI_Timer > RandomWindup + endtime)
            {
                HitPlayer = false;
                RandomSide = 0;
                if (PhaseOne)
                {
                    oldP1state = P1state;
                    P1stateReset();
                }
                else
                {
                    oldstate = state;
                    StateReset();
                }

            }

        }
        public void AttackRunningMinigun()
        {
            ref float ShotCount = ref NPC.ai[2];

            Player Player = Main.player[NPC.target];
            int startup = WorldSavingSystem.MasochistModeReal ? 40 : WorldSavingSystem.EternityMode ? 50 : 60;

            if (AttackF1)
            {
                AttackF1 = false;
                SoundEngine.PlaySound(SoundID.Zombie100, NPC.Center);
                NPC.netUpdate = true;
                rot = 0;

                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<BloomLine>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, 1, NPC.DirectionTo(Player.Center).ToRotation());
            }
            Flying = false;
            float flySpeed3 = 5f;
            float inertia3 = 5f;
            Vector2 flyonPlayer2 = NPC.DirectionTo(Player.Center) * flySpeed3;
            NPC.velocity = (NPC.velocity * (inertia3 - 1f) + flyonPlayer2) / inertia3;

            int endtime = 360 + startup;

            //replacing below outdated code
            float rampRatio = AI_Timer / endtime;
            rampRatio *= 0.2f;
            NPC.position += NPC.velocity * rampRatio;
            /*
            if (false)
            {
                endtime = (endtime - startup) / 2 + startup;
            }
            if (true)
            {
                float rampRatio = AI_Timer / endtime;
                rampRatio *= 0.2f;
                NPC.position += NPC.velocity * rampRatio;
            }
            */

            //rotation
            if (NPC.velocity.ToRotation() > MathHelper.Pi)
            {
                NPC.rotation = 0f - MathHelper.Pi * NPC.velocity.X / 100;
            }
            else
            {
                NPC.rotation = 0f + MathHelper.Pi * NPC.velocity.X / 100;
            }
            const float timescale = 1.5f; // frames per shot = 10 * timescale
            if (AI_Timer >= startup && AI_Timer % (10 * timescale) == 0)
            {
                SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, NPC.Center);
                for (int i = -1; i < 2; i += 2)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 ShootPlayer = (NPC.DirectionTo(Player.Center) * 12f).RotatedBy(i * rot * MathHelper.Pi / 180);
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, ShootPlayer, ModContent.ProjectileType<LifeProjLarge>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer);
                    }
                }
                if (rot >= 0)
                    rot += ShotCount < 8 / timescale || ShotCount >= 16 / timescale && ShotCount < 24 / timescale ? 5 * timescale : -5 * timescale;
                else
                    rot = 0;
                ShotCount++;
            }
            if (AI_Timer == endtime || true && AI_Timer == (endtime + startup) / 2) //final shot towards player to prevent dodging by just standing still
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 ShootPlayer = NPC.DirectionTo(Player.Center) * 12f;
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, ShootPlayer, ModContent.ProjectileType<LifeProjLarge>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer);
                }
            }
            /*
            //firing machinegun
            if (AI_Timer > 15 && NPC.ai[2] >= (72 - (11 * Rampup)) && Main.netMode != NetmodeID.MultiplayerClient)
            {
                SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, NPC.Center);
                for (int i = -1; i < 2; i++)
                {
                    Vector2 ShootPlayer = (NPC.DirectionTo(Player.Center) * 5f*Rampup).RotatedBy(i * MathHelper.Pi / 7f);
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, ShootPlayer, ModContent.ProjectileType<LifeProjLarge>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer);
                }
                if (Rampup < 6)
                    Rampup++;
                NPC.ai[2] = 0;
            }
            NPC.ai[2]++;

            //firing circle in p3
            if (WorldSavingSystem.MasochistModeReal && AI_Timer > 90 && AI_Timer % 45 == 0 && Main.netMode != NetmodeID.MultiplayerClient && PhaseThree)
            {
                float ProjectileSpeed = 10f;
                Vector2 shootatPlayer3 = NPC.DirectionTo(Player.Center) * ProjectileSpeed;
                int amount2 = 14;
                for (int i = 0; i < amount2; i++)
                {
                    Vector2 shootoffset = shootatPlayer3.RotatedBy(i * (MathHelper.Pi / (amount2 / 2)));
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, shootoffset, ModContent.ProjectileType<LifeWave>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer);
                }
            }
            */
            if (AI_Timer >= endtime)
            {
                Flying = true;
                oldstate = state;
                StateReset();
            }
        }
        //unused
        /*
        public void AttackRain()
        {
            Player Player = Main.player[NPC.target];

            NPC.localAI[0] = Player.Center.X; //expose for proj to access
            NPC.localAI[1] = Player.Center.Y - 300;

            if (AttackF1)
            {
                AttackF1 = false;
                NPC.netUpdate = true;

                if (Main.netMode != NetmodeID.MultiplayerClient)
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), new Vector2(NPC.localAI[0], NPC.localAI[1]), Vector2.Zero, ModContent.ProjectileType<LifeTpTelegraph>(), 0, 0f, Main.myPlayer, -60);
            }

            if (AI_Timer > 60f && AI_Timer < 360f)
            {
                Flying = true;
                NPC.Center = new Vector2(NPC.localAI[0], NPC.localAI[1]);

                if (NPC.ai[2] > 1)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        NPC.ai[0]++;
                        float knockBack6 = 3f;
                        NPC.ai[3] = Main.rand.Next(-750, 750);
                        Vector2 spawnPos = new Vector2(Player.Center.X - NPC.ai[3], Player.Center.Y - 750f);
                        if (!WorldSavingSystem.MasochistModeReal)
                            spawnPos.X += Player.velocity.X * 30; //malice
                        float ai1 = -1;
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), spawnPos, new Vector2(0f, 7f), ModContent.ProjectileType<LifeProjSmall>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), knockBack6, Main.myPlayer, NPC.ai[0] * 1000, ai1);
                    }
                    NPC.ai[2] = 0f;
                }
                NPC.ai[2] += 1f;
            }
            if (AI_Timer > 420f)
            {
                Flying = true;
                oldstate = state;
                StateReset();
            }
        }
        */
        public void AttackTeleportNukes()
        {
            ref float TeleportX = ref NPC.localAI[0];
            ref float TeleportY = ref NPC.localAI[1];

            ref float ShotCount = ref NPC.ai[2];

            Player Player = Main.player[NPC.target];
            int StartTime = (WorldSavingSystem.MasochistModeReal ? 60 : WorldSavingSystem.EternityMode ? 75 : 85);
            if (AttackF1)
            {
                AttackF1 = false;

                LockVector1 = Player.Center;

                Flying = false;
                NPC.velocity = Vector2.Zero;
                NPC.netUpdate = true;
            }

            TeleportX = LockVector1.X; //exposing so proj can access
            TeleportY = LockVector1.Y;

            if (AI_Timer == 1 && Main.netMode != NetmodeID.MultiplayerClient) //telegraph teleport and first shots
            {
                Projectile.NewProjectile(NPC.GetSource_FromThis(), LockVector1, Vector2.Zero, ModContent.ProjectileType<LifeTpTelegraph>(), 0, 0f, Main.myPlayer, -60);
                for (int i = 0; i < 16; i++)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), LockVector1, Vector2.Zero, ModContent.ProjectileType<BloomLine>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, 1, MathHelper.Pi / 8 * i);
                }
            }

            if (AI_Timer == StartTime) //teleport and first shots
            {
                NPC.Center = LockVector1;
                NPC.netUpdate = true;
                SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
                for (int i = 0; i < 16; i++)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(24f, 0f).RotatedBy(MathHelper.Pi / 8 * i), ModContent.ProjectileType<LifeProjLarge>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer);
                }
                Main.LocalPlayer.GetModPlayer<FargoSoulsPlayer>().Screenshake = 90;

                //telegraph nukes
                for (int i = 0; i < 6; i++)
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<BloomLine>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, 1, MathHelper.Pi / 3 * i);
            }

            if (AI_Timer >= StartTime + 60 && (AI_Timer - (StartTime + 60)) % 3 == 0 && AI_Timer < StartTime + 60 + 17) //nukes
            {
                SoundEngine.PlaySound(SoundID.Item91, NPC.Center);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    float ai0 = WorldSavingSystem.MasochistModeReal ? 32 : 24;
                    float ai1 = 0;
                    float speed = 16;
                    int p = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(speed, 0f).RotatedBy(MathHelper.Pi / 3 * ShotCount), ModContent.ProjectileType<LifeNuke>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage, 1.5f), 300f, Main.myPlayer, ai0, ai1);
                    if (p != Main.maxProjectiles)
                        Main.projectile[p].timeLeft = 60;
                }
                ShotCount++;
            }
            if (AI_Timer > StartTime + 360)
            {
                Flying = true;
                oldstate = state;
                StateReset();
            }
        }
        public void AttackRuneExpand()
        {
            ref float ExtraShots = ref NPC.ai[3];
            ref float RandomAngle = ref NPC.ai[2];

            Player Player = Main.player[NPC.target];
            //let projectiles access
            NPC.localAI[0] = RuneDistance;
            NPC.localAI[1] = BodyRotation;
            NPC.localAI[2] = RuneCount;

            const int ExpandTime = 175;
            int AttackDuration = PhaseOne ? 5 : 390 + 80; //change this depending on phase

            if (AttackF1)
            {
                AttackF1 = false;
                NPC.netUpdate = true;
                SoundEngine.PlaySound(SoundID.Item84, NPC.Center);

                //invisible rune hitbox
                for (int i = 0; i < RuneCount; i++)
                {
                    float runeRot = (float)(BodyRotation + Math.PI * 2 / RuneCount * i);
                    Vector2 runePos = NPC.Center + runeRot.ToRotationVector2() * RuneDistance;

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), runePos, Vector2.Zero, ModContent.ProjectileType<LifeRuneHitbox>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer, NPC.whoAmI, i);
                }
                if (!PhaseOne)
                {
                    Flying = false;
                    NPC.velocity = Vector2.Zero;
                }
                else //PhaseOne
                {
                    //decrease size to size of pyramid
                    NPC.position = NPC.Center;
                    NPC.Size = new Vector2(90, 90);
                    NPC.Center = NPC.position;
                }

            }

            if (NPC.Distance(Player.Center) > 2000)
                FlyingState(1.5f);
            else
                NPC.velocity *= 0.95f;

            if (AI_Timer < ExpandTime) //expand
            {
                if (WorldSavingSystem.MasochistModeReal)
                {
                    RuneDistance = Math.Min((float)(100 + Math.Pow(AI_Timer / 5, 2)), 1200);
                }
                else
                {
                    RuneDistance = (float)(100 + Math.Pow(AI_Timer / 5, 2));
                }
                RPS += 0.0005f;
            }

            if (AI_Timer >= ExpandTime && !PhaseOne) //p2-3 shots during expansion
            {
                HitPlayer = true; //start dealing contact damage (anti-cheese)
                int startShots = 24;
                float ProjectileSpeed = 30f;
                float knockBack2 = 3f;
                int Shots = startShots + (int)ExtraShots;
                float spread = MathHelper.TwoPi / ExtraShots;
                if ((AI_Timer - ExpandTime) % 40 == 0f && ExtraShots < 9)
                {
                    LockVector1 = NPC.Center;
                    LockVector2 = (NPC.DirectionTo(Player.Center) * ProjectileSpeed).RotatedBy(MathHelper.Pi / 80 * (Main.rand.NextFloat() - 0.5f));
                    RandomAngle = Main.rand.NextFloat(-spread / 2, spread / 2);
                    if (Main.netMode != NetmodeID.MultiplayerClient) //telegraph
                    {
                        for (int i = 0; (float)i < Shots; i++)
                        {
                            double rotationrad = MathHelper.TwoPi / Shots * i;
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), LockVector1, Vector2.Zero, ModContent.ProjectileType<BloomLine>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, 0, LockVector2.RotatedBy(rotationrad).ToRotation());
                        }
                    }
                    NPC.netUpdate = true;
                }
                if ((AI_Timer - ExpandTime) % 40 == 39 && ExtraShots < 9)
                {
                    SoundEngine.PlaySound(SoundID.Item12, NPC.Center);
                    if (Main.netMode != NetmodeID.MultiplayerClient) //shoot
                    {
                        for (int i = 0; (float)i < Shots; i++)
                        {
                            double rotationrad = MathHelper.TwoPi / Shots * i;
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), LockVector1, LockVector2.RotatedBy(rotationrad), ModContent.ProjectileType<LifeWave>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), knockBack2, Main.myPlayer);
                        }
                    }
                    ExtraShots += 1f;
                }
            }

            if (AI_Timer == ExpandTime + AttackDuration) //noise
            {
                SoundEngine.PlaySound(SoundID.Item84, NPC.Center);
            }
            if (AI_Timer >= ExpandTime + AttackDuration) //retract
            {
                HitPlayer = false; //stop dealing contact damage (anti-cheese)
                if (WorldSavingSystem.MasochistModeReal)
                {
                    RuneDistance = Math.Min((float)(100 + Math.Pow((ExpandTime - (AI_Timer - ExpandTime - AttackDuration)) / 5, 2)), 1200);
                }
                else
                {
                    RuneDistance = (float)(100 + Math.Pow((ExpandTime - (AI_Timer - ExpandTime - AttackDuration)) / 5, 2));
                }
                RPS -= 0.0005f;
            }
            if (AI_Timer >= ExpandTime + AttackDuration + ExpandTime)
            {
                RuneDistance = 100; //make sure

                //kill rune hitboxes
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    foreach (Projectile p in Main.projectile)
                    {
                        if (p.type == ModContent.ProjectileType<LifeRuneHitbox>())
                        {
                            p.Kill();
                        }
                    }
                }
                if (PhaseOne)
                {
                    //revert size
                    NPC.position = NPC.Center;
                    NPC.Size = new Vector2(DefaultWidth, DefaultHeight);
                    NPC.Center = NPC.position;

                    oldP1state = P1state;
                    P1stateReset();
                }
                else
                {
                    Flying = true;
                    oldstate = state;
                    StateReset();
                }
            }
        }
        #endregion
        #endregion
        #region Overrides
        public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
        {
            modifiers.FinalDamage /= 2f;

            if (useDR)
                modifiers.FinalDamage /= 4f;

            if (phaseProtectionDR)
                modifiers.FinalDamage /= 4f;
        }

        public override void UpdateLifeRegen(ref int damage)
        {
            if ((useDR || phaseProtectionDR) && NPC.lifeRegen < 0)
                NPC.lifeRegen /= 2;
        }

        #region Hitbox
        public override bool CanHitPlayer(Player target, ref int CooldownSlot)
        {
            if (HitPlayer)
            {
                Vector2 boxPos = target.position;
                Vector2 boxDim = target.Size;
                return Collides(boxPos, boxDim);
            }
            return false;
        }
        public override bool CanHitNPC(NPC target)
        {
            if (HitPlayer)
            {
                Vector2 boxPos = target.position;
                Vector2 boxDim = target.Size;
                return Collides(boxPos, boxDim);
            }
            return false;
        }
        public bool Collides(Vector2 boxPos, Vector2 boxDim)
        {
            //circular hitbox-inator
            Vector2 ellipseDim = NPC.Size;
            Vector2 ellipseCenter = NPC.position + 0.5f * new Vector2(NPC.width, NPC.height);

            float x = 0f; //ellipse center
            float y = 0f; //ellipse center
            if (boxPos.X > ellipseCenter.X)
            {
                x = boxPos.X - ellipseCenter.X; //left corner
            }
            else if (boxPos.X + boxDim.X < ellipseCenter.X)
            {
                x = boxPos.X + boxDim.X - ellipseCenter.X; //right corner
            }
            if (boxPos.Y > ellipseCenter.Y)
            {
                y = boxPos.Y - ellipseCenter.Y; //top corner
            }
            else if (boxPos.Y + boxDim.Y < ellipseCenter.Y)
            {
                y = boxPos.Y + boxDim.Y - ellipseCenter.Y; //bottom corner
            }
            float a = ellipseDim.X / 2f;
            float b = ellipseDim.Y / 2f;

            return x * x / (a * a) + y * y / (b * b) < 1; //point collision detection
        }
        #endregion
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 100; i++)
                {
                    int DustType = Main.rand.NextFromList(DustID.YellowTorch, DustID.PinkTorch, DustID.UltraBrightTorch);
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustType, 0, 0, 100, new Color(), 1f);
                }
                for (int i = 1; i <= 12; i++)
                {
                    Vector2 rand = new(Main.rand.NextFloat(NPC.width), Main.rand.NextFloat(NPC.height));
                    int j = Main.rand.Next(ChunkSpriteCount) + 1;
                    if (!Main.dedServ)
                        Gore.NewGore(NPC.GetSource_FromThis(), NPC.position + rand, NPC.velocity, ModContent.Find<ModGore>(Mod.Name, $"ShardGold{j}").Type, NPC.scale);
                }
                for (int i = 1; i <= 4; i++)
                {
                    Vector2 rand = new(Main.rand.NextFloat(NPC.width), Main.rand.NextFloat(NPC.height));
                    if (!Main.dedServ)
                        Gore.NewGore(NPC.GetSource_FromThis(), NPC.position + rand, NPC.velocity, ModContent.Find<ModGore>(Mod.Name, $"Pyramid{i}").Type, NPC.scale);
                }
                return;
            }
        }

        public override bool CheckDead()
        {
            //if (!resigned) //no dying before final phase
            //{
            //    NPC.life = 1;
            //    return false;
            //}
            return base.CheckDead();
        }

        public const int ChunkCount = 12;
        public const int RuneCount = 12;
        const int ChunkSpriteCount = 12;
        const string PartsPath = "FargowiltasSouls/Assets/ExtraTextures/LifeChallengerParts/";

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) //DRAW BODY AND WINGS
        {

            if (Draw || NPC.IsABestiaryIconDummy)
            {
                //if chunk list is empty, fill list
                //for list, for loop, rotate by rotation + 360 * current item / max item
                //for chunk list, place using triangle formula
                //also rotate each chunk individually, random start rotation


                //const int RuneSpriteCount = 12;

                const float ChunkRotationSpeed = MathHelper.TwoPi / (8 * 60);

                if (chunklist.Count < ChunkCount)
                {
                    chunklist = InitializeSpriteList(ChunkSpriteCount, ChunkCount);
                }

                if (chunkrotlist.Count < ChunkCount)
                {
                    for (int i = 0; i < ChunkCount; i++)
                    {
                        chunkrotlist.Add(MathHelper.ToRadians(Main.rand.Next(360)));
                    }
                }


                if (SpritePhase > 1)
                {
                    for (int i = 0; i < ChunkCount; i++)
                    {
                        float drawRot = (float)(-BodyRotation - Math.PI * 2 / ChunkCount * i);
                        Vector2 drawPos = NPC.Center + drawRot.ToRotationVector2() * ChunkDistance - screenPos;
                        //Vector2 drawPos = Trianglinator(i, screenPos);

                        Texture2D ChunkTexture = ModContent.Request<Texture2D>(PartsPath + $"ShardGold{chunklist[i]}", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                        float ChunkRotation = chunkrotlist[i];
                        chunkrotlist[i] += ChunkRotationSpeed;

                        spriteBatch.Draw(origin: new Vector2(ChunkTexture.Width / 2, ChunkTexture.Height / 2), texture: ChunkTexture, position: drawPos, sourceRectangle: null, color: drawColor, rotation: ChunkRotation, scale: NPC.scale, effects: SpriteEffects.None, layerDepth: 0f);
                    }
                }
                for (int i = 0; i < RuneCount; i++)
                {
                    float drawRot = (float)(BodyRotation + Math.PI * 2 / RuneCount * i);
                    Texture2D RuneTexture = ModContent.Request<Texture2D>(PartsPath + $"Rune{i + 1}", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                    Vector2 drawPos = NPC.Center + drawRot.ToRotationVector2() * RuneDistance - screenPos;
                    float RuneRotation = drawRot + MathHelper.PiOver2;

                    //rune glow
                    for (int j = 0; j < RuneCount; j++)
                    {
                        Vector2 afterimageOffset = (MathHelper.TwoPi * j / 12f).ToRotationVector2() * 1f;
                        Color glowColor;

                        if (i % 3 == 0) //cyan
                            glowColor = new Color(0f, 1f, 1f, 0f) * 0.7f;
                        else if (i % 3 == 1) //yellow
                            glowColor = new Color(1f, 1f, 0f, 0f) * 0.7f;
                        else //pink
                            glowColor = new Color(1, 192 / 255f, 203 / 255f, 0f) * 0.7f;

                        Main.spriteBatch.Draw(RuneTexture, drawPos + afterimageOffset, null, glowColor, RuneRotation, RuneTexture.Size() * 0.5f, NPC.scale, SpriteEffects.None, 0f);
                    }
                    spriteBatch.Draw(origin: new Vector2(RuneTexture.Width / 2, RuneTexture.Height / 2), texture: RuneTexture, position: drawPos, sourceRectangle: null, color: Color.White, rotation: RuneRotation, scale: NPC.scale, effects: SpriteEffects.None, layerDepth: 0f);
                }

                //draw arena runes
                if (DoAura)
                {
                    const int AuraRuneCount = 48;
                    for (int i = 0; i < AuraRuneCount; i++)
                    {
                        float drawRot = (float)(BodyRotation + Math.PI * 2 / AuraRuneCount * i);
                        Texture2D RuneTexture = ModContent.Request<Texture2D>(PartsPath + $"Rune{(i % RuneCount) + 1}", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                        Vector2 drawPos = AuraCenter + drawRot.ToRotationVector2() * (1100 + RuneDistance) - screenPos;
                        float RuneRotation = drawRot + MathHelper.PiOver2;

                        //rune glow
                        for (int j = 0; j < AuraRuneCount; j++)
                        {
                            Vector2 afterimageOffset = (MathHelper.TwoPi * j / 12f).ToRotationVector2() * 1f;
                            Color glowColor;

                            if (i % 3 == 0) //cyan
                                glowColor = new Color(0f, 1f, 1f, 0f) * 0.7f;
                            else if (i % 3 == 1) //yellow
                                glowColor = new Color(1f, 1f, 0f, 0f) * 0.7f;
                            else //pink
                                glowColor = new Color(1, 192 / 255f, 203 / 255f, 0f) * 0.7f;

                            Main.spriteBatch.Draw(RuneTexture, drawPos + afterimageOffset, null, glowColor, RuneRotation, RuneTexture.Size() * 0.5f, NPC.scale, SpriteEffects.None, 0f);
                        }
                        spriteBatch.Draw(origin: new Vector2(RuneTexture.Width / 2, RuneTexture.Height / 2), texture: RuneTexture, position: drawPos, sourceRectangle: null, color: Color.White, rotation: RuneRotation, scale: NPC.scale, effects: SpriteEffects.None, layerDepth: 0f);
                    }
                }

                //draw wings
                //draws 4 things: 2 upper wings, 2 lower wings

                Texture2D wingUtexture = ModContent.Request<Texture2D>(PartsPath + "LifeChallenger_WingUpper", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                Texture2D wingLtexture = ModContent.Request<Texture2D>(PartsPath + "LifeChallenger_WingLower", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                Vector2 wingdrawPos = NPC.Center - screenPos;
                int currentFrame = NPC.frame.Y;
                int wingUHeight = wingUtexture.Height / Main.npcFrameCount[NPC.type];
                Rectangle wingURectangle = new(0, currentFrame * wingUHeight, wingUtexture.Width, wingUHeight);
                int wingLHeight = wingLtexture.Height / Main.npcFrameCount[NPC.type];
                Rectangle wingLRectangle = new(0, currentFrame * wingLHeight, wingLtexture.Width, wingLHeight);
                Vector2 wingUOrigin = new(wingUtexture.Width / 2, wingUtexture.Height / 2 / Main.npcFrameCount[NPC.type]);
                Vector2 wingLOrigin = new(wingLtexture.Width / 2, wingLtexture.Height / 2 / Main.npcFrameCount[NPC.type]);

                for (int i = -1; i < 2; i += 2)
                {
                    float wingLRotation = NPC.rotation - MathHelper.PiOver2 + MathHelper.ToRadians(110 * i);
                    float wingURotation = NPC.rotation - MathHelper.PiOver2 + MathHelper.ToRadians(70 * i);
                    SpriteEffects flip = i == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;
                    spriteBatch.Draw(origin: wingUOrigin, texture: wingUtexture, position: wingdrawPos + wingURotation.ToRotationVector2() * (DefaultWidth / 2 + 30), sourceRectangle: wingURectangle, color: drawColor, rotation: wingURotation, scale: NPC.scale, effects: flip, layerDepth: 0f);
                    spriteBatch.Draw(origin: wingLOrigin, texture: wingLtexture, position: wingdrawPos + wingLRotation.ToRotationVector2() * (DefaultWidth / 2 + 30), sourceRectangle: wingLRectangle, color: drawColor, rotation: wingLRotation, scale: NPC.scale, effects: flip, layerDepth: 0f);
                }


            }
            return false;
        }
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) //DRAW STAR
        {
            if ((SpritePhase > 1 || !Draw) && !NPC.IsABestiaryIconDummy) //star
            {
                spriteBatch.End(); spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);


                Texture2D star = ModContent.Request<Texture2D>("FargowiltasSouls/Assets/Effects/LifeStar", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                Rectangle rect = new(0, 0, star.Width, star.Height);
                float scale = 0.45f * Main.rand.NextFloat(1f, 2.5f);
                Vector2 origin = new(star.Width / 2 + scale, star.Height / 2 + scale);

                spriteBatch.Draw(star, NPC.Center - screenPos, new Rectangle?(rect), Color.HotPink, 0, origin, scale, SpriteEffects.None, 0);
                DrawData starDraw = new(star, NPC.Center - screenPos, new Rectangle?(rect), Color.HotPink, 0, origin, scale, SpriteEffects.None, 0);
                GameShaders.Misc["LCWingShader"].UseColor(Color.HotPink).UseSecondaryColor(Color.HotPink);
                GameShaders.Misc["LCWingShader"].Apply(new DrawData?());
                starDraw.Draw(spriteBatch);


                spriteBatch.End(); spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);
            }
            float PyramidRot = 0;
            if (NPC.velocity.ToRotation() !> MathHelper.Pi)
            {
                PyramidRot = 0f + MathHelper.Pi * NPC.velocity.X / 300;
            }
            if (SpritePhase == 1 && Draw || NPC.IsABestiaryIconDummy) //whole pyramid
            {
                Texture2D pyramid = ModContent.Request<Texture2D>(PartsPath + "Phase1", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                Rectangle rect = new(0, 0, pyramid.Width, pyramid.Height);
                Vector2 origin = pyramid.Size() / 2;
                Vector2 wobble = new Vector2((float)Math.Sin(MathHelper.ToRadians(5.632167f /*purposefully random weird number*/ * DrawTime)), (float)Math.Sin(MathHelper.ToRadians(3 * DrawTime))) * PyramidWobble;
                spriteBatch.Draw(origin: origin, texture: pyramid, position: NPC.Center + wobble - screenPos, sourceRectangle: rect, color: drawColor, rotation: PyramidRot, scale: NPC.scale, effects: SpriteEffects.None, layerDepth: 0f);
            }
            else if (Draw) //broken pyramid
            {
                Texture2D[] pyramidp = new Texture2D[4];
                Rectangle[] rects = new Rectangle[4];
                Vector2[] origins = new Vector2[4];
                Vector2[] offsets = new Vector2[4];

                pyramidp[0] = ModContent.Request<Texture2D>(PartsPath + "Phase2U", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                pyramidp[1] = ModContent.Request<Texture2D>(PartsPath + "Phase2L", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                pyramidp[2] = ModContent.Request<Texture2D>(PartsPath + "Phase2R", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                pyramidp[3] = ModContent.Request<Texture2D>(PartsPath + "Phase2D", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                float expansion = ChunkDistance / ChunkDistanceMax;
                float P = (float)Math.Sqrt(SpritePhase - 1); //1 in p2, sqrt2 in p3, this doesn't draw in p1
                offsets[0] = new Vector2(0, -15) * (float)Math.Abs(Math.Sin(MathHelper.ToRadians(DrawTime * P))) * expansion + new Vector2(0, -30); //top
                offsets[1] = new Vector2(-12.5f, 3) * (float)Math.Abs(Math.Sin(MathHelper.ToRadians(DrawTime * P))) * expansion + new Vector2(-25, 10) * P; //left
                offsets[2] = new Vector2(12.5f, 3) * (float)Math.Abs(Math.Sin(MathHelper.ToRadians(DrawTime * P))) * expansion + new Vector2(25, 10) * P; //right
                offsets[3] = new Vector2(0, 10) * (float)Math.Abs(Math.Sin(MathHelper.ToRadians(DrawTime * P))) * expansion + new Vector2(0, 20) * P;  //bottom

                for (int i = 0; i < 4; i++)
                {
                    rects[i] = new Rectangle(0, 0, pyramidp[i].Width, pyramidp[i].Height);
                    origins[i] = pyramidp[i].Size() / 2;
                    offsets[i] = offsets[i].RotatedBy(PyramidRot);

                    spriteBatch.Draw(origin: origins[i], texture: pyramidp[i], position: NPC.Center + offsets[i] - screenPos, sourceRectangle: rects[i], color: drawColor, rotation: PyramidRot, scale: NPC.scale, effects: SpriteEffects.None, layerDepth: 0f);
                }


            }

            DrawTime++;
        }

        public override void FindFrame(int frameHeight)
        {
            //Texture2D wingUtexture = ModContent.Request<Texture2D>(PartsPath + "LifeChallenger_WingUpper", ReLogic.Content.AssetRequestMode.DoNotLoad).Value;
            double fpf = 60 / 10; //  60/fps
            NPC.spriteDirection = NPC.direction;
            NPC.frameCounter += 1;
            NPC.frameCounter %= Main.npcFrameCount[NPC.type] * fpf;
            NPC.frame.Y = (int)(NPC.frameCounter / fpf);
        }
        public override void OnKill()
        {
            NPC.SetEventFlagCleared(ref WorldSavingSystem.DownedBoss[(int)WorldSavingSystem.Downed.LifeChallenger], -1);
        }
        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ItemID.GreaterHealingPotion;
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<LifeChallengerBag>()));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<LifeChallengerTrophy>(), 10));

            npcLoot.Add(ItemDropRule.MasterModeCommonDrop(ModContent.ItemType<LifeChallengerRelic>()));
            npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<LifelightMasterPet>(), 4));

            LeadingConditionRule rule = new(new Conditions.NotExpert());
            rule.OnSuccess(ItemDropRule.OneFromOptions(1, ModContent.ItemType<EnchantedLifeblade>(), ModContent.ItemType<Lightslinger>(), ModContent.ItemType<CrystallineCongregation>(), ModContent.ItemType<KamikazePixieStaff>()));
            rule.OnSuccess(ItemDropRule.Common(ItemID.HallowedFishingCrateHard, 1, 1, 5)); //hallowed crate
            rule.OnSuccess(ItemDropRule.Common(ItemID.SoulofLight, 1, 1, 3));
            rule.OnSuccess(ItemDropRule.Common(ItemID.PixieDust, 1, 15, 25));

            npcLoot.Add(rule);
        }
        #endregion
        #region Help Methods
        public bool FlightCheck()
        {
            if (WorldSavingSystem.MasochistModeReal)
                return false;

            if (++flyTimer < (WorldSavingSystem.EternityMode ? 90 : 120))
            {
                float speed = WorldSavingSystem.EternityMode ? 1.2f : 0.8f;
                FlyingState(speed);
                return true;
            }
            return false;
        }

        public void P1stateReset()
        {
            AI_Timer = 0f;
            NPC.ai[2] = 0f;
            NPC.ai[3] = 0f;
            AttackF1 = true;
            NPC.netUpdate = true;
        }

        public void RandomizeP1state()
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                P1state = Main.rand.Next(P1statecount);
                if (P1state == oldP1state)
                    P1state = (P1state + 1) % P1statecount;
            }

            if (NPC.life < P2Threshold) //phase 2 switch
            {
                P1state = -1;
                flyTimer = 9000;
            }

            NPC.netUpdate = true;
        }
        public void StateReset()
        {
            NPC.TargetClosest(true);
            NPC.netUpdate = true;
            RandomizeState();
            AI_Timer = 0f;
            NPC.ai[2] = 0f;
            NPC.ai[3] = 0f;
            NPC.ai[0] = 0f;
            NPC.localAI[1] = 0;
            AttackF1 = true;
        }

        public void RandomizeState() //it's done this way so it cycles between attacks in a random order: for increased variety
        {
            if (availablestates.Count < 1)
            {
                availablestates.Clear();
                for (int j = 0; j < statecount; j++)
                {
                    availablestates.Add(j);
                }
            }
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                index = Main.rand.Next(availablestates.Count);
                state = availablestates[index];

                if ((state == 2 && oldstate == 4) || (state == 4 && oldstate == 2)) //cannot follow pixie dash into normal dash or vice versa
                {
                    state = 5 + Main.rand.Next(4);
                }

                availablestates.Remove(state);

            }

            Variant = Main.rand.NextBool();
            /*
            if (!PhaseThree && NPC.life < P3Threshold)
			{
				state = 100;
			}
            */

            if (!PhaseOne && NPC.life < SansThreshold)
            {
                state = 101;
                oldstate = -665;
            }

            if (first)
            {
                state = 0;
                availablestates.Remove(0);
                first = false;
                oldstate = -666;
            }
            NPC.netUpdate = true;
        }

        private static List<int> InitializeSpriteList(int countSprites, int count)
        {
            List<int> lst = new();

            for (int j = 1; j <= countSprites; j++)
            {
                lst.Add(j);
            }

            //shuffle
            int n = lst.Count;
            while (n > 1)
            {
                n--;
                int k = Main.rand.Next(n + 1);
                (lst[n], lst[k]) = (lst[k], lst[n]);
            }

            //delete so we have correct amount
            if (countSprites - count > 0)
            {
                for (int k = 0; k < countSprites - count; k++)
                {
                    lst.RemoveAt(k);
                }
            }
            return lst;
        }

        /*
        private Vector2 Trianglinator(int chunk, Vector2 screenPos)
        {
            //determine positions for each chunk, depending on their rotation (evenly spread), to form a triangle
            //orth are lines with angles orthogonal to triangle sides, used with sec (1/cos) to find length from center to place on triangle side
            float OrthLineLength = (NPC.width / 2) * 0.9f * 0.5f; //triangle vertex distance from center * cos(60deg)
            float drawRot = (float)(ChunkTriangleInnerRotation + (Math.PI * 2 / ChunkCount * chunk));
            float RotDif;
            float RotDeg = MathHelper.ToDegrees(drawRot);

            float Orth1 = 60;
            float Orth2 = 180;
            //orth3 is the "else"

            if (RotDeg % 360 >= Orth1 - 60 && RotDeg % 360 < Orth1 + 60)
            {
                RotDif = MathHelper.ToRadians(RotDeg - 60);
            }
            else if (RotDeg % 360 >= Orth2 - 60 && RotDeg % 360 < Orth2 + 60)
            {
                RotDif = MathHelper.ToRadians(RotDeg - 180);
            }
            else
            {
                RotDif = MathHelper.ToRadians(RotDeg - 300);
            }
            //pos = center + rot * (r / 2) * sec(rotdif)
            return NPC.Center + (drawRot + ChunkTriangleOuterRotation).ToRotationVector2() * (float)(OrthLineLength / Math.Cos(RotDif)) - screenPos; //the trianglinator
        }
        */
        #endregion
    }
}
