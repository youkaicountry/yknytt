namespace YKnyttLib
{
    // How the game lays out a sign: its lines, and where its box goes on the screen
    public static class KnyttSign
    {
        // Where the box's text sits from the sign's corner, and how far the box reaches past the text
        public const float TEXT_X = -66, TEXT_Y = -81.6637f;
        public const float BOX_LEFT = 9, BOX_TOP = 4, BOX_WIDTH = 175, BOX_EXTRA_HEIGHT = 10;
        public const float SCREEN_WIDTH = 600, SCREEN_HEIGHT = 240;

        // A page as written in World.ini, as the game shows it: quotes off, \n as line breaks, and a double space
        // that ends a line of 22 letters (23 when that one is a space or \) breaks there. The label wraps the rest
        public static string wrap(string msg)
        {
            if (msg == null) { return null; }
            if (msg.Length >= 2 && msg[0] == '"' && msg[msg.Length - 1] == '"') { msg = msg.Substring(1, msg.Length - 2); }
            msg = msg.Replace("\\n", "\n");
            msg = msg.Replace('\x7f', '\n');
            for (int i = 0; i < msg.Length; )
            {
                int line_break = msg.IndexOf('\n', i);
                string line = msg.Substring(i,
                                line_break != -1 && line_break + 1 - i <= 23 ? line_break + 1 - i:  // line ends with \n (23 symbols max)
                                msg.Length <= i + 22 ? msg.Length - i :                             // msg ends (line length <= 22)
                                msg[i + 22] == ' ' || msg[i + 22] == '\\' ? 23 :                    // line can be 23 symbols if ends with space or \
                                22);                                                                // 22 is normal line length
                int last_space = line.LastIndexOf(" ", System.StringComparison.Ordinal);
                if (line[line.Length - 1] != '\n' && last_space == line.LastIndexOf("  ", System.StringComparison.Ordinal) + 1)  // break on double spaces
                {
                    msg = msg.Remove(i + last_space, 1).Insert(i + last_space, "\n");
                    line = line.Substring(0, last_space + 1);
                }
                i += line.Length;
            }
            return msg;
        }

        // Where the text goes for a sign at (x, y) on the screen, its box kept on the screen.
        // left and right: the part of the screen in view (all of it unless the game scrolls sideways)
        public static (float x, float y) place(float x, float y, float text_height, float left = 0, float right = SCREEN_WIDTH)
        {
            float box_x = x - BOX_LEFT, box_y = y - BOX_TOP, box_height = text_height + BOX_EXTRA_HEIGHT;
            float text_x = TEXT_X, text_y = TEXT_Y;
            if (box_x + text_x < left) { text_x = left - box_x; }
            if (box_y + text_y < 0) { text_y = -box_y; }
            if (box_x + text_x + BOX_WIDTH > right) { text_x = right - box_x - BOX_WIDTH; }
            if (box_y + text_y + box_height > SCREEN_HEIGHT) { text_y = SCREEN_HEIGHT - box_y - box_height; }
            return (text_x, text_y);
        }
    }
}
