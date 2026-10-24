using Godot;
using System.Collections.Generic;
using YKnyttLib;

public class EditorAssets
{
    private readonly KnyttWorld world;
    private readonly Dictionary<int, Texture> tilesets = new Dictionary<int, Texture>();
    private readonly Dictionary<int, Texture> gradients = new Dictionary<int, Texture>();
    private readonly Dictionary<KnyttPoint, Texture> objects = new Dictionary<KnyttPoint, Texture>();

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
            var texture = GDKnyttAssetManager.loadInternalTexture($"res://knytt/editor/objects/Bank{id.x}/Object{id.y}.png");
            objects[id] = texture == null ? null : GDKnyttAssetManager.preprocessTilesetTexture(texture);
        }
        return objects[id];
    }

    private Texture loadTexture(string path)
    {
        return world.getWorldTexture(path) as Texture ?? GDKnyttAssetManager.loadInternalTexture($"res://knytt/data/{path}");
    }
}
