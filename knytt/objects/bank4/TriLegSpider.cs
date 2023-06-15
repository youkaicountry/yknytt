public class TriLegSpider : Spider
{
    private void _on_RunTimer_timeout()
    {
        if (random.Next(3) == 0) { tryRun(); }
    }
}
