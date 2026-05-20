using Godot;
using System;
using YKnyttLib;

public class StandartSoundPlayer : Node2D
{
    public KnyttWorld KWorld { get; set; }

    public void playSound(string sound)
    {
        string[] available_sounds = {"door", "electronic", "switch", "teleport", "powerup", "bounce", "save"};
        int found_sound = Array.IndexOf(available_sounds, sound.ToLower());
        var custom_sound = found_sound >= 4 ? null : KWorld.getWorldSound($"Sounds/{sound}.ogg", loop: false) as AudioStream;

        if (custom_sound != null)
        {
            var custom_player = GetNode<AudioStreamPlayer2D>("CustomPlayer2D");
            custom_player.Stream = custom_sound;
            custom_player.Play();
        }
        else if (found_sound != -1)
        {
            string node_prefix = char.ToUpper(sound[0]) + sound.Substring(1).ToLower();
            GetNode<AudioStreamPlayer2D>($"{node_prefix}Player2D").Play();
        }
    }

    public void stopAll()
    {
        foreach (AudioStreamPlayer2D child in GetChildren())
        {
            child.Stop();
        }
    }
}
