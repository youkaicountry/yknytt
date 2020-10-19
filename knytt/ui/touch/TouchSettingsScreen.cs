using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public class TouchSettingsScreen : Control
{
	private readonly float[] SCALES = {0.5f, 0.75f, 1f, 1.25f, 1.5f};
	private Dictionary<float, int> optionByScale = new Dictionary<float, int>();
	
	public TouchSettingsScreen()
	{
		int i = 0;
		foreach (var scale in SCALES)
		{
			optionByScale.Add(scale, i++);
		}
	}

	public override void _Ready()
	{
		fillControls();
	}
	
	public void fillControls()
	{
		GetNode<CheckBox>("SettingsContainer/EnableButton").Pressed = TouchSettings.EnablePanel;
		GetNode<CheckBox>("SettingsContainer/SwapButton").Pressed = TouchSettings.SwapHands;
		GetNode<OptionButton>("SettingsContainer/AnchorContainer/AnchorDropdown").Select((int)TouchSettings.Position);
		GetNode<OptionButton>("SettingsContainer/ScaleContainer/ScaleDropdown").Select(
			optionByScale.TryGetValue(TouchSettings.Scale, out var i) ? i : 0);
	}
	
	private void _on_EnableButton_pressed()
	{
		TouchSettings.EnablePanel = GetNode<CheckBox>("SettingsContainer/EnableButton").Pressed;
	}

	private void _on_SwapButton_pressed()
	{
		TouchSettings.SwapHands = GetNode<CheckBox>("SettingsContainer/SwapButton").Pressed;
	}

	private void _on_AnchorDropdown_item_selected(int index)
	{
		TouchSettings.Position = (TouchSettings.VerticalPosition)index;
	}

	private void _on_ScaleDropdown_item_selected(int index)
	{
		TouchSettings.Scale = SCALES[index];
	}
	
	private void _on_BackButton_pressed()
	{
		ClickPlayer.Play();
		QueueFree();
	}
}

