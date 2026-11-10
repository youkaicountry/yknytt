using System;
using System.Collections.Generic;
using Godot;
using YKnyttLib;

// Draws a screen the way the editor shows it: on a canvas for the screen view and the world map, or into an image
public class AreaPainter
{
    public static readonly Vector2 TILE = new Vector2(GDKnyttAssetManager.TILE_WIDTH, GDKnyttAssetManager.TILE_HEIGHT);
    public static readonly Vector2 SIZE = new Vector2(GDKnyttArea.Width, GDKnyttArea.Height);

    private readonly EditorAssets assets;
    private readonly Dictionary<Texture, Image> images = new Dictionary<Texture, Image>();

    public AreaPainter(EditorAssets assets)
    {
        this.assets = assets;
    }

    private static readonly int[] ALL = { 0, 1, 2, 3 };
    private static readonly int[] NOT_TWO = { 0, 1, 3 };
    private static readonly int[] TWO = { 2 };
    private static readonly int[] NOT_THREE = { 0, 1, 2 };
    private static readonly int[] THREE = { 3 };

    // With Overlay=True the game draws tile layer 2 between object layers 6 and 7
    public static bool overlay(KnyttArea area) => area.getExtraData("Overlay")?.ToLower() == "true";

    public void drawArea(CanvasItem canvas, KnyttArea area, Vector2 origin, Func<int, float> alpha)
    {
        if (area == null || area.Empty) { return; }
        drawGradient(canvas, area.Background, origin);
        bool over = overlay(area);
        drawTiles(canvas, area, origin, alpha, over ? NOT_TWO : ALL);
        drawObjects(canvas, area, origin, alpha, over ? NOT_THREE : ALL);
        if (!over) { return; }
        drawTiles(canvas, area, origin, alpha, TWO);
        drawObjects(canvas, area, origin, alpha, THREE);
    }

    public void drawGradient(CanvasItem canvas, int num, Vector2 origin)
    {
        var texture = assets.getGradient(num);
        if (texture == null) { return; }

        int width = texture.GetWidth();
        for (int x = 0; x < GDKnyttArea.Width; x += width)
        {
            var size = new Vector2(Mathf.Min(width, GDKnyttArea.Width - x), GDKnyttArea.Height);
            canvas.DrawTextureRectRegion(texture, new Rect2(origin + new Vector2(x, 0), size), new Rect2(Vector2.Zero, size));
        }
    }

    private void drawTiles(CanvasItem canvas, KnyttArea area, Vector2 origin, Func<int, float> alpha, int[] layers)
    {
        // KnyttArea.TilesetB holds tiles 0-127, which the editor calls tileset A
        var tileset_a = assets.getTileset(area.TilesetB);
        var tileset_b = assets.getTileset(area.TilesetA);

        foreach (int layer in layers)
        {
            var color = new Color(1, 1, 1, alpha(layer));
            if (color.a == 0) { continue; }

            for (int y = 0; y < KnyttArea.AREA_HEIGHT; y++)
            {
                for (int x = 0; x < KnyttArea.AREA_WIDTH; x++)
                {
                    int tile = area.TileLayers[layer].getTile(x, y);
                    var texture = tile < 128 ? tileset_a : tileset_b;
                    if (tile % 128 == 0 || texture == null) { continue; }

                    canvas.DrawTextureRectRegion(texture, new Rect2(origin + new Vector2(x, y) * TILE, TILE), new Rect2(tileSource(tile), TILE), color);
                }
            }
        }
    }

    private void drawObjects(CanvasItem canvas, KnyttArea area, Vector2 origin, Func<int, float> alpha, int[] layers)
    {
        foreach (int layer in layers)
        {
            var color = new Color(1, 1, 1, alpha(KnyttArea.AREA_TILE_LAYERS + layer));
            if (color.a == 0) { continue; }

            for (int y = 0; y < KnyttArea.AREA_HEIGHT; y++)
            {
                for (int x = 0; x < KnyttArea.AREA_WIDTH; x++)
                {
                    var id = area.ObjectLayers[layer].getObjectID(x, y);
                    if (id.y == 0) { continue; }

                    var texture = assets.getObject(id);
                    if (texture != null) { canvas.DrawTexture(texture, origin + new Vector2(x, y) * TILE, color); }
                }
            }
        }
    }

