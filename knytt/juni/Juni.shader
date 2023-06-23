shader_type canvas_item;

uniform vec4 clothes_color;
uniform vec4 skin_color;
uniform bool clothes_skip;
uniform bool skin_skip;

const vec4 CLOTHES = vec4(.937, .937, .937, 1);
const vec4 SKIN = vec4(.847, .753, .651, 1);

bool approx_eq(vec4 c1, vec4 c2)
{ 
	return all( lessThan( abs( c1 - c2 ), vec4( 0.02, 0.02, 0.02, 0.02) ) );
}

void fragment()
{
	vec4 color = texture(TEXTURE, UV);
	if (!clothes_skip && approx_eq(color, CLOTHES)) { COLOR = clothes_color; }
	else if (!skin_skip && approx_eq(color, SKIN)) { COLOR = skin_color; }
	else { COLOR = color; }
}
