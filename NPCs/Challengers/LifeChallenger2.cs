using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.Chat;
using System.Linq;
using System.Collections.Generic;
using Terraria.Graphics.Effects;
using Terraria.DataStructures;
using FargowiltasSouls;
using FargowiltasSouls.Buffs.Boss;
using FargowiltasSouls.Projectiles.Challengers;
using Terraria.Graphics.Shaders;

namespace FargowiltasSouls.NPCs.Challengers
{

    [AutoloadBossHead]
    public class LifeChallenger2 : ModNPC
    {
        #region Variables
        public double Phase;

        private bool TooFar;

        private bool firsttime1 = true;

        private bool flyfast;

        private bool Flying = true;

        private bool Charging = false;

        private bool talk;

        private int Attacking = -1;

        private bool PhaseThree;

        private bool LoseDialogue;

        private int dustcounter;

        private int state;

        private int oldstate = 999;

        private int statecount = 8;

        private bool shoot = false;

        private List<int> availablestates = new List<int>(0);

        private List<int> choicelist = new List<int>(0);

        private Vector2 LockVector2 = new Vector2(0, 0);

        public Vector2 LockVector1 = new Vector2(0, 0);

        private float choice;

        private int oldchoice = 999;

        private int index;

        private int npcIndex = -1;

        private double rotspeed = 0;

        private double rot = 0;

        private bool dodebuff = true;

        private bool HitPlayer = false;

        int interval = 0;

        int index2;

        int firstblaster = 2;

        private bool resigned = false;

        private bool ShutTheFuckUpNerd = false;

        float BodyRotation = 0;

        public float SPR = 0.5f;

