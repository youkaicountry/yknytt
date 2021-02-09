using Godot;
using IniParser.Model;
using YKnyttLib;

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


    public override void _Ready()
    {
        string key = $"Custom Object {ObjectID.y}";
        var section = GDArea.GDWorld.KWorld.INIData[key];
        if (section == null) { Visible = false; return; }

        int bank = getInt(section, "Bank", 0);
        int obj = getInt(section, "Object", 0);
        bool safe = getString(section, "Hurts")?.ToLower() == "false";
        if (bank != 0 && obj != 0)
        {
            var bundle = GDKnyttObjectFactory.buildKnyttObject(new KnyttPoint(bank, obj));
            if (bundle != null)
            {
                var node = bundle.getNode(Layer, Coords);
                if (safe) { node.makeSafe(); }
                AddChild(node);
            }
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
        fillAnimation($"custom{ObjectID.y}");
        sprite.Play();
    }

    private static string getString(KeyDataCollection section, string key)
    {
        return section.ContainsKey(key) ? section[key] : null;
    }

    private static int getInt(KeyDataCollection section, string key, int @default)
    {
        return int.TryParse(getString(section, key), out int i) ? i : @default;
    }

    protected bool fillAnimation(string animation_name)
    {
        if (!sprite.Frames.HasAnimation(animation_name))
        {
            if (info.image == null) { return false; }
            var image_texture = GDArea.GDWorld.KWorld.getWorldTexture("Custom Objects/" + info.image) as Texture;
            if (image_texture == null) { return false; }
            if (image_texture.HasAlpha()) { sprite.Material = null; }
            sprite.Frames.AddAnimation(animation_name);
            fillAnimationInternal(image_texture, animation_name);
            sprite.Frames.SetAnimationSpeed(animation_name, info.anim_speed / 20);
            sprite.Frames.SetAnimationLoop(animation_name, info.anim_repeat == 0 && info.anim_loopback == 0);
        }
        sprite.Offset = new Vector2(info.offset_x, info.offset_y);
        sprite.Animation = animation_name;
        sprite.Frame = info.anim_from;
        return true;
    }

    private void fillAnimationInternal(Texture image_texture, string animation_name)
    {
        int pos = 0;
        if (image_texture.GetHeight() < info.tile_height) { info.tile_height = image_texture.GetHeight(); }
        if (image_texture.GetWidth()  < info.tile_width)  { info.tile_width  = image_texture.GetWidth();  }
        for (int i = 0; i < image_texture.GetHeight() / info.tile_height; i++)
        {
            for (int j = 0; j < image_texture.GetWidth() / info.tile_width; j++)
            {
                var tile = new AtlasTexture();
                tile.Atlas = image_texture;
                tile.Region = new Rect2(j * info.tile_width, i * info.tile_height, info.tile_width, info.tile_height);
                sprite.Frames.AddFrame(animation_name, tile, pos++);
                if (info.anim_to != -1 && pos > info.anim_to) { return; }
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
