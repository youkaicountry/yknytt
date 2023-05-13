namespace YKnyttLib
{
    public static class KnyttUtil
    {
        public static int BGRToRGBA(int kc)
        {
            return ((0xFF0000 & kc) >> 8) + ((0xFF00 & kc) << 8) + ((0xFF & kc) << 24) + 0xFF;
        }

        public static int BGRToRGB(int kc)
        {
            return ((0xFF0000 & kc) >> 16) + (0xFF00 & kc) + ((0xFF & kc) << 16);
        }

        // Returns a value in a BGR encoded int. Returns default value if parse fails
        public static int parseBGRString(string val, int def_col)
        {
            if (int.TryParse(val, out var result)) { return result; }

            return def_col;
        }
    }
}
