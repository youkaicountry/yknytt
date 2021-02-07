using Godot;

public class Win : GDKnyttBaseObject
{
    public void _on_Area2D_body_entered(Node body)
    {
        if (!(body is Juni juni)) { return; }

        string ending = GDArea.Area.getExtraData("Ending") ?? "Ending";
        
        juni.Powers.Endings.Add(ending);
        var save = GDArea.GDWorld.KWorld.CurrentSave;
        // TODO: use previous save, change endings only! do not change powers!
        // TODO: send signal to server if no cutscene is shown?
        GDArea.GDWorld.Game.saveGame(save.getArea(), save.getAreaPosition(), true);

        juni.win(ending);
    }
}
