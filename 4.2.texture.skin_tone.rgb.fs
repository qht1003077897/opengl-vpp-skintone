#version 330 core

in vec2 TexCoord;
out vec4 FragColor;
//采样
uniform sampler2D texture1;

const float SKIN_HUE = 0.05;
const float SKIN_HUE_TOLERANCE = 50.0;    
const float MAX_HUE_SHIFT = 0.04;
const float MAX_SATURATION_SHIFT = 0.25;

vec2 Resolution = vec2(1280.0f,852.0f);

// RGB <-> HSV conversion, thanks to http://lolengine.net/blog/2013/07/27/rgb-to-hsv-in-glsl
vec3 rgb2hsv(vec3 c)
{
    vec4 K = vec4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    vec4 p = mix(vec4(c.bg, K.wz), vec4(c.gb, K.xy), step(c.b, c.g));
    vec4 q = mix(vec4(p.xyw, c.r), vec4(c.r, p.yzx), step(p.x, c.r));

    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return vec3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

vec3 hsv2rgb(vec3 c)
{
    vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

void main()
{   
    // sample video texture
	vec2 uv = TexCoord.xy;
    vec4 tex = texture(texture1, uv);    
    vec3 colorRGB = tex.rgb;
		
    // mouse Y => skin_tone_shift in [-1, 1]
    float skin_tone_shift = ((550.0 / Resolution.y) * 2.0 - 1.0);
    //skin_tone_shift = clamp(skin_tone_shift * 1.25, -1.0, 1.0);
    
    // Convert color to HSV, extract hue
	vec3 colorHSV = rgb2hsv(colorRGB);	
    float hue = colorHSV.x;
	
	// check how far from skin hue
    float dist = hue - SKIN_HUE;        
   	if (dist > 0.5)
		dist -= 1.0;
	if (dist < -0.5)
		dist += 1.0;
	dist = abs(dist)/0.5; // normalized to [0,1]
    
	// Apply Gaussian like filter
    float weight = exp(-dist*dist*SKIN_HUE_TOLERANCE);  
	weight = clamp(weight, 0.0, 1.0);
    
	// We want more orange, so increase saturation
	if (skin_tone_shift > 0.0)
		colorHSV.y += skin_tone_shift * weight * MAX_SATURATION_SHIFT;
	// we want more pinks, so decrease hue
	else
		colorHSV.x += skin_tone_shift * weight * MAX_HUE_SHIFT;
		
    // final color
	vec3 finalColorRGB = hsv2rgb(colorHSV.rgb);		
        
	// apply only on right part of the screen
	if (uv.x < 0.4)
		finalColorRGB = colorRGB;
		
	// add black vertical line
    if (abs(uv.x - 0.4) < (1.0 / Resolution.x))
        finalColorRGB *= 0.0;
    
    // display
    FragColor = vec4(finalColorRGB, 1.0);
}