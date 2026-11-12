namespace YKnyttLib
{
    // How a level dresses Juni: World.ini's Character, or Juni in its Clothes and Skin, painted the way Juni.shader does
    public static class KnyttJuniLook
    {
        // Juni's own colours in juni.png, and how near a pixel has to be to count as one
        private static readonly float[] CLOTHES = { .937f, .937f, .937f, 1 };
        private static readonly float[] SKIN = { .82f, .72f, .6f, 1 };
        private const float NEAR = 0.06f;

        // A custom character's name, or null for Juni
        public static string character(KnyttWorldInfo info)
        {
            string name = info.Character?.Trim();
            return name == null || name == "" || name.ToLower() == "juni" ? null : name;
        }

        // A pixel as 0xRRGGBBAA. Clothes and skin are KS colours, -1 leaves Juni's own
        public static uint recolor(uint rgba, int clothes, int skin)
        {
            if (clothes != -1 && near(rgba, CLOTHES)) { return (uint)KnyttUtil.BGRToRGBA(clothes); }
            if (skin != -1 && near(rgba, SKIN)) { return (uint)KnyttUtil.BGRToRGBA(skin); }
            return rgba;
        }

        private static bool near(uint rgba, float[] color)
        {
            for (int i = 0; i < 4; i++)
            {
                float channel = ((rgba >> (24 - i * 8)) & 255) / 255f;
                if (System.Math.Abs(channel - color[i]) >= NEAR) { return false; }
            }
            return true;
        }
    }
}
