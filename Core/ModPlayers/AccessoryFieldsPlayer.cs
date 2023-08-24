using Terraria;

namespace FargowiltasSouls.Core.ModPlayers
{
    public partial class FargoSoulsPlayer
    {
        public Item QueenStingerItem;
        public bool EridanusSet;
        public bool EridanusEmpower;
        public int EridanusTimer;
        public bool GaiaSet;
        public bool GaiaOffense;
        public bool StyxSet;
        public int StyxMeter;
        public int StyxTimer;
        public bool NekomiSet;
        public int NekomiMeter;
        public int NekomiTimer;
        public int NekomiHitCD;

        //        //minions
        public bool BrainMinion;
        public bool EaterMinion;
        public bool BigBrainMinion;
        public bool DukeFishron;

        //mount
        public bool SquirrelMount;

        //pet
        public bool SeekerOfAncientTreasures;
        public bool AccursedSarcophagus;
        public bool BabySilhouette;
        public bool BabyLifelight;
        public bool BiteSizeBaron;
        public bool Nibble;
        public bool ChibiDevi;
        public bool MutantSpawn;
        public bool BabyAbom;

        public bool PetsActive;

        #region enchantments
        //force of timber
        public Item BorealEnchantItem;
        public int BorealCD;
        public Item MahoganyEnchantItem;
        public bool MahoganyCanUseDR;
        public Item PalmEnchantItem;
        public Item PearlwoodEnchantItem;
        public int PearlwoodCD;
        public Item EbonwoodEnchantItem;
        public Item ShadewoodEnchantItem;
        public int ShadewoodCD;
        public Item WoodEnchantItem;
        public bool WoodEnchantDiscount;
        //force of terra
        public Item CopperEnchantItem;
        public int CopperProcCD;
        public Item IronEnchantItem;
        public bool GuardRaised;
        public int ParryDebuffImmuneTime;
        public Item LeadEnchantItem;
        public Item ObsidianEnchantItem;
        public int ObsidianCD;
        public bool LavaWet;
        public Item SilverEnchantItem;
        public Item TinEnchantItem;
        public float TinCritMax;
        public float TinCrit = 5;
        public int TinProcCD;
        public bool TinCritBuffered;
        public Item TungstenEnchantItem;
        public int TungstenCD;
        //force of earth
        public Item AdamantiteEnchantItem;
        public bool AdamantiteCanSplit;
        public Item CobaltEnchantItem;
        public bool CanCobaltJump;
        public bool JustCobaltJumped;
        public int CobaltCooldownTimer;
        public int CobaltImmuneTimer;
        public Item MythrilEnchantItem;
        public int MythrilTimer;
        public int MythrilMaxTime => EarthForce ? 300 : 180;
        public float MythrilMaxSpeedBonus => EarthForce ? 1.75f : 1.5f;
        public Item OriEnchantItem;
        public Item PalladEnchantItem;
        public int PalladCounter;
        public Item TitaniumEnchantItem;
        public bool TitaniumDRBuff;
        public bool TitaniumCD;
        //force of nature

        //force of life

        //force of spirit

        //force of shadow

        //force of will

        //force of cosmos
        public Item MeteorEnchantItem;
        public int MeteorTimer;
        public int MeteorCD = 60;
        public bool MeteorShower;
















        public bool AncientHallowEnchantActive;
        public bool AncientShadowEnchantActive;
        public bool ApprenticeEnchantActive;
        public int ApprenticeCD;
        public bool BeeEnchantActive;
        public int BeeCD;

        public bool CactusEnchantActive;
        public int CactusProcCD;
        public bool ChloroEnchantActive;
        public Item ChloroEnchantItem;

        public bool CrimsonEnchantActive;
        public int CrimsonRegenAmount;

        public bool DarkArtistEnchantActive;
        public bool DarkArtistSpawn;
        public int DarkArtistSpawnCD;
        public bool ForbiddenEnchantActive;
        public Item FossilEnchantItem;
        public bool FrostEnchantActive;
        public int IcicleCount;
        private int icicleCD;
        public bool GladiatorEnchantActive;
        public int GladiatorCD;
        public bool GoldEnchantActive;
        public bool GoldEnchMoveCoins;
        public bool GoldShell;
        private int goldHP;
        public bool HallowEnchantActive;
        public bool HuntressEnchantActive;
        public int HuntressStage;
        public int HuntressCD;

        public bool JungleEnchantActive;
        public int JungleCD;




        public bool MoltenEnchantActive;
        public bool MonkEnchantActive;
        public int MonkDashing;
        private int monkTimer;



