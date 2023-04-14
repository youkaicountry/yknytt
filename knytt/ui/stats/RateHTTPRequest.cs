using Godot;
using Godot.Collections;
using System.Text;

public partial class RateHTTPRequest : HttpRequest
{
    [Signal] public delegate void RateAddedEventHandler(int action);

    public enum Action
    {
        Undefined = 0,
        Download = 1,
        Upvote = 2,
        Downvote = 3,
        Complain = 4,
        Cutscene = 5,
        Ending = 6,
        Enter = 7,
        Exit = 8
    }

    public void send(string level_name, string level_author, int action, string cutscene = null)
    {
        string serverURL = GDKnyttSettings.ServerURL;
        var dict = new Dictionary()
        {
            ["name"] = level_name,
            ["author"] = level_author,
            ["action"] = action,
            ["uid"] = GDKnyttSettings.UUID,
            ["platform"] = OS.GetName()
        };
        if (cutscene != null) { dict.Add("cutscene", cutscene); }
        Request($"{serverURL}/rate/", method: HttpClient.Method.Post, requestData: Json.Stringify(dict));
    }

    private void _on_HTTPRequest_request_completed(int result, int response_code, string[] headers, byte[] body)
    {
        if (result == (int)HttpRequest.Result.Success && response_code == 200) {; } else { return; }
        var response = Encoding.UTF8.GetString(body, 0, body.Length);
        var jsonObject = new Json();
        var error = jsonObject.Parse(response);
        if (error != Error.Ok) { return; }

        if (HTTPUtil.jsonInt(jsonObject.Data, "added") == 1)
        {
            EmitSignal(SignalName.RateAdded, HTTPUtil.jsonInt(jsonObject.Data, "action"));
        }
    }
}
