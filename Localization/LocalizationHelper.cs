using FargowiltasSouls.Content.Items;
using FargowiltasSouls.Content.Items.Accessories.Enchantments;
using FargowiltasSouls.Content.Items.Accessories.Forces;
using FargowiltasSouls.Content.Items.Accessories.Masomode;
using FargowiltasSouls.Content.Items.Accessories.Souls;
using FargowiltasSouls.Content.Items.Armor;
using FargowiltasSouls.Content.Items.Consumables;
using FargowiltasSouls.Content.Items.Materials;
using FargowiltasSouls.Content.Items.Placables;
using FargowiltasSouls.Content.Items.Summons;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using FargowiltasSouls.Content.Patreon.DemonKing;
using FargowiltasSouls.Content.Patreon.LaBonez;
using FargowiltasSouls.Content.Patreon.DevAesthetic;
using FargowiltasSouls.Content.Patreon.GreatestKraken;
using FargowiltasSouls.Content.Patreon.Purified;
using FargowiltasSouls.Content.Patreon.Volknet;
using FargowiltasSouls.Content.Patreon.Shucks;
using FargowiltasSouls.Content.Patreon.Sam;
using FargowiltasSouls.Content.Patreon.Daawnz;
using FargowiltasSouls.Content.Patreon.Catsounds;
using FargowiltasSouls.Content.Patreon.ParadoxWolf;
using FargowiltasSouls.Content.Patreon.Sasha;
using FargowiltasSouls.Content.Patreon.ManliestDove;
using FargowiltasSouls.Content.Patreon.Gittle;
using Terraria;