        private List<int> intervalist = new List<int>(0);
        #endregion
        #region Standard
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Lifelight");
            Main.npcFrameCount[NPC.type] = 8;
            NPCID.Sets.TrailCacheLength[NPC.type] = 18;
            NPCID.Sets.TrailingMode[NPC.type] = 1;
        }
        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.lifeMax = 3000;
            NPC.defense = 0;
            NPC.damage = 55;
            NPC.knockBackResist = 0f;
            NPC.width = 150;
            NPC.height = 150;
            NPC.boss = true;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit5;
            NPC.DeathSound = SoundID.NPCDeath7;
            Music = ModLoader.TryGetMod("FargowiltasMusic", out Mod musicMod)
                ? MusicLoader.GetMusicSlot(musicMod, "Assets/Music/Champions") : MusicID.OtherworldlyBoss1; //Throne
            if (PhaseThree)
            {
                Music = ModLoader.TryGetMod("FargowiltasMusic", out Mod musicMod2)
                ? MusicLoader.GetMusicSlot(musicMod2, "Assets/Music/Champions") : MusicID.OtherworldlyBoss1; //Father
            }
            SceneEffectPriority = SceneEffectPriority.BossHigh;
            NPC.value = Item.buyPrice(0, 10);
        }

        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * bossLifeScale);
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(state);
            writer.Write(oldstate);
            writer.Write(choice);
            writer.Write(index);
            writer.Write(index2);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            state = reader.ReadInt32();
            oldstate = reader.ReadInt32();
            choice = reader.ReadInt32();
            index = reader.ReadInt32();
            index2 = reader.ReadInt32();
        }
        #endregion
        #region AI
        public override void AI()
        {
            //Defaults
            /*SoundStyle VineBoom = new SoundStyle("VoidDungEonMod/Sounds/Item/vineboom");
			SoundStyle gasterA = new SoundStyle("VoidDungEonMod/Sounds/Item/gasta_appears") with
			{
				Volume = 0.75f,
			};*/
            Player Player = Main.player[NPC.target];
            Main.time = 27000; //noon
            Main.dayTime = true;
            BodyRotation += (float)((Math.PI / 30f) / SPR); //divide by sec/rotation

            //Find index of this NPC
            npcIndex = -1;
            for (int i = 0; i < 200; i++)
            {
                if (Main.npc[i] == NPC)
                {
                    npcIndex = i;
                }
            }

            //Checks
            if (PhaseThree)
            {
                Music = ModLoader.TryGetMod("FargowiltasMusic", out Mod musicMod3)
                ? MusicLoader.GetMusicSlot(musicMod3, "Assets/Music/Champions") : MusicID.OtherworldlyBoss1; //Father
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
            if (!LoseDialogue && Player.dead)
            {
                UtterWordsRed("");
                LoseDialogue = true;
            }

            //Aura, may rework to push you in instead of damaging you if outside, if you want
            if (dustcounter > 5 && dodebuff)
            {
                for (int l = 0; l < 180; l++)
                {
                    double rad2 = 2.0 * (double)l * (Math.PI / 180.0);
                    double dustdist2 = 1200.0;
                    int DustX2 = (int)NPC.Center.X - (int)(Math.Cos(rad2) * dustdist2);
                    int DustY2 = (int)NPC.Center.Y - (int)(Math.Sin(rad2) * dustdist2);
                    Dust.NewDust(new Vector2(DustX2, DustY2), 1, 1, DustID.GemTopaz);
                }
                dustcounter = 0;
            }
            dustcounter++;
            float distance = NPC.Distance(Player.Center);
            if (distance > 1200f && !TooFar && dodebuff)
            {
                if (!firsttime1 && !Player.dead)
                {
                    UtterWordsRed("");
                }
                TooFar = true;
                firsttime1 = false;
            }
            if (distance > 1200f && dodebuff)
            {
                Player.AddBuff(ModContent.BuffType<FadingSoul>(), 10);
            }
            if (TooFar && distance < 1200f)
            {
                TooFar = false;
            }

            //Targeting
            if (!Player.active || Player.dead)
            {
                Retarget(false);
                Player = Main.player[NPC.target];
                if (!Player.active || Player.dead)
                {
                    NPC.velocity = new Vector2(0f, 10f);
                    if (NPC.timeLeft > 10)
                    {
                        NPC.timeLeft = 10;
                    }
                    return;
                }
            }
            if (Phase == 0.0)
            {
                Retarget(true);
                Phase = 0.5;
            }
            /*//Intro Phase
			if (Phase == 0.0 && NPC.ai[1] > 120f)
			{
				if (!Stomped)
				{
					Retarget(true);
					EntranceStomp();
				}
				if (NPC.ai[1] == 135f)
				{
					EntranceStomp();
					Phase = 0.5;
					Stomped = false;
				}
			}
			if (Phase == 0.5 && NPC.ai[1] > 160f)
			{
				UtterWordsRed("IT WAS A MISTAKE TO CUM HERE.");
				Phase = 1.0;
			}
			if (Phase == 1.0 && NPC.ai[1] > 325f)
			{
				if (!Stomped)
				{
					EntranceStomp();
				}
				if (NPC.ai[1] == 340f)
				{
					EntranceStomp();
					Phase = 1.5;
					Stomped = false;
				}
			}
			if (Phase == 1.5 && NPC.ai[1] > 365f)
			{
				UtterWordsWhite("YOUR CUM SUCKS DICK.");
				Phase = 2.0;
			}
			if (Phase == 2.0 && NPC.ai[1] > 530f)
			{
				if (!Stomped)
				{
					EntranceStomp();
				}
				if (NPC.ai[1] == 545f)
				{
					EntranceStomp();
					Phase = 2.5;
					Stomped = false;
				}
			}
			if (Phase == 2.5 && NPC.ai[1] > 570f)
			{
				UtterWordsWhite("A SPLASH OF CUM TO SEAL THE DEAL.");
				Phase = 3.0;
			}
			if (Phase == 3.0 && NPC.ai[1] > 735f)
			{
				if (!Stomped)
				{
					EntranceStomp();
				}
				if (NPC.ai[1] == 750f)
				{
					EntranceStomp();
					Phase = 3.5;
					Stomped = false;
				}
			}
			if (Phase == 3.5 && NPC.ai[1] > 775f)
			{
				UtterWordsRed("COUNCIL OF CUM.");
				Phase = 4.0;
				NPC.dontTakeDamage = false;
				NPC.netUpdate = true;
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X - 464, (int)NPC.Center.Y, ModContent.NPCType<SansUndertale>(), NPC.whoAmI);
					NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X + 400, (int)NPC.Center.Y, ModContent.NPCType<MargaretThatcher>(), NPC.whoAmI);
					NPC.ai[1] = 0;
				}
			}*/
            if (Phase < 4) //Initial Attack
            {
                AttackP2Start();
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
                    NPC.rotation = NPC.velocity.ToRotation() + (float)Math.PI / 2;
                    if (NPC.velocity == Vector2.Zero)
                    {
                        NPC.rotation = 0f;
                    }
                }
                if (!Charging && !Flying) //standard upright orientation
                {
                    NPC.rotation = 0f;
                }
                if (Attacking == 1) //Phases and random attack choosing
                {
                    if (Phase == 4.0)
                    {
                        NPC.ai[1] = 0f;
                        Phase = 5.0;
                        StateReset();
                    }
                    if (state == oldstate) //ensure you never get the same attack twice (might happen when the possible state list is refilled)
                    {
                        RandomizeState();
                        if (!PhaseThree && NPC.life < NPC.lifeMax / 18)
                        {
                            state = 100;
                        }
                        if (PhaseThree && NPC.life < NPC.lifeMax / 8)
                        {
                            state = 99;
                            oldstate = 0;
                        }
                    }
                    if (state != oldstate)
                    {
                        switch (state) //Attack Choices
                        {
                            case 0: //slurp n burp attack
                                AttackSlurpBurp();
                                break;
                            case 1: //cum shotgun attack
                                AttackShotgun();
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
                            case 8: //bullet hell from sky attack
                                AttackRain();
                                break;
                            case 9: //teleport on you -> shit nukes
                                AttackTeleportNukes();
                                break;
                            case 99: //CUM SCYTHE attack (big fake spinny projectile with real damaging lingering spinning scar projectiles and invisible damaging projectiles under it covering the entirety)
                                AttackP3Start();
                                break;
                            case 100: //phase 3 transition
                                {
                                    P3Transition();
                                    break;
                                }
                            case 101: // Life is a cage, and death is the key.
                                {
                                    AttackFinal();
                                    break;
                                }
                        }
                    }
                }
            }
            NPC.ai[1] += 1f;
        }
        #endregion
        #region States
        public void FlyingState()
        {
            Player Player = Main.player[NPC.target];
            //flight AI
            float flySpeed = 0f;
            float inertia = 10f;
            Vector2 AbovePlayer = new Vector2(Player.Center.X, Player.Center.Y - 400f);
            if (state == 8)
            {
                AbovePlayer.Y = Player.Center.Y - 700f;
            }
            bool Close = ((Math.Abs(AbovePlayer.Y - NPC.Center.Y) < 25f && Math.Abs(AbovePlayer.X - NPC.Center.X) < 300f) ? true : false);
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
            if (NPC.Distance(AbovePlayer) > 500f)
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
            if (NPC.velocity.ToRotation() > Math.PI)
            {
                NPC.rotation = 0f - (float)Math.PI * NPC.velocity.X / 100;
            }
            else
            {
                NPC.rotation = 0f + (float)Math.PI * NPC.velocity.X / 100;
            }
        }
        public void AttackP2Start()
        {
            Player Player = Main.player[NPC.target];
            NPC.velocity.X = 0f;
            NPC.velocity.Y = 0f;
            float ProjectileSpeed = 16f;
            if (NPC.ai[1] == 0)
            {
                NPC.ai[0] = Main.rand.NextBool(2) ? 55 : -55;
                NPC.netUpdate = true;
                rotspeed = 0;
            }
            if (NPC.ai[1] == 60f)
            {
                LockVector1 = (NPC.DirectionTo(Player.Center) * ProjectileSpeed).RotatedBy(MathHelper.ToRadians(NPC.ai[0]));
                NPC.netUpdate = true;
                SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Zombie_104") with { Volume = 0.5f }, NPC.Center);
            }
            if (NPC.ai[1] > 60f)
            {
                //this is unnecessarily complicated but works and i personally advise you shouldn't touch it
                Vector2 PV = NPC.DirectionTo(Player.Center);
                Vector2 LV = LockVector1;
                float anglediff = (float)(Math.Atan2(PV.Y * LV.X - PV.X * LV.Y, LV.X * PV.X + LV.Y * PV.Y)); //between 0 and pi depending on angle between player and laser
                float RotAccel = 0.008f * anglediff;
                float rotMinSpeed = 0.15f; //very important
                float rotMaxSpeed = 0.8f * anglediff + rotMinSpeed * Math.Sign(anglediff);
                //change rotation towards player
                LockVector1 = LockVector1.RotatedBy(rotspeed * Math.PI / 180);
                if (rotspeed > Math.Abs(rotMaxSpeed) || rotspeed < -Math.Abs(rotMaxSpeed))
                {
                    rotspeed = rotMaxSpeed;
                }
                else
                {
                    rotspeed += RotAccel + (rotMinSpeed * Math.Sign(RotAccel));
                }
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Normalize(LockVector1),
                                ModContent.ProjectileType<LifeChalDeathray>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, /*RotDirSpeed <= 0 ? -(float)rotspeed : (float)rotspeed*/0, NPC.whoAmI);
                }
            }
            if (NPC.ai[3] > 55f)
            {
                SoundEngine.PlaySound(SoundID.Item17, NPC.Center);

                NPC.netUpdate = true;
                int amount = 6;
                for (int m = 0; m <= amount; m++)
                {
                    float knockBack9 = 3f;
                    double rad5 = (double)m * (360 / amount) * (Math.PI / 180.0);
                    Vector2 shootoffset5 = new Vector2(0f, 3f).RotatedBy(rad5);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, shootoffset5, ModContent.ProjectileType<LifeBee>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), knockBack9, Main.myPlayer, 0, NPC.ai[1]);
                    }
                }
                NPC.ai[3] = 0f;

            }
            NPC.ai[3] += 1f;
            if (NPC.ai[1] > 775f)
            {
                Phase = 4.0;
                NPC.ai[2] = 0;
                NPC.ai[3] = 0;
                NPC.dontTakeDamage = false;
                NPC.netUpdate = true;
            }
        }
        public void P3Transition()
        {
            Player Player = Main.player[NPC.target];
            NPC.defense = 999999;
            if (talk && !PhaseThree)
            {
                UtterWordsWhite("");
                talk = false;
                NPC.netUpdate = true;
            }
            Music = MusicLoader.GetMusicSlot(Mod, "Sounds/Music/Champions");
            int heal = (int)((float)(NPC.lifeMax / 100) * Main.rand.NextFloat(1f, 1.5f));
            NPC.life += heal;
            if (NPC.life > NPC.lifeMax)
            {
                NPC.life = NPC.lifeMax;
            }
            CombatText.NewText(NPC.Hitbox, CombatText.HealLife, heal);
            statecount = 10;
            availablestates.Clear();
            if (NPC.ai[1] > 120f)
            {
                talk = true;
                if (talk && !PhaseThree)
                {
                    UtterWordsRed("");
                    SoundEngine.PlaySound(SoundID.Item82, Main.LocalPlayer.Center);
                    Main.LocalPlayer.GetModPlayer<FargoSoulsPlayer>().Screenshake += 240;
                    talk = false;
                    PhaseThree = true;
                    NPC.netUpdate = true;
                }
            }
            if (NPC.ai[1] > 180f)
            {
                Retarget(true);
                NPC.defense = 0;
                NPC.netUpdate = true;
                state = 99;
                NPC.ai[1] = 0f;
                NPC.ai[2] = 0f;
                NPC.ai[3] = 0f;
                NPC.ai[0] = 0f;
                talk = true;
            }
        }
        public void AttackP3Start()
        {
            Player Player = Main.player[NPC.target];
            if (talk)
            {
                SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Zombie_104") with { Volume = 0.5f }, NPC.Center);
                UtterWordsRed("");
                talk = false;
                NPC.velocity.X = 0;
                NPC.velocity.Y = 0;
                Flying = false;
                NPC.defense = 99999;
                NPC.netUpdate = true;
                rotspeed = 0;
            }
            //for a starting time, make it fade in, then make it spin faster and faster up to a max speed
            int fadeintime = 10;
            if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[1] < fadeintime)
            {
                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(0f, -1f), ModContent.ProjectileType<LifeScythe>(), 0, 0, Main.myPlayer, 0, NPC.ai[1]);
            }
            if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[1] >= fadeintime)
            {
                if (rotspeed < 1.1f)
                {
                    rotspeed += (double)((2f / 60) / 4);
                }
                rot += ((Math.PI / 180) * rotspeed);
                Vector2 rotV = new Vector2(0f, -1f).RotatedBy(rot);
                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, rotV, ModContent.ProjectileType<LifeScythe>(), 0, 0, Main.myPlayer);
                //randomly make CumScar obstacles at specific points, obstacles have Projectile.ai[1] = NPC.ai[1]
                if (NPC.ai[1] % 6 == 0 && Main.netMode != NetmodeID.MultiplayerClient && rotspeed > 1f)
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

                    NPC.ai[0] = Main.rand.Next(200);
                    int dist = (200 * interval) + (int)NPC.ai[0];
                    Vector2 distV = NPC.Center - new Vector2(0f, dist).RotatedBy(rot);
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), distV, Vector2.Zero, ModContent.ProjectileType<LifeScar>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer, 0, NPC.ai[1]);
                }
                //invisible execution projectiles under scythe
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int i = 0; i < 50; i++)
                    {
                        int dist = (24 * i);
                        Vector2 distV = NPC.Center - new Vector2(0f, dist).RotatedBy(rot);
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), distV, Vector2.Zero, ModContent.ProjectileType<InvisibleScytheHitbox>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage * 50), 3f, Main.myPlayer);
                    }
                }

            }
            if (NPC.ai[1] > 1200f)
            {
                NPC.defense = 0;
                Flying = true;
                oldstate = state;
                StateReset();
                rotspeed = 0;
                rot = 0;
            }
        }
        public void AttackFinal()
        {
            Player Player = Main.player[NPC.target];
            if (talk) //first he silences you and gets 99999 defense
            {
                UtterWordsRed("");
                talk = false;
                ShutTheFuckUpNerd = true;
                Main.LocalPlayer.AddBuff(BuffID.Cursed, 120 * 60);
                NPC.defense = 99999;
                NPC.netUpdate = true;

            }
            for (int i = 0; i < Main.musicFade.Length; i++) //shut up music
                if (Main.musicFade[i] > 0f)
                    Main.musicFade[i] -= 1f / 60;
            
            if (NPC.ai[1] < 240 && Main.netMode != NetmodeID.MultiplayerClient) // cage size is 600x600, 300 from center, 25 projectiles per side, 24x24 each
            {
                Projectile.NewProjectile(NPC.GetSource_FromThis(), Player.Center, Vector2.Zero, ModContent.ProjectileType<LifeCageTelegraph>(), 0, 0f, Main.myPlayer);
            }
            if (NPC.ai[1] == 240)
            {
                SoundEngine.PlaySound(SoundID.DD2_DefenseTowerSpawn, Player.Center);
                UtterWordsRed("");
                for (int i = 0; i < 26; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), new Vector2(Player.Center.X - 300 + (600 * j), Player.Center.Y - 300 + (24 * i)), Vector2.Zero, ModContent.ProjectileType<LifeCageProjectile>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer, j);
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), new Vector2(Player.Center.X - 300 + (24 * i), Player.Center.Y - 300 + (600 * j)), Vector2.Zero, ModContent.ProjectileType<LifeCageProjectile>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer, 2 + j);
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

            if (NPC.ai[1] > 240) //make sure to teleport any player outside the cage inside
            {
                if ((Main.LocalPlayer.active && ((Math.Abs(Main.LocalPlayer.Center.X - LockVector1.X) > 320) || (Math.Abs(Main.LocalPlayer.Center.Y - LockVector1.Y) > 320))) && (Main.LocalPlayer.active && ((Math.Abs(Main.LocalPlayer.Center.X - LockVector1.X) < 1500) || (Math.Abs(Main.LocalPlayer.Center.Y - LockVector1.Y) < 1500))))
                {
                    Main.LocalPlayer.position = LockVector1;
                }
            }
            //attack 1: arena is divided in 9 squares, only 1 is safe
            #region GridShots
            const int Attack1Time = 80;
            const int Attack1Start = 300;
            const int Attack1End = Attack1Start + (Attack1Time * 5);
            const int telegdist = 175;
            int time1 = (int)NPC.ai[1] - Attack1Start;

            if (NPC.ai[1] > Attack1Start && time1 % Attack1Time == 0 && NPC.ai[1] < Attack1End) // get random choice
            {
                SoundEngine.PlaySound(SoundID.Unlock, Player.Center);
                NPC.ai[0] = Main.rand.Next(3);
                NPC.ai[2] = Main.rand.Next(3);
                NPC.netUpdate = true;
            }
            if (NPC.ai[1] > Attack1Start && time1 % Attack1Time > 0 && NPC.ai[1] < Attack1End) // chosen telegraphs (all but the Chosen One)
            {
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient && (i != NPC.ai[0] || j != NPC.ai[2]))
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), new Vector2(LockVector1.X + (telegdist * (i - 1)), LockVector1.Y + (telegdist * (j - 1))), Vector2.Zero, ModContent.ProjectileType<LifeCrosshair>(), 0, 0, Main.myPlayer);
                        }
                    }
                }
            }
            if (NPC.ai[1] > Attack1Start && time1 % Attack1Time == Attack1Time - 1 && NPC.ai[1] <= Attack1End) //shoot
            {

                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        SoundEngine.PlaySound(SoundID.Item41, LockVector1);
                        if (Main.netMode != NetmodeID.MultiplayerClient && (i != NPC.ai[0] || j != NPC.ai[2]))
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), new Vector2(LockVector1.X + (telegdist * (i - 1)), LockVector1.Y + (telegdist * (j - 1))), Vector2.Zero, ModContent.ProjectileType<LifeCageExplosion>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer);
                        }
                    }
                }
            }
            //end of square attack
            #endregion
            #region BulletHell
            //start of shooting attack: cum god fires a nuke or two straight up while he shoots slow shots straight down from him
            const int Attack2Time = 25;
            const int Attack2Start = Attack1End + 60;
            const int Attack2End = Attack2Start + (60 * 8);
            int time2 = (int)NPC.ai[1] - Attack2Start;

            if (NPC.ai[1] > Attack2Start && time2 % (Attack2Time * 3) + 1 == 1 && NPC.ai[1] < Attack2End) //cum nuke up
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    for (int i = 0; i < 2; i++)
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(-4 + (8 * i), -2f), ModContent.ProjectileType<LifeNuke>(), 2 * FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer);
            }
            if (NPC.ai[1] > Attack2Start && time2 % Attack2Time + 1 == Attack2Time && NPC.ai[1] < Attack2End) //fire shots down
            {
                SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, NPC.Center);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(0, 4f), ModContent.ProjectileType<LifeProjLarge>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer);
            }
            #endregion
            #region Jevilsknife
            //const int Attack3Time = 60;
            const int Attack3Start = Attack2End + 300;
            const int Attack3End = Attack3Start + (60 * 12);
            int time3 = (int)NPC.ai[1] - Attack3Start;
            if (NPC.ai[1] >= Attack3Start && NPC.ai[1] < Attack3End)
                NPC.position = LockVector1 + new Vector2(-(NPC.width / 2), -600 - (NPC.height / 2)); //position self above cage

            if (NPC.ai[1] == Attack3Start) // get random
            {
                NPC.ai[0] = Main.rand.Next(90);
                NPC.ai[2] = Main.rand.Next(2); //random which 2 spots the aimed shots start at
                //position of knife: start pos + spin speed (1.75) * time since start, offset by 45 deg to get space in between them
                choice = NPC.ai[0] + 45;
                NPC.netUpdate = true;
            }
            if (NPC.ai[1] >= Attack3Start && time3 == 10) // spawn circle
            {
                SoundEngine.PlaySound(SoundID.Item71, LockVector1);
                Vector2 pos = LockVector1 - new Vector2(500, 0);
                Vector2 ScopeToPos = pos - LockVector1;
                for (int i = 0; i < 4; i++)
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), LockVector1 + ScopeToPos.RotatedBy((i * Math.PI / 2) + MathHelper.ToRadians(NPC.ai[0])), /*Vector2.Normalize(ScopeToPos.RotatedBy((Math.PI/2) + MathHelper.ToRadians(NPC.ai[0]) + (i * Math.PI / 2))) * 10f*/ Vector2.Zero, ModContent.ProjectileType<JevilScar>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer, npcIndex, i * 90 + NPC.ai[0]);
            }
            float spin = choice * (float)Math.PI / 180f;
            if (time3 >= 10 && NPC.ai[1] <= Attack3End - 20 && (time3 - 10) % 100 >= 1) //telegraphs (spinning with knife)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        for (int j = 1; j < 3; j++)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), LockVector1 + ((250 / j) * (choice * (float)Math.PI / 180f).ToRotationVector2()).RotatedBy(i * Math.PI), Vector2.Zero, ModContent.ProjectileType<LifeCrosshair>(), 0, 0, Main.myPlayer);
                        }
                    }
                }
            }
            if (time3 > 10 && (time3 - 10) % 100 == 0 && NPC.ai[1] <= Attack3End) //shoot when knives are at peak length
            {
                SoundEngine.PlaySound(SoundID.Item41, LockVector1);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        for (int j = 1; j < 3; j++)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), LockVector1 + ((250 / j) * (choice * (float)Math.PI / 180f).ToRotationVector2() / j).RotatedBy(i * Math.PI), Vector2.Zero, ModContent.ProjectileType<LifeCageExplosion>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer);
                        }
                    }
                }
            }
            if (time3 >= 60)
            {
                choice += 1.75f; // degrees/bounce rotation
            }
            if (NPC.ai[1] >= Attack3Start && time3 % 100 == 0 && NPC.ai[1] < Attack3End) // periodic sound
            {
                SoundEngine.PlaySound(SoundID.Item71, LockVector1);
            }
            // square walls from bottom and right
            #endregion
            #region Excel
            const int Attack4Time = 75;
            const int Attack4Start = Attack3End - 120; //start earlier so they overlap
            const int Attack4End = Attack4Start + (Attack4Time * 8) + 240;
            int time4 = (int)NPC.ai[1] - Attack4Start;
            if (NPC.ai[1] >= Attack4Start && time4 % Attack4Time + 1 == 1 && NPC.ai[1] < Attack4End - 240) // get random
            {
                NPC.ai[0] = Main.rand.Next(-60, 60);
                NPC.ai[2] = Main.rand.Next(-60, 60);
                NPC.netUpdate = true;
            }
            if (NPC.ai[1] >= Attack4Start && time4 % Attack4Time + 1 == Attack4Time && NPC.ai[1] < Attack4End - 240) // spawn walls
            {
                SoundEngine.PlaySound(SoundID.Item12, NPC.Center);
                for (int i = -10; i <= 10; i++)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), LockVector1 + new Vector2(1000, i * 120 + NPC.ai[0]), new Vector2(-3, 0), ModContent.ProjectileType<LifeProjSmall>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer, 0, 1);
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), LockVector1 + new Vector2(i * 120 + NPC.ai[2], 1000), new Vector2(0, -3), ModContent.ProjectileType<LifeProjSmall>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer, 0, 1);
                    }
                }
            }
            if (time4 == 200) //respawn cage because projectile limit
            {
                for (int i = 0; i < 26; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), new Vector2(LockVector1.X - 300 + (600 * j), LockVector1.Y - 300 + (24 * i)), Vector2.Zero, ModContent.ProjectileType<LifeCageProjectile>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer, j);
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), new Vector2(LockVector1.X - 300 + (24 * i), LockVector1.Y - 300 + (600 * j)), Vector2.Zero, ModContent.ProjectileType<LifeCageProjectile>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer, 2 + j);
                        }
                    }
                }
            }
            #endregion
            #region Blaster1
            //GASTER BLASTER 1
            const int Attack5Time = 90;
            const int Attack5Start = Attack4End + 60;
            const int Attack5End = Attack5Start + (Attack5Time * 8);
            int time5 = (int)NPC.ai[1] - Attack5Start;
            if (NPC.ai[1] >= Attack5Start && time5 % Attack5Time + 1 == 1 && NPC.ai[1] < Attack5End) // get random angle
            {
                NPC.ai[0] = Main.rand.Next(-90, 90);
                NPC.netUpdate = true;
            }
            if (NPC.ai[1] >= Attack5Start && time5 % Attack5Time + 1 == Attack5Time && NPC.ai[1] < Attack5End) // spawn blasters
            {
                Vector2 aim = new Vector2(0, 450);
                if (firstblaster < 1 || firstblaster > 1)
                    SoundEngine.PlaySound(SoundID.Item92, NPC.Center);
                for (int i = 0; i <= 12; i++)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient && (firstblaster < 1 || firstblaster > 1))
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), LockVector1 + aim.RotatedBy((i * Math.PI / 6) + MathHelper.ToRadians(NPC.ai[0])), -Vector2.Normalize(aim).RotatedBy((i * Math.PI / 6) + MathHelper.ToRadians(NPC.ai[0])) * 0.001f, ModContent.ProjectileType<LifeBlaster>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer, 0, firstblaster);
                    }
                }
                if (firstblaster > 0)
                    firstblaster -= 1;

            }
            #endregion
            #region Blaster2
            //GASTER BLASTER 2 FINAL BIG SPIN FINAL CUM GOD DONE DUN DEAL
            const int Attack6Time = 4;
            const int Attack6Start = Attack5End + 90;
            const int Attack6End = Attack6Start + (180 * 5); //2 seconds per rotation
            int time6 = (int)NPC.ai[1] - Attack6Start;
            if (NPC.ai[1] >= Attack6Start && time6 == 0) // reset NPC.ai[0]
            {
                NPC.ai[0] = 0;
                NPC.netUpdate = true;
            }

            if (NPC.ai[1] > Attack6Start && time5 % Attack6Time == Attack6Time - 1 && NPC.ai[1] < Attack6End) // spawn blasters. 1 every 4th frame, 2 seconds per rotation, 45 total
            {
                SoundEngine.PlaySound(SoundID.Item92, NPC.Center);
                Vector2 aim = new Vector2(0, 550);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), LockVector1 + aim.RotatedBy(NPC.ai[0] * Math.PI / 18), -Vector2.Normalize(aim).RotatedBy(NPC.ai[0] * Math.PI / 18) * 0.001f, ModContent.ProjectileType<LifeBlaster>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer);
                }
                NPC.ai[0] += 1;
            }
            #endregion
            #region End
            int end = Attack6End + 120;
            if (NPC.ai[1] == end)
            {
                UtterWordsWhite("");
                resigned = true;
                Main.LocalPlayer.ClearBuff(BuffID.Cursed);
            }
            if (NPC.ai[1] >= end)
            {
                resigned = true;
                NPC.defense = 0;
            }
            if (NPC.ai[1] == end + 240f)
            {
                UtterWordsWhite("");
            }
            #endregion
        }
        public void AttackSlurpBurp()
        {
            Player Player = Main.player[NPC.target];
            if (talk)
            {
                UtterWordsWhite("");
                talk = false;
                NPC.netUpdate = true;
            }
            NPC.velocity.X = 0f;
            NPC.velocity.Y = 0f;
            Flying = false;
            float knockBack = 3f;
            double rad = (double)NPC.ai[1] * 5.721237 * (Math.PI / 180.0);
            double dustdist = 1200.0;
            int DustX = (int)NPC.Center.X - (int)(Math.Cos(rad) * dustdist);
            int DustY = (int)NPC.Center.Y - (int)(Math.Sin(rad) * dustdist);
            Vector2 DustV = new Vector2(DustX, DustY);
            if (NPC.ai[2] >= 2f && NPC.ai[1] <= 300f)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), DustV, Vector2.Zero, ModContent.ProjectileType<LifeSlurp>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), knockBack, Main.myPlayer, 0, npcIndex);
                }
                
                NPC.ai[2] = 0f;
            }
            NPC.ai[2] += 1f;
            if (NPC.ai[1] < 300f)
            {
                if (NPC.ai[3] > 15f)
                {
                    SoundEngine.PlaySound(SoundID.Item101, DustV);

                    if (PhaseThree && shoot != false) //extra projectiles during p3
                    {
                        SoundEngine.PlaySound(SoundID.Item12, NPC.Center);
                        float ProjectileSpeed = 10f;
                        float knockBack2 = 3f;
                        Vector2 shootatPlayer = NPC.DirectionTo(Player.Center) * ProjectileSpeed;
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, shootatPlayer, ModContent.ProjectileType<LifeWave>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), knockBack2, Main.myPlayer);
                        }
                        shoot = false;
                    }
                    else
                    {
                        shoot = true;
                    }
                    NPC.ai[3] = 0f;
                }
                NPC.ai[3] += 1f;
            }
            if (NPC.ai[1] > 300f && NPC.ai[1] < 600f)
            {
                if (NPC.ai[3] > 15f)
                {
                    SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, NPC.Center);
                    NPC.ai[3] = 0f;
                }
                NPC.ai[3] += 1f;
            }
            if (NPC.ai[1] >= 660f)
            {
                oldstate = state;
                Flying = true;
                StateReset();
            }
        }
        public void AttackShotgun()
        {
            Player Player = Main.player[NPC.target];
            if (talk)
            {
                UtterWordsWhite("");
                talk = false;
                NPC.netUpdate = true;
                if (PhaseThree)
                {
                    SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
                }
            }
            if (PhaseThree)
            {
                Flying = false;
                float flySpeed2 = 8f;
                float inertia2 = 8f;
                Vector2 OnPlayer = new Vector2(Player.Center.X, Player.Center.Y);
                Vector2 flyonPlayer = NPC.DirectionTo(OnPlayer) * flySpeed2;
                NPC.velocity = (NPC.velocity * (inertia2 - 1f) + flyonPlayer) / inertia2;

                //rotation
                if (NPC.velocity.ToRotation() > Math.PI)
                {
                    NPC.rotation = 0f - (float)Math.PI * NPC.velocity.X / 100;
                }
                else
                {
                    NPC.rotation = 0f + (float)Math.PI * NPC.velocity.X / 100;
                }
            }
            if (NPC.ai[3] < 3f)
            {
                NPC.ai[3] = 3f;
            }
            if (NPC.ai[2] >= 120f)
            {
                SoundEngine.PlaySound(SoundID.Item12, NPC.Center);
                float ProjectileSpeed = 10f;
                float knockBack2 = 3f;
                Vector2 shootatPlayer = NPC.DirectionTo(Player.Center) * ProjectileSpeed;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int spread;
                    if (PhaseThree)
                    {
                        spread = 10;
                    }
                    else
                    {
                        spread = 18;
                    }
                    for (int i = 0; (float)i <= NPC.ai[3]; i++)
                    {
                        double rotationrad = MathHelper.ToRadians(0f - NPC.ai[3] * spread / 2 + (float)(i * spread));
                        Vector2 shootoffset = shootatPlayer.RotatedBy(rotationrad);
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, shootoffset, ModContent.ProjectileType<LifeWave>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), knockBack2, Main.myPlayer);
                    }
                }
                NPC.ai[3] += 1f;
                NPC.ai[2] = 80f;
            }
            NPC.ai[2] += 1f;
            if (NPC.ai[3] >= 12f)
            {
                Flying = true;
                oldstate = state;
                StateReset();
            }
        }
        public void AttackCharge()
        {
            Player Player = Main.player[NPC.target];
            if (talk)
            {
                UtterWordsWhite("");
                talk = false;
                //SoundEngine.PlaySound(VineBoom, NPC.Center); //PLACEHOLDER
                NPC.netUpdate = true;
                SoundEngine.PlaySound(SoundID.ScaryScream, NPC.Center);
                if (PhaseThree)
                {
                    dodebuff = false;
                }
            }
            Flying = false;
            Charging = true;
            HitPlayer = true;
            if (PhaseThree) //tp
            {
                if (NPC.ai[1] == 0f)
                {
                    NPC.ai[2] = Main.rand.Next(360);
                    if (NPC.ai[3] >= 6f)
                    {
                        NPC.ai[2] = 90f;
                    }
                }
                double rad3 = (double)NPC.ai[2] * (Math.PI / 180.0);
                double tpdist = 350.0;
                int TpX = (int)Player.Center.X - (int)(Math.Cos(rad3) * tpdist) - NPC.width / 2;
                int TpY = (int)Player.Center.Y - (int)(Math.Sin(rad3) * tpdist) - NPC.height / 2;
                Vector2 TpPos = new Vector2(TpX, TpY);
                if (NPC.ai[1] <= 70f && Main.netMode != NetmodeID.MultiplayerClient) //telegraph
                {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), new Vector2(TpPos.X + (float)(NPC.width / 2), TpPos.Y + (float)(NPC.height / 2)), Vector2.Zero, ModContent.ProjectileType<TpTelegraph>(), 0, 0f, Main.myPlayer);
                }
                if (NPC.ai[1] == 75f) //tp
                {
                    NPC.position.X = TpX;
                    NPC.position.Y = TpY;
                    NPC.velocity.X = 0f;
                    NPC.velocity.Y = 0f;
                    NPC.rotation = NPC.DirectionTo(Player.Center).ToRotation();
                    SoundEngine.PlaySound(SoundID.Item8, NPC.Center); //PLACEHOLDER
                    NPC.netUpdate = true;
                }
            }
            if (((NPC.ai[1] == 60f && Main.netMode != NetmodeID.MultiplayerClient && !PhaseThree) || (NPC.ai[1] == 80f && Main.netMode != NetmodeID.MultiplayerClient && PhaseThree)) && NPC.ai[3] < 6f)
            {
                SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
                SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, NPC.Center);
                //circle of cum before charge
                float ProjectileSpeed = 10f;
                Vector2 shootatPlayer = NPC.DirectionTo(Player.Center) * ProjectileSpeed;
                int amount = 14;
                for (int i = 0; i < amount; i++)
                {
                    Vector2 shootoffset = shootatPlayer.RotatedBy(i * (Math.PI / (amount / 2)));
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, shootoffset, ModContent.ProjectileType<LifeProjLarge>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer);
                }
                //charge
                float chargeSpeed = 22f;
                Vector2 chargeatPlayer = NPC.DirectionTo(Player.Center) * chargeSpeed;
                NPC.velocity = chargeatPlayer;
                NPC.ai[2] = Main.rand.Next(360);
                NPC.netUpdate = true;
                if (NPC.ai[3] >= 6f)
                {
                    NPC.ai[2] = 90f;
                }
                NPC.ai[1] = 0f;
                NPC.ai[3] += 1f;
            }
            if (!PhaseThree)
            {
                NPC.velocity = NPC.velocity * 0.99f;
            }
            if ((NPC.ai[3] >= 6f && NPC.ai[1] >= 75f && !PhaseThree) || (NPC.ai[3] >= 6f && NPC.ai[1] >= 105f && PhaseThree))
            {
                NPC.velocity.X = 0f;
                NPC.velocity.Y = 0f;
                HitPlayer = false;
                Flying = true;
                if (PhaseThree)
                {
                    dodebuff = true;
                }
                Charging = false;
                oldstate = state;
                StateReset();
            }
        }
        public void AttackPlunge()
        {
            Player Player = Main.player[NPC.target];
            if (talk)
            {
                UtterWordsWhite("");
                talk = false;
                NPC.netUpdate = true;
            }
            Vector2 TpPos2 = new Vector2(Player.Center.X - (float)(NPC.width / 2), Player.Center.Y - 400f - (float)(NPC.width / 2));
            if (NPC.ai[1] <= 40 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                Projectile.NewProjectile(NPC.GetSource_FromThis(), new Vector2(TpPos2.X + (float)(NPC.width / 2), TpPos2.Y + (float)(NPC.height / 2)), Vector2.Zero, ModContent.ProjectileType<TpTelegraph>(), 0, 0f, Main.myPlayer);
                //below wall telegraph
                Projectile.NewProjectile(NPC.GetSource_FromThis(), new Vector2(Player.Center.X, Player.Center.Y + 300f), Vector2.Zero, ModContent.ProjectileType<LifeCrosshair>(), 0, 0f, Main.myPlayer);
            }
            if (NPC.ai[1] == 45f)
            {
                Flying = false;
                Charging = true;
                NPC.position.X = TpPos2.X;
                NPC.position.Y = TpPos2.Y;
                NPC.velocity.X = 0f;
                NPC.velocity.Y = 0f;
                NPC.rotation = (float)Math.PI;
                SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
                NPC.netUpdate = true;
            }
            if (NPC.ai[1] == 60)
            {
                SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
                HitPlayer = true;
                float chargeSpeed2 = 55f;
                NPC.velocity.Y = chargeSpeed2;
                NPC.netUpdate = true;
                //below wall
                for (int i = 0; i < 120; i++)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), new Vector2(Player.Center.X - 1200, Player.Center.Y + 150f + (30 * i)), new Vector2(60, 0), ModContent.ProjectileType<LifeProjLarge>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer);
                }
                for (int i = 0; i < 120; i++)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), new Vector2(Player.Center.X + 1200, Player.Center.Y + 150f + (30 * i)), new Vector2(-60, 0), ModContent.ProjectileType<LifeProjLarge>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer);
                }
            }
            if (NPC.ai[1] >= 60)
            {
                HitPlayer = true;
                NPC.velocity = NPC.velocity * 0.96f;
            }
            if (NPC.ai[1] == 90 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                float knockBack4 = 3f;
                Vector2 shootdown2 = new Vector2(0f, 10f);
                SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, NPC.Center);
                for (int k = 0; k <= 15; k++)
                {
                    double rotationrad3 = MathHelper.ToRadians(-90 + k * 12);
                    Vector2 shootoffset3 = shootdown2.RotatedBy(rotationrad3);
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, shootoffset3, ModContent.ProjectileType<LifeNeggravProj>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), knockBack4, Main.myPlayer);
                }
            }
            if (NPC.ai[1] == 105 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                float knockBack3 = 3f;
                Vector2 shootdown = new Vector2(0f, 10f);
                SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, NPC.Center);
                for (int j = 0; j <= 15; j++)
                {
                    double rotationrad2 = MathHelper.ToRadians(-90 + j * 10);
                    Vector2 shootoffset2 = shootdown.RotatedBy(rotationrad2);
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, shootoffset2, ModContent.ProjectileType<LifeNeggravProj>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), knockBack3, Main.myPlayer);
                }
            }
            if (NPC.ai[1] == 110 && PhaseThree && NPC.ai[2] < 5f)
            {
                NPC.ai[1] = 0f;
                NPC.ai[2] = 10f;
            }
            if (NPC.ai[1] == 240)
            { //teleport back up
                NPC.position.X = Player.position.X;
                NPC.position.Y = Player.position.Y - 450f;
                //SoundEngine.PlaySound(VineBoom, NPC.Center); //PLACEHOLDER
                HitPlayer = false;
                Flying = true;
                Charging = false;
                NPC.netUpdate = true;
            }
            if (NPC.ai[1] >= 300)
            {
                HitPlayer = false;
                oldstate = state;
                StateReset();
            }
        }
        public void AttackPixies()
        {
            Player Player = Main.player[NPC.target];
            if (talk)
            {
                UtterWordsWhite("");
                talk = false;
                NPC.netUpdate = true;
                NPC.ai[3] = 0;
                if (PhaseThree)
                {
                    SoundEngine.PlaySound(SoundID.ScaryScream, NPC.Center);
                }
            }
            if (!PhaseThree) //p2 version
            {
                if (NPC.ai[2] > 60f && (NPC.ai[2] % 5) == 0)
                {
                    SoundEngine.PlaySound(SoundID.NPCDeath7, NPC.Center);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        float knockBack10 = 3f;
                        Vector2 shootoffset4 = new Vector2(0f, -5f).RotatedBy(NPC.ai[3]);
                        NPC.ai[3] = (float)(Main.rand.Next(-30, 30) * (Math.PI / 180.0)); //change random offset after so game has time to sync
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, shootoffset4, ModContent.ProjectileType<LifeHomingProj2>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), knockBack10, Main.myPlayer, 0f, npcIndex);
                    }
                }
                NPC.ai[2] += 1f;
                if (NPC.ai[1] > 280f)
                {
                    oldstate = state;
                    StateReset();
                }
            }
            if (PhaseThree) //p3 version
            {
                Flying = false;
                Charging = true;
                if (PhaseThree && NPC.ai[1] == 60)
                {
                    LockVector1 = Player.Center;
                    NPC.netUpdate = true;
                }
                const int ChargeCD = 60;
                if (NPC.ai[2] == ChargeCD) //charge
                {
                    SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
                    float chargeSpeed = 22f;
                    Vector2 chargeatPlayer = NPC.DirectionTo(Player.Center) * chargeSpeed;
                    NPC.velocity = chargeatPlayer;
                    NPC.netUpdate = true;
                }
                if ((NPC.ai[1] % 5) == 0 && NPC.ai[2] > ChargeCD && NPC.ai[2] < ChargeCD + 40) //fire pixies during charges
                {
                    SoundEngine.PlaySound(SoundID.NPCDeath7, NPC.Center);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        float knockBack10 = 3f;
                        Vector2 shootoffset4 = Vector2.Normalize(NPC.velocity).RotatedBy(NPC.ai[3]) * 5f;
                        NPC.ai[3] = (float)(Main.rand.Next(-30, 30) * (Math.PI / 180.0)); //change random offset after so game has time to sync
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, shootoffset4, ModContent.ProjectileType<LifeHomingProj2>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), knockBack10, Main.myPlayer, 0f, npcIndex);
                    }
                }
                if (NPC.ai[2] >= ChargeCD + 60)
                {
                    NPC.ai[2] = ChargeCD - 1;
                }
                NPC.ai[2]++;
                NPC.velocity *= 0.99f;
                if (NPC.ai[1] >= ChargeCD * 5)
                {
                    Flying = true;
                    Charging = false;
                    oldstate = state;
                    StateReset();
                }
            }


        }
        public void AttackRoulette()
        {
            Player Player = Main.player[NPC.target];
            if (talk)
            {
                UtterWordsWhite("");
                talk = false;
                Flying = false;
                NPC.velocity = Vector2.Zero;
                NPC.ai[3] = 1000 * (Main.rand.Next(2));
                NPC.netUpdate = true;

            }
            Vector2 RouletteTpPos = new Vector2((Player.position.X - 500) + NPC.ai[3], Player.position.Y - 500);
            if (NPC.ai[1] < 40 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                Projectile.NewProjectile(NPC.GetSource_FromThis(), RouletteTpPos, Vector2.Zero, ModContent.ProjectileType<TpTelegraph>(), 0, 0f, Main.myPlayer);
            }

            if (NPC.ai[1] == 40)
            {
                NPC.position.X = RouletteTpPos.X;
                NPC.position.Y = RouletteTpPos.Y;
                SoundEngine.PlaySound(SoundID.Item8, NPC.Center); //PLACEHOLDER
                LockVector1 = NPC.DirectionTo(Player.Center);
                NPC.netUpdate = true;
            }
            if (NPC.ai[1] < 420 && NPC.ai[1] % 4 == 0 && NPC.ai[1] > 60 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                Vector2 offset1 = LockVector1.RotatedBy(Math.PI / 3f) * 20f;
                Vector2 offset2 = LockVector1.RotatedBy(-Math.PI / 3f) * 20f;
                //in p3, rotate offsets by +-5 degrees determined by sine curve, one loop is 4 seconds
                if (PhaseThree)
                {
                    offset1 = offset1.RotatedBy((Math.PI / 6.5) * Math.Sin(MathHelper.ToRadians(1.5f * NPC.ai[1])));
                    offset2 = offset2.RotatedBy((Math.PI / 6.5) * -Math.Sin(MathHelper.ToRadians(1.5f * NPC.ai[1])));
                }


                float knockBackRo = 3f;
                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, offset1, ModContent.ProjectileType<LifeWave>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), knockBackRo, Main.myPlayer);
                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, offset2, ModContent.ProjectileType<LifeWave>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), knockBackRo, Main.myPlayer);
            }
            if (NPC.ai[1] < 420 && NPC.ai[1] % 60 == 10 && NPC.ai[1] > 60) // get Choice half a second before because terraria netcode fucking sucks
            {
                if (choicelist.Count < 1)
                {
                    choicelist.Clear();
                    for (int j = 0; j < 4; j++)
                    {
                        choicelist.Add(j);
                    }
                }
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    index = Main.rand.Next(choicelist.Count);
                    if (oldchoice != choicelist[index]) //can't get same attack twice in a row
                    {
                        choice = choicelist[index];
                        oldchoice = (int)choice;
                        choicelist.RemoveAt(index);
                    }
                    else
                    {
                        int index2 = index;
                        choicelist.RemoveAt(index);
                        index = Main.rand.Next(choicelist.Count);
                        choice = choicelist[index];
                        choicelist.Add(index2);
                        oldchoice = (int)choice;
                        choicelist.RemoveAt(index);
                    }

                }
                NPC.netUpdate = true;
            }
            if (NPC.ai[1] < 420 && NPC.ai[1] % 70 == 55 && NPC.ai[1] > 70) //fire a random assortment of things
            {
                switch (choice)
                {
                    case 0: //nuke

                        SoundEngine.PlaySound(SoundID.Item91, NPC.Center);
                        float knockBack10 = 300f;
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, NPC.DirectionTo(Player.Center) * 12f, ModContent.ProjectileType<LifeNuke>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage * 2), knockBack10, Main.myPlayer);
                        break;
                    case 1: //small random spread
                        SoundEngine.PlaySound(SoundID.Item12, NPC.Center);
                        for (int i = 0; i < 10; i++)
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                NPC.ai[0] = Main.rand.Next(8, 13);
                                NPC.ai[2] = Main.rand.Next(-40, 40);
                                Vector2 offsetRAND = (NPC.DirectionTo(Player.Center) * NPC.ai[0]).RotatedBy((Math.PI / 180) * NPC.ai[2]);
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, offsetRAND, ModContent.ProjectileType<LifeProjSmall>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer);
                            }
                        }
                        break;
                    case 2: //consistent spread of big shots
                        SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, NPC.Center);
                        for (int i = 0; i < 8; i++)
                        {
                            Vector2 offset1 = (NPC.DirectionTo(Player.Center) * 9f).RotatedBy((i - 3) * Math.PI / 8f);
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, offset1, ModContent.ProjectileType<LifeProjLarge>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer);
                        }
                        break;
                    case 3: //dark souls
                        SoundEngine.PlaySound(SoundID.NPCDeath7, NPC.Center);
                        for (int i = 0; i < 5; i++)
                        {
                            Vector2 offset1 = (NPC.DirectionTo(Player.Center) * 4f).RotatedBy((i - 2) * Math.PI / 12f);
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, -offset1, ModContent.ProjectileType<LifeHomingProj2>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer, 0f, npcIndex);
                        }
                        break;
                }
            }
            if (NPC.ai[1] > 480)
            {
                Flying = true;
                oldstate = state;
                StateReset();
            }
        }
        public void AttackReactionShotgun()
        {
            Player Player = Main.player[NPC.target];
            if (talk)
            {
                UtterWordsWhite("");
                talk = false;
                HitPlayer = true;
                NPC.netUpdate = true;
            }
            NPC.velocity = Vector2.Zero;
            if (NPC.ai[1] == 1)
            {
                NPC.ai[2] = Main.rand.Next(140, 220);
                SoundEngine.PlaySound(SoundID.Unlock, Player.Center);
                NPC.ai[3] = (Main.rand.Next(2));
                NPC.netUpdate = true;
            }


            if (NPC.ai[1] < NPC.ai[2])
            { //wait for blast
                Flying = false;
                float flySpeed2 = 0.5f;
                //float inertia2 = 1f;
                Vector2 OnPlayer = new Vector2(Player.Center.X, Player.Center.Y);
                Vector2 flyonPlayer = NPC.DirectionTo(OnPlayer) * flySpeed2;
                // NPC.velocity = (NPC.velocity * (inertia2 - 1f) + flyonPlayer) / inertia2;
                if (NPC.ai[1] < (NPC.ai[2] - 30) && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center - ((NPC.Center - Player.Center).RotatedBy(-Math.PI / 12)), Vector2.Zero, ModContent.ProjectileType<LifeCrosshair>(), 0, 0f, Main.myPlayer);
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center - ((NPC.Center - Player.Center).RotatedBy(Math.PI / 12)), Vector2.Zero, ModContent.ProjectileType<LifeCrosshair>(), 0, 0f, Main.myPlayer);
                    if (PhaseThree)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center - ((NPC.Center - Player.Center).RotatedBy(Math.PI / 4)), Vector2.Zero, ModContent.ProjectileType<LifeCrosshair>(), 0, 0f, Main.myPlayer);
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center - ((NPC.Center - Player.Center).RotatedBy(-Math.PI / 4)), Vector2.Zero, ModContent.ProjectileType<LifeCrosshair>(), 0, 0f, Main.myPlayer);
                    }
                }
            }
            if (NPC.ai[1] == (NPC.ai[2] - 30))
            {
                SoundEngine.PlaySound(SoundID.Unlock, Player.Center);
            }
            NPC.netUpdate = true;
            if (NPC.ai[1] >= (NPC.ai[2] - 20) && NPC.ai[1] < NPC.ai[2] && Main.netMode != NetmodeID.MultiplayerClient)
            {
                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center - ((NPC.Center - Player.Center).RotatedBy((-Math.PI / 12) + (NPC.ai[3] * Math.PI / 6))), Vector2.Zero, ModContent.ProjectileType<LifeCrosshair>(), 0, 0f, Main.myPlayer);
                if (PhaseThree)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center - ((NPC.Center - Player.Center).RotatedBy(Math.PI / 4)), Vector2.Zero, ModContent.ProjectileType<LifeCrosshair>(), 0, 0f, Main.myPlayer);
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center - ((NPC.Center - Player.Center).RotatedBy(-Math.PI / 4)), Vector2.Zero, ModContent.ProjectileType<LifeCrosshair>(), 0, 0f, Main.myPlayer);
                }
            }
            else if (NPC.ai[1] == NPC.ai[2])
            {
                float shootSpeed = 27f;
                LockVector2 = NPC.DirectionTo(Player.Center) * shootSpeed;
            }
            else if ((NPC.ai[1] - NPC.ai[2]) % 5 == 0 && NPC.ai[1] > NPC.ai[2] && Main.netMode != NetmodeID.MultiplayerClient && (((NPC.ai[1] < (NPC.ai[2] + 90) && !PhaseThree)) || ((NPC.ai[1] < (NPC.ai[2] + 270) && PhaseThree)))) //blast
            {
                SoundEngine.PlaySound(SoundID.Item12, Player.Center);
                float knockBack10 = 3f;
                for (int i = -3; i < 17; i++)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, (LockVector2).RotatedBy((i * -Math.PI / 48) + (i * NPC.ai[3] * Math.PI / 24)), ModContent.ProjectileType<LifeWave>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), knockBack10, Main.myPlayer);
                    if (PhaseThree)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, (LockVector2).RotatedBy(((Math.PI / 4) + ((i + 4) * Math.PI / 48) - ((NPC.ai[3] * Math.PI / 2) + ((i + 4) * NPC.ai[3] * Math.PI / 24)))), ModContent.ProjectileType<LifeWave>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), knockBack10, Main.myPlayer);
                    }
                }
                NPC.netUpdate = true;
            }
            //in p3, shoot volleys in closed area
            if (PhaseThree && NPC.ai[1] >= NPC.ai[2] && NPC.ai[1] < NPC.ai[2] + 244)
            {
                if ((NPC.ai[1] - NPC.ai[2]) % 61 == 0) //choose spot
                {
                    NPC.ai[0] = MathHelper.ToRadians(Main.rand.Next(-15, 15));
                    LockVector1 = (Vector2.Normalize(LockVector2)).RotatedBy(MathHelper.ToRadians(25 - (50 * NPC.ai[3])) - NPC.ai[0]);
                    NPC.netUpdate = true;
                }
                if ((NPC.ai[1] - NPC.ai[2]) % 61 < 55 && Main.netMode != NetmodeID.MultiplayerClient) //telegraph
                {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + LockVector1 * 600f, Vector2.Zero, ModContent.ProjectileType<LifeCrosshair>(), 0, 0f, Main.myPlayer);

                }
                if ((NPC.ai[1] - NPC.ai[2]) % 61 > 55 && (NPC.ai[1] - NPC.ai[2]) % 2 == 0) //fire
                {
                    SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, Player.Center);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int i = -1; i <= 1; i++)
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, 30f * LockVector1.RotatedBy(MathHelper.Pi / 32 * i), ModContent.ProjectileType<LifeProjLarge>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer);
                    }
                }

            }

            if ((NPC.ai[1] == NPC.ai[2] + 90 && !PhaseThree) || (NPC.ai[1] == NPC.ai[2] + 300 && PhaseThree))
            {
                NPC.position.X = Player.position.X;
                NPC.position.Y = Player.position.Y - 450f;
                SoundEngine.PlaySound(SoundID.Item8, NPC.Center); //PLACEHOLDER
                HitPlayer = false;
                Flying = true;
                NPC.netUpdate = true;
            }
            if ((NPC.ai[1] > NPC.ai[2] + 110 && !PhaseThree) || (NPC.ai[1] > NPC.ai[2] + 340 && PhaseThree))
            {
                HitPlayer = false;
                oldstate = state;
                StateReset();
            }
        }
        public void AttackRunningMinigun()
        {
            Player Player = Main.player[NPC.target];
            if (talk)
            {
                UtterWordsWhite("");
                talk = false;
                SoundEngine.PlaySound(SoundID.Zombie100, NPC.Center);
                NPC.netUpdate = true;
            }
            Flying = false;
            float flySpeed3 = 5.5f;
            float inertia3 = 5.5f;
            Vector2 OnPlayer2 = new Vector2(Player.Center.X, Player.Center.Y);
            Vector2 flyonPlayer2 = NPC.DirectionTo(OnPlayer2) * flySpeed3;
            NPC.velocity = (NPC.velocity * (inertia3 - 1f) + flyonPlayer2) / inertia3;

            //rotation
            if (NPC.velocity.ToRotation() > Math.PI)
            {
                NPC.rotation = 0f - (float)Math.PI * NPC.velocity.X / 100;
            }
            else
            {
                NPC.rotation = 0f + (float)Math.PI * NPC.velocity.X / 100;
            }

            //firing machinegun
            if (NPC.ai[1] > 90 && NPC.ai[1] % 6 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, NPC.Center);
                for (int i = -1; i < 2; i++)
                {
                    Vector2 ShootPlayer = (NPC.DirectionTo(Player.Center) * 30f).RotatedBy(i * Math.PI / 7f);
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, ShootPlayer, ModContent.ProjectileType<LifeProjLarge>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer);
                }
            }
            //firing circle in p3
            if (NPC.ai[1] > 90 && NPC.ai[1] % 30 == 0 && Main.netMode != NetmodeID.MultiplayerClient && PhaseThree)
            {
                float ProjectileSpeed = 10f;
                Vector2 shootatPlayer3 = NPC.DirectionTo(Player.Center) * ProjectileSpeed;
                int amount2 = 14;
                for (int i = 0; i < amount2; i++)
                {
                    Vector2 shootoffset = shootatPlayer3.RotatedBy(i * (Math.PI / (amount2 / 2)));
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, shootoffset, ModContent.ProjectileType<LifeWave>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer);
                }
            }
            if (NPC.ai[1] >= 300f)
            {
                Flying = true;
                oldstate = state;
                StateReset();
            }
        }
        public void AttackRain()
        {
            Player Player = Main.player[NPC.target];
            if (talk)
            {
                UtterWordsWhite("");
                talk = false;
                NPC.position.X = Player.position.X;
                NPC.position.Y = Player.position.Y - 500f;
                NPC.netUpdate = true;
            }
            if (NPC.ai[1] > 60f && NPC.ai[1] < 360f)
            {
                if (NPC.ai[2] > 1f)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        float knockBack6 = 3f;
                        NPC.ai[3] = Main.rand.Next(-750, 750);
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), new Vector2(Player.Center.X - NPC.ai[3], Player.Center.Y - 750f), new Vector2(0f, 7f), ModContent.ProjectileType<LifeProjSmall>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), knockBack6, Main.myPlayer, 0, 0);
                    }
                    NPC.ai[2] = 0f;
                }
                NPC.ai[2] += 1f;
            }
            if (NPC.ai[1] > 420f)
            {
                Flying = true;
                oldstate = state;
                StateReset();
            }
        }
        public void AttackTeleportNukes()
        {
            Player Player = Main.player[NPC.target];
            if (talk)
            {
                UtterWordsWhite("");
                talk = false;
                LockVector1 = new Vector2(Player.Center.X - (NPC.width / 2), Player.Center.Y - (NPC.height / 2));
                Flying = false;
                NPC.velocity = Vector2.Zero;
                NPC.netUpdate = true;
            }
            if (NPC.ai[1] < 60 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                Projectile.NewProjectile(NPC.GetSource_FromThis(), new Vector2(LockVector1.X + (float)(NPC.width / 2), LockVector1.Y + (float)(NPC.height / 2)), Vector2.Zero, ModContent.ProjectileType<TpTelegraph>(), 0, 0f, Main.myPlayer);
            }
            if (NPC.ai[1] == 60)
            {
                NPC.position.X = LockVector1.X;
                NPC.position.Y = LockVector1.Y;
                NPC.netUpdate = true;
                SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
                for (int i = 0; i < 16; i++)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    { Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(8f, 0f).RotatedBy((Math.PI / 8) * i), ModContent.ProjectileType<LifeProjLarge>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer); }
                }
                Main.LocalPlayer.GetModPlayer<FargoSoulsPlayer>().Screenshake += 180;
            }
            if (NPC.ai[1] >= 120 && (NPC.ai[1] - 120) % 3 == 0 && NPC.ai[1] < 137)
            {
                SoundEngine.PlaySound(SoundID.Item91, NPC.Center);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(12f, 0f).RotatedBy((Math.PI / 3) * NPC.ai[2]), ModContent.ProjectileType<LifeNuke>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage * 2), 300f, Main.myPlayer);
                NPC.ai[2]++;
            }
            if (NPC.ai[1] > 420f)
            {
                Flying = true;
                oldstate = state;
                StateReset();
            }
        }
        #endregion
        #region Overrides
        public override bool StrikeNPC(ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit)
        {
            damage /= 5;
            if (!PhaseThree && NPC.life < NPC.lifeMax / 20)
            {
                NPC.life = NPC.lifeMax / 20 - 1;
                damage = 1;
                crit = false;
                return false;
            }
            if (PhaseThree && !resigned && NPC.life < NPC.lifeMax / 8)
            {
                NPC.life = NPC.lifeMax / 8 - 1;
                damage = 1;
                crit = false;
                return false;
            }
            return true;
        }
        public override bool CanHitPlayer(Player target, ref int CooldownSlot)
        {
            if (HitPlayer)
            {
                //circular hitbox-inator
                Vector2 ellipseDim = NPC.Size;
                Vector2 ellipseCenter = NPC.position + 0.5f * new Vector2(NPC.width, NPC.height);
                Vector2 boxPos = target.position;
                Vector2 boxDim = target.Size;
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
                return (x * x) / (a * a) + (y * y) / (b * b) < 1; //point collision detection
            }
            return false;
        }
        
        public override void OnKill()
        {
            NPC.SetEventFlagCleared(ref FargoSoulsWorld.downedBoss[(int)FargoSoulsWorld.Downed.LifeChallenger], -1);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D bodytexture = Terraria.GameContent.TextureAssets.Npc[NPC.type].Value;
            Texture2D wingtexture = FargowiltasSouls.Instance.Assets.Request<Texture2D>("NPCs/Challengers/LifeChallenger_Wings", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            Vector2 drawPos = NPC.Center - screenPos;
            int currentFrame = NPC.frame.Y / (bodytexture.Height / Main.npcFrameCount[NPC.type]);
            int wingHeight = wingtexture.Height / Main.npcFrameCount[NPC.type];
            Rectangle wingRectangle = new Rectangle(0, currentFrame * wingHeight, wingtexture.Width, wingHeight);
            Vector2 wingOrigin = new Vector2(wingtexture.Width / 2, wingtexture.Height / 2 / Main.npcFrameCount[NPC.type]);

            for (int i = 0; i < NPCID.Sets.TrailCacheLength[NPC.type]; i++)
            {
                Vector2 value4 = NPC.oldPos[i];
                double fpf = (int)(60 / Main.npcFrameCount[NPC.type] * SPR); //multiply by sec/rotation)
                int oldFrame = (int)((NPC.frameCounter - i) / fpf);
                Rectangle oldWingRectangle = new Rectangle(0, oldFrame * wingHeight, wingtexture.Width, wingHeight);
                DrawData wingTrailGlow = new DrawData(wingtexture, value4 + NPC.Size / 2f - screenPos + new Vector2(0, NPC.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(oldWingRectangle), drawColor * (0.5f / i), NPC.rotation, wingOrigin, NPC.scale, SpriteEffects.None, 0);
                GameShaders.Misc["LCWingShader"].UseColor(Color.HotPink).UseSecondaryColor(Color.HotPink);
                GameShaders.Misc["LCWingShader"].Apply(wingTrailGlow);
                wingTrailGlow.Draw(spriteBatch);
            }

            spriteBatch.Draw(origin: new Vector2(bodytexture.Width / 2, bodytexture.Height / 2 / Main.npcFrameCount[NPC.type]), texture: bodytexture, position: drawPos, sourceRectangle: NPC.frame, color: drawColor, rotation: BodyRotation, scale: NPC.scale, effects: SpriteEffects.None, layerDepth: 0f);
            spriteBatch.Draw(origin: wingOrigin, texture: wingtexture, position: drawPos, sourceRectangle: wingRectangle, color: drawColor, rotation: NPC.rotation, scale: NPC.scale, effects: SpriteEffects.None, layerDepth: 0f);
            return false;
		}
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {

            if (!NPC.IsABestiaryIconDummy)
            {
                spriteBatch.End(); spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);
            }

            Texture2D star = FargowiltasSouls.Instance.Assets.Request<Texture2D>("Effects/LifeStar", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            Rectangle rect = new Rectangle(0, 0, star.Width, star.Height);
            float scale = 0.45f * Main.rand.NextFloat(1f, 2.5f);
            Vector2 origin = new Vector2((star.Width / 2) + scale, (star.Height / 2) + scale);

            spriteBatch.Draw(star, NPC.Center - screenPos, new Rectangle?(rect), Color.HotPink, 0, origin, scale, SpriteEffects.None, 0);
            DrawData starDraw = new DrawData(star, NPC.Center - screenPos, new Rectangle?(rect), Color.HotPink, 0, origin, scale, SpriteEffects.None, 0);
            GameShaders.Misc["LCWingShader"].UseColor(Color.HotPink).UseSecondaryColor(Color.HotPink);
            GameShaders.Misc["LCWingShader"].Apply(new DrawData?());
            starDraw.Draw(spriteBatch);

            if (!NPC.IsABestiaryIconDummy)
            {
                spriteBatch.End(); spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);
            }
        }

        public override void FindFrame(int frameHeight)
        {
            double fpf = (int)(60 / Main.npcFrameCount[NPC.type] * SPR); //multiply by sec/rotation)
            NPC.spriteDirection = NPC.direction;
            NPC.frameCounter += 1;
            NPC.frameCounter %= Main.npcFrameCount[NPC.type] * fpf;
            NPC.frame.Y = frameHeight * (int)(NPC.frameCounter / fpf);
        }
        #endregion
        #region Help Methods
        /*public void EntranceStomp()
		{
			Player Player = Main.player[NPC.target];
			Main.player[Main.myPlayer].GetModPlayer<FargoSoulsPlayer>().Screenshake += 40;
			for (int i = 0; i < 50; i++)
			{
				Dust.NewDust(NPC.position, NPC.width, NPC.height, 87, NPC.velocity.X, NPC.velocity.Y, 0, default(Color), 1f);
			}
			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				for (int j = 0; j < 30; j++)
				{
					int damage = DAMAGE;
					float knockBack = 3f;
					NPC.ai[0] = MathHelper.ToRadians(Main.rand.Next(-180, 180));
					Vector2 shootrandom = (NPC.DirectionTo(Player.Center) * 5f).RotatedBy(NPC.ai[0]);
					Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, shootrandom, ModContent.ProjectileType<LifeProjLarge>(), damage, knockBack, Main.myPlayer);
				}
			}
			NPC.netUpdate = true;
		}*/
        public void UtterWordsWhite(string text) //deactivated unless we the boss to say something
        {
            /*
                if (Main.netMode == 0)
                {
                    Main.NewText(Language.GetTextValue(text), Color.Gray);
                }
                else if (Main.netMode == 2)
                {
                    ChatHelper.BroadcastChatMessage(NetworkText.FromKey(text), Color.Gray);
                }
                CombatText.NewText(NPC.Hitbox, Color.Gray, Language.GetTextValue(text), dramatic: true);
            */
        }

        public void UtterWordsRed(string text) //deactivated unless we the boss to say something
        {
            
                if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    Main.NewText(Language.GetTextValue(text), Color.Red);
                }
                else if (Main.netMode == NetmodeID.Server)
                {
                    ChatHelper.BroadcastChatMessage(NetworkText.FromKey(text), Color.Red);
                }
                CombatText.NewText(NPC.Hitbox, Color.Red, Language.GetTextValue(text), dramatic: true);
            
        }
        public void StateReset()
		{
			Retarget(true);
			NPC.netUpdate = true;
			RandomizeState();
			NPC.ai[1] = 0f;
			NPC.ai[2] = 0f;
			NPC.ai[3] = 0f;
			NPC.ai[0] = 0f;
			talk = true;
		}

		public void Retarget(bool faTa)
		{
			Player oldtarget = Main.player[NPC.target];
			NPC.TargetClosest(faceTarget: faTa);
			if (oldtarget != Main.player[NPC.target])
			{
				Projectile.NewProjectile(NPC.GetSource_FromThis(), Main.player[NPC.target].Center, Vector2.Zero, ModContent.ProjectileType<TargetCrosshair>(), 0, 0, Main.myPlayer, 0f, npcIndex);
			}
			NPC.netUpdate = true;
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
				availablestates.RemoveAt(index);

			}
			if (!PhaseThree && NPC.life < NPC.lifeMax / 4)
			{
				state = 100;
			}
			if (PhaseThree && NPC.life < NPC.lifeMax / 8)
			{
				state = 101;
				oldstate = 0;
			}
			NPC.netUpdate = true;
		}
    
		#endregion
	}
}
