using System.Collections.Generic;

namespace YKnyttLib
{
    // The cells a fill or a replace changes, on one layer of a screen. Cells go row by row
    public static class KnyttFill
    {
        // The cells joined to start, side by side or up and down, that have its value
        public static List<int> flood(int[] cells, int width, int start)
        {
            var found = new List<int>();
            if (start < 0 || start >= cells.Length) { return found; }

            int value = cells[start];
            var seen = new bool[cells.Length];
            var next = new Stack<int>();
            next.Push(start);
            seen[start] = true;
            while (next.Count > 0)
            {
                int cell = next.Pop();
                found.Add(cell);
                int x = cell % width;
                if (x > 0) { visit(cell - 1); }
                if (x < width - 1) { visit(cell + 1); }
                visit(cell - width);
                visit(cell + width);
            }
            found.Sort();
            return found;

            void visit(int cell)
            {
                if (cell < 0 || cell >= cells.Length || seen[cell] || cells[cell] != value) { return; }
                seen[cell] = true;
                next.Push(cell);
            }
        }

        // Every cell that has the value
        public static List<int> matching(int[] cells, int value)
        {
            var found = new List<int>();
            for (int i = 0; i < cells.Length; i++) { if (cells[i] == value) { found.Add(i); } }
            return found;
        }
    }
}
