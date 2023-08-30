

using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;
using Terraria.Audio;
using Microsoft.Xna.Framework;
using Terraria.ModLoader.IO;
using FargowiltasSouls.Content.Bosses.Champions;
using FargowiltasSouls.Content.Bosses.BanishedBaron;

namespace FargowiltasSouls.Content.Items.Summons
{

    public class BaronSummon : SoulsItem
    {
        public override string Texture => "FargowiltasSouls/Content/Items/Placeholder";
        public override void SetStaticDefaults()
        {
            //DisplayName.SetDefault("Baron Summon");
            //Tooltip.SetDefault("While underwater at the Ocean, summon the Banished Baron");

            Terraria.GameContent.Creative.CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 5;
        }

        //public override bool IsLoadingEnabled(Mod mod) => false; //prevent appearing

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.rare = ItemRarityID.Orange;
            Item.consumable = true;
            Item.maxStack = 20;
            Item.noUseGraphic = false;
        }

        public override void AddRecipes()
        {
            CreateRecipe() //change
                .AddRecipeGroup("FargowiltasSouls:AnyGoldBar", 4)
                .AddTile(TileID.DemonAltar)
                .Register();
        }

        public override bool CanUseItem(Player Player)
        {
            if (Player.ZoneBeach && Player.wet)
                return !NPC.AnyNPCs(ModContent.NPCType<BanishedBaron>()); //not (x or y)
            return false;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                // If the player using the item is the client
                // (explicitely excluded serverside here)
                SoundEngine.PlaySound(new SoundStyle("FargowiltasSouls/Assets/Sounds/BaronSummon"), player.Center);

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    // If the player is not in multiplayer, spawn directly
                    NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<BanishedBaron>());
                }
                else
                {
                    // If the player is in multiplayer, request a spawn
                    // This will only work if NPCID.Sets.MPAllowedEnemies[type] is true, set in NPC code
                    NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, number: player.whoAmI, number2: ModContent.NPCType<BanishedBaron>());
                }
            }
            return true;
        }
    }
}
