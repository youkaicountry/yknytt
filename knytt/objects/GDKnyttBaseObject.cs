using Godot;
using YKnyttLib;

public abstract class GDKnyttBaseObject : Node2D
{
    [Export] public string objectName;

    public KnyttPoint ObjectID { get; private set; }
    public KnyttPoint Coords { get; private set; }

    public GDKnyttObjectLayer Layer { get; private set; }

    public GDKnyttArea GDArea { get { return Layer.ObjectLayers.GDArea; } }
    public Juni Juni { get { return GDArea.GDWorld.Game.Juni; } }

    public bool OrganicEnemy { get; protected set; }

    public void initialize(KnyttPoint object_id, GDKnyttObjectLayer layer, KnyttPoint coords)
    {
        this.Layer = layer;
        this.ObjectID = object_id;
        this.Coords = coords;
        this._impl_initialize();
    }

    protected abstract void _impl_initialize();
    protected abstract void _impl_process(float delta);

    public override void _PhysicsProcess(float delta)
    {
        // TODO: Check mode ( edit should be paused )
        // TODO: Ensure area active
        if (OrganicEnemy) { updateOrganicEnemy(); }

        this._impl_process(delta);
    }

    private void updateOrganicEnemy()
    {
        // TODO: Iterate through all Junis
        Juni.updateOrganicEnemy(GlobalPosition);
    }
}
