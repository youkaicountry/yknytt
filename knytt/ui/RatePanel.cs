using Godot;
using Godot.Collections;
using System;
using System.Text;
using YKnyttLib;

public class RatePanel : Panel
{
    public GameButtonInfo ButtonInfo { get; set; }
    public KnyttWorld KWorld { get; set; }

    [Export] string serverURL;
    [Export] string complainURL;
    
    public override void _Ready()
    {
        update();
    }

    private void update()
    {
        if (ButtonInfo.LevelId == 0)
        {
            requestRates();
            GetNode<Label>("UpvoteLabel").Text = GetNode<Label>("DownvoteLabel").Text = "";
        }
        else
        {
            GetNode<Label>("UpvoteLabel").Text = $"+{ButtonInfo.Upvotes}";
            GetNode<Label>("DownvoteLabel").Text = $"-{ButtonInfo.Downvotes}";
        }
    }

    private void _on_UpvoteButton_pressed()
    {
        sendRating(2);
    }

    private void _on_DownvoteButton_pressed()
    {
        sendRating(3);
    }

    private void _on_ComplainButton_pressed()
    {
        sendRating(4);
        OS.ShellOpen(complainURL);
    }

    private void sendRating(int action)
    {
        HTTPRequest rest_node = GetNode<HTTPRequest>("RestHTTPRequest");
        var dict = new Dictionary() { 
            ["level_id"] = ButtonInfo.LevelId, ["action"] = action, 
            ["uid"] = GDKnyttSettings.getUUID(), ["platform"] = OS.GetName() };
        rest_node.Request($"{serverURL}/rate/", method: HTTPClient.Method.Post, requestData: JSON.Print(dict));
    }

    private void requestRates()
    {
        var rest_node = GetNode<HTTPRequest>("RestHTTPRequest");
        rest_node.Request($"{serverURL}/rating/?name={Uri.EscapeUriString(ButtonInfo.Name)}&author={Uri.EscapeUriString(ButtonInfo.Author)}");
    }

    private static int jsonInt(object obj, string attr) => LevelSelection.jsonInt(obj, attr);

    private void _on_RestHTTPRequest_request_completed(int result, int response_code, string[] headers, byte[] body)
    {
        if (result == (int)HTTPRequest.Result.Success && response_code == 200) { ; } else { return; }
        var response = Encoding.UTF8.GetString(body, 0, body.Length);
        var json = JSON.Parse(response);
        if (json.Error != Error.Ok) { return; }

        if (jsonInt(json.Result, "added") == 1) // /rate/ response: upvote/downvote successful
        {
            if (jsonInt(json.Result, "action") == 2) { ButtonInfo.Upvotes++; }
            if (jsonInt(json.Result, "action") == 3) { ButtonInfo.Downvotes++; }
            update();
        }

        if (jsonInt(json.Result, "level") != 0) // /rating/ response: find level_id for local world (to upvote/downvote it)
        {
            ButtonInfo.LevelId = jsonInt(json.Result, "level");
            ButtonInfo.Downloads = jsonInt(json.Result, "downloads");
            ButtonInfo.Upvotes = jsonInt(json.Result, "upvotes");
            ButtonInfo.Downvotes = jsonInt(json.Result, "downvotes");
            update();
        }
    }
}
