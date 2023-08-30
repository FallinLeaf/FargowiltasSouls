using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using FargowiltasSouls.Content.Items.Materials;
using FargowiltasSouls.Content.Items.Pets;
using FargowiltasSouls.Content.Items.Placables.MusicBoxes;
using FargowiltasSouls.Content.Items.Placables.Trophies;
using FargowiltasSouls.Content.Items.Summons;
using FargowiltasSouls.Content.Items.Accessories.Forces;
using FargowiltasSouls.Content.Items.Weapons.Challengers;
using FargowiltasSouls.Core.Systems;
using FargowiltasSouls.Content.Bosses.Lieflight;
using FargowiltasSouls.Content.Bosses.TrojanSquirrel;
using FargowiltasSouls.Content.Bosses.Champions.Cosmos;
using FargowiltasSouls.Content.Bosses.Champions.Will;
using FargowiltasSouls.Content.Bosses.Champions.Spirit;
using FargowiltasSouls.Content.Bosses.Champions.Shadow;
using FargowiltasSouls.Content.Bosses.Champions.Life;
using FargowiltasSouls.Content.Bosses.Champions.Nature;
using FargowiltasSouls.Content.Bosses.Champions.Earth;
using FargowiltasSouls.Content.Bosses.Champions.Terra;
using FargowiltasSouls.Content.Bosses.Champions.Timber;
using FargowiltasSouls.Content.Bosses.MutantBoss;
using FargowiltasSouls.Content.Bosses.AbomBoss;
using FargowiltasSouls.Content.Bosses.DeviBoss;
using FargowiltasSouls.Content.Bosses.BanishedBaron;

