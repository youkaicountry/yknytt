using Godot;

public class FileHTTPRequest : HTTPRequest
{
    public override void _ExitTree()
    {
        cleanup();
    }

    public void cleanup()
    {
        if (DownloadFile != null && new File().FileExists(DownloadFile))
        {
            new Directory().Remove(DownloadFile);
        }
    }
}
