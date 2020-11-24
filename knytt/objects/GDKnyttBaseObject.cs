using Godot;
using YKnyttLib;

public class GDKnyttBaseObject : Node2D
{
    [Export] public string objectName;

    public KnyttPoint ObjectID { get; private set; }
    public KnyttPoint Coords { get; private set; }

    public Vector2 Center 
    { 
        get 
        { 
            var gp = GlobalPosition;
            return gp + new Vector2(12f, 12f);
        }  
    }

    public GDKnyttObjectLayer Layer { get; private set; }

    public GDKnyttArea GDArea { get { return Layer.ObjectLayers.GDArea; } }
    public Juni Juni { get { return GDArea.GDWorld.Game.Juni; } }

    [Export] public bool OrganicEnemy { get; protected set; } = false;

    public void initialize(KnyttPoint object_id, GDKnyttObjectLayer layer, KnyttPoint coords)
    {
        SetPhysicsProcess(false);
        this.Layer = layer;
        this.ObjectID = object_id;
        this.Coords = coords;
        this._Initialize();
    }

    protected virtual void _Initialize() { }

    public override void _PhysicsProcess(float delta)
    {
        // TODO: Check mode ( edit should be paused )
        // TODO: Ensure area active
        if (OrganicEnemy) { updateOrganicEnemy(); }
    }

    private void updateOrganicEnemy()
    {
        // TODO: Iterate through all Junis
        Juni.updateOrganicEnemy(GlobalPosition);
    }

    // Connect signal to this function if you have simple deadly area in an object
    protected void onDeadlyAreaEntered(Node body)
    {
        if (body is Juni juni) { juni.die(); }
    }
}
