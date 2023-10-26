﻿using FargowiltasSouls.Content.Items.Accessories.Enchantments;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Items.Accessories.Masomode
{
    public class DarkenedHeart : SoulsItem
    {
        public override bool Eternity => true;

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Darkened Heart");
            /* Tooltip.SetDefault(@"Grants immunity to Rotting
10% increased movement speed and increased turnaround traction
You spawn mini eaters to seek out enemies every few attacks
'Flies refuse to approach it'"); */
            //             DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "腐化之心");
            //             Tooltip.AddTranslation((int)GameCulture.CultureName.Chinese, @"'苍蝇都不想接近它'
            // 免疫腐败
            // 增加10%移动速度
            // 每隔几次攻击就会产生一个迷你噬魂者追踪敌人");

            Terraria.GameContent.Creative.CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.accessory = true;
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(0, 2);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            FargoSoulsPlayer modPlayer = player.FargoSouls();
            player.buffImmune[ModContent.BuffType<Buffs.Masomode.RottingBuff>()] = true;
            player.moveSpeed += 0.1f;
            modPlayer.DarkenedHeartItem = Item;
            if (modPlayer.DarkenedHeartCD > 0)
                modPlayer.DarkenedHeartCD--;
        }
    }
    public class TinyEaterGlobalProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
            => entity.type == ProjectileID.TinyEater;

        bool fromEnch;

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            if (!projectile.owner.IsWithinBounds(Main.maxPlayers))
                return;
            Player player = Main.player[projectile.owner];
            Item heartItem = player.FargoSouls().DarkenedHeartItem;
            if (player != null && heartItem != null && player.active && source is EntitySource_ItemUse itemSource  && itemSource.Item.type == heartItem.type)
            {
                fromEnch = true;
            }
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (fromEnch)
            {
                target.AddBuff(BuffID.CursedInferno, 60 * 2);
            }
        }
    }
}