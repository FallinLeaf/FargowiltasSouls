using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using FargowiltasSouls.Content.Buffs.Masomode;
using FargowiltasSouls.Core.Systems;
using Terraria.DataStructures;

namespace FargowiltasSouls.Content.Items
{
    public class EModeGlobalItem : GlobalItem
    {
        public override void PickAmmo(Item weapon, Item ammo, Player player, ref int type, ref float speed, ref StatModifier damage, ref float knockback)
        {
            if (!WorldSavingSystem.EternityMode)
                return;

            //ammo nerf
            //if (ammo.ammo == AmmoID.Arrow || ammo.ammo == AmmoID.Bullet || ammo.ammo == AmmoID.Dart)
            //{
            //    damage -= (int)Math.Round(ammo.damage * player.GetDamage(DamageClass.Ranged).Additive * 0.5, MidpointRounding.AwayFromZero); //always round up
            //}
        }
        public override void HoldItem(Item item, Player player)
        {
            EModePlayer ePlayer = player.GetModPlayer<EModePlayer>();
            if (item.type == ItemID.MythrilHalberd)
            {
                if (!player.ItemAnimationActive)
                    ePlayer.MythrilHalberdTimer++;

                if (ePlayer.MythrilHalberdTimer > 121)
                    ePlayer.MythrilHalberdTimer = 121;

                if (ePlayer.MythrilHalberdTimer == 120)
                {
                    SoundEngine.PlaySound(new SoundStyle("FargowiltasSouls/Assets/Sounds/ChargeSound"), player.Center);
                }
            }
            else
            {
                ePlayer.MythrilHalberdTimer = 0;
            }
            base.HoldItem(item, player);
        }
        public override bool CanUseItem(Item item, Player player)
        {
            if (!WorldSavingSystem.EternityMode)
                return base.CanUseItem(item, player);

            if (item.damage <= 0 && (item.type == ItemID.RodofDiscord || item.type == ItemID.ActuationRod || item.type == ItemID.WireKite || item.type == ItemID.WireCutter || item.type == ItemID.Wrench || item.type == ItemID.BlueWrench || item.type == ItemID.GreenWrench || item.type == ItemID.MulticolorWrench || item.type == ItemID.YellowWrench || item.type == ItemID.Actuator))
            {
                //either player is affected by lihzahrd curse, or cursor is targeting a place in temple (player standing outside)
                if (player.GetModPlayer<FargoSoulsPlayer>().LihzahrdCurse || Framing.GetTileSafely(Main.MouseWorld).WallType == WallID.LihzahrdBrickUnsafe && !player.buffImmune[ModContent.BuffType<LihzahrdCurseBuff>()])
                    return false;
            }

            if (item.type == ItemID.RodofDiscord && FargoSoulsUtil.AnyBossAlive())
            {
                player.chaosState = true;
            }

            if (item.type == ItemID.RodOfHarmony && FargoSoulsUtil.AnyBossAlive())
            {
                player.hurtCooldowns[0] = 0;
                var defense = player.statDefense;
                float endurance = player.endurance;
                player.statDefense.FinalMultiplier *= 0;
                player.endurance = 0;
                player.Hurt(PlayerDeathReason.ByCustomReason(player.name + " didn't materialize."), player.statLifeMax2 / 7, 0, false, false, 0, false);
                player.statDefense = defense;
                player.endurance = endurance;
                
            }

            return base.CanUseItem(item, player);
        }
        public override bool? UseItem(Item item, Player player)
        {
            if (!WorldSavingSystem.EternityMode)
                return base.UseItem(item, player);

            if (item.type == ItemID.MechdusaSummon && Main.zenithWorld)
            {
                Main.time = 18000;
            }
            return base.UseItem(item, player);
        }
        public override void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (!WorldSavingSystem.EternityMode)
                return;

            if (!NPC.downedBoss3 && item.type == ItemID.WaterBolt)
            {
                type = ProjectileID.WaterGun;
                damage = 0;
            }

            if (!NPC.downedBoss2 && item.type == ItemID.SpaceGun)
            {
                type = ProjectileID.ConfettiGun;
                damage = 0;
            }

            if (player.GetModPlayer<EModePlayer>().MythrilHalberdTimer >= 120 && item.type == ItemID.MythrilHalberd)
            {
                damage *= 3;
                player.GetModPlayer<EModePlayer>().MythrilHalberdTimer = 0;
            }
        }
    }
}