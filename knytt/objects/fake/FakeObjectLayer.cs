using System.Collections.Generic;
using Godot;
using YKnyttLib;

public class FakeObjectLayer : Node2D
{
    private static int[] IMAGE_BANK_CAPACITY = 
        { 10, 24, 32, 44, 22, 0, 17, 0, 14, 5, 13, 11, 16, 14, 24, 38, 0, 12, 5, 51 };
    private static int[] IMAGE_BANK_INDEX = new int[IMAGE_BANK_CAPACITY.Length];
    
    static FakeObjectLayer()
    {
        int counter = 0;
        for (int i = 0; i < IMAGE_BANK_CAPACITY.Length; i++)
        {
            IMAGE_BANK_INDEX[i] = counter;
            counter += IMAGE_BANK_CAPACITY[i];
        }
    }

    private List<KnyttPoint> BIG_OBJECTS = new List<KnyttPoint> {
        new KnyttPoint(0, 21), new KnyttPoint(0, 22), new KnyttPoint(0, 23), new KnyttPoint(0, 24), 
        new KnyttPoint(0, 35), new KnyttPoint(4, 9),  new KnyttPoint(5, 1),  new KnyttPoint(5, 2), 
        new KnyttPoint(5, 3),  new KnyttPoint(6, 1),  new KnyttPoint(6, 5),  new KnyttPoint(8, 7),
        new KnyttPoint(12, 6), new KnyttPoint(12, 9), new KnyttPoint(12, 10), new KnyttPoint(13, 3), 
        new KnyttPoint(13, 4), new KnyttPoint(19, 31), 
    };

    private TileMap[] layers;

    public override void _Ready()
    {
        layers = new TileMap[] { GetNode<TileMap>("Layer0"), GetNode<TileMap>("Layer1"), 
            GetNode<TileMap>("Layer2"), GetNode<TileMap>("Layer3") };
    }

    private void setCell(int layer, int x, int y, KnyttPoint obj)
    {
        int big_index = BIG_OBJECTS.IndexOf(obj);
        if (big_index != -1)
        {
            layers[layer].SetCell(x, y, big_index + 1);
        }
        else
        {
            if (obj.x == 19 && obj.y >= 51 && obj.y <= 150) { obj = new KnyttPoint(19, 51); }
            if (obj.x >= IMAGE_BANK_CAPACITY.Length || obj.y > IMAGE_BANK_CAPACITY[obj.x]) { return; }
            int tile_index = IMAGE_BANK_INDEX[obj.x] + obj.y - 1;
            layers[layer].SetCell(x, y, 0, autotileCoord: new Vector2(tile_index % 16, tile_index / 16));
        }
    }

    public void Load(KnyttArea.ObjectLayer[] layers, bool eye)
    {
        for (int i = 0; i < layers.Length; i++)
        {
            for (int y = 0; y < KnyttArea.AREA_HEIGHT; y++)
            {
                for (int x = 0; x < KnyttArea.AREA_WIDTH; x++)
                {
                    var oid = layers[i].getObjectID(x, y);
                    if (oid.isZero()) { continue; }
                    if (oid.x == 12 && !eye) { continue; }
                    setCell(i, x, y, oid);
                }
            }
        }
    }
}
