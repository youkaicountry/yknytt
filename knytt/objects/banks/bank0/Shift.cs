using System.Collections.Generic;
using Godot;
using YKnyttLib;
using static YKnyttLib.KnyttShift;

public class Shift : GDKnyttBaseObject
{
    KnyttShift shift;

    private AudioStreamPlayer2D enterPlayer;

    public override void _Ready()
    {
        shift = new KnyttShift(GDArea.Area, Coords, (ShiftID)(ObjectID.y-14));
        setupFromShift();
    }

    private void setupFromShift()
    {
        var shape = GetNode<Node>("Shapes").GetChild<Node2D>((int)shift.Shape);
        shape.Visible = shift.Visible;
        shape.GetNode<Area2D>("Area2D").SetDeferred("monitoring", true);

        var sounds_dict = new Dictionary<string, string>
        {
            {"Door", "DoorPlayer"},
            {"Electronic", "ElectronicPlayer"},
            {"Switch", "SwitchPlayer"},
        };
        var player_name = shift.Sound == null ? "TeleportPlayer" : 
                          sounds_dict.TryGetValue(shift.Sound, out var p) ? p : null;
        if (player_name != null) { enterPlayer = GetNode<AudioStreamPlayer2D>(player_name); }
    }

    public void _on_Area2D_body_entered(Node body)
    {
        if (!(body is Juni juni)) { return; }

        GDArea.Selector.Register(this);
        
        if (shift.OnTouch)
        { 
            if (shift.DenyHologram && juni.Hologram != null)
            {
                juni.Connect(nameof(Juni.HologramStopped), this, nameof(executeShift));
            }
            else
            {
                executeShift(juni);
            }
        }
        else
        {
            juni.Connect(nameof(Juni.DownEvent), this, nameof(executeShift));
        }
    }

    public void _on_Area2D_body_exited(Node body)
    {
        if (!(body is Juni juni)) { return; }

        GDArea.Selector.Unregister(this);

        if (!shift.OnTouch && juni.IsConnected(nameof(Juni.DownEvent), this, nameof(executeShift)))
        {
            juni.Disconnect(nameof(Juni.DownEvent), this, nameof(executeShift));
        }

        if (shift.DenyHologram && juni.IsConnected(nameof(Juni.HologramStopped), this, nameof(executeShift)))
        {
            juni.Disconnect(nameof(Juni.HologramStopped), this, nameof(executeShift));
        }
    }

    public void executeShift(Juni juni)
    {
        if (GDArea.Selector.IsObjectSelected(this))
        {
            CallDeferred("_execute_shift", juni);
        }
    }

    private async void _execute_shift(Juni juni)
    {
        var game = GDArea.GDWorld.Game;

        var relative_area = shift.RelativeArea;
        if (!relative_area.isZero())
        {
            game.changeAreaDelta(relative_area, true);
        }

        var jgp = juni.GlobalPosition;
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

        if (enterPlayer != null)
        {
            enterPlayer.Play();
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

        if (shift.Effect)
        {
            juni.shiftEffect();
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
