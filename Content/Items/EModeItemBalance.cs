using FargowiltasSouls.Core.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static FargowiltasSouls.Content.Items.EmodeItemBalance;

namespace FargowiltasSouls.Content.Items
{
    public class EmodeItemBalance : EModePlayer
    {
        public enum BalanceTypes
        {
            None,
            CustomNerf,
            CustomBuff,
            CustomNeutral,
            Damage,
            AttackSpeed,
        };
        /// <summary>
        /// Applies an Eternity-exclusive balance change to chosen item type. <br />
        /// Non-custom BalanceTypes have their effects automatically applied. <br />
        /// Return BalanceTypes.CustomBuff or BalanceTypes.CustomNerf for manually applied effects. <br />
        /// Default balanceNumber is -1. <br />
        /// Default balanceTextKeys is null. <br />
        /// Default extra is empty. <br />
        /// Number and extra is used on the first tooltip line.
        /// </summary>
        public static BalanceTypes EmodeBalance(ref Item item, ref float balanceNumber, ref string[] balanceTextKeys, ref string extra)
        {
            switch (item.type)
            {
                case ItemID.RodofDiscord:
                case ItemID.RodOfHarmony:
                    balanceTextKeys = new string[] { "RodofDiscord" };
                    return BalanceTypes.CustomNerf;

                //case ItemID.ArcheryPotion:
                //case ItemID.MagicQuiver:
                //case ItemID.ShroomiteHelmet:
                //case ItemID.ShroomiteHeadgear:
                //case ItemID.ShroomiteMask:
                //    tooltips.Add(new TooltipLine(Mod, "masoNerf", "[c/ff0000:Eternity Mode:] Grants additive damage instead of multiplicative"));
                //    break;

                //case ItemID.CrystalBullet:
                //case ItemID.HolyArrow:
                    //ItemBalance(tooltips, EModeChange.Nerf, "Split");
                    //break;

                //case ItemID.ChlorophyteBullet:
                    //ItemBalance(tooltips, EModeChange.Nerf, "ChlorophyteBullet");
                    //tooltips.Add(new TooltipLine(Mod, "masoNerf3", "[c/ff0000:Eternity Mode:] Further reduced damage"));
                    //break;

                case ItemID.WaterBolt:
                    if (!NPC.downedBoss3)
                    {
                        balanceTextKeys = new string[] { "WaterBolt" };
                        return BalanceTypes.CustomNerf;
                    }
                    return BalanceTypes.None;

                case ItemID.HallowedGreaves:
                case ItemID.HallowedHeadgear:
                case ItemID.HallowedHelmet:
                case ItemID.HallowedHood:
                case ItemID.HallowedMask:
                case ItemID.HallowedPlateMail:
                case ItemID.AncientHallowedGreaves:
                case ItemID.AncientHallowedHeadgear:
                case ItemID.AncientHallowedHelmet:
                case ItemID.AncientHallowedHood:
                case ItemID.AncientHallowedMask:
                case ItemID.AncientHallowedPlateMail:
                    balanceTextKeys = new string[] { "HolyDodge" };
                    return BalanceTypes.CustomNerf;

                case ItemID.FrozenTurtleShell:
                case ItemID.FrozenShield:
                    balanceTextKeys = new string[] { "FrozenTurtleShell" };
                    return BalanceTypes.CustomNerf;

                case ItemID.BrainOfConfusion:
                    balanceTextKeys = new string[] { "BrainOfConfusion" };
                    return BalanceTypes.CustomNerf;

                case ItemID.Zenith:
                    if (WorldSavingSystem.DownedMutant || ModLoader.HasMod("CalamityMod"))
                    {
                        balanceTextKeys = new string[] { "ZenithNone" };
                        return BalanceTypes.CustomNeutral;
                    }
                    else
                    {
                        string bossesToKill = "";
                        if (!WorldSavingSystem.DownedAbom)
                        {
                            bossesToKill += $"{Language.GetTextValue("Mods.FargowiltasSouls.NPCs.AbomBoss.DisplayName")},";
                        }
                        bossesToKill += $"{Language.GetTextValue("Mods.FargowiltasSouls.NPCs.MutantBoss.DisplayName")}";

                        balanceTextKeys = new string[] { "ZenithHitRate" };
                        extra = bossesToKill;
                        return BalanceTypes.CustomNerf;
                    }
                    

                case ItemID.VampireKnives:
                    balanceTextKeys = new string[] { "VampireKnives" };
                    return BalanceTypes.CustomNerf;

                case ItemID.ZapinatorGray:
                case ItemID.ZapinatorOrange:
                    balanceTextKeys = new string[] { "Zapinator" };
                    return BalanceTypes.CustomNerf;

                //case ItemID.EmpressBlade:
                //    tooltips.Add(new TooltipLine(Mod, "masoNerf", "[c/ff0000:Eternity Mode:] Reduced damage by 15%"));
                //    goto case ItemID.StardustCellStaff;
                //case ItemID.StardustCellStaff:
                //    tooltips.Add(new TooltipLine(Mod, "masoNerf", "[c/ff0000:Eternity Mode:] Damage slightly reduced as more are summoned"));
                //    break;

                //case ItemID.DD2BetsyBow:
                //case ItemID.Uzi:
                //case ItemID.PhoenixBlaster:
                //case ItemID.Handgun:
                //case ItemID.SpikyBall:
                //case ItemID.Xenopopper:
                //case ItemID.PainterPaintballGun:
                //case ItemID.MoltenFury:
                //    tooltips.Add(new TooltipLine(Mod, "masoNerf", "[c/ff0000:Eternity Mode:] Reduced damage by 25%"));
                //    break;

                //case ItemID.SkyFracture:
                //case ItemID.SnowmanCannon:
                //    tooltips.Add(new TooltipLine(Mod, "masoNerf", "[c/ff0000:Eternity Mode:] Reduced damage by 20%"));
                //    break;

                //case ItemID.StarCannon:
                //case ItemID.ElectrosphereLauncher:
                //case ItemID.DaedalusStormbow:
                //case ItemID.BeesKnees:
                //case ItemID.LaserMachinegun:
                //    tooltips.Add(new TooltipLine(Mod, "masoNerf", "[c/ff0000:Eternity Mode:] Reduced damage by 33%"));
                //    break;

                //case ItemID.Beenade:
                //case ItemID.BlizzardStaff:
                //    ItemBalance(tooltips, EModeChange.Nerf, "Damage", 33);
                //    ItemBalance(tooltips, EModeChange.Nerf, "Speed", 33);
                //    break;

                //case ItemID.Tsunami:
                //case ItemID.Flairon:
                //case ItemID.ChlorophyteShotbow:
                //case ItemID.HellwingBow:
                //case ItemID.DartPistol:
                //case ItemID.DartRifle:
                //case ItemID.Megashark:
                //case ItemID.ChainGun:
                //case ItemID.VortexBeater:
                //case ItemID.RavenStaff:
                //case ItemID.XenoStaff:
                //case ItemID.Phantasm:
                //case ItemID.NebulaArcanum:
                //case ItemID.Razorpine:
                //case ItemID.StardustDragonStaff:
                //case ItemID.SDMG:
                //case ItemID.LastPrism:
                //    ItemBalance(tooltips, EModeChange.Nerf, "Damage", 15);
                //    break;

                //case ItemID.DemonScythe:
                //    if (NPC.downedBoss2)
                //    {
                //        tooltips.Add(new TooltipLine(Mod, "masoNerf", "[c/ff0000:Eternity Mode:] Reduced damage by 33%"));
                //    }
                //    else
                //    {
                //        tooltips.Add(new TooltipLine(Mod, "masoNerf", "[c/ff0000:Eternity Mode:] Reduced damage by 50% until an evil boss is defeated"));
                //        tooltips.Add(new TooltipLine(Mod, "masoNerf2", "[c/ff0000:Eternity Mode:] Reduced attack speed by 25% until an evil boss is defeated"));
                //    }
                //    break;

                //case ItemID.SpaceGun:
                //    if (NPC.downedBoss2)
                //    {
                //        tooltips.Add(new TooltipLine(Mod, "masoNerf", "[c/ff0000:Eternity Mode:] Reduced damage by 15%"));
                //    }
                //    else
                //    {
                //        tooltips.Add(new TooltipLine(Mod, "masoNerf", "[c/ff0000:Eternity Mode:] Cannot be used until an evil boss is defeated"));
                //    }
                //    break;

                //case ItemID.BeeGun:
                //case ItemID.Grenade:
                //case ItemID.StickyGrenade:
                //case ItemID.BouncyGrenade:
                //    tooltips.Add(new TooltipLine(Mod, "masoNerf2", "[c/ff0000:Eternity Mode:] Reduced attack speed by 33%"));
                //    break;

                //case ItemID.DD2BallistraTowerT1Popper:
                //case ItemID.DD2BallistraTowerT2Popper:
                //case ItemID.DD2BallistraTowerT3Popper:
                //case ItemID.DD2ExplosiveTrapT1Popper:
                //case ItemID.DD2ExplosiveTrapT2Popper:
                //case ItemID.DD2ExplosiveTrapT3Popper:
                //case ItemID.DD2FlameburstTowerT1Popper:
                //case ItemID.DD2FlameburstTowerT2Popper:
                //case ItemID.DD2FlameburstTowerT3Popper:
                //case ItemID.DD2LightningAuraT1Popper:
                //case ItemID.DD2LightningAuraT2Popper:
                //case ItemID.DD2LightningAuraT3Popper:
                case ItemID.FetidBaghnakhs:
                    balanceNumber = 0.75f;
                    return BalanceTypes.AttackSpeed;
                case ItemID.MoonlordTurretStaff:
                    balanceNumber = 0.5f;
                    return BalanceTypes.Damage;

                //case ItemID.MonkAltHead:
                //case ItemID.MonkAltPants:
                //case ItemID.MonkAltShirt:
                //case ItemID.MonkBrows:
                //case ItemID.MonkPants:
                //case ItemID.MonkShirt:
                //case ItemID.SquireAltHead:
                //case ItemID.SquireAltPants:
                //case ItemID.SquireAltShirt:
                //case ItemID.SquireGreatHelm:
                //case ItemID.SquireGreaves:
                //case ItemID.SquirePlating:
                //case ItemID.HuntressAltHead:
                //case ItemID.HuntressAltPants:
                //case ItemID.HuntressAltShirt:
                //case ItemID.HuntressJerkin:
                //case ItemID.HuntressPants:
                //case ItemID.HuntressWig:
                //case ItemID.ApprenticeAltHead:
                //case ItemID.ApprenticeAltPants:
                //case ItemID.ApprenticeAltShirt:
                //case ItemID.ApprenticeHat:
                //case ItemID.ApprenticeRobe:
                //case ItemID.ApprenticeTrousers:
                //case ItemID.AncientBattleArmorHat:
                //case ItemID.AncientBattleArmorPants:
                //case ItemID.AncientBattleArmorShirt:
                //    ItemBalance(tooltips, EModeChange.Buff, "OOASet");
                //    break;

                case ItemID.PumpkinMoonMedallion:
                case ItemID.NaughtyPresent:
                    balanceNumber = 15;
                    balanceTextKeys = new string[] { "MoonsDrops", "MoonsWaves" };
                    return BalanceTypes.CustomNerf;

                case ItemID.CobaltNaginata:
                    balanceNumber = -1;
                    balanceTextKeys = new string[] { "SpearRework", "CobaltNaginataRework" };
                    return BalanceTypes.CustomBuff;
                case ItemID.MythrilHalberd:
                    balanceNumber = -1;
                    balanceTextKeys = new string[] { "SpearRework", "MythrilHalberdRework" };
                    return BalanceTypes.CustomBuff;
                case ItemID.OrichalcumHalberd:
                    balanceNumber = -1;
                    balanceTextKeys = new string[] { "SpearRework", "OrichalcumHalberdRework" };
                    return BalanceTypes.CustomBuff;
                case ItemID.PalladiumPike:
                    balanceNumber = -1;
                    balanceTextKeys = new string[] { "SpearRework", "PalladiumPikeRework" };
                    return BalanceTypes.CustomBuff;
                case ItemID.Spear:
                case ItemID.AdamantiteGlaive:
                case ItemID.TitaniumTrident:
                case ItemID.Trident:
                case ItemID.ObsidianSwordfish:
                case ItemID.Swordfish:
                case ItemID.ChlorophytePartisan:
                    balanceNumber = 1;
                    balanceTextKeys = new string[] { "SpearRework" };
                    return BalanceTypes.CustomBuff;

                default:
                    return BalanceTypes.None;
            }
        }