        public bool NecroEnchantActive;
        public int NecroCD;
        public Item NinjaEnchantItem;
        public bool CrystalEnchantActive;
        public Projectile CrystalSmokeBombProj = null;
        public bool FirstStrike;
        public int SmokeBombCD;




        public int PumpkinSpawnCD;
        public bool RainEnchantActive;
        public Item RedRidingEnchantItem;
        public int RedRidingArrowCD;

        public bool ShadowEnchantActive;
        public bool ShinobiEnchantActive;
        public int dashCD;
        public bool ShroomEnchantActive;

        public bool PlatinumEnchantActive;
        public bool SnowEnchantActive;
        public bool SnowVisual;
        public bool SolarEnchantActive;
        public bool SpectreEnchantActive;
        public int SpectreCD;
        public bool SpiderEnchantActive;
        public bool SpookyEnchantActive;
        public bool SquireEnchantActive;
        public bool squireReduceIframes;
        public bool StardustEnchantActive;
        public bool FreezeTime;
        public int freezeLength;
        public const int TIMESTOP_DURATION = 540; //300
        public bool ChillSnowstorm;
        public int chillLength;
        public int CHILL_DURATION => FrostEnchantActive ? 60 * 20 : 60 * 15;
        public bool TikiEnchantActive;

        public bool TurtleEnchantActive;
        public int TurtleCounter;
        public int TurtleShellHP = 25;
        private int turtleRecoverCD = 240;
        public bool ShellHide;
        public bool ValhallaEnchantActive;
        public bool VortexEnchantActive;
        public bool VortexStealth;
        public bool WizardEnchantActive;

        public bool NebulaEnchantActive;
        public bool BeetleEnchantActive;
        public int BeetleEnchantDefenseTimer;

        public int CritterAttackTimer;

        public bool CosmoForce;
        public bool EarthForce;
        public bool LifeForce;
        public bool NatureForce;
        public bool SpiritForce;
        public bool ShadowForce;
        public bool TerraForce;
        public bool WillForce;
        public bool WoodForce;

        #endregion

        //        //soul effects
        public bool MagicSoul;
        public bool RangedSoul;
        public bool RangedEssence;
        public bool BuilderMode;
        public bool UniverseSoul;
        public bool UniverseCore;
        public bool FishSoul1;
        public bool FishSoul2;
        public bool TerrariaSoul;
        public bool VoidSoul;
        public int HealTimer;
        public int HurtTimer;
        public bool Eternity;
        public float TinEternityDamage;

        //maso items
        public Item SlimyShieldItem;
        public bool SlimyShieldFalling;
        public Item AgitatingLensItem;
        public int AgitatingLensCD;
        public Item DarkenedHeartItem;
        public int DarkenedHeartCD;
        public bool GuttedHeart;
        public int GuttedHeartCD = 60; //should prevent spawning despite disabled toggle when loading into world
        public Item NecromanticBrewItem;
        public float NecromanticBrewRotation;
        public Item DeerclawpsItem;
        public int IsDashingTimer;
        public bool DeerSinewNerf;
        public int DeerSinewFreezeCD;
        public bool PureHeart;
        public bool PungentEyeballMinion;
        public bool CrystalSkullMinion;
        public bool FusedLens;
        public bool FusedLensCanDebuff;
        public bool GroundStick;
        public bool Supercharged;
        public bool Probes;
        public bool MagicalBulb;
        public bool PlanterasChild;
        public bool SkullCharm;
        public bool PungentEyeball;
        public bool LumpOfFlesh;
        public Item PumpkingsCapeItem;
        public Item LihzahrdTreasureBoxItem;
        public int GroundPound;
        public Item BetsysHeartItem;
        public bool BetsyDashing;
        public int SpecialDashCD;
        public bool MutantAntibodies;
        public Item GravityGlobeEXItem;
        public Item CelestialRuneItem;
        public bool AdditionalAttacks;
        public int AdditionalAttacksTimer;
        public bool MoonChalice;
        public bool LunarCultist;
        public bool TrueEyes;
        public Item AbomWandItem;
        public int AbomWandCD;
        public bool MasochistSoul;
        public bool MasochistHeart;
        public bool MutantsPactSlot;
        public bool HasClickedWrench;
        public bool SandsofTime;
        public bool DragonFang;
        public bool StabilizedGravity;
        public bool SecurityWallet;
        public Item FrigidGemstoneItem;
        public int FrigidGemstoneCD;
        public Item WretchedPouchItem;
        public int WretchedPouchCD;
        public bool NymphsPerfume;
        public bool NymphsPerfumeRespawn;
        public int NymphsPerfumeRestoreLife;
        public int NymphsPerfumeCD = 30;
        public bool SqueakyAcc;
        public bool RainbowSlime;
        public bool SkeletronArms;
        public bool IceQueensCrown;
        public bool CirnoGraze;
        public bool MiniSaucer;
        public bool CanAmmoCycle;
        public bool TribalCharm;
        public bool TribalCharmEquipped;
        public bool TribalCharmClickBonus;
        public bool SupremeDeathbringerFairy;
        public bool GodEaterImbue;
        public Item MutantSetBonusItem;
        public bool AbomMinion;
        public bool PhantasmalRing;
        public bool MutantsDiscountCard;
        public bool MutantsCreditCard;
        public bool DeerSinew;
        public bool RabiesVaccine;
        public bool TwinsEX;
        public bool TimsConcoction;
        public bool ReceivedMasoGift;
        public bool DeviGraze;
        public bool Graze;
        public float GrazeRadius;
        public int DeviGrazeCounter;
        public int CirnoGrazeCounter;
        public double DeviGrazeBonus;
        public Item DevianttHeartItem;
        public int DevianttHeartsCD;
        public Item MutantEyeItem;
        public bool MutantEyeVisual;
        public int MutantEyeCD;
        public bool AbominableWandRevived;
        public bool AbomRebirth;
        public bool WasHurtBySomething;
        public bool PrecisionSeal;
        public bool PrecisionSealHurtbox;
        public bool PrecisionSealNoDashNoJump;
        public Item GelicWingsItem;
        public bool ConcentratedRainbowMatter;

