using Godot;
using System;
using System.Collections.Generic;
using YKnyttLib;

public class EditorAssets
{
    private readonly KnyttWorld world;
    private readonly Dictionary<int, Texture> tilesets = new Dictionary<int, Texture>();
    private readonly Dictionary<int, Texture> gradients = new Dictionary<int, Texture>();
    private readonly Dictionary<KnyttPoint, Texture> objects = new Dictionary<KnyttPoint, Texture>();
    private readonly Dictionary<int, Texture> banks = new Dictionary<int, Texture>();
    private readonly Dictionary<int, Texture> swatches = new Dictionary<int, Texture>();

    private const int SWATCH_SIZE = 12;
    private static readonly Color SWATCH_BORDER = new Color(0.6f, 0.6f, 0.6f);

    public EditorAssets(KnyttWorld world)
    {
        this.world = world;
    }

    public Texture getTileset(int num)
    {
        if (!tilesets.ContainsKey(num))
        {
            var texture = loadTexture($"Tilesets/Tileset{num}.png");
            tilesets[num] = texture == null || texture.HasAlpha() ? texture : GDKnyttAssetManager.preprocessTilesetTexture(texture);
        }
        return tilesets[num];
    }

    public Texture getGradient(int num)
    {
        if (!gradients.ContainsKey(num)) { gradients[num] = loadTexture($"Gradients/Gradient{num}.png"); }
        return gradients[num];
    }

    public Texture getObject(KnyttPoint id)
    {
        if (!objects.ContainsKey(id))
        {
            objects[id] = (ObjectCatalog.isCustom(id.x) ? loadCustomObject(id) : null) ?? loadIcon(id);
        }
        return objects[id];
    }

    private static Texture loadIcon(KnyttPoint id)
    {
        var texture = GDKnyttAssetManager.loadInternalTexture($"res://knytt/editor/objects/Bank{id.x}/Object{id.y}.png");
        return texture == null ? null : GDKnyttAssetManager.preprocessTilesetTexture(texture);
    }

    // The first frame of its image, or the icon of the object it works like
    private Texture loadCustomObject(KnyttPoint id)
    {
        var section = ObjectCatalog.customSection(world, id);
        if (section == null) { return null; }

        // Sheets too big for the graphics card: their first frame, or repacked like the game does
        string path = $"Custom Objects/{section["Image"]}";
        int frame_width = KnyttUtil.parseIniInt(section["Tile Width"]) ?? 24;
        int frame_height = KnyttUtil.parseIniInt(section["Tile Height"]) ?? 24;
        var image = section["Image"] == null ? null : firstFrame(path, frame_width, frame_height) ??
            (world as GDKnyttWorldImpl)?.loadOverflownTexture(path, frame_width, frame_height) ?? world.getWorldTexture(path) as Texture;
        if (image == null || image.GetWidth() == 0)
        {
            int? bank = KnyttUtil.parseIniInt(section["Bank"]);
            int? obj = KnyttUtil.parseIniInt(section["Object"]);
            return bank == null || obj == null || ObjectCatalog.isCustom(bank.Value) ? null : getObject(new KnyttPoint(bank.Value, obj.Value));
        }

        if (!image.HasAlpha()) { image = GDKnyttAssetManager.preprocessTilesetTexture(image); }
        var size = new Vector2(
            Mathf.Min(frame_width, image.GetWidth()),
            Mathf.Min(frame_height, image.GetHeight()));
        return new AtlasTexture() { Atlas = image, Region = new Rect2(Vector2.Zero, size) };
    }

    private const long BIGGEST_SHEET_PX = 40_000_000;

    // Only the first frame of a sheet too big to be a texture, cut out of the decoded file. The preview needs no more
    private Texture firstFrame(string path, int width, int height)
    {
        int limit = OS.GetName() == "Unix" ? 8192 : 16384;
        var data = world.getWorldData(path);
        if (data == null || !MiniPng.TryReadSize(data, out int sheet_width, out int sheet_height)) { return null; }
        if (sheet_width <= limit && sheet_height <= limit) { return null; }
        if ((long)sheet_width * sheet_height > BIGGEST_SHEET_PX || !MiniPng.TryDecode(data, out sheet_width, out sheet_height, out byte[] rgba)) { return null; }

        width = Math.Min(width, sheet_width);
        height = Math.Min(height, sheet_height);
        var frame = new byte[width * height * 4];
        for (int y = 0; y < height; y++) { Array.Copy(rgba, y * sheet_width * 4, frame, y * width * 4, width * 4); }
        var image = new Image();
        image.CreateFromData(width, height, false, Image.Format.Rgba8, frame);
        var texture = new ImageTexture();
        texture.CreateFromImage(image, 0);
        return texture;
    }

    // Custom objects come from World.ini, which may have changed
    public void forgetObjects()
    {
        objects.Clear();
    }

    public Texture getBank(int bank)
    {
        if (!banks.ContainsKey(bank)) { banks[bank] = GDKnyttAssetManager.loadInternalTexture($"res://knytt/editor/objects/Bank{bank}/Bank.png"); }
        return banks[bank];
    }

    // A small swatch in the colour of the bank box
    public Texture getBankSwatch(int bank)
    {
        if (!swatches.ContainsKey(bank))
        {
            var box = getBank(bank)?.GetData();
            var image = new Image();
            image.Create(SWATCH_SIZE, SWATCH_SIZE, false, Image.Format.Rgba8);
            image.Fill(SWATCH_BORDER);
            if (box != null)
            {
                var inside = new Image();
                inside.Create(SWATCH_SIZE - 2, SWATCH_SIZE - 2, false, Image.Format.Rgba8);
                box.Lock();
                inside.Fill(box.GetPixel(12, box.GetHeight() - 12));
                box.Unlock();
                image.BlitRect(inside, new Rect2(Vector2.Zero, inside.GetSize()), Vector2.One);
            }
            var texture = new ImageTexture();
            texture.CreateFromImage(image, 0);
            swatches[bank] = texture;
        }
        return swatches[bank];
    }

    // For previews, so they loop
    public AudioStream getMusic(int num) => num == 0 ? null : world.getWorldSound($"Music/Song{num}.ogg", loop: true) as AudioStream;
    public AudioStream getAmbiance(int num) => num == 0 ? null : world.getWorldSound($"Ambiance/Ambi{num}.ogg", loop: true) as AudioStream;

    private Texture loadTexture(string path)
    {
        return world.getWorldTexture(path) as Texture ?? GDKnyttAssetManager.loadInternalTexture($"res://knytt/data/{path}");
    }
}
