namespace YKnyttLib
{
    // A block of cells to paint: tiles from a tileset, or an area copied off a screen. Cells go row by row
    public class KnyttStamp
    {
        public int Width { get; }
        public int Height { get; }
        public bool Single => Width == 1 && Height == 1;

        private readonly int[] cells;

        public KnyttStamp(int width, int height, int[] cells)
        {
            Width = width;
            Height = height;
            this.cells = cells;
        }

        // Past its edges the stamp repeats, which is how a fill uses it
        public int at(int x, int y) => cells[KnyttPoint.positiveMod(y, Height) * Width + KnyttPoint.positiveMod(x, Width)];

        // Where the stamp goes for a cell of a stroke that began at origin, so a drag lays it side by side
        public KnyttPoint snap(KnyttPoint origin, KnyttPoint cell) => new KnyttPoint(
            cell.x - KnyttPoint.positiveMod(cell.x - origin.x, Width),
            cell.y - KnyttPoint.positiveMod(cell.y - origin.y, Height));

        public static KnyttStamp cut(int[] layer, int layer_width, int x, int y, int width, int height)
        {
            var cells = new int[width * height];
            for (int row = 0; row < height; row++)
            {
                for (int column = 0; column < width; column++)
                {
                    cells[row * width + column] = layer[(y + row) * layer_width + x + column];
                }
            }
            return new KnyttStamp(width, height, cells);
        }

        // A block of a tileset. Offset picks the tileset; tile 0 erases in either
        public static KnyttStamp tiles(int first, int width, int height, int tileset_width, int offset)
        {
            var cells = new int[width * height];
            for (int row = 0; row < height; row++)
            {
                for (int column = 0; column < width; column++)
                {
                    int tile = first + row * tileset_width + column;
                    cells[row * width + column] = tile == 0 ? 0 : tile + offset;
                }
            }
            return new KnyttStamp(width, height, cells);
        }
    }
}
