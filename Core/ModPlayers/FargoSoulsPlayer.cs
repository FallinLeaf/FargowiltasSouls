using FargowiltasSouls.Core;
using FargowiltasSouls.Content.Items.Accessories.Masomode;
using FargowiltasSouls.Content.Items.Accessories.Souls;
using FargowiltasSouls.Content.NPCs;
using FargowiltasSouls.Content.Projectiles.ChallengerItems;
using FargowiltasSouls.Content.Projectiles.Masomode;
using FargowiltasSouls.Content.Projectiles.Minions;
using FargowiltasSouls.Content.Projectiles.Pets;
using FargowiltasSouls.Content.Projectiles.Souls;
using FargowiltasSouls.Toggler;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;
using FargowiltasSouls.Content.Projectiles;
using FargowiltasSouls.Content.Items.Armor;
using FargowiltasSouls.Content.Items.Dyes;
using FargowiltasSouls.Content.Items.Accessories.Enchantments;
using FargowiltasSouls.Content.Buffs;
using FargowiltasSouls.Content.Buffs.Souls;
using FargowiltasSouls.Content.Buffs.Masomode;
using FargowiltasSouls.Content.Items.Accessories.Forces;
using FargowiltasSouls.Content.NPCs.AbomBoss;
using FargowiltasSouls.Content.NPCs.DeviBoss;
using FargowiltasSouls.Content.NPCs.MutantBoss;

namespace FargowiltasSouls.Core.ModPlayers
{
    public partial class FargoSoulsPlayer : ModPlayer
    {
        public ToggleBackend Toggler = new();

        public Dictionary<string, bool> TogglesToSync = new();

        public List<string> disabledToggles = new List<string>();

        public List<BaseEnchant> EquippedEnchants = new List<BaseEnchant>();


        public bool IsStandingStill;
        public float AttackSpeed;
        public float WingTimeModifier = 1f;

        public bool FreeEaterSummon = true;
        public int Screenshake;

        public Dictionary<int, bool> KnownBuffsToPurify = new Dictionary<int, bool>();

        public bool DoubleTap
        {
            get
            {
                return Main.ReversedUpDownArmorSetBonuses ?
                    Player.controlUp && Player.releaseUp && Player.doubleTapCardinalTimer[1] > 0 && Player.doubleTapCardinalTimer[1] != 15
                    : Player.controlDown && Player.releaseDown && Player.doubleTapCardinalTimer[0] > 0 && Player.doubleTapCardinalTimer[0] != 15;
            }
        }

        public bool IsStillHoldingInSameDirectionAsMovement
            => (Player.velocity.X > 0 && Player.controlRight)
            || (Player.velocity.X < 0 && Player.controlLeft)
            || Player.dashDelay < 0
            || IsDashingTimer > 0;

        //grapple check needed because grapple state extends dash state forever
        public bool IsInADashState
            => (Player.dashDelay == -1 || IsDashingTimer > 0) && Player.grapCount <= 0;

        public override void SaveData(TagCompound tag)
        {
            var playerData = new List<string>();
            if (MutantsPactSlot) playerData.Add("MutantsPactSlot");
            if (MutantsDiscountCard) playerData.Add("MutantsDiscountCard");
            if (MutantsCreditCard) playerData.Add("MutantsCreditCard");
            if (ReceivedMasoGift) playerData.Add("ReceivedMasoGift");
            if (RabiesVaccine) playerData.Add("RabiesVaccine");
            if (DeerSinew) playerData.Add("DeerSinew");
            if (HasClickedWrench) playerData.Add("HasClickedWrench");
            tag.Add($"{Mod.Name}.{Player.name}.Data", playerData);

            var togglesOff = new List<string>();
            if (Toggler != null && Toggler.Toggles != null)
            {
                foreach (KeyValuePair<string, Toggle> entry in Toggler.Toggles)
                {
                    if (!Toggler.Toggles[entry.Key].ToggleBool)
                        togglesOff.Add(entry.Key);
                }
            }
            tag.Add($"{Mod.Name}.{Player.name}.TogglesOff", togglesOff);

            Toggler.Save();
        }

        public override void LoadData(TagCompound tag)
        {
            var playerData = tag.GetList<string>($"{Mod.Name}.{Player.name}.Data");
            MutantsPactSlot = playerData.Contains("MutantsPactSlot");
            MutantsDiscountCard = playerData.Contains("MutantsDiscountCard");
            MutantsCreditCard = playerData.Contains("MutantsCreditCard");
            ReceivedMasoGift = playerData.Contains("ReceivedMasoGift");
            RabiesVaccine = playerData.Contains("RabiesVaccine");
            DeerSinew = playerData.Contains("DeerSinew");
            HasClickedWrench = playerData.Contains("HasClickedWrench");

            disabledToggles = tag.GetList<string>($"{Mod.Name}.{Player.name}.TogglesOff").ToList();
        }

        public override void Initialize()
        {
            //Toggler.LoadInMenu();
        }

