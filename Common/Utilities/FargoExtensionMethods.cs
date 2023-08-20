using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace FargowiltasSouls.Common.Utilities
{
    public static class FargoExtensionMethods
    {
        /// <summary>
        /// Adjusts a TooltipLine to account for prefixes. <br />
        /// Inteded to be used specifically for item names. <br />
        /// This only modifies it in the inventory.
        /// </summary>
        public static TooltipLine ArticlePrefixAdjustment(this TooltipLine itemName, string[] localizationArticles)
        {
            List<string> splitName = itemName.Text.Split(' ').ToList();

            for (int i = 0; i < localizationArticles.Length; i++)
                if (splitName.Remove(localizationArticles[i]))
                {
                    splitName.Insert(0, localizationArticles[i]);
                    break;
                }

            itemName.Text = string.Join(" ", splitName);
            return itemName;
        }

        /// <summary>
        /// Uses <see cref="Enumerable.First{TSource}(IEnumerable{TSource}, System.Func{TSource, bool})"/> to find the specified tooltip line. <br />
        /// Returns true if the tooltipLine isn't null and false if it is. <br />
        /// Assumes Terraria as the mod.
        /// </summary>
        public static bool TryFindTooltipLine(this List<TooltipLine> tooltips, string tooltipName, out TooltipLine tooltipLine)
        {
            tooltips.TryFindTooltipLine(tooltipName, "Terraria", out tooltipLine);

            return tooltipLine != null;
        }

        /// <summary>
        /// Uses <see cref="Enumerable.First{TSource}(IEnumerable{TSource}, System.Func{TSource, bool})"/> to find the specified tooltip line. <br />
        /// Returns true if the tooltipLine isn't null and false if it is.
        /// </summary>
        public static bool TryFindTooltipLine(this List<TooltipLine> tooltips, string tooltipName, string tooltipMod, out TooltipLine tooltipLine)
        {
            tooltipLine = tooltips.First(line => line.Name == tooltipName && line.Mod == tooltipMod);

            return tooltipLine != null;
        }

        private static readonly FieldInfo _damageFieldHitInfo =
            typeof(NPC.HitInfo).GetField("_damage", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void Null(ref this NPC.HitInfo hitInfo)
        {
            // hitInfo.Damage = 0;
            
            // https://stackoverflow.com/questions/27226731/getfield-setvalue-doesnt-work
            object unboxedHitInfo = hitInfo;
            _damageFieldHitInfo.SetValue(unboxedHitInfo, 0);
            hitInfo = (NPC.HitInfo)unboxedHitInfo;
            hitInfo.Knockback = 0;
            hitInfo.Crit = false;
            // TODO: should we?
            // hitInfo.HideCombatText = true;
            hitInfo.InstantKill = false;
        }

        public static void Null(ref this NPC.HitModifiers hitModifiers)
        {
            // doesn't work because tModLoader maxes it with 1
            // statModifier = statModifier.Scale(0f);
            
            // will break if basically any mod registers this hook as well
            hitModifiers.ModifyHitInfo += (ref NPC.HitInfo hitInfo) => hitInfo.Null();
        }

        private static readonly FieldInfo _damageFieldHurtInfo =
            typeof(Player.HurtInfo).GetField("_damage", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void Null(ref this Player.HurtInfo hurtInfo)
        {
            // hurtInfo.Damage = 0;
            object unboxedHurtInfo = hurtInfo;
            _damageFieldHurtInfo.SetValue(unboxedHurtInfo, 0);
            hurtInfo = (Player.HurtInfo)unboxedHurtInfo;
            hurtInfo.Knockback = 0;
        }

        public static void Null(ref this Player.HurtModifiers hurtModifiers)
        {
            // will break if basically any mod registers this hook as well
            hurtModifiers.ModifyHurtInfo += (ref Player.HurtInfo hurtInfo) => hurtInfo.Null();
        }
    }
}