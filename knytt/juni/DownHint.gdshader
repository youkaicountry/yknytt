shader_type canvas_item;

uniform vec4 dark_hint;
uniform vec4 light_hint;

void fragment()
{
	vec4 color = texture(TEXTURE, UV);
	vec4 screen_color = texture(SCREEN_TEXTURE, SCREEN_UV);
	if (color.a == 1.0)
	{
		if (screen_color.r + screen_color.g + screen_color.b < 1.5) 
			COLOR = light_hint;
		else
			COLOR = dark_hint;
	}
	else
	{
		COLOR = screen_color;
	}
}