    // A brush of many cells, cut off where the screen ends. Objects are bank * 256 + number
    public void drawStamp(CanvasItem canvas, KnyttStamp stamp, bool objects, Texture tileset_a, Texture tileset_b, KnyttPoint cell, Vector2 origin, Color color)
    {
        for (int y = 0; y < stamp.Height && cell.y + y < KnyttArea.AREA_HEIGHT; y++)
        {
            for (int x = 0; x < stamp.Width && cell.x + x < KnyttArea.AREA_WIDTH; x++)
            {
                int value = stamp.at(x, y);
                var position = origin + new Vector2(x, y) * TILE;
                if (objects)
                {
                    var icon = value % 256 == 0 ? null : assets.getObject(new KnyttPoint(value >> 8, value & 255));
                    if (icon != null) { canvas.DrawTexture(icon, position, color); }
                    continue;
                }
                var texture = value < 128 ? tileset_a : tileset_b;
                if (value % 128 != 0 && texture != null) { canvas.DrawTextureRectRegion(texture, new Rect2(position, TILE), new Rect2(tileSource(value), TILE), color); }
            }
        }
    }

    private static Vector2 tileSource(int tile) =>
        new Vector2(tile % 128 % GDKnyttAssetManager.TILESET_WIDTH, tile % 128 / GDKnyttAssetManager.TILESET_WIDTH) * TILE;

    // The same picture made on the CPU, for the world map's small screens
    public Image makeImage(KnyttArea area)
    {
        var image = new Image();
        image.Create((int)GDKnyttArea.Width, (int)GDKnyttArea.Height, false, Image.Format.Rgba8);

        var gradient = imageOf(assets.getGradient(area.Background));
        if (gradient != null)
        {
            for (int x = 0; x < GDKnyttArea.Width; x += gradient.GetWidth())
            {
                image.BlitRect(gradient, new Rect2(0, 0, Mathf.Min(gradient.GetWidth(), GDKnyttArea.Width - x), GDKnyttArea.Height), new Vector2(x, 0));
            }
        }

        var tileset_a = imageOf(assets.getTileset(area.TilesetB));
        var tileset_b = imageOf(assets.getTileset(area.TilesetA));
        bool over = overlay(area);
        blendTiles(image, area, tileset_a, tileset_b, over ? NOT_TWO : ALL);
        blendObjects(image, area, over ? NOT_THREE : ALL);
        if (over)
        {
            blendTiles(image, area, tileset_a, tileset_b, TWO);
            blendObjects(image, area, THREE);
        }
        return image;
    }

    private static void blendTiles(Image image, KnyttArea area, Image tileset_a, Image tileset_b, int[] layers)
    {
        foreach (int layer in layers)
        {
            for (int y = 0; y < KnyttArea.AREA_HEIGHT; y++)
            {
                for (int x = 0; x < KnyttArea.AREA_WIDTH; x++)
                {
                    int tile = area.TileLayers[layer].getTile(x, y);
                    var tileset = tile < 128 ? tileset_a : tileset_b;
                    if (tile % 128 == 0 || tileset == null) { continue; }
                    image.BlendRect(tileset, new Rect2(tileSource(tile), TILE), new Vector2(x, y) * TILE);
                }
            }
        }
    }

    private void blendObjects(Image image, KnyttArea area, int[] layers)
    {
        foreach (int layer in layers)
        {
            for (int y = 0; y < KnyttArea.AREA_HEIGHT; y++)
            {
                for (int x = 0; x < KnyttArea.AREA_WIDTH; x++)
                {
                    var id = area.ObjectLayers[layer].getObjectID(x, y);
                    var icon = id.y == 0 ? null : imageOf(assets.getObject(id));
                    if (icon != null) { image.BlendRect(icon, new Rect2(Vector2.Zero, icon.GetSize()), new Vector2(x, y) * TILE); }
                }
            }
        }
    }

    private Image imageOf(Texture texture)
    {
        if (texture == null) { return null; }
        if (images.TryGetValue(texture, out var image)) { return image; }
        image = texture.GetData();
        if (image.IsCompressed()) { image.Decompress(); }
        image.Convert(Image.Format.Rgba8);
        return images[texture] = image;
    }

    public void forgetImages()
    {
        images.Clear();
    }
}