namespace FargowiltasSouls
{
    public partial class FargowiltasSouls
    {
        /// <summary>
        /// Input item as string (item class name) for FargoSouls items, and as int for vanilla items.
        /// </summary>
        public void AddToggle(string toggle, object item, string color = "ffffff") 
        {
            if (item.GetType() == typeof(int))
            {
                Language.GetOrRegister(toggle, () => $"[i:{item.ToString()}] [c/{color}:{{$Mods.{Name}.Toggler.{toggle}}}]");
            }
            else if (item.GetType() == typeof(string))
            {
                Language.GetOrRegister(toggle, () => $"[i:FargowiltasSouls/{item}] [c/{color}:{{$Mods.{Name}.Toggler.{toggle}}}]");
            }

        }
        private void AddLocalizations()
        {
            #region helpers

            void Add(string key, string message)
            {
                Language.GetOrRegister(key, () => message);
            }

            void AddBossSpawnInfo(string bossName, string spawnInfo)
            {
                Add($"BossChecklist.{bossName}SpawnInfo", spawnInfo);
            }
            

            #endregion helpers

            Add("Message.MasochistExtraTooltip", $"[i:{"MutantsPact"}] {{$Mods.{Name}.Message.MasochistPreTooltip}}");


            #region boss spawn info

            if (FargoSoulsUtil.IsChinese())
            {
                AddBossSpawnInfo("DeviBoss", $"使用[i:{"DevisCurse"}]召唤");
                AddBossSpawnInfo("AbomBoss", $"使用[i:{"AbomsCurse"}]召唤");
                AddBossSpawnInfo("MutantBoss", $"在突变体和憎恶存活时将[i:{"AbominationnVoodooDoll"}]投入岩浆池中。");

                AddBossSpawnInfo("TimberChampion", $"白天时在地表使用[i:{"SigilOfChampions"}]召唤。");
                AddBossSpawnInfo("TerraChampion", $"在地下使用[i:{"SigilOfChampions"}]召唤。");
                AddBossSpawnInfo("EarthChampion", $"在地狱使用[i:{"SigilOfChampions"}]召唤。");
                AddBossSpawnInfo("NatureChampion", $"在地下雪原使用[i:{"SigilOfChampions"}]召唤。");
                AddBossSpawnInfo("LifeChampion", $"白天时在神圣之地使用[i:{"SigilOfChampions"}]召唤。");
                AddBossSpawnInfo("ShadowChampion", $"夜晚时在腐化之地或猩红之地使用[i:{"SigilOfChampions"}]召唤。");
                AddBossSpawnInfo("SpiritChampion", $"在地下沙漠使用[i:{"SigilOfChampions"}]召唤。");
                AddBossSpawnInfo("WillChampion", $"在海洋使用[i:{"SigilOfChampions"}]召唤。");
                AddBossSpawnInfo("CosmosChampion", $"在太空使用[i:{"SigilOfChampions"}]召唤。");

                AddBossSpawnInfo("TrojanSquirrel", $"使用[i:{"SquirrelCoatofArms"}]召唤");
                AddBossSpawnInfo("LifeChallenger", $"Spawn by using [i:{"FragilePixieLamp"}] in the Hallow at day.");
                //JAVYZ TODO: Cursed Coffin and Banished Baron
                //AddBossSpawnInfo("CursedCoffin", $"Spawn by using [i:{"CoffinSummon"}] in the Underground Desert.");
                //AddBossSpawnInfo("BanishedBaron", $"Spawn by using [i:{"BaronSummon"}] underwater at the ocean.");
            }
            else if (FargoSoulsUtil.IsPortuguese())
            {
                AddBossSpawnInfo("DeviBoss", $"Invoque usando [i:{"DevisCurse"}]");
                AddBossSpawnInfo("AbomBoss", $"Invoque usando [i:{"AbomsCurse"}]");
                AddBossSpawnInfo("MutantBoss", $"Arremesse [i:{"AbominationnVoodooDoll"}] em uma poça de lava enquanto Abominationn está vivo na presença de Mutant.");

                AddBossSpawnInfo("TimberChampion", $"Invoque usando [i:{"SigilOfChampions"}] na superfície durante o dia.");
                AddBossSpawnInfo("TerraChampion", $"Invoque usando [i:{"SigilOfChampions"}] no subterrâneo.");
                AddBossSpawnInfo("EarthChampion", $"Invoque usando [i:{"SigilOfChampions"}] no submundo.");
                AddBossSpawnInfo("NatureChampion", $"Invoque usando [i:{"SigilOfChampions"}] na neve subterrânea.");
                AddBossSpawnInfo("LifeChampion", $"Invoque usando [i:{"SigilOfChampions"}] no Sagrado de dia.");
                AddBossSpawnInfo("ShadowChampion", $"Invoque usando [i:{"SigilOfChampions"}] na Corrupção ou Carmim de noite.");
                AddBossSpawnInfo("SpiritChampion", $"Invoque usando [i:{"SigilOfChampions"}] no deserto subterrâneo.");
                AddBossSpawnInfo("WillChampion", $"Invoque usando [i:{"SigilOfChampions"}] no oceano.");
                AddBossSpawnInfo("CosmosChampion", $"Invoque usando [i:{"SigilOfChampions"}] no espaço.");

                AddBossSpawnInfo("TrojanSquirrel", $"Invoque usando [i:{"SquirrelCoatofArms"}]");
            }
            else {
                AddBossSpawnInfo("DeviBoss", $"Spawn by using [i:{"DevisCurse"}]");
                AddBossSpawnInfo("AbomBoss", $"Spawn by using [i:{"AbomsCurse"}]");
                AddBossSpawnInfo("MutantBoss", $"Throw [i:{"AbominationnVoodooDoll"}] into a pool of lava while Abominationn is alive in Mutant's presence.");

                AddBossSpawnInfo("TimberChampion", $"Spawn by using [i:{"SigilOfChampions"}] on the surface during day.");
                AddBossSpawnInfo("TerraChampion", $"Spawn by using [i:{"SigilOfChampions"}] underground.");
                AddBossSpawnInfo("EarthChampion", $"Spawn by using [i:{"SigilOfChampions"}] in the underworld.");
                AddBossSpawnInfo("NatureChampion", $"Spawn by using [i:{"SigilOfChampions"}] in underground snow.");
                AddBossSpawnInfo("LifeChampion", $"Spawn by using [i:{"SigilOfChampions"}] in the Hallow at day.");
                AddBossSpawnInfo("ShadowChampion", $"Spawn by using [i:{"SigilOfChampions"}] in the Corruption or Crimson at night.");
                AddBossSpawnInfo("SpiritChampion", $"Spawn by using [i:{"SigilOfChampions"}] in the underground desert.");
                AddBossSpawnInfo("WillChampion", $"Spawn by using [i:{"SigilOfChampions"}] at the ocean.");
                AddBossSpawnInfo("CosmosChampion", $"Spawn by using [i:{"SigilOfChampions"}] in space.");

                AddBossSpawnInfo("TrojanSquirrel", $"Spawn by using [i:{"SquirrelCoatofArms"}]");
                AddBossSpawnInfo("LifeChallenger", $"Spawn by using [i:{"FragilePixieLamp"}] in the Hallow at day.");
                //JAVYZ TODO: Cursed Coffin and Banished Baron
                //AddBossSpawnInfo("CursedCoffin", $"Spawn by using [i:{"CoffinSummon"}] in the Underground Desert.");
                //AddBossSpawnInfo("BanishedBaron", $"Spawn by using [i:{"BaronSummon"}] underwater at the ocean.");
            }

            #endregion boss spawn info


            #region Toggles

            //AddToggle("PresetHeader", "Masochist");

            #region enchants

            AddToggle("WoodHeader", "TimberForce");
            AddToggle("BorealConfig", "BorealWoodEnchant", "8B7464");
            AddToggle("MahoganyConfig", "RichMahoganyEnchant", "b56c64");
            AddToggle("EbonConfig", "EbonwoodEnchant", "645a8d");
            AddToggle("ShadeConfig", "ShadewoodEnchant", "586876");
            AddToggle("ShadeOnHitConfig", "ShadewoodEnchant", "586876");
            AddToggle("PalmConfig", "PalmWoodEnchant", "b78d56");
            AddToggle("PearlConfig", "PearlwoodEnchant", "ad9a5f");

            AddToggle("EarthHeader", "EarthForce");
            AddToggle("AdamantiteConfig", "AdamantiteEnchant", "dd557d");
            AddToggle("CobaltConfig", "CobaltEnchant", "3da4c4");
            AddToggle("AncientCobaltConfig", "AncientCobaltEnchant", "354c74");
            AddToggle("MythrilConfig", "MythrilEnchant", "9dd290");
            AddToggle("OrichalcumConfig", "OrichalcumEnchant", "eb3291");
            AddToggle("PalladiumConfig", "PalladiumEnchant", "f5ac28");
            AddToggle("PalladiumOrbConfig", "PalladiumEnchant", "f5ac28");
            AddToggle("TitaniumConfig", "TitaniumEnchant", "828c88");

            AddToggle("TerraHeader", "TerraForce");
            AddToggle("CopperConfig", "CopperEnchant", "d56617");
            AddToggle("IronMConfig", "IronEnchant", "988e83");
            AddToggle("SilverSConfig", "SilverEnchant", "b4b4cc");
            AddToggle("TinConfig", "TinEnchant", "a28b4e");
            AddToggle("TungstenConfig", "TungstenEnchant", "b0d2b2");
            AddToggle("TungstenProjConfig", "TungstenEnchant", "b0d2b2");
            AddToggle("ObsidianConfig", "ObsidianEnchant", "453e73");

            AddToggle("WillHeader", "WillForce");
            AddToggle("GladiatorConfig", "GladiatorEnchant", "9c924e");
            AddToggle("GoldConfig", "GoldEnchant", "e7b21c");
            AddToggle("GoldToPiggyConfig", "GoldEnchant", "e7b21c");
            AddToggle("HuntressConfig", "HuntressEnchant", "7ac04c");
            AddToggle("RedRidingRainConfig", "RedRidingEnchant", "c01b3c");
            AddToggle("ValhallaConfig", "ValhallaKnightEnchant", "93651e");
            AddToggle("SquirePanicConfig", "SquireEnchant", "948f8c");

            AddToggle("LifeHeader", "LifeForce");
            AddToggle("BeeConfig", "BeeEnchant", "FEF625");
            AddToggle("BeetleConfig", "BeetleEnchant", "6D5C85");
            AddToggle("CactusConfig", "CactusEnchant", "799e1d");
            AddToggle("PumpkinConfig", "PumpkinEnchant", "e3651c");
            AddToggle("SpiderConfig", "SpiderEnchant", "6d4e45");
            AddToggle("TurtleConfig", "TurtleEnchant", "f89c5c");

            AddToggle("NatureHeader", "NatureForce");
            AddToggle("ChlorophyteConfig", "ChlorophyteEnchant", "248900");
            AddToggle("CrimsonConfig", "CrimsonEnchant", "C8364B");
            AddToggle("RainConfig", "RainEnchant", "ffec00");
            AddToggle("RainInnerTubeConfig", "RainEnchant", "ffec00");
            AddToggle("FrostConfig", "FrostEnchant", "7abdb9");
            AddToggle("JungleConfig", "JungleEnchant", "71971f");
            AddToggle("JungleDashConfig", "JungleEnchant", "71971f");
            AddToggle("MoltenConfig", "MoltenEnchant", "c12b2b");
            AddToggle("MoltenEConfig", "MoltenEnchant", "c12b2b");
            AddToggle("ShroomiteConfig", "ShroomiteEnchant", "008cf4");
            AddToggle("ShroomiteShroomConfig", "ShroomiteEnchant", "008cf4");

            AddToggle("ShadowHeader", "ShadowForce");
            AddToggle("DarkArtConfig", "DarkArtistEnchant", "9b5cb0");
            AddToggle("ApprenticeConfig", "ApprenticeEnchant", "5d86a6");
            AddToggle("NecroConfig", "NecroEnchant", "565643");
            AddToggle("NecroGloveConfig", "NecroEnchant", "565643");
            AddToggle("ShadowConfig", "ShadowEnchant", "42356f");
            AddToggle("AncientShadowConfig", "AncientShadowEnchant", "42356f");
            AddToggle("MonkConfig", "MonkEnchant", "920520");
            AddToggle("ShinobiDashConfig", "ShinobiEnchant", "935b18");
            AddToggle("ShinobiConfig", "ShinobiEnchant", "935b18");
            AddToggle("SpookyConfig", "SpookyEnchant", "644e74");
            AddToggle("NinjaSpeedConfig", "NinjaEnchant", "565643");
            AddToggle("CrystalDashConfig", "CrystalAssassinEnchant", "249dcf");

            AddToggle("SpiritHeader", "SpiritForce");
            AddToggle("FossilConfig", "FossilEnchant", "8c5c3b");
            AddToggle("ForbiddenConfig", "ForbiddenEnchant", "e7b21c");
            AddToggle("HallowDodgeConfig", "HallowEnchant", "968564");
            AddToggle("HallowedConfig", "AncientHallowEnchant", "968564");
            AddToggle("HallowSConfig", "AncientHallowEnchant", "968564");
            AddToggle("SpectreConfig", "SpectreEnchant", "accdfc");
            AddToggle("TikiConfig", "TikiEnchant", "56A52B");

            AddToggle("CosmoHeader", "CosmoForce");
            AddToggle("MeteorConfig", "MeteorEnchant", "5f4752");
            AddToggle("NebulaConfig", "NebulaEnchant", "fe7ee5");
            AddToggle("SolarConfig", "SolarEnchant", "fe9e23");
            AddToggle("SolarFlareConfig", "SolarEnchant", "fe9e23");
            AddToggle("StardustConfig", "StardustEnchant", "00aeee");
            AddToggle("VortexSConfig", "VortexEnchant", "00f2aa");
            AddToggle("VortexVConfig", "VortexEnchant", "00f2aa");

            #endregion enchants

            #region masomode toggles
            //Masomode Header
            AddToggle("MasoHeader", "MutantStatue");
            AddToggle("MasoBossRecolors", "Masochist");
            AddToggle("MasoCanPlay", "Masochist", "ff0000");

            AddToggle("MasoHeader2", "DeviatingEnergy");
            AddToggle("DeerSinewDashConfig", "DeerSinew");
            AddToggle("MasoAeolusConfig", "AeolusBoots");
            AddToggle("MasoAeolusFlowerConfig", "AeolusBoots");
            AddToggle("MasoIconConfig", "SinisterIcon");
            AddToggle("MasoIconDropsConfig", "SinisterIcon");
            AddToggle("MasoGrazeConfig", "SparklingAdoration");
            AddToggle("MasoGrazeRingConfig", "SparklingAdoration");
            AddToggle("MasoDevianttHeartsConfig", "SparklingAdoration");
            AddToggle("PrecisionSealHurtboxConfig", "PrecisionSeal");

            //supreme death fairy header
            AddToggle("SupremeFairyHeader", "SupremeDeathbringerFairy");
            AddToggle("MasoSlimeConfig", "SlimyShield");
            AddToggle("SlimeFallingConfig", "SlimyShield");
            AddToggle("MasoEyeConfig", "AgitatingLens");
            AddToggle("MasoEyeInstallConfig", "AgitatingLens");
            AddToggle("MasoSkeleConfig", "NecromanticBrew");
            AddToggle("MasoSkeleSpinConfig", "NecromanticBrew");

            //bionomic
            AddToggle("BionomicHeader", "BionomicCluster");
            AddToggle("MasoConcoctionConfig", "TimsConcoction");
            AddToggle("MasoCarrotConfig", "OrdinaryCarrot");
            AddToggle("MasoRainbowConfig", "ConcentratedRainbowMatter");
            AddToggle("MasoHealingPotionConfig", "ConcentratedRainbowMatter");
            AddToggle("MasoNymphConfig", "NymphsPerfume");
            AddToggle("MasoSqueakConfig", "SqueakyToy");
            AddToggle("MasoPouchConfig", "WretchedPouch");
            AddToggle("MasoClippedConfig", "WyvernFeather");
			AddToggle("MasoGrav2Config", "WyvernFeather");
            AddToggle("TribalCharmConfig", "TribalCharm");
            AddToggle("TribalCharmClickBonusConfig", "TribalCharm");
            //AddToggle("WalletHeader", "SecurityWallet");

            //dubious
            AddToggle("DubiousHeader", "DubiousCircuitry");
            AddToggle("FusedLensInstallConfig", "FusedLens");
            AddToggle("MasoLightningConfig", "GroundStick");
            AddToggle("MasoProbeConfig", "GroundStick");

            //pure heart
            AddToggle("PureHeartHeader", "PureHeart");
            AddToggle("MasoEaterConfig", "DarkenedHeart");
            AddToggle("MasoBrainConfig", "GuttedHeart");

            //lump of flesh
            AddToggle("LumpofFleshHeader", "LumpOfFlesh");
            AddToggle("MasoPungentCursorConfig", "PungentEyeball");
            AddToggle("MasoPugentConfig", "LumpOfFlesh");
            AddToggle("DreadShellParryConfig", "DreadShell");
            AddToggle("DeerclawpsConfig", "Deerclawps");

            //chalice
            AddToggle("ChaliceHeader", "ChaliceoftheMoon");
            AddToggle("MasoCultistConfig", "ChaliceoftheMoon");
            AddToggle("MasoPlantConfig", "MagicalBulb");
            AddToggle("MasoGolemConfig", "LihzahrdTreasureBox");
            AddToggle("MasoBoulderConfig", "LihzahrdTreasureBox");
            AddToggle("MasoCelestConfig", "CelestialRune");
            AddToggle("MasoVisionConfig", "CelestialRune");

            //heart of the masochist
            AddToggle("HeartHeader", "HeartoftheMasochist");
            AddToggle("MasoPumpConfig", "PumpkingsCape");
            AddToggle("IceQueensCrownConfig", "IceQueensCrown");
            AddToggle("MasoUfoConfig", "SaucerControlConsole");
            AddToggle("MasoGravConfig", "GalacticGlobe");
            AddToggle("MasoTrueEyeConfig", "GalacticGlobe");

            //cyclonic fin
            AddToggle("CyclonicHeader", "AbominableWand");
            AddToggle("MasoFishronConfig", "AbominableWand");

            //mutant armor
            AddToggle("MutantArmorHeader", "HeartoftheMasochist");
            AddToggle("MasoAbomConfig", "MutantMask");
            AddToggle("MasoRingConfig", "MutantMask");
            AddToggle("MasoReviveDeathrayConfig", "MutantMask");

            #endregion masomode toggles

            #region soul toggles

            AddToggle("UniverseHeader", "UniverseSoul");
            AddToggle("MeleeConfig", "BerserkerSoul");
            AddToggle("MagmaStoneConfig", "BerserkerSoul");
            AddToggle("YoyoBagConfig", "BerserkerSoul");
            AddToggle("MoonCharmConfig", "BerserkerSoul");
            AddToggle("NeptuneShellConfig", "BerserkerSoul");
            AddToggle("SniperConfig", "SnipersSoul");
            AddToggle("ManaFlowerConfig", "ArchWizardsSoul");
            AddToggle("UniverseConfig", "UniverseSoul");

            AddToggle("WorldShaperHeader", "WorldShaperSoul");
            AddToggle("MiningHuntConfig", "MinerEnchant");
            AddToggle("MiningDangerConfig", "MinerEnchant");
            AddToggle("MiningSpelunkConfig", "MinerEnchant");
            AddToggle("MiningShineConfig", "MinerEnchant");
            AddToggle("BuilderConfig", "WorldShaperSoul");

            AddToggle("ColossusHeader", "ColossusSoul");
            AddToggle("DefenseStarConfig", "ColossusSoul");
            AddToggle("DefenseBrainConfig", "ColossusSoul");
            AddToggle("DefenseBeeConfig", "ColossusSoul");
            AddToggle("DefensePanicConfig", "ColossusSoul");
            AddToggle("DefenseFleshKnuckleConfig", "ColossusSoul");
            AddToggle("DefensePaladinConfig", "ColossusSoul");
            AddToggle("DefenseFrozenConfig", "ColossusSoul");

            AddToggle("FlightMasteryHeader", "FlightMasterySoul");
            AddToggle("FlightMasteryInsigniaConfig", "FlightMasterySoul");
            AddToggle("FlightMasteryGravityConfig", "FlightMasterySoul");

            AddToggle("SupersonicHeader", "SupersonicSoul");
            AddToggle("RunSpeedConfig", "SupersonicSoul");
            AddToggle("MomentumConfig", "SupersonicSoul");
            AddToggle("SupersonicTabiConfig", "SupersonicSoul");
            AddToggle("SupersonicClimbingConfig", "SupersonicSoul");
            AddToggle("SupersonicConfig", "SupersonicSoul");
            AddToggle("SupersonicJumpsConfig", "SupersonicSoul");
            AddToggle("SupersonicRocketBootsConfig", "SupersonicSoul");
            AddToggle("SupersonicCarpetConfig", "SupersonicSoul");
            AddToggle("SupersonicFlowerConfig", "SupersonicSoul");
            AddToggle("CthulhuShieldConfig", "SupersonicSoul");
            AddToggle("BlackBeltConfig", "SupersonicSoul");

            AddToggle("TrawlerHeader", "TrawlerSoul");
            AddToggle("TrawlerSporeConfig", "TrawlerSoul");
            AddToggle("TrawlerConfig", "TrawlerSoul");
            AddToggle("TrawlerJumpConfig", "TrawlerSoul");
            AddToggle("TrawlerGelConfig", "TrawlerSoul");

            AddToggle("EternityHeader", "EternitySoul");
            AddToggle("EternityConfig", "EternitySoul");

            #endregion soul toggles

            #region pet toggles
            AddToggle("PetHeader", ItemID.ZephyrFish);
            AddToggle("PetDinoConfig", ItemID.AmberMosquito);
            AddToggle("PetEaterConfig", ItemID.EatersBone);
            AddToggle("PetFaceMonsterConfig", ItemID.BoneRattle);
            AddToggle("PetGrinchConfig", ItemID.BabyGrinchMischiefWhistle);
            AddToggle("PetHornetConfig", ItemID.Nectar);
            AddToggle("PetImpConfig", ItemID.HellCake);
            AddToggle("PetPenguinConfig", ItemID.Fish);
            AddToggle("PetPandaConfig", ItemID.BambooLeaf);
            AddToggle("PetDGConfig", ItemID.BoneKey);
            AddToggle("PetSnowmanConfig", ItemID.ToySled);
            AddToggle("PetShroomConfig", ItemID.StrangeGlowingMushroom);
            AddToggle("PetWerewolfConfig", ItemID.FullMoonSqueakyToy);
            AddToggle("PetBernieConfig", ItemID.BerniePetItem);
            AddToggle("PetBlackCatConfig", ItemID.UnluckyYarn);
            //AddToggle("PetBlueChickenConfig", ItemID.egg);
            //AddToggle("PetCavelingConfig", ItemID.glowt);
            AddToggle("PetChesterConfig", ItemID.ChesterPetItem);
            AddToggle("PetCompanionCubeConfig", ItemID.CompanionCube);
            AddToggle("PetCursedSaplingConfig", ItemID.CursedSapling);
            //public string PetDirt;
            AddToggle("PetKittenConfig", ItemID.BallOfFuseWire);
            AddToggle("PetEsteeConfig", ItemID.CelestialWand);
            AddToggle("PetEyeSpringConfig", ItemID.EyeSpring);
            AddToggle("PetFoxConfig", ItemID.ExoticEasternChewToy);
            AddToggle("PetButterflyConfig", ItemID.BedazzledNectar);
            AddToggle("PetGlommerConfig", ItemID.GlommerPetItem);
            AddToggle("PetDragonConfig", ItemID.DD2PetDragon);
            //AddToggle("PetJunimoConfig", ItemID.stardr);
            AddToggle("PetHarpyConfig", ItemID.BirdieRattle);
            AddToggle("PetLizardConfig", ItemID.LizardEgg);
            AddToggle("PetMinitaurConfig", ItemID.TartarSauce);
            AddToggle("PetParrotConfig", ItemID.ParrotCracker);
            AddToggle("PetPigmanConfig", ItemID.PigPetItem);
            AddToggle("PetPlanteroConfig", ItemID.MudBud);
            AddToggle("PetGatoConfig", ItemID.DD2PetGato);
            AddToggle("PetPupConfig", ItemID.DogWhistle);
            AddToggle("PetSeedConfig", ItemID.Seedling);
            AddToggle("PetSpiderConfig", ItemID.SpiderEgg);
            AddToggle("PetMimicConfig", ItemID.OrnateShadowKey);
            AddToggle("PetSharkConfig", ItemID.SharkBait);
            //AddToggle("PetSpiffoConfig", ItemID.spiff);
            AddToggle("PetSquashConfig", ItemID.MagicalPumpkinSeed);
            AddToggle("PetGliderConfig", ItemID.EucaluptusSap);
            AddToggle("PetTikiConfig", ItemID.TikiTotem);
            AddToggle("PetTurtleConfig", ItemID.Seaweed);
            AddToggle("PetVoltConfig", ItemID.LightningCarrot);
            AddToggle("PetZephyrConfig", ItemID.ZephyrFish);

            AddToggle("PetOrbConfig", ItemID.ShadowOrb);
            AddToggle("PetHeartConfig", ItemID.CrimsonHeart);
            AddToggle("PetLanternConfig", ItemID.MagicLantern);
            AddToggle("PetNaviConfig", ItemID.FairyBell);
            AddToggle("PetFlickerConfig", ItemID.DD2OgrePetItem);
            AddToggle("PetWispConfig", ItemID.WispinaBottle);
            AddToggle("PetSuspEyeConfig", ItemID.SuspiciousLookingTentacle);

            AddToggle("PetKSConfig", ItemID.KingSlimePetItem);
            AddToggle("PetEoCConfig", ItemID.EyeOfCthulhuPetItem);
            AddToggle("PetEoWConfig", ItemID.EaterOfWorldsPetItem);
            AddToggle("PetBoCConfig", ItemID.BrainOfCthulhuPetItem);
            AddToggle("PetDeerConfig", ItemID.DeerclopsPetItem);
            AddToggle("PetQBConfig", ItemID.QueenBeePetItem);
            AddToggle("PetSkeleConfig", ItemID.SkeletronPetItem);
            AddToggle("PetQSConfig", ItemID.QueenSlimePetItem);
            AddToggle("PetDestroyerConfig", ItemID.DestroyerPetItem);
            AddToggle("PetTwinsConfig", ItemID.TwinsPetItem);
            AddToggle("PetSkelePrimeConfig", ItemID.SkeletronPrimePetItem);

            AddToggle("PetOgreConfig", ItemID.DD2OgrePetItem);
            AddToggle("PetPlanteraConfig", ItemID.PlanteraPetItem);
            AddToggle("PetPumpkingConfig", ItemID.PumpkingPetItem);
            AddToggle("PetEverscreamConfig", ItemID.EverscreamPetItem);
            AddToggle("PetIceQueenConfig", ItemID.IceQueenPetItem);
            AddToggle("PetDukeConfig", ItemID.DukeFishronPetItem);
            AddToggle("PetGolemConfig", ItemID.GolemPetItem);
            AddToggle("PetEoLConfig", ItemID.FairyQueenPetItem);
            AddToggle("PetBetsyConfig", ItemID.DD2BetsyPetItem);
            AddToggle("PetMartianConfig", ItemID.MartianPetItem);
            AddToggle("PetLCConfig", ItemID.LunaticCultistPetItem);
            AddToggle("PetMLConfig", ItemID.MoonLordPetItem);


        #endregion pet toggles

        #region patreon toggles
        AddToggle("PatreonHeader", "RoombaPet");
            AddToggle("PatreonRoomba", "RoombaPet");
            AddToggle("PatreonOrb", "ComputationOrb");
            AddToggle("PatreonFishingRod", "MissDrakovisFishingPole");
            AddToggle("PatreonDoor", "SquidwardDoor");
            AddToggle("PatreonWolf", "ParadoxWolfSoul");
            AddToggle("PatreonDove", "FigBranch");
            AddToggle("PatreonKingSlime", "MedallionoftheFallenKing");
            AddToggle("PatreonFishron", "StaffOfUnleashedOcean");
            AddToggle("PatreonPlant", "PiranhaPlantVoodooDoll");
            AddToggle("PatreonDevious", "DeviousAestheticus");
            AddToggle("PatreonVortex", "VortexMagnetRitual");
            AddToggle("PatreonPrime", "PrimeStaff");
            AddToggle("PatreonCrimetroid", "CrimetroidEgg");
            AddToggle("PatreonNanoCore", "NanoCore");
            #endregion patreon toggles

            #endregion Toggles


        }
    }
}
