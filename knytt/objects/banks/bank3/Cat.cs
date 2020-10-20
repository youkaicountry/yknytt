using Godot;
using YUtil.Random;

public class Cat : GDKnyttBaseObject
{
    string cat_type;

    private Vector2 IDLE_RANGE = new Vector2(.2f, 1f);

    public override void _Ready()
    {
        cat_type = ObjectID.y == 42 ? "BigCat" : "SmallCat";
        nextAnimation();
    }

    private async void nextAnimation()
    {
        string anim_name = "";
        switch(GDKnyttDataStore.random.Next(3))
        {
            case 0: anim_name = "Blink"; break;
            case 1: anim_name = "Ears"; break;
            case 2: anim_name = "Tail"; break;
        }

        // Play animation
        var anim = GetNode<AnimatedSprite>("AnimatedSprite");
        anim.Stop();
        anim.Play($"{cat_type}{anim_name}");
        await ToSignal(anim, "animation_finished");

        // Idle
        var timer = GetNode<Timer>("IdleTimer");
        timer.WaitTime = GDKnyttDataStore.random.NextFloat(IDLE_RANGE.x, IDLE_RANGE.y);
        timer.Start();
    }

    private void _on_IdleTimer_timeout()
    {
        nextAnimation();
    }
}
