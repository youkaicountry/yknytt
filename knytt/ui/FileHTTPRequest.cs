using Godot;

public partial class FileHttpRequest : HttpRequest
{
    public override void _ExitTree()
    {
        cleanup();
    }

    public void cleanup()
    {
        if (DownloadFile != null && FileAccess.FileExists(DownloadFile))
        {
            new DirAccess().Remove(DownloadFile);
        }
    }
}