        //debuffs
        public bool Hexed;
        public bool Unstable;
        private int unstableCD;
        public bool Fused;
        public bool Shadowflame;
        public bool Oiled;
        public bool DeathMarked;
        public bool Hypothermia;
        public bool noDodge;
        public bool noSupersonic;
        public bool NoMomentum;
        public bool Bloodthirsty;
        public bool Unlucky;
        public bool DisruptedFocus;
        public bool SinisterIcon;
        public bool SinisterIconDrops;

        public bool Smite;
        public bool Anticoagulation;
        public bool GodEater;               //defense removed, endurance removed, colossal DOT
        public bool FlamesoftheUniverse;    //activates various vanilla debuffs
        public bool MutantNibble;           //moon bite effect, feral bite effect, disables lifesteal
        public int StatLifePrevious = -1;   //used for mutantNibble
        public bool Asocial;                //disables minions, disables pets
        public bool WasAsocial;
        public bool HidePetToggle0 = true;
        public bool HidePetToggle1 = true;
        public bool Kneecapped;             //disables running :v
        public bool Defenseless;            //-30 defense, no damage reduction, cross necklace and knockback prevention effects disabled
        public bool Purified;               //purges all other buffs
        public bool Infested;               //weak DOT that grows exponentially stronger
        public int MaxInfestTime;
        public bool FirstInfection = true;
        public float InfestedDust;
        public bool Rotting;                //inflicts DOT and almost every stat reduced
        public bool SqueakyToy;             //all attacks do one damage and make squeaky noises
        public bool Atrophied;              //melee speed and damage reduced. maybe Player cannot fire melee projectiles?
        public bool Jammed;                 //ranged damage and speed reduced, all non-custom ammo set to baseline ammos
        public bool Slimed;
        public byte lightningRodTimer;
        public bool ReverseManaFlow;
        public bool CurseoftheMoon;
        public bool OceanicMaul;
        public int MaxLifeReduction;
        public int CurrentLifeReduction;
        public int LifeReductionUpdateTimer;
        public bool Midas;
        public bool MutantPresence;
        public bool MutantFang;
        public bool DevianttPresence;
        public bool Swarming;
        public bool LowGround;
        public bool Flipped;
        public bool Mash;
        public bool[] MashPressed = new bool[4];
        public int MashCounter;
        public int StealingCooldown;
        public bool LihzahrdCurse;
        //public bool LihzahrdBlessing;
        public bool Berserked;
        public bool CerebralMindbreak;
        public bool NanoInjection;
        public bool Stunned;
        public bool HaveCheckedAttackSpeed;



        public int ReallyAwfulDebuffCooldown;

        public bool BoxofGizmos;




        public Item DreadShellItem;
        public int DreadShellVulnerabilityTimer;
        public int shieldTimer;
        public int shieldCD;
        public bool wasHoldingShield;
        public int LightslingerHitShots;

        public int NoUsingItems;

        public bool HasDash;

        public int WeaponUseTimer;
    }
}
