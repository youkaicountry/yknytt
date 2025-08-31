using Godot;
using Godot.Collections;
using System.Text;
using System.Collections.Generic;

public class RateHTTPRequest : HTTPRequest
{
    [Signal] public delegate void RateAdded(int action);

    public enum Action
    {
        Download = 1,
        Complain = 4,
        Cutscene = 5,
        Ending = 6,
        WinExit = 9,
        Cheat = 10,
    }

    private int retry;
    private Dictionary dict;
    private HashSet<int> sent_actions = new HashSet<int>();
    private bool is_requesting = false;

    public async void send(string level_name, string level_author, int action, string cutscene = null, bool once = true)
    {
        if (once && sent_actions.Contains(action)) { return; }

        while (is_requesting)
        {
            await ToSignal(this, "request_completed");
            await ToSignal(GetTree(), "idle_frame");
        }

        string serverURL = GDKnyttSettings.ServerURL;
        dict = new Dictionary()
        {
            ["name"] = level_name,
            ["author"] = level_author,
            ["action"] = action,
            ["uid"] = GDKnyttSettings.UUID,
            ["platform"] = OS.GetName() + " " + GDKnyttSettings.Version
        };
        if (cutscene != null) { dict.Add("cutscene", cutscene); }
        retry = 1;
        var error = Request($"{serverURL}/rate/", method: HTTPClient.Method.Post, requestData: JSON.Print(dict));
        if (error == Error.Ok) { is_requesting = true; }
    }

    private void _on_HTTPRequest_request_completed(int result, int response_code, string[] headers, byte[] body)
    {
        if (result != (int)HTTPRequest.Result.Success || response_code == 500)
        {
            if (retry-- <= 0) { is_requesting = false; return; }
            CancelRequest();
            Request($"{GDKnyttSettings.ServerURL}/rate/", method: HTTPClient.Method.Post, requestData: JSON.Print(dict));
            return;
        }
        is_requesting = false;
        if (response_code != 200) { return; }

        var response = Encoding.UTF8.GetString(body, 0, body.Length);
        var json = JSON.Parse(response);
        if (json.Error != Error.Ok) { return; }

        int action = HTTPUtil.jsonInt(json.Result, "action");
        sent_actions.Add(action);
        if (HTTPUtil.jsonInt(json.Result, "added") == 1)
        {
            EmitSignal(nameof(RateAdded), action);
        }
    }

    private void _on_RateHTTPRequest_tree_exiting()
    {
        is_requesting = false;
    }
}
