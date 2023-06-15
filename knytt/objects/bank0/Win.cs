using Godot;

public class Win : GDKnyttBaseObject
{
    public void _on_Area2D_body_entered(Node body)
    {
        if (!(body is Juni juni)) { return; }

        string ending = GDArea.Area.getExtraData("Ending") ?? "Ending";

        var save = GDArea.GDWorld.KWorld.CurrentSave;
        var endings = save.Endings;
        endings.Add(ending);
        save.Endings = endings; // to write to ini
        GDArea.GDWorld.Game.saveGame(save);

        // TODO: send signal to server if no cutscene is shown?
        juni.win(ending);
    }
}