        public override void OnEnterWorld(Player player)
        {
            Toggler.TryLoad();
            Toggler.LoadPlayerToggles(this);
            disabledToggles.Clear();

            if (!ModLoader.TryGetMod("FargowiltasMusic", out Mod musicMod))
            {
                Main.NewText(Language.GetTextValue($"Mods.{Mod.Name}.Message.NoMusic1"), Color.LimeGreen);
                Main.NewText(Language.GetTextValue($"Mods.{Mod.Name}.Message.NoMusic2"), Color.LimeGreen);
            }

            if (Toggler.CanPlayMaso)
            {
                if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    FargoSoulsWorld.CanPlayMaso = true;
                }
                else if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    //Main.NewText("send it");
                    ModPacket packet = Mod.GetPacket();
                    packet.Write((byte)FargowiltasSouls.PacketID.SyncCanPlayMaso);
                    packet.Write(Toggler.CanPlayMaso);
                    packet.Send();
                }
            }
        }
        
        public override void ResetEffects()
        {
            HasDash = false;

            AttackSpeed = 1f;
            if (Screenshake > 0)
                Screenshake--;

            if (NoUsingItems > 0)
                NoUsingItems--;

            //            Wood = false;

            WingTimeModifier = 1f;

            NinjaEnchantItem = null;

            QueenStingerItem = null;
            EridanusSet = false;
            GaiaSet = false;
            StyxSet = false;
            NekomiSet = false;

            BrainMinion = false;
            EaterMinion = false;
            BigBrainMinion = false;
            DukeFishron = false;

            SquirrelMount = false;

            SeekerOfAncientTreasures = false;
            AccursedSarcophagus = false;
            BabyLifelight = false;
            BabySilhouette = false;
            BiteSizeBaron = false;
            ChibiDevi = false;
            MutantSpawn = false;
            BabyAbom = false;

            //            #region enchantments 
            PetsActive = true;
            ShadowEnchantActive = false;
            CrimsonEnchantActive = false;
            //CrimsonRegen = false;
            SpectreEnchantActive = false;
            BeeEnchantActive = false;
            SpiderEnchantActive = false;
            StardustEnchantActive = false;
            MythrilEnchantItem = null;
            FossilEnchantItem = null;
            JungleEnchantActive = false;
            ShroomEnchantActive = false;
            CobaltEnchantItem = null;
            SpookyEnchantActive = false;
            NebulaEnchantActive = false;
            BeetleEnchantActive = false;
            HallowEnchantActive = false;
            AncientHallowEnchantActive = false;
            ChloroEnchantActive = false;
            ChloroEnchantItem = null;
            VortexEnchantActive = false;
            AdamantiteEnchantItem = null;
            FrostEnchantActive = false;
            PalladEnchantItem = null;
            OriEnchantItem = null;
            MeteorEnchantItem = null;
            MoltenEnchantActive = false;
            CopperEnchantItem = null;
            PlatinumEnchantActive = false;
            CrystalEnchantActive = false;
            FirstStrike = false;
            IronEnchantItem = null;
            TurtleEnchantActive = false;
            ShellHide = false;
            LeadEnchantItem = null;
            GladiatorEnchantActive = false;
            GoldEnchantActive = false;
            GoldShell = false;
            CactusEnchantActive = false;
            ForbiddenEnchantActive = false;
            NecroEnchantActive = false;
            ObsidianEnchantItem = null;
            LavaWet = false;
            TinEnchantItem = null;
            TikiEnchantActive = false;
            SolarEnchantActive = false;
            ShinobiEnchantActive = false;
            ValhallaEnchantActive = false;
            DarkArtistEnchantActive = false;
            RedRidingEnchantItem = null;
            TungstenEnchantItem = null;

            MahoganyEnchantItem = null;
            BorealEnchantItem = null;
            WoodEnchantItem = null;
			WoodEnchantDiscount = false;
            PalmEnchantItem = null;
            EbonwoodEnchantItem = null;
            ShadewoodEnchantItem = null;
            PearlwoodEnchantItem = null;

            RainEnchantActive = false;
            AncientShadowEnchantActive = false;
            SquireEnchantActive = false;
            ApprenticeEnchantActive = false;
            HuntressEnchantActive = false;
            MonkEnchantActive = false;
            SnowEnchantActive = false;
            SnowVisual = false;
            TitaniumEnchantItem = null;
            TitaniumDRBuff = false;
            TitaniumCD = false;

            CosmoForce = false;
            EarthForce = false;
            LifeForce = false;
            NatureForce = false;
            SpiritForce = false;
            TerraForce = false;
            ShadowForce = false;
            WillForce = false;
            WoodForce = false;

            //            #endregion

            //souls
            MagicSoul = false;
            RangedSoul = false;
            RangedEssence = false;
            BuilderMode = false;
            UniverseSoul = false;
            UniverseCore = false;
            FishSoul1 = false;
            FishSoul2 = false;
            TerrariaSoul = false;
            VoidSoul = false;
            Eternity = false;

            //maso
            SlimyShieldItem = null;
            AgitatingLensItem = null;
            DarkenedHeartItem = null;
            GuttedHeart = false;
            NecromanticBrewItem = null;
            DeerclawpsItem = null;
            DeerSinewNerf = false;
            PureHeart = false;
            PungentEyeballMinion = false;
            CrystalSkullMinion = false;
            FusedLens = false;
            FusedLensCanDebuff = false;
            GroundStick = false;
            Supercharged = false;
            Probes = false;
            MagicalBulb = false;
            PlanterasChild = false;
            SkullCharm = false;
            PungentEyeball = false;
            LumpOfFlesh = false;
            PumpkingsCapeItem = null;
            LihzahrdTreasureBoxItem = null;
            BetsysHeartItem = null;
            BetsyDashing = false;
            MutantAntibodies = false;
            GravityGlobeEXItem = null;
            CelestialRuneItem = null;
            AdditionalAttacks = false;
            MoonChalice = false;
            LunarCultist = false;
            TrueEyes = false;
            AbomWandItem = null;
            MasochistSoul = false;
            MasochistHeart = false;
            SandsofTime = false;
            DragonFang = false;
			StabilizedGravity = false;
            SecurityWallet = false;
            FrigidGemstoneItem = null;
            WretchedPouchItem = null;
            NymphsPerfume = false;
            NymphsPerfumeRespawn = false;
            SqueakyAcc = false;
            RainbowSlime = false;
            SkeletronArms = false;
            IceQueensCrown = false;
            CirnoGraze = false;
            MiniSaucer = false;
            CanAmmoCycle = false;
            TribalCharm = false;
            TribalCharmEquipped = false;
            SupremeDeathbringerFairy = false;
            GodEaterImbue = false;
            MutantSetBonusItem = null;
            AbomMinion = false;
            PhantasmalRing = false;
            TwinsEX = false;
            TimsConcoction = false;
            DeviGraze = false;
            Graze = false;
            GrazeRadius = 100f;
            DevianttHeartItem = null;
            MutantEyeItem = null;
            MutantEyeVisual = false;
            AbomRebirth = false;
            WasHurtBySomething = false;
            PrecisionSeal = false;
            PrecisionSealHurtbox = false;
            GelicWingsItem = null;
            ConcentratedRainbowMatter = false;

            //debuffs
            Hexed = false;
            Unstable = false;
            Fused = false;
            Shadowflame = false;
            Oiled = false;
            Slimed = false;
            noDodge = false;
            noSupersonic = false;
            NoMomentum = false;
            Bloodthirsty = false;
            DisruptedFocus = false;
            SinisterIcon = false;
            SinisterIconDrops = false;

            Smite = false;
            Anticoagulation = false;
            GodEater = false;
            FlamesoftheUniverse = false;
            MutantNibble = false;
            Asocial = false;
            Kneecapped = false;
            Defenseless = false;
            Purified = false;
            Infested = false;
            Rotting = false;
            SqueakyToy = false;
            Atrophied = false;
            Jammed = false;
            ReverseManaFlow = false;
            CurseoftheMoon = false;
            OceanicMaul = false;
            DeathMarked = false;
            Hypothermia = false;
            Midas = false;
            MutantPresence = MutantPresence ? Player.HasBuff(ModContent.BuffType<FargowiltasSouls.Content.Buffs.Boss.MutantPresence>()) : false;
            MutantFang = false;
            DevianttPresence = false;
            Swarming = false;
            LowGround = false;
            Flipped = false;
            LihzahrdCurse = false;
            //LihzahrdBlessing = false;
            Berserked = false;
            CerebralMindbreak = false;
            NanoInjection = false;
            Stunned = false;
            HaveCheckedAttackSpeed = false;
            BoxofGizmos = false;
            //IronEnchantShield = false;
            SilverEnchantItem = null;
            DreadShellItem = null;

            EquippedEnchants.Clear();

            if (WizardEnchantActive)
            {
                WizardEnchantActive = false;
                for (int i = 3; i <= 9; i++)
                {
                    if (!Player.armor[i].IsAir && (Player.armor[i].type == ModContent.ItemType<WizardEnchant>() || Player.armor[i].type == ModContent.ItemType<CosmoForce>()))
                    {
                        WizardEnchantActive = true;
                        CosmoForce = true;
                        EarthForce = true;
                        LifeForce = true;
                        NatureForce = true;
                        ShadowForce = true;
                        SpiritForce = true;
                        TerraForce = true;
                        WillForce = true;
                        WoodForce = true;
                        break;
                    }
                }
            }

            if (!Mash && MashCounter > 0)
                MashCounter--;
            Mash = false;
        }

        public override void OnRespawn(Player player)
        {
            if (NymphsPerfumeRespawn)
                NymphsPerfumeRestoreLife = 6;
        }

        public override void UpdateDead()
        {
            bool wasSandsOfTime = SandsofTime;
            bool wasNymphsPerfumeRespawn = NymphsPerfumeRespawn;

            ResetEffects();

            SandsofTime = wasSandsOfTime;
            NymphsPerfumeRespawn = wasNymphsPerfumeRespawn;

            if (SandsofTime && !FargoSoulsUtil.AnyBossAlive() && Player.respawnTimer > 10)
                Player.respawnTimer -= Eternity ? 6 : 1;

            if (FargoSoulsWorld.MasochistModeReal && FargoSoulsUtil.AnyBossAlive())
            {
                if (Player.respawnTimer < 10)
                    Player.respawnTimer = 10;

                if (Main.netMode == NetmodeID.MultiplayerClient && Main.npc[FargoSoulsGlobalNPC.boss].HasValidTarget && Main.npc[FargoSoulsGlobalNPC.boss].HasPlayerTarget)
                    Player.Center = Main.player[Main.npc[FargoSoulsGlobalNPC.boss].target].Center;
            }

            BeetleEnchantDefenseTimer = 0;

            ReallyAwfulDebuffCooldown = 0;
            ParryDebuffImmuneTime = 0;

            WingTimeModifier = 1f;
            FreeEaterSummon = true;

            AbominableWandRevived = false;

            EridanusTimer = 0;
            StyxMeter = 0;
            StyxTimer = 0;
            NekomiMeter = 0;
            NekomiTimer = 0;

            CirnoGrazeCounter = 0;

            //debuffs
            unstableCD = 0;
            lightningRodTimer = 0;

            BuilderMode = false;
            NoUsingItems = 0;

            FreezeTime = false;
            freezeLength = 0;

            ChillSnowstorm = false;
            chillLength = 0;

            SlimyShieldFalling = false;
            DarkenedHeartCD = 60;
            GuttedHeartCD = 60;
            IsDashingTimer = 0;
            GroundPound = 0;
            NymphsPerfumeCD = 30;
            WretchedPouchCD = 0;

            DeviGrazeBonus = 0;
            MutantEyeCD = 60;

            Mash = false;
            WizardEnchantActive = false;
            MashCounter = 0;

            MaxLifeReduction = 0;
            CurrentLifeReduction = 0;

            MythrilTimer = MythrilMaxTime;
        }

        

        public override void ModifyLuck(ref float luck)
        {
            if (Unlucky)
                luck -= 1.0f;

            Unlucky = false;
        }

        List<int> prevDyes = null;

        public void ManageLifeReduction()
        {
            if (OceanicMaul && LifeReductionUpdateTimer <= 0)
                LifeReductionUpdateTimer = 1; //trigger life reduction behaviour

            if (LifeReductionUpdateTimer > 0)
            {
                const int threshold = 30;
                if (LifeReductionUpdateTimer++ > threshold)
                {
                    LifeReductionUpdateTimer = 1;

                    if (OceanicMaul) //with maul, real max life gradually decreases to the desired point
                    {
                        if (MutantFang) //update faster
                            LifeReductionUpdateTimer = threshold - 10;

                        int newLifeReduction = CurrentLifeReduction + 5;
                        if (newLifeReduction > MaxLifeReduction)
                            newLifeReduction = MaxLifeReduction;
                        if (newLifeReduction > Player.statLifeMax2 - 100) //i.e. max life wont go below 100
                            newLifeReduction = Player.statLifeMax2 - 100;

                        if (CurrentLifeReduction < newLifeReduction)
                        {
                            CurrentLifeReduction = newLifeReduction;
                            CombatText.NewText(Player.Hitbox, Color.DarkRed, Language.GetTextValue($"Mods.{Mod.Name}.Message.OceanicMaulLifeDown"));
                        }
                    }
                    else //after maul wears off, real max life gradually recovers to normal value
                    {
                        CurrentLifeReduction -= 5;
                        if (MaxLifeReduction > CurrentLifeReduction)
                            MaxLifeReduction = CurrentLifeReduction;
                        CombatText.NewText(Player.Hitbox, Color.DarkGreen, Language.GetTextValue($"Mods.{Mod.Name}.Message.OceanicMaulLifeUp"));
                    }
                }
            }

            if (CurrentLifeReduction > 0)
            {
                if (CurrentLifeReduction > Player.statLifeMax2 - 100) //i.e. max life wont go below 100
                    CurrentLifeReduction = Player.statLifeMax2 - 100;
                Player.statLifeMax2 -= CurrentLifeReduction;
                //if (Player.statLife > Player.statLifeMax2) Player.statLife = Player.statLifeMax2;
            }
            else if (!OceanicMaul) //deactivate behaviour
            {
                CurrentLifeReduction = 0;
                MaxLifeReduction = 0;
                LifeReductionUpdateTimer = 0;
            }
        }

        public override void PostUpdate()
        {
            if (!FreeEaterSummon && !Main.npc.Any(n => n.active && (n.type == NPCID.EaterofWorldsHead || n.type == NPCID.EaterofWorldsBody || n.type == NPCID.EaterofWorldsTail)))
            {
                FreeEaterSummon = true;
            }

            if (NymphsPerfumeRestoreLife > 0 && --NymphsPerfumeRestoreLife == 0)
            {
                if (Player.statLife < Player.statLifeMax2)
                    Player.statLife = Player.statLifeMax2;
                //doing it like this so it accounts for your lifeMax after respawn
                //regular OnRespawn() doesnt account for lifeforce, and is lowered by dying with oceanic maul
            }

            ConcentratedRainbowMatterTryAutoHeal();
        }

        public override float UseSpeedMultiplier(Item item)
        {
            int useTime = item.useTime;
            int useAnimate = item.useAnimation;

            if (useTime <= 0 || useAnimate <= 0 || item.damage <= 0)
                return base.UseSpeedMultiplier(item);

            if (!HaveCheckedAttackSpeed)
            {
                HaveCheckedAttackSpeed = true;

                if (!Berserked && !TribalCharm && BoxofGizmos && !item.autoReuse && !Player.FeralGloveReuse(item))
                {
                    int targetUseTime = useTime + 6;
                    while (useTime / AttackSpeed < targetUseTime)
                    {
                        AttackSpeed -= .05f;
                    }
                }

                if (Berserked)
                {
                    AttackSpeed += .1f;
                }

                if (MagicSoul && item.CountsAsClass(DamageClass.Magic))
                {
                    AttackSpeed += .2f;
                }

                if (MythrilEnchantItem != null)
                {
                    MythrilEnchant.CalcMythrilAttackSpeed(this, item);
                }

                if (WretchedPouchItem != null && !MasochistSoul && AttackSpeed > 1f)
                {
                    float diff = AttackSpeed - 1f;
                    diff /= 2;
                    AttackSpeed -= diff;
                }

                if (NinjaEnchantItem != null && Player.GetToggleValue("NinjaSpeed"))
                {
                    AttackSpeed *= 2;
                }

                //modify attack speed so it rounds up
                //int useTimeRoundUp = (int)Math.Round(useTime / AttackSpeed, MidpointRounding.ToPositiveInfinity);
                //if (useTimeRoundUp < useTime) //sanity check
                //{
                //    while (useTime / AttackSpeed < useTimeRoundUp)
                //    {
                //        AttackSpeed -= .01f; //small increments to avoid skipping past any integers
                //    }
                //}

                //checks so weapons dont break
                while (useTime / AttackSpeed < 1)
                {
                    AttackSpeed -= .01f;
                }

                while (useAnimate / AttackSpeed < 3)
                {
                    AttackSpeed -= .01f;
                }

                if (AttackSpeed < .1f)
                    AttackSpeed = .1f;
            }

            return AttackSpeed;
        }

        public override void UpdateBadLifeRegen()
        {
            if (Player.electrified && Player.wet)
                Player.lifeRegen -= 16;

            void DamageOverTime(int badLifeRegen, bool affectLifeRegenCount = false)
            {
                if (Player.lifeRegen > 0)
                    Player.lifeRegen = 0;

                if (affectLifeRegenCount && Player.lifeRegenCount > 0)
                    Player.lifeRegen = 0;

                Player.lifeRegenTime = 0;
                Player.lifeRegen -= badLifeRegen;
            }

            if (NanoInjection)
                DamageOverTime(10);

            if (Shadowflame)
                DamageOverTime(10);

            if (GodEater)
            {
                DamageOverTime(170, true);

                Player.lifeRegenCount -= 70;
            }

            if (MutantNibble)
                DamageOverTime(0, true);

            if (Infested)
                DamageOverTime(InfestedExtraDot());

            if (Rotting)
                DamageOverTime(2);

            if (CurseoftheMoon)
                DamageOverTime(20);

            if (Oiled && Player.lifeRegen < 0)
            {
                Player.lifeRegen *= 2;
            }

            if (MutantPresence)
            {
                if (Player.lifeRegen > 5)
                    Player.lifeRegen = 5;
            }

            if (FlamesoftheUniverse)
                DamageOverTime((30 + 50 + 48 + 30) / 2, true);

            if (Smite)
                DamageOverTime(0, true);

            if (Anticoagulation)
                DamageOverTime(4, true);

            if (Player.lifeRegen < 0)
            {
                if (TerraForce)
                {
                    Player.lifeRegen = (int)(Player.lifeRegen * 0.4f);
                }
                else if (LeadEnchantItem != null)
                {
                    Player.lifeRegen = (int)(Player.lifeRegen * 0.6f);
                }

                FusedLensCanDebuff = true;
            }
        }

        public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
        {
            //            if (squireReduceIframes && (SquireEnchant || ValhallaEnchant))
            //            {
            //                if (Main.rand.NextBool(3))
            //                {
            //                    float scale = ValhallaEnchant ? 2f : 1.5f;
            //                    int type = ValhallaEnchant ? 87 : 91;
            //                    int dust = Dust.NewDust(Player.position, Player.width, Player.height, type, Player.velocity.X * 0.4f, Player.velocity.Y * 0.4f, 87, default(Color), scale);
            //                    Main.dust[dust].noGravity = true;
            //                    Main.dust[dust].velocity *= 1.8f;
            //                    Main.dust[dust].velocity.Y -= 0.5f;
            //                    if (Main.rand.NextBool(4))
            //                    {
            //                        Main.dust[dust].noGravity = false;
            //                        Main.dust[dust].scale *= 0.5f;
            //                    }
            //                    Main.PlayerDrawDust.Add(dust);
            //                }
            //                fullBright = true;
            //            }

            if (Shadowflame)
            {
                if (Main.rand.NextBool(4) && drawInfo.shadow == 0f)
                {
                    int dust = Dust.NewDust(Player.position - new Vector2(2f, 2f), Player.width, Player.height, DustID.Shadowflame, Player.velocity.X * 0.4f, Player.velocity.Y * 0.4f, 100, default(Color), 2f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 1.8f;
                    Main.dust[dust].velocity.Y -= 0.5f;
                    drawInfo.DustCache.Add(dust);
                }
                fullBright = true;
            }

            if (Rotting)
            {
                if (drawInfo.shadow == 0f)
                {
                    int dust = Dust.NewDust(Player.position - new Vector2(2f, 2f), Player.width, Player.height, DustID.Blood, Player.velocity.X * 0.1f, Player.velocity.Y * 0.1f, 0, default(Color), 2f);
                    Main.dust[dust].noGravity = Main.rand.NextBool();
                    Main.dust[dust].velocity *= 1.8f;
                    Main.dust[dust].velocity.Y -= 0.5f;
                    drawInfo.DustCache.Add(dust);
                }
            }

            if (Purified)
            {
                if (drawInfo.shadow == 0f)
                {
                    int index2 = Dust.NewDust(Player.position, Player.width, Player.height, 91, 0.0f, 0.0f, 100, default, 2.5f);
                    Dust dust = Main.dust[index2];
                    Main.dust[index2].velocity *= 2f;
                    Main.dust[index2].noGravity = true;
                    drawInfo.DustCache.Add(index2);
                }
            }

            if (Smite)
            {
                if (drawInfo.shadow == 0f)
                {
                    Color color = Main.DiscoColor;
                    int index2 = Dust.NewDust(Player.position, Player.width, Player.height, 91, 0.0f, 0.0f, 100, color, 2.5f);
                    Main.dust[index2].velocity *= 2f;
                    Main.dust[index2].noGravity = true;
                    drawInfo.DustCache.Add(index2);
                }
            }

            if (Anticoagulation)
            {
                if (drawInfo.shadow == 0f)
                {
                    int d = Dust.NewDust(Player.position, Player.width, Player.height, DustID.Blood);
                    Main.dust[d].velocity *= 2f;
                    Main.dust[d].scale += 1f;
                }
            }

            if (Hexed)
            {
                if (Main.rand.NextBool(3) && drawInfo.shadow == 0f)
                {
                    int dust = Dust.NewDust(Player.position - new Vector2(2f, 2f), Player.width, Player.height, DustID.BubbleBlock, Player.velocity.X * 0.4f, Player.velocity.Y * 0.4f, 100, default(Color), 2.5f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 2f;
                    Main.dust[dust].velocity.Y -= 0.5f;
                    Main.dust[dust].color = Color.GreenYellow;
                    drawInfo.DustCache.Add(dust);
                }
                if (Main.rand.NextBool() && drawInfo.shadow == 0f)
                {
                    int index2 = Dust.NewDust(Player.position, Player.width, Player.height, 106, 0.0f, 0.0f, 100, default, 2.5f);
                    Dust dust = Main.dust[index2];
                    Main.dust[index2].noGravity = true;
                    drawInfo.DustCache.Add(index2);
                }
                fullBright = true;
            }

            if (Infested)
            {
                if (Main.rand.NextBool(4) && drawInfo.shadow == 0f)
                {
                    int dust = Dust.NewDust(Player.position - new Vector2(2f, 2f), Player.width, Player.height, 44, Player.velocity.X * 0.4f, Player.velocity.Y * 0.4f, 100, default(Color), InfestedDust);
                    Main.dust[dust].noGravity = true;
                    //Main.dust[dust].velocity *= 1.8f;
                    // Main.dust[dust].velocity.Y -= 0.5f;
                    drawInfo.DustCache.Add(dust);
                }
                fullBright = true;
            }

            if (CurrentLifeReduction > 0)
            {
                if (Main.rand.NextBool() && drawInfo.shadow == 0f)
                {
                    int d = Dust.NewDust(Player.position, Player.width, Player.height, DustID.Blood);
                    Main.dust[d].velocity *= 2f;
                    Main.dust[d].scale += 1f;
                    drawInfo.DustCache.Add(d);
                }
            }

            if (GodEater)
            {
                if (Main.rand.NextBool(3) && drawInfo.shadow == 0f)
                {
                    int dust = Dust.NewDust(Player.position - new Vector2(2f, 2f), Player.width + 4, Player.height + 4, 86, Player.velocity.X * 0.4f, Player.velocity.Y * 0.4f, 100, default(Color), 3f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 1.2f;
                    Main.dust[dust].velocity.Y -= 0.15f;
                    drawInfo.DustCache.Add(dust);
                }
                r *= 0.15f;
                g *= 0.03f;
                b *= 0.09f;
                fullBright = true;
            }

            if (FlamesoftheUniverse)
            {
                /*drawInfo.drawPlayer.onFire = true;
                drawInfo.drawPlayer.onFire2 = true;
                drawInfo.drawPlayer.onFrostBurn = true;
                drawInfo.drawPlayer.ichor = true;
                drawInfo.drawPlayer.burned = true;*/
                if (Main.rand.NextBool(4) && drawInfo.shadow == 0f)
                {
                    int d = Dust.NewDust(Player.position, Player.width, Player.height, 21, Player.velocity.X * 0.2f, Player.velocity.Y * 0.2f, 100, new Color(50 * Main.rand.Next(6) + 5, 50 * Main.rand.Next(6) + 5, 50 * Main.rand.Next(6) + 5), 2.5f);
                    Main.dust[d].velocity.Y -= 1;
                    Main.dust[d].velocity *= 2f;
                    Main.dust[d].noGravity = true;
                    drawInfo.DustCache.Add(d);
                }
                fullBright = true;
            }

            if (CurseoftheMoon)
            {
                if (Main.rand.NextBool(5))
                {
                    int d = Dust.NewDust(Player.Center, 0, 0, 229, Player.velocity.X * 0.4f, Player.velocity.Y * 0.4f);
                    Main.dust[d].noGravity = true;
                    Main.dust[d].velocity *= 3f;
                    drawInfo.DustCache.Add(d);
                }
                if (Main.rand.NextBool(5))
                {
                    int d = Dust.NewDust(Player.position, Player.width, Player.height, 229, Player.velocity.X * 0.4f, Player.velocity.Y * 0.4f);
                    Main.dust[d].noGravity = true;
                    Main.dust[d].velocity.Y -= 1f;
                    Main.dust[d].velocity *= 2f;
                    drawInfo.DustCache.Add(d);
                }
            }

            if (DeathMarked)
            {
                if (Main.rand.NextBool() && drawInfo.shadow == 0f)
                {
                    int dust = Dust.NewDust(Player.position - new Vector2(2f, 2f), Player.width, Player.height, 109, Player.velocity.X * 0.4f, Player.velocity.Y * 0.4f, 0, default(Color), 1.5f);
                    Main.dust[dust].velocity.Y--;
                    if (!Main.rand.NextBool(3))
                    {
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].scale += 0.5f;
                        Main.dust[dust].velocity *= 3f;
                        Main.dust[dust].velocity.Y -= 0.5f;
                    }
                    drawInfo.DustCache.Add(dust);
                }
                r *= 0.2f;
                g *= 0.2f;
                b *= 0.2f;
                fullBright = true;
            }

            if (Fused)
            {
                if (Main.rand.NextBool() && drawInfo.shadow == 0f)
                {
                    int dust = Dust.NewDust(Player.position + new Vector2(Player.width / 2, Player.height / 5), 0, 0, DustID.Torch, Player.velocity.X * 0.4f, Player.velocity.Y * 0.4f, 0, default(Color), 2f);
                    Main.dust[dust].velocity.Y -= 2f;
                    Main.dust[dust].velocity *= 2f;
                    if (Main.rand.NextBool(4))
                    {
                        Main.dust[dust].scale += 0.5f;
                        Main.dust[dust].noGravity = true;
                    }
                    drawInfo.DustCache.Add(dust);
                }
            }

            if (Supercharged)
            {
                if (Main.rand.NextBool() && drawInfo.shadow == 0f)
                {
                    int dust = Dust.NewDust(Player.position, Player.width, Player.height, 229, Player.velocity.X * 0.4f, Player.velocity.Y * 0.4f);
                    Main.dust[dust].scale += 0.5f;
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 1.8f;
                    if (Main.rand.NextBool(3))
                    {
                        Main.dust[dust].noGravity = false;
                        Main.dust[dust].scale *= 0.5f;
                    }
                }
            }

            if (ForbiddenEnchantActive && drawInfo.shadow == 0f)
            {
                Color color12 = Player.GetImmuneAlphaPure(Lighting.GetColor((int)(drawInfo.Position.X + Player.width * 0.5) / 16, (int)(drawInfo.Position.Y + Player.height * 0.5) / 16, Color.White), drawInfo.shadow);
                Color color21 = Color.Lerp(color12, value2: Color.White, 0.7f);

                Texture2D texture2D2 = TextureAssets.Extra[74].Value;
                Texture2D texture = TextureAssets.GlowMask[217].Value;
                bool flag8 = !Player.setForbiddenCooldownLocked;
                int num52 = (int)((Player.miscCounter / 300f * 6.28318548f).ToRotationVector2().Y * 6f);
                float num53 = (Player.miscCounter / 75f * 6.28318548f).ToRotationVector2().X * 4f;
                Color color22 = new Color(80, 70, 40, 0) * (num53 / 8f + 0.5f) * 0.8f;
                if (!flag8)
                {
                    num52 = 0;
                    num53 = 2f;
                    color22 = new Color(80, 70, 40, 0) * 0.3f;
                    color21 = color21.MultiplyRGB(new Color(0.5f, 0.5f, 1f));
                }
                Vector2 vector4 = new Vector2(((int)(drawInfo.Position.X - Main.screenPosition.X - (Player.bodyFrame.Width / 2) + (Player.width / 2))), ((int)(drawInfo.Position.Y - Main.screenPosition.Y + Player.height - Player.bodyFrame.Height + 4f))) + Player.bodyPosition + new Vector2((Player.bodyFrame.Width / 2), (Player.bodyFrame.Height / 2));
                vector4 += new Vector2((float)(-(float)Player.direction * 10), (float)(-20 + num52));
                DrawData value = new DrawData(texture2D2, vector4, null, color21, Player.bodyRotation, texture2D2.Size() / 2f, 1f, drawInfo.playerEffect, 0);

                int num6 = 0;
                if (Player.dye[1] != null)
                {
                    num6 = Player.dye[1].dye;
                }
                value.shader = num6;
                drawInfo.DrawDataCache.Add(value);
                for (float num54 = 0f; num54 < 4f; num54 += 1f)
                {
                    value = new DrawData(texture, vector4 + (num54 * 1.57079637f).ToRotationVector2() * num53, null, color22, Player.bodyRotation, texture2D2.Size() / 2f, 1f, drawInfo.playerEffect, 0);
                    drawInfo.DrawDataCache.Add(value);
                }
            }
        }

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            if (proj.hostile)
                return;

            if (SpiderEnchantActive && FargoSoulsUtil.IsSummonDamage(proj))
            {
                if (Main.rand.Next(100) < Player.ActualClassCrit(DamageClass.Summon))
                    crit = true;
            }

            if (apprenticeBonusDamage)
            {
                if (ShadowForce)
                {
                    damage = (int)(damage * 2.5f);
                }
                else
                {
                    damage = (int)(damage * 1.5f);
                }

                apprenticeBonusDamage = false;
                apprenticeSwitchReady = false;
                ApprenticeCD = 0;

                //dust
                int dustId = Dust.NewDust(new Vector2(proj.position.X, proj.position.Y + 2f), proj.width, proj.height + 5, DustID.FlameBurst, 0, 0, 100, Color.Black, 2f);
                Main.dust[dustId].noGravity = true;

                int blastDamage = damage;
                if (!TerrariaSoul)
                    blastDamage = Math.Min(blastDamage, FargoSoulsUtil.HighestDamageTypeScaling(Player, 300));
                Projectile.NewProjectile(Player.GetSource_Misc(""), target.Center, Vector2.Zero, ProjectileID.InfernoFriendlyBlast, blastDamage, 0, Player.whoAmI);
            }

            if (Hexed || (ReverseManaFlow && proj.CountsAsClass(DamageClass.Magic)))
            {
                target.life += damage;
                target.HealEffect(damage);

                if (target.life > target.lifeMax)
                {
                    target.life = target.lifeMax;
                }

                damage = 0;
                knockback = 0;
                crit = false;

                return;

            }

            if (SqueakyToy)
            {
                damage = 1;
                Squeak(target.Center);
                return;
            }

            if (Asocial && FargoSoulsUtil.IsSummonDamage(proj, true, false))
            {
                damage = 0;
                knockback = 0;
                crit = false;
            }

            if (Atrophied && (proj.CountsAsClass(DamageClass.Melee) || proj.CountsAsClass(DamageClass.Throwing)))
            {
                damage = 0;
                knockback = 0;
                crit = false;
            }

            if (TungstenEnchantItem != null && proj.GetGlobalProjectile<FargoSoulsGlobalProjectile>().TungstenScale != 1)
            {
                TungstenEnchant.TungstenModifyDamage(Player, ref damage, ref crit, proj.DamageType);
            }

            if (HuntressEnchantActive && proj.GetGlobalProjectile<FargoSoulsGlobalProjectile>().HuntressProj == 1)
            {
                HuntressEnchant.HuntressBonus(this, proj, target, ref damage);
            }

            ModifyHitNPCBoth(target, ref damage, ref crit, proj.DamageType);
        }

        public override void ModifyHitNPC(Item item, NPC target, ref int damage, ref float knockback, ref bool crit)
        {
			if (NinjaEnchantItem != null && Player.GetToggleValue("NinjaSpeed"))
			{
				damage /= 2;
			}
			
            if (Hexed || (ReverseManaFlow && item.CountsAsClass(DamageClass.Magic)))
            {
                target.life += damage;
                target.HealEffect(damage);

                if (target.life > target.lifeMax)
                {
                    target.life = target.lifeMax;
                }

                damage = 0;
                knockback = 0;
                crit = false;

                return;

            }

            if (SqueakyToy)
            {
                damage = 1;
                Squeak(target.Center);
                return;
            }

            if (Atrophied)
            {
                damage = 0;
                knockback = 0;
                crit = false;
            }

            if (TungstenEnchantItem != null && Toggler != null && Player.GetToggleValue("Tungsten")
                && (TerraForce || item.shoot == ProjectileID.None))
            {
                TungstenEnchant.TungstenModifyDamage(Player, ref damage, ref crit, item.DamageType);
            }

            ModifyHitNPCBoth(target, ref damage, ref crit, item.DamageType);
        }

        public void ModifyHitNPCBoth(NPC target, ref int damage, ref bool crit, DamageClass damageClass)
        {
            if (crit)
            {
                if (Eternity)
                    damage *= 5;
                else if (UniverseCore)
                    damage *= 2;

                if (SpiderEnchantActive && damageClass.CountsAsClass(DamageClass.Summon) && !TerrariaSoul)
                    damage = (int)Math.Round(damage * 0.75);
            }

            if (DeerSinewNerf)
            {
                float ratio = Math.Min(Player.velocity.Length() / 20f, 1f);
                damage = (int)Math.Round(damage * MathHelper.Lerp(1f, 0.85f, ratio));
            }

            if (CerebralMindbreak)
                damage = (int)(0.7 * damage);

            if (FirstStrike)
            {
                crit = true;
                damage = (int)(damage * 1.5f);
                Player.ClearBuff(ModContent.BuffType<FirstStrike>());
                //target.defense -= 5;
                target.AddBuff(BuffID.BrokenArmor, 600);
            }
        }

        public override void ModifyHitPvp(Item item, Player target, ref int damage, ref bool crit)
        {
            if (!SqueakyToy) return;
            damage = 1;
            Squeak(target.Center);
        }

        public override void ModifyHitPvpWithProj(Projectile proj, Player target, ref int damage, ref bool crit)
        {
            if (!SqueakyToy) return;
            damage = 1;
            Squeak(target.Center);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit)
        {
            if (target.type == NPCID.TargetDummy || target.friendly)
                return;

            if (proj.minion)// && proj.type != ModContent.ProjectileType<CelestialRuneAncientVision>() && proj.type != ModContent.ProjectileType<SpookyScythe>())
                TryAdditionalAttacks(proj.damage, proj.DamageType);

            OnHitNPCEither(target, damage, knockback, crit, proj.DamageType, projectile: proj);

            if (OriEnchantItem != null && proj.type == ProjectileID.FlowerPetal)
            {
                target.AddBuff(ModContent.BuffType<OriPoison>(), 300);
                target.immune[proj.owner] = 2;
            }
        }

        private void OnHitNPCEither(NPC target, int damage, float knockback, bool crit, DamageClass damageClass, Projectile projectile = null, Item item = null)
        {
            //doing this so that damage-inheriting effects dont double dip or explode due to taking on crit boost
            int GetBaseDamage()
            {
                int baseDamage = damage;
                if (projectile != null)
                    baseDamage = projectile.damage;
                else if (item != null)
                    baseDamage = Player.GetWeaponDamage(item);
                return baseDamage;
            }

            if (StyxSet)
            {
                StyxMeter += damage;
                if (StyxTimer <= 0 && !target.friendly && target.lifeMax > 5 && target.type != NPCID.TargetDummy)
                    StyxTimer = 60;
            }

            if (BeetleEnchantActive && Player.beetleOffense && damageClass != DamageClass.Melee)
            {
                Player.beetleCounter += damage;
            }

            if (PearlwoodEnchantItem != null && Player.GetToggleValue("Pearl") && PearlwoodCD == 0 && !(projectile != null && projectile.type == ProjectileID.FairyQueenMagicItemShot && projectile.usesIDStaticNPCImmunity && projectile.GetGlobalProjectile<FargoSoulsGlobalProjectile>().noInteractionWithNPCImmunityFrames))
            {
                PearlwoodEnchant.PearlwoodStarDrop(this, target, GetBaseDamage());
            }

            if (BeeEnchantActive && Player.GetToggleValue("Bee") && BeeCD <= 0 && target.realLife == -1
                && (projectile == null || (projectile.type != ProjectileID.Bee && projectile.type != ProjectileID.GiantBee && projectile.maxPenetrate != 1 && !projectile.usesLocalNPCImmunity && !projectile.usesIDStaticNPCImmunity && projectile.owner == Main.myPlayer)))
            {
                bool force = LifeForce;
                if (force || Main.rand.NextBool())
                {
                    int beeDamage = GetBaseDamage();
                    if (beeDamage > 0)
                    {
                        if (!TerrariaSoul)
                            beeDamage = Math.Min(beeDamage, FargoSoulsUtil.HighestDamageTypeScaling(Player, 300));

                        float beeKB = projectile != null ? projectile.knockBack : item != null ? item.knockBack : knockback;

                        int p = Projectile.NewProjectile(item != null ? Player.GetSource_ItemUse(item) : projectile.GetSource_FromThis(), target.Center.X, target.Center.Y, Main.rand.Next(-35, 36) * 0.2f, Main.rand.Next(-35, 36) * 0.2f,
                            force ? ProjectileID.GiantBee : Player.beeType(), beeDamage, Player.beeKB(beeKB), Player.whoAmI);
                        if (p != Main.maxProjectiles)
                            Main.projectile[p].DamageType = damageClass;
                    }
                    BeeCD = 15;
                }
            }

            if (PalladEnchantItem != null && !Player.onHitRegen)
            {
                Player.AddBuff(BuffID.RapidHealing, Math.Min(300, damage / 3)); //heal time based on damage dealt, capped at 5sec
            }

            if (CopperEnchantItem != null && crit)
            {
                CopperEnchant.CopperProc(this, target);
            }

            if (ShadewoodEnchantItem != null)
            {
                ShadewoodEnchant.ShadewoodProc(this, target, projectile);
            }

            if (TitaniumEnchantItem != null && (projectile == null || projectile.type != ProjectileID.TitaniumStormShard))
            {
                TitaniumEnchant.TitaniumShards(this, Player);
            }


            if (Player.GetToggleValue("Obsidian") && ObsidianEnchantItem != null && ObsidianCD == 0)
            {
                ObsidianEnchant.ObsidianProc(this, target, GetBaseDamage());
            }        

            if (DevianttHeartItem != null && DevianttHeartsCD <= 0 && Player.GetToggleValue("MasoDevianttHearts")
                && (projectile == null || (projectile.type != ModContent.ProjectileType<FriendRay>() && projectile.type != ModContent.ProjectileType<FriendHeart>())))
            {
                DevianttHeartsCD = AbomWandItem == null ? 600 : 300;

                if (Main.myPlayer == Player.whoAmI)
                {
                    Vector2 offset = 300 * Player.DirectionFrom(Main.MouseWorld);
                    for (int i = -3; i <= 3; i++)
                    {
                        Vector2 spawnPos = Player.Center + offset.RotatedBy(Math.PI / 7 * i);
                        Vector2 speed = Vector2.Normalize(Main.MouseWorld - spawnPos);

                        int baseHeartDamage = AbomWandItem == null ? 17 : 170;
                        //heartDamage = (int)(heartDamage * Player.ActualClassDamage(DamageClass.Summon));

                        float ai1 = (Main.MouseWorld - spawnPos).Length() / 17;

                        if (MutantEyeItem == null)
                            FargoSoulsUtil.NewSummonProjectile(Player.GetSource_Accessory(DevianttHeartItem), spawnPos, 17f * speed, ModContent.ProjectileType<FriendHeart>(), baseHeartDamage, 3f, Player.whoAmI, -1, ai1);
                        else
                            FargoSoulsUtil.NewSummonProjectile(Player.GetSource_Accessory(DevianttHeartItem), spawnPos, speed, ModContent.ProjectileType<FriendRay>(), baseHeartDamage, 3f, Player.whoAmI, (float)Math.PI / 7 * i);

                        FargoSoulsUtil.HeartDust(spawnPos, speed.ToRotation());
                    }
                }
            }

            if (SnowEnchantActive)
            {
                target.AddBuff(BuffID.Frostburn, 120);
            }

            if (GodEaterImbue)
            {
                /*if (target.FindBuffIndex(ModContent.BuffType<GodEater>()) < 0 && target.aiStyle != 37)
                {
                    if (target.type != ModContent.NPCType<NPCs.MutantBoss.MutantBoss>())
                    {
                        target.DelBuff(4);
                        target.buffImmune[ModContent.BuffType<GodEater>()] = false;
                    }
                }*/
                target.AddBuff(ModContent.BuffType<GodEater>(), 420);
            }

            if (GladiatorEnchantActive && Player.whoAmI == Main.myPlayer && Player.GetToggleValue("Gladiator") && GladiatorCD <= 0 && (projectile == null || projectile.type != ModContent.ProjectileType<GladiatorJavelin>()))
            {
                GladiatorEnchant.GladiatorSpearDrop(this, target, GetBaseDamage());
            }

            if (SolarEnchantActive && Player.GetToggleValue("SolarFlare") && Main.rand.NextBool(4))
                target.AddBuff(ModContent.BuffType<SolarFlare>(), 300);

            if (TinEnchantItem != null)
            {
                TinEnchant.TinOnHitEnemy(this, damage, crit);
            }

            if (LeadEnchantItem != null)
            {
                target.AddBuff(ModContent.BuffType<LeadPoison>(), 30);
            }


            //            /*if (PalladEnchant && !TerrariaSoul && palladiumCD == 0 && !target.immortal && !Player.moonLeech)
            //            {
            //                int heal = damage / 10;

            //                if ((EarthForce) && heal > 16)
            //                    heal = 16;
            //                else if (!EarthForce && !WizardEnchant && heal > 8)
            //                    heal = 8;
            //                else if (heal < 1)
            //                    heal = 1;
            //                Player.statLife += heal;
            //                Player.HealEffect(heal);
            //                palladiumCD = 240;
            //            }*/

            if (NymphsPerfume && NymphsPerfumeCD <= 0 && !target.immortal && !Player.moonLeech)
            {
                NymphsPerfumeCD = 600;

                if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    Item.NewItem(Player.GetSource_OnHit(target), target.Hitbox, ItemID.Heart);
                }
                else if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    var netMessage = Mod.GetPacket();
                    netMessage.Write((byte)FargowiltasSouls.PacketID.RequestPerfumeHeart);
                    netMessage.Write((byte)Player.whoAmI);
                    netMessage.Write((byte)target.whoAmI);
                    netMessage.Send();
                }
            }

            if (UniverseCore)
                target.AddBuff(ModContent.BuffType<FlamesoftheUniverse>(), 240);

            if (MasochistSoul)
            {
                target.AddBuff(ModContent.BuffType<Sadism>(), 600);
                //if (target.FindBuffIndex(ModContent.BuffType<Sadism>()) < 0 && target.aiStyle != 37)
                //{
                //    if (target.type != ModContent.NPCType<MutantBoss>())
                //    {
                //        target.DelBuff(4);
                //        target.buffImmune[ModContent.BuffType<Sadism>()] = false;
                //    }
                //    target.AddBuff(ModContent.BuffType<Sadism>(), 600);
                //}
            }

            if (FusedLens)
            {
                if (Player.onFire2 || FusedLensCanDebuff)
                    target.AddBuff(BuffID.CursedInferno, 360);
                if (Player.ichor || FusedLensCanDebuff)
                    target.AddBuff(BuffID.Ichor, 360);
            }

            if (!TerrariaSoul)
            {
                if (AncientShadowEnchantActive && Player.GetToggleValue("AncientShadow") && (projectile == null || projectile.type != ProjectileID.ShadowFlame) && Main.rand.NextBool(5))
                    target.AddBuff(BuffID.Darkness, 600, true);
            }

            if (Supercharged)
            {
                target.AddBuff(BuffID.Electrified, 240);
                target.AddBuff(ModContent.BuffType<LightningRod>(), 60);
            }

            if (GoldEnchantActive)
                target.AddBuff(BuffID.Midas, 120, true);

            if (DragonFang && !target.boss && !target.buffImmune[ModContent.BuffType<ClippedWings>()] && Main.rand.NextBool(10))
            {
                target.velocity.X = 0f;
                target.velocity.Y = 10f;
                target.AddBuff(ModContent.BuffType<ClippedWings>(), 240);
                target.netUpdate = true;
            }

            if (SpectreEnchantActive && Player.GetToggleValue("Spectre") && !target.immortal && SpectreCD <= 0 && Main.rand.NextBool())
            {
                if (projectile == null)
                {
                    //forced orb spawn reeeee
                    float num = 4f;
                    float speedX = Main.rand.Next(-100, 101);
                    float speedY = Main.rand.Next(-100, 101);
                    float num2 = (float)Math.Sqrt((double)(speedX * speedX + speedY * speedY));
                    num2 = num / num2;
                    speedX *= num2;
                    speedY *= num2;
                    Projectile p = FargoSoulsUtil.NewProjectileDirectSafe(Player.GetSource_Misc(""), target.position, new Vector2(speedX, speedY), ProjectileID.SpectreWrath, damage / 2, 0, Player.whoAmI, target.whoAmI);

                    if ((SpiritForce || (crit && Main.rand.NextBool(5))) && p != null)
                    {
                        SpectreHeal(target, p);
                        SpectreCD = SpiritForce ? 5 : 20;
                    }
                }
                else if (projectile.type != ProjectileID.SpectreWrath)
                {
                    SpectreHurt(projectile);

                    if (SpiritForce || (crit && Main.rand.NextBool(5)))
                        SpectreHeal(target, projectile);

                    SpectreCD = SpiritForce ? 5 : 20;
                }
            }

            if (AbomWandItem != null)
            {
                //target.AddBuff(ModContent.BuffType<OceanicMaul>(), 900);
                //target.AddBuff(ModContent.BuffType<CurseoftheMoon>(), 900);
                if (crit && AbomWandCD <= 0 && Player.GetToggleValue("MasoFishron") && (projectile == null || projectile.type != ModContent.ProjectileType<AbomScytheFriendly>()))
                {
                    AbomWandCD = 360;

                    float screenX = Main.screenPosition.X;
                    if (Player.direction < 0)
                        screenX += Main.screenWidth;
                    float screenY = Main.screenPosition.Y;
                    screenY += Main.rand.Next(Main.screenHeight);
                    Vector2 spawn = new Vector2(screenX, screenY);
                    Vector2 vel = target.Center - spawn;
                    vel.Normalize();
                    vel *= 27f;

                    int dam = 150;
                    if (MutantEyeItem != null)
                        dam *= 3;

                    if (projectile != null && FargoSoulsUtil.IsSummonDamage(projectile))
                    {
                        int p = FargoSoulsUtil.NewSummonProjectile(Player.GetSource_Accessory(AbomWandItem), spawn, vel, ModContent.ProjectileType<SpectralAbominationn>(), dam, 10f, Player.whoAmI, target.whoAmI);
                        if (p != Main.maxProjectiles)
                            Main.projectile[p].DamageType = DamageClass.Summon;
                    }
                    else
                    {
                        dam = (int)(dam * Player.ActualClassDamage(damageClass));

                        int p = Projectile.NewProjectile(Player.GetSource_Accessory(AbomWandItem), spawn, vel, ModContent.ProjectileType<SpectralAbominationn>(), dam, 10f, Player.whoAmI, target.whoAmI);
                        if (p != Main.maxProjectiles)
                            Main.projectile[p].DamageType = damageClass;
                    }
                }
            }

            if (DarkenedHeartItem != null)
                DarkenedHeartAttack(projectile);

            if (NebulaEnchantActive)
                NebulaOnHit(target, projectile, damageClass);
        }

        public override void OnHitNPC(Item item, NPC target, int damage, float knockback, bool crit)
        {
            if (target.type == NPCID.TargetDummy || target.friendly)
                return;

            OnHitNPCEither(target, damage, knockback, crit, item.DamageType, item: item);
        }

        public override void MeleeEffects(Item item, Rectangle hitbox)
        {
            if (ShroomEnchantActive && Player.GetToggleValue("ShroomiteShroom"))
                ShroomiteMeleeEffect(item, hitbox);
        }

        public override bool CanBeHitByNPC(NPC npc, ref int CooldownSlot)
        {
            if (BetsyDashing || GoldShell)
                return false;
            return true;
        }

        public override bool CanBeHitByProjectile(Projectile proj)
        {
            if (BetsyDashing || GoldShell)
                return false;
            if (PrecisionSealHurtbox && !proj.Colliding(proj.Hitbox, GetPrecisionHurtbox()))
                return false;
            return true;
        }

        public override void ModifyHitByNPC(NPC npc, ref int damage, ref bool crit)
        {
            if (NecromanticBrewItem != null && IsInADashState)
            {
                damage /= 4;
            }

            TitaniumEnchant.TryTitaniumDR(this, npc);

            if (GladiatorEnchantActive && Player.direction == Math.Sign(npc.Center.X - Player.Center.X))
                Player.noKnockback = true;

            if (Smite)
                damage = (int)(damage * 1.2);

            if (npc.coldDamage && Hypothermia)
                damage = (int)(damage * 1.2);

            if (npc.GetGlobalNPC<FargoSoulsGlobalNPC>().CurseoftheMoon)
                damage = (int)(damage * 0.8);
        }

        public override void ModifyHitByProjectile(Projectile proj, ref int damage, ref bool crit)
        {
            if (GroundStick)
            {
                GroundStickCheck(proj, ref damage);
            }

            TitaniumEnchant.TryTitaniumDR(this, proj);

            if (GladiatorEnchantActive && Player.direction == Math.Sign(proj.Center.X - Player.Center.X))
                Player.noKnockback = true;

            if (Smite)
                damage = (int)(damage * 1.2);

            if (proj.coldDamage && Hypothermia)
                damage = (int)(damage * 1.2);

            //if (npc.GetGlobalNPC<FargoSoulsGlobalNPC>().CurseoftheMoon)
            //damage = (int)(damage * 0.8);
        }

        public override void OnHitByNPC(NPC npc, int damage, bool crit)
        {
            OnHitByEither(npc, null, damage, crit);
        }

        public override void OnHitByProjectile(Projectile proj, int damage, bool crit)
        {
            OnHitByEither(null, proj, damage, crit);
        }

        public void OnHitByEither(NPC npc, Projectile proj, int damage, bool crit)
        {
            if (Anticoagulation && Main.myPlayer == Player.whoAmI)
            {
                Entity source = null;
                if (npc != null)
                    source = npc;
                else if (proj != null)
                    source = proj;

                int type = ModContent.ProjectileType<Bloodshed>();
                for (int i = 0; i < 6; i++)
                {
                    if (Main.rand.NextBool(Player.ownedProjectileCounts[type] + 2))
                    {
                        const float speed = 12f;
                        Projectile.NewProjectile(Player.GetSource_OnHurt(source), Player.Center, Main.rand.NextVector2Circular(speed, speed), type, 0, 0f, Main.myPlayer, 0f);
                    }
                }
            }

            if (ModContent.GetInstance<SoulConfig>().BigTossMode)
            {
                AddBuffNoStack(ModContent.BuffType<Stunned>(), 120);

                Vector2 attacker = default;
                if (npc != null)
                    attacker = npc.Center;
                else if (proj != null)
                    attacker = proj.Center;
                if (attacker != default)
                    Player.velocity = Vector2.Normalize(Player.Center - attacker) * 30;
            }
        }

        public void ConcentratedRainbowMatterTryAutoHeal()
        {
            if (ConcentratedRainbowMatter
                && Player.statLife < Player.statLifeMax2
                && Player.potionDelay <= 0
                && Player.GetToggleValue("MasoHealingPotion", false))
            {
                Item potion = Player.QuickHeal_GetItemToUse();
                if (potion != null)
                {
                    int heal = getHealMultiplier(potion.healLife);
                    if (Player.statLife < Player.statLifeMax2 - heal && //only heal when full benefit (no wasted overheal)
                        (Player.statLife < Player.statLifeMax2 * 0.4 || //heal when very low or when danger nearby (not after respawn in safety)
                        Main.npc.Any(n => n.active && n.damage > 0 && !n.friendly
                                     && Player.Distance(n.Center) < 1200 && (n.noTileCollide || Collision.CanHitLine(Player.Center, 0, 0, n.Center, 0, 0)))))
                    {
                        Player.QuickHeal();
                    }
                }
            }
        }

        public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource, ref int cooldownCounter)
        {
            if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.deviBoss, ModContent.NPCType<DeviBoss>()))
                ((DeviBoss)Main.npc[EModeGlobalNPC.deviBoss].ModNPC).playerInvulTriggered = true;

            if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.abomBoss, ModContent.NPCType<AbomBoss>()))
                ((AbomBoss)Main.npc[EModeGlobalNPC.abomBoss].ModNPC).playerInvulTriggered = true;

            if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.mutantBoss, ModContent.NPCType<MutantBoss>()))
                ((MutantBoss)Main.npc[EModeGlobalNPC.mutantBoss].ModNPC).playerInvulTriggered = true;

            if (DeathMarked)
                damage = (int)(damage * 1.5);

            if (Player.whoAmI == Main.myPlayer && !noDodge && SqueakyAcc && Player.GetToggleValue("MasoSqueak") && Main.rand.NextBool(10))
            {
                Squeak(Player.Center);
                damage = 1;
            }

            if (TryParryAttack(ref damage))
                return false;

            if (CrimsonEnchantActive && Player.GetToggleValue("Crimson"))
            {
                CrimsonEnchant.CrimsonHurt(Player, this, ref damage);
            }

            if (StyxSet && !BetsyDashing && !GoldShell && damage > 1 && Player.ownedProjectileCounts[ModContent.ProjectileType<StyxArmorScythe>()] > 0)
            {
                int scythesSacrificed = 0;
                const int maxSacrifice = 4;
                const double maxDR = 0.20;
                int scytheType = ModContent.ProjectileType<StyxArmorScythe>();
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    if (Main.projectile[i].active && Main.projectile[i].type == scytheType && Main.projectile[i].owner == Player.whoAmI)
                    {
                        if (Player.whoAmI == Main.myPlayer)
                            Main.projectile[i].Kill();
                        if (++scythesSacrificed >= maxSacrifice)
                            break;
                    }
                }

                damage = (int)(damage * (1.0 - maxDR / maxSacrifice * scythesSacrificed));
            }

            if (DeerSinewNerf && DeerSinewFreezeCD <= 0 && (damageSource.SourceNPCIndex != -1 || (damageSource.SourceProjectileIndex != -1 && Main.projectile[damageSource.SourceProjectileIndex].aiStyle != ProjAIStyleID.FallingTile)))
            {
                DeerSinewFreezeCD = 120;
                FargoSoulsUtil.AddDebuffFixedDuration(Player, BuffID.Frozen, 20);
            }

            return true;
        }

        public void OnHurtEffects(double damage)
        {
            if (HurtTimer <= 0)
            {
                HurtTimer = 20;

                if (CelestialRuneItem != null && Player.GetToggleValue("MasoVision"))
                {
                    if (MoonChalice)
                    {
                        int dam = 50;
                        if (MasochistSoul)
                            dam *= 2;
                        for (int i = 0; i < 5; i++)
                        {
                            Projectile.NewProjectile(Player.GetSource_Accessory(CelestialRuneItem), Player.Center, Main.rand.NextVector2Circular(20, 20),
                                    ModContent.ProjectileType<AncientVision>(), (int)(dam * Player.ActualClassDamage(DamageClass.Summon)), 6f, Player.whoAmI);
                        }
                    }
                    else
                    {
                        Projectile.NewProjectile(Player.GetSource_Accessory(CelestialRuneItem), Player.Center, new Vector2(0, -10), ModContent.ProjectileType<AncientVision>(),
                            (int)(40 * Player.ActualClassDamage(DamageClass.Summon)), 3f, Player.whoAmI);
                    }
                }
            }

            if (CobaltEnchantItem != null)
                CobaltEnchant.CobaltHurt(Player, damage);

            if (FossilEnchantItem != null)
                FossilEnchant.FossilHurt(this, (int)damage);
        }

        public override void Hurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit, int cooldownCounter)
        {
            WasHurtBySomething = true;

            MahoganyCanUseDR = false;
			
			if (Player.HasBuff(ModContent.BuffType<TitaniumDRBuff>())
				&& !Player.HasBuff(ModContent.BuffType<TitaniumCD>()))
			{
				Player.AddBuff(ModContent.BuffType<TitaniumCD>(), 60 * 10);
			}

            if (NekomiSet)
            {
                const int heartsLost = 1;
                int meterPerHeart = NekomiHood.MAX_METER / NekomiHood.MAX_HEARTS;
                int meterLost = meterPerHeart * heartsLost;

                int heartsToConsume = NekomiMeter / meterPerHeart;
                if (heartsToConsume > heartsLost)
                    heartsToConsume = heartsLost;
                Player.AddBuff(BuffID.RapidHealing, heartsToConsume * 60 * 5 / heartsLost);

                NekomiMeter -= meterLost;
                if (NekomiMeter < 0)
                    NekomiMeter = 0;
            }

            if (BeetleEnchantActive)
                BeetleHurt();

            if (TinEnchantItem != null)
                TinEnchant.TinHurt(this);

            if (ShellHide)
            {
                TurtleShellHP--;

                //some funny dust
                const int max = 30;
                for (int i = 0; i < max; i++)
                {
                    Vector2 vector6 = Vector2.UnitY * 5f;
                    vector6 = vector6.RotatedBy((i - (max / 2 - 1)) * 6.28318548f / max) + Main.LocalPlayer.Center;
                    Vector2 vector7 = vector6 - Main.LocalPlayer.Center;
                    int d = Dust.NewDust(vector6 + vector7, 0, 0, DustID.GoldFlame, 0f, 0f, 0, default(Color), 2f);
                    Main.dust[d].noGravity = true;
                    Main.dust[d].velocity = vector7;
                }
            }

            OnHurtEffects(damage);

            if (Midas && Main.myPlayer == Player.whoAmI)
                Player.DropCoins();

            DeviGrazeBonus = 0;
            DeviGrazeCounter = 0;
        }

        private PlayerDeathReason DeathByLocalization(string key)
        {
            string death = Language.GetTextValue($"Mods.FargowiltasSouls.DeathMessage.{key}");
            if (FargoSoulsUtil.IsChinese())
            {
                return PlayerDeathReason.ByCustomReason($"{Player.name}{death}");
            }
            else
            {
                return PlayerDeathReason.ByCustomReason($"{Player.name} {death}");
            }
        }

        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            bool retVal = true;

            if (Player.statLife <= 0) //revives
            {
                if (Player.whoAmI == Main.myPlayer && retVal && AbomRebirth)
                {
                    if (!WasHurtBySomething)
                    {
                        Player.statLife = 1;
                        return false; //short circuits the rest, this is deliberate
                    }
                }

                if (Player.whoAmI == Main.myPlayer && retVal && MutantSetBonusItem != null && Player.FindBuffIndex(ModContent.BuffType<MutantRebirth>()) == -1)
                {
                    Player.statLife = Player.statLifeMax2;
                    Player.HealEffect(Player.statLifeMax2);
                    Player.immune = true;
                    Player.immuneTime = 180;
                    Player.hurtCooldowns[0] = 180;
                    Player.hurtCooldowns[1] = 180;
                    string text = Language.GetTextValue($"Mods.{Mod.Name}.Message.Revived");
                    Main.NewText(text, Color.LimeGreen);
                    Player.AddBuff(ModContent.BuffType<MutantRebirth>(), 120 * 60);
                    retVal = false;

                    Projectile.NewProjectile(Player.GetSource_Accessory(MutantSetBonusItem), Player.Center, -Vector2.UnitY, ModContent.ProjectileType<GiantDeathray>(), (int)(7000 * Player.ActualClassDamage(DamageClass.Magic)), 10f, Player.whoAmI);
                }

                if (Player.whoAmI == Main.myPlayer && retVal && FossilEnchantItem != null && Player.FindBuffIndex(ModContent.BuffType<FossilReviveCD>()) == -1)
                {
                    FossilEnchant.FossilRevive(this);
                    retVal = false;
                }

                if (Player.whoAmI == Main.myPlayer && retVal && AbomWandItem != null && !AbominableWandRevived)
                {
                    AbominableWandRevived = true;
                    int heal = 1;
                    Player.statLife = heal;
                    Player.HealEffect(heal);
                    Player.immune = true;
                    Player.immuneTime = 120;
                    Player.hurtCooldowns[0] = 120;
                    Player.hurtCooldowns[1] = 120;
                    string text = Language.GetTextValue($"Mods.{Mod.Name}.Message.Revived");
                    CombatText.NewText(Player.Hitbox, Color.Yellow, text, true);
                    Main.NewText(text, Color.Yellow);
                    Player.AddBuff(ModContent.BuffType<AbomRebirth>(), 900);
                    retVal = false;
                    for (int i = 0; i < 24; i++)
                    {
                        Projectile.NewProjectile(Player.GetSource_Accessory(AbomWandItem), Player.Center, Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(4f, 16f),
                            ModContent.ProjectileType<StyxArmorScythe2>(), 0, 10f, Main.myPlayer, -60 - Main.rand.Next(60), -1);
                    }
                }
            }

            //killed by damage over time
            if (damage == 10.0 && hitDirection == 0 && damageSource.SourceOtherIndex == 8)
            {
                if (Infested)
                    damageSource = DeathByLocalization("Infested");

                if (Anticoagulation)
                    damageSource = DeathByLocalization("Anticoagulation");

                if (Rotting)
                    damageSource = DeathByLocalization("Rotting");

                if (Shadowflame)
                    damageSource = DeathByLocalization("Shadowflame");

                if (NanoInjection)
                    damageSource = DeathByLocalization("NanoInjection");

                if (GodEater || FlamesoftheUniverse || CurseoftheMoon || MutantFang)
                    damageSource = DeathByLocalization("DivineWrath");
            }

            /*if (MutantPresence)
            {
                damageSource = PlayerDeathReason.ByCustomReason(Player.name + " was penetrated.");
            }*/

            if (StatLifePrevious > 0 && Player.statLife > StatLifePrevious)
                StatLifePrevious = Player.statLife;

            return retVal;
        }

        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        {
            if (GaiaOffense) //set armor and accessory shaders to gaia shader if set bonus is triggered
            {
                int gaiaShader = GameShaders.Armor.GetShaderIdFromItemId(ModContent.ItemType<GaiaDye>());
                drawInfo.cBody = gaiaShader;
                drawInfo.cHead = gaiaShader;
                drawInfo.cLegs = gaiaShader;
                drawInfo.cWings = gaiaShader;
                drawInfo.cHandOn = gaiaShader;
                drawInfo.cHandOff = gaiaShader;
                drawInfo.cShoe = gaiaShader;
            }

            if (GuardRaised)
            {
                Player.bodyFrame.Y = Player.bodyFrame.Height * 10;
                if (shieldTimer > 0)
                {
                    List<int> shaders = new List<int>();
                    shaders.Add(GameShaders.Armor.GetShaderIdFromItemId(ItemID.ReflectiveSilverDye));
                    if (DreadShellItem != null)
                        shaders.Add(GameShaders.Armor.GetShaderIdFromItemId(ItemID.BloodbathDye));
                    if (PumpkingsCapeItem != null)
                        shaders.Add(GameShaders.Armor.GetShaderIdFromItemId(ItemID.PixieDye));

                    int shader = shaders[(int)(Main.GameUpdateCount / 4 % shaders.Count)];
                    drawInfo.cBody = shader;
                    drawInfo.cHead = shader;
                    drawInfo.cLegs = shader;
                    drawInfo.cWings = shader;
                    drawInfo.cHandOn = shader;
                    drawInfo.cHandOff = shader;
                    drawInfo.cShoe = shader;
                    drawInfo.cBack = shader;
                    drawInfo.cBackpack = shader;
                    drawInfo.cShield = shader;
                    drawInfo.cNeck = shader;
                    drawInfo.cHandOn = shader;
                    drawInfo.cHandOff = shader;
                    drawInfo.cBalloon = shader;
                    drawInfo.cBalloonFront = shader;
                    drawInfo.cFace = shader;
                    drawInfo.cFaceHead = shader;
                    drawInfo.cFront = shader;
                }
            }
        }

        public void AddPet(bool toggle, bool vanityToggle, int buff, int proj)
        {
            if (vanityToggle)
            {
                PetsActive = false;
                return;
            }

            if (Player.whoAmI == Main.myPlayer && toggle && Player.FindBuffIndex(buff) == -1 && Player.ownedProjectileCounts[proj] < 1)
            {
                Projectile p = Main.projectile[Projectile.NewProjectile(Player.GetSource_Misc("Pet"), Player.Center.X, Player.Center.Y, 0f, -1f, proj, 0, 0f, Player.whoAmI)];
                p.netUpdate = true;
            }
        }

        public void AddMinion(Item item, bool toggle, int proj, int damage, float knockback)
        {
            if (Player.whoAmI != Main.myPlayer) return;
            if (Player.ownedProjectileCounts[proj] < 1 && Player.whoAmI == Main.myPlayer && toggle)
            {
                FargoSoulsUtil.NewSummonProjectile(Player.GetSource_Accessory(item), Player.Center, -Vector2.UnitY, proj, damage, knockback, Main.myPlayer);
            }
        }

        private void KillPets()
        {
            int petId = Player.miscEquips[0].buffType;
            int lightPetId = Player.miscEquips[1].buffType;

            Player.buffImmune[petId] = true;
            Player.buffImmune[lightPetId] = true;

            Player.ClearBuff(petId);
            Player.ClearBuff(lightPetId);

            //memorizes Player selections
            if (!WasAsocial)
            {
                HidePetToggle0 = Player.hideMisc[0];
                HidePetToggle1 = Player.hideMisc[1];

                WasAsocial = true;
            }

            //disables pet and light pet too!
            if (!Player.hideMisc[0])
            {
                Player.TogglePet();
            }

            if (!Player.hideMisc[1])
            {
                Player.ToggleLight();
            }

            Player.hideMisc[0] = true;
            Player.hideMisc[1] = true;
        }

        public void Squeak(Vector2 center)
        {
            if (!Main.dedServ)
                SoundEngine.PlaySound(new SoundStyle($"FargowiltasSouls/Assets/Sounds/SqueakyToy/squeak{Main.rand.Next(1, 7)}"), center);
        }

        private int InfestedExtraDot()
        {
            int buffIndex = Player.FindBuffIndex(ModContent.BuffType<Infested>());
            if (buffIndex == -1)
            {
                buffIndex = Player.FindBuffIndex(ModContent.BuffType<Neurotoxin>());
                if (buffIndex == -1)
                    return 0;
            }

            int timeLeft = Player.buffTime[buffIndex];
            float baseVal = (float)(MaxInfestTime - timeLeft) / 90; //change the denominator to adjust max power of DOT
            int modifier = (int)(baseVal * baseVal + 4);

            InfestedDust = baseVal / 10 + 1f;
            if (InfestedDust > 5f)
                InfestedDust = 5f;

            return modifier * 2;
        }

        //        public override void CatchFish(Item fishingRod, Item bait, int power, int liquidType, int poolSize, int worldLayer, int questFish, ref int caughtType, ref bool junk)
        //        {
        //            if (bait.type == ModContent.ItemType<TruffleWormEX>())
        //            {
        //                caughtType = 0;
        //                bool spawned = false;
        //                for (int i = 0; i < 1000; i++)
        //                {
        //                    if (Main.projectile[i].active && Main.projectile[i].bobber
        //                        && Main.projectile[i].owner == Player.whoAmI && Player.whoAmI == Main.myPlayer)
        //                    {
        //                        Main.projectile[i].ai[0] = 2f; //cut fishing lines
        //                        Main.projectile[i].netUpdate = true;

        //                        if (!spawned && Main.projectile[i].wet && FargoSoulsWorld.EternityMode && !NPC.AnyNPCs(NPCID.DukeFishron)) //should spawn boss
        //                        {
        //                            spawned = true;
        //                            if (Main.netMode == NetmodeID.SinglePlayer) //singlePlayer
        //                            {
        //                                EModeGlobalNPC.spawnFishronEX = true;
        //                                NPC.NewNPC((int)Main.projectile[i].Center.X, (int)Main.projectile[i].Center.Y + 100,
        //                                    NPCID.DukeFishron, 0, 0f, 0f, 0f, 0f, Player.whoAmI);
        //                                EModeGlobalNPC.spawnFishronEX = false;
        //                                Main.NewText("Duke Fishron EX has awoken!", 50, 100, 255);
        //                            }
        //                            else if (Main.netMode == NetmodeID.MultiPlayerClient) //MP, broadcast(?) packet from spawning Player's client
        //                            {
        //                                var netMessage = mod.GetPacket();
        //                                netMessage.Write((byte)FargowiltasSouls.PacketID.SpawnFishronEX);
        //                                netMessage.Write((byte)Player.whoAmI);
        //                                netMessage.Write((int)Main.projectile[i].Center.X);
        //                                netMessage.Write((int)Main.projectile[i].Center.Y + 100);
        //                                netMessage.Send();
        //                            }
        //                            else if (Main.netMode == NetmodeID.Server)
        //                            {
        //                                ChatHelper.BroadcastChatMessage(Terraria.Localization.NetworkText.FromLiteral("???????"), Color.White);
        //                            }
        //                        }
        //                    }
        //                }
        //                if (spawned)
        //                {
        //                    bait.stack--;
        //                    if (bait.stack <= 0)
        //                        bait.SetDefaults(0);
        //                }
        //            }
        //        }

        public override void PostNurseHeal(NPC nurse, int health, bool removeDebuffs, int price)
        {
            if (GuttedHeart)
                GuttedHeartNurseHeal();
        }

        public override bool CanConsumeAmmo(Item weapon, Item ammo)
        {
            if (weapon.CountsAsClass(DamageClass.Ranged))
            {
                if (RangedEssence && Main.rand.NextBool(10))
                    return false;
                if (RangedSoul && Main.rand.NextBool(5))
                    return false;
            }
            if (GaiaSet && Main.rand.NextBool(10))
                return false;
            return true;
        }

        public int frameCounter = 0;
        public int frameSnow = 1;
        public int frameMutantAura = 0;
        //public int frameMutantLightning = 0;

        public override void HideDrawLayers(PlayerDrawSet drawInfo)
        {
            if (BetsyDashing || ShellHide || GoldShell)
            {
                foreach (var layer in PlayerDrawLayerLoader.Layers)
                {
                    //if (layer.Mod == null) //Only hide vanilla layers, optional. Does not hide modded layers added as Between or Multiple, but will hide layers marked as Before/After regardless
                    //{
                    layer.Hide();
                    //}
                }
            }

            //if (SquirrelMount)
            //{
            //    foreach (PlayerDrawLayer layer in PlayerDrawLayerLoader.Layers)
            //    {
            //        layer.


            //        if (layer != PlayerLayer.MountBack && PlayerLayer != PlayerLayer.MountFront && PlayerLayer != PlayerLayer.MiscEffectsFront && PlayerLayer != PlayerLayer.MiscEffectsBack)
            //        {
            //            PlayerLayer.visible = false;
            //        }
            //    }
            //}

        }


        //public override void HideDrawLayers(PlayerDrawSet drawInfo)
        //{
        //    //base.HideDrawLayers(drawInfo);

        //    if (BetsyDashing || ShellHide || GoldShell)
        //    {

        //        //drawInfo.headOnlyRender = true;
        //        drawInfo.DrawDataCache.Clear();
        //        //drawInfo.drawPlayer.invis = true;
        //    }
        //}

        //public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        //{
        //    if (BetsyDashing || ShellHide || GoldShell)
        //    {
        //        drawInfo.DrawDataCache.Clear();
        //    }
        //}

        public override void ModifyDrawLayerOrdering(IDictionary<PlayerDrawLayer, PlayerDrawLayer.Position> positions)
        {

            //if (BetsyDashing || ShellHide || GoldShell) //dont draw Player 
            //{
            //    foreach (KeyValuePair<PlayerDrawLayer, PlayerDrawLayer.Position> entry in positions)
            //    {
            //        positions.Remove(entry.Key);
            //    }
            //}



            //            if (SquirrelMount)
            //            {
            //                foreach (PlayerLayer PlayerLayer in layers)
            //                {
            //                    if (PlayerLayer != PlayerLayer.MountBack && PlayerLayer != PlayerLayer.MountFront && PlayerLayer != PlayerLayer.MiscEffectsFront && PlayerLayer != PlayerLayer.MiscEffectsBack)
            //                    {
            //                        PlayerLayer.visible = false;
            //                    }
            //                }
            //            }
        }

        public int getHealMultiplier(int heal)
        {
            float bonus = 0f;

            if ((SquireEnchantActive || ValhallaEnchantActive) && Player.GetToggleValue("Valhalla", false))
            {
                if (Eternity)
                    bonus = 4f;
                else if (WillForce && ValhallaEnchantActive)
                    bonus = 1f / 2f;
                else if (ValhallaEnchantActive || (WillForce && SquireEnchantActive))
                    bonus = 1f / 3f;
                else if (SquireEnchantActive)
                    bonus = 1f / 4f;
            }

            heal = (int)(heal * (1f + bonus));

            return heal;
        }

        public override void GetHealLife(Item item, bool quickHeal, ref int healValue)
        {
            healValue = getHealMultiplier(healValue);
        }

        public void HealPlayer(int amount)
        {
            amount = getHealMultiplier(amount);

            Player.statLife += amount;
            if (Player.statLife > Player.statLifeMax2)
                Player.statLife = Player.statLifeMax2;
            Player.HealEffect(amount);
        }

        public override void ModifyScreenPosition()
        {
            if (Screenshake > 0)
                Main.screenPosition += Main.rand.NextVector2Circular(7, 7);
        }

        public override void clientClone(ModPlayer clientClone)
        {
            FargoSoulsPlayer modPlayer = clientClone as FargoSoulsPlayer;
            modPlayer.Toggler = Toggler;
        }

        public void SyncToggle(string key)
        {
            if (!TogglesToSync.ContainsKey(key))
                TogglesToSync.Add(key, Player.GetToggle(key).ToggleBool);
        }

        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            foreach (KeyValuePair<string, bool> toggle in TogglesToSync)
            {
                ModPacket packet = Mod.GetPacket();

                packet.Write((byte)FargowiltasSouls.PacketID.SyncOneToggle);
                packet.Write((byte)Player.whoAmI);
                packet.Write(toggle.Key);
                packet.Write(toggle.Value);

                packet.Send(toWho, fromWho);
            }

            TogglesToSync.Clear();
        }

        public override void SendClientChanges(ModPlayer clientPlayer)
        {
            FargoSoulsPlayer modPlayer = clientPlayer as FargoSoulsPlayer;
            if (modPlayer.Toggler.Toggles != Toggler.Toggles)
            {
                ModPacket packet = Mod.GetPacket();
                packet.Write((byte)FargowiltasSouls.PacketID.SyncTogglesOnJoin);
                packet.Write((byte)Player.whoAmI);
                packet.Write((byte)Toggler.Toggles.Count);

                for (int i = 0; i < Toggler.Toggles.Count; i++)
                {
                    packet.Write(Toggler.Toggles.Values.ElementAt(i).ToggleBool);
                }

                packet.Send();
            }
        }

        public void AddBuffNoStack(int buff, int duration)
        {
            if (!Player.HasBuff(buff) && ReallyAwfulDebuffCooldown <= 0)
            {
                Player.AddBuff(buff, duration);
                int d = Player.FindBuffIndex(buff);
                if (d != -1) //if debuff successfully applied
                    ReallyAwfulDebuffCooldown = Player.buffTime[d] + 240;
            }
        }

        public void TryAdditionalAttacks(int damage, DamageClass damageType)
        {
            if (Player.whoAmI != Main.myPlayer)
                return;

            if (CactusEnchantActive)
            {
                CactusEnchant.CactusSelfProc(this);
            }

            if (BorealEnchantItem != null && Player.GetToggleValue("Boreal") && BorealCD <= 0)
            {
                BorealCD = 60;

                if (WoodForce)
                {
                    BorealCD = 30;
                }

                BorealWoodEnchant.BorealSnowballs(this, damage);
            }

            if (AdditionalAttacks && AdditionalAttacksTimer <= 0)
            {
                AdditionalAttacksTimer = 60;

                if (CelestialRuneItem != null && Player.GetToggleValue("MasoCelest"))
                {
                    CelestialRuneSupportAttack(damage, damageType);
                }
            }
        }

        public Rectangle GetPrecisionHurtbox()
        {
            Rectangle hurtbox = Player.Hitbox;
            hurtbox.X += hurtbox.Width / 2;
            hurtbox.Y += hurtbox.Height / 2;
            hurtbox.Width = Math.Min(hurtbox.Width, hurtbox.Height);
            hurtbox.Height = Math.Min(hurtbox.Width, hurtbox.Height);
            hurtbox.X -= hurtbox.Width / 2;
            hurtbox.Y -= hurtbox.Height / 2;
            return hurtbox;
        }

        
    }
}