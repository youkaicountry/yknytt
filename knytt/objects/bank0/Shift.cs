using System;
using Godot;
using YKnyttLib;
using YKnyttLib.Logging;
using static YKnyttLib.KnyttSwitch;

public class Shift : Switch
{
    KnyttShift shift;

    public override void _Ready()
    {
        @switch = shift = GDArea.Area.ExtraData != null ? 
            new KnyttShift(GDArea.Area, Coords, (SwitchID)(ObjectID.y - 14)) :
            new KnyttShift(GDArea.Area.Position, Coords, (SwitchID)(ObjectID.y - 14)); // just show shift spot (original game has also effect)
        base._Ready();
    }

    protected override async void _execute(Juni juni)
    {
        var game = GDArea.GDWorld.Game;
        if (GDArea != game.CurrentArea) { return; }
        juni.juniInput.SwitchHeld = true;

        if (shift.Coin != 0 && juni.Powers.getCoinCount() < shift.Coin) { return; }

        if (shift.Delay > 0)
        {
            // workaround: juni.just_reset == 0 doesn't work with timers
            // every additional shift timer guarantees 0.1s of motion
            if (juni.just_reset == -1)
            {
                for (float i = shift.Delay / 1000f; i < 0.1f; i += 1 / 60.0f)
                {
                    juni._PhysicsProcess(1 / 60.0f);
                }
            }

            if (shift.Hide)
            {
                juni.Visible = false;
                GDArea.BlockInput = true;
                juni.StopMotion = true;
                Trigger trigger = GDArea.Objects.findObject(new KnyttPoint(0, ObjectID.y + 18)) as Trigger;
                trigger?.executeAnyway(juni);
            }

            var delay_timer = GetNode<Timer>("DelayTimer");
            if (!delay_timer.IsStopped()) { return; }
            delay_timer.Start(shift.Delay / 1000f);
            await ToSignal(delay_timer, "timeout");

            if (shift.Hide)
            {
                juni.Visible = true;
                GDArea.BlockInput = false;
                juni.StopMotion = false;
            }

            if (GDArea != game.CurrentArea) { return; }
        }

        if (shift.Coin != 0)
        {
            juni.Powers.CoinsSpent += shift.Coin;
            juni.updateCollectables();
        }

        if (shift.Cutscene != null)
        {
            GetTree().Paused = true;
            await game.fade(fast: true, color: Cutscene.getCutsceneColor(shift.Cutscene));
            GDKnyttDataStore.playCutscene(shift.Cutscene, sound);
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
                game.UI.Location.updateFlags(juni.Powers.Flags);
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
                game.UI.Location.updateFlags(juni.Powers.Flags);
            }
        }

        var relative_area = shift.RelativeArea;
        if (!relative_area.isZero())
        {
            relative_area += GDArea.GDWorld.Game.getFlagWarp(shift.AbsoluteArea, juni) ?? new KnyttPoint(0, 0);
            game.changeAreaDelta(relative_area, true);
        }

        KnyttLogger.Debug($"Shift {shift.RelativeArea} {shift.RelativePosition} / {shift.AbsoluteArea} {shift.AbsolutePosition}");

        // Move Juni to the correct location in the area
        if (shift.Quantize || shift.AbsoluteTarget)
        {
            juni.moveToPosition(game.CurrentArea, shift.AbsolutePosition, up_correction: true); // up_correction replaces juni._PhysicsProcess sometimes
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

        if (!(shift.RelativeArea.isZero() && shift.RelativePosition.isZero()))
        {
            _on_Area2D_body_exited(juni); // sometimes exit signal is late
        }

        if (juni.just_reset == 0)
        {
            juni._PhysicsProcess(1 / 60.0f); // to exit endless shift loops
            juni._PhysicsProcess(1 / 60.0f); // MoveAndSlide can't use other fps
            juni._PhysicsProcess(1 / 60.0f);
            juni._PhysicsProcess(1 / 60.0f);
        }

        juni.just_reset = 2; // sometimes inside checker needs time to start working

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
            if (!relative_area.isZero())
            {
                game.MusicChannel.playQueued();
            }
            game.MusicChannel.fadeIn(3);
        }

        if (shift.Character != null)
        {
            juni.changeCharacter(shift.Character);
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

        if (shift.SaveFile != null && shift.SaveFile != "true" && shift.SaveFile != "false" && 
            game.GDWorld.KWorld.worldFileExists($"savegame{shift.SaveFile}.ini"))
        {
            try
            {
                string savefile = GDKnyttAssetManager.loadTextFile(game.GDWorld.KWorld.getWorldData($"savegame{shift.SaveFile}.ini"));
                KnyttSave save = new KnyttSave(game.GDWorld.KWorld, savefile, game.GDWorld.KWorld.CurrentSave.Slot);
                game.GDWorld.KWorld.CurrentSave = save;
                game.saveGame(save);
                save.SourcePowers = new JuniValues();
                save.SourcePowers.readFromSave(save);
            }
            catch (Exception) {}
        }
    }
}
