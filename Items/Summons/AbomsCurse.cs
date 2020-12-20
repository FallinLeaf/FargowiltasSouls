using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace FargowiltasSouls.Items.Summons
{
    public class AbomsCurse : SoulsItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Abominationn's Curse");
            Main.RegisterItemAnimation(item.type, new DrawAnimationVertical(5, 10));
        }

        public override void SetDefaults()
        {
            item.width = 20;
            item.height = 20;
            item.rare = ItemRarityID.Purple;
            item.maxStack = 999;
            item.useAnimation = 30;
            item.useTime = 30;
            item.useStyle = ItemUseStyleID.HoldingUp;
            item.consumable = true;
            item.value = Item.buyPrice(0, 8);

            item.noUseGraphic = true;
        }

        public override bool UseItem(Player player)
        {
            int abom = NPC.FindFirstNPC(ModLoader.GetMod("Fargowiltas").NPCType("Abominationn"));
            if (abom > -1 && Main.npc[abom].active)
            {
                Main.npc[abom].Transform(mod.NPCType("AbomBoss"));
                if (Main.netMode == NetmodeID.SinglePlayer)
                    Main.NewText("Abominationn has awoken!", 175, 75, 255);
                else if (Main.netMode == NetmodeID.Server)
                    NetMessage.BroadcastChatMessage(NetworkText.FromLiteral("Abominationn has awoken!"), new Color(175, 75, 255));
            }
            else
            {
                NPC.SpawnOnPlayer(player.whoAmI, mod.NPCType("AbomBoss"));
            }
            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> list)
        {
            foreach (TooltipLine line2 in list)
            {
                if (line2.mod == "Terraria" && line2.Name == "ItemName")
                {
                    line2.overrideColor = Main.DiscoColor;
                }
            }
        }

        public override void AddRecipes() //make this harder again when changed to abom's gift
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.GoblinBattleStandard);
            recipe.AddIngredient(ItemID.PirateMap);
            recipe.AddIngredient(ItemID.PumpkinMoonMedallion);
            recipe.AddIngredient(ItemID.NaughtyPresent);
            recipe.AddIngredient(ItemID.SnowGlobe);
            recipe.AddIngredient(ItemID.DD2ElderCrystal);
            recipe.AddIngredient(ItemID.LunarBar, 5);

            recipe.AddTile(ModLoader.GetMod("Fargowiltas").TileType("CrucibleCosmosSheet"));
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}