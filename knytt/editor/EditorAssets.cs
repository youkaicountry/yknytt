using Godot;
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

        var image = section["Image"] == null ? null : world.getWorldTexture($"Custom Objects/{section["Image"]}") as Texture;
        if (image == null || image.GetWidth() == 0)
        {
            int? bank = KnyttUtil.parseIniInt(section["Bank"]);
            int? obj = KnyttUtil.parseIniInt(section["Object"]);
            return bank == null || obj == null || ObjectCatalog.isCustom(bank.Value) ? null : getObject(new KnyttPoint(bank.Value, obj.Value));
        }

        if (!image.HasAlpha()) { image = GDKnyttAssetManager.preprocessTilesetTexture(image); }
        var size = new Vector2(
            Mathf.Min(KnyttUtil.parseIniInt(section["Tile Width"]) ?? 24, image.GetWidth()),
            Mathf.Min(KnyttUtil.parseIniInt(section["Tile Height"]) ?? 24, image.GetHeight()));
        return new AtlasTexture() { Atlas = image, Region = new Rect2(Vector2.Zero, size) };
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

    private Texture loadTexture(string path)
    {
        return world.getWorldTexture(path) as Texture ?? GDKnyttAssetManager.loadInternalTexture($"res://knytt/data/{path}");
    }
}
