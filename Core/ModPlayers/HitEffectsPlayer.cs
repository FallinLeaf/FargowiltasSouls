using FargowiltasSouls.Content.Buffs.Masomode;
using FargowiltasSouls.Content.Buffs.Souls;
using FargowiltasSouls.Content.Items.Accessories.Enchantments;
using FargowiltasSouls.Content.Projectiles.Masomode;
using FargowiltasSouls.Content.Projectiles.Minions;
using FargowiltasSouls.Content.Projectiles.Souls;
using FargowiltasSouls.Content.Projectiles;
using Microsoft.Xna.Framework;
using System;
using FargowiltasSouls.Common.Utilities;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using FargowiltasSouls.Content.Items.Armor;
using Terraria.DataStructures;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.Toggler;
using FargowiltasSouls.Content.Bosses.DeviBoss;
using FargowiltasSouls.Content.Bosses.AbomBoss;
using FargowiltasSouls.Content.Bosses.MutantBoss;
using Terraria.WorldBuilding;

namespace FargowiltasSouls.Core.ModPlayers
{
    public partial class FargoSoulsPlayer
    {
        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (proj.hostile)
                return;

            if (SpiderEnchantActive && FargoSoulsUtil.IsSummonDamage(proj))
            {
                if (Main.rand.Next(100) < Player.ActualClassCrit(DamageClass.Summon))
                    modifiers.SetCrit();
            }

            if (apprenticeBonusDamage)
            {
                if (ShadowForce)
                {
                    modifiers.FinalDamage *= 2.5f;
                }
                else
                {
                    modifiers.FinalDamage *= 1.5f;
                }

                apprenticeBonusDamage = false;
                apprenticeSwitchReady = false;
                ApprenticeCD = 0;

                //dust
                int dustId = Dust.NewDust(new Vector2(proj.position.X, proj.position.Y + 2f), proj.width, proj.height + 5, DustID.FlameBurst, 0, 0, 100, Color.Black, 2f);
                Main.dust[dustId].noGravity = true;

                modifiers.ModifyHitInfo += (ref NPC.HitInfo hitInfo) =>
                {
                    var blastDamage = hitInfo.Damage;
                    if (!TerrariaSoul)
                        blastDamage = Math.Min(blastDamage, FargoSoulsUtil.HighestDamageTypeScaling(Player, 300));
                    Projectile.NewProjectile(Player.GetSource_Misc(""), target.Center, Vector2.Zero, ProjectileID.InfernoFriendlyBlast, blastDamage, 0, Player.whoAmI);
                };
            }
            /*
            if (Hexed || (ReverseManaFlow && proj.CountsAsClass(DamageClass.Magic)))
            {
                target.life += (int)modifiers.FinalDamage.Base;
                target.HealEffect((int)modifiers.FinalDamage.Base);

                if (target.life > target.lifeMax)
                {
                    target.life = target.lifeMax;
                }

                modifiers.Null();
                return;

            }
            */
            if (SqueakyToy)
            {
                modifiers.FinalDamage.Base = 1;
                Squeak(target.Center);
                return;
            }

            if (Asocial && FargoSoulsUtil.IsSummonDamage(proj, true, false))
            {
                modifiers.Null();
            }

            if (Atrophied && (proj.CountsAsClass(DamageClass.Melee) || proj.CountsAsClass(DamageClass.Throwing)))
            {
                modifiers.Null();
            }

            if (TungstenEnchantItem != null && proj.GetGlobalProjectile<FargoSoulsGlobalProjectile>().TungstenScale != 1)
            {
                TungstenEnchant.TungstenModifyDamage(Player, ref modifiers, proj.DamageType);
            }

            if (HuntressEnchantActive && proj.GetGlobalProjectile<FargoSoulsGlobalProjectile>().HuntressProj == 1)
            {
                HuntressEnchant.HuntressBonus(this, proj, target, ref modifiers);
            }

            ModifyHitNPCBoth(target, ref modifiers, proj.DamageType);
        }

        public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (NinjaEnchantItem != null && Player.GetToggleValue("NinjaSpeed"))
            {
                modifiers.FinalDamage /= 2;
            }
            /*
            if (Hexed || (ReverseManaFlow && item.CountsAsClass(DamageClass.Magic)))
            {
                modifiers.ModifyHitInfo += (ref NPC.HitInfo hitInfo) =>
                {
                    target.life += hitInfo.Damage;
                    target.HealEffect(hitInfo.Damage);

                    if (target.life > target.lifeMax)
                    {
                        target.life = target.lifeMax;
                    }
                    
                    hitInfo.Null();
                };

                return;

            }
            */
            if (SqueakyToy)
            {
                modifiers.SetMaxDamage(1);
                Squeak(target.Center);
                return;
            }

