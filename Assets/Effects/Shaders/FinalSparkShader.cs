using FargowiltasSouls.Content.Bosses.MutantBoss;
using FargowiltasSouls.Core.Globals;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace FargowiltasSouls.Assets.Effects.Shaders
{
    public class FinalSparkShader : ScreenShaderData
    {
        public FinalSparkShader(Ref<Effect> shader, string passName) : base(shader, passName)
        {

        }

        public override void Update(GameTime gameTime)
        {
            if (Filters.Scene["FargowiltasSouls:FinalSpark"].IsActive())
            {
                if (!(FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.mutantBoss, ModContent.NPCType<MutantBoss>())
                    && Main.npc[EModeGlobalNPC.mutantBoss].ai[0] == -5 && Main.npc[EModeGlobalNPC.mutantBoss].ai[2] >= 420))
                {
                    Filters.Scene.Deactivate("FargowiltasSouls:FinalSpark");
                }
            }
        }

        public override void Apply()
        {
            base.Apply();
        }
    }
}