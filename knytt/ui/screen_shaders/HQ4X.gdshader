shader_type canvas_item;

/*
Made with a whole lot of nightmare fuel.
Logic's Under Pressure and Bobby Tarantino II Albums was in rotation during
the making process.
Hope you enjoy!

You're auto-friended to me if you're a Logic fan
RattPack all day!
*/

//image mipmap level, for base upscaling
const int ML = 0;

//equality threshold of 2 colors before forming lines
uniform float THRESHOLD = 0.1;

//anti aliasing scaling, smaller value make lines more blurry
uniform float AA_SCALE = 10.0;

//draw diagonal line connecting 2 pixels if within threshold
bool diag(inout vec4 sum, vec2 uv, vec2 p1, vec2 p2, sampler2D iChannel0, float LINE_THICKNESS) {
    vec4 v1 = texelFetch(iChannel0,ivec2(uv+vec2(p1.x,p1.y)),ML),
        v2 = texelFetch(iChannel0,ivec2(uv+vec2(p2.x,p2.y)),ML);
    if (length(v1-v2) < THRESHOLD) {
    	vec2 dir = p2-p1,
            lp = uv-(floor(uv+p1)+.5);
    	dir = normalize(vec2(dir.y,-dir.x));
        float l = clamp((LINE_THICKNESS-dot(lp,dir))*AA_SCALE,0.,1.);
        sum = mix(sum,v1,l);
    	return true;
    }
    return false;
}

void fragment(){
	//line thickness
	float LINE_THICKNESS;
	vec2 ip = UV * (1.0 / TEXTURE_PIXEL_SIZE);
	
	
		//start with nearest pixel as 'background'
		vec4 s = texelFetch(TEXTURE,ivec2(int(ip.x),int(ip.y)),ML);
		
		//draw anti aliased diagonal lines of surrounding pixels as 'foreground'
		LINE_THICKNESS = 0.4;
		if (diag(s,ip,vec2(-1,0),vec2(0,1), TEXTURE, LINE_THICKNESS)) {
			LINE_THICKNESS = 0.3;
			diag(s,ip,vec2(-1,0),vec2(1,1), TEXTURE, LINE_THICKNESS);
			diag(s,ip,vec2(-1,-1),vec2(0,1), TEXTURE, LINE_THICKNESS);
		}
		LINE_THICKNESS = 0.4;
		if (diag(s,ip,vec2(0,1),vec2(1,0), TEXTURE, LINE_THICKNESS)) {
			LINE_THICKNESS = 0.3;
			diag(s,ip,vec2(0,1),vec2(1,-1), TEXTURE, LINE_THICKNESS);
			diag(s,ip,vec2(-1,1),vec2(1,0), TEXTURE, LINE_THICKNESS);
		}
		LINE_THICKNESS = 0.4;
		if (diag(s,ip,vec2(1,0),vec2(0,-1), TEXTURE, LINE_THICKNESS)) {
			LINE_THICKNESS = 0.3;
			diag(s,ip,vec2(1,0),vec2(-1,-1), TEXTURE, LINE_THICKNESS);
			diag(s,ip,vec2(1,1),vec2(0,-1), TEXTURE, LINE_THICKNESS);
		}
		LINE_THICKNESS = 0.4;
		if (diag(s,ip,vec2(0,-1),vec2(-1,0), TEXTURE, LINE_THICKNESS)) {
			LINE_THICKNESS = 0.3;
			diag(s,ip,vec2(0,-1),vec2(-1,1), TEXTURE, LINE_THICKNESS);
			diag(s,ip,vec2(1,-1),vec2(-1,0), TEXTURE, LINE_THICKNESS);
		}
		
		COLOR = s;
}