using Godot;
using System.Linq;
using IniParser.Model;
using YKnyttLib;
using System.Collections.Generic;
using YKnyttLib.Logging;

public class CustomObject : GDKnyttBaseObject
{
    public class CustomObjectInfo
    {
        public string image;
        public int tile_width = 24;
        public int tile_height = 24;
        public int anim_speed = 500;
        public int offset_x = 0;
        public int offset_y = 0;
        public int anim_repeat = 0;
        public int anim_from = 0;
        public int anim_to = -1;
        public int anim_loopback = 0;
    }

    protected CustomObjectInfo info;
    protected AnimatedSprite sprite;
    public GDKnyttBaseObject node;
    private int counter = 0;
    private int cache_key;
    private static Dictionary<int, SpriteFrames> oco_cache = new Dictionary<int, SpriteFrames>();
    private static SpriteFrames custom_frames;
    private static Dictionary<int, int> not_used = new Dictionary<int, int>();
    private static int AREAS_BEFORE_CLEAN = 10; // more than distance to an average next checkpoint

    public override void _Ready()
    {
        string mod = ObjectID.x == 254 ? "B" : "";
        string key = $"Custom Object {mod}{ObjectID.y}";
        var section = GDArea.GDWorld.KWorld.INIData[key];
        if (section == null) { QueueFree(); Deleted = true; return; }

        sprite = GetNode<AnimatedSprite>("AnimatedSprite");
        custom_frames = sprite.Frames;
        cache_key = ObjectID.y + (ObjectID.x == 254 ? 256 : 0);

        int bank = getInt(section, "Bank", -1);
        int obj = getInt(section, "Object", -1);
        bool safe = getString(section, "Hurts")?.ToLower() == "false";
        string image = getString(section, "Image");
        Color color = new Color(KnyttUtil.BGRToRGBA(KnyttUtil.parseBGRString(getString(section, "Color"), 0xFFFFFF)));
        if (bank != -1 && obj != -1)
        {
            var bundle = GDKnyttObjectFactory.buildKnyttObject(new KnyttPoint(bank, obj));
            if (bundle == null) { return; }
            node = bundle.getNode(Layer, Coords);
            node.Position = Position;
            GetParent<GDKnyttObjectLayer>().CallDeferred("add_child", node);

            if (safe) { node.makeSafe(); }
            if (bank == 7) { node.Modulate = color; return; }

            Vector2 tile_size = new Vector2(getInt(section, "Tile Width", 0), getInt(section, "Tile Height", 0));
            Vector2 offset = new Vector2(getInt(section, "Offset X", 0), getInt(section, "Offset Y", 0));
            overrideAnimation(node, image, tile_size, offset);
            return;
        }

        info = new CustomObjectInfo();
        info.image = getString(section, "Image");
        info.tile_width = getInt(section, "Tile Width", info.tile_width);
        info.tile_height = getInt(section, "Tile Height", info.tile_height);
        info.anim_speed = getInt(section, "Init AnimSpeed", info.anim_speed);
        info.offset_x = getInt(section, "Offset X", info.offset_x);
        info.offset_y = getInt(section, "Offset Y", info.offset_y);
        info.anim_repeat = getInt(section, "Init AnimRepeat", info.anim_repeat);
        info.anim_from = getInt(section, "Init AnimFrom", info.anim_from);
        info.anim_to = getInt(section, "Init AnimTo", info.anim_to);
        info.anim_loopback = getInt(section, "Init AnimLoopback", info.anim_loopback);

        if (fillAnimation(cache_key.ToString())) { sprite.Play(); }
    }

    private static string getString(KeyDataCollection section, string key)
    {
        return section.ContainsKey(key) ? section[key] : null;
    }

    private static int getInt(KeyDataCollection section, string key, int @default)
    {
        return KnyttUtil.parseIniInt(getString(section, key)) ?? @default;
    }

