using FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.LunarEvents.Solar;
using FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.LunarEvents.Vortex;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.LunarEvents
{
    public class PillarSpawner : ModProjectile
    {

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Cosmic Meteor");
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override string Texture => "FargowiltasSouls/Content/Projectiles/Explosion";
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.DamageType = DamageClass.Default;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 60 * 30;
            Projectile.scale = 1;
            AIType = 0;
            Projectile.aiStyle = 0;
        }

        public override void AI()
        {

        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.Kill();
            return true;
        }
        public override void Kill(int timeLeft)
        {
            switch (Projectile.ai[0])
            {
                case 1: //solar pillar flamepillar
                    {
                        SoundEngine.PlaySound(SoundID.Item34, Projectile.Center);
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<SolarFlamePillar>(), Projectile.damage, Projectile.knockBack, Main.myPlayer);
                        }
                        break;
                    }
                case 2: //vortex pillar thunder
                    {
                        SoundEngine.PlaySound(new("FargowiltasSouls/Assets/Sounds/NukeBeep"), Projectile.Center);
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            for (int i = 0; i < 14; i++)
                            {
                                Vector2 posOrig = Projectile.position;
                                NPC parent = Main.npc[(int)Projectile.ai[2]];
                                if (parent.active && Main.player[parent.target].active && !Main.player[parent.target].ghost)
                                {
                                    posOrig.Y = Math.Min(Main.player[parent.target].Center.Y + 500, posOrig.Y); //use the furthest up position of the two
                                }
                                Vector2 pos = posOrig - (Vector2.UnitY * 150 * i);
                                
                                
                                Projectile.NewProjectile(Projectile.GetSource_FromThis(), pos, Vector2.Zero, ModContent.ProjectileType<LightningTelegraph>(), Projectile.damage, Projectile.knockBack, Main.myPlayer, i);
                            }
                        }
                        break;
                    }
                default:
                    {
                        Main.NewText("you shouldn't be seeing this, show javyz");
                        break;
                    }
            }
            
        }
    }
}