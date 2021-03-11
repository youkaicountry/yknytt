using Godot;
using YKnyttLib;
using static YKnyttLib.KnyttSwitch;

public class Shift : Switch
{
    KnyttShift shift;

    public override void _Ready()
    {
        if (GDArea.Area.ExtraData == null) { return; }
        @switch = shift = new KnyttShift(GDArea.Area, Coords, (SwitchID)(ObjectID.y - 14));
        base._Ready();
    }

    protected override async void _execute(Juni juni)
    {
        var game = GDArea.GDWorld.Game;
        if (GDArea != game.CurrentArea) { return; }

        if (shift.Delay > 0)
        {
            var delay_timer = GetNode<Timer>("DelayTimer");
            if (!delay_timer.IsStopped()) { return; }
            delay_timer.Start(shift.Delay / 1000f);
            await ToSignal(delay_timer, "timeout");
            if (GDArea != game.CurrentArea) { return; }
        }

        if (shift.Coin != 0)
        {
            if (juni.Powers.getCoinCount() - shift.Coin >= 0)
            {
                juni.Powers.CoinsSpent += shift.Coin;
                juni.updateCollectables();
            }
            else
            {
                return;
            }
        }

        if (shift.FlagOn != null)
        {
            if (shift.FlagOn.power)
            {
                // TODO: it fires RateHTTPRequest which will stop if shift has a cutscene. Power update will not be sent to the server!
                juni.setPower(shift.FlagOn.number, true);
            }
            else
            {
                juni.Powers.setFlag(shift.FlagOn.number, true);
            }
        }

        if (shift.FlagOff != null)
        {
            if (shift.FlagOff.power)
            {
                juni.setPower(shift.FlagOff.number, false);
            }
            else
            {
                juni.Powers.setFlag(shift.FlagOff.number, false);
            }
        }

        var relative_area = shift.RelativeArea;
        if (!relative_area.isZero())
        {
            relative_area += GDArea.GDWorld.Game.getFlagWarp(shift.AbsoluteArea, juni) ?? new KnyttPoint(0, 0);
            game.changeAreaDelta(relative_area, true);
        }
        GD.Print($"shift {shift.RelativeArea} {shift.RelativePosition} / {shift.AbsoluteArea} {shift.AbsolutePosition}");

        // Move Juni to the correct location in the area
        if (shift.Quantize || shift.AbsoluteTarget)
        {
            juni.moveToPosition(game.CurrentArea, shift.AbsolutePosition);
        }
        else
        {
            var jgp = juni.GlobalPosition;
            // Move Juni to the same spot in the new area
            jgp.x += relative_area.x * GDKnyttArea.Width;
            jgp.y += relative_area.y * GDKnyttArea.Height;

            var relative_pos = shift.RelativePosition;
            jgp.x += relative_pos.x * GDKnyttAssetManager.TILE_WIDTH;
            jgp.y += relative_pos.y * GDKnyttAssetManager.TILE_HEIGHT;
            juni.GlobalPosition = jgp;
        }

        if (shift.Effect)
        {
            game.CurrentArea.playEffect(juni.AreaPosition);
        }

        if (sound != null && shift.Cutscene == null)
        {
            juni.playSound(sound);
        }

        if (shift.StopMusic)
        {
            if (relative_area.isZero())
            {
                game.MusicChannel.fadeIn(3);
            }
            else
            {
                game.MusicChannel.Stop();
            }
        }

        if (shift.Cutscene != null)
        {
            var list = shift.Cutscene.ToLower() != "ending" ? juni.Powers.Cutscenes : juni.Powers.Endings;
            list.Add(shift.Cutscene);
        }

        if (shift.Save)
        {
            game.saveGame(shift.AbsoluteArea, shift.AbsolutePosition, true);
        }

        if (shift.Cutscene != null)
        {
            // !! If a cutscene appears two times in a row, revert the commit about micro-delay
            GetTree().Paused = true;
            await game.fade(fast: true, color: Cutscene.getCutsceneColor(shift.Cutscene));
            GDKnyttDataStore.playCutscene(shift.Cutscene, sound);
        }
    }
}
