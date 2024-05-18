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
    private int counter = 0;
    private static Dictionary<(string, int), SpriteFrames> oco_cache = new Dictionary<(string, int), SpriteFrames>();

    // TODO: load textures in _Initialize and loadArea in a background thread, because loading may take a long time
    // Currently _Initialize is called in activateArea, and there is no way to do some initializations before it
    // GDKnyttObjectLayer.addObject should be split in two: getNode for loadArea, and addChild for activateArea
    // Nodes must be stored somehow, since they are not in the tree (to prevent _Ready execution)
    // Or just give up, because it's a lot of changes and seems it's noticable only for image loading in custom objects
    // (and only first time, because cache is implemented)
    public override void _Ready()
    {
        string mod = ObjectID.x == 254 ? "B" : "";
        string key = $"Custom Object {mod}{ObjectID.y}";
        var section = GDArea.GDWorld.KWorld.INIData[key];
        if (section == null) { QueueFree(); return; }

        int bank = getInt(section, "Bank", -1);
        int obj = getInt(section, "Object", -1);
        bool safe = getString(section, "Hurts")?.ToLower() == "false";
        string image = getString(section, "Image");
        Color color = new Color(KnyttUtil.BGRToRGBA(KnyttUtil.parseBGRString(getString(section, "Color"), 0xFFFFFF)));
        if (bank != -1 && obj != -1)
        {
            var bundle = GDKnyttObjectFactory.buildKnyttObject(new KnyttPoint(bank, obj));
            if (bundle == null) { return; }
            var node = bundle.getNode(Layer, Coords);
            node.Position = Position;
            GetParent<GDKnyttObjectLayer>().CallDeferred("add_child", node);

            if (safe) { node.makeSafe(); }
            if (bank == 7) { node.Modulate = color; return; }

            Vector2 tile_size = new Vector2(getInt(section, "Tile Width", 0), getInt(section, "Tile Height", 0));
            Vector2 offset = new Vector2(getInt(section, "Offset X", 0), getInt(section, "Offset Y", 0));
            if (image != null) { overrideAnimation(node, image, tile_size, offset); }
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

        sprite = GetNode<AnimatedSprite>("AnimatedSprite");
        if (fillAnimation($"{GDArea.GDWorld.KWorld.WorldDirectoryName} custom{mod}{ObjectID.y}")) { sprite.Play(); }
    }

    private static string getString(KeyDataCollection section, string key)
    {
        return section.ContainsKey(key) ? section[key] : null;
    }

    private static int getInt(KeyDataCollection section, string key, int @default)
    {
        string value = new string(getString(section, key)?.Where(c => char.IsDigit(c) || c == '-')?.ToArray());
        return int.TryParse(value, out int i) ? i : @default;
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

        var cache_key = (GDArea.GDWorld.KWorld.WorldDirectoryName, ObjectID.y + (ObjectID.x == 254 ? 256 : 0));
        if (oco_cache.ContainsKey(cache_key))
        {
            obj.CustomAnimation = true;
            sprite.Frames = oco_cache[cache_key];
            return;
        }

        var image_texture = GDArea.GDWorld.KWorld.getWorldTexture("Custom Objects/" + image) as Texture;
        if (image_texture == null || image_texture.GetHeight() == 0 || image_texture.GetWidth() == 0) { return; }
        obj.CustomAnimation = true;
        
        if (static_sprite != null)
        {
            static_sprite.Texture = image_texture;
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
        bool has_alpha_animation = sprite.Frames.HasAnimation(animation_name);
        bool has_replace_animation = sprite.Frames.HasAnimation(animation_name + " replace");
        if (has_replace_animation) { animation_name += " replace"; }

        if (!has_alpha_animation && !has_replace_animation)
        {
            if (info.image == null) { return false; }
            var image_texture = GDArea.GDWorld.KWorld.getWorldTexture("Custom Objects/" + info.image) as Texture;
    
            // If texture wasn't loaded, create empty animation, don't try to load it every time
            if (image_texture == null || image_texture.GetHeight() == 0 || image_texture.GetWidth() == 0)
            {
                sprite.Frames.AddAnimation(animation_name);
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
}