        public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
        {
            if (!WorldSavingSystem.EternityMode)
                return;

            string extra = string.Empty;
            float balanceNumber = -1;
            string[] balanceTextKeys = null;
            BalanceTypes balance = EmodeBalance(ref item, ref balanceNumber, ref balanceTextKeys, ref extra);

            switch (balance)
            {
                case BalanceTypes.Damage:
                    damage *= balanceNumber;
                    break;
                case BalanceTypes.AttackSpeed:
                    AttackSpeed *= balanceNumber;
                    break;
            }

        }
    }
    public class EmodeItemBalanceTooltip : GlobalItem
    {
        enum EModeChange
        {
            Nerf,
            Buff,
            Neutral
        }


        void ItemBalance(List<TooltipLine> tooltips, EModeChange change, string key, int amount = 0)
        {
            string prefix = Language.GetTextValue($"Mods.FargowiltasSouls.EModeBalance.{change}");
            string nerf = Language.GetTextValue($"Mods.FargowiltasSouls.EModeBalance.{key}", amount == 0 ? null : amount);
            tooltips.Add(new TooltipLine(Mod, $"{change}{key}", $"{prefix} {nerf}"));
        }

        void ItemBalance(List<TooltipLine> tooltips, EModeChange change, string key, string extra)
        {
            string prefix = Language.GetTextValue($"Mods.FargowiltasSouls.EModeBalance.{change}");
            string nerf = Language.GetTextValue($"Mods.FargowiltasSouls.EModeBalance.{key}");
            tooltips.Add(new TooltipLine(Mod, $"{change}{key}", $"{prefix} {nerf} {extra}"));
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (!WorldSavingSystem.EternityMode)
                return;

            //if (item.damage > 0 && (item.ammo == AmmoID.Arrow || item.ammo == AmmoID.Bullet || item.ammo == AmmoID.Dart))
            //{
            //    tooltips.Add(new TooltipLine(Mod, "masoAmmoNerf", "[c/ff0000:Eternity Mode:] Contributes 50% less damage to weapons"));
            //}
            string extra = string.Empty;
            float balanceNumber = -1;
            string[] balanceTextKeys = null;
            BalanceTypes balance = EmodeBalance(ref item, ref balanceNumber, ref balanceTextKeys, ref extra);

            if (balanceTextKeys != null)
            {
                for (int i = 0; i < balanceTextKeys.Length; i++)
                {
                    switch (balance)
                    {
                        case BalanceTypes.Damage:
                            {

                                EModeChange change = balanceNumber > 1 ? EModeChange.Buff : balanceNumber < 1 ? EModeChange.Nerf : EModeChange.Neutral;
                                int amount = change == EModeChange.Buff ? (int)((balanceNumber - 1) * 100) : (int)((1 - balanceNumber) * 100);
                                ItemBalance(tooltips, change, "Damage", amount);
                                break;
                            }

                        case BalanceTypes.AttackSpeed:
                            {
                                EModeChange change = balanceNumber > 1 ? EModeChange.Buff : balanceNumber < 1 ? EModeChange.Nerf : EModeChange.Neutral;
                                int amount = change == EModeChange.Buff ? (int)((balanceNumber - 1) * 100) : (int)((1 - balanceNumber) * 100);
                                ItemBalance(tooltips, change, "Speed", amount);
                                break;
                            }

                        case BalanceTypes.CustomBuff:
                        case BalanceTypes.CustomNerf:
                        case BalanceTypes.CustomNeutral:
                            {
                                EModeChange change = balance == BalanceTypes.CustomBuff ? EModeChange.Buff : balance == BalanceTypes.CustomNerf ? EModeChange.Nerf : EModeChange.Neutral;
                                if (balanceNumber != -1 && balanceTextKeys != null && i == 0)
                                {
                                    ItemBalance(tooltips, change, balanceTextKeys[i], (int)balanceNumber);
                                }
                                else if (extra != string.Empty && balanceTextKeys != null && i == 0)
                                {
                                    ItemBalance(tooltips, change, balanceTextKeys[i], extra);
                                }
                                else
                                {
                                    ItemBalance(tooltips, change, balanceTextKeys[i]);
                                }
                                break;
                            }
                    }
                }
            }
            


            if (item.shoot > ProjectileID.None && ProjectileID.Sets.IsAWhip[item.shoot])
            {
                ItemBalance(tooltips, EModeChange.Nerf, "WhipSpeed");
                ItemBalance(tooltips, EModeChange.Nerf, "WhipStack");
            }
            //else if (item.CountsAsClass(DamageClass.Summon))
            //{
            //    if (!(EModeGlobalProjectile.IgnoreMinionNerf.TryGetValue(item.shoot, out bool ignoreNerf) && ignoreNerf))
            //        ItemBalance(tooltips, EModeChange.Nerf, "MinionStack");

            //    ItemBalance(tooltips, EModeChange.Nerf, "SummonMulticlass");
            //}
        }
    }
}
