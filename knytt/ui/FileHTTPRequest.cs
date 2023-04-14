using Godot;

public partial class FileHTTPRequest : HttpRequest
{
    public override void _ExitTree()
    {
        cleanup();
    }

    public void cleanup()
    {
        if (DownloadFile != null && FileAccess.FileExists(DownloadFile))
        {
            DirAccess.RemoveAbsolute(DownloadFile);
        }
    }
}