    private void overrideAnimation(GDKnyttBaseObject obj, string image, Vector2 tile_size, Vector2 offset)
    {
        var sprite = obj.GetNodeOrNull<AnimatedSprite>("AnimatedSprite") ??
                     obj.GetNodeOrNull<AnimatedSprite>("PathFollow2D/AnimatedSprite");
        var static_sprite = obj.GetNodeOrNull<Sprite>("Sprite");
        if (sprite == null && static_sprite == null) { KnyttLogger.Error($"No sprite found for custom object {ObjectID.y}"); return; }

        if (sprite != null)
        {
            sprite.Offset += offset;
            if (obj is BigSpiker)
            {
                sprite.Offset = new Vector2(offset.x == 0 ? sprite.Offset.x : -24 + offset.x,
                                            offset.y == 0 ? sprite.Offset.y : -24 + offset.y);
            }
        }

        if (oco_cache.ContainsKey(cache_key))
        {
            obj.CustomAnimation = true;
            sprite.Frames = oco_cache[cache_key];
            return;
        }

        var image_texture = GDArea.GDWorld.KWorld.getWorldTexture("Custom Objects/" + image) as Texture;
        if (image != null && (image_texture == null || image_texture.GetHeight() == 0 || image_texture.GetWidth() == 0)) { return; }
        obj.CustomAnimation = true;

        if (static_sprite != null)
        {
            var new_tex = new AtlasTexture();
            new_tex.Atlas = image_texture;
            new_tex.Region = new Rect2(0, 0, static_sprite.Texture.GetSize());
            static_sprite.Texture = new_tex;
            static_sprite.Offset += offset;
            return;
        }

        var new_frames = sprite.Frames.Duplicate() as SpriteFrames;
        sprite.Frames = oco_cache[cache_key] = new_frames;
        int obj_y = !(obj is PowerItem) ? obj.ObjectID.y : PowerItem.Object2Power[obj.ObjectID.y];
        bool one_animation_mode = new_frames.GetAnimationNames().Any(a => a.EndsWith(obj_y.ToString()));
        foreach (var anim in new_frames.GetAnimationNames())
        {
            if (one_animation_mode && !anim.EndsWith(obj_y.ToString())) { continue; }
            for (int i = 0; i < new_frames.GetFrameCount(anim); i++)
            {
                if (image == null) { new_frames.SetFrame(anim, i, null); continue; }

                var tex = new_frames.GetFrame(anim, i) as AtlasTexture;
                if (tex == null) { continue; }
                int columns = tex.Atlas.GetWidth() / (int)tex.Region.Size.x;
                int row = (int)tex.Region.Position.y / (int)tex.Region.Size.y;
                int column = (int)tex.Region.Position.x / (int)tex.Region.Size.x;
                int index = row * columns + column;

                int tile_width = tile_size.x == 0 ? (int)tex.Region.Size.x : (int)tile_size.x;
                int tile_height = tile_size.y == 0 ? (int)tex.Region.Size.y : (int)tile_size.y;
                int new_columns = Mathf.Max(1, image_texture.GetWidth() / tile_width);
                int new_row = index / new_columns;
                int new_column = index % new_columns;

                var new_tex = new AtlasTexture();
                new_tex.Atlas = image_texture;
                Vector2 corner = new Vector2(new_column * tile_width, new_row * tile_height);
                if (corner.y + tile_height > image_texture.GetHeight()) { corner.y %= (image_texture.GetHeight() / tile_height) * tile_height; }
                new_tex.Region = new Rect2(corner, tile_width, tile_height);
                new_frames.SetFrame(anim, i, new_tex);
            }
        }
    }

    protected bool fillAnimation(string animation_name)
    {
        if (not_used.TryGetValue(cache_key, out int i) && i == -1) { return false; }

        bool has_alpha_animation = sprite.Frames.HasAnimation(animation_name);
        bool has_replace_animation = sprite.Frames.HasAnimation(animation_name + " replace");
        if (has_replace_animation) { animation_name += " replace"; }

        if (!has_alpha_animation && !has_replace_animation)
        {
            if (info.image == null) { return false; }
            var image_texture = GDArea.GDWorld.KWorld.getWorldTexture("Custom Objects/" + info.image) as Texture;

            // If texture wasn't loaded, don't try to load it every time
            if (image_texture == null || image_texture.GetHeight() == 0 || image_texture.GetWidth() == 0)
            {
                not_used[cache_key] = -1;
                return false;
            }

            if (image_texture.HasAlpha()) { has_alpha_animation = true; }
            else { has_replace_animation = true; animation_name += " replace"; }

            sprite.Frames.AddAnimation(animation_name);
            fillAnimationInternal(image_texture, animation_name);
            sprite.Frames.SetAnimationSpeed(animation_name, info.anim_speed / 20.0f);
            sprite.Frames.SetAnimationLoop(animation_name, info.anim_repeat == 0 && info.anim_loopback == 0);
        }
        sprite.Offset = new Vector2(info.offset_x, info.offset_y);
        sprite.Animation = animation_name;
        sprite.Frame = info.anim_from;
        if (!has_replace_animation) { sprite.Material = null; } // or if (has_alpha_animation)
        not_used[cache_key] = 0;
        return true;
    }

    private void fillAnimationInternal(Texture image_texture, string animation_name)
    {
        int pos = 0;
        if (image_texture.GetHeight() < info.tile_height) { info.tile_height = image_texture.GetHeight(); }
        if (image_texture.GetWidth() < info.tile_width) { info.tile_width = image_texture.GetWidth(); }
        for (int i = 0; i < image_texture.GetHeight() / info.tile_height; i++)
        {
            for (int j = 0; j < image_texture.GetWidth() / info.tile_width; j++)
            {
                var tile = new AtlasTexture();
                tile.Atlas = image_texture;
                tile.Region = new Rect2(j * info.tile_width, i * info.tile_height, info.tile_width, info.tile_height);
                sprite.Frames.AddFrame(animation_name, tile, pos++);
                if (pos > info.anim_to) { return; }
            }
        }
    }

    private void _on_AnimatedSprite_animation_finished()
    {
        if (sprite.Frames.GetAnimationLoop(sprite.Animation)) { return; }
        counter++;
        if (counter >= info.anim_repeat && info.anim_repeat > 0) { return; }
        sprite.Frame = info.anim_loopback;
        sprite.Play();
    }

    public static void cleanUnused()
    {
        foreach (var key in not_used.Keys.ToArray())
        {
            if (key != 0 && not_used[key] != -1) { not_used[key]++; } // not coins, artifacts, failed images
            if (not_used[key] > AREAS_BEFORE_CLEAN)
            {
                if (custom_frames.HasAnimation($"{key}")) { custom_frames.RemoveAnimation($"{key}"); }
                if (custom_frames.HasAnimation($"{key} replace")) { custom_frames.RemoveAnimation($"{key} replace"); }
                not_used.Remove(key);
            }
        }
    }

    public static void clean()
    {
        oco_cache.Clear();
        not_used.Clear();
        custom_frames?.ClearAll();
        custom_frames = null;
    }
}
