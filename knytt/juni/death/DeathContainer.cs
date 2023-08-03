using Godot;
using System.Collections.Generic;

public class DeathContainer : Node2D
{
    List<Vector2> DeathPositions = new List<Vector2>();

    private bool _on = false;
    public bool On {
        get { return _on; }
        set
        {
            if (_on)
            {
                if (value) { return; }
                clearDeathSprites();
            }
            else
            {
                if (!value) { return; }
                syncDeathSprites();
            }

            _on = value;
            Visible = value;
        }
    }

    PackedScene marker_scene;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        marker_scene = ResourceLoader.Load("res://knytt/juni/death/DeathMarker.tscn") as PackedScene;
        Visible = false;
    }

    public void onDied(Juni juni)
    {
        DeathPositions.Add(juni.Position);
        if (On) { spawnAt(juni.Position); }
    }

    private void spawnAt(Vector2 pos)
    {
        var marker = marker_scene.Instance<Sprite>();
        marker.Position = pos;
        AddChild(marker);
    }

    private void syncDeathSprites()
    {
        foreach (var pos in DeathPositions)
        {
            spawnAt(pos);
        }
    }

    private void clearDeathSprites()
    {
        for (int i = 0; i < GetChildCount(); i++)
        {
            var child = GetChild(i);
            child.QueueFree();
        }
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
