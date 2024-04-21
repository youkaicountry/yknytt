using Godot;
using System;
using YKnyttLib;

public class GDKnyttBaseObject : Node2D
{
    [Export] public string objectName;

    public KnyttPoint ObjectID { get; private set; }
    public KnyttPoint Coords { get; private set; }

    public Vector2 Center => GlobalPosition + new Vector2(12f, 12f);

    public GDKnyttObjectLayer Layer { get; private set; }

    public GDKnyttArea GDArea => Layer.ObjectLayers.GDArea; 
    public Juni Juni => GDArea.GDWorld.Game.Juni; 

    [Export] public bool OrganicEnemy { get; protected set; } = false;

    protected float organic_enemy_max_distance = 170f;
    protected bool discrete_detector = false;
    protected Color enemy_detector_color = new Color(1, 0, 0);

    private bool safe = false;
    public bool CustomAnimation { get; set; } = false;

    [Export] public bool DenyDeletion;

    protected Random random = GDKnyttDataStore.random; // Shortcut

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
        if (OrganicEnemy) { updateOrganicEnemy(); }
    }

    private void updateOrganicEnemy()
    {
        // TODO: Iterate through all Junis
        if (Juni.GDArea != GDArea) { return; }
        if (!Juni.Powers.getPower(JuniValues.PowerNames.EnemyDetector)) { return; }
        var md = Juni.distance(Center, false);
        if (md < organic_enemy_max_distance)
        {
            float rev_distance = discrete_detector ? 1 : 1f - (md / organic_enemy_max_distance);
            Juni.updateOrganicEnemy(rev_distance, enemy_detector_color);
        }
    }

    public void makeSafe()
    {
        safe = true;
        OrganicEnemy = false;
    }

    public virtual void customDeletion() { }

    public void juniDie(Juni juni)
    {
        if (!safe && juni.GDArea == GDArea) { juni.die(); }
    }

    // Connect signal to this function if you have simple deadly area in an object
    protected void onDeadlyAreaEntered(Node body)
    {
        if (body is Juni juni) { juniDie(juni); }
    }

    protected KinematicCollision2D moveAndCollide(Vector2 relVec, bool infiniteInertia = true,
        bool excludeRaycastShapes = true, bool testOnly = false)
    {
        return Call("move_and_collide", relVec, infiniteInertia, excludeRaycastShapes, testOnly) as KinematicCollision2D;
    }
}