namespace FargowiltasSouls
{
    public partial class FargowiltasSouls
    {
        private void BossChecklistCompatibility()
        {
            if (ModLoader.TryGetMod("BossChecklist", out Mod bossChecklist))
            {
                static bool AllPlayersAreDead() => Main.player.All(plr => !plr.active || plr.dead);

                void Add(string type, string bossName, List<int> npcIDs, float progression, Func<bool> downed, Func<bool> available, List<int> collectibles, List<int> spawnItems, bool hasKilledAllMessage, string portrait = null)
                {
                    bossChecklist.Call(
                        $"Log{type}",
                        this,
                        bossName,
                        progression,
                        downed,
                        npcIDs,
                        new Dictionary<string, object>()
                        {
                            { "spawnItems", spawnItems },
                            // { "collectibles", collectibles }, // it's fetched from npc loot? TODO: refactor method calls below
                            { "availability", available },
                            { "despawnMessage", hasKilledAllMessage ? new Func<NPC, string>(npc => AllPlayersAreDead() ? $"Mods.{Name}.BossChecklist.{bossName}KilledAll" : $"Mods.{Name}.BossChecklist.{bossName}Despawn") : $"Mods.{Name}.BossChecklist.{bossName}Despawn" },
                            {
                                "customPortrait",
                                portrait == null ? null : new Action<SpriteBatch, Rectangle, Color>((spriteBatch, rect, color) =>
                                {
                                    Texture2D tex = Assets.Request<Texture2D>(portrait, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                                    Rectangle sourceRect = tex.Bounds;
                                    float scale = Math.Min(1f, (float)rect.Width / sourceRect.Width);
                                    spriteBatch.Draw(tex, rect.Center.ToVector2(), sourceRect, color, 0f, sourceRect.Size() / 2, scale, SpriteEffects.None, 0);
                                })
                            }
                        }
                        // available,
                        // collectibles,
                        // spawnItems,
                        // hasKilledAllMessage ? new Func<NPC, string>(npc => AllPlayersAreDead() ? $"Mods.{Name}.BossChecklist.{bossName}KilledAll" : $"Mods.{Name}.BossChecklist.{bossName}Despawn") : $"Mods.{Name}.BossChecklist.{bossName}Despawn",
                        // portrait == null ? null : new Action<SpriteBatch, Rectangle, Color>((spriteBatch, rect, color) =>
                        // {
                        //     Texture2D tex = Assets.Request<Texture2D>(portrait, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                        //     Rectangle sourceRect = tex.Bounds;
                        //     float scale = Math.Min(1f, (float)rect.Width / sourceRect.Width);
                        //     spriteBatch.Draw(tex, rect.Center.ToVector2(), sourceRect, color, 0f, sourceRect.Size() / 2, scale, SpriteEffects.None, 0);
                        // })
                    );
                }
                bool calamity = ModLoader.HasMod("CalamityMod");
                Add("Boss",
                    "DeviBoss",
                    new List<int> { ModContent.NPCType<DeviBoss>() },
                    6.9f,
                    () => WorldSavingSystem.DownedDevi,
                    () => true,
                    new List<int>(new int[] {
                        ModContent.ItemType<DeviMusicBox>(),
                        ModContent.ItemType<DeviatingEnergy>(),
                        ModContent.ItemType<DeviTrophy>(),
                        ModContent.ItemType<ChibiHat>(),
                        ModContent.ItemType<BrokenBlade>()
                    }),
                    new List<int> { ModContent.ItemType<DevisCurse>() },
                    true
                );
                float abomValue = calamity ? 22.6f : 20;
                Add("Boss",
                    "AbomBoss",
                    new List<int> { ModContent.NPCType<AbomBoss>() },
                    abomValue,
                    () => WorldSavingSystem.DownedAbom,
                    () => true,
                    new List<int>(new int[] {
                        ModContent.ItemType<AbomMusicBox>(),
                        ModContent.ItemType<AbomEnergy>(),
                        ModContent.ItemType<AbomTrophy>(),
                        ModContent.ItemType<BabyScythe>(),
                        ModContent.ItemType<BrokenHilt>()
                    }),
                    new List<int> { ModContent.ItemType<AbomsCurse>() },
                    true
                );
                float mutantValue = calamity ? 30 : 23;
                Add("Boss",
                    "MutantBoss",
                    new List<int> { ModContent.NPCType<MutantBoss>() },
                    mutantValue,
                    () => WorldSavingSystem.DownedMutant,
                    () => true,
                    new List<int>(new int[] {
                        ModContent.ItemType<MutantMusicBox>(),
                        ModContent.ItemType<EternalEnergy>(),
                        ModContent.ItemType<MutantTrophy>(),
                        ModContent.ItemType<SpawnSack>(),
                        ModContent.ItemType<PhantasmalEnergy>()
                    }),
                    new List<int> { ModContent.ItemType<AbominationnVoodooDoll>() },
                    true
                );


                #region champions

                Add("MiniBoss",
                    "TimberChampion",
                    new List<int> { ModContent.NPCType<TimberChampion>() },
                    18.1f,
                    () => WorldSavingSystem.DownedBoss[(int)WorldSavingSystem.Downed.TimberChampion],
                    () => true,
                    new List<int>(TimberForce.Enchants).Append(ModContent.ItemType<ChampionMusicBox>()).ToList(),
                    new List<int> { ModContent.ItemType<SigilOfChampions>() },
                    false
                );
                Add("MiniBoss",
                    "TerraChampion",
                    new List<int> { ModContent.NPCType<TerraChampion>(), ModContent.NPCType<TerraChampionBody>(), ModContent.NPCType<TerraChampionTail>() },
                    18.15f,
                    () => WorldSavingSystem.DownedBoss[(int)WorldSavingSystem.Downed.TerraChampion],
                    () => true,
                    new List<int>(TerraForce.Enchants).Append(ModContent.ItemType<ChampionMusicBox>()).ToList(),
                    new List<int> { ModContent.ItemType<SigilOfChampions>() },
                    false,
                    "Content/Bosses/Champions/Terra/TerraChampion_Still"
                );
                Add("MiniBoss",
                    "EarthChampion",
                    new List<int> { ModContent.NPCType<EarthChampion>(), ModContent.NPCType<EarthChampionHand>() },
                    18.2f,
                    () => WorldSavingSystem.DownedBoss[(int)WorldSavingSystem.Downed.EarthChampion],
                    () => true,
                    new List<int>(EarthForce.Enchants).Append(ModContent.ItemType<ChampionMusicBox>()).ToList(),
                    new List<int> { ModContent.ItemType<SigilOfChampions>() },
                    false,
                    "Content/Bosses/Champions/Earth/EarthChampion_Still"
                );
                Add("MiniBoss",
                    "NatureChampion",
                    new List<int> { ModContent.NPCType<NatureChampion>(), ModContent.NPCType<NatureChampionHead>() },
                    18.25f,
                    () => WorldSavingSystem.DownedBoss[(int)WorldSavingSystem.Downed.NatureChampion],
                    () => true,
                    new List<int>(NatureForce.Enchants).Append(ModContent.ItemType<ChampionMusicBox>()).ToList(),
                    new List<int> { ModContent.ItemType<SigilOfChampions>() },
                    false,
                    "Content/Bosses/Champions/Nature/NatureChampion_Still"
                );
                Add("MiniBoss",
                    "LifeChampion",
                    new List<int> { ModContent.NPCType<LifeChampion>() },
                    18.3f,
                    () => WorldSavingSystem.DownedBoss[(int)WorldSavingSystem.Downed.LifeChampion],
                    () => true,
                    new List<int>(LifeForce.Enchants).Append(ModContent.ItemType<ChampionMusicBox>()).ToList(),
                    new List<int> { ModContent.ItemType<SigilOfChampions>() },
                    false,
                    "Content/Bosses/Champions/Life/LifeChampion_Still"
                );
                Add("MiniBoss",
                    "ShadowChampion",
                    new List<int> { ModContent.NPCType<ShadowChampion>() },
                    18.35f,
                    () => WorldSavingSystem.DownedBoss[(int)WorldSavingSystem.Downed.ShadowChampion],
                    () => true,
                    new List<int>(ShadowForce.Enchants).Append(ModContent.ItemType<ChampionMusicBox>()).ToList(),
                    new List<int> { ModContent.ItemType<SigilOfChampions>() },
                    false
                );
                Add("MiniBoss",
                    "SpiritChampion",
                    new List<int> { ModContent.NPCType<SpiritChampion>(), ModContent.NPCType<SpiritChampionHand>() },
                    18.4f,
                    () => WorldSavingSystem.DownedBoss[(int)WorldSavingSystem.Downed.SpiritChampion],
                    () => true,
                    new List<int>(SpiritForce.Enchants).Append(ModContent.ItemType<ChampionMusicBox>()).ToList(),
                    new List<int> { ModContent.ItemType<SigilOfChampions>() },
                    false,
                    "Content/Bosses/Champions/Spirit/SpiritChampion_Still"
                );
                Add("MiniBoss",
                    "WillChampion",
                    new List<int> { ModContent.NPCType<WillChampion>() },
                    18.45f,
                    () => WorldSavingSystem.DownedBoss[(int)WorldSavingSystem.Downed.WillChampion],
                    () => true,
                    new List<int>(WillForce.Enchants).Append(ModContent.ItemType<ChampionMusicBox>()).ToList(),
                    new List<int> { ModContent.ItemType<SigilOfChampions>() },
                    false
                );

                Add("Boss",
                    "CosmosChampion",
                    new List<int> { ModContent.NPCType<CosmosChampion>() },
                    18.5f,
                    () => WorldSavingSystem.DownedBoss[(int)WorldSavingSystem.Downed.CosmosChampion],
                    () => true,
                    new List<int>(CosmoForce.Enchants).Append(ModContent.ItemType<ChampionMusicBox>()).ToList(),
                    new List<int> { ModContent.ItemType<SigilOfChampions>() },
                    true
                );

                #endregion champions


                #region challengers

                Add("Boss",
                    "TrojanSquirrel",
                    new List<int> { ModContent.NPCType<TrojanSquirrel>() },
                    0.5f,
                    () => WorldSavingSystem.DownedBoss[(int)WorldSavingSystem.Downed.TrojanSquirrel],
                    () => true,
                    new List<int>(new int[]
                    {
                        ModContent.ItemType<TrojanSquirrelTrophy>(),
                        ModContent.ItemType<TreeSword>(),
                        ModContent.ItemType<MountedAcornGun>(),
                        ModContent.ItemType<SnowballStaff>(),
                        ModContent.ItemType<KamikazeSquirrelStaff>()
                    }),
                    new List<int> { ModContent.ItemType<SquirrelCoatofArms>() },
                    false,
                    "Content/Bosses/TrojanSquirrel/TrojanSquirrel_Still"
                );
                Add("Boss",
                    "LifeChallenger",
                    new List<int> { ModContent.NPCType<LifeChallenger>() },
                    11.49f,
                    () => WorldSavingSystem.DownedBoss[(int)WorldSavingSystem.Downed.LifeChallenger],
                    () => true,
                    new List<int>(new int[]
                    {
                        ModContent.ItemType<LifeChallengerTrophy>(),
                        ModContent.ItemType<EnchantedLifeblade>(),
                        ModContent.ItemType<Lightslinger>(),
                        ModContent.ItemType<CrystallineCongregation>(),
                        ModContent.ItemType<KamikazePixieStaff>(),
                        ModContent.ItemType<LifelightMasterPet>()
                    }),
                    new List<int> { ModContent.ItemType<FragilePixieLamp>() },
                    false,
                    "Content/Bosses/Lieflight/LifeChallenger"
                );
                
                Add("Boss",
                    "BanishedBaron",
                    new List<int> { ModContent.NPCType<BanishedBaron>() },
                    8.7f,
                    () => WorldSavingSystem.downedBoss[(int)WorldSavingSystem.Downed.BanishedBaron],
                    () => true,
                    new List<int>(new int[]
                    {

                    }),
                    new List<int> { ModContent.ItemType<BaronSummon>() },
                    true
                );
                
                //Add("Boss",
                //    "CursedCoffin",
                //    //TODO: ADD LOOT
                //    new List<int> { ModContent.NPCType<CursedCoffin>() },
                //    4.75f,
                //    () => WorldSavingSystem.downedBoss[(int)WorldSavingSystem.Downed.CursedCoffin],
                //    () => true,
                //    new List<int>(new int[]
                //    {

                //    }),
                //    new List<int> { ModContent.ItemType<CoffinSummon>() },
                //    false,
                //    "Content/NPCs/Challengers/CursedCoffin_Still"
                //);

                #endregion challengers
            }
        }
    }
}
