using Godot;
using System;
using YKnyttLib;

public class StandartSoundPlayer : Node2D
{
    public KnyttWorld KWorld { get; set; }

    public void playSound(string sound)
    {
        string[] available_sounds = {"door", "electronic", "switch", "teleport", "powerup", "bounce"};
        if (Array.IndexOf(available_sounds, sound.ToLower()) != -1)
        {
            GetNode<AudioStreamPlayer2D>($"{sound.Capitalize()}Player2D").Play();
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
        foreach (var child in GetChildren())
        {
            ((AudioStreamPlayer2D)child).Stop();
        }
    }
}
