public class RandomLabyrinthSpike : LabyrinthSpike
{
    protected override void onCollide()
    {
        speed = 50 * (random.Next(2) + 1);
    }
}
