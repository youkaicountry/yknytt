using Godot;
using System;
using YKnyttLib;

public class StandartSoundPlayer : Node2D
{
    public KnyttWorld KWorld { get; set; }

    public void playSound(string sound)
    {
        string[] available_sounds = {"door", "electronic", "switch", "teleport", "powerup", "bounce", "save"};
        if (Array.IndexOf(available_sounds, sound.ToLower()) != -1)
        {
            string node_prefix = char.ToUpper(sound[0]) + sound.Substring(1).ToLower();
            GetNode<AudioStreamPlayer2D>($"{node_prefix}Player2D").Play();
        }
        else
        {
            var custom_player = GetNode<AudioStreamPlayer2D>("CustomPlayer2D");
            custom_player.Stream = KWorld.getWorldSound($"Sounds/{sound}.ogg", loop: false) as AudioStream;
            if (custom_player.Stream != null) { custom_player.Play(); }
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
