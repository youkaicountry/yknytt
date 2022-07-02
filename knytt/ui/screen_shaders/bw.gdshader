shader_type canvas_item;

void fragment()
{
	vec4 bg = texture(SCREEN_TEXTURE, SCREEN_UV);
	float avg = (bg.r + bg.g + bg.b) / 3f;
	COLOR = vec4(avg, avg, avg, 1f);
}