
public partial class ClearBlock : Password
{
    public override void _Ready()
    {
        current_char = 22;
        base._Ready();
    }

    protected override bool checkPassword()
    {
        return true;
    }
}
