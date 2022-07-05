shader_type canvas_item;

// CurveTexture(s)
uniform sampler2D rgb_curve;
uniform sampler2D red_curve;
uniform sampler2D green_curve;
uniform sampler2D blue_curve;
uniform sampler2D alpha_curve;
uniform sampler2D hue_curve;
uniform sampler2D sat_curve;
uniform sampler2D value_curve;


vec3 rgb2hsb(vec3 c) {
	vec4 K = vec4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
	vec4 p = mix(vec4(c.bg, K.wz),
				vec4(c.gb, K.xy),
				step(c.b, c.g));
	vec4 q = mix(vec4(p.xyw, c.r),
				vec4(c.r, p.yzx),
				step(p.x, c.r));
	float d = q.x - min(q.w, q.y);
	float e = 1.0e-10;
	return vec3(abs(q.z + (q.w - q.y) / (6.0 * d + e)),
				d / (q.x + e),
				q.x);
}

vec3 hsb2rgb(vec3 c)
{
	vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
	vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
	return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}


void fragment() {
	COLOR = texture(TEXTURE, UV);

	float red_curve_color = texture(red_curve, vec2(COLOR.r, 0.0)).a;
	float green_curve_color = texture(green_curve, vec2(COLOR.g, 0.0)).a;
	float blue_curve_color = texture(blue_curve, vec2(COLOR.b, 0.0)).a;
	float alpha_curve_color = texture(alpha_curve, vec2(COLOR.a, 0.0)).a;
	COLOR.r = red_curve_color;
	COLOR.g = green_curve_color;
	COLOR.b = blue_curve_color;
	COLOR.a = alpha_curve_color;
	
	vec3 hsb = rgb2hsb(COLOR.rgb);
	float hue_curve_color = texture(hue_curve, vec2(hsb.r, 0.0)).a;
	float sat_curve_color = texture(sat_curve, vec2(hsb.g, 0.0)).a;
	float value_curve_color = texture(value_curve, vec2(hsb.b, 0.0)).a;
	hsb.r = hue_curve_color;
	hsb.g = sat_curve_color;
	hsb.b = value_curve_color;

	COLOR.rgb = hsb2rgb(hsb);

	float rgb_curve_color_r = texture(rgb_curve, vec2(COLOR.r, 0.0)).a;
	float rgb_curve_color_g = texture(rgb_curve, vec2(COLOR.g, 0.0)).a;
	float rgb_curve_color_b = texture(rgb_curve, vec2(COLOR.b, 0.0)).a;
	COLOR.rgb = vec3(rgb_curve_color_r, rgb_curve_color_g, rgb_curve_color_b);
}