            if (Atrophied)
            {
                modifiers.Null();
            }

            if (TungstenEnchantItem != null && Toggler != null && Player.GetToggleValue("Tungsten")
                && (TerraForce || item.shoot == ProjectileID.None))
            {
                TungstenEnchant.TungstenModifyDamage(Player, ref modifiers, item.DamageType);
            }

            ModifyHitNPCBoth(target, ref modifiers, item.DamageType);
        }

        public void ModifyHitNPCBoth(NPC target, ref NPC.HitModifiers modifiers, DamageClass damageClass)
        {
            modifiers.ModifyHitInfo += (ref NPC.HitInfo hitInfo) =>
            {
                if (hitInfo.Crit)
                {
                    if (Eternity)
                        hitInfo.Damage *= 5;
                    else if (UniverseCore)
                        hitInfo.Damage *= 2;
                
                    if (SpiderEnchantActive && damageClass.CountsAsClass(DamageClass.Summon) && !TerrariaSoul)
                        hitInfo.Damage = (int)(hitInfo.Damage * 0.75);
                }

                if (Hexed || (ReverseManaFlow && damageClass == DamageClass.Magic))
                {
                    target.life += hitInfo.Damage;
                    target.HealEffect(hitInfo.Damage);
                    if (target.life > target.lifeMax)
                    {
                        target.life = target.lifeMax;
                    }
                    hitInfo.Null();
                    return;

                }
            };

            if (DeerSinewNerf)
            {
                float ratio = Math.Min(Player.velocity.Length() / 20f, 1f);
                modifiers.FinalDamage *= MathHelper.Lerp(1f, 0.85f, ratio);
            }

            if (CerebralMindbreak)
                modifiers.FinalDamage *= 0.7f;

            if (FirstStrike)
            {
                modifiers.SetCrit();
                modifiers.FinalDamage *= 1.5f;
                Player.ClearBuff(ModContent.BuffType<FirstStrikeBuff>());
                //target.defense -= 5;
                target.AddBuff(BuffID.BrokenArmor, 600);
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)/* tModPorter If you don't need the Projectile, consider using OnHitNPC instead */
        {
            if (target.type == NPCID.TargetDummy || target.friendly)
                return;

            if (proj.minion)// && proj.type != ModContent.ProjectileType<CelestialRuneAncientVision>() && proj.type != ModContent.ProjectileType<SpookyScythe>())
                TryAdditionalAttacks(proj.damage, proj.DamageType);

            OnHitNPCEither(target, hit, proj.DamageType, projectile: proj);

            if (OriEnchantItem != null && proj.type == ProjectileID.FlowerPetal)
            {
                target.AddBuff(ModContent.BuffType<OriPoisonBuff>(), 300);
                target.immune[proj.owner] = 2;
            }
        }

        private void OnHitNPCEither(NPC target, NPC.HitInfo hitInfo, DamageClass damageClass, Projectile projectile = null, Item item = null)
        {

            //doing this so that damage-inheriting effects dont double dip or explode due to taking on crit boost
            int GetBaseDamage()
            {
                // TODO: I guess? test this
                int baseDamage = hitInfo.SourceDamage;
                if (projectile != null)
                    baseDamage = projectile.damage;
                else if (item != null)
                    baseDamage = Player.GetWeaponDamage(item);
                return baseDamage;
            }

            

            if (StyxSet)
            {
                StyxMeter += hitInfo.Damage;
                if (StyxTimer <= 0 && !target.friendly && target.lifeMax > 5 && target.type != NPCID.TargetDummy)
                    StyxTimer = 60;
            }

            if (BeetleEnchantActive && Player.beetleOffense && damageClass != DamageClass.Melee)
            {
                Player.beetleCounter += hitInfo.Damage;
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

                        float beeKB = projectile?.knockBack ?? (item?.knockBack ?? hitInfo.Knockback);

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
                Player.AddBuff(BuffID.RapidHealing, Math.Min(300, hitInfo.Damage / 3)); //heal time based on damage dealt, capped at 5sec
            }

            if (CopperEnchantItem != null && hitInfo.Crit)
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
                    if (target.type != ModContent.NPCType<MutantBoss>())
                    {
                        target.DelBuff(4);
                        target.buffImmune[ModContent.BuffType<GodEater>()] = false;
                    }
                }*/
                target.AddBuff(ModContent.BuffType<GodEaterBuff>(), 420);
            }

            if (GladiatorEnchantActive && Player.whoAmI == Main.myPlayer && Player.GetToggleValue("Gladiator") && GladiatorCD <= 0 && (projectile == null || projectile.type != ModContent.ProjectileType<GladiatorJavelin>()))
            {
                GladiatorEnchant.GladiatorSpearDrop(this, target, GetBaseDamage());
            }

            if (SolarEnchantActive && Player.GetToggleValue("SolarFlare") && Main.rand.NextBool(4))
                target.AddBuff(ModContent.BuffType<SolarFlareBuff>(), 300);

            if (TinEnchantItem != null)
            {
                TinEnchant.TinOnHitEnemy(this, hitInfo);
            }

            if (LeadEnchantItem != null)
            {
                target.AddBuff(ModContent.BuffType<LeadPoisonBuff>(), 30);
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
                target.AddBuff(ModContent.BuffType<FlamesoftheUniverseBuff>(), 240);

            if (MasochistSoul)
            {
                target.AddBuff(ModContent.BuffType<SadismBuff>(), 600);
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
                target.AddBuff(ModContent.BuffType<LightningRodBuff>(), 60);
            }

            if (GoldEnchantActive)
                target.AddBuff(BuffID.Midas, 120, true);

            if (DragonFang && !target.boss && !target.buffImmune[ModContent.BuffType<ClippedWingsBuff>()] && Main.rand.NextBool(10))
            {
                target.velocity.X = 0f;
                target.velocity.Y = 10f;
                target.AddBuff(ModContent.BuffType<ClippedWingsBuff>(), 240);
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
                    Projectile p = FargoSoulsUtil.NewProjectileDirectSafe(Player.GetSource_Misc(""), target.position, new Vector2(speedX, speedY), ProjectileID.SpectreWrath, hitInfo.Damage / 2, 0, Player.whoAmI, target.whoAmI);

                    if ((SpiritForce || (hitInfo.Crit && Main.rand.NextBool(5))) && p != null)
                    {
                        SpectreHeal(target, p);
                        SpectreCD = SpiritForce ? 5 : 20;
                    }
                }
                else if (projectile.type != ProjectileID.SpectreWrath)
                {
                    SpectreHurt(projectile);

                    if (SpiritForce || (hitInfo.Crit && Main.rand.NextBool(5)))
                        SpectreHeal(target, projectile);

                    SpectreCD = SpiritForce ? 5 : 20;
                }
            }

            if (AbomWandItem != null)
            {
                //target.AddBuff(ModContent.BuffType<OceanicMaul>(), 900);
                //target.AddBuff(ModContent.BuffType<CurseoftheMoon>(), 900);
                if (hitInfo.Crit && AbomWandCD <= 0 && Player.GetToggleValue("MasoFishron") && (projectile == null || projectile.type != ModContent.ProjectileType<AbomScytheFriendly>()))
                {
                    AbomWandCD = 360;

                    float screenX = Main.screenPosition.X;
                    if (Player.direction < 0)
                        screenX += Main.screenWidth;
                    float screenY = Main.screenPosition.Y;
                    screenY += Main.rand.Next(Main.screenHeight);
                    Vector2 spawn = new(screenX, screenY);
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

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (target.type == NPCID.TargetDummy || target.friendly)
                return;

            OnHitNPCEither(target, hit, item.DamageType, item: item);
        }

        public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
        {
            if (NecromanticBrewItem != null && IsInADashState)
            {
                modifiers.FinalDamage /= 4;
            }

            TitaniumEnchant.TryTitaniumDR(this, npc);

            if (GladiatorEnchantActive && Player.direction == Math.Sign(npc.Center.X - Player.Center.X))
                Player.noKnockback = true;

            if (Smite)
                modifiers.FinalDamage *= 1.2f;

            if (npc.coldDamage && Hypothermia)
                modifiers.FinalDamage *= 1.2f;

            if (npc.GetGlobalNPC<FargoSoulsGlobalNPC>().CurseoftheMoon)
                modifiers.FinalDamage *= 0.8f;
        }

        public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
        {
            if (GroundStick)
            {
                GroundStickCheck(proj, ref modifiers);
            }

            TitaniumEnchant.TryTitaniumDR(this, proj);

            if (GladiatorEnchantActive && Player.direction == Math.Sign(proj.Center.X - Player.Center.X))
                Player.noKnockback = true;

            if (Smite)
                modifiers.FinalDamage *= 1.2f;

            if (proj.coldDamage && Hypothermia)
                modifiers.FinalDamage *= 1.2f;

            //if (npc.GetGlobalNPC<FargoSoulsGlobalNPC>().CurseoftheMoon)
            //damage = (int)(damage * 0.8);
        }

        public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
        {
            OnHitByEither(npc, null);
        }

        public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
        {
            OnHitByEither(null, proj);
        }

        public void OnHitByEither(NPC npc, Projectile proj)
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
                AddBuffNoStack(ModContent.BuffType<StunnedBuff>(), 120);

                Vector2 attacker = default;
                if (npc != null)
                    attacker = npc.Center;
                else if (proj != null)
                    attacker = proj.Center;
                if (attacker != default)
                    Player.velocity = Vector2.Normalize(Player.Center - attacker) * 30;
            }
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
        public override void ModifyHurt(ref Player.HurtModifiers modifiers)/* tModPorter Override ImmuneTo, FreeDodge or ConsumableDodge instead to prevent taking damage */
        {
            if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.deviBoss, ModContent.NPCType<DeviBoss>()))
                ((DeviBoss)Main.npc[EModeGlobalNPC.deviBoss].ModNPC).playerInvulTriggered = true;

            if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.abomBoss, ModContent.NPCType<AbomBoss>()))
                ((AbomBoss)Main.npc[EModeGlobalNPC.abomBoss].ModNPC).playerInvulTriggered = true;

            if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.mutantBoss, ModContent.NPCType<MutantBoss>()))
                ((MutantBoss)Main.npc[EModeGlobalNPC.mutantBoss].ModNPC).playerInvulTriggered = true;

            if (DeathMarked)
                modifiers.FinalDamage *= 1.5f;

            if (Player.whoAmI == Main.myPlayer && !noDodge && SqueakyAcc && Player.GetToggleValue("MasoSqueak") && Main.rand.NextBool(10))
            {
                Squeak(Player.Center);
                modifiers.SetMaxDamage(1);
            }

            modifiers.ModifyHurtInfo += TryParryAttack;

            if (CrimsonEnchantActive && Player.GetToggleValue("Crimson"))
            {
                CrimsonEnchant.CrimsonHurt(Player, this, ref modifiers);
            }

            if (StyxSet && !BetsyDashing && !GoldShell && Player.ownedProjectileCounts[ModContent.ProjectileType<StyxArmorScythe>()] > 0)
            {
                modifiers.ModifyHurtInfo += (ref Player.HurtInfo hurtInfo) =>
                {
                    if(hurtInfo.Damage <= 1) return;
                    
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

                    // should not go below 1 due to math so no hacking here
                    hurtInfo.Damage *= (int)(1.0f - (float)maxDR / maxSacrifice * scythesSacrificed);
                };
            }

            if (DeerSinewNerf && DeerSinewFreezeCD <= 0 && (modifiers.DamageSource.SourceNPCIndex != -1 || (modifiers.DamageSource.SourceProjectileType != -1 && Main.projectile[modifiers.DamageSource.SourceProjectileType].aiStyle != ProjAIStyleID.FallingTile)))
            {
                DeerSinewFreezeCD = 120;
                FargoSoulsUtil.AddDebuffFixedDuration(Player, BuffID.Frozen, 20);
            }
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

        public override void OnHurt(Player.HurtInfo info)
        {
            WasHurtBySomething = true;

            MahoganyCanUseDR = false;

            if (Player.HasBuff(ModContent.BuffType<TitaniumDRBuff>())
                && !Player.HasBuff(ModContent.BuffType<TitaniumCDBuff>()))
            {
                Player.AddBuff(ModContent.BuffType<TitaniumCDBuff>(), 60 * 10);
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
                    int d = Dust.NewDust(vector6 + vector7, 0, 0, DustID.GoldFlame, 0f, 0f, 0, default, 2f);
                    Main.dust[d].noGravity = true;
                    Main.dust[d].velocity = vector7;
                }
            }

            OnHurtEffects(info.Damage);

            if (Midas && Main.myPlayer == Player.whoAmI)
                Player.DropCoins();

            DeviGrazeBonus = 0;
            DeviGrazeCounter = 0;
        }
    }
}
