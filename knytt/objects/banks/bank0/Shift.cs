using Godot;
using YKnyttLib;
using static YKnyttLib.KnyttSwitch;

public class Shift : Switch
{
    KnyttShift shift;

    public override void _Ready()
    {
        @switch = shift = new KnyttShift(GDArea.Area, Coords, (SwitchID)(ObjectID.y-14));
        base._Ready();
    }

    protected override async void _execute(Juni juni)
    {
        var game = GDArea.GDWorld.Game;
        var jgp = juni.GlobalPosition;

        if (shift.Delay > 0)
        {
            var delay_timer = GetNode<Timer>("DelayTimer");
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

        var relative_area = shift.RelativeArea;
        if (!relative_area.isZero())
        {
            game.changeAreaDelta(relative_area, true);
        }

        // Move Juni to the same spot in the new area
        jgp.x += relative_area.x * GDKnyttArea.Width;
        jgp.y += relative_area.y * GDKnyttArea.Height;

        // Move Juni to the correct location in the area
        if (shift.Quantize)
        {
            juni.moveToPosition(game.CurrentArea, shift.AbsolutePosition);
        }
        else
        {
            var relative_pos = shift.RelativePosition;
            jgp.x += relative_pos.x * GDKnyttAssetManager.TILE_WIDTH;
            jgp.y += relative_pos.y * GDKnyttAssetManager.TILE_HEIGHT;
            juni.GlobalPosition = jgp;
        }
        
        if (shift.FlagOn != null)
        {
            if (shift.FlagOn.power)
            {
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

        if (shift.Effect)
        {
            game.CurrentArea.playEffect(shift.AbsolutePosition);
        }

        if (sound != null)
        {
            juni.playSound(sound);
        }

        if (shift.StopMusic)
        {
            if (relative_area.isZero())
            {
                // TODO: This is a workaround to mute the music for a while
                var player = game.MusicChannel.GetNode<AnimationPlayer>("AnimationPlayer");
                player.PlaybackSpeed = 0.3f;
                player.Play("FadeIn");
            }
            else
            {
                game.MusicChannel.Stop();
            }
        }

        if (shift.Save)
        {
            game.saveGame(shift.AbsoluteArea, shift.AbsolutePosition, true);
        }

        if (shift.Cutscene != null)
        {
            // Cutscene can't be paused right now, because it re-emits signals after unpausing
            // TODO: remove this timer when cutscene fade-in is ready
            GetNode<Timer>("CutsceneTimer").Start();
            await ToSignal(GetNode<Timer>("CutsceneTimer"), "timeout");

            // TODO: Cutscene stops the sound and it will be played later
            // In the original game sound is played at the start of cutscene while fading in
            GDKnyttDataStore.playCutscene(shift.Cutscene);
        }
    }
}
