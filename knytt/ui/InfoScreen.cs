using Godot;
using YKnyttLib;

public class InfoScreen : CanvasLayer
{
    public GDKnyttWorldImpl KWorld { get; private set; }

    public GameButtonInfo ButtonInfo { set { GetNode<RatePanel>("InfoRect/RatePanel").ButtonInfo = value; } }

    public void initialize(GDKnyttWorldImpl world)
    {
        this.KWorld = world;
        GetNode<RatePanel>("InfoRect/RatePanel").KWorld = world;
        if (world.BinMode)
        {
            var loader = new KnyttBinWorldLoader(GDKnyttAssetManager.loadFile(world.WorldDirectory));
            world.setBinMode(loader);
            world.setDirectory(world.WorldDirectory, loader.RootDirectory); // only WorldDirectory was set earlier
        }
        Texture info = (world.worldFileExists("Info+.png") ? world.getWorldTexture("Info+.png") : 
                                                             world.getWorldTexture("Info.png")) as Texture;
        info.Flags |= (uint)Texture.FlagsEnum.Filter;
        GetNode<TextureRect>("InfoRect").Texture = (Texture)info;
        GetNode<SlotButton>("InfoRect/Slot1Button").BaseFile = "user://Saves/" + world.WorldDirectoryName;
        GetNode<SlotButton>("InfoRect/Slot2Button").BaseFile = "user://Saves/" + world.WorldDirectoryName;
        GetNode<SlotButton>("InfoRect/Slot3Button").BaseFile = "user://Saves/" + world.WorldDirectoryName;
    }

    public void _on_BackButton_pressed()
    {
        ClickPlayer.Play();
        KWorld.purgeBinFile();
        this.QueueFree();
    }

    public void closeOtherSlots(int slot)
    {
        closeSlotIfNot(GetNode<SlotButton>("InfoRect/Slot1Button"), slot);
        closeSlotIfNot(GetNode<SlotButton>("InfoRect/Slot2Button"), slot);
        closeSlotIfNot(GetNode<SlotButton>("InfoRect/Slot3Button"), slot);
    }

    private void closeSlotIfNot(SlotButton sb, int slot)
    {
        if (sb.slot != slot) { sb.close(); }
    }

    public void _on_SlotButton_StartGame(bool new_save, string filename, int slot)
    {
        GetNode<LevelSelection>("../LevelSelection").killConsumers();
        
        KnyttSave save = new KnyttSave(KWorld, 
                         new_save ? GDKnyttAssetManager.loadTextFile(KWorld.getWorldData("DefaultSavegame.ini")) :
                                    GDKnyttAssetManager.loadTextFile(filename), 
                                    slot);
        
        KWorld.CurrentSave = save;
        GDKnyttDataStore.KWorld = KWorld;
        GDKnyttDataStore.startGame(new_save);
        this.QueueFree();
    }

}
